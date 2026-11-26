# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MtApi is a .NET bridge that lets .NET applications control MetaTrader 4 and MetaTrader 5 trading terminals via WebSocket RPC. It consists of four language layers working together: MQL Expert Advisors → C++ connector DLLs → C++ WebSocket server → C# client library.

## Build

**Requirements:** Visual Studio 2026 (C++ toolset `v145`), .NET 10 SDK, HeatWave VS extension (for installers), Boost headers in `thirdparty/include/`.

Open `MetaTraderApi.slnx` in Visual Studio 2026:
- **MT4**: select `x86` platform, build `MtApiInstaller` (builds MtApi, MtClient, MtService, MTConnector)
- **MT5**: select `x64` platform, build `MtApi5Installer` (builds MtApi5, MtClient, MtService, MT5Connector)

Build outputs go to `../build/products/{Configuration}/{Platform}/` and installers to `../build/installers/`.

**MQL experts on Linux:** `scripts/build_mql.sh` compiles `mq5/MtApi5.mq5` and `mq4/MtApi.mq4` with MetaEditor under Wine and copies the `.ex5`/`.ex4` into the repo; `--check` compiles without copying, and `mq5`/`mq4` arguments pick one expert. It builds each expert whose terminal is found: MT5 in the Wine prefix `~/.mt5`, MT4 in `~/.mt4` (override with `MT5_WINEPREFIX`/`MT5_DIR`, `MT4_WINEPREFIX`/`MT4_DIR`). Exit code: 0 ok, 1 compile error, 2 Wine or MetaEditor missing. MetaEditor's own exit code is unreliable (1 on success), so the script reads `Result: N errors` from the build log.

One-time setup (Ubuntu; other distros: same steps with their WineHQ repo):

1. Install WineHQ `winehq-staging` (Wine 11+). Ubuntu's own `wine` 10 cannot start `terminal64.exe`/`MetaEditor64.exe` (exit 222, `NtRaiseHardError`). apt rejects the ASCII-armored key, so dearmor it, and use the repo for your release (`resolute` = 26.04), not the one MetaQuotes' `mt5linux.sh` picks (that script also runs `apt upgrade -y`):
   ```bash
   sudo dpkg --add-architecture i386
   curl -fsSL https://dl.winehq.org/wine-builds/winehq.key | sudo gpg --yes --dearmor -o /etc/apt/keyrings/winehq-archive.key
   sudo wget -NP /etc/apt/sources.list.d/ https://dl.winehq.org/wine-builds/ubuntu/dists/$(. /etc/os-release; echo $VERSION_CODENAME)/winehq-$(. /etc/os-release; echo $VERSION_CODENAME).sources
   sudo apt update && sudo apt install -y --install-recommends winehq-staging xvfb
   ```
   (`xvfb` lets the script run MetaEditor without a display.)
2. MT5: `WINEPREFIX=~/.mt5 winecfg -v win11`, install WebView2 (`MicrosoftEdgeWebview2Setup.exe /silent /install`, link in `mt5linux.sh`), then `WINEPREFIX=~/.mt5 wine mt5setup.exe /auto` with `mt5setup.exe` from `https://download.mql5.com/cdn/web/metaquotes.software.corp/mt5/mt5setup.exe`.
3. MT4: `WINEPREFIX=~/.mt4 winecfg -v win10`, then `WINEPREFIX=~/.mt4 wine mt4setup.exe /auto` with `mt4setup.exe` from `https://download.mql5.com/cdn/web/metaquotes.software.corp/mt4/mt4setup.exe` (still a real MT4 installer).
4. Start each terminal once in portable mode so it unpacks the standard library (`MQL5/Include`, `MQL4/Include` with `WinUser32.mqh`, `stdlib.mqh`) next to the exe, then close it (`wineserver -k`). The setup's `/auto` run alone does not create it:
   ```bash
   cd ~/.mt5/drive_c/Program\ Files/MetaTrader\ 5 && WINEPREFIX=~/.mt5 wine terminal64.exe /portable
   cd ~/.mt4/drive_c/Program\ Files\ \(x86\)/MetaTrader\ 4 && WINEPREFIX=~/.mt4 wine terminal.exe /portable
   ```
5. Check: `scripts/build_mql.sh --check` should report `0 errors` for both experts.

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

**Python unit tests:** `PyMtApi5/tests/` (stdlib `unittest`, no terminal needed) drives `Mt5ApiClient` over a fake RPC client and asserts the command id and JSON payload sent plus reply conversion. Run from the repo root: `python -m unittest discover -s PyMtApi5/tests -t .`. Currently covers the trade, position, order, history, global variable, chart, chart object, terminal info, time, indicator (iXxx) and common functions; add a test when you add or fix a `Mt5ApiClient` method.

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

## Branching and Pull Requests

- `dev` is the development branch: all development work happens on `dev` (or feature branches created from `dev`).
- Pull requests must target `dev`, never `master` (e.g. `gh pr create --base dev`).
- `master` is used only for releases; changes reach it by merging `dev` at release time.
