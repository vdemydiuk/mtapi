# MtApi - .NET API for MetaTrader trading platform (MetaQuotes).
MtApi provides an .NET API interface to work with famous trading platfroms MetaTrader4 and MetaTrader5 (MetaQuotes).
The api is using directly connection to MetaTrader terminal and is working with MQL functions and the most functions of the api have MQL interface.
The connection can be local or remote (by TCP).

# MtApi structure
The project has two parts: 
 - client side (C#): MtApi and MtApi5;
 - server side (C# and C++/CLI): MTApiService, MTConnector, MT5Connector, MQL experts.
Server side was designed with using WCF framework so it can be flaxible to setup connections but can be more slow compared with another connections types (for example, shared memory).
MTApiService is common engine communication project of the API for MT4 and MT5. 
MTApiService library should be placed in Windows GAC (Global Assembly Cache). Installers in the project will copied it to GAC automatically.

# How to build solution
The project is supported by Visual Studio 2015. It also requires WIX Tools (http://wixtoolset.org/).
To build solution you need to update sign key file in MtApiService project: 
- open properties of MTApiService project;
- go to tab Signing and select item MtApiKey.pfx;
- input password "MtApiService".
To make api for MetaTrader4 use MtApiInstaller and for MetaTrader5 use MtApi5Installer. 
All installers will be placed in folder "[root]\build\installers\" and all *.dll files will be placed in "[root]\build\products\".
MQL files have been build to ex4 and stored into folders "mq4" for MetaTrader and "mq5" for MetaTrader5. They are ready to using in terminals.
If you change source code of MQL expert you have to recompile it with MetaEditor. In this case you need to copy files "hash.mqh" and "json.mqh" to MetaEditor include folder.

# Home website
Please visit http://mtapi4.net
