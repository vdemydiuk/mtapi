using System;
using System.Collections.Generic;
using System.Linq;
using MTApiService;
using System.Drawing;
using System.Collections;
using System.ServiceModel;
using MtApi.Requests;
using MtApi.Responses;
using Newtonsoft.Json;

namespace MtApi
{
    public delegate void MtApiQuoteHandler(object sender, string symbol, double bid, double ask);

    public sealed class MtApiClient
    {
        private volatile bool _isBacktestingMode;

        #region MetaTrader Constants

        //Special constant
        public const int NULL = 0;
        public const int EMPTY = -1;

        private const string LogProfileName = "MtApiClient";
        #endregion

        #region Private Fields
        private readonly MtClient _client = new MtClient();
        private readonly object _locker = new object();
        private MtConnectionState _connectionState = MtConnectionState.Disconnected;
        #endregion

        #region ctor

        public MtApiClient()
        {
            LogConfigurator.Setup(LogProfileName);

            _client.QuoteAdded += _client_QuoteAdded;
            _client.QuoteRemoved += _client_QuoteRemoved;
            _client.QuoteUpdated += _client_QuoteUpdated;
            _client.ServerDisconnected += _client_ServerDisconnected;
            _client.ServerFailed += _client_ServerFailed;
            _client.MtEventReceived += _client_MtEventReceived;
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
            Action<string, int> connectAction = Connect;
            connectAction.BeginInvoke(host, port, null, null);
        }

        ///<summary>
        ///Connect with MetaTrader API. Async method.
        ///</summary>
        ///<param name="port">Port of host connection (default 8222) </param>
        public void BeginConnect(int port)
        {
            Action<int> connectAction = Connect;
            connectAction.BeginInvoke(port, null, null);
        }

        ///<summary>
        ///Disconnect from MetaTrader API. Async method.
        ///</summary>
        public void BeginDisconnect()
        {
            Action disconnectAction = Disconnect;
            disconnectAction.BeginInvoke(null, null);
        }

        ///<summary>
        ///Load quotes connected into MetaTrader API.
        ///</summary>
        public IEnumerable<MtQuote> GetQuotes()
        {
            IEnumerable<MTApiService.MtQuote> quotes;
            lock (_client)
            {
                quotes = _client.GetQuotes();
            }
            return quotes?.Select(q => q.Convert());
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
            return SendCommand<double>(MtCommandType.OrderClosePrice, null);
        }

        [Obsolete("OrderClosePrice is deprecated, please use GetOrder instead.")]
        public double OrderClosePrice(int ticket)
        {
            var commandParameters = new ArrayList { ticket };
            double retVal = SendCommand<double>(MtCommandType.OrderClosePriceByTicket, commandParameters);

            return retVal;
        }

        [Obsolete("OrderCloseTime is deprecated, please use GetOrder instead.")]
        public DateTime OrderCloseTime()
        {
            var commandResponse = SendCommand<int>(MtCommandType.OrderCloseTime, null);
            return MtApiTimeConverter.ConvertFromMtTime(commandResponse);
        }

        [Obsolete("OrderComment is deprecated, please use GetOrder instead.")]
        public string OrderComment()
        {
            return SendCommand<string>(MtCommandType.OrderComment, null);
        }

        [Obsolete("OrderCommission is deprecated, please use GetOrder instead.")]
        public double OrderCommission()
        {
            return SendCommand<double>(MtCommandType.OrderCommission, null);
        }

        [Obsolete("OrderExpiration is deprecated, please use GetOrder instead.")]
        public DateTime OrderExpiration()
        {
            var commandResponse = SendCommand<int>(MtCommandType.OrderExpiration, null);
            return MtApiTimeConverter.ConvertFromMtTime(commandResponse);
        }

        [Obsolete("OrderLots is deprecated, please use GetOrder instead.")]
        public double OrderLots()
        {
            return SendCommand<double>(MtCommandType.OrderLots, null);
        }

        [Obsolete("OrderMagicNumber is deprecated, please use GetOrder instead.")]
        public int OrderMagicNumber()
        {
            return SendCommand<int>(MtCommandType.OrderMagicNumber, null);
        }

        [Obsolete("OrderOpenPrice is deprecated, please use GetOrder instead.")]
        public double OrderOpenPrice()
        {
            return SendCommand<double>(MtCommandType.OrderOpenPrice, null);
        }

        [Obsolete("OrderOpenPrice is deprecated, please use GetOrder instead.")]
        public double OrderOpenPrice(int ticket)
        {
            var commandParameters = new ArrayList { ticket };
            var retVal = SendCommand<double>(MtCommandType.OrderOpenPriceByTicket, commandParameters);

            return retVal;
        }

        [Obsolete("OrderOpenTime is deprecated, please use GetOrder instead.")]
        public DateTime OrderOpenTime()
        {
            var commandResponse = SendCommand<int>(MtCommandType.OrderOpenTime, null);
            return MtApiTimeConverter.ConvertFromMtTime(commandResponse);
        }

        [Obsolete("OrderProfit is deprecated, please use GetOrder instead.")]
        public double OrderProfit()
        {
            return SendCommand<double>(MtCommandType.OrderProfit, null);
        }

        [Obsolete("OrderStopLoss is deprecated, please use GetOrder instead.")]
        public double OrderStopLoss()
        {
            return SendCommand<double>(MtCommandType.OrderStopLoss, null);
        }

        [Obsolete("OrderSymbol is deprecated, please use GetOrder instead.")]
        public string OrderSymbol()
        {
            return SendCommand<string>(MtCommandType.OrderSymbol, null);
        }

        [Obsolete("OrderTakeProfit is deprecated, please use GetOrder instead.")]
        public double OrderTakeProfit()
        {
            return SendCommand<double>(MtCommandType.OrderTakeProfit, null);
        }

        [Obsolete("OrderTicket is deprecated, please use GetOrder instead.")]
        public int OrderTicket()
        {
            return SendCommand<int>(MtCommandType.OrderTicket, null);
        }

        [Obsolete("OrderType is deprecated, please use GetOrder instead.")]
        public TradeOperation OrderType()
        {
            var retVal = SendCommand<int>(MtCommandType.OrderType, null);

            return (TradeOperation)retVal;
        }

        [Obsolete("OrderSwap is deprecated, please use GetOrder instead.")]
        public double OrderSwap()
        {
            return SendCommand<double>(MtCommandType.OrderSwap, null);
        }
        #endregion

        #region Trading functions

