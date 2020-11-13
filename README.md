## Introduction
MtApi provides a .NET API for working with famous trading platfrom [MetaTrader(MetaQuotes)](https://www.metatrader5.com/).  
It is not API for connection to MT servers directly. MtApi is just a bridge between MT terminal and .NET applications designed by developers.  
MtApi executes MQL commands and functions by MtApi's expert linked to chart of MetaTrader.  
Most of the API's functions duplicates MQL interface.  
The project was designed using [WCF](https://docs.microsoft.com/en-us/dotnet/framework/wcf/whats-wcf) framework with the intention of using flexibility to setup connections.

## Build environment
The project is supported by Visual Studio 2017.  
It requires WIX Tools for preparing project's installers (http://wixtoolset.org/).

Installing WIX for mtapi:
1. Make sure you install one of the latest (3.14+) development releases of the wixtoolset.
(If you use an older installer you will have to install the ancient .NET 3.5 framework, and that I am sure you will regret, if you do!).
2. Run the installer and wait for completion or for asking to also install the VS extensions.

![alt text](https://user-images.githubusercontent.com/52289379/97868674-c8c97a80-1d18-11eb-89f3-cdef9d9cc02f.png)

3. Install the WiX Toolset Visual Studio Extension depending on your VS version.
For example, if you use VS 2017, go [here](https://marketplace.visualstudio.com/items?itemName=WixToolset.WixToolsetVisualStudio2017Extension) or download from their GitHub, releases.

Use [MetaEditor](https://www.metatrader5.com/en/automated-trading/metaeditor) to working with MQL files.

## How to Build Solution

To build the solution for **MT4**, you need to choose the configuration to build for **`x86`** 
and start with building the **`MtApiInstaller`**. This will build all projects related to MT4:  
- `MtApi` 
- `MTApiService` 
- `MTConnector`

For building the solution for MT5, you need to choose the configuration to build for **`x64`** (or **`x86`** for the 32-bit MT5)) and start build **`MtApi5Installer`**.  This will build all projects related to MT5:  
- `MtApi5` 
- `MTApiService` 
- `MT5Connector`

All binaries are placed in the project root folder, in the *build* directory:   **`../build/`**.   
The installers (**`*.msi, *.exe`**) will be found under:  **`../build/installers/`**.   
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

* `MTApiService`: **`(C#, .dll)`**  
The common engine communication  project of the API. It contains the implementations of client and server sides.  

* `MTConnector, MT5Connector`: **`(C++/CLI, .dll)`**   
The libraries  that are working as proxy between MQL and C# layers. They provides the interfaces.

* `MtApi, MtApi5`: **`(C#, .dll)`**  
The client side libraries that are using in user's projects.  

* **`(MQL4/MQL5, .ex4)`**  
MT4 and MT5 *Expert Advisors* linked to terminal's charts.  They executes API's functions and provides trading events.  

* `MtApiInstaller, MtApi5Installer` **`(WIX, .msi)`**  
The project's installers.  

* `MtApiBootstrapper, MtApi5Bootstrapper` **`(WIX, .exe)`**  
The installation package bundles.  There are wrappers for installers that contains the vc_redist 
libraries (Visual C++ runtime) placed in [root]\vcredist\.


## Installation

Use the installers to setup all libraries automatically.  
- For *MT4*, use:  `MtApiInstaller_setup.exe`  
- For *MT5*, use:  `MtApi5Installer setup.exe`

`MtApiBootstrapper` and `MtApi5Bootstrapper` are installation package bundles that contain the installers and `vc_redist` Windows libraries. The installers place the `MTApiService.dll` into the Windows GAC (*Global Assembly Cache*) and copies `MTConnector.dll` and `MT5Connector.dll` into the Windows's system folder, whose location depend on your Windows OS. After installation, the **`MtApi.ex4`**  (or **`MtApi5.ex5`**) EA, must be copied into your Terminal data folder for Expert Advisors, which is normally located in: **`../MQL5/Experts/`**.

To quickly navigate to the trading platform *data folder*, click:  **`File >> "Open data folder"`** in your *MetaTrader* Terminal.

## Using
MtApi provides two types of connection to MetaTrader terminal: local (using Pipe or TCP) and remote (via TCP).  
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


# Telegram Channel
https://t.me/mtapi4

https://t.me/joinchat/GfnfUxvelQCLvvIvLO16-w
