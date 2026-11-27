import asyncio
from datetime import datetime, timezone
import flet as ft
import threading
import time
from typing import cast
import sys
from pathlib import Path

# Add the repository root (the folder that contains the 'PyMtApi5' package)
# to the module search path, independent of the current working directory.
REPO_ROOT = Path(__file__).resolve().parents[2]
if str(REPO_ROOT) not in sys.path:
    sys.path.insert(0, str(REPO_ROOT))

from PyMtApi5 import (  # noqa: E402
    Mt5ApiClient,
    Mt5EventType,
    Mt5Quote,
    MqlTick,
    MqlRates,
    MqlTradeRequest,
    MqlTradeResult,
    MqlTradeTransaction,
    MqlBookInfo,
)
from PyMtApi5 import mt5enums  # noqa: E402
from command_tabs import Command, CommandTabs  # noqa: E402

# Lists returned by commands (e.g. CopyRates) are logged up to this number of items.
LOG_RESULT_MAX_ITEMS = 100

def main(page: ft.Page):
    page.title = "MTAPI5 Test Client"
    page.vertical_alignment = ft.MainAxisAlignment.START
    page.padding = 20
    page.theme_mode = ft.ThemeMode.DARK

    # Connection parameters input fields
    host_input = ft.TextField(label="Host", value="192.168.0.139", width=180, height=50)
    port_input = ft.TextField(label="Port", value="8228", width=100, height=50)
    
    # Status indicator
    status_text = ft.Text("Disconnected", color="red", weight=ft.FontWeight.BOLD)
    
    # Log container view
    log_view = ft.ListView(expand=True, spacing=5, auto_scroll=True)

    # Events (e.g. OnBookEvent) can arrive many times per second: keep only the last lines.
    LOG_MAX_LINES = 1000
    log_lock = threading.Lock()

    def log_message(message: str):
        timestamp = time.strftime("%H:%M:%S")
        with log_lock:
            log_view.controls.append(ft.Text(f"[{timestamp}] {message}", size=12, font_family="Consolas"))
            if len(log_view.controls) > LOG_MAX_LINES:
                del log_view.controls[: len(log_view.controls) - LOG_MAX_LINES]
            log_view.update()

    # Quotes list: one row per expert attached to a chart in MetaTrader (as in MtApi5TestClient).
    # Rows are changed from the client's event thread and from UI threads, so they are
    # guarded by quotes_lock. Tick updates only mark the table dirty; quotes_refresh_loop
    # redraws it periodically instead of on every tick.
    quotes_lock = threading.Lock()
    quote_rows: dict[int, ft.DataRow] = {}
    selected_expert: int | None = None
    quotes_dirty = threading.Event()

    quotes_table = ft.DataTable(
        columns=[
            ft.DataColumn(ft.Text("Instrument")),
            ft.DataColumn(ft.Text("Bid"), numeric=True),
            ft.DataColumn(ft.Text("Ask"), numeric=True),
            ft.DataColumn(ft.Text("ExpertHandle"), numeric=True),
        ],
        rows=[],
        show_checkbox_column=False,
        column_spacing=24,
        heading_row_height=36,
        data_row_min_height=30,
        data_row_max_height=30,
    )

    def select_quote(e):
        nonlocal selected_expert
        with quotes_lock:
            selected_expert = e.control.data
            for handle, row in quote_rows.items():
                row.selected = handle == selected_expert
            quotes_table.update()

    def format_mt_time(mt_time: int) -> str:
        # MetaTrader time is seconds since 1970 in the trade server's time zone: no conversion.
        return datetime.fromtimestamp(mt_time, tz=timezone.utc).strftime("%Y.%m.%d %H:%M:%S")

    def format_price(price: float) -> str:
        return f"{price:.5f}"

    def set_quote(quote: Mt5Quote):
        """Add a row for the quote's expert or update its prices. Caller holds quotes_lock."""
        row = quote_rows.get(quote.expert_handle)
        if row is None:
            row = ft.DataRow(
                cells=[
                    ft.DataCell(ft.Text(quote.instrument)),
                    ft.DataCell(ft.Text(format_price(quote.bid))),
                    ft.DataCell(ft.Text(format_price(quote.ask))),
                    ft.DataCell(ft.Text(str(quote.expert_handle))),
                ],
                data=quote.expert_handle,
                selected=quote.expert_handle == selected_expert,
                on_select_change=select_quote,
            )
            quote_rows[quote.expert_handle] = row
            quotes_table.rows.append(row)
        else:
            cast(ft.Text, row.cells[1].content).value = format_price(quote.bid)
            cast(ft.Text, row.cells[2].content).value = format_price(quote.ask)

    def add_quote(quote: Mt5Quote):
        with quotes_lock:
            set_quote(quote)
            quotes_table.update()

    def update_quote(quote: Mt5Quote):
        with quotes_lock:
            if quote.expert_handle in quote_rows:
                set_quote(quote)
                quotes_dirty.set()

    def remove_quote(quote: Mt5Quote):
        nonlocal selected_expert
        with quotes_lock:
            row = quote_rows.pop(quote.expert_handle, None)
            if row is None:
                return
            quotes_table.rows.remove(row)
            if selected_expert == quote.expert_handle:
                selected_expert = None
            quotes_table.update()

    def set_quotes(quotes: list[Mt5Quote]):
        """Replace all rows, keeping the selection if its expert is still present."""
        nonlocal selected_expert
        with quotes_lock:
            quote_rows.clear()
            quotes_table.rows.clear()
            for quote in quotes:
                set_quote(quote)
            if selected_expert not in quote_rows:
                selected_expert = None
            quotes_table.update()

    def quotes_refresh_loop():
        while not closing.wait(0.25):
            if quotes_dirty.is_set():
                quotes_dirty.clear()
                with quotes_lock:
                    quotes_table.update()

    # MtApi5 client state. Mt5ApiClient.connect()/disconnect() are blocking, so they
    # are run on worker threads (page.run_thread) to keep the UI responsive.
    client_lock = threading.Lock()
    client: Mt5ApiClient | None = None
    # Set when the page is closing: Flet's thread pool is shut down then and the page
    # must not be updated anymore.
    closing = threading.Event()

    def set_connection_state(state: str):
        if state == "connected":
            status_text.value, status_text.color = "Connected", "green"
        elif state == "connecting":
            status_text.value, status_text.color = "Connecting...", "orange"
        else:
            status_text.value, status_text.color = "Disconnected", "red"
        connect_btn.disabled = state != "disconnected"
        disconnect_btn.disabled = state != "connected"
        host_input.disabled = state != "disconnected"
        port_input.disabled = state != "disconnected"
        page.update()

    def release_client():
        """Detach the current client and close it. Safe to call more than once."""
        nonlocal client
        with client_lock:
            old_client, client = client, None
        if old_client is not None:
            old_client.disconnect()

    class ClientCallback:
        """Receives Mt5ApiClient events. Called on the client's event thread."""

        def on_disconnect(self, error_msg=None):
            if closing.is_set():
                return
            if error_msg is not None:
                log_message(f"Disconnected with error: {error_msg}")
            else:
                log_message("Disconnected from MetaTrader.")
            # The connection may have been closed by the server: release the client's
            # threads. This must not run on the client's event thread (disconnect()
            # joins it), hence a separate thread. A plain thread is used instead of
            # page.run_thread(): Flet's thread pool may already be shut down.
            threading.Thread(target=on_connection_closed, daemon=True).start()

        def on_quote_update(self, quote):
            if not closing.is_set():
                update_quote(quote)  # ticks are too frequent to be logged

        def on_quote_added(self, quote):
            if closing.is_set():
                return
            add_quote(quote)
            log_message(
                f"QuoteAdded: ExpertHandle = {quote.expert_handle}, Symbol = {quote.instrument}, "
                f"Bid = {format_price(quote.bid)}, Ask = {format_price(quote.ask)}"
            )

        def on_quote_removed(self, quote):
            if closing.is_set():
                return
            remove_quote(quote)
            log_message(f"QuoteRemoved: ExpertHandle = {quote.expert_handle}, Symbol = {quote.instrument}")

        def on_book_event(self, expert_handle, symbol):
            if closing.is_set():
                return
            log_message(f"OnBookEvent: ExpertHandle = {expert_handle}, Symbol = {symbol}")

        def on_last_time_bar(self, expert_handle, instrument, rates):
            if closing.is_set():
                return
            log_message(
                f"OnLastTimeBar: ExpertHandle = {expert_handle}, Symbol = {instrument}, "
                f"time = {format_mt_time(rates.time)}, open = {rates.open}, high = {rates.high}, low = {rates.low}, "
                f"close = {rates.close}, tick_volume = {rates.tick_volume}"
            )

        def on_trade_transaction(self, expert_handle, trade_transaction, trade_request, trade_result):
            if closing.is_set():
                return
            log_message(
                f"OnTradeTransaction: ExpertHandle = {expert_handle}\n"
                f"  Transaction = {trade_transaction}\n"
                f"  Request = {trade_request}\n"
                f"  Result = {trade_result}"
            )

    def on_connection_closed():
        release_client()
        if not closing.is_set():
            set_quotes([])
            set_connection_state("disconnected")

    def do_connect(host: str, port: int):
        nonlocal client
        new_client = Mt5ApiClient(host, port, ClientCallback())
        try:
            new_client.connect()
        except Exception as ex:
            if closing.is_set():
                return
            log_message(f"Connection failed: {ex}")
            set_connection_state("disconnected")
            return
        with client_lock:
            if not closing.is_set():
                client = new_client
        if closing.is_set():
            # The page was closed while connecting: on_page_close has already run.
            new_client.disconnect()
            return
        set_connection_state("connected")
        quotes = new_client.get_quotes()
        set_quotes(quotes)
        log_message(f"Connected to {host}:{port}. Quotes: {len(quotes)}")

    def connect_click(e):
        host = (host_input.value or "").strip()
        try:
            port = int(port_input.value or "")
        except ValueError:
            log_message(f"Error: invalid port '{port_input.value}'.")
            return
        if not host:
            log_message("Error: host is empty.")
            return
        log_message(f"Connecting to {host}:{port}...")
        set_connection_state("connecting")
        page.run_thread(do_connect, host, port)

    def disconnect_click(e):
        log_message("Disconnecting...")
        disconnect_btn.disabled = True
        page.update()
        page.run_thread(on_connection_closed)

    async def on_page_close(e):
        # Client threads are not daemons: close the connection so the process can exit.
        # Async on purpose: Flet awaits async handlers before shutting down its thread
        # pool, while sync handlers are queued to that pool and may be cancelled.
        closing.set()
        await asyncio.to_thread(release_client)

    page.on_close = on_page_close
    page.on_disconnect = on_page_close

    def button_style(bgcolor: ft.Colors) -> ft.ButtonStyle:
        # Per-state colors: a plain 'bgcolor' would also paint the disabled button.
        return ft.ButtonStyle(
            color={ft.ControlState.DISABLED: ft.Colors.ON_SURFACE_VARIANT, ft.ControlState.DEFAULT: ft.Colors.WHITE},
            bgcolor={ft.ControlState.DISABLED: ft.Colors.SURFACE_CONTAINER_HIGHEST, ft.ControlState.DEFAULT: bgcolor},
        )

    connect_btn = ft.Button("Connect", on_click=connect_click, style=button_style(ft.Colors.GREEN_700))
    disconnect_btn = ft.Button("Disconnect", on_click=disconnect_click, disabled=True, style=button_style(ft.Colors.RED_700))

    def run_command(name: str, command: Command, current_client: Mt5ApiClient):
        try:
            result = command(current_client)
        except Exception as ex:
            log_message(f"{name}: failed - {ex}")
            return
        if isinstance(result, list) and any(not isinstance(item, (int, float, str)) for item in result):
            # list of structures (MqlRates, MqlTick, ...): one item per line
            lines = [f"{name}: count = {len(result)}"]
            lines += [f"  [{i}] {item}" for i, item in enumerate(result[:LOG_RESULT_MAX_ITEMS])]
            if len(result) > LOG_RESULT_MAX_ITEMS:
                lines.append(f"  ... {len(result) - LOG_RESULT_MAX_ITEMS} more")
            log_message("\n".join(lines))
        elif isinstance(result, list):
            log_message(f"{name}: count = {len(result)}, result = {result}")
        else:
            log_message(f"{name}: result = {result}")

    def execute_command(name: str, command: Command):
        """Runs a command from the command tabs on a worker thread and logs its result."""
        with client_lock:
            current_client = client
        if current_client is None or not current_client.is_connected():
            log_message(f"{name}: no active connection to the server.")
            return
        page.run_thread(run_command, name, command, current_client)

    def get_selected_expert() -> int | None:
        with quotes_lock:
            return selected_expert

    command_tabs = CommandTabs(execute_command, get_selected_expert)

    def refresh_quotes(e):
        with client_lock:
            current_client = client
        if current_client is None or not current_client.is_connected():
            log_message("Error: No active connection to the server.")
            return
        quotes = current_client.get_quotes()
        set_quotes(quotes)
        log_message(f"Quotes refreshed: {len(quotes)}")

    refresh_quotes_btn = ft.OutlinedButton("Refresh Quotes", on_click=refresh_quotes)

    # UI Layout configuration
    connection_row = ft.Row(
        [host_input, port_input, connect_btn, disconnect_btn, ft.VerticalDivider(), ft.Row([ft.Text("Status:"), status_text])], 
        alignment=ft.MainAxisAlignment.START, 
        wrap=True
    )

    page.add(
        ft.Text("MTAPI Test Client (Python / Flet)", size=20, weight=ft.FontWeight.BOLD),
        ft.Divider(),
        connection_row,
        ft.Divider(),
        ft.Row(
            [
                ft.Column(
                    [
                        ft.Text("Quotes:", weight=ft.FontWeight.BOLD),
                        ft.Container(
                            content=ft.Column([quotes_table], scroll=ft.ScrollMode.AUTO, expand=True),
                            border=ft.Border.all(1, ft.Colors.OUTLINE),
                            border_radius=8,
                            expand=True,
                            bgcolor=ft.Colors.SURFACE_CONTAINER_HIGHEST,
                        ),
                        refresh_quotes_btn,
                    ],
                    width=420,
                ),
                ft.Column(
                    [
                        ft.Container(
                            content=command_tabs.control,
                            border=ft.Border.all(1, ft.Colors.OUTLINE),
                            border_radius=8,
                            expand=3,
                        ),
                        ft.Text("Event Logs:", weight=ft.FontWeight.BOLD),
                        ft.Container(
                            content=log_view,
                            border=ft.Border.all(1, ft.Colors.OUTLINE),
                            border_radius=8,
                            padding=10,
                            expand=2,
                            bgcolor=ft.Colors.SURFACE_CONTAINER_HIGHEST
                        ),
                    ],
                    expand=True,
                ),
            ],
            expand=True,
            vertical_alignment=ft.CrossAxisAlignment.STRETCH,
        ),
    )

    threading.Thread(target=quotes_refresh_loop, daemon=True).start()

ft.run(main)