        public int OrderSend(string symbol, TradeOperation cmd, double volume, double price, int slippage, double stoploss, double takeprofit
            , string comment, int magic, DateTime expiration, Color arrowColor)
        {
            var response = SendRequest<OrderSendResponse>(new OrderSendRequest
            {
                Symbol = symbol,
                Cmd = (int)cmd,
                Volume = volume,
                Price = price,
                Slippage = slippage,
                Stoploss = stoploss,
                Takeprofit = takeprofit,
                Comment = comment,
                Magic = magic,
                Expiration = MtApiTimeConverter.ConvertToMtTime(expiration),
                ArrowColor = MtApiColorConverter.ConvertToMtColor(arrowColor)
            });
            return response?.Ticket ?? -1;
        }

        public int OrderSend(string symbol, TradeOperation cmd, double volume, double price, int slippage, double stoploss, double takeprofit
                    , string comment, int magic, DateTime expiration)
        {
            return OrderSend(symbol, cmd, volume, price, slippage, stoploss, takeprofit, comment, magic, expiration, Color.Empty);
        }

        public int OrderSend(string symbol, TradeOperation cmd, double volume, double price, int slippage, double stoploss, double takeprofit
                    , string comment, int magic)
        {
            return OrderSend(symbol, cmd, volume, price, slippage, stoploss, takeprofit, comment, magic, DateTime.MinValue, Color.Empty);
        }

        public int OrderSend(string symbol, TradeOperation cmd, double volume, double price, int slippage, double stoploss, double takeprofit
                    , string comment)
        {
            return OrderSend(symbol, cmd, volume, price, slippage, stoploss, takeprofit, comment, 0, DateTime.MinValue, Color.Empty);
        }

        public int OrderSend(string symbol, TradeOperation cmd, double volume, double price, int slippage, double stoploss, double takeprofit)
        {
            return OrderSend(symbol, cmd, volume, price, slippage, stoploss, takeprofit, null, 0, DateTime.MinValue, Color.Empty);
        }

        public int OrderSend(string symbol, TradeOperation cmd, double volume, string price, int slippage, double stoploss, double takeprofit)
        {
            double dPrice;
            return double.TryParse(price, out dPrice) ? 
                OrderSend(symbol, cmd, volume, dPrice, slippage, stoploss, takeprofit, null, 0, DateTime.MinValue, Color.Empty) : 0;
        }

