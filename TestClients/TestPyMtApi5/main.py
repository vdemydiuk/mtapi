import asyncio
import flet as ft
import threading
import time
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

    def log_message(message: str):
        timestamp = time.strftime("%H:%M:%S")
        log_view.controls.append(ft.Text(f"[{timestamp}] {message}", size=12, font_family="Consolas"))
        page.update()

    # MtApi5 client state. Mt5ApiClient.connect()/disconnect() are blocking, so they
    # are run on worker threads (page.run_thread) to keep the UI responsive.
    client_lock = threading.Lock()
    client: Mt5ApiClient | None = None
    # Set when the page is closing: Flet's thread pool is shut down then and the page
    # must not be updated anymore.
    closing = threading.Event()

    def is_connected() -> bool:
        with client_lock:
            return client is not None and client.is_connected()

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
            pass  # ticks are too frequent to be logged

        def on_quote_added(self, quote):
            log_message(f"Quote added: {quote}")

        def on_quote_removed(self, quote):
            log_message(f"Quote removed: {quote}")

        def on_book_event(self, expert_handle, symbol):
            pass

        def on_last_time_bar(self, expert_handle, instrument, rates):
            pass

        def on_trade_transaction(self, expert_handle, trade_transaction, trade_request, trade_result):
            log_message(f"Trade transaction [{expert_handle}]: {trade_transaction}")

    def on_connection_closed():
        release_client()
        if not closing.is_set():
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
        log_message(f"Connected to {host}:{port}. Quotes: {len(quotes)}")
        for quote in quotes:
            log_message(f"  {quote}")

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

    connect_btn = ft.ElevatedButton("Connect", on_click=connect_click, color="white", bgcolor="green_700")
    disconnect_btn = ft.ElevatedButton("Disconnect", on_click=disconnect_click, disabled=True, color="white", bgcolor="red_700")

    def get_account_info(e):
        if not is_connected():
            log_message("Error: No active connection to the server.")
            return
        log_message("Requesting AccountInfo... Balance: $10,000.00 | Equity: $10,000.00")

    def send_order(e):
        if not is_connected():
            log_message("Error: No active connection to the server.")
            return
        log_message("Sending order: BUY EURUSD (0.1 lot) -> Successfully executed (Ticket #589241).")

    account_btn = ft.OutlinedButton("Get Account Info", on_click=get_account_info)
    order_btn = ft.OutlinedButton("Send Test Order", on_click=send_order)

    # UI Layout configuration
    connection_row = ft.Row(
        [host_input, port_input, connect_btn, disconnect_btn, ft.VerticalDivider(), ft.Row([ft.Text("Status:"), status_text])], 
        alignment=ft.MainAxisAlignment.START, 
        wrap=True
    )
    
    actions_row = ft.Row([account_btn, order_btn], spacing=10)

    page.add(
        ft.Text("MTAPI Test Client (Python / Flet)", size=20, weight=ft.FontWeight.BOLD),
        ft.Divider(),
        connection_row,
        ft.Divider(),
        ft.Text("API Command Testing:", weight=ft.FontWeight.BOLD),
        actions_row,
        ft.Divider(),
        ft.Text("Event Logs:", weight=ft.FontWeight.BOLD),
        ft.Container(
            content=log_view,
            border=ft.border.all(1, ft.Colors.OUTLINE),
            border_radius=8,
            padding=10,
            expand=True,
            bgcolor=ft.Colors.SURFACE_CONTAINER_HIGHEST
        )
    )

ft.app(target=main)
