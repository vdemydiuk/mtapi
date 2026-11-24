# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MtApi is a .NET bridge that lets .NET applications control MetaTrader 4 and MetaTrader 5 trading terminals via WebSocket RPC. It consists of four language layers working together: MQL Expert Advisors → C++ connector DLLs → C++ WebSocket server → C# client library.

## Build

**Requirements:** Visual Studio 2022, .NET 8 SDK, HeatWave VS extension (for installers), Boost headers in `thirdparty/include/`.

Open `MetaTraderApi_2022.sln` in Visual Studio 2022:
- **MT4**: select `x86` platform, build `MtApiInstaller` (builds MtApi, MtClient, MtService, MTConnector)
- **MT5**: select `x64` platform, build `MtApi5Installer` (builds MtApi5, MtClient, MtService, MT5Connector)

Build outputs go to `../build/products/{Configuration}/{Platform}/` and installers to `../build/installers/`.

## Architecture

The communication stack flows top-to-bottom:

```
.NET App
  └─ MtApi / MtApi5 (C# library — public trading API surface)
       └─ MtClient (C# WebSocket RPC client)
            └─ [WebSocket / TCP]
                 └─ MtService (C++ Boost.Asio + Beast WebSocket server)
                      └─ MTConnector / MT5Connector (C++ DLL loaded by MT terminal)
                           └─ MtApi.mq4 / MtApi5.mq5 (MQL Expert Advisor in terminal)
```

**MtApi / MtApi5** (`MtApi/`, `MtApi5/`) — The C# library .NET consumers reference. Exposes event-driven quotes, trading operations (Buy, Sell, OrderClose, OrderModify), account info, indicators, and order history. MT4 and MT5 implementations are largely parallel but kept separate.

**MtClient** (`MtClient/`) — Shared by both MT4 and MT5. Handles WebSocket connection lifecycle and JSON-serialized RPC message framing over `MtRpcClient`.

**MtService** (`MtService/`) — C++ server using Boost.Asio for async I/O and Boost.Beast for WebSocket. Receives commands from MtClient and dispatches them to the connector layer.

**MTConnector / MT5Connector** (`MTConnector/`, `MT5Connector/`) — C++ DLLs that are imported by the MQL Expert Advisor. They bridge native MQL function calls to the MtService layer.

**MQL scripts** (`mql4/MtApi.mq4`, `mql5/MtApi5.mq5`) — Expert Advisors attached to a chart in the MetaTrader terminal. Pre-compiled `.ex4`/`.ex5` binaries are committed; source requires MetaEditor to recompile.

**Monitors** (`MtApi/Monitors/`, `MtApi5/Monitors/`) — `TradeMonitor` and `ModifiedOrdersMonitor` poll for order/position changes and fire events to subscribers. See `MtApi/Monitors/readme.md`.

**SBE Protocol** (`specs/mtprotocol_sbe.xml`) — Simple Binary Encoding specification that defines the binary message format of the WebSocket RPC between MtService and the clients. Generated codecs live in `MtService/*.h` (C++) and `MtClient/*.g.cs` (C#).

**PyMtApi5** (`PyMtApi5/`) — Python client for MT5. `mtrpcclient.py` is the Python counterpart of `MtClient/MtRpcClient.cs`; it uses the SBE codec in `mtprotocol_sbe.py`, which is hand-written (the SBE tool has no Python generator, and the spec is small enough that an extra library isn't worth it).

## Testing

**Unit tests (MT5 only):** `MtApi5.Tests/` is an xUnit project; run with `dotnet test MtApi5.Tests/MtApi5.Tests.csproj`. It needs no MetaTrader terminal: `MtApi5Client` is driven over an in-memory `FakeRpcClient` (see `MtApi5ClientTestHelper.Connect()`), and tests assert the command id and JSON payload sent plus reply conversion. It also covers the SBE codec/schema, JSON protocol, converters, enums, models and the RPC logger. Add a test whenever you add or fix an `MtApi5Client` method. There are no unit tests for MtApi (MT4) yet.

**Python unit tests:** `PyMtApi5/tests/` (stdlib `unittest`, no terminal needed) drives `Mt5ApiClient` over a fake RPC client and asserts the command id and JSON payload sent plus reply conversion. Run from the repo root: `python -m unittest discover -s PyMtApi5/tests -t .`. Currently covers the trade, position and order functions; add a test when you add or fix a `Mt5ApiClient` method.

**Manual testing** against a real terminal is done via GUI test client apps:

- `TestClients/TestApiClientUI/` — MT4 test client
- `TestClients/MtApi5TestClient/` — MT5 test client
- `TestClients/TestMtApi/TestMtApi.sln` — standalone test solution

Example integrations are in `Examples/MtApi5Samples/`.

## Key Conventions

- MT4 and MT5 implementations are intentionally parallel and separate — do not try to unify them.
- MQL source changes must be recompiled in MetaEditor; only commit the resulting `.ex4`/`.ex5` binaries.
- The SBE-generated files (`MtService/*.h` and `MtClient/*.g.cs`) are derived from `specs/mtprotocol_sbe.xml` — edit the spec, then regenerate rather than hand-editing generated files.
- `PyMtApi5/mtprotocol_sbe.py` is not generated: whenever `specs/mtprotocol_sbe.xml` changes, update it by hand to match (message ids, field order/types, block lengths). Do not replace it with a third-party SBE library or a Python code generator unless the spec grows significantly.