        public int OrderSendBuy(string symbol, double volume, int slippage)
        {
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

        public int OrderSendBuy(string symbol, double volume, int slippage, double stoploss, double takeprofit, string comment, int magic)
        {
            var response = SendRequest<OrderSendResponse>(new OrderSendRequest
            {
                Symbol = symbol,
                Cmd = (int)TradeOperation.OP_BUY,
                Volume = volume,
                Slippage = slippage,
                Stoploss = stoploss,
                Takeprofit = takeprofit,
                Comment = comment,
                Magic = magic,
            });
            return response?.Ticket ?? -1;
        }

        public int OrderSendSell(string symbol, double volume, int slippage, double stoploss, double takeprofit, string comment, int magic)
        {
            var response = SendRequest<OrderSendResponse>(new OrderSendRequest
            {
                Symbol = symbol,
                Cmd = (int)TradeOperation.OP_SELL,
                Volume = volume,
                Slippage = slippage,
                Stoploss = stoploss,
                Takeprofit = takeprofit,
                Comment = comment,
                Magic = magic,
            });
            return response?.Ticket ?? -1;
        }

        public bool OrderClose(int ticket, double lots, double price, int slippage, Color color)
        {
            var response = SendRequest<ResponseBase>(new OrderCloseRequest
            {
                Ticket = ticket,
                Lots = lots,
                Price = price,
                Slippage = slippage,
                ArrowColor = MtApiColorConverter.ConvertToMtColor(color)
            });
            return response != null;
        }

        public bool OrderClose(int ticket, double lots, double price, int slippage)
        {
            var response = SendRequest<ResponseBase>(new OrderCloseRequest
            {
                Ticket = ticket,
                Lots = lots,
                Price = price,
                Slippage = slippage,
            });
            return response != null;
        }

        public bool OrderClose(int ticket, double lots, int slippage)
        {
            var response = SendRequest<ResponseBase>(new OrderCloseRequest
            {
                Ticket = ticket,
                Lots = lots,
                Slippage = slippage,
            });
            return response != null;
        }

        public bool OrderClose(int ticket, int slippage)
        {
            var response = SendRequest<ResponseBase>(new OrderCloseRequest
            {
                Ticket = ticket,
                Slippage = slippage,
            });
            return response != null;
        }

        public bool OrderCloseBy(int ticket, int opposite, Color color)
        {
            var response = SendRequest<ResponseBase>(new OrderCloseByRequest
            {
                Ticket = ticket,
                Opposite = opposite,
                ArrowColor = MtApiColorConverter.ConvertToMtColor(color)
            });
            return response != null;
        }

        public bool OrderCloseBy(int ticket, int opposite)
        {
            var response = SendRequest<ResponseBase>(new OrderCloseByRequest
            {
                Ticket = ticket,
                Opposite = opposite,
            });
            return response != null;
        }

        public bool OrderDelete(int ticket, Color color)
        {
            var response = SendRequest<ResponseBase>(new OrderDeleteRequest
            {
                Ticket = ticket,
                ArrowColor = MtApiColorConverter.ConvertToMtColor(color),
            });
            return response != null;
        }

        public bool OrderDelete(int ticket)
        {
            var response = SendRequest<ResponseBase>(new OrderDeleteRequest
            {
                Ticket = ticket,
            });
            return response != null;
        }

        public bool OrderModify(int ticket, double price, double stoploss, double takeprofit, DateTime expiration, Color arrowColor)
        {
            var response = SendRequest<ResponseBase>(new OrderModifyRequest
            {
                Ticket = ticket,
                Price = price,
                Stoploss = stoploss,
                Takeprofit = takeprofit,
                Expiration = MtApiTimeConverter.ConvertToMtTime(expiration),
                ArrowColor = MtApiColorConverter.ConvertToMtColor(arrowColor)
            });
            return response != null;
        }

        public bool OrderModify(int ticket, double price, double stoploss, double takeprofit, DateTime expiration)
        {
            var response = SendRequest<ResponseBase>(new OrderModifyRequest
            {
                Ticket = ticket,
                Price = price,
                Stoploss = stoploss,
                Takeprofit = takeprofit,
                Expiration = MtApiTimeConverter.ConvertToMtTime(expiration),
            });
            return response != null;
        }

        public void OrderPrint()
        {
            SendCommand<object>(MtCommandType.OrderPrint, null);
        }

        public bool OrderSelect(int index, OrderSelectMode select, OrderSelectSource pool)
        {
            var commandParameters = new ArrayList { index, (int)select, (int)pool };
            return SendCommand<bool>(MtCommandType.OrderSelect, commandParameters);
        }

        public bool OrderSelect(int index, OrderSelectMode select)
        {
            return OrderSelect(index, select, OrderSelectSource.MODE_TRADES);
        }

        public int OrdersHistoryTotal()
        {
            return SendCommand<int>(MtCommandType.OrdersHistoryTotal, null);
        }

        public int OrdersTotal()
        {
            return SendCommand<int>(MtCommandType.OrdersTotal, null);
        }

        public bool OrderCloseAll()
        {
            return SendCommand<bool>(MtCommandType.OrderCloseAll, null);
        }

        public MtOrder GetOrder(int index, OrderSelectMode select, OrderSelectSource pool)
        {
            var response = SendRequest<GetOrderResponse>(new GetOrderRequest { Index = index, Select = (int) select, Pool = (int) pool});
            return response?.Order;
        }

        public List<MtOrder> GetOrders(OrderSelectSource pool)
        {
            var response = SendRequest<GetOrdersResponse>(new GetOrdersRequest { Pool = (int)pool });
            return response != null ? response.Orders : new List<MtOrder>();
        }
        #endregion

        #region Check Status

        public int GetLastError()
        {
            return SendCommand<int>(MtCommandType.GetLastError, null);
        }

        public bool IsConnected()
        {
            return SendCommand<bool>(MtCommandType.IsConnected, null);
        }

        public bool IsDemo()
        {
            return SendCommand<bool>(MtCommandType.IsDemo, null);
        }

        public bool IsDllsAllowed()
        {
            return SendCommand<bool>(MtCommandType.IsDllsAllowed, null);
        }

        public bool IsExpertEnabled()
        {
            return SendCommand<bool>(MtCommandType.IsExpertEnabled, null);
        }

        public bool IsLibrariesAllowed()
        {
            return SendCommand<bool>(MtCommandType.IsLibrariesAllowed, null);
        }

        public bool IsOptimization()
        {
            return SendCommand<bool>(MtCommandType.IsOptimization, null);
        }

        public bool IsStopped()
        {
            return SendCommand<bool>(MtCommandType.IsStopped, null);
        }

        public bool IsTesting()
        {
            return SendCommand<bool>(MtCommandType.IsTesting, null);
        }

        public bool IsTradeAllowed()
        {
            return SendCommand<bool>(MtCommandType.IsTradeAllowed, null);
        }

        public bool IsTradeContextBusy()
        {
            return SendCommand<bool>(MtCommandType.IsTradeContextBusy, null);
        }

        public bool IsVisualMode()
        {
            return SendCommand<bool>(MtCommandType.IsVisualMode, null);
        }

        public int UninitializeReason()
        {
            return SendCommand<int>(MtCommandType.UninitializeReason, null);
        }

        public string ErrorDescription(int errorCode)
        {
            var commandParameters = new ArrayList { errorCode };
            return SendCommand<string>(MtCommandType.ErrorDescription, commandParameters);
        }

        #endregion

        #region Account Information
        
        public double AccountBalance()
        {
            return SendCommand<double>(MtCommandType.AccountBalance, null);
        }

        public double AccountCredit()
        {
            return SendCommand<double>(MtCommandType.AccountCredit, null);
        }

        public string AccountCompany()
        {
            return SendCommand<string>(MtCommandType.AccountCompany, null);
        }

        public string AccountCurrency()
        {
            return SendCommand<string>(MtCommandType.AccountCurrency, null);
        }

        public double AccountEquity()
        {
            return SendCommand<double>(MtCommandType.AccountEquity, null);
        }

        public double AccountFreeMargin()
        {
            return SendCommand<double>(MtCommandType.AccountFreeMargin, null);
        }

        public double AccountFreeMarginCheck(string symbol, TradeOperation cmd, double volume)
        {
            var commandParameters = new ArrayList { symbol, (int)cmd, volume };
            return SendCommand<double>(MtCommandType.AccountFreeMarginCheck, commandParameters);
        }

        public double AccountFreeMarginMode()
        {
            return SendCommand<double>(MtCommandType.AccountFreeMarginMode, null);
        }

        public int AccountLeverage()
        {
            return SendCommand<int>(MtCommandType.AccountLeverage, null);
        }

        public double AccountMargin()
        {
            return SendCommand<double>(MtCommandType.AccountMargin, null);
        }

        public string AccountName()
        {
            return SendCommand<string>(MtCommandType.AccountName, null);
        }

        public int AccountNumber()
        {
            return SendCommand<int>(MtCommandType.AccountNumber, null);
        }

        public double AccountProfit()
        {
            return SendCommand<double>(MtCommandType.AccountProfit, null);
        }

        public string AccountServer()
        {
            return SendCommand<string>(MtCommandType.AccountServer, null);
        }

        public int AccountStopoutLevel()
        {
            return SendCommand<int>(MtCommandType.AccountStopoutLevel, null);
        }

        public int AccountStopoutMode()
        {
            return SendCommand<int>(MtCommandType.AccountStopoutMode, null);
        }

        #endregion

        #region Symbols
        ///<summary>
        ///Returns the number of available (selected in Market Watch or all) symbols.
        ///</summary>
        ///<param name="selected">Request mode. Can be true or false.</param>
        ///<returns>
        ///If the 'selected' parameter is true, the function returns the number of symbols selected in MarketWatch. If the value is false, it returns the total number of all symbols.
        ///</returns>
        public int SymbolsTotal(bool selected)
        {
            var commandParameters = new ArrayList { selected };
            return SendCommand<int>(MtCommandType.SymbolsTotal, commandParameters);
        }

        ///<summary>
        ///Returns the name of a symbol.
        ///</summary>
        ///<param name="pos">Order number of a symbol.</param>
        ///<param name="selected">Request mode. If the value is true, the symbol is taken from the list of symbols selected in MarketWatch. If the value is false, the symbol is taken from the general list.</param>
        ///<returns>
        ///Value of string type with the symbol name.
        ///</returns>
        public string SymbolName(int pos, bool selected)
        {
            var commandParameters = new ArrayList { pos, selected };
            return SendCommand<string>(MtCommandType.SymbolName, commandParameters);
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
            var commandParameters = new ArrayList { name, select };
            return SendCommand<bool>(MtCommandType.SymbolSelect, commandParameters);
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
            var commandParameters = new ArrayList { name, (int)propId };
            return SendCommand<long>(MtCommandType.SymbolInfoInteger, commandParameters);
        }
        #endregion

        #region Common Function

        public void Alert(string msg)
        {
            var commandParameters = new ArrayList { msg };
            SendCommand<object>(MtCommandType.Alert, commandParameters);
        }

        public void Comment(string msg)
        {
            var commandParameters = new ArrayList { msg };
            SendCommand<object>(MtCommandType.Comment, commandParameters);
        }

        public int GetTickCount()
        {
            return SendCommand<int>(MtCommandType.GetTickCount, null);
        }

        public double MarketInfo(string symbol, MarketInfoModeType type)
        {
            var commandParameters = new ArrayList { symbol, (int)type };
            return SendCommand<double>(MtCommandType.MarketInfo, commandParameters);
        }

        public int MessageBox(string text, string caption, int flag)
        {
            var commandParameters = new ArrayList { text, caption, flag };
            return SendCommand<int>(MtCommandType.MessageBoxA, commandParameters);
        }

        public int MessageBox(string text, string caption)
        {
            return MessageBox(text, caption, EMPTY);
        }

        public int MessageBox(string text)
        {
            var commandParameters = new ArrayList { text };
            return SendCommand<int>(MtCommandType.MessageBox, commandParameters);
        }

        public void PlaySound(string filename)
        {
            var commandParameters = new ArrayList { filename };
            SendCommand<object>(MtCommandType.PlaySound, commandParameters);
        }

        public void Print(string msg)
        {
            var commandParameters = new ArrayList { msg };
            SendCommand<object>(MtCommandType.Print, commandParameters);
        }

        public bool SendFTP(string filename)
        {
            var commandParameters = new ArrayList { filename };
            return SendCommand<bool>(MtCommandType.SendFTP, commandParameters);
        }

        public bool SendFTP(string filename, string ftpPath)
        {
            var commandParameters = new ArrayList { filename, ftpPath };
            return SendCommand<bool>(MtCommandType.SendFTPA, commandParameters);
        }

        public void SendMail(string subject, string someText)
        {
            var commandParameters = new ArrayList { subject, someText };
            SendCommand<object>(MtCommandType.SendMail, commandParameters);
        }

        public void Sleep(int milliseconds)
        {
            var commandParameters = new ArrayList { milliseconds };
            SendCommand<object>(MtCommandType.Sleep, commandParameters);
        }

        #endregion

        #region Client Terminal Functions

        public string TerminalCompany()
        {
            return SendCommand<string>(MtCommandType.TerminalCompany, null);
        }

        public string TerminalName()
        {
            return SendCommand<string>(MtCommandType.TerminalName, null);
        }

        public string TerminalPath()
        {
            return SendCommand<string>(MtCommandType.TerminalPath, null);
        }

        #endregion

        #region Date and Time Functions

        public int Day()
        {
            return SendCommand<int>(MtCommandType.Day, null);
        }

        public int DayOfWeek()
        {
            return SendCommand<int>(MtCommandType.DayOfWeek, null);
        }

        public int DayOfYear()
        {
            return SendCommand<int>(MtCommandType.DayOfYear, null);
        }

        public int Hour()
        {
            return SendCommand<int>(MtCommandType.Hour, null);
        }

        public int Minute()
        {
            return SendCommand<int>(MtCommandType.Minute, null);
        }

        public int Month()
        {
            return SendCommand<int>(MtCommandType.Month, null);
        }

        public int Seconds()
        {
            return SendCommand<int>(MtCommandType.Seconds, null);
        }

        public DateTime TimeCurrent()
        {
            var commandResponse = SendCommand<int>(MtCommandType.TimeCurrent, null);
            return MtApiTimeConverter.ConvertFromMtTime(commandResponse);
        }

        public int TimeDay(DateTime date)
        {
            var commandParameters = new ArrayList { MtApiTimeConverter.ConvertToMtTime(date) };
            return SendCommand<int>(MtCommandType.TimeDay, commandParameters);
        }

        public int TimeDayOfWeek(DateTime date)
        {
            var commandParameters = new ArrayList { MtApiTimeConverter.ConvertToMtTime(date) };
            return SendCommand<int>(MtCommandType.TimeDayOfWeek, commandParameters);
        }

        public int TimeDayOfYear(DateTime date)
        {
            var commandParameters = new ArrayList { MtApiTimeConverter.ConvertToMtTime(date) };
            return SendCommand<int>(MtCommandType.TimeDayOfYear, commandParameters);
        }

        public int TimeHour(DateTime time)
        {
            var commandParameters = new ArrayList { MtApiTimeConverter.ConvertToMtTime(time) };
            return SendCommand<int>(MtCommandType.TimeHour, commandParameters);
        }

        public DateTime TimeLocal()
        {
            var commandResponse = SendCommand<int>(MtCommandType.TimeLocal, null);
            return MtApiTimeConverter.ConvertFromMtTime(commandResponse);
        }

        public int TimeMinute(DateTime time)
        {
            var commandParameters = new ArrayList { MtApiTimeConverter.ConvertToMtTime(time) };
            return SendCommand<int>(MtCommandType.TimeMinute, commandParameters);
        }

        public int TimeMonth(DateTime time)
        {
            var commandParameters = new ArrayList { MtApiTimeConverter.ConvertToMtTime(time) };
            return SendCommand<int>(MtCommandType.TimeMonth, commandParameters);
        }

        public int TimeSeconds(DateTime time)
        {
            var commandParameters = new ArrayList { MtApiTimeConverter.ConvertToMtTime(time) };
            return SendCommand<int>(MtCommandType.TimeSeconds, commandParameters);
        }

        public int TimeYear(DateTime time)
        {
            var commandParameters = new ArrayList { MtApiTimeConverter.ConvertToMtTime(time) };
            return SendCommand<int>(MtCommandType.TimeYear, commandParameters);
        }

        public int Year(DateTime time)
        {
            var commandParameters = new ArrayList { MtApiTimeConverter.ConvertToMtTime(time) };
            return SendCommand<int>(MtCommandType.Year, commandParameters);
        }

        #endregion

        #region Global Variables Functions
        public bool GlobalVariableCheck(string name)
        {
            var commandParameters = new ArrayList { name };
            return SendCommand<bool>(MtCommandType.GlobalVariableCheck, commandParameters);
        }

        public bool GlobalVariableDel(string name)
        {
            var commandParameters = new ArrayList { name };
            return SendCommand<bool>(MtCommandType.GlobalVariableDel, commandParameters);
        }

        public double GlobalVariableGet(string name)
        {
            var commandParameters = new ArrayList { name };
            return SendCommand<double>(MtCommandType.GlobalVariableGet, commandParameters);
        }

        public string GlobalVariableName(int index)
        {
            var commandParameters = new ArrayList { index };
            return SendCommand<string>(MtCommandType.GlobalVariableName, commandParameters);
        }

        public DateTime GlobalVariableSet(string name, double value)
        {
            var commandParameters = new ArrayList { name, value };
            var commandResponse = SendCommand<int>(MtCommandType.GlobalVariableSet, commandParameters);
            return MtApiTimeConverter.ConvertFromMtTime(commandResponse);
        }

        public bool GlobalVariableSetOnCondition(string name, double value, double checkValue)
        {
            var commandParameters = new ArrayList { name, value, checkValue };
            return SendCommand<bool>(MtCommandType.GlobalVariableSetOnCondition, commandParameters);
        }

        public int GlobalVariablesDeleteAll(string prefixName)
        {
            var commandParameters = new ArrayList { prefixName };
            return SendCommand<int>(MtCommandType.GlobalVariableSetOnCondition, commandParameters);
        }

        public int GlobalVariablesTotal()
        {
            return SendCommand<int>(MtCommandType.GlobalVariablesTotal, null);
        }

        #endregion

        #region Technical Indicators
        public double iAC(string symbol, ChartPeriod timeframe, int shift)
        {
            var commandParameters = new ArrayList { symbol, (int)timeframe, shift };
            return SendCommand<double>(MtCommandType.iAC, commandParameters);
        }

        public double iAD(string symbol, int timeframe, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, shift };
            return SendCommand<double>(MtCommandType.iAD, commandParameters);
        }

