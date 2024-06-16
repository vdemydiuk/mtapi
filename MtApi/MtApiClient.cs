using System.Drawing;
using System.Collections;
using Newtonsoft.Json;
using MtApi.Events;
using MtApi.MtProtocol;
using MtClient;

namespace MtApi
{
    public delegate void MtApiQuoteHandler(object sender, string symbol, double bid, double ask);

    public sealed class MtApiClient
    {
        #region MetaTrader Constants

        //Special constant
        public const int NULL = 0;
        public const int EMPTY = -1;
        #endregion

        #region Private Fields
        private IMtLogger Log { get; }

        private MtRpcClient? _client;
        private readonly object _locker = new();

        private readonly Dictionary<MtEventTypes, Action<int, string>> _mtEventHandlers = [];
        private HashSet<int> _experts = [];
        private Dictionary<int, MtQuote> _quotes = [];
        private readonly EventWaitHandle _quotesWaiter = new AutoResetEvent(false);

        private MtConnectionState _connectionState = MtConnectionState.Disconnected;
        private int _executorHandle;
        #endregion

        #region ctor


        public MtApiClient(IMtLogger? log = null)
        {
            _mtEventHandlers[MtEventTypes.ChartEvent] = ReceiveOnChartEvent;
            _mtEventHandlers[MtEventTypes.LastTimeBar] = ReceivedOnLastTimeBarEvent;
            _mtEventHandlers[MtEventTypes.OnLockTicks] = ReceivedOnLockTicksEvent;
            _mtEventHandlers[MtEventTypes.OnTick] = ReceivedOnTickEvent;

            Log = log ?? new StubMtLogger();
        }
        #endregion

        #region Public Methods
        ///<summary>
        ///Connect with MetaTrader API. Async method.
        ///</summary>
        ///<param name="host">Address of MetaTrader host (ex. 192.168.1.2)</param>
        ///<param name="port">Port of host connection (default 8222) </param>
        public void BeginConnect(string host, int port)
        {
            Log.Info($"BeginConnect: host = {host}, port = {port}");
            Task.Factory.StartNew(() => Connect(host, port));
        }

        ///<summary>
        ///Connect with MetaTrader API. Async method.
        ///</summary>
        ///<param name="port">Port of host connection (default 8222) </param>
        public void BeginConnect(int port)
        {
            Log.Info($"BeginConnect: localhost, port = {port}");
            Task.Factory.StartNew(() => Connect("localhost", port));
        }

        ///<summary>
        ///Disconnect from MetaTrader API. Async method.
        ///</summary>
        public void BeginDisconnect()
        {
            Log.Info("BeginDisconnect called.");
            Task.Factory.StartNew(() => Disconnect(false));
        }

        ///<summary>
        ///Load quotes connected into MetaTrader API.
        ///</summary>
        public List<MtQuote> GetQuotes()
        {
            _quotesWaiter.WaitOne(10000); // wait 10 sec for loading all quotes from MetaTrader
            lock (_locker)
            {
                return _quotes.Values.ToList();
            }
        }
        #endregion

        #region Properties

        ///<summary>
        ///Connection status of MetaTrader API.
        ///</summary>
        public MtConnectionState ConnectionState
        {
            get
            {
                lock (_locker)
                {
                    return _connectionState;
                }
            }
        }

        ///<summary>
        ///Handle of expert used to execute commands
        ///</summary>
        public int ExecutorHandle
        {
            get
            {
                lock (_locker)
                {
                    return _executorHandle;
                }
            }
            set
            {
                lock (_locker)
                {
                    _executorHandle = value;
                }
            }
        }
        #endregion


        #region Deprecated Methods
        [Obsolete("OrderCloseByCurrentPrice is deprecated, please use OrderClose instead.")]
        public bool OrderCloseByCurrentPrice(int ticket, int slippage)
        {
            return OrderClose(ticket, slippage);
        }

        [Obsolete("OrderClosePrice is deprecated, please use GetOrder instead.")]
        public double OrderClosePrice()
        {
            return SendCommand<double>(ExecutorHandle, MtCommandType.OrderClosePrice);
        }

        [Obsolete("OrderClosePrice is deprecated, please use GetOrder instead.")]
        public double OrderClosePrice(int ticket)
        {
            Dictionary<string, object> cmdParams = new() { { "Ticket", ticket } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.OrderClosePriceByTicket, cmdParams);
        }

        [Obsolete("OrderCloseTime is deprecated, please use GetOrder instead.")]
        public DateTime OrderCloseTime()
        {
            var commandResponse = SendCommand<int>(ExecutorHandle, MtCommandType.OrderCloseTime);
            return MtApiTimeConverter.ConvertFromMtTime(commandResponse);
        }

        [Obsolete("OrderComment is deprecated, please use GetOrder instead.")]
        public string? OrderComment()
        {
            return SendCommand<string>(ExecutorHandle, MtCommandType.OrderComment);
        }

        [Obsolete("OrderCommission is deprecated, please use GetOrder instead.")]
        public double OrderCommission()
        {
            return SendCommand<double>(ExecutorHandle, MtCommandType.OrderCommission);
        }

        [Obsolete("OrderExpiration is deprecated, please use GetOrder instead.")]
        public DateTime OrderExpiration()
        {
            var commandResponse = SendCommand<int>(ExecutorHandle, MtCommandType.OrderExpiration);
            return MtApiTimeConverter.ConvertFromMtTime(commandResponse);
        }

        [Obsolete("OrderLots is deprecated, please use GetOrder instead.")]
        public double OrderLots()
        {
            return SendCommand<double>(ExecutorHandle, MtCommandType.OrderLots);
        }

