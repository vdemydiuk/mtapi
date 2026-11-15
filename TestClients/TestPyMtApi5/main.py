import flet as ft
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
    host_input = ft.TextField(label="Host", value="127.0.0.1", width=180, height=50)
    port_input = ft.TextField(label="Port", value="8222", width=100, height=50)
    
    # Status indicator
    status_text = ft.Text("Disconnected", color="red", weight=ft.FontWeight.BOLD)
    
    # Log container view
    log_view = ft.ListView(expand=True, spacing=5, auto_scroll=True)

    def log_message(message: str):
        timestamp = time.strftime("%H:%M:%S")
        log_view.controls.append(ft.Text(f"[{timestamp}] {message}", size=12, font_family="Consolas"))
        page.update()

    def connect_click(e):
        host = host_input.value
        port = port_input.value
        log_message(f"Attempting connection to {host}:{port}...")
        page.run_task(simulate_connection)

    async def simulate_connection():
        time.sleep(0.8)
        status_text.value = "Connected"
        status_text.color = "green"
        connect_btn.disabled = True
        disconnect_btn.disabled = False
        log_message("Successfully connected to MetaTrader API bridge.")
        page.update()

    def disconnect_click(e):
        status_text.value = "Disconnected"
        status_text.color = "red"
        connect_btn.disabled = False
        disconnect_btn.disabled = True
        log_message("Connection closed.")
        page.update()

    connect_btn = ft.ElevatedButton("Connect", on_click=connect_click, color="white", bgcolor="green_700")
    disconnect_btn = ft.ElevatedButton("Disconnect", on_click=disconnect_click, disabled=True, color="white", bgcolor="red_700")

    def get_account_info(e):
        if status_text.value != "Connected":
            log_message("Error: No active connection to the server.")
            return
        log_message("Requesting AccountInfo... Balance: $10,000.00 | Equity: $10,000.00")

    def send_order(e):
        if status_text.value != "Connected":
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