        public double iAlligator(string symbol, int timeframe, int jawPeriod, int jawShift, int teethPeriod, int teethShift, int lipsPeriod, int lipsShift, int maMethod, int appliedPrice, int mode, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, jawPeriod, jawShift, teethPeriod, teethShift, lipsPeriod, lipsShift, maMethod, appliedPrice, mode, shift };
            return SendCommand<double>(MtCommandType.iAlligator, commandParameters);
        }

        public double iADX(string symbol, int timeframe, int period, int appliedPrice, int mode, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, period, appliedPrice, mode, shift };
            return SendCommand<double>(MtCommandType.iADX, commandParameters);
        }

        public double iATR(string symbol, int timeframe, int period, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, period, shift };
            return SendCommand<double>(MtCommandType.iATR, commandParameters);
        }

        public double iAO(string symbol, int timeframe, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, shift };
            return SendCommand<double>(MtCommandType.iAO, commandParameters);
        }

        public double iBearsPower(string symbol, int timeframe, int period, int appliedPrice, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, period, appliedPrice, shift };
            return SendCommand<double>(MtCommandType.iBearsPower, commandParameters);
        }

        public double iBands(string symbol, int timeframe, int period, int deviation, int bandsShift, int appliedPrice, int mode, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, period, deviation, bandsShift, appliedPrice, mode, shift };
            return SendCommand<double>(MtCommandType.iBands, commandParameters);
        }

        public double iBandsOnArray(double[] array, int total, int period, int deviation, int bandsShift, int mode, int shift)
        {
            var arraySize = array?.Length ?? 0;
            var commandParameters = new ArrayList { arraySize };
            commandParameters.AddRange(array ?? new double[] {});
            commandParameters.Add(total);
            commandParameters.Add(period);
            commandParameters.Add(deviation);
            commandParameters.Add(bandsShift);
            commandParameters.Add(mode);
            commandParameters.Add(shift);

            return SendCommand<double>(MtCommandType.iBandsOnArray, commandParameters);
        }

        public double iBullsPower(string symbol, int timeframe, int period, int appliedPrice, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, period, appliedPrice, shift };
            return SendCommand<double>(MtCommandType.iBullsPower, commandParameters);
        }

        public double iCCI(string symbol, int timeframe, int period, int appliedPrice, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, period, appliedPrice, shift };
            return SendCommand<double>(MtCommandType.iCCI, commandParameters);
        }

        public double iCCIOnArray(double[] array, int total, int period, int shift)
        {
            var arraySize = array?.Length ?? 0;
            var commandParameters = new ArrayList { arraySize };
            commandParameters.AddRange(array ?? new double[] { });
            commandParameters.Add(total);
            commandParameters.Add(period);
            commandParameters.Add(shift);

            return SendCommand<double>(MtCommandType.iCCIOnArray, commandParameters);
        }

        public double iCustom(string symbol, int timeframe, string name, int[] parameters, int mode, int shift)
        {
            var response = SendRequest<ICustomResponse>(new ICustomRequest
            {
                Symbol = symbol,
                Timeframe = timeframe,
                Name = name,
                Mode = mode,
                Shift = shift,
                Params = new ArrayList(parameters),
                ParamsType = ICustomRequest.ParametersType.Int
            });
            return response?.Value ?? double.NaN;
        }

        public double iCustom(string symbol, int timeframe, string name, double[] parameters, int mode, int shift)
        {
            var response = SendRequest<ICustomResponse>(new ICustomRequest
            {
                Symbol = symbol,
                Timeframe = timeframe,
                Name = name,
                Mode = mode,
                Shift = shift,
                Params = new ArrayList(parameters),
                ParamsType = ICustomRequest.ParametersType.Double
            });
            return response?.Value ?? double.NaN;
        }

        public double iCustom(string symbol, int timeframe, string name, int mode, int shift)
        {
            var response = SendRequest<ICustomResponse>(new ICustomRequest
            {
                Symbol = symbol,
                Timeframe = timeframe,
                Name = name,
                Mode = mode,
                Shift = shift
            });
            return response?.Value ?? double.NaN;
        }

        public double iDeMarker(string symbol, int timeframe, int period, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, period, shift };
            return SendCommand<double>(MtCommandType.iDeMarker, commandParameters);
        }

        public double iEnvelopes(string symbol, int timeframe, int maPeriod, int maMethod, int maShift, int appliedPrice, double deviation, int mode, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, maPeriod, maMethod, maShift, appliedPrice, deviation, mode, shift };
            return SendCommand<double>(MtCommandType.iEnvelopes, commandParameters);
        }

        public double iEnvelopesOnArray(double[] array, int total, int maPeriod, int maMethod, int maShift, double deviation, int mode, int shift)
        {
            var arraySize = array?.Length ?? 0;
            var commandParameters = new ArrayList { arraySize };
            commandParameters.AddRange(array ?? new double[] {});
            commandParameters.Add(total);
            commandParameters.Add(maPeriod);
            commandParameters.Add(maMethod);
            commandParameters.Add(maShift);
            commandParameters.Add(deviation);
            commandParameters.Add(mode);
            commandParameters.Add(shift);

            return SendCommand<double>(MtCommandType.iEnvelopesOnArray, commandParameters);
        }

        public double iForce(string symbol, int timeframe, int period, int maMethod, int appliedPrice, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, period, maMethod, appliedPrice, shift };
            return SendCommand<double>(MtCommandType.iForce, commandParameters);
        }

        public double iFractals(string symbol, int timeframe, int mode, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, mode, shift };
            return SendCommand<double>(MtCommandType.iFractals, commandParameters);
        }

        public double iGator(string symbol, int timeframe, int jawPeriod, int jawShift, int teethPeriod, int teethShift, int lipsPeriod, int lipsShift, int maMethod, int appliedPrice, int mode, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, jawPeriod, jawShift, teethPeriod, teethShift, lipsPeriod, lipsShift, maMethod, appliedPrice, mode, shift };
            return SendCommand<double>(MtCommandType.iGator, commandParameters);
        }

        public double iIchimoku(string symbol, int timeframe, int tenkanSen, int kijunSen, int senkouSpanB, int mode, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, tenkanSen, kijunSen, senkouSpanB, mode, shift };
            return SendCommand<double>(MtCommandType.iIchimoku, commandParameters);
        }

        public double iBWMFI(string symbol, int timeframe, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, shift };
            return SendCommand<double>(MtCommandType.iBWMFI, commandParameters);
        }

        public double iMomentum(string symbol, int timeframe, int period, int appliedPrice, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, period, appliedPrice, shift };
            return SendCommand<double>(MtCommandType.iMomentum, commandParameters);
        }

        public double iMomentumOnArray(double[] array, int total, int period, int shift)
        {
            var arraySize = array?.Length ?? 0;
            var commandParameters = new ArrayList { arraySize };
            commandParameters.AddRange(array ?? new double[] {});
            commandParameters.Add(total);
            commandParameters.Add(period);
            commandParameters.Add(shift);

            return SendCommand<double>(MtCommandType.iMomentumOnArray, commandParameters);
        }

        public double iMFI(string symbol, int timeframe, int period, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, period, shift };
            return SendCommand<double>(MtCommandType.iMFI, commandParameters);
        }

        public double iMA(string symbol, int timeframe, int period, int maShift, int maMethod, int appliedPrice, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, period, maShift, maMethod, appliedPrice, shift };
            return SendCommand<double>(MtCommandType.iMA, commandParameters);
        }

        public double iMAOnArray(double[] array, int total, int period, int maShift, int maMethod, int shift)
        {
            var arraySize = array?.Length ?? 0;
            var commandParameters = new ArrayList { arraySize };
            commandParameters.AddRange(array ?? new double[] {});
            commandParameters.Add(total);
            commandParameters.Add(period);
            commandParameters.Add(maShift);
            commandParameters.Add(maMethod);
            commandParameters.Add(shift);

            return SendCommand<double>(MtCommandType.iMAOnArray, commandParameters);
        }

        public double iOsMA(string symbol, int timeframe, int fastEmaPeriod, int slowEmaPeriod, int signalPeriod, int appliedPrice, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, fastEmaPeriod, slowEmaPeriod, signalPeriod, appliedPrice, shift };
            return SendCommand<double>(MtCommandType.iOsMA, commandParameters);
        }

        public double iMACD(string symbol, int timeframe, int fastEmaPeriod, int slowEmaPeriod, int signalPeriod, int appliedPrice, int mode, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, fastEmaPeriod, slowEmaPeriod, signalPeriod, appliedPrice, mode, shift };
            return SendCommand<double>(MtCommandType.iMACD, commandParameters);
        }

        public double iOBV(string symbol, int timeframe, int appliedPrice, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, appliedPrice, shift };
            return SendCommand<double>(MtCommandType.iOBV, commandParameters);
        }

        public double iSAR(string symbol, int timeframe, double step, double maximum, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, step, maximum, shift };
            return SendCommand<double>(MtCommandType.iSAR, commandParameters);
        }

        public double iRSI( string symbol, int timeframe, int period, int appliedPrice, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, period, appliedPrice, shift };
            return SendCommand<double>(MtCommandType.iRSI, commandParameters);
        }

        public double iRSIOnArray(double[] array, int total, int period, int shift)
        {
            var arraySize = array?.Length ?? 0;
            var commandParameters = new ArrayList { arraySize };
            commandParameters.AddRange(array ?? new double[] {});
            commandParameters.Add(total);
            commandParameters.Add(period);
            commandParameters.Add(shift);

            return SendCommand<double>(MtCommandType.iMomentumOnArray, commandParameters);
        }

        public double iRVI(string symbol, int timeframe, int period, int mode, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, period, mode, shift };
            return SendCommand<double>(MtCommandType.iRVI, commandParameters);
        }

        public double iStdDev(string symbol, int timeframe, int maPeriod, int maShift, int maMethod, int appliedPrice, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, maPeriod, maShift, maMethod, appliedPrice, shift };
            return SendCommand<double>(MtCommandType.iStdDev, commandParameters);
        }

        public double iStdDevOnArray(double[] array, int total, int maPeriod, int maShift, int maMethod, int shift)
        {
            var arraySize = array?.Length ?? 0;
            var commandParameters = new ArrayList { arraySize };
            commandParameters.AddRange(array ?? new double[] {});
            commandParameters.Add(total);
            commandParameters.Add(maPeriod);
            commandParameters.Add(maShift);
            commandParameters.Add(maMethod);
            commandParameters.Add(shift);

            return SendCommand<double>(MtCommandType.iStdDevOnArray, commandParameters);
        }

        public double iStochastic(string symbol, int timeframe, int pKperiod, int pDperiod, int slowing, int method, int priceField, int mode, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, pKperiod, pDperiod, slowing, method, priceField, mode, shift };
            return SendCommand<double>(MtCommandType.iStochastic, commandParameters);
        }

        public double iWPR(string symbol, int timeframe, int period, int shift)
        {
            var commandParameters = new ArrayList { symbol, timeframe, period, shift };
            return SendCommand<double>(MtCommandType.iWPR, commandParameters);
        }
        #endregion

        #region Timeseries access
        public int iBars(string symbol, ChartPeriod timeframe)
        {
            var commandParameters = new ArrayList { symbol, (int)timeframe };
            return SendCommand<int>(MtCommandType.iBars, commandParameters);
        }

        public int iBarShift(string symbol, ChartPeriod timeframe, DateTime time, bool exact)
        {
            var commandParameters = new ArrayList { symbol, (int)timeframe, MtApiTimeConverter.ConvertToMtTime(time), exact };
            return SendCommand<int>(MtCommandType.iBarShift, commandParameters);
        }

        public double iClose(string symbol, ChartPeriod timeframe, int shift)
        {
            var commandParameters = new ArrayList { symbol, (int)timeframe, shift };
            return SendCommand<double>(MtCommandType.iClose, commandParameters);
        }

        public double iHigh(string symbol, ChartPeriod timeframe, int shift)
        {
            var commandParameters = new ArrayList { symbol, (int)timeframe, shift };
            return SendCommand<double>(MtCommandType.iHigh, commandParameters);
        }

        public int iHighest(string symbol, ChartPeriod timeframe, SeriesIdentifier type, int count, int start)
        {
            var commandParameters = new ArrayList { symbol, (int)timeframe, (int)type, count, start };
            return SendCommand<int>(MtCommandType.iHighest, commandParameters);
        }

        public double iLow(string symbol, ChartPeriod timeframe, int shift)
        {
            var commandParameters = new ArrayList { symbol, (int)timeframe, shift };
            return SendCommand<double>(MtCommandType.iLow, commandParameters);
        }

        public int iLowest(string symbol, ChartPeriod timeframe, SeriesIdentifier type, int count, int start)
        {
            var commandParameters = new ArrayList { symbol, (int)timeframe, (int)type, count, start };
            return SendCommand<int>(MtCommandType.iLowest, commandParameters);
        }

        public double iOpen(string symbol, ChartPeriod timeframe, int shift)
        {
            var commandParameters = new ArrayList { symbol, (int)timeframe, shift };
            return SendCommand<double>(MtCommandType.iOpen, commandParameters);
        }

        public DateTime iTime(string symbol, ChartPeriod timeframe, int shift)
        {
            var commandParameters = new ArrayList { symbol, (int)timeframe, shift };
            var commandResponse = SendCommand<int>(MtCommandType.iTime, commandParameters);
            return MtApiTimeConverter.ConvertFromMtTime(commandResponse);
        }

        public double iVolume(string symbol, ChartPeriod timeframe, int shift)
        {
            var commandParameters = new ArrayList { symbol, (int)timeframe, shift };
            return SendCommand<double>(MtCommandType.iVolume, commandParameters);
        }

        //public double[] iCloseArray(string symbol, ChartPeriod timeframe, int shift, int valueCount)
        //{
        //    int doubleArraySendLimit = DoubleArrayLimit;
        //    int limitCount = valueCount / doubleArraySendLimit;
        //    int valueCountModulo = valueCount - limitCount * doubleArraySendLimit;

        //    var resultArray = new double[valueCount];
        //    for (int i = 0; i < limitCount; i++)
        //    {
        //        var commandParameters = new ArrayList { symbol, (int)timeframe, i * doubleArraySendLimit, doubleArraySendLimit };
        //        var result = SendCommand<double[]>(MtCommandType.iCloseArray, commandParameters);
        //        if (result != null)
        //            Array.Copy(result, 0, resultArray, i * doubleArraySendLimit, doubleArraySendLimit);
        //    }

        //    if (valueCountModulo > 0)
        //    {
        //        var commandParameters = new ArrayList { symbol, (int)timeframe, limitCount * doubleArraySendLimit, valueCountModulo };
        //        var result = SendCommand<double[]>(MtCommandType.iCloseArray, commandParameters);
        //        if (result != null)
        //            Array.Copy(result, 0, resultArray, limitCount * doubleArraySendLimit, valueCountModulo);
        //    }
        //    return resultArray;

        //    var commandParameters = new ArrayList { symbol, (int)timeframe, shift, valueCount };
        //    return SendCommand<double[]>(MtCommandType.iCloseArray, commandParameters);
        //}

        public double[] iCloseArray(string symbol, ChartPeriod timeframe)
        {
            var commandParameters = new ArrayList { symbol, (int)timeframe };
            return SendCommand<double[]>(MtCommandType.iCloseArray, commandParameters);
        }

        public double[] iHighArray(string symbol, ChartPeriod timeframe)
        {
            var commandParameters = new ArrayList { symbol, (int)timeframe };
            return SendCommand<double[]>(MtCommandType.iHighArray, commandParameters);
        }

        public double[] iLowArray(string symbol, ChartPeriod timeframe)
        {
            var commandParameters = new ArrayList { symbol, (int)timeframe };
            return SendCommand<double[]>(MtCommandType.iLowArray, commandParameters);
        }

        public double[] iOpenArray(string symbol, ChartPeriod timeframe)
        {
            var commandParameters = new ArrayList { symbol, (int)timeframe };
            return SendCommand<double[]>(MtCommandType.iOpenArray, commandParameters);
        }

        public double[] iVolumeArray(string symbol, ChartPeriod timeframe)
        {
            var commandParameters = new ArrayList { symbol, (int)timeframe };
            return SendCommand<double[]>(MtCommandType.iVolumeArray, commandParameters);
        }

        public DateTime[] iTimeArray(string symbol, ChartPeriod timeframe)
        {
            DateTime[] result = null;

            var commandParameters = new ArrayList { symbol, (int)timeframe };
            
            var response = SendCommand<int[]>(MtCommandType.iTimeArray, commandParameters);

            if (response != null)
            {
                result = new DateTime[response.Length];

                for(var i = 0; i < response.Length; i++)
                {
                    result[i] = MtApiTimeConverter.ConvertFromMtTime(response[i]);
                }
            }

            return result;
        }

        public bool RefreshRates()
        {
            return SendCommand<bool>(MtCommandType.RefreshRates, null);
        }

        public List<MqlRates> CopyRates(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count)
        {
            var response = SendRequest<CopyRatesResponse>(new CopyRates1Request
            {
                SymbolName = symbolName,
                Timeframe = timeframe,
                StartPos = startPos,
                Count = count
            });
            return response?.Rates;
        }

        public List<MqlRates> CopyRates(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, int count)
        {
            var response = SendRequest<CopyRatesResponse>(new CopyRates2Request
            {
                SymbolName = symbolName,
                Timeframe = timeframe,
                StartTime = MtApiTimeConverter.ConvertToMtTime(startTime),
                Count = count
            });
            return response?.Rates;
        }

        public List<MqlRates> CopyRates(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, DateTime stopTime)
        {
            var response = SendRequest<CopyRatesResponse>(new CopyRates3Request
            {
                SymbolName = symbolName,
                Timeframe = timeframe,
                StartTime = MtApiTimeConverter.ConvertToMtTime(startTime),
                StopTime = MtApiTimeConverter.ConvertToMtTime(stopTime)
            });
            return response?.Rates;
        }

        #endregion

        #region Checkup
        public string TerminalInfoString(ENUM_TERMINAL_INFO_STRING propertyId)
        {
            var commandParameters = new ArrayList { (int)propertyId };
            return SendCommand<string>(MtCommandType.TerminalInfoString, commandParameters);
        }

        public string SymbolInfoString(string name, ENUM_SYMBOL_INFO_STRING propId)
        {
            var commandParameters = new ArrayList { name, (int)propId };
            return SendCommand<string>(MtCommandType.SymbolInfoString, commandParameters); ;
        }

        public MtSession SymbolInfoSession(string symbol, DayOfWeek dayOfWeek, uint index, SessionType type)
        {
            var responce = SendRequest<SessionResponse>(new SessionRequest { Symbol = symbol, DayOfWeek = dayOfWeek, SessionIndex = (int)index, SessionType = type });
            return responce != null ? responce.Session : null;
        }
        #endregion

        #region Private Methods
        private void Connect(string host, int port)
        {
            UpdateConnectionState(MtConnectionState.Connecting, $"Connecting to {host}:{port}");
            try
            {
                lock (_client)
                {
                    _client.Open(host, port);
                    _client.Connect();
                }
            }
            catch (Exception e)
            {
                UpdateConnectionState(MtConnectionState.Failed, $"Failed connection to {host}:{port}. {e.Message}");
                return;
            }
            UpdateConnectionState(MtConnectionState.Connected, $"Connected  to  {host}:{port}");
            OnConnected();
        }

        private void Connect(int port)
        {
            UpdateConnectionState(MtConnectionState.Connecting, $"Connecting to 'localhost':{port}");
            try
            {
                lock (_client)
                {
                    _client.Open(port);
                    _client.Connect();
                }
            }
            catch (Exception e)
            {
                UpdateConnectionState(MtConnectionState.Failed, $"Failed connection  to 'localhost':{port}. {e.Message}");
                return;
            }
            UpdateConnectionState(MtConnectionState.Connected, $"Connected to 'localhost':{port}");
            OnConnected();
        }

        private void OnConnected()
        {
            _isBacktestingMode = IsTesting();
            if (_isBacktestingMode)
            {
                BacktestingReady();
            }
        }

        private void Disconnect()
        {
            lock (_client)
            {
                _client.Disconnect();
                _client.Close();
            }

            UpdateConnectionState(MtConnectionState.Disconnected, "Disconnected");
        }

        private void UpdateConnectionState(MtConnectionState state, string message)
        {
            var changed = false;
            lock (_locker)
            {
                if (_connectionState != state)
                {
                    _connectionState = state;
                    changed = true;
                }
            }

            if (changed)
            {
                ConnectionStateChanged.FireEventAsync(this, new MtConnectionEventArgs(state, message));
            }
        }

        private T SendCommand<T>(MtCommandType commandType, ArrayList commandParameters)
        {
            MtResponse response;
            try
            {
                lock (_client)
                {
                    response = _client.SendCommand((int)commandType, commandParameters);
                }
            }
            catch (CommunicationException ex)
            {
                throw new MtConnectionException(ex.Message, ex);
            }

            var responseValue = response.GetValue();
            return responseValue != null ? (T)responseValue : default(T);
        }

        private T SendRequest<T>(RequestBase request) where T : ResponseBase, new()
        {
            if (request == null)
                return default(T);

            var serializer = JsonConvert.SerializeObject(request, Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });
            var commandParameters = new ArrayList { serializer };

            MtResponseString res;
            try
            {
                lock (_client)
                {
                    res = (MtResponseString)_client.SendCommand((int)MtCommandType.MtRequest, commandParameters);
                }
            }
            catch (CommunicationException ex)
            {
                throw new MtConnectionException(ex.Message, ex);
            }

            if (res == null)
            {
                throw new MtExecutionException(MtErrorCode.MtApiCustomError, "Response from MetaTrader is null");
            }

            var response = JsonConvert.DeserializeObject<T>(res.Value);
            if (response.ErrorCode != 0)
            {
                throw new MtExecutionException((MtErrorCode)response.ErrorCode, response.ErrorMessage);
            }

            return response;
        }

        private void _client_QuoteUpdated(MTApiService.MtQuote quote)
        {
            if (quote != null)
            {
                if (_isBacktestingMode)
                {
                    QuoteUpdated?.Invoke(this, quote.Instrument, quote.Bid, quote.Ask);
                }
                else
                {
                    QuoteUpdated.FireEventAsync(this, quote.Instrument, quote.Bid, quote.Ask);
                }
            }
        }

        private void _client_ServerDisconnected(object sender, EventArgs e)
        {
            UpdateConnectionState(MtConnectionState.Disconnected, "MtApi is disconnected");
        }

        private void _client_ServerFailed(object sender, EventArgs e)
        {
            UpdateConnectionState(MtConnectionState.Failed, "Failed connection with MtApi");
        }

        private void _client_QuoteRemoved(MTApiService.MtQuote quote)
        {
            QuoteRemoved.FireEventAsync(this, new MtQuoteEventArgs(quote.Convert()));
        }

        private void _client_QuoteAdded(MTApiService.MtQuote quote)
        {
            QuoteAdded.FireEventAsync(this, new MtQuoteEventArgs(quote.Convert()));
        }

        private void _client_MtEventReceived(object sender, MtEventArgs e)
        {
            var eventType = (MtEventTypes) e.Event.EventType;

            switch(eventType)
            {
                case MtEventTypes.LastTimeBar:
                    {
                        FireOnLastTimeBar(JsonConvert.DeserializeObject<MtTimeBar>(e.Event.Payload));
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void FireOnLastTimeBar(MtTimeBar timeBar)
        {
            OnLastTimeBar.FireEventAsync(this, new TimeBarArgs(timeBar));
        }

        private void BacktestingReady()
        {
            SendCommand<object>(MtCommandType.BacktestingReady, null);
        }

        #endregion

        #region Events

        public event MtApiQuoteHandler QuoteUpdated;
        public event EventHandler<MtQuoteEventArgs> QuoteAdded;
        public event EventHandler<MtQuoteEventArgs> QuoteRemoved;
        public event EventHandler<MtConnectionEventArgs> ConnectionStateChanged;
        public event EventHandler<TimeBarArgs> OnLastTimeBar;

        #endregion
    }
}
