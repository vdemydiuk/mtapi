# MtApi - .NET API for MetaTrader Trading Platform (MetaQuotes).
MtApi provides a .NET API to work with famous trading platfroms MetaTrader4 and MetaTrader5 (MetaQuotes).
The API connects directly to the MetaTrader terminal and works with MQL functions. Most functions of the API have MQL interface.
The connection can be local or remote (by TCP).

# MtApi Structure
The project has two parts: 
 - client side (C#): MtApi and MtApi5;
 - server side (C# and C++/CLI): MTApiService, MTConnector, MT5Connector, MQL experts.
Server side was designed using WCF framework with the intention of using flexibility to setup connections.  One downside of using the WCF framework is the slower speed compared to other connection types (for example, shared memory).
MTApiService is a common engine communication project of the API for MT4 and MT5.
MTApiService library should be placed in Windows GAC (Global Assembly Cache). Installers in the project will be copied to GAC automatically.

# How to Build Solution
The project is supported by Visual Studio 2017 and requires WIX Tools (http://wixtoolset.org/).

To make an API for MetaTrader4 use MtApiInstaller and for MetaTrader5 use MtApi5Installer. 
All installers will be placed in the folder "[root]\build\installers\" and all *.dll files will be placed in "[root]\build\products\".
MQL files have been build to ex4 and stored into folders "mq4" for MetaTrader and "mq5" for MetaTrader5. They are ready to be used in terminals.
Changing the source code of MQL expert requires recompilation with MetaEditor. Resulting in the need to copy files "hash.mqh" and "json.mqh" to the MetaEditor include folder.

# Home Website
Please visit http://mtapi4.net
