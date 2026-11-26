## Introduction
MtApi provides an API for working with famous trading platform [MetaTrader(MetaQuotes)](https://www.metatrader5.com/) from .NET applications (MT4 and MT5) and from Python (MT5).  
It is not API for connection to MT servers directly. MtApi is just a bridge between MT terminal and external applications (.NET, Python) designed by developers.  
MtApi executes MQL commands and functions by MtApi's expert linked to chart of MetaTrader.  
Most of the API's functions duplicates MQL interface.  
The project was designed using WebSocket framework with the intention of using flexibility to setup connections.

## Build environment
The project is supported by Visual Studio 2026 and based on .NET 10. Open `MetaTraderApi.slnx` in Visual Studio.
It requires HeatWave for VS2026 - set of tools for creating WiX installation packages (https://docs.firegiant.com/heatwave/).
Just install the VS extensions.

Use [MetaEditor](https://www.metatrader5.com/en/automated-trading/metaeditor) to working with MQL files.

## How to Build Solution

To build the solution for **MT4**, you need to choose the configuration to build for **`x86`** 
and start with building the **`MtApiInstaller`**. This will build all projects related to MT4:  
- `MtApi`
- `MtClient`
- `MtService`
- `MTConnector`

For building the solution for MT5, you need to choose the configuration to build for **`x64`** (or **`x86`** for the 32-bit MT5)) and start build **`MtApi5Installer`**.  This will build all projects related to MT5:  
- `MtApi5` 
- `MtClient`
- `MtService` 
- `MT5Connector`

All binaries are placed in the project root folder, in the *build* directory:   **`../build/`**.   
The installers (**`*.msi) will be found under:  **`../build/installers/`**.   
All the DLL library binaries (**`*.dll`**) in:  **`../bin/`**.

MQL files have been pre-compiled to **`*.ex4`** and can be found in the repository here:  
- **`..\mql4\`**  
- **`..\mql5\`**

Changing the source code of the MQL *Expert Advisor* (EA), requires recompilation with *`MetaEditor`*.  

Before you can recompile the EA, you need to add/place the following MQL library files, in the *MetaEditor* **`../Include/`** folder.
- **`hash.mqh`**
- **`json.mqh`**

The *`MetaEditor`* *include* folder is usually located here:  
```xml
C:\Users\<username>\AppData\Roaming\MetaQuotes\Terminal\<terminal-hash>\MQL5\Include\.
```


## Project Structure

* `MtService`: **`(C++, .dll)`**  
The common engine communication  project of the API. It contains the implementations of server sides based on WebSocket (from Boost library).

* `MtClient`: **`(C#, .dll)`**  
The client side communication  project of the API (based on WebSocket).

* `MTConnector, MT5Connector`: **`(C++, .dll)`**   
The libraries  that are working as proxy between MQL and C# layers. They provides the API interfaces for working with MQL experts.

* `MtApi, MtApi5`: **`(C#, .dll)`**  
The client side libraries that are using in user's projects.  

* **`(MQL4/MQL5, .ex4/.ex5)`**  
MT4 and MT5 *Expert Advisors* linked to terminal's charts.  They executes API's functions and provides trading events.  

* `MtApi5.Tests` **`(C#, xUnit)`**  
Unit tests of the MtApi5 library (no MetaTrader terminal required). `MtApi5Client` is driven over an in-memory `FakeRpcClient`, so the tests check the command id and JSON payload sent for each API method and the conversion of replies. They also cover the SBE codec and schema, JSON protocol, converters, enums, model classes and the RPC logger. Run them with `dotnet test MtApi5.Tests/MtApi5.Tests.csproj`.  

* `PyMtApi5` **`(Python)`**  
A pure-Python client for MT5 (no .NET required). `Mt5ApiClient` talks to MtService over WebSocket using a hand-written SBE codec (`mtprotocol_sbe.py`) and exposes the MtApi5 functions with snake_case names (`buy`, `symbol_info_tick`, `copy_rates`, ...). Unit tests live in `PyMtApi5/tests/` and run without a terminal: `python -m unittest discover -s PyMtApi5/tests -t .` from the repository root.  

* `MtApiInstaller, MtApi5Installer` **`(WIX, .msi)`**  
The project's installers.  

* `Boost libraries` **`(*.lib)`**  
Precompiled x64 and x86 libraries for using with C++ projects, placed in [root]\thirdparty\.


## Communication Protocol (SBE)

`MtClient` and `MtService` communicate over WebSocket using binary messages encoded with
[Simple Binary Encoding (SBE)](https://github.com/real-logic/simple-binary-encoding).
Each WebSocket binary frame carries exactly one SBE message.

The protocol is defined by the schema **`specs/mtprotocol_sbe.xml`** (schema id `1`, version `1`, little-endian).
The codecs are generated from this schema with the SBE tool:
- C++ (`MtService`): `MtService/*.h` (`MtCommand.h`, `MtResponse.h`, `MessageHeader.h`, ...)
- C# (`MtClient`): `MtClient/*.g.cs`

Do not edit the generated files by hand: change the schema and regenerate the codecs for both languages.

Every message starts with the standard SBE header (`blockLength`, `templateId`, `schemaId`, `version`, all `uint16`),
followed by the fixed-size fields, then repeating groups and variable-length data (`uint32` length prefix + UTF-8 bytes).

| Message               | Id  | Direction        | Fields                                                     | Description |
|-----------------------|-----|------------------|------------------------------------------------------------|-------------|
| `MtCommand`           | 100 | Client → Service | `expert_handle`, `command_id`, `command_type`, `stringPayload` | Command to be executed by the MetaTrader expert. `expert_handle = 0` means the default (first) expert. |
| `MtResponse`          | 101 | Service → Client | `expert_handle`, `command_id`, `status`, `stringPayload`   | Response to `MtCommand`, matched by `command_id`. |
| `MtEvent`             | 102 | Service → Client | `expert_handle`, `event_type`, `stringPayload`             | Event from MetaTrader (quotes, trade events, etc.). |
| `MtExpertList`        | 103 | Service → Client | `request_id`, group `expert_list` of `expert_handle`        | Response to `MtRequestExpertList` with handles of all connected experts. |
| `MtExpertAdded`       | 104 | Service → Client | `expert_handle`                                            | An expert has been attached to a chart. |
| `MtExpertRemoved`     | 105 | Service → Client | `expert_handle`                                            | An expert has been removed from a chart. |
| `MtRequestExpertList` | 106 | Client → Service | `request_id`                                               | Request of the current expert list. |

The `stringPayload` of commands, responses and events is a JSON string: SBE provides the framing and routing,
while the command parameters and results are serialized as JSON (as they are processed by the MQL expert).

`MtResponse.status` (`ResponseStatus`) reports the result of command delivery by `MtService`:
- `Ok` (0) – the command was executed by MetaTrader, the payload contains its response;
- `NoExperts` (1) – no expert is attached to `MtService`;
- `ExpertNotFound` (2) – there is no expert with the requested handle;
- `ExpertRemoved` (3) – the expert was removed before the command was executed.

For non-`Ok` statuses the payload contains an error message. Every command gets a response, so the client never waits for a timeout because of a missing expert.

Notes:
- Both sides validate received messages (header size, schema id, block lengths, payload bounds) and reject malformed ones.
- Messages with a newer schema version are accepted (SBE forward compatibility): unknown fields added in newer versions are skipped.
- `MtService` limits the send queue to 3000 events per connection: on overflow the oldest events are dropped. Responses are sent ahead of queued events and are never dropped.


## Installation

Use the installers to setup all libraries automatically.  
- For *MT4*, use:  `MtApiInstaller.msi`  
- For *MT5*, use:  `MtApi5Installer_x64.msi`

To quickly navigate to the trading platform *data folder*, click:  **`File >> "Open data folder"`** in your *MetaTrader* Terminal.

## Additional packets
VCRedist packet must be installed for correct working MtApi connectors (MTConnector or MT5Connector).
Use Microsoft website to download required installers:
https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170


## Using
MtApi provides two types of connection to MetaTrader terminal: local and remote (via TCP).  
The port of connection is defined by MtApi expert.

Console sample for MT5:
```C#
using System;
using System.Threading;
using System.Threading.Tasks;
using MtApi5;

namespace MtApi5Console
{
    class Program
    {
        static readonly EventWaitHandle _connnectionWaiter = new AutoResetEvent(false);
        static readonly MtApi5Client _mtapi = new MtApi5Client();

        static void _mtapi_ConnectionStateChanged(object sender, Mt5ConnectionEventArgs e)
        {
            switch (e.Status)
            {
                case Mt5ConnectionState.Connecting:
                    Console.WriteLine("Connnecting...");
                    break;
                case Mt5ConnectionState.Connected:
                    Console.WriteLine("Connnected.");
                    _connnectionWaiter.Set();
                    break;
                case Mt5ConnectionState.Disconnected:
                    Console.WriteLine("Disconnected.");
                    _connnectionWaiter.Set();
                    break;
                case Mt5ConnectionState.Failed:
                    Console.WriteLine("Connection failed.");
                    _connnectionWaiter.Set();
                    break;
            }
        }

        static void _mtapi_QuoteAdded(object sender, Mt5QuoteEventArgs e)
        {
            Console.WriteLine("Quote added with symbol {0}", e.Quote.Instrument);
        }

        static void _mtapi_QuoteRemoved(object sender, Mt5QuoteEventArgs e)
        {
            Console.WriteLine("Quote removed with symbol {0}", e.Quote.Instrument);
        }

        static void _mtapi_QuoteUpdate(object sender, Mt5QuoteEventArgs e)
        {
            Console.WriteLine("Quote updated: {0} - {1} : {2}", e.Quote.Instrument, e.Quote.Bid, e.Quote.Ask);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Application started.");

            _mtapi.ConnectionStateChanged += _mtapi_ConnectionStateChanged;
            _mtapi.QuoteAdded += _mtapi_QuoteAdded;
            _mtapi.QuoteRemoved += _mtapi_QuoteRemoved;
            _mtapi.QuoteUpdate += _mtapi_QuoteUpdate;

            _mtapi.BeginConnect(8228);
            _connnectionWaiter.WaitOne();

            if (_mtapi.ConnectionState == Mt5ConnectionState.Connected)
            {
                Run();
            }

            Console.WriteLine("Application finished. Press any key...");
            Console.ReadKey();
        }

        private static void Run()
        {
            ConsoleKeyInfo cki;
            do
            {
                cki = Console.ReadKey();
                switch (cki.KeyChar.ToString())
                {
                    case "b":
                        Buy();
                        break;
                    case "s":
                        Sell();
                        break;
                }
            } while (cki.Key != ConsoleKey.Escape);

            _mtapi.BeginDisconnect();
            _connnectionWaiter.WaitOne();
        }

        private static async void Buy()
        {
            const string symbol = "EURUSD";
            const double volume = 0.1;
            MqlTradeResult tradeResult = null;
            var retVal = await Execute(() => _mtapi.Buy(out tradeResult, volume, symbol));
            Console.WriteLine($"Buy: symbol EURUSD retVal = {retVal}, result = {tradeResult}");
        }

        private static async void Sell()
        {
            const string symbol = "EURUSD";
            const double volume = 0.1;
            MqlTradeResult tradeResult = null;
            var retVal = await Execute(() => _mtapi.Sell(out tradeResult, volume, symbol));
            Console.WriteLine($"Sell: symbol EURUSD retVal = {retVal}, result = {tradeResult}");
        }

        private static async Task<TResult> Execute<TResult>(Func<TResult> func)
        {
            return await Task.Factory.StartNew(() =>
            {
                var result = default(TResult);
                try
                {
                    result = func();
                }
                catch (ExecutionException ex)
                {
                    Console.WriteLine($"Exception: {ex.ErrorCode} - {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }

                return result;
            });
        }
    }
}

```


## Python client (MT5)
`PyMtApi5` is a Python client for MT5 that connects directly to the MtApi5 expert (local or remote via TCP), so it does not need the .NET libraries. It requires the `websockets` package (`pip install websockets`).  
Copy or reference the `PyMtApi5` folder in your project (it must be importable as a package) and use `Mt5ApiClient` as a context manager or call `connect()`/`disconnect()` explicitly:

```python
from PyMtApi5 import Mt5ApiClient

with Mt5ApiClient("127.0.0.1", 8228) as client:
    print(client.get_quotes())
    tick = client.symbol_info_tick("EURUSD")
    print(tick)
```

Pass an optional `callback` object as the third argument to receive events (`on_quote_update`, `on_quote_added`, `on_quote_removed`, `on_disconnect`, `on_book_event`, `on_last_time_bar`, `on_trade_transaction`).  
The port is the one configured in the MtApi5 expert.

A GUI test client built with [Flet](https://flet.dev/) is available in `TestClients/TestPyMtApi5` (`pip install -r requirements.txt`, then `python main.py`).  
`Examples/Python/Python_to_MT5` shows an alternative way: using the C# `MtApi5.dll` from Python through `pythonnet`.

## Contributing
Development happens on the `dev` branch; `master` is used only for releases.

- Create your branch from `dev`.
- Open pull requests against `dev`, not `master`.

# Telegram Channel
https://t.me/joinchat/GfnfUxvelQCLvvIvLO16-w