        [Obsolete("OrderMagicNumber is deprecated, please use GetOrder instead.")]
        public int OrderMagicNumber()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.OrderMagicNumber);
        }

        [Obsolete("OrderOpenPrice is deprecated, please use GetOrder instead.")]
        public double OrderOpenPrice()
        {
            return SendCommand<double>(ExecutorHandle, MtCommandType.OrderOpenPrice);
        }

        [Obsolete("OrderOpenPrice is deprecated, please use GetOrder instead.")]
        public double OrderOpenPrice(int ticket)
        {
            Dictionary<string, object> cmdParams = new() { { "Ticket", ticket } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.OrderOpenPriceByTicket, cmdParams);
        }

        [Obsolete("OrderOpenTime is deprecated, please use GetOrder instead.")]
        public DateTime OrderOpenTime()
        {
            var commandResponse = SendCommand<int>(ExecutorHandle, MtCommandType.OrderOpenTime);
            return MtApiTimeConverter.ConvertFromMtTime(commandResponse);
        }

        [Obsolete("OrderProfit is deprecated, please use GetOrder instead.")]
        public double OrderProfit()
        {
            return SendCommand<double>(ExecutorHandle, MtCommandType.OrderProfit);
        }

        [Obsolete("OrderStopLoss is deprecated, please use GetOrder instead.")]
        public double OrderStopLoss()
        {
            return SendCommand<double>(ExecutorHandle, MtCommandType.OrderStopLoss);
        }

        [Obsolete("OrderSymbol is deprecated, please use GetOrder instead.")]
        public string? OrderSymbol()
        {
            return SendCommand<string>(ExecutorHandle, MtCommandType.OrderSymbol);
        }

        [Obsolete("OrderTakeProfit is deprecated, please use GetOrder instead.")]
        public double OrderTakeProfit()
        {
            return SendCommand<double>(ExecutorHandle, MtCommandType.OrderTakeProfit);
        }

        [Obsolete("OrderTicket is deprecated, please use GetOrder instead.")]
        public int OrderTicket()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.OrderTicket);
        }

        [Obsolete("OrderType is deprecated, please use GetOrder instead.")]
        public TradeOperation OrderType()
        {
            return (TradeOperation) SendCommand<int>(ExecutorHandle, MtCommandType.OrderType);
        }

        [Obsolete("OrderSwap is deprecated, please use GetOrder instead.")]
        public double OrderSwap()
        {
            return SendCommand<double>(ExecutorHandle, MtCommandType.OrderSwap);
        }
        #endregion

        #region Trading functions

        public int OrderSend(string symbol, TradeOperation cmd, double volume, double price, int slippage, double stoploss, double takeprofit
            , string comment, int magic, DateTime expiration, Color arrowColor)
        {
            Log.Debug($"OrderSend: symbol = {symbol}, cmd = {cmd}, volume = {volume}, price = {price}, slippage = {slippage}, stoploss = {stoploss}, takeprofit = {takeprofit}, comment = {comment}, magic = {magic}, expiration = {expiration}, arrowColor = {arrowColor}");

            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol }, { "Cmd", cmd },
                { "Volume", volume }, { "Price", price }, { "Slippage", slippage }, { "Stoploss", stoploss},
                { "Takeprofit", takeprofit }, { "Comment", comment }, { "Magic", magic },
                { "Expiration",  MtApiTimeConverter.ConvertToMtTime(expiration) }, { "ArrowColor", MtApiColorConverter.ConvertToMtColor(arrowColor) } };

            return SendCommand<int>(ExecutorHandle, MtCommandType.OrderSend, cmdParams);
        }

        public int OrderSend(string symbol, TradeOperation cmd, double volume, double price, int slippage, double stoploss, double takeprofit
                    , string comment, int magic, DateTime expiration)
        {
            Log.Debug($"OrderSend: symbol = {symbol}, cmd = {cmd}, volume = {volume}, price = {price}, slippage = {slippage}, stoploss = {stoploss}, takeprofit = {takeprofit}, comment = {comment}, magic = {magic}, expiration = {expiration}");

            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol }, { "Cmd", cmd },
                { "Volume", volume }, { "Price", price }, { "Slippage", slippage }, { "Stoploss", stoploss},
                { "Takeprofit", takeprofit }, { "Comment", comment }, { "Magic", magic },
                { "Expiration",  MtApiTimeConverter.ConvertToMtTime(expiration) } };

            return SendCommand<int>(ExecutorHandle, MtCommandType.OrderSend, cmdParams);
        }

        public int OrderSend(string symbol, TradeOperation cmd, double volume, double price, int slippage, double stoploss, double takeprofit
                    , string comment, int magic)
        {
            Log.Debug($"OrderSend: symbol = {symbol}, cmd = {cmd}, volume = {volume}, price = {price}, slippage = {slippage}, stoploss = {stoploss}, takeprofit = {takeprofit}, comment = {comment}, magic = {magic}");

            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol }, { "Cmd", cmd },
                { "Volume", volume }, { "Price", price }, { "Slippage", slippage }, { "Stoploss", stoploss},
                { "Takeprofit", takeprofit }, { "Comment", comment }, { "Magic", magic } };

            return SendCommand<int>(ExecutorHandle, MtCommandType.OrderSend, cmdParams);
        }

        public int OrderSend(string symbol, TradeOperation cmd, double volume, double price, int slippage, double stoploss, double takeprofit
                    , string comment)
        {
            Log.Debug($"OrderSend: symbol = {symbol}, cmd = {cmd}, volume = {volume}, price = {price}, slippage = {slippage}, stoploss = {stoploss}, takeprofit = {takeprofit}, comment = {comment}");

            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol }, { "Cmd", cmd },
                { "Volume", volume }, { "Price", price }, { "Slippage", slippage }, { "Stoploss", stoploss},
                { "Takeprofit", takeprofit }, { "Comment", comment } };

            return SendCommand<int>(ExecutorHandle, MtCommandType.OrderSend, cmdParams);
        }

        public int OrderSend(string symbol, TradeOperation cmd, double volume, double price, int slippage, double stoploss, double takeprofit)
        {
            Log.Debug($"OrderSend: symbol = {symbol}, cmd = {cmd}, volume = {volume}, price = {price}, slippage = {slippage}, stoploss = {stoploss}, takeprofit = {takeprofit}");

            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol }, { "Cmd", cmd },
                { "Volume", volume }, { "Price", price }, { "Slippage", slippage }, { "Stoploss", stoploss},
                { "Takeprofit", takeprofit } };

            return SendCommand<int>(ExecutorHandle, MtCommandType.OrderSend, cmdParams);
        }

        public int OrderSend(string symbol, TradeOperation cmd, double volume, string price, int slippage, double stoploss, double takeprofit)
        {
            Log.Debug($"OrderSend: symbol = {symbol}, cmd = {cmd}, volume = {volume}, price = {price}, slippage = {slippage}, stoploss = {stoploss}, takeprofit = {takeprofit}");

            return double.TryParse(price, out double dPrice) ?
                OrderSend(symbol, cmd, volume, dPrice, slippage, stoploss, takeprofit) : 0;
        }

        public int OrderSendBuy(string symbol, double volume, int slippage)
        {
            Log.Debug($"OrderSendBuy: symbol = {symbol}, volume = {volume}, slippage = {slippage}");

            return OrderSendBuy(symbol, volume, slippage, 0, 0, null, 0);
        }

        public int OrderSendSell(string symbol, double volume, int slippage)
        {
            return OrderSendSell(symbol, volume, slippage, 0, 0, null, 0);
        }

        public int OrderSendBuy(string symbol, double volume, int slippage, double stoploss, double takeprofit)
        {
            return OrderSendBuy(symbol, volume, slippage, stoploss, takeprofit, null, 0);
        }

        public int OrderSendSell(string symbol, double volume, int slippage, double stoploss, double takeprofit)
        {
            return OrderSendSell(symbol, volume, slippage, stoploss, takeprofit, null, 0);
        }

        public int OrderSendBuy(string symbol, double volume, int slippage, double stoploss, double takeprofit, string? comment, int magic)
        {
            Log.Debug($"OrderSendBuy: symbol = {symbol}, volume = {volume}, slippage = {slippage}, stoploss = {stoploss}, takeprofit = {takeprofit}, comment = {comment}, magic = {magic}");

            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol }, { "Cmd", (int)TradeOperation.OP_BUY },
                { "Volume", volume }, { "Slippage", slippage }, { "Stoploss", stoploss},
                { "Takeprofit", takeprofit }, { "Magic", magic } };
            if (string.IsNullOrEmpty(comment) == false)
                cmdParams["Comment"] = comment;
            return SendCommand<int>(ExecutorHandle, MtCommandType.OrderSend, cmdParams);
        }

        public int OrderSendSell(string symbol, double volume, int slippage, double stoploss, double takeprofit, string? comment, int magic)
        {
            Log.Debug($"OrderSendSell: symbol = {symbol}, volume = {volume}, slippage = {slippage}, stoploss = {stoploss}, takeprofit = {takeprofit}, comment = {comment}, magic = {magic}");

            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol }, { "Cmd", (int)TradeOperation.OP_SELL },
                { "Volume", volume }, { "Slippage", slippage }, { "Stoploss", stoploss},
                { "Takeprofit", takeprofit }, { "Magic", magic } };
            if (string.IsNullOrEmpty(comment) == false)
                cmdParams["Comment"] = comment;
            return SendCommand<int>(ExecutorHandle, MtCommandType.OrderSend, cmdParams);
        }

        public bool OrderClose(int ticket, double lots, double price, int slippage, Color color)
        {
            Log.Debug($"OrderClose: ticket = {ticket}, lots = {lots}, price = {price}, slippage = {slippage}, color = {color}");

            Dictionary<string, object> cmdParams = new() { { "Ticket", ticket }, { "Lots", lots },
                { "Price", price }, { "Slippage", slippage }, { "ArrowColor", color} };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.OrderClose, cmdParams);
        }

        public bool OrderClose(int ticket, double lots, double price, int slippage)
        {
            Log.Debug($"OrderClose: ticket = {ticket}, lots = {lots}, price = {price}, slippage = {slippage}");

            Dictionary<string, object> cmdParams = new() { { "Ticket", ticket }, { "Lots", lots },
                { "Price", price }, { "Slippage", slippage } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.OrderClose, cmdParams);
        }

        public bool OrderClose(int ticket, double lots, int slippage)
        {
            Log.Debug($"OrderClose: ticket = {ticket}, lots = {lots}, slippage = {slippage}");

            Dictionary<string, object> cmdParams = new() { { "Ticket", ticket }, { "Lots", lots },
                { "Slippage", slippage } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.OrderClose, cmdParams);
        }

        public bool OrderClose(int ticket, int slippage)
        {
            Log.Debug($"OrderClose: ticket = {ticket}, slippage = {slippage}");

            Dictionary<string, object> cmdParams = new() { { "Ticket", ticket }, { "Slippage", slippage } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.OrderClose, cmdParams);
        }

        public bool OrderCloseBy(int ticket, int opposite, Color color)
        {
            Log.Debug($"OrderCloseBy: ticket = {ticket}, opposite = {opposite}, color = {color}");

            Dictionary<string, object> cmdParams = new() { { "Ticket", ticket }, { "Opposite", opposite },
                { "ColorValue",  MtApiColorConverter.ConvertToMtColor(color) } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.OrderCloseBy, cmdParams);
        }

        public bool OrderCloseBy(int ticket, int opposite)
        {
            Log.Debug($"OrderCloseBy: ticket = {ticket}, opposite = {opposite}");

            Dictionary<string, object> cmdParams = new() { { "Ticket", ticket }, { "Opposite", opposite } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.OrderCloseBy, cmdParams);
        }

        public bool OrderDelete(int ticket, Color color)
        {
            Log.Debug($"OrderDelete: ticket = {ticket}, color = {color}");

            Dictionary<string, object> cmdParams = new() { { "Ticket", ticket }, 
                { "ArrowColor", MtApiColorConverter.ConvertToMtColor(color) } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.OrderDelete, cmdParams);
        }

        public bool OrderDelete(int ticket)
        {
            Log.Debug($"OrderDelete: ticket = {ticket}");

            Dictionary<string, object> cmdParams = new() { { "Ticket", ticket } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.OrderDelete, cmdParams);
        }

        public bool OrderModify(int ticket, double price, double stoploss, double takeprofit, DateTime expiration, Color arrowColor)
        {
            Log.Debug($"OrderModify: ticket = {ticket}, price = {price}, stoploss = {stoploss}, takeprofit = {takeprofit}, expiration = {expiration}, arrowColor = {arrowColor}");

            Dictionary<string, object> cmdParams = new() { { "Ticket", ticket }, 
                { "Price", price }, { "Stoploss", stoploss}, { "Takeprofit", takeprofit },
                { "Expiration", MtApiTimeConverter.ConvertToMtTime(expiration) },
                { "ArrowColor", MtApiColorConverter.ConvertToMtColor(arrowColor) } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.OrderModify, cmdParams);
        }

        public bool OrderModify(int ticket, double price, double stoploss, double takeprofit, DateTime expiration)
        {
            Log.Debug($"OrderModify: ticket = {ticket}, price = {price}, stoploss = {stoploss}, takeprofit = {takeprofit}, expiration = {expiration}");

            Dictionary<string, object> cmdParams = new() { { "Ticket", ticket },
                { "Price", price }, { "Stoploss", stoploss}, { "Takeprofit", takeprofit },
                { "Expiration", MtApiTimeConverter.ConvertToMtTime(expiration) } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.OrderModify, cmdParams);
        }

        public void OrderPrint()
        {
            SendCommand<object>(ExecutorHandle, MtCommandType.OrderPrint);
        }

        public bool OrderSelect(int index, OrderSelectMode select, OrderSelectSource pool)
        {
            Log.Debug($"OrderSelect: index = {index}, select = {select}, pool = {pool}");

            Dictionary<string, object> cmdParams = new() { { "Index", index },
                { "Select", (int)select }, { "Pool", (int)pool} };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.OrderSelect, cmdParams);
        }

        public bool OrderSelect(int index, OrderSelectMode select)
        {
            return OrderSelect(index, select, OrderSelectSource.MODE_TRADES);
        }

        public int OrdersHistoryTotal()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.OrdersHistoryTotal);
        }

        public int OrdersTotal()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.OrdersTotal);
        }

        public bool OrderCloseAll()
        {
            return SendCommand<bool>(ExecutorHandle, MtCommandType.OrderCloseAll);
        }

        public MtOrder? GetOrder(int index, OrderSelectMode select, OrderSelectSource pool)
        {
            Dictionary<string, object> cmdParams = new() { { "Index", index },
                { "Select", (int)select }, { "Pool", (int)pool} };
            return SendCommand<MtOrder>(ExecutorHandle, MtCommandType.GetOrder, cmdParams);
        }

        public List<MtOrder>? GetOrders(OrderSelectSource pool)
        {
            Dictionary<string, object> cmdParams = new() { { "Pool", (int)pool} };
            return SendCommand<List<MtOrder>>(ExecutorHandle, MtCommandType.GetOrders, cmdParams);
        }
        #endregion

        #region Checkup

        ///<summary>
        ///Returns the contents of the system variable _LastError.
        ///After the function call, the contents of _LastError are reset.
        ///</summary>
        ///<returns>
        ///Returns the value of the last error that occurred during the execution of an mql4 program.
        ///</returns>
        public int GetLastError()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.GetLastError);
        }

        ///<summary>
        ///Checks connection between client terminal and server.
        ///</summary>
        ///<returns>
        ///It returns true if connection to the server was successfully established, otherwise, it returns false.
        ///</returns>
        public bool IsConnected()
        {
            return SendCommand<bool>(ExecutorHandle, MtCommandType.IsConnected);
        }

        ///<summary>
        ///Checks if the Expert Advisor runs on a demo account.
        ///</summary>
        ///<returns>
        ///Returns true if the Expert Advisor runs on a demo account, otherwise returns false.
        ///</returns>
        public bool IsDemo()
        {
            return SendCommand<bool>(ExecutorHandle, MtCommandType.IsDemo);
        }

        ///<summary>
        ///Checks if the DLL function call is allowed for the Expert Advisor.
        ///</summary>
        ///<returns>
        ///Returns true if the DLL function call is allowed for the Expert Advisor, otherwise returns false.
        ///</returns>
        public bool IsDllsAllowed()
        {
            return SendCommand<bool>(ExecutorHandle, MtCommandType.IsDllsAllowed);
        }

        ///<summary>
        ///Checks if Expert Advisors are enabled for running.
        ///</summary>
        ///<returns>
        ///Returns true if Expert Advisors are enabled for running, otherwise returns false.
        ///</returns>
        public bool IsExpertEnabled()
        {
            return SendCommand<bool>(ExecutorHandle, MtCommandType.IsExpertEnabled);
        }

        ///<summary>
        ///Checks if the Expert Advisor can call library function.
        ///</summary>
        ///<returns>
        ///Returns true if the Expert Advisor can call library function, otherwise returns false.
        ///</returns>
        public bool IsLibrariesAllowed()
        {
            return SendCommand<bool>(ExecutorHandle, MtCommandType.IsLibrariesAllowed);
        }

        ///<summary>
        ///Checks if Expert Advisor runs in the Strategy Tester optimization mode.
        ///</summary>
        ///<returns>
        ///Returns true if Expert Advisor runs in the Strategy Tester optimization mode, otherwise returns false.
        ///</returns>
        public bool IsOptimization()
        {
            return SendCommand<bool>(ExecutorHandle, MtCommandType.IsOptimization);
        }

        ///<summary>
        ///Checks the forced shutdown of an mql4 program.
        ///</summary>
        ///<returns>
        ///Returns true, if the _StopFlag system variable contains a value other than 0. 
        ///A nonzero value is written into _StopFlag, if a mql4 program has been commanded to complete its operation. 
        ///In this case, you must immediately terminate the program, otherwise the program will be completed 
        ///forcibly from the outside after 3 seconds.
        ///</returns>
        public bool IsStopped()
        {
            return SendCommand<bool>(ExecutorHandle, MtCommandType.IsStopped);
        }

        ///<summary>
        ///Checks if the Expert Advisor runs in the testing mode.
        ///</summary>
        ///<returns>
        ///Returns true if the Expert Advisor runs in the testing mode, otherwise returns false.
        ///</returns>
        public bool IsTesting()
        {
            return SendCommand<bool>(ExecutorHandle, MtCommandType.IsTesting);
        }

        ///<summary>
        ///Checks if the Expert Advisor is allowed to trade and trading context is not busy.
        ///</summary>
        ///<returns>
        ///Returns true if the Expert Advisor is allowed to trade and trading context is not busy, otherwise returns false.
        ///</returns>
        public bool IsTradeAllowed()
        {
            return SendCommand<bool>(ExecutorHandle, MtCommandType.IsTradeAllowed);
        }

        ///<summary>
        ///Returns the information about trade context.
        ///</summary>
        ///<returns>
        ///Returns true if a thread for trading is occupied by another Expert Advisor, otherwise returns false.
        ///</returns>
        public bool IsTradeContextBusy()
        {
            return SendCommand<bool>(ExecutorHandle, MtCommandType.IsTradeContextBusy);
        }

        ///<summary>
        ///Checks if the Expert Advisor is tested in visual mode.
        ///</summary>
        ///<returns>
        ///Returns true if the Expert Advisor is tested with checked "Visual Mode" button, otherwise returns false.
        ///</returns>
        public bool IsVisualMode()
        {
            return SendCommand<bool>(ExecutorHandle, MtCommandType.IsVisualMode);
        }

        ///<summary>
        ///Returns the code of a reason for deinitialization.
        ///</summary>
        ///<returns>
        ///Returns the value of _UninitReason which is formed before OnDeinit() is called. 
        ///Value depends on the reasons that led to deinitialization.
        ///</returns>
        public int UninitializeReason()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.UninitializeReason);
        }

        ///<summary>
        ///Print the error description.
        ///</summary>
        public string? ErrorDescription(int errorCode)
        {
            Dictionary<string, object> cmdParams = new() { { "ErrorCode", errorCode } };
            return SendCommand<string>(ExecutorHandle, MtCommandType.ErrorDescription, cmdParams);
        }

        ///<summary>
        ///Returns the value of a corresponding property of the mql4 program environment. 
        ///</summary>
        ///<param name="propertyId">Identifier of a property. Can be one of the values of the ENUM_TERMINAL_INFO_STRING enumeration.</param>
        ///<returns>
        ///Value of string type.
        ///</returns>
        public string? TerminalInfoString(ENUM_TERMINAL_INFO_STRING propertyId)
        {
            Dictionary<string, object> cmdParams = new() { { "PropertyId", (int)propertyId } };
            return SendCommand<string>(ExecutorHandle, MtCommandType.TerminalInfoString, cmdParams);
        }

        ///<summary>
        ///Returns the value of a corresponding property of the mql4 program environment.
        ///</summary>
        ///<param name="propertyId">Identifier of a property. Can be one of the values of the ENUM_TERMINAL_INFO_INTEGER enumeration.</param>
        ///<returns>
        ///Value of int type.
        ///</returns>
        public int TerminalInfoInteger(EnumTerminalInfoInteger propertyId)
        {
            Dictionary<string, object> cmdParams = new() { { "PropertyId", (int)propertyId } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.TerminalInfoInteger, cmdParams);
        }

        ///<summary>
        ///Returns the value of a corresponding property of the mql4 program environment.
        ///</summary>
        ///<param name="propertyId">Identifier of a property. Can be one of the values of the ENUM_TERMINAL_INFO_DOUBLE enumeration.</param>
        ///<returns>
        ///Value of double type.
        ///</returns>
        public double TerminalInfoDouble(EnumTerminalInfoDouble propertyId)
        {
            Dictionary<string, object> cmdParams = new() { { "PropertyId", (int)propertyId } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.TerminalInfoDouble, cmdParams);
        }

        ///<summary>
        ///Returns the name of company owning the client terminal.
        ///</summary>
        ///<returns>
        ///The name of company owning the client terminal.
        ///</returns>
        public string? TerminalCompany()
        {
            return SendCommand<string>(ExecutorHandle, MtCommandType.TerminalCompany);
        }

        ///<summary>
        ///Returns client terminal name.
        ///</summary>
        ///<returns>
        ///Client terminal name.
        ///</returns>
        public string? TerminalName()
        {
            return SendCommand<string>(ExecutorHandle, MtCommandType.TerminalName);
        }

        ///<summary>
        ///Returns the directory, from which the client terminal was launched.
        ///</summary>
        ///<returns>
        ///The directory, from which the client terminal was launched.
        ///</returns>
        public string? TerminalPath()
        {
            return SendCommand<string>(ExecutorHandle, MtCommandType.TerminalPath);
        }

        #endregion

        #region Account functions

        public double AccountBalance()
        {
            return SendCommand<double>(ExecutorHandle, MtCommandType.AccountBalance);
        }

        public double AccountCredit()
        {
            return SendCommand<double>(ExecutorHandle, MtCommandType.AccountCredit);
        }

        public string? AccountCompany()
        {
            return SendCommand<string>(ExecutorHandle, MtCommandType.AccountCompany);
        }

        public string? AccountCurrency()
        {
            return SendCommand<string>(ExecutorHandle, MtCommandType.AccountCurrency);
        }

        public double AccountEquity()
        {
            return SendCommand<double>(ExecutorHandle, MtCommandType.AccountEquity);
        }

        public double AccountFreeMargin()
        {
            return SendCommand<double>(ExecutorHandle, MtCommandType.AccountFreeMargin);
        }

        public double AccountFreeMarginCheck(string symbol, TradeOperation cmd, double volume)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Cmd", cmd }, { "Volume", volume } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.AccountFreeMarginCheck, cmdParams);
        }

        public double AccountFreeMarginMode()
        {
            return SendCommand<double>(ExecutorHandle, MtCommandType.AccountFreeMarginMode);
        }

        public int AccountLeverage()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.AccountLeverage);
        }

        public double AccountMargin()
        {
            return SendCommand<double>(ExecutorHandle, MtCommandType.AccountMargin);
        }

        public string? AccountName()
        {
            return SendCommand<string>(ExecutorHandle, MtCommandType.AccountName);
        }

        public int AccountNumber()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.AccountNumber);
        }

        public double AccountProfit()
        {
            return SendCommand<double>(ExecutorHandle, MtCommandType.AccountProfit);
        }

        public string? AccountServer()
        {
            return SendCommand<string>(ExecutorHandle, MtCommandType.AccountServer);
        }

        public int AccountStopoutLevel()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.AccountStopoutLevel);
        }

        public int AccountStopoutMode()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.AccountStopoutMode);
        }

        public bool ChangeAccount(string login, string password, string host)
        {
            Dictionary<string, object> cmdParams = new() { { "Login", login },
                { "Password", password }, { "Host", host } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ChangeAccount, cmdParams);
        }

        #endregion

        #region Common Function

        public void Alert(string msg)
        {
            Dictionary<string, object> cmdParams = new() { { "Msg", msg } };
            SendCommand<object>(ExecutorHandle, MtCommandType.Alert, cmdParams);
        }

        public void Comment(string msg)
        {
            Dictionary<string, object> cmdParams = new() { { "Msg", msg } };
            SendCommand<object>(ExecutorHandle, MtCommandType.Comment, cmdParams);
        }

        public int GetTickCount()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.GetTickCount);
        }

        public int MessageBox(string text, string caption, int flag)
        {
            Dictionary<string, object> cmdParams = new() { { "Text", text },
                { "Caption", caption }, { "Flag", flag } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.MessageBoxA, cmdParams);
        }

        public int MessageBox(string text, string caption)
        {
            return MessageBox(text, caption, EMPTY);
        }

        public int MessageBox(string text)
        {
            Dictionary<string, object> cmdParams = new() { { "Text", text } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.MessageBox, cmdParams);
        }

        public bool PlaySound(string filename)
        {
            Dictionary<string, object> cmdParams = new() { { "Filename", filename } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.PlaySound, cmdParams);
        }

        public void Print(string msg)
        {
            Dictionary<string, object> cmdParams = new() { { "Msg", msg } };
            SendCommand<object>(ExecutorHandle, MtCommandType.Print, cmdParams);
        }

        public bool SendFTP(string filename)
        {
            Dictionary<string, object> cmdParams = new() { { "Filename", filename } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.SendFTP, cmdParams);
        }

        public bool SendFTP(string filename, string ftpPath)
        {
            Dictionary<string, object> cmdParams = new() { { "Filename", filename },
                { "FtpPath", ftpPath } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.SendFTPA, cmdParams);
        }

        public bool SendMail(string subject, string someText)
        {
            Dictionary<string, object> cmdParams = new() { { "Subject", subject },
                { "SomeText", someText } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.SendMail, cmdParams);
        }

        public void Sleep(int milliseconds)
        {
            Dictionary<string, object> cmdParams = new() { {  "Milliseconds", milliseconds } };
            SendCommand<object>(ExecutorHandle, MtCommandType.Sleep, cmdParams);
        }

        #endregion

        #region Date and Time Functions

        public int Day()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.Day);
        }

        public int DayOfWeek()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.DayOfWeek);
        }

        public int DayOfYear()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.DayOfYear);
        }

        public int Hour()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.Hour);
        }

        public int Minute()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.Minute);
        }

        public int Month()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.Month);
        }

        public int Seconds()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.Seconds);
        }

        public DateTime TimeCurrent()
        {
            var commandResponse = SendCommand<int>(ExecutorHandle, MtCommandType.TimeCurrent);
            return MtApiTimeConverter.ConvertFromMtTime(commandResponse);
        }

        public DateTime TimeGMT()
        {
            var commandResponse = SendCommand<int>(ExecutorHandle, MtCommandType.TimeGMT);
            return MtApiTimeConverter.ConvertFromMtTime(commandResponse);
        }

        public int TimeDay(DateTime date)
        {
            Dictionary<string, object> cmdParams = new() { { "Date", MtApiTimeConverter.ConvertToMtTime(date) } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.TimeDay, cmdParams);
        }

        public int TimeDayOfWeek(DateTime date)
        {
            Dictionary<string, object> cmdParams = new() { { "Date", MtApiTimeConverter.ConvertToMtTime(date) } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.TimeDayOfWeek, cmdParams);
        }

        public int TimeDayOfYear(DateTime date)
        {
            Dictionary<string, object> cmdParams = new() { { "Date", MtApiTimeConverter.ConvertToMtTime(date) } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.TimeDayOfYear, cmdParams);
        }

        public int TimeHour(DateTime time)
        {
            Dictionary<string, object> cmdParams = new() { { "TIme", MtApiTimeConverter.ConvertToMtTime(time) } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.TimeHour, cmdParams);
        }

        public DateTime TimeLocal()
        {
            var commandResponse = SendCommand<int>(ExecutorHandle, MtCommandType.TimeLocal);
            return MtApiTimeConverter.ConvertFromMtTime(commandResponse);
        }

        public int TimeMinute(DateTime time)
        {;
            Dictionary<string, object> cmdParams = new() { { "TIme", MtApiTimeConverter.ConvertToMtTime(time) } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.TimeMinute, cmdParams);
        }

        public int TimeMonth(DateTime time)
        {
            Dictionary<string, object> cmdParams = new() { { "TIme", MtApiTimeConverter.ConvertToMtTime(time) } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.TimeMonth, cmdParams);
        }

        public int TimeSeconds(DateTime time)
        {
            Dictionary<string, object> cmdParams = new() { { "TIme", MtApiTimeConverter.ConvertToMtTime(time) } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.TimeSeconds, cmdParams);
        }

        public int TimeYear(DateTime time)
        {
            Dictionary<string, object> cmdParams = new() { { "TIme", MtApiTimeConverter.ConvertToMtTime(time) } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.TimeYear, cmdParams);
        }

        public int Year(DateTime time)
        {
            Dictionary<string, object> cmdParams = new() { { "TIme", MtApiTimeConverter.ConvertToMtTime(time) } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.Year, cmdParams);
        }

        #endregion

        #region Global Variables Functions
        public bool GlobalVariableCheck(string name)
        {
            Dictionary<string, object> cmdParams = new() { { "Name", name } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.GlobalVariableCheck, cmdParams);
        }

        public bool GlobalVariableDel(string name)
        {
            Dictionary<string, object> cmdParams = new() { { "Name", name } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.GlobalVariableDel, cmdParams);
        }

        public double GlobalVariableGet(string name)
        {
            Dictionary<string, object> cmdParams = new() { { "Name", name } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.GlobalVariableGet, cmdParams);
        }

        public string? GlobalVariableName(int index)
        {
            Dictionary<string, object> cmdParams = new() { { "Index", index } };
            return SendCommand<string>(ExecutorHandle, MtCommandType.GlobalVariableName, cmdParams);
        }

        public DateTime GlobalVariableSet(string name, double value)
        {
            Dictionary<string, object> cmdParams = new() { { "Name", name }, { "Value", value } };
            var commandResponse = SendCommand<int>(ExecutorHandle, MtCommandType.GlobalVariableSet, cmdParams);
            return MtApiTimeConverter.ConvertFromMtTime(commandResponse);
        }

        public bool GlobalVariableSetOnCondition(string name, double value, double checkValue)
        {
            Dictionary<string, object> cmdParams = new() { { "Name", name }, { "Value", value },
                { "CheckValue", checkValue } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.GlobalVariableSetOnCondition, cmdParams);
        }

        public int GlobalVariablesDeleteAll(string prefixName)
        {
            Dictionary<string, object> cmdParams = new() { { "PrefixName", prefixName } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.GlobalVariableSetOnCondition, cmdParams);
        }

        public int GlobalVariablesTotal()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.GlobalVariablesTotal, null);
        }

        #endregion

        #region Technical Indicators
        public double iAC(string symbol, ChartPeriod timeframe, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", (int)timeframe }, { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iAC, cmdParams);
        }

        public double iAD(string symbol, int timeframe, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe }, { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iAD, cmdParams);
        }

        public double iAlligator(string symbol, int timeframe, int jawPeriod, int jawShift, int teethPeriod, int teethShift, int lipsPeriod, int lipsShift, int maMethod, int appliedPrice, int mode, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe }, { "JawPeriod", jawPeriod },
                { "JawShift", jawShift }, { "TeethPeriod", teethPeriod },
                { "TeethShift", teethShift }, { "LipsPeriod", lipsPeriod },
                { "LipsShift", lipsShift }, { "MaMethod", maMethod },
                { "AppliedPrice", appliedPrice }, { "Mode", mode },
                { "Shift", shift } };

            return SendCommand<double>(ExecutorHandle, MtCommandType.iAlligator, cmdParams);
        }

        public double iADX(string symbol, int timeframe, int period, int appliedPrice, int mode, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe }, { "Period", period },
                { "AppliedPrice", appliedPrice }, { "Mode", mode },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iADX, cmdParams);
        }

        public double iATR(string symbol, int timeframe, int period, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe },  { "Period", period },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iATR, cmdParams);
        }

        public double iAO(string symbol, int timeframe, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe }, { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iAO, cmdParams);
        }

        public double iBearsPower(string symbol, int timeframe, int period, int appliedPrice, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe }, { "Period", period },
                { "AppliedPrice", appliedPrice },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iBearsPower, cmdParams);
        }

        public double iBands(string symbol, int timeframe, int period, int deviation, int bandsShift, int appliedPrice, int mode, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe }, { "Period", period },
                { "Deviation", deviation }, { "BandsShift", bandsShift },
                { "AppliedPrice", appliedPrice }, { "Mode", mode },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iBands, cmdParams);
        }

        public double iBandsOnArray(double[] array, int total, int period, int deviation, int bandsShift, int mode, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Total", total },
                { "Period", period }, { "Deviation", deviation },
                { "BandsShift", bandsShift }, { "Mode", mode },
                { "Data", array ?? [] },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iBandsOnArray, cmdParams);
        }

        public double iBullsPower(string symbol, int timeframe, int period, int appliedPrice, int shift)
        {
            Dictionary<string, object> cmdParams = new() { {  "Symbol", symbol },
                { "Timeframe", timeframe }, { "Period", period },
                { "AppliedPrice", appliedPrice },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iBullsPower, cmdParams);
        }

        public double iCCI(string symbol, int timeframe, int period, int appliedPrice, int shift)
        {
            Dictionary<string, object> cmdParams = new() { {  "Symbol", symbol },
                { "Timeframe", timeframe }, { "Period", period },
                { "AppliedPrice", appliedPrice },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iCCI, cmdParams);
        }

        public double iCCIOnArray(double[] array, int total, int period, int shift)
        {
            Dictionary<string, object> cmdParams = new() { {  "Total", total },
                { "Period", period },
                { "Data", array ?? [] },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iCCIOnArray, cmdParams);
        }

        public double iCustom(string symbol, int timeframe, string name, int[] parameters, int mode, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe }, { "Name", name }, { "Mode", mode },
                { "Shift", shift }, { "ParamsType", (int)ParametersType.Int },
                { "Params", new ArrayList(parameters) } };

            return SendCommand<double>(ExecutorHandle, MtCommandType.iCustom, cmdParams);
        }

        public double iCustom(string symbol, int timeframe, string name, double[] parameters, int mode, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe }, { "Name", name }, { "Mode", mode },
                { "Shift", shift }, { "ParamsType", (int)ParametersType.Double },
                { "Params", new ArrayList(parameters) } };

            return SendCommand<double>(ExecutorHandle, MtCommandType.iCustom, cmdParams);
        }

        public double iCustom(string symbol, int timeframe, string name, int mode, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe }, { "Name", name }, { "Mode", mode },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iCustom, cmdParams);
        }

        public double iDeMarker(string symbol, int timeframe, int period, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe }, { "Period", period },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iDeMarker, cmdParams);
        }

        public double iEnvelopes(string symbol, int timeframe, int maPeriod, int maMethod, int maShift, int appliedPrice, double deviation, int mode, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe }, { "MaPeriod", maPeriod },
                { "MaMethod", maMethod }, { "MaShift", maShift },
                { "AppliedPrice", appliedPrice }, { "Deviation", deviation },
                { "Mode", mode }, { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iEnvelopes, cmdParams);
        }

        public double iEnvelopesOnArray(double[] array, int total, int maPeriod, int maMethod, int maShift, double deviation, int mode, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Total", total },
                { "MaPeriod", maPeriod }, { "MaMethod", maMethod },
                { "MaShift", maShift }, { "Deviation", deviation },
                { "Mode", mode }, { "Shift", shift },
                { "Data", array ?? [] } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iEnvelopesOnArray, cmdParams);
        }

        public double iForce(string symbol, int timeframe, int period, int maMethod, int appliedPrice, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe }, { "Period", period },
                { "MaMethod", maMethod }, { "AppliedPrice", appliedPrice },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iForce, cmdParams);
        }

        public double iFractals(string symbol, int timeframe, int mode, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe }, { "Mode", mode },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iFractals, cmdParams);
        }

        public double iGator(string symbol, int timeframe, int jawPeriod, int jawShift, int teethPeriod, int teethShift, int lipsPeriod, int lipsShift, int maMethod, int appliedPrice, int mode, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe }, { "JawPeriod", jawPeriod },
                { "JawShift", jawShift }, { "TeethPeriod", teethPeriod },
                { "TeethShift", teethShift }, { "LipsPeriod", lipsPeriod },
                { "LipsShift", lipsShift }, { "MaMethod", maMethod },
                { "AppliedPrice", appliedPrice },
                { "Mode", mode }, { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iGator, cmdParams);
        }

        public double iIchimoku(string symbol, int timeframe, int tenkanSen, int kijunSen, int senkouSpanB, int mode, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe },
                { "TenkanSen", tenkanSen },
                { "KijunSen", kijunSen },
                { "SenkouSpanB", senkouSpanB },
                { "Mode", mode }, { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iIchimoku, cmdParams);
        }

        public double iBWMFI(string symbol, int timeframe, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iBWMFI, cmdParams);
        }

        public double iMomentum(string symbol, int timeframe, int period, int appliedPrice, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe }, { "Period", period },
                { "AppliedPrice", appliedPrice }, { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iMomentum, cmdParams);
        }

        public double iMomentumOnArray(double[] array, int total, int period, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Total", total },
                { "Period", period },
                { "Shift", shift }, { "Data", array ?? [] } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iMomentumOnArray, cmdParams);
        }

        public double iMFI(string symbol, int timeframe, int period, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe }, { "Period", period },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iMFI, cmdParams);
        }

        public double iMA(string symbol, int timeframe, int period, int maShift, int maMethod, int appliedPrice, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe }, { "Period", period },
                { "MaShift", maShift },
                { "MaMethod", maMethod },
                {  "AppliedPrice", appliedPrice },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iMA, cmdParams);
        }

        public double iMAOnArray(double[] array, int total, int period, int maShift, int maMethod, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Total", total },
                { "Period", period },
                { "MaShift", maShift },
                { "MaMethod", maMethod },
                { "Shift", shift },
                { "Data", array ?? [] } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iMAOnArray, cmdParams);
        }

        public double iOsMA(string symbol, int timeframe, int fastEmaPeriod, int slowEmaPeriod, int signalPeriod, int appliedPrice, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe },
                { "FastEmaPeriod", fastEmaPeriod },
                { "SlowEmaPeriod", slowEmaPeriod },
                { "SignalPeriod", signalPeriod },
                {  "AppliedPrice", appliedPrice },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iOsMA, cmdParams);
        }

        public double iMACD(string symbol, int timeframe, int fastEmaPeriod, int slowEmaPeriod, int signalPeriod, int appliedPrice, int mode, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe },
                { "FastEmaPeriod", fastEmaPeriod },
                { "SlowEmaPeriod", slowEmaPeriod },
                { "SignalPeriod", signalPeriod },
                { "AppliedPrice", appliedPrice },
                { "Mode", mode },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iMACD, cmdParams);
        }

        public double iOBV(string symbol, int timeframe, int appliedPrice, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe },
                { "AppliedPrice", appliedPrice },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iOBV, cmdParams);
        }

        public double iSAR(string symbol, int timeframe, double step, double maximum, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe },
                { "Step", step },
                { "Maximum", maximum },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iSAR, cmdParams);
        }

        public double iRSI( string symbol, int timeframe, int period, int appliedPrice, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe },
                { "Period", period },
                { "AppliedPrice", appliedPrice },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iRSI, cmdParams);
        }

        public double iRSIOnArray(double[] array, int total, int period, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Total", total },
                { "Period", period },
                { "Shift", shift },
                { "Data", array ?? [] } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iMomentumOnArray, cmdParams);
        }

        public double iRVI(string symbol, int timeframe, int period, int mode, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe },
                { "Period", period },
                { "Mode", mode },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iRVI, cmdParams);
        }

        public double iStdDev(string symbol, int timeframe, int maPeriod, int maShift, int maMethod, int appliedPrice, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe },
                { "MaPeriod", maPeriod },
                { "MaShift", maShift },
                { "MaMethod", maMethod },
                { "AppliedPrice", appliedPrice },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iStdDev, cmdParams);
        }

        public double iStdDevOnArray(double[] array, int total, int maPeriod, int maShift, int maMethod, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Total", total },
                { "MaPeriod", maPeriod },
                { "MaShift", maShift },
                { "MaMethod", maMethod },
                { "Shift", shift },
                { "Data", array ?? [] } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iStdDevOnArray, cmdParams);
        }

        public double iStochastic(string symbol, int timeframe, int pKperiod, int pDperiod, int slowing, int method, int priceField, int mode, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe },
                { "Kperiod", pKperiod },
                { "Dperiod", pDperiod },
                { "Slowing", slowing },
                { "Method", method },
                { "PriceField", priceField },
                { "Mode", mode },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iStochastic, cmdParams);
        }

        public double iWPR(string symbol, int timeframe, int period, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe },
                { "Period", period },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iWPR, cmdParams);
        }
        #endregion

        #region Timeseries access
        public int iBars(string symbol, ChartPeriod timeframe)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", (int)timeframe } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.iBars, cmdParams);
        }

        public int iBarShift(string symbol, ChartPeriod timeframe, DateTime time, bool exact)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", (int)timeframe },
                { "Time", MtApiTimeConverter.ConvertToMtTime(time) },
                { "Exact", exact } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.iBarShift, cmdParams);
        }

        public double iClose(string symbol, ChartPeriod timeframe, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", (int)timeframe },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iClose, cmdParams);
        }

        public double iHigh(string symbol, ChartPeriod timeframe, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", (int)timeframe },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iHigh, cmdParams);
        }

        public int iHighest(string symbol, ChartPeriod timeframe, SeriesIdentifier type, int count, int start)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", (int)timeframe },
                { "Type", (int)type },
                { "Count", count },
                { "StartValue", start } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.iHighest, cmdParams);
        }

        public double iLow(string symbol, ChartPeriod timeframe, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", (int)timeframe },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iLow, cmdParams);
        }

        public int iLowest(string symbol, ChartPeriod timeframe, SeriesIdentifier type, int count, int start)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", (int)timeframe },
                { "Type", type },
                { "Count", count },
                { "Start", start } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.iLowest, cmdParams);
        }

        public double iOpen(string symbol, ChartPeriod timeframe, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", (int)timeframe },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iOpen, cmdParams);
        }

        public DateTime iTime(string symbol, ChartPeriod timeframe, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", (int)timeframe },
                { "Shift", shift } };
            var commandResponse = SendCommand<int>(ExecutorHandle, MtCommandType.iTime, cmdParams);
            return MtApiTimeConverter.ConvertFromMtTime(commandResponse);
        }

        public double iVolume(string symbol, ChartPeriod timeframe, int shift)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", (int)timeframe },
                { "Shift", shift } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.iVolume, cmdParams);
        }

        public double[]? iCloseArray(string symbol, ChartPeriod timeframe)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", (int)timeframe } };
            return SendCommand<double[]>(ExecutorHandle, MtCommandType.iCloseArray, cmdParams);
        }

        public double[]? iHighArray(string symbol, ChartPeriod timeframe)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", (int)timeframe } };
            return SendCommand<double[]>(ExecutorHandle, MtCommandType.iHighArray, cmdParams);
        }

        public double[]? iLowArray(string symbol, ChartPeriod timeframe)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", (int)timeframe } };
            return SendCommand<double[]>(ExecutorHandle, MtCommandType.iLowArray, cmdParams);
        }

        public double[]? iOpenArray(string symbol, ChartPeriod timeframe)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", (int)timeframe } };
            return SendCommand<double[]>(ExecutorHandle, MtCommandType.iOpenArray, cmdParams);
        }

        public double[]? iVolumeArray(string symbol, ChartPeriod timeframe)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", (int)timeframe } };
            return SendCommand<double[]>(ExecutorHandle, MtCommandType.iVolumeArray, cmdParams);
        }

        public DateTime[]? iTimeArray(string symbol, ChartPeriod timeframe)
        {
            DateTime[]? result = null;
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", (int)timeframe } };
            var response = SendCommand<int[]>(ExecutorHandle, MtCommandType.iTimeArray, cmdParams);
            if (response != null)
            {
                result = new DateTime[response.Length];
                for(var i = 0; i < response.Length; i++)
                    result[i] = MtApiTimeConverter.ConvertFromMtTime(response[i]);
            }
            return result;
        }

        public bool RefreshRates()
        {
            return SendCommand<bool>(ExecutorHandle, MtCommandType.RefreshRates, null);
        }

        public List<MqlRates>? CopyRates(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count)
        {
            Dictionary<string, object> cmdParams = new() { { "SymbolName", symbolName },
                { "Timeframe", (int)timeframe }, { "StartPos", startPos }, { "Count", count },
                { "CopyRatesType", 1 } };
            return SendCommand<List<MqlRates>>(ExecutorHandle, MtCommandType.CopyRates, cmdParams);
        }

        public List<MqlRates>? CopyRates(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, int count)
        {
            Dictionary<string, object> cmdParams = new() { { "SymbolName", symbolName },
                { "Timeframe", (int)timeframe }, { "StartTime", MtApiTimeConverter.ConvertToMtTime(startTime) },{ "Count", count },
                { "CopyRatesType", 2 } };
            return SendCommand<List<MqlRates>>(ExecutorHandle, MtCommandType.CopyRates, cmdParams);
        }

        public List<MqlRates>? CopyRates(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, DateTime stopTime)
        {
            Dictionary<string, object> cmdParams = new() { { "SymbolName", symbolName },
                { "Timeframe", (int)timeframe }, { "StartTime", MtApiTimeConverter.ConvertToMtTime(startTime) },
                { "StopTime", MtApiTimeConverter.ConvertToMtTime(stopTime) },
                { "CopyRatesType", 3 } };
            return SendCommand<List<MqlRates>>(ExecutorHandle, MtCommandType.CopyRates, cmdParams);
        }

        ///<summary>
        ///Returns information about the state of historical data.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="propId">Identifier of the requested property, value of the ENUM_SERIES_INFO_INTEGER enumeration.</param>
        ///<returns>
        ///Returns value of the long type.
        ///</returns>
        public long SeriesInfoInteger(string symbolName, ENUM_TIMEFRAMES timeframe, EnumSeriesInfoInteger propId)
        {
            Dictionary<string, object> cmdParams = new() { { "SymbolName", symbolName },
                { "Timeframe", (int)timeframe }, { "PropId", (int)propId } };
            return SendCommand<long>(ExecutorHandle, MtCommandType.SeriesInfoInteger, cmdParams);
        }

        #endregion

        #region Market Info

        ///<summary>
        ///Returns various data about securities listed in the "Market Watch" window.
        ///</summary>
        ///<param name="symbol">Symbol name.</param>
        ///<param name="type">Request identifier that defines the type of information to be returned. Can be any of values of request identifiers.</param>
        ///<returns>
        ///Returns various data about securities listed in the "Market Watch" window.
        ///</returns>
        public double MarketInfo(string symbol, MarketInfoModeType type)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Type", (int)type } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.MarketInfo, cmdParams);
        }

        ///<summary>
        ///Returns the number of available (selected in Market Watch or all) symbols.
        ///</summary>
        ///<param name="selected">Request mode. Can be true or false.</param>
        ///<returns>
        ///If the 'selected' parameter is true, the function returns the number of symbols selected in MarketWatch. If the value is false, it returns the total number of all symbols.
        ///</returns>
        public int SymbolsTotal(bool selected)
        {
            Dictionary<string, object> cmdParams = new() { { "Selected", selected } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.SymbolsTotal, cmdParams);
        }

        ///<summary>
        ///Returns the name of a symbol.
        ///</summary>
        ///<param name="pos">Order number of a symbol.</param>
        ///<param name="selected">Request mode. If the value is true, the symbol is taken from the list of symbols selected in MarketWatch. If the value is false, the symbol is taken from the general list.</param>
        ///<returns>
        ///Value of string type with the symbol name.
        ///</returns>
        public string? SymbolName(int pos, bool selected)
        {
            Dictionary<string, object> cmdParams = new() { { "Pos", pos },
                { "Selected", selected } };
            return SendCommand<string>(ExecutorHandle, MtCommandType.SymbolName, cmdParams);
        }

        ///<summary>
        ///Selects a symbol in the Market Watch window or removes a symbol from the window.
        ///</summary>
        ///<param name="name">Symbol name</param>
        ///<param name="select">Switch. If the value is false, a symbol should be removed from MarketWatch, otherwise a symbol should be selected in this window. A symbol can't be removed if the symbol chart is open, or there are open orders for this symbol.</param>
        ///<returns>
        ///In case of failure returns false.
        ///</returns>
        public bool SymbolSelect(string name, bool select)
        {
            Dictionary<string, object> cmdParams = new() { { "Name", name },
                { "Select", select } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.SymbolSelect, cmdParams);
        }

        ///<summary>
        ///Returns the corresponding property of a specified symbol.
        ///</summary>
        ///<param name="name">Symbol name</param>
        ///<param name="propId">Identifier of a symbol property. The value can be one of the values of the EnumSymbolInfoInteger enumeration</param>
        ///<returns>
        ///The value of long type.
        ///</returns>
        public long SymbolInfoInteger(string name, EnumSymbolInfoInteger propId)
        {
            Dictionary<string, object> cmdParams = new() { { "Name", name },
                { "PropId", (int)propId } };
            return SendCommand<long>(ExecutorHandle, MtCommandType.SymbolInfoInteger, cmdParams);
        }

        ///<summary>
        ///Returns the corresponding property of a specified symbol.
        ///</summary>
        ///<param name="name">Symbol name</param>
        ///<param name="propId">Identifier of a symbol property. The value can be one of the values of the ENUM_SYMBOL_INFO_STRING enumeration.</param>
        ///<returns>
        ///The value of string type.
        ///</returns>
        public string? SymbolInfoString(string name, ENUM_SYMBOL_INFO_STRING propId)
        {
            Dictionary<string, object> cmdParams = new() { { "Name", name },
                { "PropId", (int)propId } };
            return SendCommand<string>(ExecutorHandle, MtCommandType.SymbolInfoString, cmdParams);
        }

        ///<summary>
        ///Allows receiving time of beginning and end of the specified quoting/trading  sessions for a specified symbol and day of week.
        /// 
        ///</summary>
        ///<param name="symbol">Symbol name.</param>
        ///<param name="dayOfWeek">Day of the week.</param>
        ///<param name="index">Ordinal number of a session, whose beginning and end time we want to receive. Indexing of sessions starts with 0.</param>
        ///<param name="type">Session type: Quote, Trade</param>
        ///<returns>
        ///The value session.
        ///</returns>
        public MtSession? SymbolInfoSession(string symbol, DayOfWeek dayOfWeek, uint index, SessionType type)
        {
            Dictionary<string, object> cmdParams = new() { { "SymbolName", symbol },
                { "DayOfWeek", (int)dayOfWeek }, { "SessionIndex", (int)index }, { "SessionType", type } };
            return SendCommand<MtSession>(ExecutorHandle, MtCommandType.Session, cmdParams);
        }

        ///<summary>
        ///Returns the corresponding property of a specified symbol.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="propId">Identifier of a symbol property. The value can be one of the values of the ENUM_SYMBOL_INFO_DOUBLE enumeration.</param>
        ///<returns>
        /// The value of double type.
        ///</returns>
        public double SymbolInfoDouble(string symbolName, EnumSymbolInfoDouble propId)
        {
            Dictionary<string, object> cmdParams = new() { { "SymbolName", symbolName }, { "PropId", (int)propId } };

            return SendCommand<double>(ExecutorHandle, MtCommandType.SymbolInfoDouble, cmdParams);
        }

        ///<summary>
        ///Returns the corresponding property of a specified symbol.
        ///</summary>
        ///<param name="symbol">Symbol name.</param>
        ///<returns>
        /// MqlTick object, to which the current prices and time of the last price update will be placed.
        ///</returns>
        public MqlTick? SymbolInfoTick(string symbol)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol } };
            return SendCommand<MqlTick>(ExecutorHandle, MtCommandType.SymbolInfoTick, cmdParams);
        }
        #endregion

        #region Chart Operations

        ///<summary>
        ///Returns the ID of the current chart.
        ///</summary>
        ///<returns>
        /// Value of long type.
        ///</returns>
        public long ChartId()
        {
            return SendCommand<long>(ExecutorHandle, MtCommandType.ChartId);
        }

        ///<summary>
        ///This function calls a forced redrawing of a specified chart.
        ///</summary>
        public void ChartRedraw(long chartId = 0)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId } };
            SendCommand<object>(ExecutorHandle, MtCommandType.ChartRedraw, cmdParams);
        }

        ///<summary>
        ///Applies a specific template from a specified file to the chart.
        ///</summary>
        ///<param name="chartId">Chart ID.</param>
        ///<param name="filename">The name of the file containing the template.</param>
        ///<returns>
        ///Returns true if the command has been added to chart queue, otherwise false.
        ///</returns>
        public bool ChartApplyTemplate(long chartId, string filename)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "Filename", filename } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ChartApplyTemplate, cmdParams);
        }

        ///<summary>
        ///Saves current chart settings in a template with a specified name.
        ///</summary>
        ///<param name="chartId">Chart ID.</param>
        ///<param name="filename">The filename to save the template. The ".tpl" extension will be added to the filename automatically; there is no need to specify it. The template is saved in data_folder\templates\ and can be used for manual application in the terminal. If a template with the same filename already exists, the contents of this file will be overwritten.</param>
        ///<returns>
        ///Returns true if the command has been added to chart queue, otherwise false.
        ///</returns>
        public bool ChartSaveTemplate(long chartId, string filename)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "Filename", filename } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ChartSaveTemplate, cmdParams);
        }

        ///<summary>
        ///The function returns the number of a subwindow where an indicator is drawn.
        ///</summary>
        ///<param name="chartId">Chart ID.</param>
        ///<param name="indicatorShortname">Short name of the indicator.</param>
        ///<returns>
        ///Subwindow number in case of success. In case of failure the function returns -1.
        ///</returns>
        public int ChartWindowFind(long chartId, string indicatorShortname)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "IndicatorShortname", indicatorShortname } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.ChartWindowFind, cmdParams);
        }

        ///<summary>
        ///The function returns the number of a subwindow where an indicator is drawn.
        ///</summary>
        ///<param name="chartId">Chart ID.</param>
        ///<param name="subWindow">The number of the chart subwindow. 0 means the main chart window.</param>
        ///<param name="time">The time value on the chart, for which the value in pixels along the X axis will be received.</param>
        ///<param name="price">The price value on the chart, for which the value in pixels along the Y axis will be received.</param>
        ///<param name="x">The variable, into which the conversion of time to X will be received. The origin is in the upper left corner of the main chart window.</param>
        ///<param name="y">The variable, into which the conversion of price to Y will be received. The origin is in the upper left corner of the main chart window.</param>
        ///<returns>
        ///Subwindow number in case of success. In case of failure the function returns -1.
        ///</returns>
        public bool ChartTimePriceToXY(long chartId, int subWindow, DateTime? time, double price, out int x, out int y)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "SubWindow", subWindow },
                { "Time", MtApiTimeConverter.ConvertToMtTime(time) },
                { "Price", price } };
            var response = SendCommand<FuncResult<Dictionary<string, int>>>(ExecutorHandle, MtCommandType.ChartTimePriceToXY, cmdParams);
            if (response != null && response.Result != null
                && response.Result.TryGetValue("X", out x)
                && response.Result.TryGetValue("Y", out y))
                return response.RetVal;
            x = 0; y = 0;
            return false;
        }

        ///<summary>
        ///The function returns the number of a subwindow where an indicator is drawn.
        ///</summary>
        ///<param name="chartId">Chart ID.</param>
        ///<param name="x">The variable, into which the conversion of time to X will be received. The origin is in the upper left corner of the main chart window.</param>
        ///<param name="y">The variable, into which the conversion of price to Y will be received. The origin is in the upper left corner of the main chart window.</param>
        ///<param name="subWindow">The number of the chart subwindow. 0 means the main chart window.</param>
        ///<param name="time">The time value on the chart, for which the value in pixels along the X axis will be received.</param>
        ///<param name="price">The price value on the chart, for which the value in pixels along the Y axis will be received.</param>
        ///<returns>
        ///Subwindow number in case of success. In case of failure the function returns -1.
        ///</returns>
        public bool ChartXYToTimePrice(long chartId, int x, int y, out int subWindow, out DateTime? time, out double price)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "X", x },
                { "Y", y } };
            var response = SendCommand<FuncResult<Dictionary<string, object>>>(ExecutorHandle, MtCommandType.ChartXYToTimePrice, cmdParams);
            if (response != null && response.Result != null
                && response.Result.TryGetValue("SubWindow", out object? mtSubWindow)
                && response.Result.TryGetValue("Time", out object? mtTime)
                && response.Result.TryGetValue("Price", out object? mtPrice))
            {
                subWindow = Convert.ToInt32(mtSubWindow);
                time = MtApiTimeConverter.ConvertFromMtTime(Convert.ToInt32(mtTime));
                price = Convert.ToDouble(mtPrice);
                return response.RetVal;
            }
            subWindow = 0;
            time = null;
            price = double.NaN;
            return false;
        }

        ///<summary>
        ///Opens a new chart with the specified symbol and period.
        ///</summary>
        ///<param name="symbol">Chart symbol. NULL means the symbol of the  current chart (the Expert Advisor is attached to).</param>
        ///<param name="period"> Chart period (timeframe). Can be one of the ENUM_TIMEFRAMES values. 0 means the current chart period.</param>
        ///<returns>
        ///If successful, it returns the opened chart ID. Otherwise returns 0.
        ///</returns>
        public long ChartOpen(string symbol, ENUM_TIMEFRAMES period)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Period", period } };
            return SendCommand<long>(ExecutorHandle, MtCommandType.ChartOpen, cmdParams);
        }

        ///<summary>
        ///Returns the ID of the first chart of the client terminal.
        ///</summary>
        public long ChartFirst()
        {
            return SendCommand<long>(ExecutorHandle, MtCommandType.ChartFirst);
        }

        ///<summary>
        ///Returns the chart ID of the chart next to the specified one.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 does not mean the current chart. 0 means "return the first chart ID".</param>
        ///<returns>
        ///Chart ID. If this is the end of the chart list, it returns -1.
        ///</returns>
        public long ChartNext(long chartId)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId } };
            return SendCommand<long>(ExecutorHandle, MtCommandType.ChartNext, cmdParams);
        }

        ///<summary>
        ///Closes the specified chart.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<returns>
        ///If successful, returns true, otherwise false.
        ///</returns>
        public bool ChartClose(long chartId)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ChartClose, cmdParams);
        }

        ///<summary>
        ///Returns the symbol name for the specified chart.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<returns>
        ///If chart does not exist, the result will be an empty string.
        ///</returns>
        public string? ChartSymbol(long chartId)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId } };
            return SendCommand<string>(ExecutorHandle, MtCommandType.ChartSymbol, cmdParams);
        }

        ///<summary>
        ///Returns the timeframe period of specified chart.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<returns>
        ///The function returns one of the ENUM_TIMEFRAMES values. If chart does not exist, it returns 0.
        ///</returns>
        public ENUM_TIMEFRAMES ChartPeriod(long chartId)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId } };
            return (ENUM_TIMEFRAMES) SendCommand<int>(ExecutorHandle, MtCommandType.ChartPeriod, cmdParams);
        }

        ///<summary>
        ///Sets a value for a corresponding property of the specified chart. Chart property should be of a double type.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<param name="propId">Chart property ID. Can be one of the ENUM_CHART_PROPERTY_DOUBLE values (except the read-only properties).</param>
        ///<param name="value">Property value.</param>
        ///<returns>
        ///Returns true if the command has been added to chart queue, otherwise false.
        ///</returns>
        public bool ChartSetDouble(long chartId, EnumChartPropertyDouble propId, double value)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "PropId", (int)propId },
                { "Value", value } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ChartSetDouble, cmdParams);
        }

        ///<summary>
        ///Sets a value for a corresponding property of the specified chart. Chart property must be datetime, int, color, bool or char.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<param name="propId">Chart property ID. It can be one of the ENUM_CHART_PROPERTY_INTEGER value (except the read-only properties).</param>
        ///<param name="value">Property value.</param>
        ///<returns>
        ///Returns true if the command has been added to chart queue, otherwise false.
        ///</returns>
        public bool ChartSetInteger(long chartId, EnumChartPropertyInteger propId, long value)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "PropId", (int)propId },
                { "Value", value } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ChartSetInteger, cmdParams);
        }

        ///<summary>
        ///Sets a value for a corresponding property of the specified chart. Chart property must be of the string type.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<param name="propId">Chart property ID. Its value can be one of the ENUM_CHART_PROPERTY_STRING values (except the read-only properties).</param>
        ///<param name="value">Property value string. String length cannot exceed 2045 characters (extra characters will be truncated).</param>
        ///<returns>
        ///Returns true if the command has been added to chart queue, otherwise false.
        ///</returns>
        public bool ChartSetString(long chartId, EnumChartPropertyString propId, string value)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "PropId", (int)propId },
                { "Value", value } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ChartSetString, cmdParams);
        }

        ///<summary>
        ///Sets a value for a corresponding property of the specified chart. Chart property must be of the string type.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<param name="propId">Chart property ID. This value can be one of the ENUM_CHART_PROPERTY_DOUBLE values.</param>
        ///<param name="subWindow">Number of the chart subwindow. For the first case, the default value is 0 (main chart window). The most of the properties do not require a subwindow number.</param>
        ///<returns>
        ///The value of double type.
        ///</returns>
        public double ChartGetDouble(long chartId, EnumChartPropertyDouble propId, int subWindow = 0)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "PropId", (int)propId },
                { "SubWindow", subWindow } };
            return SendCommand<double>(ExecutorHandle, MtCommandType.ChartGetDouble, cmdParams);
        }

        ///<summary>
        ///Returns the value of a corresponding property of the specified chart. Chart property must be of datetime, int or bool type.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<param name="propId">Chart property ID. This value can be one of the ENUM_CHART_PROPERTY_INTEGER values.</param>
        ///<param name="subWindow">Number of the chart subwindow. For the first case, the default value is 0 (main chart window). The most of the properties do not require a subwindow number.</param>
        ///<returns>
        ///The value of long type.
        ///</returns>
        public long ChartGetInteger(long chartId, EnumChartPropertyInteger propId, int subWindow = 0)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "PropId", (int)propId },
                { "SubWindow", subWindow } };
            return SendCommand<long>(ExecutorHandle, MtCommandType.ChartGetInteger, cmdParams);
        }

        ///<summary>
        ///Returns the value of a corresponding property of the specified chart. Chart property must be of string type.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<param name="propId">Chart property ID. This value can be one of the ENUM_CHART_PROPERTY_STRING values.</param>
        ///<returns>
        ///The value of string type.
        ///</returns>
        public string? ChartGetString(long chartId, EnumChartPropertyString propId)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "PropId", (int)propId } };
            return SendCommand<string>(ExecutorHandle, MtCommandType.ChartGetString, cmdParams);
        }

        ///<summary>
        ///Performs shift of the specified chart by the specified number of bars relative to the specified position in the chart.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<param name="position">Chart position to perform a shift. Can be one of the ENUM_CHART_POSITION values.</param>
        ///<param name="shift">Number of bars to shift the chart. Positive value means the right shift (to the end of chart), negative value means the left shift (to the beginning of chart). The zero shift can be used to navigate to the beginning or end of chart.</param>
        ///<returns>
        ///Returns true if successful, otherwise returns false.
        ///</returns>
        public bool ChartNavigate(long chartId, int position, int shift = 0)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "Position",  position },
                { "Shift", shift } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ChartNavigate, cmdParams);
        }

        ///<summary>
        ///Performs shift of the specified chart by the specified number of bars relative to the specified position in the chart.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<param name="subWindow">Number of the chart subwindow. 0 denotes the main chart subwindow.</param>
        ///<param name="indicatorShortname">The short name of the indicator which is set in the INDICATOR_SHORTNAME property with the IndicatorSetString() function. To get the short name of an indicator use the ChartIndicatorName() function.</param>
        ///<returns>
        ///Returns true if the command has been added to chart queue, otherwise false.
        ///</returns>
        public bool ChartIndicatorDelete(long chartId, int subWindow, string indicatorShortname)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "SubWindow", subWindow },
                { "IndicatorShortname", indicatorShortname } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ChartIndicatorDelete, cmdParams);
        }

        ///<summary>
        ///Returns the short name of the indicator by the number in the indicators list on the specified chart window.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<param name="subWindow">Number of the chart subwindow. 0 denotes the main chart subwindow.</param>
        ///<param name="index">the index of the indicator in the list of indicators. The numeration of indicators start with zero, i.e. the first indicator in the list has the 0 index. To obtain the number of indicators in the list use the ChartIndicatorsTotal() function.</param>
        ///<returns>
        ///The short name of the indicator which is set in the INDICATOR_SHORTNAME property with the IndicatorSetString() function.
        ///</returns>
        public string? ChartIndicatorName(long chartId, int subWindow, int index)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "SubWindow", subWindow },
                { "Index", index } };
            return SendCommand<string>(ExecutorHandle, MtCommandType.ChartIndicatorName, cmdParams);
        }

        ///<summary>
        ///Returns the number of all indicators applied to the specified chart window.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<param name="subWindow">Number of the chart subwindow. 0 denotes the main chart subwindow.</param>
        ///<returns>
        ///The number of indicators in the specified chart window.
        ///</returns>
        public int ChartIndicatorsTotal(long chartId, int subWindow)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "SubWindow", subWindow } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.ChartIndicatorsTotal, cmdParams);
        }

        ///<summary>
        ///Returns the number (index) of the chart subwindow the Expert Advisor or script has been dropped to. 0 means the main chart window.
        ///</summary>
        public int ChartWindowOnDropped()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.ChartWindowOnDropped);
        }

        ///<summary>
        ///Returns the price coordinate corresponding to the chart point the Expert Advisor or script has been dropped to.
        ///</summary>
        public double ChartPriceOnDropped()
        {
            return SendCommand<double>(ExecutorHandle, MtCommandType.ChartPriceOnDropped);
        }

        ///<summary>
        ///Returns the time coordinate corresponding to the chart point the Expert Advisor or script has been dropped to.
        ///</summary>
        public DateTime ChartTimeOnDropped()
        {
            var res = SendCommand<int>(ExecutorHandle, MtCommandType.ChartTimeOnDropped);
            return MtApiTimeConverter.ConvertFromMtTime(res);
        }

        ///<summary>
        ///Returns the X coordinate of the chart point the Expert Advisor or script has been dropped to.
        ///</summary>
        public int ChartXOnDropped()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.ChartXOnDropped);
        }

        ///<summary>
        ///Returns the Y coordinateof the chart point the Expert Advisor or script has been dropped to.
        ///</summary>
        public int ChartYOnDropped()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.ChartYOnDropped);
        }

        ///<summary>
        ///Changes the symbol and period of the specified chart. The function is asynchronous, i.e. it sends the command and does not wait for its execution completion.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<param name="symbol">Chart symbol. NULL value means the current chart symbol (Expert Advisor is attached to)</param>
        ///<param name="period">Chart period (timeframe). Can be one of the ENUM_TIMEFRAMES values. 0 means the current chart period.</param>
        ///<returns>
        ///Returns true if the command has been added to chart queue, otherwise false.
        ///</returns>
        public bool ChartSetSymbolPeriod(long chartId, string symbol, ENUM_TIMEFRAMES period)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "Symbol", symbol },
                { "Period", period } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ChartSetSymbolPeriod, cmdParams);
        }

        ///<summary>
        ///Saves current chart screen shot as a GIF, PNG or BMP file depending on specified extension.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<param name="filename">Screenshot file name. Cannot exceed 63 characters. Screenshot files are placed in the \Files directory.</param>
        ///<param name="width">Screenshot width in pixels.</param>
        ///<param name="height">Screenshot height in pixels.</param>
        ///<param name="alignMode">Output mode of a narrow screenshot.</param>
        ///<returns>
        ///Returns true if the command has been added to chart queue, otherwise false.
        ///</returns>
        public bool ChartScreenShot(long chartId, string filename, int width, int height, EnumAlignMode alignMode = EnumAlignMode.ALIGN_RIGHT)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "Filename", filename },
                { "Width", width },
                { "Height", height },
                { "AlignMode", (int)alignMode } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ChartScreenShot, cmdParams);
        }

        ///<summary>
        ///Returns the amount of bars visible on the chart.
        ///</summary>
        ///<returns>
        ///The amount of bars visible on the chart.
        ///</returns>
        public int WindowBarsPerChart()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.WindowBarsPerChart);
        }

        ///<summary>
        ///Returns the name of the executed Expert Advisor, script, custom indicator, or library.
        ///</summary>
        ///<returns>
        ///The name of the executed Expert Advisor, script, custom indicator, or library, depending on the MQL4 program, from which this function has been called.
        ///</returns>
        public string? WindowExpertName()
        {
            return SendCommand<string>(ExecutorHandle, MtCommandType.WindowExpertName);
        }

        ///<summary>
        ///Returns the window index containing this specified indicator.
        ///</summary>
        ///<param name="name">Indicator short name.</param>
        ///<returns>
        ///If indicator with name was found, the function returns the window index containing this specified indicator, otherwise it returns -1.
        ///</returns>
        public int WindowFind(string name)
        {
            Dictionary<string, object> cmdParams = new() { { "Name", name } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.WindowFind, cmdParams);
        }

        ///<summary>
        ///Returns index of the first visible bar in the current chart window.
        ///</summary>
        ///<returns>
        ///Index of the first visible bar number in the current chart window.
        ///</returns>
        public int WindowFirstVisibleBar()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.WindowFirstVisibleBar);
        }

        ///<summary>
        ///Returns the system handle of the chart window.
        ///</summary>
        ///<param name="symbol">Symbol.</param>
        ///<param name="timeframe">Timeframe. It can be any of Timeframe enumeration values. 0 means the current chart timeframe.</param>
        ///<returns>
        ///Returns the system handle of the chart window. If the chart of symbol and timeframe has not been opened by the moment of function calling, 0 will be returned.
        ///</returns>
        public int WindowHandle(string symbol, int timeframe)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol },
                { "Timeframe", timeframe } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.WindowHandle, cmdParams);
        }

        ///<summary>
        ///Returns the visibility flag of the chart subwindow.
        ///</summary>
        ///<param name="index">Subwindow index.</param>
        ///<returns>
        ///Returns true if the chart subwindow is visible, otherwise returns false. The chart subwindow can be hidden due to the visibility properties of the indicator placed in it.
        ///</returns>
        public bool WindowIsVisible(int index)
        {
            Dictionary<string, object> cmdParams = new() { { "Index", index } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.WindowIsVisible, cmdParams);
        }

        ///<summary>
        ///Returns the window index where Expert Advisor, custom indicator or script was dropped.
        ///</summary>
        ///<returns>
        ///The window index where Expert Advisor, custom indicator or script was dropped. This value is valid if the Expert Advisor, custom indicator or script was dropped by mouse.
        ///</returns>
        public int WindowOnDropped()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.WindowOnDropped);
        }

        ///<summary>
        ///Returns the maximal value of the vertical scale of the specified subwindow of the current chart.
        ///</summary>
        ///<param name="index">Chart subwindow index (0 - main chart window).</param>
        ///<returns>
        ///The maximal value of the vertical scale of the specified subwindow of the current chart.
        ///</returns>
        public int WindowPriceMax(int index = 0)
        {
            Dictionary<string, object> cmdParams = new() { { "Index", index } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.WindowPriceMax, cmdParams);
        }

        ///<summary>
        ///Returns the minimal value of the vertical scale of the specified subwindow of the current chart.
        ///</summary>
        ///<param name="index">Chart subwindow index (0 - main chart window).</param>
        ///<returns>
        ///The minimal value of the vertical scale of the specified subwindow of the current chart.
        ///</returns>
        public int WindowPriceMin(int index = 0)
        {
            Dictionary<string, object> cmdParams = new() { { "Index", index } };
            return SendCommand<int>(ExecutorHandle, MtCommandType.WindowPriceMin, cmdParams);
        }

        ///<summary>
        ///Returns the price of the chart point where Expert Advisor or script was dropped.
        ///</summary>
        ///<returns>
        ///The price of the chart point where Expert Advisor or script was dropped. This value is only valid if the expert or script was dropped by mouse.
        ///</returns>
        public double WindowPriceOnDropped()
        {
            return SendCommand<double>(ExecutorHandle, MtCommandType.WindowPriceOnDropped);
        }

        ///<summary>
        ///Redraws the current chart forcedly.
        ///</summary>
        ///<returns>
        ///Redraws the current chart forcedly. It is normally used after the objects properties have been changed.
        ///</returns>
        public void WindowRedraw()
        {
            SendCommand<object>(ExecutorHandle, MtCommandType.WindowRedraw);
        }

        ///<summary>
        ///Saves current chart screen shot as a GIF file.
        ///</summary>
        ///<param name="filename">Screen shot file name. Screenshot is saved to \Files folder.</param>
        ///<param name="sizeX">Screen shot width in pixels.</param>
        ///<param name="sizeY">Screen shot height in pixels.</param>
        ///<param name="startBar">Index of the first visible bar in the screen shot. If 0 value is set, the current first visible bar will be shot. If no value or negative value has been set, the end-of-chart screen shot will be produced, indent being taken into consideration.</param>
        ///<param name="chartScale">Horizontal chart scale for screen shot. Can be in the range from 0 to 5. If no value or negative value has been set, the current chart scale will be used.</param>
        ///<param name="chartMode"> Chart displaying mode. It can take the following values: CHART_BAR (0 is a sequence of bars), CHART_CANDLE (1 is a sequence of candlesticks), CHART_LINE (2 is a close prices line). If no value or negative value has been set, the chart will be shown in its current mode.</param>
        ///<returns>
        ///Returns true if succeed, otherwise false.
        ///</returns>
        public bool WindowScreenShot(string filename, int sizeX, int sizeY, int startBar = -1, int chartScale = -1, int chartMode = -1)
        {
            Dictionary<string, object> cmdParams = new() { { "Filename", filename },
                { "SizeX", sizeX },
                { "SizeY", sizeY },
                { "StartBar", startBar },
                { "ChartScale", chartScale },
                { "ChartMode", chartMode } };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.WindowScreenShot, cmdParams);
        }

        ///<summary>
        ///Returns the time of the chart point where Expert Advisor or script was dropped.
        ///</summary>
        ///<returns>
        ///The time value of the chart point where expert or script was dropped. This value is only valid if the expert or script was dropped by mouse.
        ///</returns>
        public DateTime WindowTimeOnDropped()
        {
            var res = SendCommand<int>(ExecutorHandle, MtCommandType.WindowTimeOnDropped);
            return MtApiTimeConverter.ConvertFromMtTime(res);
        }

        ///<summary>
        ///Returns total number of indicator windows on the chart.
        ///</summary>
        ///<returns>
        ///Total number of indicator windows on the chart (including main chart).
        ///</returns>
        public int WindowsTotal()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.WindowsTotal);
        }

        ///<summary>
        ///Returns the value at X axis in pixels for the chart window client area point at which the Expert Advisor or script was dropped.
        ///</summary>
        ///<returns>
        ///The value at X axis in pixels for the chart window client area point at which the expert or script was dropped. The value will be true only if the expert or script were moved with the mouse ("Drag'n'Drop") technique.
        ///</returns>
        public int WindowXOnDropped()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.WindowXOnDropped);
        }

        ///<summary>
        ///Returns the value at Y axis in pixels for the chart window client area point at which the Expert Advisor or script was dropped.
        ///</summary>
        ///<returns>
        ///Returns the value at Y axis in pixels for the chart window client area point at which the Expert Advisor or script was dropped. The value will be true only if the expert or script were moved with the mouse ("Drag'n'Drop") technique.
        ///</returns>
        public int WindowYOnDropped()
        {
            return SendCommand<int>(ExecutorHandle, MtCommandType.WindowYOnDropped);
        }
        #endregion

        #region Object Functions

        ///<summary>
        ///The function creates an object with the specified name, type, and the initial coordinates in the specified chart subwindow of the specified chart.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="objectName">Name of the object. The name must be unique within a chart, including its subwindows.</param>
        ///<param name="objectType">Object type.</param>
        ///<param name="subWindow">Number of the chart subwindow. 0 means the main chart window.</param>
        ///<param name="time1">The time coordinate of the first anchor point.</param>
        ///<param name="price1">The price coordinate of the first anchor point.</param>
        ///<param name="time2">The time coordinate of the second anchor point.</param>
        ///<param name="price2">The price coordinate of the second anchor point.</param>
        ///<param name="time3">The time coordinate of the third anchor point.</param>
        ///<param name="price3">The price coordinate of the third anchor point.</param>
        ///<returns>
        ///Returns true or false depending on whether the object is created or not. 
        ///</returns>
        public bool ObjectCreate(long chartId, string objectName, EnumObject objectType, int subWindow, 
            DateTime? time1, double price1, DateTime? time2 = null, double? price2 = null, DateTime? time3 = null, double? price3 = null)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ChartId", chartId },
                { "ObjectName", objectName },
                { "ObjectType", (int)objectType },
                { "SubWindow", subWindow },
                { "Time1", MtApiTimeConverter.ConvertToMtTime(time1) },
                { "Price1", price1 },
                { "Time2", time2 != null ? MtApiTimeConverter.ConvertToMtTime(time2.Value) : 0 },
                { "Price2", price2 != null ? price2.Value : 0.0 },
                { "Time3", time3 != null ? MtApiTimeConverter.ConvertToMtTime(time3.Value) : 0 },
                { "Price3", price3 != null ? price3.Value : 0.0 }
            };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ObjectCreate, cmdParams);
        }

        ///<summary>
        ///The function returns the name of the corresponding object by its index in the objects list.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="objectIndex">Object index. This value must be greater or equal to 0 and less than ObjectsTotal().</param>
        ///<param name="subWindow">Number of the chart window. Must be greater or equal to -1 (-1 mean all subwindows, 0 means the main chart window) and less than WindowsTotal().</param>
        ///<param name="objectType">Type of the object. The value can be one of the values of the EnumObject enumeration. EMPTY (-1) means all types.</param>
        ///<returns>
        ///Name of the object is returned in case of success.
        ///</returns>
        public string? ObjectName(long chartId, int objectIndex, int subWindow = EMPTY, int objectType = EMPTY)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ChartId", chartId },
                { "ObjectIndex", objectIndex },
                { "SubWindow", subWindow },
                { "ObjectType", objectType }
            };
            return SendCommand<string>(ExecutorHandle, MtCommandType.ObjectName, cmdParams);
        }

        ///<summary>
        ///The function removes the object with the specified name at the specified chart.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="objectName">Name of object to be deleted.</param>
        ///<returns>
        ///Returns true if the removal was successful, otherwise returns false.
        ///</returns>
        public bool ObjectDelete(long chartId, string objectName)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ChartId", chartId },
                { "ObjectName", objectName }
            };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ObjectDelete, cmdParams);
        }

        ///<summary>
        ///The function removes the object with the specified name at the specified chart.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="subWindow">Number of the chart window. Must be greater or equal to -1 (-1 mean all subwindows, 0 means the main chart window) and less than WindowsTotal().</param>
        ///<param name="objectType">Type of the object. The value can be one of the values of the EnumObject enumeration. EMPTY (-1) means all types.</param>
        ///<returns>
        ///Returns true if the removal was successful, otherwise returns false.
        ///</returns>
        public int ObjectsDeleteAll(long chartId, int subWindow = EMPTY, int objectType = EMPTY)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ChartId", chartId },
                { "SubWindow", subWindow },
                { "ObjectType", objectType }
            };
            return SendCommand<int>(ExecutorHandle, MtCommandType.ObjectsDeleteAll, cmdParams);
        }

        ///<summary>
        ///The function searches for an object having the specified name.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="objectName">The name of the object to find.</param>
        ///<returns>
        ///If successful the function returns the number of the subwindow (0 means the main window of the chart), in which the object is found. 
        ///</returns>
        public int ObjectFind(long chartId, string objectName)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ChartId", chartId },
                { "ObjectName", objectName }
            };
            return SendCommand<int>(ExecutorHandle, MtCommandType.ObjectFind, cmdParams);
        }

        ///<summary>
        ///The function returns the time value for the specified price value of the specified object.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="objectName">Name of the object.</param>
        ///<param name="value">Price value.</param>
        ///<param name="lineId">Line identifier.</param>
        ///<returns>
        ///The time value for the specified price value of the specified object.
        ///</returns>
        public DateTime ObjectGetTimeByValue(long chartId, string objectName, double value, int lineId = 0)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ChartId", chartId },
                { "ObjectName", objectName },
                { "Value", value },
                { "LineId", lineId }
            };
            var res = SendCommand<int>(ExecutorHandle, MtCommandType.ObjectGetTimeByValue, cmdParams);
            return MtApiTimeConverter.ConvertFromMtTime(res);
        }

        ///<summary>
        ///The function returns the price value for the specified time value of the specified object.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="objectName">Name of the object.</param>
        ///<param name="time">Time value.</param>
        ///<param name="lineId">Line identifier.</param>
        ///<returns>
        ///The price value for the specified time value of the specified object.
        ///</returns>
        public double ObjectGetValueByTime(long chartId, string objectName, DateTime? time, int lineId = 0)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ChartId", chartId },
                { "ObjectName", objectName },
                { "Time", MtApiTimeConverter.ConvertToMtTime(time) },
                { "LineId", lineId }
            };
            return SendCommand<double>(ExecutorHandle, MtCommandType.ObjectGetValueByTime, cmdParams);
        }

        ///<summary>
        ///The function changes coordinates of the specified anchor point of the object at the specified chart
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="objectName">Name of the object.</param>
        ///<param name="pointIndex">Index of the anchor point.</param>
        ///<param name="time">Time coordinate of the selected anchor point.</param>
        ///<param name="price">Price coordinate of the selected anchor point.</param>
        ///<returns>
        ///If successful, returns true, in case of failure returns false.
        ///</returns>
        public bool ObjectMove(long chartId, string objectName, int pointIndex, DateTime? time, double price)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ChartId", chartId },
                { "ObjectName", objectName },
                { "PointIndex", pointIndex },
                { "Time", MtApiTimeConverter.ConvertToMtTime(time) },
                { "Price", price }
            };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ObjectMove, cmdParams);
        }

        ///<summary>
        ///The function changes coordinates of the specified anchor point of the object at the specified chart.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="subWindow">Number of the chart subwindow. 0 means the main chart window, -1 means all the subwindows of the chart, including the main window.</param>
        ///<param name="type">Type of the object. The value can be one of the values of the EnumObject enumeration. EMPTY(-1) means all types.</param>
        ///<returns>
        ///The number of objects.
        ///</returns>
        public int ObjectsTotal(long chartId, int subWindow = EMPTY, int type = EMPTY)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ChartId", chartId },
                { "SubWindow", subWindow },
                { "Type", type }
            };
            return SendCommand<int>(ExecutorHandle, MtCommandType.ObjectsTotal, cmdParams);
        }

        ///<summary>
        ///The function returns the value of the corresponding object property.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="objectName">Name of the object.</param>
        ///<param name="propId">ID of the object property. The value can be one of the values of the EnumObjectPropertyDouble enumeration.</param>
        ///<param name="propModifier">Modifier of the specified property. For the first variant, the default modifier value is equal to 0. Most properties do not require a modifier.</param>
        ///<returns>
        ///Value of the double type.
        ///</returns>
        public double ObjectGetDouble(long chartId, string objectName, EnumObjectPropertyDouble propId, int propModifier = 0)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ChartId", chartId },
                { "ObjectName", objectName },
                { "PropId", (int)propId },
                { "PropModifier", propModifier }
            };
            return SendCommand<double>(ExecutorHandle, MtCommandType.ObjectGetDouble, cmdParams);
        }

        ///<summary>
        ///The function returns the value of the corresponding object property. The object property must be of the datetime, int, color, bool or char type.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="objectName">Name of the object.</param>
        ///<param name="propId">ID of the object property. The value can be one of the values of the EnumObjectPropertyDouble enumeration.</param>
        ///<param name="propModifier">Modifier of the specified property. For the first variant, the default modifier value is equal to 0. Most properties do not require a modifier.</param>
        ///<returns>
        ///The long value.
        ///</returns>
        public long ObjectGetInteger(long chartId, string objectName, EnumObjectPropertyInteger propId, int propModifier = 0)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ChartId", chartId },
                { "ObjectName", objectName },
                { "PropId", (int)propId },
                { "PropModifier", propModifier }
            };
            return SendCommand<long>(ExecutorHandle, MtCommandType.ObjectGetInteger, cmdParams);
        }

        ///<summary>
        ///The function returns the value of the corresponding object property. The object property must be of the string type.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="objectName">Name of the object.</param>
        ///<param name="propId">ID of the object property. The value can be one of the values of the EnumObjectPropertyString enumeration.</param>
        ///<param name="propModifier">Modifier of the specified property. For the first variant, the default modifier value is equal to 0. Most properties do not require a modifier.  It denotes the number of the level in Fibonacci tools and in the graphical object Andrew's pitchfork. The numeration of levels starts from zero.</param>
        ///<returns>
        ///String value.
        ///</returns>
        public string? ObjectGetString(long chartId, string objectName, EnumObjectPropertyString propId, int propModifier = 0)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ChartId", chartId },
                { "ObjectName", objectName },
                { "PropId", (int)propId },
                { "PropModifier", propModifier }
            };
            return SendCommand<string>(ExecutorHandle, MtCommandType.ObjectGetString, cmdParams);
        }

        ///<summary>
        ///The function sets the value of the corresponding object property. The object property must be of the double type. 
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="objectName">Name of the object.</param>
        ///<param name="propId">ID of the object property. The value can be one of the values of the EnumObjectPropertyDouble enumeration.</param>
        ///<param name="propValue">The value of the property.</param>
        ///<returns>
        ///The function returns true only if the command to change properties of a graphical object has been sent to a chart successfully.
        ///</returns>
        public bool ObjectSetDouble(long chartId, string objectName, EnumObjectPropertyDouble propId, double propValue)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ChartId", chartId },
                { "ObjectName", objectName },
                { "PropId", (int)propId },
                { "PropValue", propValue }
            };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ObjectSetDouble, cmdParams);
        }

        ///<summary>
        ///The function sets the value of the corresponding object property. The object property must be of the double type. 
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="objectName">Name of the object.</param>
        ///<param name="propId">ID of the object property. The value can be one of the values of the EnumObjectPropertyDouble enumeration.</param>
        ///<param name="propModifier">Modifier of the specified property. It denotes the number of the level in Fibonacci tools and in the graphical object Andrew's pitchfork. The numeration of levels starts from zero.</param>
        ///<param name="propValue">The value of the property.</param>
        ///<returns>
        ///The function returns true only if the command to change properties of a graphical object has been sent to a chart successfully.
        ///</returns>
        public bool ObjectSetDouble(long chartId, string objectName, EnumObjectPropertyDouble propId, int propModifier, double propValue)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ChartId", chartId },
                { "ObjectName", objectName },
                { "PropId", (int)propId },
                { "PropModifier", propModifier },
                { "PropValue", propValue }
            };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ObjectSetDouble, cmdParams);
        }

        ///<summary>
        ///The function sets the value of the corresponding object property. The object property must be of the int type. 
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="objectName">Name of the object.</param>
        ///<param name="propId">ID of the object property. The value can be one of the values of the EnumObjectPropertyInteger enumeration.</param>
        ///<param name="propValue">The value of the property.</param>
        ///<returns>
        ///The function returns true only if the command to change properties of a graphical object has been sent to a chart successfully. Otherwise it returns false. 
        ///</returns>
        public bool ObjectSetInteger(long chartId, string objectName, EnumObjectPropertyInteger propId, long propValue)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ChartId", chartId },
                { "ObjectName", objectName },
                { "PropId", (int)propId },
                { "PropValue", propValue }
            };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ObjectSetInteger, cmdParams);
        }

        ///<summary>
        ///The function sets the value of the corresponding object property. The object property must be of the int type. 
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="objectName">Name of the object.</param>
        ///<param name="propId">ID of the object property. The value can be one of the values of the EnumObjectPropertyInteger enumeration.</param>
        ///<param name="propModifier">Modifier of the specified property. It denotes the number of the level in Fibonacci tools and in the graphical object Andrew's pitchfork. The numeration of levels starts from zero.</param>
        ///<param name="propValue">The value of the property.</param>
        ///<returns>
        ///The function returns true only if the command to change properties of a graphical object has been sent to a chart successfully. Otherwise it returns false. 
        ///</returns>
        public bool ObjectSetInteger(long chartId, string objectName, EnumObjectPropertyInteger propId, int propModifier, long propValue)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ChartId", chartId },
                { "ObjectName", objectName },
                { "PropId", (int)propId },
                { "PropModifier", propModifier },
                { "PropValue", propValue }
            };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ObjectSetInteger, cmdParams);
        }

        ///<summary>
        ///The function sets the value of the corresponding object property. The object property must be of the string type. 
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="objectName">Name of the object.</param>
        ///<param name="propId">ID of the object property. The value can be one of the values of the EnumObjectPropertyString enumeration.</param>
        ///<param name="propValue">The value of the property.</param>
        ///<returns>
        ///The function returns true only if the command to change properties of a graphical object has been sent to a chart successfully. Otherwise it returns false. 
        ///</returns>
        public bool ObjectSetString(long chartId, string objectName, EnumObjectPropertyString propId, string propValue)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ChartId", chartId },
                { "ObjectName", objectName },
                { "PropId", (int)propId },
                { "PropValue", propValue }
            };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ObjectSetString, cmdParams);
        }

        ///<summary>
        ///The function sets the value of the corresponding object property. The object property must be of the string type. 
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="objectName">Name of the object.</param>
        ///<param name="propId">ID of the object property. The value can be one of the values of the EnumObjectPropertyString enumeration.</param>
        ///<param name="propModifier">Modifier of the specified property. It denotes the number of the level in Fibonacci tools and in the graphical object Andrew's pitchfork. The numeration of levels starts from zero.</param>
        ///<param name="propValue">The value of the property.</param>
        ///<returns>
        ///The function returns true only if the command to change properties of a graphical object has been sent to a chart successfully. Otherwise it returns false. 
        ///</returns>
        public bool ObjectSetString(long chartId, string objectName, EnumObjectPropertyString propId, int propModifier, string propValue)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ChartId", chartId },
                { "ObjectName", objectName },
                { "PropId", (int)propId },
                { "PropModifier", propModifier },
                { "PropValue", propValue }
            };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ObjectSetString, cmdParams);
        }

        ///<summary>
        ///The function sets the font for displaying the text using drawing methods and returns the result of that operation. Arial font with the size -120 (12 pt) is used by default.
        ///</summary>
        ///<param name="name">Font name in the system or the name of the resource containing the font or the path to font file on the disk.</param>
        ///<param name="size">The font size that can be set using positive and negative values. In case of positive values, the size of a displayed text does not depend on the operating system's font size settings. In case of negative values, the value is set in tenths of a point and the text size depends on the operating system settings ("standard scale" or "large scale"). See the Note below for more information about the differences between the modes.</param>
        ///<param name="flags">Combination of flags describing font style.</param>
        ///<param name="orientation">Text's horizontal inclination to X axis, the unit of measurement is 0.1 degrees. It means that orientation=450 stands for inclination equal to 45 degrees.</param>
        ///<returns>
        ///Returns true if the current font is successfully installed, otherwise false.
        ///</returns>
        public bool TextSetFont(string name, int size, FlagFontStyle flags = 0, int orientation = 0)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "Name", name },
                { "Size", size },
                { "Flags", flags },
                { "Orientation", orientation }
            };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.TextSetFont, cmdParams);
        }

        ///<summary>
        ///Returns the object description.
        ///</summary>
        ///<param name="objectName">Object name.</param>
        ///<returns>
        ///Object description. For objects of OBJ_TEXT and OBJ_LABEL types, the text drawn by these objects will be returned.
        ///</returns>
        public string? ObjectDescription(string objectName)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ObjectName", objectName }
            };
            return SendCommand<string>(ExecutorHandle, MtCommandType.ObjectDescription, cmdParams);
        }

        ///<summary>
        ///Returns the level description of a Fibonacci object.
        ///</summary>
        ///<param name="objectName">Fibonacci object name.</param>
        ///<param name="index">Index of the Fibonacci level (0-31).</param>
        ///<returns>
        ///The level description of a Fibonacci object.
        ///</returns>
        public string? ObjectGetFiboDescription(string objectName, int index)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ObjectName", objectName },
                { "Index", index }
            };
            return SendCommand<string>(ExecutorHandle, MtCommandType.ObjectGetFiboDescription, cmdParams);
        }

        ///<summary>
        ///The function calculates and returns bar index (shift related to the current bar) for the given price.
        ///</summary>
        ///<param name="objectName">Object name.</param>
        ///<param name="value">Price value.</param>
        ///<returns>
        ///The function calculates and returns bar index (shift related to the current bar) for the given price. The bar index is calculated by the first and second coordinates using a linear equation. Applied to trendlines and similar objects.
        ///</returns>
        public int ObjectGetShiftByValue(string objectName, double value)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ObjectName", objectName },
                { "Value", value }
            };
            return SendCommand<int>(ExecutorHandle, MtCommandType.ObjectGetShiftByValue, cmdParams);
        }

        ///<summary>
        ///The function calculates and returns the price value for the specified bar (shift related to the current bar).
        ///</summary>
        ///<param name="objectName">Object name.</param>
        ///<param name="shift">Bar index.</param>
        ///<returns>
        ///The function calculates and returns the price value for the specified bar (shift related to the current bar). The price value is calculated by the first and second coordinates using a linear equation. Applied to trendlines and similar objects.
        ///</returns>
        public double ObjectGetValueByShift(string objectName, int shift)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ObjectName", objectName },
                { "Shift", shift }
            };
            return SendCommand<double>(ExecutorHandle, MtCommandType.ObjectGetValueByShift, cmdParams);
        }

        ///<summary>
        ///Changes the value of the specified object property.
        ///</summary>
        ///<param name="objectName">Object name.</param>
        ///<param name="index">Object property index. It can be any of object properties enumeration values.</param>
        ///<param name="value">New value of the given property.</param>
        ///<returns>
        ///If the function succeeds, the returned value will be true, otherwise it returns false.
        ///</returns>
        public bool ObjectSet(string objectName, int index, double value)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ObjectName", objectName },
                { "Index", index },
                { "Value", value }
            };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ObjectSet, cmdParams);
        }

        ///<summary>
        ///The function sets a new description to a level of a Fibonacci object.
        ///</summary>
        ///<param name="objectName">Object name.</param>
        ///<param name="index">Index of the Fibonacci level (0-31).</param>
        ///<param name="text">New description of the level.</param>
        ///<returns>
        ///The function returns true if successful, otherwise false.
        ///</returns>
        public bool ObjectSetFiboDescription(string objectName, int index, string text)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ObjectName", objectName },
                { "Index", index },
                { "Text", text }
            };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ObjectSetFiboDescription, cmdParams);
        }

        ///<summary>
        ///The function changes the object description.
        ///</summary>
        ///<param name="objectName">Object name.</param>
        ///<param name="text">A text describing the object.</param>
        ///<param name="fontSize">Font size in points.</param>
        ///<param name="fontName">Font name.</param>
        ///<param name="textColor">Font color.</param>
        ///<returns>
        ///Changes the object description.  If the function succeeds, the returned value will be true, otherwise false.
        ///</returns>
        public bool ObjectSetText(string objectName, string text, int fontSize = 0, string? fontName = null, Color? textColor = null)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ObjectName", objectName },
                { "Text", text },
                { "FontSize", fontSize },
                { "FontName", fontName ?? string.Empty },
                { "TextColor", MtApiColorConverter.ConvertToMtColor(textColor) }
            };
            return SendCommand<bool>(ExecutorHandle, MtCommandType.ObjectSetText, cmdParams);
        }

        ///<summary>
        ///The function returns the object type value.
        ///</summary>
        ///<param name="objectName">Object name.</param>
        ///<returns>
        ///The function returns the object type value. 
        ///</returns>
        public EnumObject ObjectType(string objectName)
        {
            Dictionary<string, object> cmdParams = new()
            {
                { "ObjectName", objectName },
            };
            return (EnumObject)SendCommand<int>(ExecutorHandle, MtCommandType.ObjectType, cmdParams);
        }

        #endregion

        #region Backtesting functions

        public void UnlockTicks()
        {
            SendCommand<object>(ExecutorHandle, MtCommandType.UnlockTicks);
        }

        #endregion

        #region Private Methods
        private MtRpcClient? Client
        {
            get
            {
                lock(_locker)
                {
                    return _client;
                }
            }
        }

        private async Task Connect(string host, int port)
        {
            lock (_locker)
            {
                if (_connectionState == MtConnectionState.Connected
                    || _connectionState == MtConnectionState.Connecting)
                {
                    return;
                }

                _connectionState = MtConnectionState.Connecting;
            }

            string message = $"Connecting to {host}:{port}";
            ConnectionStateChanged?.Invoke(this, new MtConnectionEventArgs(MtConnectionState.Connecting, message));

            var client = new MtRpcClient(host, port, new RpcClientLogger(Log));
            client.ExpertList += Client_ExpertList;
            client.ExpertAdded += Client_ExpertAdded;
            client.ExpertRemoved += Client_ExpertRemoved;
            client.MtEventReceived += Client_MtEventReceived;
            client.ConnectionFailed += Client_OnConnectionFailed;
            client.Disconnected += Client_Disconnected;

            try
            {
                await client.Connect();
                Log.Info($"Connected to {host}:{port}");
                lock (_locker)
                {
                    _client = client;
                    _connectionState = MtConnectionState.Connected;
                }
                client.NotifyClientReady();
                ConnectionStateChanged?.Invoke(this, new MtConnectionEventArgs(MtConnectionState.Connected, $"Connected to {host}:{port}"));
            }
            catch (Exception e)
            {
                Log.Warn($"Failed connection to {host}:{port}. {e.Message}");
                lock (_locker)
                {
                    _connectionState = MtConnectionState.Failed;
                }
                ConnectionStateChanged?.Invoke(this, new MtConnectionEventArgs(MtConnectionState.Failed, e.Message));
            }
        }

        private void Disconnect(bool failed)
        {
            var state = failed ? MtConnectionState.Failed : MtConnectionState.Disconnected;
            var message = failed ? "Connection Failed" : "Disconnected";

            MtRpcClient? client;

            lock (_locker)
            {
                if (_connectionState == MtConnectionState.Disconnected
                    || _connectionState == MtConnectionState.Failed)
                    return;

                _connectionState = state;
                client = _client;
                _client = null;

                _quotes.Clear();
                _experts.Clear();
                _executorHandle = 0;
            }

            client?.Disconnect();

            Log.Info(message);

            ConnectionStateChanged?.Invoke(this, new MtConnectionEventArgs(state, message));
        }

        private T? SendCommand<T>(int expertHandle, MtCommandType commandType, object? payload = null)
        {
            var client = Client;
            if (client == null)
            {
                Log.Warn("SendCommand: No connection");
                throw new Exception("No connection");
            }

            var payloadJson = payload == null ? string.Empty : JsonConvert.SerializeObject(payload);
            Log.Debug($"SendCommand: sending '{payloadJson}' ...");

            var responseJson = client.SendCommand(expertHandle, (int)commandType, payloadJson);

            Log.Debug($"SendCommand: received response JSON [{responseJson}]");

            if (string.IsNullOrEmpty(responseJson))
            {
                Log.Warn("SendCommand: Response JSON from MetaTrader is null or empty");
                throw new MtExecutionException(MtErrorCode.MtApiCustomError, "Response from MetaTrader is null");
            }

            var response = JsonConvert.DeserializeObject<Response<T>>(responseJson);
            if (response == null)
            {
                Log.Warn("SendCommand: Failed to deserialize response from JSON");
                throw new MtExecutionException(MtErrorCode.MtApiCustomError, "Response from MetaTrader is null");
            }

            if (response.ErrorCode != 0)
            {
                Log.Warn($"SendCommand: ErrorCode = {response.ErrorCode}. {response.ErrorMessage}");
                throw new MtExecutionException((MtErrorCode)response.ErrorCode, response.ErrorMessage);
            }

            return (response.Value == null) ? default : response.Value;
        }

        private void Client_MtEventReceived(object? sender, MtEventArgs e)
        {
            Task.Run(() => _mtEventHandlers[(MtEventTypes)e.EventType](e.ExpertHandle, e.Payload));
        }

        private void Client_ExpertList(object? sender, MtExpertListEventArgs e)
        {
            Task.Run(() => ProcessExpertList(e.Experts));
        }

        private void Client_ExpertAdded(object? sender, MtExpertEventArgs e)
        {
            Task.Run(() => ProcessExpertAdded(e.Expert));
        }

        private void Client_ExpertRemoved(object? sender, MtExpertEventArgs e)
        {
            Task.Run(() => ProcessExpertRemoved(e.Expert));
        }

        private void Client_OnConnectionFailed(object? sender, EventArgs e)
        {
            Log.Info("Received connection failed");
            Disconnect(true);
        }

        private void Client_Disconnected(object? sender, EventArgs e)
        {
            Log.Info("Received normal disconnection");
            Disconnect(false);
        }

        private void ReceivedOnTickEvent(int expertHandle, string payload)
        {
            var e = JsonConvert.DeserializeObject<MtRpcQuote>(payload);
            if (e == null || string.IsNullOrEmpty(e.Instrument) || e.Tick == null)
                return;

            QuoteUpdated?.Invoke(this, e.Instrument, e.Tick.Bid, e.Tick.Ask);

            MtQuote quote = new()
            {
                Instrument = e.Instrument,
                Bid = e.Tick.Bid,
                Ask = e.Tick.Ask,
                ExpertHandle = expertHandle
            };
            QuoteUpdate?.Invoke(this, new MtQuoteEventArgs(quote));
        }

        private void ReceivedOnLastTimeBarEvent(int expertHandle, string payload)
        {
            var e = JsonConvert.DeserializeObject<MtTimeBar>(payload);
            if (e == null || string.IsNullOrEmpty(e.Symbol))
                return;
            OnLastTimeBar?.Invoke(this, new TimeBarArgs(expertHandle, e));
        }

        private void ReceiveOnChartEvent(int expertHandle, string payload)
        {
            var e = JsonConvert.DeserializeObject<MtChartEvent>(payload);
            if (e == null)
                return;

            OnChartEvent?.Invoke(this, new ChartEventArgs(expertHandle, e));
        }

        private void ReceivedOnLockTicksEvent(int expertHandle, string payload)
        {
            var e = JsonConvert.DeserializeObject<OnLockTicksEvent>(payload);
            if (e == null || string.IsNullOrEmpty(e.Instrument))
                return;
            OnLockTicks?.Invoke(this, new MtLockTicksEventArgs(expertHandle, e.Instrument));
        }

        private void ProcessExpertList(HashSet<int> experts)
        {
            if (experts == null || experts.Count == 0)
            {
                Log.Warn("ProcessExpertList: expert list invalid or empty");
                return;
            }

            Dictionary<int, MtQuote> quotes = [];
            foreach (var handle in experts)
            {
                var quote = GetQuote(handle);
                if (quote != null)
                    quotes[handle] = quote;
            }

            lock (_locker)
            {
                _experts = experts;
                _quotes = quotes;
                if (_executorHandle == 0)
                    _executorHandle = (_experts.Count > 0) ? _experts.ElementAt(0) : 0;
            }
            _quotesWaiter.Set();

            QuoteList?.Invoke(this, new(quotes.Values.ToList()));

            if (IsTesting())
            {
                BacktestingReady();
            }
        }

        private void ProcessExpertAdded(int handle)
        {
            Log.Debug($"ProcessExpertAdded: {handle}");

            bool added;
            lock (_locker)
            {
                added = _experts.Add(handle);
                if (_executorHandle == 0)
                    _executorHandle = (_experts.Count > 0) ? _experts.ElementAt(0) : 0;
            }

            if (added)
            {
                var quote = GetQuote(handle);
                if (quote != null)
                {
                    lock (_locker)
                    {
                        _quotes[handle] = quote;
                    }

                    QuoteAdded?.Invoke(this, new MtQuoteEventArgs(quote));
                }
                else
                    Log.Warn($"ProcessExpertAdded: failed to get quote for expert {handle}");
            }
            else
                Log.Warn($"ProcessExpertAdded: expert handle {handle} is already exist");
        }

        private void ProcessExpertRemoved(int handle)
        {
            Log.Debug($"ProcessExpertRemoved: {handle}");

            MtQuote? quote = null;
            lock (_locker)
            {
                _experts.Remove(handle);
                if (_quotes.TryGetValue(handle, out quote))
                    _quotes.Remove(handle);
                if (_executorHandle == handle)
                    _executorHandle = (_experts.Count > 0) ? _experts.ElementAt(0) : 0;
            }

            if (quote != null)
                QuoteRemoved?.Invoke(this, new MtQuoteEventArgs(quote));
        }

        private MtQuote? GetQuote(int expertHandle)
        {
            Log.Debug($"GetQuote: expertHandle = {expertHandle}");

            var e = SendCommand<MtRpcQuote>(expertHandle, MtCommandType.GetQuote);
            if (e == null || string.IsNullOrEmpty(e.Instrument) || e.Tick == null)
                return null;

            MtQuote quote = new()
            {
                Instrument = e.Instrument,
                Bid = e.Tick.Bid,
                Ask = e.Tick.Ask,
                ExpertHandle = expertHandle,
            };

            return quote;
        }

        private void BacktestingReady()
        {
            SendCommand<object>(ExecutorHandle, MtCommandType.BacktestingReady);
        }

        #endregion

        #region Events

        public event MtApiQuoteHandler? QuoteUpdated;
        public event EventHandler<MtQuoteEventArgs>? QuoteUpdate;
        public event EventHandler<MtQuoteEventArgs>? QuoteAdded;
        public event EventHandler<MtQuoteEventArgs>? QuoteRemoved;
        public event EventHandler<MtConnectionEventArgs>? ConnectionStateChanged;
        public event EventHandler<TimeBarArgs>? OnLastTimeBar;
        public event EventHandler<ChartEventArgs>? OnChartEvent;
        public event EventHandler<MtLockTicksEventArgs>? OnLockTicks;
        public event EventHandler<MtQuotesEventArgs>? QuoteList;

        #endregion
    }

    internal enum ParametersType
    {
        Int = 0,
        Double = 1,
        String = 2,
        Boolean = 3
    }

    internal class RpcClientLogger(IMtLogger logger) : IRpcLogger
    {
        public void Debug(string message)
        {
            logger_.Debug(message);
        }

        public void Error(string message)
        {
            logger_.Debug(message);
        }

        public void Info(string message)
        {
            logger_.Debug(message);
        }

        public void Warn(string message)
        {
            logger_.Debug(message);
        }

        private readonly IMtLogger logger_ = logger;
    }

    internal class StubMtLogger : IMtLogger
    {
        public void Debug(object message)
        {
        }

        public void Error(object message)
        {
        }

        public void Fatal(object message)
        {
        }

        public void Info(object message)
        {
        }

        public void Warn(object message)
        {
        }
    }
}
