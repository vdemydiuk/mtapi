# PyMtApi5

Python client for [MetaTrader 5](https://www.metatrader5.com/) that talks to the MtApi5 expert over WebSocket. It is part of [MtApi](https://github.com/vdemydiuk/mtapi) and needs no .NET libraries.

The terminal side still comes from MtApi: install `MtApi5Installer` (it copies the `MtApi5.ex5` expert and `MT5Connector.dll`), attach the MtApi5 expert to a chart and set its port. The Python client can run on the same machine or connect to the terminal remotely over TCP.

## Install

```bash
pip install pymtapi5
```

Use the package version that matches the MtApi5 release installed on the terminal side.

## Usage

```python
from PyMtApi5 import Mt5ApiClient

with Mt5ApiClient("127.0.0.1", 8228) as client:
    print(client.get_quotes())
    tick = client.symbol_info_tick("EURUSD")
    print(tick)
```

`Mt5ApiClient` works as a context manager or with explicit `connect()`/`disconnect()`. Pass an optional `callback` object as the third argument to receive events (`on_quote_update`, `on_quote_added`, `on_quote_removed`, `on_disconnect`, `on_book_event`, `on_last_time_bar`, `on_trade_transaction`).

The functions mirror MQL5 with snake_case names (`buy`, `order_send`, `symbol_info_tick`, `copy_rates`, `i_close`, ...). See the [MtApi repository](https://github.com/vdemydiuk/mtapi) for documentation and a GUI test client.

## License

MIT
