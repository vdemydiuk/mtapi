// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using System.Linq;
using MTApiService;
using System.Collections;
using System.ServiceModel;
using MtApi5.Requests;
using Newtonsoft.Json;
using System.Threading.Tasks;
using MtApi5.Events;

namespace MtApi5
{
    public class MtApi5Client
    {
        #region MT Constants
        public const int SYMBOL_EXPIRATION_GTC              = 1;
        public const int SYMBOL_EXPIRATION_DAY              = 2;
        public const int SYMBOL_EXPIRATION_SPECIFIED        = 4;
        public const int SYMBOL_EXPIRATION_SPECIFIED_DAY    = 8;

        public const int SYMBOL_FILLING_ALL_OR_NONE = 1;
        public const int SYMBOL_CANCEL_REMAIND = 1;
        public const int SYMBOL_RETURN_REMAIND = 1;

        #endregion

        private const char ParamSeparator = ';';
        private const string LogProfileName = "MtApi5Client";

        public delegate void QuoteHandler(object sender, string symbol, double bid, double ask);


        #region Private Fields
        private static readonly MtLog Log = LogConfigurator.GetLogger(typeof(MtApi5Client));

        private MtClient _client;
        private readonly object _locker = new object();
        private volatile bool _isBacktestingMode;
        private Mt5ConnectionState _connectionState = Mt5ConnectionState.Disconnected;
        private int _executorHandle;
        #endregion

        #region Public Methods
        public MtApi5Client()
        {
            LogConfigurator.Setup(LogProfileName);
        }

        ///<summary>
        ///Connect with MetaTrader API. Async method.
        ///</summary>
        ///<param name="host">Address of MetaTrader host (ex. 192.168.1.2)</param>
        ///<param name="port">Port of host connection (default 8222) </param>
        public void BeginConnect(string host, int port)
        {
            Task.Factory.StartNew(() => Connect(host, port));
        }

        ///<summary>
        ///Connect with MetaTrader API. Async method.
        ///</summary>
        ///<param name="port">Port of host connection (default 8222) </param>
        public void BeginConnect(int port)
        {
            Task.Factory.StartNew(() => Connect(port));
        }

        ///<summary>
        ///Disconnect from MetaTrader API. Async method.
        ///</summary>
        public void BeginDisconnect()
        {
            Task.Factory.StartNew(() => Disconnect(false));
        }

        ///<summary>
        ///Load quotes connected into MetaTrader API.
        ///</summary>
        public IEnumerable<Mt5Quote> GetQuotes()
        {
            var client = Client;
            var quotes = client?.GetQuotes();
            return quotes?.Select(q => new Mt5Quote(q));
        }

        ///<summary>
        ///Checks if the Expert Advisor runs in the testing mode..
        ///</summary>
        public bool IsTesting()
        {
            return SendCommand<bool>(Mt5CommandType.IsTesting, null);
        }

        #region Trading functions

        ///<summary>
        ///Sends trade requests to a server
        ///</summary>
        ///<param name="request">Reference to a object of MqlTradeRequest type describing the trade activity of the client.</param>
        ///<param name="result">Reference to a object of MqlTradeResult type describing the result of trade operation in case of a successful completion (if true is returned).</param>
        /// <returns>
        /// In case of a successful basic check of structures (index checking) returns true.
        /// However, this is not a sign of successful execution of a trade operation.
        /// For a more detailed description of the function execution result, analyze the fields of result structure.
        /// </returns>
        public bool OrderSend(MqlTradeRequest request, out MqlTradeResult result)
        {
            Log.Debug($"OrderSend: request = {request}");

            if (request == null)
            {
                Log.Warn("OrderSend: request is not defined!");
                result = null;
                return false;
            }

            var response = SendRequest<OrderSendResult>(new OrderSendRequest
            {
                TradeRequest = request
            });

            result = response?.TradeResult;
            return response != null && response.RetVal;
        }

        ///<summary>
        ///The function calculates the margin required for the specified order type, on the current account
        ///, in the current market environment not taking into account current pending orders and open positions
        ///. It allows the evaluation of margin for the trade operation planned. The value is returned in the account currency.
        ///</summary>
        ///<param name="action">The order type, can be one of the values of the ENUM_ORDER_TYPE enumeration.</param>
        ///<param name="symbol">Symbol name.</param>
        ///<param name="volume">Volume of the trade operation.</param>
        ///<param name="price">Open price.</param>
        ///<param name="margin">The variable, to which the value of the required margin will be written in case the function is successfully executed
        ///. The calculation is performed as if there were no pending orders and open positions on the current account
        ///. The margin value depends on many factors, and can differ in different market environments.</param>
        /// <returns>
        /// The function returns true in case of success; otherwise it returns false.
        /// </returns>
        public bool OrderCalcMargin(ENUM_ORDER_TYPE action, string symbol, double volume, double price, out double margin)
        {
            var commandParameters = new ArrayList { (int)action, symbol, volume, price };

            var strResult = SendCommand<string>(Mt5CommandType.OrderCalcMargin, commandParameters);

            return strResult.ParseResult(ParamSeparator, out margin);
        }

        ///<summary>
        ///The function calculates the profit for the current account,
        ///in the current market conditions, based on the parameters passed.
        ///The function is used for pre-evaluation of the result of a trade operation. 
        ///The value is returned in the account currency.
        ///</summary>
        ///<param name="action">Type of the order, can be one of the two values of the ENUM_ORDER_TYPE enumeration: ORDER_TYPE_BUY or ORDER_TYPE_SELL.</param>
        ///<param name="symbol">Symbol name.</param>
        ///<param name="volume">Volume of the trade operation.</param>
        ///<param name="priceOpen">Open price.</param>
        ///<param name="priceClose">Close price.</param>
        ///<param name="profit">The variable, to which the calculated value of the profit will be written in case the function is successfully executed. 
        ///The estimated profit value depends on many factors, and can differ in different market environments.</param>
        /// <returns>
        /// The function returns true in case of success; otherwise it returns false. If an invalid order type is specified, the function will return false.
        /// </returns>
        public bool OrderCalcProfit(ENUM_ORDER_TYPE action, string symbol, double volume, double priceOpen, double priceClose, out double profit)
        {
            var commandParameters = new ArrayList { (int)action, symbol, volume, priceOpen, priceClose };

            var strResult = SendCommand<string>(Mt5CommandType.OrderCalcProfit, commandParameters);

            return strResult.ParseResult(ParamSeparator, out profit);
        }

        ///<summary>
        ///The OrderCheck() function checks if there are enough money to execute a required trade operation. 
        ///The check results are placed to the fields of the MqlTradeCheckResult structure.
        ///</summary>
        ///<param name="request">Reference to a object of MqlTradeRequest type describing the trade activity of the client.</param>
        ///<param name="result"> Reference to the object of the MqlTradeCheckResult type, to which the check result will be placed.</param>
        /// <returns>
        /// If funds are not enough for the operation, or parameters are filled out incorrectly, the function returns false. 
        /// In case of a successful basic check of structures (check of pointers), it returns true. 
        /// However, this is not an indication that the requested trade operation is sure to be successfully executed. 
        /// For a more detailed description of the function execution result, analyze the fields of the result structure.
        /// </returns>
        public bool OrderCheck(MqlTradeRequest request, out MqlTradeCheckResult result)
        {
            Log.Debug($"OrderCheck: request = {request}");

            if (request == null)
            {
                Log.Warn("OrderCheck: request is not defined!");
                result = null;
                return false;
            }

            var response = SendRequest<OrderCheckResult>(new OrderCheckRequest
            {
                TradeRequest = request
            });

            result = response?.TradeCheckResult;
            return response != null && response.RetVal;
        }

        ///<summary>
        ///Returns the number of open positions.
        ///</summary>
        public int PositionsTotal()
        {
            return SendCommand<int>(Mt5CommandType.PositionsTotal, null);
        }

        ///<summary>
        ///Returns the symbol corresponding to the open position and automatically selects the position for further working with it using functions PositionGetDouble, PositionGetInteger, PositionGetString.
        ///</summary>
        ///<param name="index">Number of the position in the list of open positions.</param>
        public string PositionGetSymbol(int index)
        {
            var commandParameters = new ArrayList { index };

            return SendCommand<string>(Mt5CommandType.PositionGetSymbol, commandParameters);
        }

        ///<summary>
        ///Chooses an open position for further working with it. Returns true if the function is successfully completed. Returns false in case of failure.
        ///</summary>
        ///<param name="symbol">Name of the financial security.</param>
        public bool PositionSelect(string symbol)
        {
            var commandParameters = new ArrayList { symbol };

            return SendCommand<bool>(Mt5CommandType.PositionSelect, commandParameters);
        }

        ///<summary>
        ///Selects an open position to work with based on the ticket number specified in the position. If successful, returns true. Returns false if the function failed.
        ///</summary>
        ///<param name="ticket">Position ticket.</param>
        public bool PositionSelectByTicket(ulong ticket)
        {
            var commandParameters = new ArrayList { ticket };

            return SendCommand<bool>(Mt5CommandType.PositionSelectByTicket, commandParameters);
        }

        ///<summary>
        ///The function returns the requested property of an open position, pre-selected using PositionGetSymbol or PositionSelect.
        ///</summary>
        ///<param name="propertyId">Identifier of a position property.</param>
        public double PositionGetDouble(ENUM_POSITION_PROPERTY_DOUBLE propertyId)
        {
            var commandParameters = new ArrayList { (int)propertyId };

            return SendCommand<double>(Mt5CommandType.PositionGetDouble, commandParameters);
        }

        ///<summary>
        ///The function returns the requested property of an open position, pre-selected using PositionGetSymbol or PositionSelect.
        ///</summary>
        ///<param name="propertyId">Identifier of a position property.</param>
        public long PositionGetInteger(ENUM_POSITION_PROPERTY_INTEGER propertyId)
        {
            var commandParameters = new ArrayList { (int)propertyId };

            return SendCommand<long>(Mt5CommandType.PositionGetInteger, commandParameters);
        }

        ///<summary>
        ///The function returns the requested property of an open position, pre-selected using PositionGetSymbol or PositionSelect.
        ///</summary>
        ///<param name="propertyId">Identifier of a position property.</param>
        public string PositionGetString(ENUM_POSITION_PROPERTY_STRING propertyId)
        {
            var commandParameters = new ArrayList { (int)propertyId };

            return SendCommand<string>(Mt5CommandType.PositionGetString, commandParameters);
        }

        ///<summary>
        ///The function returns the ticket of a position with the specified index in the list of open positions and automatically selects the position to work with using functions PositionGetDouble, PositionGetInteger, PositionGetString.
        ///</summary>
        ///<param name="index">Identifier of a position property.</param>
        public ulong PositionGetTicket(int index)
        {
            var commandParameters = new ArrayList { index };

            return SendCommand<ulong>(Mt5CommandType.PositionGetTicket, commandParameters);
        }

        ///<summary>
        ///Returns the number of current orders.
        ///</summary>
        public int OrdersTotal()
        {
            return SendCommand<int>(Mt5CommandType.OrdersTotal, null);
        }

        ///<summary>
        ///Returns the number of current orders.
        ///</summary>
        ///<param name="index">Number of an order in the list of current orders.</param>
        public ulong OrderGetTicket(int index)
        {
            var commandParameters = new ArrayList { index };

            return SendCommand<ulong>(Mt5CommandType.OrderGetTicket, commandParameters);
        }

        ///<summary>
        ///Selects an order to work with. Returns true if the function has been successfully completed. 
        ///</summary>
        ///<param name="ticket">Order ticket.</param>
        public bool OrderSelect(ulong ticket)
        {
            var commandParameters = new ArrayList { ticket };

            return SendCommand<bool>(Mt5CommandType.OrderSelect, commandParameters);
        }

        ///<summary>
        ///Returns the requested property of an order, pre-selected using OrderGetTicket or OrderSelect.
        ///</summary>
        ///<param name="propertyId"> Identifier of the order property.</param>
        public double OrderGetDouble(ENUM_ORDER_PROPERTY_DOUBLE propertyId)
        {
            var commandParameters = new ArrayList { (int)propertyId };

            return SendCommand<double>(Mt5CommandType.OrderGetDouble, commandParameters);
        }

        ///<summary>
        ///Returns the requested property of an order, pre-selected using OrderGetTicket or OrderSelect.
        ///</summary>
        ///<param name="propertyId"> Identifier of the order property.</param>
        public long OrderGetInteger(ENUM_ORDER_PROPERTY_INTEGER propertyId)
        {
            var commandParameters = new ArrayList { (int)propertyId };

            return SendCommand<long>(Mt5CommandType.OrderGetInteger, commandParameters);
        }

        ///<summary>
        ///Returns the requested property of an order, pre-selected using OrderGetTicket or OrderSelect.
        ///</summary>
        ///<param name="propertyId"> Identifier of the order property.</param>
        public string OrderGetString(ENUM_ORDER_PROPERTY_STRING propertyId)
        {
            var commandParameters = new ArrayList { (int)propertyId };

            return SendCommand<string>(Mt5CommandType.OrderGetString, commandParameters);
        }

        ///<summary>
        ///Retrieves the history of deals and orders for the specified period of server time.
        ///</summary>
        ///<param name="fromDate">Start date of the request.</param>
        ///<param name="toDate">End date of the request.</param>
        public bool HistorySelect(DateTime fromDate, DateTime toDate)
        {
            var commandParameters = new ArrayList { Mt5TimeConverter.ConvertToMtTime(fromDate), Mt5TimeConverter.ConvertToMtTime(toDate) };

            return SendCommand<bool>(Mt5CommandType.HistorySelect, commandParameters);
        }

        ///<summary>
        ///Retrieves the history of deals and orders having the specified position identifier.
        ///</summary>
        ///<param name="positionId">Position identifier that is set to every executed order and every deal.</param>
        public bool HistorySelectByPosition(long positionId)
        {
            var commandParameters = new ArrayList { positionId };

            return SendCommand<bool>(Mt5CommandType.HistorySelectByPosition, commandParameters);
        }

        ///<summary>
        ///Selects an order from the history for further calling it through appropriate functions. 
        ///</summary>
        ///<param name="ticket">Order ticket.</param>
        public bool HistoryOrderSelect(ulong ticket)
        {
            var commandParameters = new ArrayList { ticket };

            return SendCommand<bool>(Mt5CommandType.HistoryOrderSelect, commandParameters);
        }

        ///<summary>
        ///Returns the number of orders in the history.
        ///</summary>
        public int HistoryOrdersTotal()
        {
            return SendCommand<int>(Mt5CommandType.HistoryOrdersTotal, null);
        }

        ///<summary>
        ///Return the ticket of a corresponding order in the history.
        ///</summary>
        ///<param name="index">Number of the order in the list of orders.</param>
        public ulong HistoryOrderGetTicket(int index)
        {
            var commandParameters = new ArrayList { index };

            return SendCommand<ulong>(Mt5CommandType.HistoryOrderGetTicket, commandParameters);
        }

        ///<summary>
        ///Returns the requested order property.
        ///</summary>
        ///<param name="ticketNumber">Order ticket.</param>
        ///<param name="propertyId">Identifier of the order property.</param>
        public double HistoryOrderGetDouble(ulong ticketNumber, ENUM_ORDER_PROPERTY_DOUBLE propertyId)
        {
            var commandParameters = new ArrayList { ticketNumber, (int)propertyId };

            return SendCommand<double>(Mt5CommandType.HistoryOrderGetDouble, commandParameters);
        }

        ///<summary>
        ///Returns the requested property of an order. 
        ///</summary>
        ///<param name="ticketNumber">Order ticket.</param>
        ///<param name="propertyId">Identifier of the order property.</param>
        public long HistoryOrderGetInteger(ulong ticketNumber, ENUM_ORDER_PROPERTY_INTEGER propertyId)
        {
            var commandParameters = new ArrayList { ticketNumber, (int)propertyId };

            return SendCommand<long>(Mt5CommandType.HistoryOrderGetInteger, commandParameters);
        }

        ///<summary>
        ///Returns the requested property of an order. 
        ///</summary>
        ///<param name="ticketNumber">Order ticket.</param>
        ///<param name="propertyId">Identifier of the order property.</param>
        public string HistoryOrderGetString(ulong ticketNumber, ENUM_ORDER_PROPERTY_STRING propertyId)
        {
            var commandParameters = new ArrayList { ticketNumber, (int)propertyId };

            return SendCommand<string>(Mt5CommandType.HistoryOrderGetString, commandParameters);
        }

        ///<summary>
        ///Selects a deal in the history for further calling it through appropriate functions. 
        ///</summary>
        ///<param name="ticket">Deal ticket.</param>
        public bool HistoryDealSelect(ulong ticket)
        {
            var commandParameters = new ArrayList { ticket };

            return SendCommand<bool>(Mt5CommandType.HistoryDealSelect, commandParameters);
        }

        ///<summary>
        ///Returns the number of deal in history.
        ///</summary>
        public int HistoryDealsTotal()
        {
            return SendCommand<int>(Mt5CommandType.HistoryDealsTotal, null);
        }

        ///<summary>
        ///The function selects a deal for further processing and returns the deal ticket in history. 
        ///</summary>
        ///<param name="index">Number of a deal in the list of deals.</param>
        public ulong HistoryDealGetTicket(int index)
        {
            var commandParameters = new ArrayList { index };

            return SendCommand<ulong>(Mt5CommandType.HistoryDealGetTicket, commandParameters);
        }

        ///<summary>
        ///Returns the requested property of a deal. 
        ///</summary>
        ///<param name="ticketNumber">Deal ticket.</param>
        ///<param name="propertyId"> Identifier of a deal property.</param>
        public double HistoryDealGetDouble(ulong ticketNumber, ENUM_DEAL_PROPERTY_DOUBLE propertyId)
        {
            var commandParameters = new ArrayList { ticketNumber, (int)propertyId };

            return SendCommand<double>(Mt5CommandType.HistoryDealGetDouble, commandParameters);
        }

        ///<summary>
        ///Returns the requested property of a deal. 
        ///</summary>
        ///<param name="ticketNumber">Deal ticket.</param>
        ///<param name="propertyId"> Identifier of a deal property.</param>
        public long HistoryDealGetInteger(ulong ticketNumber, ENUM_DEAL_PROPERTY_INTEGER propertyId)
        {
            var commandParameters = new ArrayList { ticketNumber, (int)propertyId };

            return SendCommand<long>(Mt5CommandType.HistoryDealGetInteger, commandParameters);
        }

        ///<summary>
        ///Returns the requested property of a deal. 
        ///</summary>
        ///<param name="ticketNumber">Deal ticket.</param>
        ///<param name="propertyId"> Identifier of a deal property.</param>
        public string HistoryDealGetString(ulong ticketNumber, ENUM_DEAL_PROPERTY_STRING propertyId)
        {
            var commandParameters = new ArrayList { ticketNumber, (int)propertyId };

            return SendCommand<string>(Mt5CommandType.HistoryDealGetString, commandParameters);
        }

        ///<summary>
        ///Close all open positions. 
        ///</summary>
        public bool OrderCloseAll()
        {
            return SendCommand<bool>(Mt5CommandType.OrderCloseAll, null);
        }

        ///<summary>
        ///Closes a position with the specified ticket.
        ///</summary>
        ///<param name="ticket">Ticket of the closed position.</param>
        ///<param name="deviation">Maximal deviation from the current price (in points).</param>
        public bool PositionClose(ulong ticket, ulong deviation = ulong.MaxValue)
        {
            var commandParameters = new ArrayList { ticket, deviation };

            return SendCommand<bool>(Mt5CommandType.PositionClose, commandParameters);
        }

        /// <summary>
        /// Opens a position with the specified parameters.
        /// </summary>
        /// <param name="symbol">symbol</param>
        /// <param name="orderType">order type to open position </param>
        /// <param name="volume">position volume</param>
        /// <param name="price">execution price</param>
        /// <param name="sl">Stop Loss price</param>
        /// <param name="tp">Take Profit price</param>
        /// <param name="comment">comment</param>
        /// <returns>true - successful check of the basic structures, otherwise - false.</returns>
        public bool PositionOpen(string symbol, ENUM_ORDER_TYPE orderType, double volume, double price, double sl, double tp, string comment = "")
        {
            var commandParameters = new ArrayList { symbol, (int) orderType, volume, price, sl, tp, comment };

            return SendCommand<bool>(Mt5CommandType.PositionOpen, commandParameters);
        }

        /// <summary>
        /// Opens a position with the specified parameters.
        /// </summary>
        /// <param name="symbol">symbol</param>
        /// <param name="orderType">order type to open position </param>
        /// <param name="volume">position volume</param>
        /// <param name="price">execution price</param>
        /// <param name="sl">Stop Loss price</param>
        /// <param name="tp">Take Profit price</param>
        /// <param name="comment">comment</param>
        /// <param name="result">output result</param>
        /// <returns>true - successful check of the basic structures, otherwise - false.</returns>
        public bool PositionOpen(string symbol, ENUM_ORDER_TYPE orderType, double volume, double price, double sl, double tp, string comment , out MqlTradeResult result)
        {
            Log.Debug($"PositionOpen: symbol = {symbol}, orderType = {orderType}, volume = {volume}, price = {price}, sl = {sl}, tp = {tp}, comment = {comment}");

            var response = SendRequest<OrderSendResult>(new PositionOpenRequest
            {
                Symbol = symbol,
                OrderType = orderType,
                Volume = volume,
                Price = price,
                Sl = sl,
                Tp = tp,
                Comment = comment
            });

            result = response?.TradeResult;
            return response != null && response.RetVal;
        }
        #endregion

        #region Account Information functions

            ///<summary>
            ///Returns the value of the corresponding account property. 
            ///</summary>
            ///<param name="propertyId">Identifier of the property.</param>
            public double AccountInfoDouble(ENUM_ACCOUNT_INFO_DOUBLE propertyId)
        {
            var commandParameters = new ArrayList { (int)propertyId };

            return SendCommand<double>(Mt5CommandType.AccountInfoDouble, commandParameters);
        }

        ///<summary>
        ///Returns the value of the corresponding account property. 
        ///</summary>
        ///<param name="propertyId">Identifier of the property.</param>
        public long AccountInfoInteger(ENUM_ACCOUNT_INFO_INTEGER propertyId)
        {
            var commandParameters = new ArrayList { (int)propertyId };

            return SendCommand<long>(Mt5CommandType.AccountInfoInteger, commandParameters);
        }

        ///<summary>
        ///Returns the value of the corresponding account property. 
        ///</summary>
        ///<param name="propertyId">Identifier of the property.</param>
        public string AccountInfoString(ENUM_ACCOUNT_INFO_STRING propertyId)
        {
            var commandParameters = new ArrayList { (int)propertyId };

            return SendCommand<string>(Mt5CommandType.AccountInfoString, commandParameters);
        }
        #endregion

        #region Timeseries and Indicators Access
        ///<summary>
        ///Returns information about the state of historical data.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe"> Period.</param>
        ///<param name="propId">Identifier of the requested property, value of the ENUM_SERIES_INFO_INTEGER enumeration.</param>
        public long SeriesInfoInteger(string symbolName, ENUM_TIMEFRAMES timeframe, ENUM_SERIES_INFO_INTEGER propId)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, (int)propId };

            return SendCommand<long>(Mt5CommandType.SeriesInfoInteger, commandParameters);
        }

        ///<summary>
        ///Returns the number of bars count in the history for a specified symbol and period.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe"> Period.</param>
        public int Bars(string symbolName, ENUM_TIMEFRAMES timeframe)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe };

            return SendCommand<int>(Mt5CommandType.Bars, commandParameters);
        }

        ///<summary>
        ///Returns the number of bars count in the history for a specified symbol and period.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">Bar time corresponding to the first element.</param>
        ///<param name="stopTime">Bar time corresponding to the last element.</param>
        public int Bars(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, DateTime stopTime)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(startTime), Mt5TimeConverter.ConvertToMtTime(stopTime) };

            return SendCommand<int>(Mt5CommandType.Bars2, commandParameters);
        }

        ///<summary>
        ///Returns the number of calculated data for the specified indicator.
        ///</summary>
        ///<param name="indicatorHandle">The indicator handle, returned by the corresponding indicator function.</param>
        public int BarsCalculated(int indicatorHandle)
        {
            var commandParameters = new ArrayList { indicatorHandle };

            return SendCommand<int>(Mt5CommandType.BarsCalculated, commandParameters);
        }

        ///<summary>
        ///Gets data of a specified buffer of a certain indicator in the necessary quantity.
        ///</summary>
        ///<param name="indicatorHandle">The indicator handle, returned by the corresponding indicator function.</param>
        ///<param name="bufferNum">The indicator buffer number.</param>
        ///<param name="startPos">The position of the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="buffer">Array of double type.</param>
        public int CopyBuffer(int indicatorHandle, int bufferNum, int startPos, int count, out double[] buffer)
        {
            var commandParameters = new ArrayList { indicatorHandle, bufferNum, startPos, count };
            buffer = SendCommand<double[]>(Mt5CommandType.CopyBuffer, commandParameters);

            return buffer?.Length ?? 0;
        }

        ///<summary>
        ///Gets data of a specified buffer of a certain indicator in the necessary quantity.
        ///</summary>
        ///<param name="indicatorHandle">The indicator handle, returned by the corresponding indicator function.</param>
        ///<param name="bufferNum">The indicator buffer number.</param>
        ///<param name="startTime">Bar time, corresponding to the first element.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="buffer">Array of double type.</param>
        public int CopyBuffer(int indicatorHandle, int bufferNum, DateTime startTime, int count, out double[] buffer)
        {
            var commandParameters = new ArrayList { indicatorHandle, bufferNum, Mt5TimeConverter.ConvertToMtTime(startTime), count };
            buffer = SendCommand<double[]>(Mt5CommandType.CopyBuffer1, commandParameters);

            return buffer?.Length ?? 0;
        }

        ///<summary>
        ///Gets data of a specified buffer of a certain indicator in the necessary quantity.
        ///</summary>
        ///<param name="indicatorHandle">The indicator handle, returned by the corresponding indicator function.</param>
        ///<param name="bufferNum">The indicator buffer number.</param>
        ///<param name="startTime">Bar time, corresponding to the first element.</param>
        ///<param name="stopTime">Bar time, corresponding to the last element.</param>
        ///<param name="buffer">Array of double type.</param>
        public int CopyBuffer(int indicatorHandle, int bufferNum, DateTime startTime, DateTime stopTime, out double[] buffer)
        {
            var commandParameters = new ArrayList { indicatorHandle, bufferNum, Mt5TimeConverter.ConvertToMtTime(startTime), Mt5TimeConverter.ConvertToMtTime(stopTime) };
            buffer = SendCommand<double[]>(Mt5CommandType.CopyBuffer1, commandParameters);

            return buffer?.Length ?? 0;
        }

        ///<summary>
        ///Gets history data of MqlRates structure of a specified symbol-period in specified quantity into the ratesArray array. The elements ordering of the copied data is from present to the past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startPos">The start position for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="ratesArray">Array of MqlRates type.</param>
        public int CopyRates(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count, out MqlRates[] ratesArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, startPos, count };

            ratesArray = null;

            var retVal = SendCommand<MtMqlRates[]>(Mt5CommandType.CopyRates, commandParameters);
            if (retVal != null)
            {
                ratesArray = new MqlRates[retVal.Length];
                for(var i = 0; i < retVal.Length; i++)
                {
                    ratesArray[i] = new MqlRates(Mt5TimeConverter.ConvertFromMtTime(retVal[i].time)
                        , retVal[i].open
                        , retVal[i].high
                        , retVal[i].low
                        , retVal[i].close
                        , retVal[i].tick_volume
                        , retVal[i].spread
                        , retVal[i].real_volume);
                }
            }

            return ratesArray?.Length ?? 0;
        }
            
        ///<summary>
        ///Gets history data of MqlRates structure of a specified symbol-period in specified quantity into the ratesArray array. The elements ordering of the copied data is from present to the past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">The start time for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="ratesArray">Array of MqlRates type.</param>
        public int CopyRates(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, int count, out MqlRates[] ratesArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(startTime), count };

            ratesArray = null;

            var retVal = SendCommand<MtMqlRates[]>(Mt5CommandType.CopyRates1, commandParameters);
            if (retVal != null)
            {
                ratesArray = new MqlRates[retVal.Length];
                for (var i = 0; i < retVal.Length; i++)
                {
                    ratesArray[i] = new MqlRates(Mt5TimeConverter.ConvertFromMtTime(retVal[i].time)
                        , retVal[i].open
                        , retVal[i].high
                        , retVal[i].low
                        , retVal[i].close
                        , retVal[i].tick_volume
                        , retVal[i].spread
                        , retVal[i].real_volume);
                }
            }

            return ratesArray?.Length ?? 0;
        }

        ///<summary>
        ///Gets history data of MqlRates structure of a specified symbol-period in specified quantity into the ratesArray array. The elements ordering of the copied data is from present to the past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">The start time for the first element to copy.</param>
        ///<param name="stopTime">Bar time, corresponding to the last element to copy.</param>
        ///<param name="ratesArray">Array of MqlRates type.</param>
        public int CopyRates(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, DateTime stopTime, out MqlRates[] ratesArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(startTime), Mt5TimeConverter.ConvertToMtTime(stopTime) };

            ratesArray = null;

            var retVal = SendCommand<MtMqlRates[]>(Mt5CommandType.CopyRates2, commandParameters);
            if (retVal != null)
            {
                ratesArray = new MqlRates[retVal.Length];
                for (var i = 0; i < retVal.Length; i++)
                {
                    ratesArray[i] = new MqlRates(Mt5TimeConverter.ConvertFromMtTime(retVal[i].time)
                        , retVal[i].open
                        , retVal[i].high
                        , retVal[i].low
                        , retVal[i].close
                        , retVal[i].tick_volume
                        , retVal[i].spread
                        , retVal[i].real_volume);
                }
            }

            return ratesArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets to time_array history data of bar opening time for the specified symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startPos">The start position for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="timeArray">Array of DatetTme type.</param>
        public int CopyTime(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count, out DateTime[] timeArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, startPos, count };

            timeArray = null;

            var retVal = SendCommand<long[]>(Mt5CommandType.CopyTime, commandParameters);
            if (retVal != null)
            {
                timeArray = new DateTime[retVal.Length];
                for (var i = 0; i < retVal.Length; i++)
                {
                    timeArray[i] = Mt5TimeConverter.ConvertFromMtTime(retVal[i]);
                }
            }

            return timeArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets to time_array history data of bar opening time for the specified symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">The start time for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="timeArray">Array of DatetTme type.</param>
        public int CopyTime(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, int count, out DateTime[] timeArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(startTime), count };

            timeArray = null;

            var retVal = SendCommand<long[]>(Mt5CommandType.CopyTime1, commandParameters);
            if (retVal != null)
            {
                timeArray = new DateTime[retVal.Length];
                for (var i = 0; i < retVal.Length; i++)
                {
                    timeArray[i] = Mt5TimeConverter.ConvertFromMtTime(retVal[i]);
                }
            }

            return timeArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets to time_array history data of bar opening time for the specified symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">The start time for the first element to copy.</param>
        ///<param name="stopTime">Bar time corresponding to the last element to copy.</param>
        ///<param name="timeArray">Array of DatetTme type.</param>
        public int CopyTime(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, DateTime stopTime, out DateTime[] timeArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(startTime), Mt5TimeConverter.ConvertToMtTime(stopTime) };

            timeArray = null;

            var retVal = SendCommand<long[]>(Mt5CommandType.CopyTime2, commandParameters);
            if (retVal != null)
            {
                timeArray = new DateTime[retVal.Length];
                for (var i = 0; i < retVal.Length; i++)
                {
                    timeArray[i] = Mt5TimeConverter.ConvertFromMtTime(retVal[i]);
                }
            }

            return timeArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets into open_array the history data of bar open prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startPos">The start position for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="openArray">Array of double type.</param>
        public int CopyOpen(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count, out double[] openArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, startPos, count };

            openArray = SendCommand<double[]>(Mt5CommandType.CopyOpen, commandParameters);

            return openArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets into open_array the history data of bar open prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">The start time for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="openArray">Array of double type.</param>
        public int CopyOpen(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, int count, out double[] openArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(startTime), count };

            openArray = SendCommand<double[]>(Mt5CommandType.CopyOpen1, commandParameters);

            return openArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets into open_array the history data of bar open prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">The start time for the first element to copy.</param>
        ///<param name="stopTime">The start time for the last element to copy.</param>
        ///<param name="openArray">Array of double type.</param>
        public int CopyOpen(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, DateTime stopTime, out double[] openArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(startTime), Mt5TimeConverter.ConvertToMtTime(stopTime) };

            openArray = SendCommand<double[]>(Mt5CommandType.CopyOpen2, commandParameters);

            return openArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets into high_array the history data of highest bar prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startPos">The start position for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="highArray">Array of double type.</param>
        public int CopyHigh(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count, out double[] highArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, startPos, count };

            highArray = SendCommand<double[]>(Mt5CommandType.CopyHigh, commandParameters);

            return highArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets into high_array the history data of highest bar prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">The start time for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="highArray">Array of double type.</param>
        public int CopyHigh(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, int count, out double[] highArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(startTime), count };

            highArray = SendCommand<double[]>(Mt5CommandType.CopyHigh1, commandParameters);

            return highArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets into high_array the history data of highest bar prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">The start time for the first element to copy.</param>
        ///<param name="stopTime">The start time for the last element to copy.</param>
        ///<param name="highArray">Array of double type.</param>
        public int CopyHigh(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, DateTime stopTime, out double[] highArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(startTime), Mt5TimeConverter.ConvertToMtTime(stopTime) };

            highArray = SendCommand<double[]>(Mt5CommandType.CopyHigh2, commandParameters);

            return highArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets into low_array the history data of minimal bar prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startPos">The start position for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="lowArray">Array of double type.</param>
        public int CopyLow(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count, out double[] lowArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, startPos, count };

            lowArray = SendCommand<double[]>(Mt5CommandType.CopyLow, commandParameters);

            return lowArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets into low_array the history data of minimal bar prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">The start time for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="lowArray">Array of double type.</param>
        public int CopyLow(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, int count, out double[] lowArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(startTime), count };

            lowArray = SendCommand<double[]>(Mt5CommandType.CopyLow1, commandParameters);

            return lowArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets into low_array the history data of minimal bar prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">The start time for the first element to copy.</param>
        ///<param name="stopTime">The start time for the last element to copy.</param>
        ///<param name="lowArray">Array of double type.</param>
        public int CopyLow(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, DateTime stopTime, out double[] lowArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(startTime), Mt5TimeConverter.ConvertToMtTime(stopTime) };

            lowArray = SendCommand<double[]>(Mt5CommandType.CopyLow2, commandParameters);

            return lowArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets into close_array the history data of bar close prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startPos">The start position for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="closeArray">Array of double type.</param>
        public int CopyClose(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count, out double[] closeArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, startPos, count };

            closeArray = SendCommand<double[]>(Mt5CommandType.CopyClose, commandParameters);

            return closeArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets into close_array the history data of bar close prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">The start time for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="closeArray">Array of double type.</param>
        public int CopyClose(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, int count, out double[] closeArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(startTime), count };

            closeArray = SendCommand<double[]>(Mt5CommandType.CopyClose1, commandParameters);

            return closeArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets into close_array the history data of bar close prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">The start time for the first element to copy.</param>
        ///<param name="stopTime">The start time for the last element to copy.</param>
        ///<param name="closeArray">Array of double type.</param>
        public int CopyClose(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, DateTime stopTime, out double[] closeArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(startTime), Mt5TimeConverter.ConvertToMtTime(stopTime) };

            closeArray = SendCommand<double[]>(Mt5CommandType.CopyClose2, commandParameters);

            return closeArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets into volume_array the history data of tick volumes for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startPos">The start position for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="volumeArray">Array of long type.</param>
        public int CopyTickVolume(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count, out long[] volumeArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, startPos, count };

            volumeArray = SendCommand<long[]>(Mt5CommandType.CopyTickVolume, commandParameters);

            return volumeArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets into volume_array the history data of tick volumes for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">The start time for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="volumeArray">Array of long type.</param>
        public int CopyTickVolume(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, int count, out long[] volumeArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(startTime), count };

            volumeArray = SendCommand<long[]>(Mt5CommandType.CopyTickVolume1, commandParameters);

            return volumeArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets into volume_array the history data of tick volumes for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">The start time for the first element to copy.</param>
        ///<param name="stopTime">The start time for the last element to copy.</param>
        ///<param name="volumeArray">Array of long type.</param>
        public int CopyTickVolume(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, DateTime stopTime, out long[] volumeArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(startTime), Mt5TimeConverter.ConvertToMtTime(stopTime) };

            volumeArray = SendCommand<long[]>(Mt5CommandType.CopyTickVolume2, commandParameters);

            return volumeArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets into volume_array the history data of trade volumes for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startPos">The start position for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="volumeArray">Array of long type.</param>
        public int CopyRealVolume(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count, out long[] volumeArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, startPos, count };

            volumeArray = SendCommand<long[]>(Mt5CommandType.CopyRealVolume, commandParameters);

            return volumeArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets into volume_array the history data of trade volumes for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">The start time for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="volumeArray">Array of long type.</param>
        public int CopyRealVolume(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, int count, out long[] volumeArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(startTime), count };

            volumeArray = SendCommand<long[]>(Mt5CommandType.CopyRealVolume1, commandParameters);

            return volumeArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets into volume_array the history data of trade volumes for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">The start time for the first element to copy.</param>
        ///<param name="stopTime">The start time for the last element to copy.</param>
        ///<param name="volumeArray">Array of long type.</param>
        public int CopyRealVolume(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, DateTime stopTime, out long[] volumeArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(startTime), Mt5TimeConverter.ConvertToMtTime(stopTime) };

            volumeArray = SendCommand<long[]>(Mt5CommandType.CopyRealVolume2, commandParameters);

            return volumeArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets into spread_array the history data of spread values for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startPos">The start position for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="spreadArray">Array of long type.</param>
        public int CopySpread(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count, out int[] spreadArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, startPos, count };

            spreadArray = SendCommand<int[]>(Mt5CommandType.CopySpread, commandParameters);

            return spreadArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets into spread_array the history data of spread values for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">The start time for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="spreadArray">Array of long type.</param>
        public int CopySpread(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, int count, out int[] spreadArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(startTime), count };

            spreadArray = SendCommand<int[]>(Mt5CommandType.CopySpread1, commandParameters);

            return spreadArray?.Length ?? 0;
        }

        ///<summary>
        ///The function gets into spread_array the history data of spread values for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">The start time for the first element to copy.</param>
        ///<param name="stopTime">The start time for the last element to copy.</param>
        ///<param name="spreadArray">Array of long type.</param>
        public int CopySpread(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, DateTime stopTime, out int[] spreadArray)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(startTime), Mt5TimeConverter.ConvertToMtTime(stopTime) };

            spreadArray = SendCommand<int[]>(Mt5CommandType.CopySpread2, commandParameters);

            return spreadArray?.Length ?? 0;
        }


        ///<summary>
        ///The function receives ticks in the MqlTick format into ticks_array. In this case, ticks are indexed from the past to the present, i.e. the 0 indexed tick is the oldest one in the array. For tick analysis, check the flags field, which shows what exactly has changed in the tick.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="flags">The flag that determines the type of received ticks.</param>
        ///<param name="from">The date from which you want to request ticks.  In milliseconds since 1970.01.01. If from=0, the last count ticks will be returned.</param>
        ///<param name="count">The number of ticks that you want to receive. If the 'from' and 'count' parameters are not specified, all available recent ticks (but not more than 2000) will be written to result.</param>
        ///<see href="https://www.mql5.com/en/docs/series/copyticks"/>
        public List<MqlTick> CopyTicks(string symbolName, CopyTicksFlag flags = CopyTicksFlag.All, ulong from = 0, uint count = 0)
        {
            var response = SendRequest<List<MqlTick>>(new CopyTicksRequest
            {
                SymbolName = symbolName,
                Flags = (int)flags,
                From = from,
                Count = count
            });
            return response;
        }

        ///<summary>
        ///The function returns the handle of a specified technical indicator created based on the array of parameters of MqlParam type.
        ///</summary>
        ///<param name="symbol">Name of a symbol, on data of which the indicator is calculated. NULL means the current symbol.</param>
        ///<param name="period">The value of the timeframe can be one of values of the ENUM_TIMEFRAMES enumeration, 0 means the current timeframe.</param>
        ///<param name="indicatorType">Indicator type, can be one of values of the ENUM_INDICATOR enumeration.</param>
        ///<param name="parameters">An array of MqlParam type, whose elements contain the type and value of each input parameter of a technical indicator.</param>
        public int IndicatorCreate(string symbol, ENUM_TIMEFRAMES period, ENUM_INDICATOR indicatorType, List<MqlParam> parameters = null)
        {
            var response = SendRequest<int>(new IndicatorCreateRequest
            {
                Symbol = symbol,
                Period = period,
                IndicatorType = indicatorType,
                Parameters = parameters
            });
            return response;
        }

        public bool IndicatorRelease(int indicatorHandle)
        {
            var commandParameters = new ArrayList { indicatorHandle };

            return SendCommand<bool>(Mt5CommandType.IndicatorRelease, commandParameters);
        }
        #endregion

        #region Market Info

        ///<summary>
        ///Returns the number of available (selected in Market Watch or all) symbols.
        ///</summary>
        ///<param name="selected">Request mode. Can be true or false.</param>
        public int SymbolsTotal(bool selected)
        {
            var commandParameters = new ArrayList { selected };

            return SendCommand<int>(Mt5CommandType.SymbolsTotal, commandParameters);
        }

        ///<summary>
        ///Returns the name of a symbol.
        ///</summary>
        ///<param name="pos">Order number of a symbol.</param>
        ///<param name="selected">Request mode. If the value is true, the symbol is taken from the list of symbols selected in MarketWatch. If the value is false, the symbol is taken from the general list.</param>
        public string SymbolName(int pos, bool selected)
        {
            var commandParameters = new ArrayList { pos, selected };

            return SendCommand<string>(Mt5CommandType.SymbolName, commandParameters);
        }

        ///<summary>
        ///Selects a symbol in the Market Watch window or removes a symbol from the window.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="selected">Switch. If the value is false, a symbol should be removed from MarketWatch, otherwise a symbol should be selected in this window. A symbol can't be removed if the symbol chart is open, or there are open positions for this symbol.</param>
        public bool SymbolSelect(string symbolName, bool selected)
        {
            var commandParameters = new ArrayList { symbolName, selected };

            return SendCommand<bool>(Mt5CommandType.SymbolSelect, commandParameters);
        }

        ///<summary>
        ///The function checks whether data of a selected symbol in the terminal are synchronized with data on the trade server.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        public bool SymbolIsSynchronized(string symbolName)
        {
            var commandParameters = new ArrayList { symbolName };

            return SendCommand<bool>(Mt5CommandType.SymbolIsSynchronized, commandParameters);
        }

        ///<summary>
        ///Returns the corresponding property of a specified symbol.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="propId">Identifier of a symbol property.</param>
        public double SymbolInfoDouble(string symbolName, ENUM_SYMBOL_INFO_DOUBLE propId)
        {
            var commandParameters = new ArrayList { symbolName, (int)propId };

            return SendCommand<double>(Mt5CommandType.SymbolInfoDouble, commandParameters);
        }

        ///<summary>
        ///Returns the corresponding property of a specified symbol.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="propId">Identifier of a symbol property.</param>
        public long SymbolInfoInteger(string symbolName, ENUM_SYMBOL_INFO_INTEGER propId)
        {
            var commandParameters = new ArrayList { symbolName, (int)propId };

            return SendCommand<long>(Mt5CommandType.SymbolInfoInteger, commandParameters);
        }

        ///<summary>
        ///Returns the corresponding property of a specified symbol. 
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="propId">Identifier of a symbol property.</param>
        public string SymbolInfoString(string symbolName, ENUM_SYMBOL_INFO_STRING propId)
        {
            var commandParameters = new ArrayList { symbolName, (int)propId };

            return SendCommand<string>(Mt5CommandType.SymbolInfoString, commandParameters);
        }

        ///<summary>
        ///Returns the corresponding property of a specified symbol. 
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="propId">Identifier of a symbol property.</param>
        ///<param name="value">Variable of the string type receiving the value of the requested property.</param>
        public bool SymbolInfoString(string symbolName, ENUM_SYMBOL_INFO_STRING propId, out string value)
        {
            var response = SendRequest<SymbolInfoStringResult>(new SymbolInfoStringRequest
            {
                SymbolName = symbolName,
                PropId = propId
            });

            value = response?.StringVar;
            return response?.RetVal ?? false;
        }

        ///<summary>
        ///The function returns current prices of a specified symbol in a variable of the MqlTick type.
        ///The function returns true if successful, otherwise returns false.
        ///</summary>
        ///<param name="symbol">Symbol name.</param>
        ///<param name="tick"> Link to the structure of the MqlTick type, to which the current prices and time of the last price update will be placed.</param>
        public bool SymbolInfoTick(string symbol, out MqlTick  tick)
        {
            var commandParameters = new ArrayList { symbol };

            var retVal = SendCommand<MtMqlTick>(Mt5CommandType.SymbolInfoTick, commandParameters);

            tick = null;
            if (retVal != null)
            {
                tick = new MqlTick { MtTime = retVal.time, ask = retVal.ask, bid = retVal.bid, last = retVal.last, volume = retVal.volume };
            }

            return tick != null;
        }


        ///<summary>
        ///Allows receiving time of beginning and end of the specified quoting sessions for a specified symbol and weekday.
        ///</summary>
        ///<param name="name">Symbol name.</param>
        ///<param name="dayOfWeek">Day of the week</param>
        ///<param name="sessionIndex">Ordinal number of a session, whose beginning and end time we want to receive. Indexing of sessions starts with 0.</param>
        ///<param name="from">Session beginning time in seconds from 00 hours 00 minutes, in the returned value date should be ignored.</param>
        ///<param name="to">Session end time in seconds from 00 hours 00 minutes, in the returned value date should be ignored.</param>
        public bool SymbolInfoSessionQuote(string name, ENUM_DAY_OF_WEEK dayOfWeek, uint sessionIndex, out DateTime from, out DateTime to)
        {
            var commandParameters = new ArrayList { name, (int)dayOfWeek, sessionIndex };

            string strResult = SendCommand<string>(Mt5CommandType.SymbolInfoSessionQuote, commandParameters);

            return strResult.ParseResult(ParamSeparator, out from, out to); 
        }

        ///<summary>
        ///Allows receiving time of beginning and end of the specified trading sessions for a specified symbol and weekday.
        ///</summary>
        ///<param name="name">Symbol name.</param>
        ///<param name="dayOfWeek">Day of the week</param>
        ///<param name="sessionIndex">Ordinal number of a session, whose beginning and end time we want to receive. Indexing of sessions starts with 0.</param>
        ///<param name="from">Session beginning time in seconds from 00 hours 00 minutes, in the returned value date should be ignored.</param>
        ///<param name="to">Session end time in seconds from 00 hours 00 minutes, in the returned value date should be ignored.</param>
        public bool SymbolInfoSessionTrade(string name, ENUM_DAY_OF_WEEK dayOfWeek, uint sessionIndex, out DateTime from, out DateTime to)
        {
            var commandParameters = new ArrayList { name, (int)dayOfWeek, sessionIndex };

            string strResult = SendCommand<string>(Mt5CommandType.SymbolInfoSessionTrade, commandParameters);

            return strResult.ParseResult(ParamSeparator, out from, out to);
        }

        ///<summary>
        ///Provides opening of Depth of Market for a selected symbol, and subscribes for receiving notifications of the DOM changes.
        ///</summary>
        ///<param name="symbol">Symbol name.</param>
        public bool MarketBookAdd(string symbol)
        {
            var commandParameters = new ArrayList { symbol };

            return SendCommand<bool>(Mt5CommandType.MarketBookAdd, commandParameters);
        }

        ///<summary>
        ///Provides closing of Depth of Market for a selected symbol, and cancels the subscription for receiving notifications of the DOM changes.
        ///</summary>
        ///<param name="symbol">Symbol name.</param>
        public bool MarketBookRelease(string symbol)
        {
            var commandParameters = new ArrayList { symbol };

            return SendCommand<bool>(Mt5CommandType.MarketBookRelease, commandParameters);
        }

        ///<summary>
        ///Returns a structure array MqlBookInfo containing records of the Depth of Market of a specified symbol.
        ///</summary>
        ///<param name="symbol">Symbol name.</param>
        ///<param name="book">Reference to an array of Depth of Market records.</param>        
        public bool MarketBookGet(string symbol, out MqlBookInfo[] book)
        {
            var response = SendRequest<List<MqlBookInfo>>(new MarketBookGetRequest
            {
                Symbol = symbol
            });

            book = response?.ToArray();
            return response != null;
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
            return SendCommand<long>(Mt5CommandType.ChartId, null);
        }

        ///<summary>
        ///This function calls a forced redrawing of a specified chart.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        public void ChartRedraw(long chartId = 0)
        {
            var commandParameters = new ArrayList { chartId };
            SendCommand<object>(Mt5CommandType.ChartRedraw, commandParameters);
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
            var commandParameters = new ArrayList { chartId, filename };
            return SendCommand<bool>(Mt5CommandType.ChartApplyTemplate, commandParameters);
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
            var commandParameters = new ArrayList { chartId, filename };
            return SendCommand<bool>(Mt5CommandType.ChartSaveTemplate, commandParameters);
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
            var commandParameters = new ArrayList { chartId, indicatorShortname };
            return SendCommand<int>(Mt5CommandType.ChartWindowFind, commandParameters);
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
            var result = SendRequest<ChartTimePriceToXyResult>(new ChartTimePriceToXyRequest
                {
                    ChartId = chartId,
                    SubWindow = subWindow,
                    Time = time,
                    Price = price
                });

            x = result?.X ?? 0;
            y = result?.Y ?? 0;
            return result?.RetVal ?? false;
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
            var result = SendRequest<ChartXyToTimePriceResult>(new ChartXyToTimePriceRequest
            {
                ChartId = chartId,
                X = x,
                Y = y
            });

            subWindow = result?.SubWindow ?? 0;
            time = result?.Time;
            price = result?.Price ?? double.NaN;
            return result?.RetVal ?? false;
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
            var commandParameters = new ArrayList { symbol, (int)period };
            return SendCommand<long>(Mt5CommandType.ChartOpen, commandParameters);
        }

        ///<summary>
        ///Returns the ID of the first chart of the client terminal.
        ///</summary>
        public long ChartFirst()
        {
            return SendCommand<long>(Mt5CommandType.ChartFirst, null);
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
            var commandParameters = new ArrayList { chartId };
            return SendCommand<long>(Mt5CommandType.ChartNext, commandParameters);
        }

        ///<summary>
        ///Closes the specified chart.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<returns>
        ///If successful, returns true, otherwise false.
        ///</returns>
        public bool ChartClose(long chartId = 0)
        {
            var commandParameters = new ArrayList { chartId };
            return SendCommand<bool>(Mt5CommandType.ChartClose, commandParameters);
        }

        ///<summary>
        ///Returns the symbol name for the specified chart.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<returns>
        ///If chart does not exist, the result will be an empty string.
        ///</returns>
        public string ChartSymbol(long chartId)
        {
            var commandParameters = new ArrayList { chartId };
            return SendCommand<string>(Mt5CommandType.ChartSymbol, commandParameters);
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
            var commandParameters = new ArrayList { chartId };
            return (ENUM_TIMEFRAMES)SendCommand<int>(Mt5CommandType.ChartPeriod, commandParameters);
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
        public bool ChartSetDouble(long chartId, ENUM_CHART_PROPERTY_DOUBLE propId, double value)
        {
            var commandParameters = new ArrayList { chartId, (int)propId, value };
            return SendCommand<bool>(Mt5CommandType.ChartSetDouble, commandParameters);
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
        public bool ChartSetInteger(long chartId, ENUM_CHART_PROPERTY_INTEGER propId, long value)
        {
            var commandParameters = new ArrayList { chartId, (int)propId, value };
            return SendCommand<bool>(Mt5CommandType.ChartSetInteger, commandParameters);
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
        public bool ChartSetString(long chartId, ENUM_CHART_PROPERTY_STRING propId, string value)
        {
            var commandParameters = new ArrayList { chartId, (int)propId, value };
            return SendCommand<bool>(Mt5CommandType.ChartSetString, commandParameters);
        }

        ///<summary>
        ///Returns the value of a corresponding property of the specified chart. Chart property must be of double type.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<param name="propId">Chart property ID. This value can be one of the ENUM_CHART_PROPERTY_DOUBLE values.</param>
        ///<param name="subWindow">Number of the chart subwindow. For the first case, the default value is 0 (main chart window). The most of the properties do not require a subwindow number.</param>
        ///<returns>
        ///The value of double type.
        ///</returns>
        public double ChartGetDouble(long chartId, ENUM_CHART_PROPERTY_DOUBLE propId, int subWindow = 0)
        {
            var commandParameters = new ArrayList { chartId, (int)propId, subWindow };
            return SendCommand<double>(Mt5CommandType.ChartGetDouble, commandParameters);
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
        public long ChartGetInteger(long chartId, ENUM_CHART_PROPERTY_INTEGER propId, int subWindow = 0)
        {
            var commandParameters = new ArrayList { chartId, (int)propId, subWindow };
            return SendCommand<long>(Mt5CommandType.ChartGetInteger, commandParameters);
        }

        ///<summary>
        ///Returns the value of a corresponding property of the specified chart. Chart property must be of string type.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<param name="propId">Chart property ID. This value can be one of the ENUM_CHART_PROPERTY_STRING values.</param>
        ///<returns>
        ///The value of string type.
        ///</returns>
        public string ChartGetString(long chartId, ENUM_CHART_PROPERTY_STRING propId)
        {
            var commandParameters = new ArrayList { chartId, (int)propId };
            return SendCommand<string>(Mt5CommandType.ChartGetString, commandParameters);
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
        public bool ChartNavigate(long chartId, ENUM_CHART_POSITION position, int shift = 0)
        {
            var commandParameters = new ArrayList { chartId, (int)position, shift };
            return SendCommand<bool>(Mt5CommandType.ChartNavigate, commandParameters);
        }

        ///<summary>
        ///Adds an indicator with the specified handle into a specified chart window. Indicator and chart should be generated on the same symbol and time frame.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<param name="subWindow">Number of the chart subwindow. 0 denotes the main chart subwindow.</param>
        ///<param name="indicatorHandle">The handle of the indicator.</param>
        ///<returns>
        ///The function returns true in case of success, otherwise it returns false. 
        ///</returns>
        public bool ChartIndicatorAdd(long chartId, int subWindow, int indicatorHandle)
        {
            var commandParameters = new ArrayList { chartId, subWindow, indicatorHandle };
            return SendCommand<bool>(Mt5CommandType.ChartIndicatorAdd, commandParameters);
        }

        ///<summary>
        ///Removes an indicator with a specified name from the specified chart window.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<param name="subWindow">Number of the chart subwindow. 0 denotes the main chart subwindow.</param>
        ///<param name="indicatorShortname">The short name of the indicator which is set in the INDICATOR_SHORTNAME property with the IndicatorSetString() function. To get the short name of an indicator use the ChartIndicatorName() function.</param>
        ///<returns>
        ///Returns true in case of successful deletion of the indicator. 
        ///</returns>
        public bool ChartIndicatorDelete(long chartId, int subWindow, string indicatorShortname)
        {
            var commandParameters = new ArrayList { chartId, subWindow, indicatorShortname };
            return SendCommand<bool>(Mt5CommandType.ChartIndicatorDelete, commandParameters);
        }

        ///<summary>
        ///Returns the handle of the indicator with the specified short name in the specified chart window.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<param name="subWindow">Number of the chart subwindow. 0 denotes the main chart subwindow.</param>
        ///<param name="indicatorShortname">The short name of the indicator which is set in the INDICATOR_SHORTNAME property with the IndicatorSetString() function. To get the short name of an indicator use the ChartIndicatorName() function.</param>
        ///<returns>
        ///Returns an indicator handle if successful, otherwise returns INVALID_HANDLE. 
        ///</returns>
        public int ChartIndicatorGet(long chartId, int subWindow, string indicatorShortname)
        {
            var commandParameters = new ArrayList { chartId, subWindow, indicatorShortname };
            return SendCommand<int>(Mt5CommandType.ChartIndicatorGet, commandParameters);
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
        public string ChartIndicatorName(long chartId, int subWindow, int index)
        {
            var commandParameters = new ArrayList { chartId, subWindow, index };
            return SendCommand<string>(Mt5CommandType.ChartIndicatorName, commandParameters);
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
            var commandParameters = new ArrayList { chartId, subWindow };
            return SendCommand<int>(Mt5CommandType.ChartIndicatorsTotal, commandParameters);
        }

        ///<summary>
        ///Returns the number (index) of the chart subwindow the Expert Advisor or script has been dropped to. 0 means the main chart window.
        ///</summary>
        public int ChartWindowOnDropped()
        {
            return SendCommand<int>(Mt5CommandType.ChartWindowOnDropped, null);
        }

        ///<summary>
        ///Returns the price coordinate corresponding to the chart point the Expert Advisor or script has been dropped to.
        ///</summary>
        public double ChartPriceOnDropped()
        {
            return SendCommand<double>(Mt5CommandType.ChartPriceOnDropped, null);
        }

        ///<summary>
        ///Returns the time coordinate corresponding to the chart point the Expert Advisor or script has been dropped to.
        ///</summary>
        public DateTime ChartTimeOnDropped()
        {
            var res = SendCommand<int>(Mt5CommandType.ChartTimeOnDropped, null);
            return Mt5TimeConverter.ConvertFromMtTime(res);
        }

        ///<summary>
        ///Returns the X coordinate of the chart point the Expert Advisor or script has been dropped to.
        ///</summary>
        public int ChartXOnDropped()
        {
            return SendCommand<int>(Mt5CommandType.ChartXOnDropped, null);
        }

        ///<summary>
        ///Returns the Y coordinateof the chart point the Expert Advisor or script has been dropped to.
        ///</summary>
        public int ChartYOnDropped()
        {
            return SendCommand<int>(Mt5CommandType.ChartYOnDropped, null);
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
            var commandParameters = new ArrayList { chartId, symbol, (int)period };
            return SendCommand<bool>(Mt5CommandType.ChartSetSymbolPeriod, commandParameters);
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
        public bool ChartScreenShot(long chartId, string filename, int width, int height, ENUM_ALIGN_MODE alignMode = ENUM_ALIGN_MODE.ALIGN_RIGHT)
        {
            var commandParameters = new ArrayList { chartId, filename, width, height, (int)alignMode };
            return SendCommand<bool>(Mt5CommandType.ChartScreenShot, commandParameters);
        }
        #endregion

        #region Commands of Terminal
        ///<summary>
        ///Returns the value of a corresponding property of the mql5 program environment. 
        ///</summary>
        ///<param name="propertyId">Identifier of a property. Can be one of the values of the ENUM_TERMINAL_INFO_STRING enumeration.</param>
        ///<returns>
        ///Value of string type.
        ///</returns>
        public string TerminalInfoString(ENUM_TERMINAL_INFO_STRING propertyId)
        {
            var commandParameters = new ArrayList { (int)propertyId };
            return SendCommand<string>(Mt5CommandType.TerminalInfoString, commandParameters);
        }

        ///<summary>
        ///Returns the value of a corresponding property of the mql5 program environment.
        ///</summary>
        ///<param name="propertyId">Identifier of a property. Can be one of the values of the ENUM_TERMINAL_INFO_INTEGER enumeration.</param>
        ///<returns>
        ///Value of int type.
        ///</returns>
        public int TerminalInfoInteger(ENUM_TERMINAL_INFO_INTEGER propertyId)
        {
            var commandParameters = new ArrayList { (int)propertyId };
            return SendCommand<int>(Mt5CommandType.TerminalInfoInteger, commandParameters);
        }

        ///<summary>
        ///Returns the value of a corresponding property of the mql5 program environment.
        ///</summary>
        ///<param name="propertyId">Identifier of a property. Can be one of the values of the ENUM_TERMINAL_INFO_DOUBLE enumeration.</param>
        ///<returns>
        ///Value of double type.
        ///</returns>
        public double TerminalInfoDouble(ENUM_TERMINAL_INFO_DOUBLE propertyId)
        {
            var commandParameters = new ArrayList { (int)propertyId };
            return SendCommand<double>(Mt5CommandType.TerminalInfoDouble, commandParameters);
        }
        #endregion 


        #region Common Functions

        ///<summary>
        ///It enters a message in the Expert Advisor log.
        ///</summary>
        ///<param name="message">Message</param>
        public bool Print(string message)
        {
            var commandParameters = new ArrayList { message };
            return SendCommand<bool>(Mt5CommandType.Print, commandParameters);
        }

        ///<summary>
        ///Displays a message in a separate window.
        ///</summary>
        ///<param name="message">Message</param>
        public void Alert(string message)
        {
            var commandParameters = new ArrayList { message };
            SendCommand<object>(Mt5CommandType.Alert, commandParameters);
        }

        #endregion // Common Functions

        #region Object Functions

        ///<summary>
        ///The function creates an object with the specified name, type, and the initial coordinates in the specified chart subwindow. 
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="name">Name of the object. The name must be unique within a chart, including its subwindows.</param>
        ///<param name="type">Object type. The value can be one of the values of the ENUM_OBJECT enumeration.</param>
        ///<param name="nwin">Number of the chart subwindow. 0 means the main chart window. The specified subwindow must exist, otherwise the function returns false.</param>
        ///<param name="time">The time coordinate of the first anchor.</param>
        ///<param name="price">The price coordinate of the first anchor point.</param>
        public bool ObjectCreate(long chartId, string name, ENUM_OBJECT type, int nwin, DateTime time, double price)
        {
            var commandParameters = new ArrayList { chartId, name, (int)type, nwin, Mt5TimeConverter.ConvertToMtTime(time), price };
            return SendCommand<bool>(Mt5CommandType.ObjectCreate, commandParameters);
        }

        ///<summary>
        ///The function returns the name of the corresponding object in the specified chart, in the specified subwindow, of the specified type.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="pos">Ordinal number of the object according to the specified filter by the number and type of the subwindow.</param>
        ///<param name="subWindow">umber of the chart subwindow. 0 means the main chart window, -1 means all the subwindows of the chart, including the main window.</param>
        ///<param name="type">Type of the object. The value can be one of the values of the ENUM_OBJECT enumeration. -1 means all types.</param>
        public string ObjectName(long chartId, int pos, int subWindow = -1, int type = -1)
        {
            var commandParameters = new ArrayList { chartId, pos, subWindow, type };
            return SendCommand<string>(Mt5CommandType.ObjectName, commandParameters);
        }

        ///<summary>
        ///The function removes the object with the specified name from the specified chart.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="name">Name of object to be deleted.</param>
        public bool ObjectDelete(long chartId, string name)
        {
            var commandParameters = new ArrayList { chartId, name };
            return SendCommand<bool>(Mt5CommandType.ObjectDelete, commandParameters);
        }

        ///<summary>
        ///The function removes the object with the specified name from the specified chart.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="subWindow">Number of the chart subwindow. 0 means the main chart window, -1 means all the subwindows of the chart, including the main window.</param>
        ///<param name="type">Type of the object. The value can be one of the values of the ENUM_OBJECT enumeration. -1 means all types.</param>
        public int ObjectsDeleteAll(long chartId, int subWindow = -1, int type = -1)
        {
            var commandParameters = new ArrayList { chartId, subWindow, type };
            return SendCommand<int>(Mt5CommandType.ObjectsDeleteAll, commandParameters);
        }

        ///<summary>
        ///The function searches for an object with the specified name in the chart with the specified ID.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="name">The name of the searched object.</param>
        public int ObjectFind(long chartId, string name)
        {
            var commandParameters = new ArrayList { chartId, name };
            return SendCommand<int>(Mt5CommandType.ObjectFind, commandParameters);
        }

        ///<summary>
        ///The function returns the time value for the specified price value of the specified object.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="name">Name of the object.</param>
        ///<param name="value">Price value.</param>
        ///<param name="lineId">Line identifier.</param>
        public DateTime ObjectGetTimeByValue(long chartId, string name, double value, int lineId)
        {
            var commandParameters = new ArrayList { chartId, name, value, lineId };
            var res = SendCommand<int>(Mt5CommandType.ObjectGetTimeByValue, commandParameters);
            return Mt5TimeConverter.ConvertFromMtTime(res);
        }

        ///<summary>
        ///The function returns the price value for the specified time value of the specified object.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="name">Name of the object.</param>
        ///<param name="time">Time value.</param>
        ///<param name="lineId">Line identifier.</param>
        public double ObjectGetValueByTime(long chartId, string name, DateTime time, int lineId)
        {
            var commandParameters = new ArrayList { chartId, name, Mt5TimeConverter.ConvertToMtTime(time), lineId };
            return SendCommand<double>(Mt5CommandType.ObjectGetValueByTime, commandParameters);
        }

        ///<summary>
        ///The function changes coordinates of the specified anchor point of the object.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="name">Name of the object.</param>
        ///<param name="pointIndex">Index of the anchor point. The number of anchor points depends on the type of object.</param>
        ///<param name="time">Time coordinate of the selected anchor point.</param>
        ///<param name="price">Price coordinate of the selected anchor point.</param>
        public bool ObjectMove(long chartId, string name, int pointIndex, DateTime time, double price)
        {
            var commandParameters = new ArrayList { chartId, name, pointIndex, Mt5TimeConverter.ConvertToMtTime(time), price };
            return SendCommand<bool>(Mt5CommandType.ObjectMove, commandParameters);
        }

        ///<summary>
        ///The function returns the number of objects in the specified chart, specified subwindow, of the specified type.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="subWindow">Number of the chart subwindow. 0 means the main chart window, -1 means all the subwindows of the chart, including the main window.</param>
        ///<param name="type">Type of the object. The value can be one of the values of the ENUM_OBJECT enumeration. -1 means all types.</param>
        public int ObjectsTotal(long chartId, int subWindow = -1, int type = -1)

        {
            var commandParameters = new ArrayList { chartId, subWindow, type };
            return SendCommand<int>(Mt5CommandType.ObjectsTotal, commandParameters);
        }

        ///<summary>
        ///The function sets the value of the corresponding object property. The object property must be of the double type.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="name">Name of the object.</param>
        ///<param name="propId">ID of the object property. The value can be one of the values of the ENUM_OBJECT_PROPERTY_DOUBLE enumeration.</param>
        ///<param name="propValue">The value of the property.</param>
        public bool ObjectSetDouble(long chartId, string name, ENUM_OBJECT_PROPERTY_DOUBLE propId, double propValue)

        {
            var commandParameters = new ArrayList { chartId, name, (int)propId, propValue };
            return SendCommand<bool>(Mt5CommandType.ObjectSetDouble, commandParameters);
        }

        ///<summary>
        ///The function sets the value of the corresponding object property. The object property must be of the datetime, int, color, bool or char type.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="name">Name of the object.</param>
        ///<param name="propId">ID of the object property. The value can be one of the values of the ENUM_OBJECT_PROPERTY_INTEGER enumeration.</param>
        ///<param name="propValue">The value of the property.</param>
        public bool ObjectSetInteger(long chartId, string name, ENUM_OBJECT_PROPERTY_INTEGER propId, long propValue)

        {
            var commandParameters = new ArrayList { chartId, name, (int)propId, propValue };
            return SendCommand<bool>(Mt5CommandType.ObjectSetInteger, commandParameters);
        }

        ///<summary>
        ///The function sets the value of the corresponding object property. The object property must be of the string type.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="name">Name of the object.</param>
        ///<param name="propId">ID of the object property. The value can be one of the values of the ENUM_OBJECT_PROPERTY_STRING enumeration.</param>
        ///<param name="propValue">The value of the property.</param>
        public bool ObjectSetString(long chartId, string name, ENUM_OBJECT_PROPERTY_STRING propId, string propValue)

        {
            var commandParameters = new ArrayList { chartId, name, (int)propId, propValue };
            return SendCommand<bool>(Mt5CommandType.ObjectSetString, commandParameters);
        }

        ///<summary>
        ///The function returns the value of the corresponding object property. The object property must be of the double type.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="name">Name of the object.</param>
        ///<param name="propId">ID of the object property. The value can be one of the values of the ENUM_OBJECT_PROPERTY_DOUBLE enumeration.</param>
        public double ObjectGetDouble(long chartId, string name, ENUM_OBJECT_PROPERTY_DOUBLE propId)
        {
            var commandParameters = new ArrayList { chartId, name, (int)propId };
            return SendCommand<double>(Mt5CommandType.ObjectGetDouble, commandParameters);
        }

        ///<summary>
        ///he function returns the value of the corresponding object property. The object property must be of the datetime, int, color, bool or char type.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="name">Name of the object.</param>
        ///<param name="propId">ID of the object property. The value can be one of the values of the ENUM_OBJECT_PROPERTY_INTEGER enumeration.</param>
        public long ObjectGetInteger(long chartId, string name, ENUM_OBJECT_PROPERTY_INTEGER propId)
        {
            var commandParameters = new ArrayList { chartId, name, (int)propId };
            return SendCommand<long>(Mt5CommandType.ObjectGetInteger, commandParameters);
        }

        ///<summary>
        ///The function returns the value of the corresponding object property. The object property must be of the string type.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="name">Name of the object.</param>
        ///<param name="propId">ID of the object property. The value can be one of the values of the ENUM_OBJECT_PROPERTY_STRING enumeration.</param>
        public string ObjectGetString(long chartId, string name, ENUM_OBJECT_PROPERTY_STRING propId)
        {
            var commandParameters = new ArrayList { chartId, name, (int)propId };
            return SendCommand<string>(Mt5CommandType.ObjectGetString, commandParameters);
        }

        #endregion //Object Functions

        #region Technical Indicators

        ///<summary>
        ///The function creates Accelerator Oscillator in a global cache of the client terminal and returns its handle.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        public int iAC(string symbol, ENUM_TIMEFRAMES period)
        {
            var commandParameters = new ArrayList { symbol, (int)period };
            return SendCommand<int>(Mt5CommandType.iAC, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Accumulation/Distribution indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="appliedVolume">The volume used. Can be any of ENUM_APPLIED_VOLUME values.</param>
        public int iAD(string symbol, ENUM_TIMEFRAMES period, ENUM_APPLIED_VOLUME appliedVolume)
        {
            var commandParameters = new ArrayList { symbol, (int)period, (int)appliedVolume };
            return SendCommand<int>(Mt5CommandType.iAD, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Average Directional Movement Index indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="adxPeriod">Period to calculate the index.</param>
        public int iADX(string symbol, ENUM_TIMEFRAMES period, int adxPeriod)
        {
            var commandParameters = new ArrayList { symbol, (int)period, adxPeriod };
            return SendCommand<int>(Mt5CommandType.iADX, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of Average Directional Movement Index by Welles Wilder.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="adxPeriod">Period to calculate the index.</param>
        public int iADXWilder(string symbol, ENUM_TIMEFRAMES period, int adxPeriod)
        {
            var commandParameters = new ArrayList { symbol, (int)period, adxPeriod };
            return SendCommand<int>(Mt5CommandType.iADXWilder, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Alligator indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="jawPeriod">Averaging period for the blue line (Alligator's Jaw).</param>
        ///<param name="jawShift">The shift of the blue line relative to the price chart.</param>
        ///<param name="teethPeriod">Averaging period for the red line (Alligator's Teeth).</param>
        ///<param name="teethShift">The shift of the red line relative to the price chart.</param>
        ///<param name="lipsPeriod">Averaging period for the green line (Alligator's lips).</param>
        ///<param name="lipsShift">The shift of the green line relative to the price chart.</param>
        ///<param name="maMethod">The method of averaging. Can be any of the ENUM_MA_METHOD values.</param>
        ///<param name="appliedPrice">The price used. Can be any of the price constants ENUM_APPLIED_PRICE or a handle of another indicator.</param>
        public int iAlligator(string symbol, ENUM_TIMEFRAMES period, int jawPeriod, int jawShift, int teethPeriod, 
            int teethShift, int lipsPeriod, int lipsShift, ENUM_MA_METHOD maMethod, ENUM_APPLIED_PRICE appliedPrice)
        {
            var commandParameters = new ArrayList { symbol, (int)period, jawPeriod, jawShift, teethPeriod, teethShift, lipsPeriod, lipsShift, (int)maMethod, (int)appliedPrice };
            return SendCommand<int>(Mt5CommandType.iAlligator, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Adaptive Moving Average indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="amaPeriod">The calculation period, on which the efficiency coefficient is calculated.</param>
        ///<param name="fastMaPeriod">Fast period for the smoothing coefficient calculation for a rapid market.</param>
        ///<param name="slowMaPeriod">Slow period for the smoothing coefficient calculation in the absence of trend.</param>
        ///<param name="amaShift">Shift of the indicator relative to the price chart.</param>
        ///<param name="appliedPrice">The price used. Can be any of the price constants ENUM_APPLIED_PRICE or a handle of another indicator.</param>
        public int iAMA(string symbol, ENUM_TIMEFRAMES period, int amaPeriod, int fastMaPeriod, int slowMaPeriod, int amaShift, ENUM_APPLIED_PRICE appliedPrice)
        {
            var commandParameters = new ArrayList { symbol, (int)period, amaPeriod, fastMaPeriod, slowMaPeriod, amaShift, (int)appliedPrice };
            return SendCommand<int>(Mt5CommandType.iAMA, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Awesome Oscillator indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        public int iAO(string symbol, ENUM_TIMEFRAMES period)
        {
            var commandParameters = new ArrayList { symbol, (int)period };
            return SendCommand<int>(Mt5CommandType.iAO, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Average True Range indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="maPeriod">The value of the averaging period for the indicator calculation.</param>
        public int iATR(string symbol, ENUM_TIMEFRAMES period, int maPeriod)
        {
            var commandParameters = new ArrayList { symbol, (int)period, maPeriod };
            return SendCommand<int>(Mt5CommandType.iATR, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Bears Power indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="maPeriod">The value of the averaging period for the indicator calculation.</param>
        public int iBearsPower(string symbol, ENUM_TIMEFRAMES period, int maPeriod)
        {
            var commandParameters = new ArrayList { symbol, (int)period, maPeriod };
            return SendCommand<int>(Mt5CommandType.iBearsPower, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Bollinger Bands® indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="bandsPeriod">The averaging period of the main line of the indicator.</param>
        ///<param name="bandsShift">The shift the indicator relative to the price chart.</param>
        ///<param name="deviation">Deviation from the main line.</param>
        ///<param name="appliedPrice">The price used. Can be any of the price constants ENUM_APPLIED_PRICE or a handle of another indicator.</param>
        public int iBands(string symbol, ENUM_TIMEFRAMES period, int bandsPeriod, int bandsShift, double deviation, ENUM_APPLIED_PRICE appliedPrice)
        {
            var commandParameters = new ArrayList { symbol, (int)period, bandsPeriod, bandsShift, deviation, (int)appliedPrice };
            return SendCommand<int>(Mt5CommandType.iBands, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Bulls Power indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="maPeriod">The averaging period for the indicator calculation.</param>
        public int iBullsPower(string symbol, ENUM_TIMEFRAMES period, int maPeriod)
        {
            var commandParameters = new ArrayList { symbol, (int)period, maPeriod };
            return SendCommand<int>(Mt5CommandType.iBullsPower, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Commodity Channel Index indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="maPeriod">The averaging period for the indicator calculation.</param>
        ///<param name="appliedPrice">The price used. Can be any of the price constants ENUM_APPLIED_PRICE or a handle of another indicator.</param>
        public int iCCI(string symbol, ENUM_TIMEFRAMES period, int maPeriod, ENUM_APPLIED_PRICE appliedPrice)
        {
            var commandParameters = new ArrayList { symbol, (int)period, maPeriod, (int)appliedPrice };
            return SendCommand<int>(Mt5CommandType.iCCI, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Bulls Power indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="fastMaPeriod">Fast averaging period for calculations.</param>
        ///<param name="slowMaPeriod">Slow averaging period for calculations.</param>
        ///<param name="maMethod">Smoothing type. Can be one of the averaging constants of ENUM_MA_METHOD.</param>
        ///<param name="appliedVolume">The volume used. Can be one of the constants of ENUM_APPLIED_VOLUME.</param>
        public int iChaikin(string symbol, ENUM_TIMEFRAMES period, int fastMaPeriod, int slowMaPeriod, ENUM_MA_METHOD maMethod, ENUM_APPLIED_VOLUME appliedVolume)
        {
            var commandParameters = new ArrayList { symbol, (int)period, fastMaPeriod, slowMaPeriod, (int)maMethod, (int)appliedVolume };
            return SendCommand<int>(Mt5CommandType.iChaikin, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Bulls Power indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="maPeriod">Averaging period (bars count) for calculations.</param>
        ///<param name="maShift">Shift of the indicator relative to the price chart.</param>
        ///<param name="appliedPrice">The price used. Can be any of the price constants ENUM_APPLIED_PRICE or a handle of another indicator.</param>
        public int iDEMA(string symbol, ENUM_TIMEFRAMES period, int maPeriod, int maShift, ENUM_APPLIED_PRICE appliedPrice)
        {
            var commandParameters = new ArrayList { symbol, (int)period, maPeriod, maShift, (int)appliedPrice };
            return SendCommand<int>(Mt5CommandType.iDEMA, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the DeMarker indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="maPeriod">Averaging period (bars count) for calculations.</param>
        public int iDeMarker(string symbol, ENUM_TIMEFRAMES period, int maPeriod)
        {
            var commandParameters = new ArrayList { symbol, (int)period, maPeriod };
            return SendCommand<int>(Mt5CommandType.iDeMarker, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Envelopes indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="maPeriod">Averaging period for the main line.</param>
        ///<param name="maShift">The shift of the indicator relative to the price chart.</param>
        ///<param name="maMethod">Smoothing type. Can be one of the values of ENUM_MA_METHOD.</param>
        ///<param name="appliedPrice">The price used. Can be any of the price constants ENUM_APPLIED_PRICE or a handle of another indicator.</param>
        ///<param name="deviation">The deviation from the main line (in percents).</param>
        public int iEnvelopes(string symbol, ENUM_TIMEFRAMES period, int maPeriod, int maShift, ENUM_MA_METHOD maMethod, ENUM_APPLIED_PRICE appliedPrice, double deviation)
        {
            var commandParameters = new ArrayList { symbol, (int)period, maPeriod, maShift, (int)maMethod, (int)appliedPrice, deviation };
            return SendCommand<int>(Mt5CommandType.iEnvelopes, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Force Index indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="maPeriod">Averaging period for the indicator calculations.</param>
        ///<param name="maMethod">Smoothing type. Can be one of the values of ENUM_MA_METHOD.</param>
        ///<param name="appliedVolume">The volume used. Can be one of the values of ENUM_APPLIED_VOLUME.</param>
        public int iForce(string symbol, ENUM_TIMEFRAMES period, int maPeriod, ENUM_MA_METHOD maMethod, ENUM_APPLIED_VOLUME appliedVolume)
        {
            var commandParameters = new ArrayList { symbol, (int)period, maPeriod, (int)maMethod, (int)appliedVolume };
            return SendCommand<int>(Mt5CommandType.iForce, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Force Index indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        public int iForce(string symbol, ENUM_TIMEFRAMES period)
        {
            var commandParameters = new ArrayList { symbol, (int)period };
            return SendCommand<int>(Mt5CommandType.iForce, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Fractal Adaptive Moving Average indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="maPeriod">Period (bars count) for the indicator calculations.</param>
        ///<param name="maShift">Shift of the indicator in the price chart.</param>
        ///<param name="appliedPrice">The price used. Can be any of the price constants ENUM_APPLIED_PRICE or a handle of another indicator.</param>
        public int iFrAMA(string symbol, ENUM_TIMEFRAMES period, int maPeriod, int maShift, ENUM_APPLIED_PRICE appliedPrice)
        {
            var commandParameters = new ArrayList { symbol, (int)period, maPeriod, maShift, (int)appliedPrice };
            return SendCommand<int>(Mt5CommandType.iFrAMA, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Gator indicator. The Oscillator shows the difference between the blue and red lines of Alligator (upper histogram) and difference between red and green lines (lower histogram).
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="jawPeriod">Averaging period for the blue line (Alligator's Jaw).</param>
        ///<param name="jawShift">The shift of the blue line relative to the price chart. It isn't directly connected with the visual shift of the indicator histogram.</param>
        ///<param name="teethPeriod">Averaging period for the red line (Alligator's Teeth).</param>
        ///<param name="teethShift">The shift of the red line relative to the price chart. It isn't directly connected with the visual shift of the indicator histogram.</param>
        ///<param name="lipsPeriod">Averaging period for the green line (Alligator's lips).</param>
        ///<param name="lipsShift">The shift of the green line relative to the price charts. It isn't directly connected with the visual shift of the indicator histogram.</param>
        ///<param name="maMethod">Smoothing type. Can be one of the values of ENUM_MA_METHOD.</param>
        ///<param name="appliedPrice">The price used. Can be any of the price constants ENUM_APPLIED_PRICE or a handle of another indicator.</param>
        public int iGator(string symbol, ENUM_TIMEFRAMES period, int jawPeriod, int jawShift, int teethPeriod, 
            int teethShift, int lipsPeriod, int lipsShift, ENUM_MA_METHOD maMethod, ENUM_APPLIED_PRICE appliedPrice)
        {
            var commandParameters = new ArrayList { symbol, (int)period, jawPeriod, jawShift, teethPeriod, teethShift, lipsPeriod, lipsShift, (int)maMethod, (int)appliedPrice };
            return SendCommand<int>(Mt5CommandType.iGator, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Ichimoku Kinko Hyo indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="tenkanSen">Averaging period for Tenkan Sen.</param>
        ///<param name="kijunSen">Averaging period for Kijun Sen.</param>
        ///<param name="senkouSpanB">Averaging period for Senkou Span B.</param>
        public int iIchimoku(string symbol, ENUM_TIMEFRAMES period, int tenkanSen, int kijunSen, int senkouSpanB)
        {
            var commandParameters = new ArrayList { symbol, (int)period, tenkanSen, kijunSen, senkouSpanB };
            return SendCommand<int>(Mt5CommandType.iIchimoku, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Market Facilitation Index indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="appliedVolume">The volume used. Can be one of the constants of ENUM_APPLIED_VOLUME.</param>
        public int iBWMFI(string symbol, ENUM_TIMEFRAMES period, ENUM_APPLIED_VOLUME appliedVolume)
        {
            var commandParameters = new ArrayList { symbol, (int)period, (int)appliedVolume };
            return SendCommand<int>(Mt5CommandType.iBWMFI, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Momentum indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="momPeriod">Averaging period (bars count) for the calculation of the price change.</param>
        ///<param name="appliedPrice">The price used. Can be any of the price constants ENUM_APPLIED_PRICE or a handle of another indicator.</param>
        public int iMomentum(string symbol, ENUM_TIMEFRAMES period, int momPeriod, ENUM_APPLIED_PRICE appliedPrice)
        {
            var commandParameters = new ArrayList { symbol, (int)period, momPeriod, (int)appliedPrice };
            return SendCommand<int>(Mt5CommandType.iMomentum, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Money Flow Index indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="maPeriod">Averaging period (bars count) for the calculation.</param>
        ///<param name="appliedVolume">The volume used. Can be any of the ENUM_APPLIED_VOLUME values.</param>
        public int iMFI(string symbol, ENUM_TIMEFRAMES period, int maPeriod, ENUM_APPLIED_VOLUME appliedVolume)
        {
            var commandParameters = new ArrayList { symbol, (int)period, maPeriod, (int)appliedVolume };
            return SendCommand<int>(Mt5CommandType.iMFI, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Moving Average indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="maPeriod">Averaging period for the calculation of the moving average.</param>
        ///<param name="maShift">Shift of the indicator relative to the price chart.</param>
        ///<param name="maMethod">Smoothing type. Can be one of the ENUM_MA_METHOD values.</param>
        ///<param name="appliedPrice">The price used. Can be any of the price constants ENUM_APPLIED_PRICE or a handle of another indicator.</param>
        public int iMA(string symbol, ENUM_TIMEFRAMES period, int maPeriod, int maShift, ENUM_MA_METHOD maMethod, ENUM_APPLIED_PRICE appliedPrice)
        {
            var commandParameters = new ArrayList { symbol, (int)period, maPeriod, maShift, (int)maMethod, (int)appliedPrice };
            return SendCommand<int>(Mt5CommandType.iMA, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Moving Average of Oscillator indicator. The OsMA oscillator shows the difference between values of MACD and its signal line. 
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="fastEmaPeriod">Period for Fast Moving Average calculation.</param>
        ///<param name="slowEmaPeriod">Period for Slow Moving Average calculation.</param>
        ///<param name="signalPeriod">Averaging period for signal line calculation.</param>
        ///<param name="appliedPrice">The price used. Can be any of the price constants ENUM_APPLIED_PRICE or a handle of another indicator.</param>
        public int iOsMA(string symbol, ENUM_TIMEFRAMES period, int fastEmaPeriod, int slowEmaPeriod, int signalPeriod, ENUM_APPLIED_PRICE appliedPrice)
        {
            var commandParameters = new ArrayList { symbol, (int)period, fastEmaPeriod, slowEmaPeriod, signalPeriod, (int)appliedPrice };
            return SendCommand<int>(Mt5CommandType.iOsMA, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Moving Averages Convergence/Divergence indicator. In systems where OsMA is called MACD Histogram, this indicator is shown as two lines. In the client terminal the Moving Averages Convergence/Divergence looks like a histogram.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="fastEmaPeriod">Period for Fast Moving Average calculation.</param>
        ///<param name="slowEmaPeriod">Period for Slow Moving Average calculation.</param>
        ///<param name="signalPeriod">Averaging period for signal line calculation.</param>
        ///<param name="appliedPrice">The price used. Can be any of the price constants ENUM_APPLIED_PRICE or a handle of another indicator.</param>
        public int iMACD(string symbol, ENUM_TIMEFRAMES period, int fastEmaPeriod, int slowEmaPeriod, int signalPeriod, ENUM_APPLIED_PRICE appliedPrice)
        {
            var commandParameters = new ArrayList { symbol, (int)period, fastEmaPeriod, slowEmaPeriod, signalPeriod, (int)appliedPrice };
            return SendCommand<int>(Mt5CommandType.iMACD, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the On Balance Volume indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="appliedVolume">The volume used. Can be any of the ENUM_APPLIED_VOLUME values.</param>
        public int iOBV(string symbol, ENUM_TIMEFRAMES period, ENUM_APPLIED_VOLUME appliedVolume)
        {
            var commandParameters = new ArrayList { symbol, (int)period, (int)appliedVolume };
            return SendCommand<int>(Mt5CommandType.iOBV, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Parabolic Stop and Reverse system indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="step">The step of price increment, usually  0.02.</param>
        ///<param name="maximum">The maximum step, usually 0.2.</param>
        public int iSAR(string symbol, ENUM_TIMEFRAMES period, double step, double maximum)
        {
            var commandParameters = new ArrayList { symbol, (int)period, step, maximum };
            return SendCommand<int>(Mt5CommandType.iSAR, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Relative Strength Index indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="maPeriod">Averaging period for the RSI calculation.</param>
        ///<param name="appliedPrice">The price used. Can be any of the price constants ENUM_APPLIED_PRICE or a handle of another indicator.</param>
        public int iRSI(string symbol, ENUM_TIMEFRAMES period, int maPeriod, ENUM_APPLIED_PRICE appliedPrice)
        {
            var commandParameters = new ArrayList { symbol, (int)period, maPeriod, (int)appliedPrice };
            return SendCommand<int>(Mt5CommandType.iRSI, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Relative Vigor Index indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="maPeriod">Averaging period for the RVI calculation.</param>
        public int iRVI(string symbol, ENUM_TIMEFRAMES period, int maPeriod)
        {
            var commandParameters = new ArrayList { symbol, (int)period, maPeriod };
            return SendCommand<int>(Mt5CommandType.iRVI, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Standard Deviation indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="maPeriod">Averaging period for the RVI calculation.</param>
        ///<param name="maShift">Shift of the indicator relative to the price chart.</param>
        ///<param name="maMethod">Type of averaging. Can be any of the ENUM_MA_METHOD values.</param>
        ///<param name="appliedPrice">The price used. Can be any of the price constants ENUM_APPLIED_PRICE or a handle of another indicator.</param>
        public int iStdDev(string symbol, ENUM_TIMEFRAMES period, int maPeriod, int maShift, ENUM_MA_METHOD maMethod, ENUM_APPLIED_PRICE appliedPrice)
        {
            var commandParameters = new ArrayList { symbol, (int)period, maPeriod, maShift, (int)maMethod, (int)appliedPrice };
            return SendCommand<int>(Mt5CommandType.iStdDev, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Stochastic Oscillator indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="Kperiod">Averaging period (bars count) for the %K line calculation.</param>
        ///<param name="Dperiod">Averaging period (bars count) for the %D line calculation.</param>
        ///<param name="slowing">Slowing value.</param>
        ///<param name="maMethod">Type of averaging. Can be any of the ENUM_MA_METHOD values.</param>
        ///<param name="priceField">Parameter of price selection for calculations. Can be one of the ENUM_STO_PRICE values.</param>
        public int iStochastic(string symbol, ENUM_TIMEFRAMES period, int Kperiod, int Dperiod, int slowing, ENUM_MA_METHOD maMethod, ENUM_STO_PRICE priceField)
        {
            var commandParameters = new ArrayList { symbol, (int)period, Kperiod, Dperiod, slowing, (int)maMethod, (int)priceField };
            return SendCommand<int>(Mt5CommandType.iStochastic, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Triple Exponential Moving Average indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="maPeriod">Averaging period (bars count) for calculation.</param>
        ///<param name="maShift">Shift of indicator relative to the price chart.</param>
        ///<param name="appliedPrice">The price used. Can be any of the price constants ENUM_APPLIED_PRICE or a handle of another indicator.</param>
        public int iTEMA(string symbol, ENUM_TIMEFRAMES period, int maPeriod, int maShift, ENUM_APPLIED_PRICE appliedPrice)
        {
            var commandParameters = new ArrayList { symbol, (int)period, maPeriod, maShift, (int)appliedPrice };
            return SendCommand<int>(Mt5CommandType.iTEMA, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Triple Exponential Moving Averages Oscillator indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="maPeriod">Averaging period (bars count) for calculation.</param>
        ///<param name="appliedPrice">The price used. Can be any of the price constants ENUM_APPLIED_PRICE or a handle of another indicator.</param>
        public int iTriX(string symbol, ENUM_TIMEFRAMES period, int maPeriod, ENUM_APPLIED_PRICE appliedPrice)
        {
            var commandParameters = new ArrayList { symbol, (int)period, maPeriod, (int)appliedPrice };
            return SendCommand<int>(Mt5CommandType.iTriX, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Larry Williams' Percent Range indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="calcPeriod">Period (bars count) for the indicator calculation.</param>
        public int iWPR(string symbol, ENUM_TIMEFRAMES period, int calcPeriod)
        {
            var commandParameters = new ArrayList { symbol, (int)period, calcPeriod };
            return SendCommand<int>(Mt5CommandType.iWPR, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Variable Index Dynamic Average indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="cmoPeriod">Period (bars count) for the Chande Momentum Oscillator calculation.</param>
        ///<param name="emaPeriod">EMA period (bars count) for smoothing factor calculation.</param>
        ///<param name="maShift">Shift of the indicator relative to the price chart.</param>
        ///<param name="appliedPrice">The price used. Can be any of the price constants ENUM_APPLIED_PRICE or a handle of another indicator.</param>
        public int iVIDyA(string symbol, ENUM_TIMEFRAMES period, int cmoPeriod, int emaPeriod, int maShift, ENUM_APPLIED_PRICE appliedPrice)
        {
            var commandParameters = new ArrayList { symbol, (int)period, cmoPeriod, emaPeriod, maShift, (int)appliedPrice };
            return SendCommand<int>(Mt5CommandType.iVIDyA, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Volumes indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="appliedVolume">The volume used. Can be any of the ENUM_APPLIED_VOLUME values.</param>
        public int iVolumes(string symbol, ENUM_TIMEFRAMES period, ENUM_APPLIED_VOLUME appliedVolume)
        {
            var commandParameters = new ArrayList { symbol, (int)period, (int)appliedVolume };
            return SendCommand<int>(Mt5CommandType.iVolumes, commandParameters);
        }

        ///<summary>
        ///The function returns the handle of the Volumes indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="name">The name of the custom indicator, with path relative to the root directory of indicators (MQL5/Indicators/). If an indicator is located in a subdirectory, for example, in MQL5/Indicators/Examples, its name must be specified like: "Examples\\indicator_name" (it is necessary to use a double slash instead of the single slash as a separator).</param>
        ///<param name="parameters">input-parameters of a custom indicator. If there is no parameters specified, then default values will be used.</param>
        public int iCustom(string symbol, ENUM_TIMEFRAMES period, string name, double[] parameters)
        {
            Log.Debug("iCustom: called.");
            var response = SendRequest<int>(new ICustomRequest
            {
                Symbol = symbol,
                Timeframe = (int)period,
                Name = name,
                Params = new ArrayList(parameters),
                ParamsType = ICustomRequest.ParametersType.Double
            });
            Log.Debug($"iCustom: response = {response}.");
            return response;
        }

        ///<summary>
        ///The function returns the handle of the Volumes indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="name">The name of the custom indicator, with path relative to the root directory of indicators (MQL5/Indicators/). If an indicator is located in a subdirectory, for example, in MQL5/Indicators/Examples, its name must be specified like: "Examples\\indicator_name" (it is necessary to use a double slash instead of the single slash as a separator).</param>
        ///<param name="parameters">input-parameters of a custom indicator. If there is no parameters specified, then default values will be used.</param>
        public int iCustom(string symbol, ENUM_TIMEFRAMES period, string name, int[] parameters)
        {
            Log.Debug("iCustom: called.");
            var response = SendRequest<int>(new ICustomRequest
            {
                Symbol = symbol,
                Timeframe = (int)period,
                Name = name,
                Params = new ArrayList(parameters),
                ParamsType = ICustomRequest.ParametersType.Int
            });
            Log.Debug($"iCustom: response = {response}.");
            return response;
        }

        ///<summary>
        ///The function returns the handle of the Volumes indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="name">The name of the custom indicator, with path relative to the root directory of indicators (MQL5/Indicators/). If an indicator is located in a subdirectory, for example, in MQL5/Indicators/Examples, its name must be specified like: "Examples\\indicator_name" (it is necessary to use a double slash instead of the single slash as a separator).</param>
        ///<param name="parameters">input-parameters of a custom indicator. If there is no parameters specified, then default values will be used.</param>
        public int iCustom(string symbol, ENUM_TIMEFRAMES period, string name, string[] parameters)
        {
            Log.Debug("iCustom: called.");
            var response = SendRequest<int>(new ICustomRequest
            {
                Symbol = symbol,
                Timeframe = (int)period,
                Name = name,
                Params = new ArrayList(parameters),
                ParamsType = ICustomRequest.ParametersType.Int
            });
            Log.Debug($"iCustom: response = {response}.");
            return response;
        }

        ///<summary>
        ///The function returns the handle of the Volumes indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="name">The name of the custom indicator, with path relative to the root directory of indicators (MQL5/Indicators/). If an indicator is located in a subdirectory, for example, in MQL5/Indicators/Examples, its name must be specified like: "Examples\\indicator_name" (it is necessary to use a double slash instead of the single slash as a separator).</param>
        ///<param name="parameters">input-parameters of a custom indicator. If there is no parameters specified, then default values will be used.</param>
        public int iCustom(string symbol, ENUM_TIMEFRAMES period, string name, bool[] parameters)
        {
            Log.Debug("iCustom: called.");
            var response = SendRequest<int>(new ICustomRequest
            {
                Symbol = symbol,
                Timeframe = (int)period,
                Name = name,
                Params = new ArrayList(parameters),
                ParamsType = ICustomRequest.ParametersType.Int
            });
            Log.Debug($"iCustom: response = {response}.");
            return response;
        }

        #endregion //Technical Indicators

        #region Date and Time

        ///<summary>
        ///Returns the last known server time, time of the last quote receipt for one of the symbols selected in the "Market Watch" window.
        ///</summary>
        public DateTime TimeCurrent()
        {
            Log.Debug("TimeCurrent: called.");
            var response = SendCommand<long>(Mt5CommandType.TimeCurrent, null);
            Log.Debug($"TimeCurrent: response = {response}.");
            return Mt5TimeConverter.ConvertFromMtTime(response);
        }

        ///<summary>
        ///Returns the calculated current time of the trade server. Unlike TimeCurrent(), the calculation of the time value is performed in the client terminal and depends on the time settings on your computer.
        ///</summary>
        public DateTime TimeTradeServer()
        {
            Log.Debug("TimeTradeServer: called.");
            var response = SendCommand<long>(Mt5CommandType.TimeTradeServer, null);
            Log.Debug($"TimeTradeServer: response = {response}.");
            return Mt5TimeConverter.ConvertFromMtTime(response);
        }

        ///<summary>
        ///Returns the local time of a computer, where the client terminal is running.
        ///</summary>
        public DateTime TimeLocal()
        {
            Log.Debug("TimeLocal: called.");
            var response = SendCommand<long>(Mt5CommandType.TimeLocal, null);
            Log.Debug($"TimeLocal: response = {response}.");
            return Mt5TimeConverter.ConvertFromMtTime(response);
        }

        ///<summary>
        ///Returns the GMT, which is calculated taking into account the DST switch by the local time on the computer where the client terminal is running.
        ///</summary>
        public DateTime TimeGMT()
        {
            Log.Debug("TimeGMT: called.");
            var response = SendCommand<long>(Mt5CommandType.TimeGMT, null);
            Log.Debug($"TimeGMT: response = {response}.");
            return Mt5TimeConverter.ConvertFromMtTime(response);
        }

        #endregion //Date and Time

        #region Checkup

        ///<summary>
        ///Returns the value of the last error that occurred during the execution of an mql5 program.
        ///</summary>
        public int GetLastError()
        {
            return SendCommand<int>(Mt5CommandType.GetLastError, null);
        }

        ///<summary>
        ///Sets the value of the predefined variable _LastError into zero.
        ///</summary>
        public void ResetLastError()
        {
            SendCommand<object>(Mt5CommandType.ResetLastError, null);
        }

        #endregion

        #endregion // Public Methods

        #region Properties
        ///<summary>
        ///Connection status of MetaTrader API.
        ///</summary>
        public Mt5ConnectionState ConnectionState
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

        #region Events
        public event QuoteHandler QuoteUpdated;
        public event EventHandler<Mt5QuoteEventArgs> QuoteUpdate;
        public event EventHandler<Mt5QuoteEventArgs> QuoteAdded;
        public event EventHandler<Mt5QuoteEventArgs> QuoteRemoved;
        public event EventHandler<Mt5ConnectionEventArgs> ConnectionStateChanged;
        public event EventHandler<Mt5TradeTransactionEventArgs> OnTradeTransaction;
        public event EventHandler<Mt5BookEventArgs> OnBookEvent;
        #endregion

        #region Private Methods
        private MtClient Client
        {
            get
            {
                lock (_locker)
                {
                    return _client;
                }
            }
        }

        private void Connect(MtClient client)
        {
            lock (_locker)
            {
                if (_connectionState == Mt5ConnectionState.Connected
                    || _connectionState == Mt5ConnectionState.Connecting)
                {
                    return;
                }

                _connectionState = Mt5ConnectionState.Connecting;
            }

            string message = string.IsNullOrEmpty(client.Host) ? $"Connecting to localhost:{client.Port}" : $"Connecting to {client.Host}:{client.Port}";
            ConnectionStateChanged?.Invoke(this, new Mt5ConnectionEventArgs(Mt5ConnectionState.Connecting, message));

            var state = Mt5ConnectionState.Failed;

            lock (_locker)
            {
                try
                {
                    client.Connect();
                    state = Mt5ConnectionState.Connected;
                }
                catch (Exception e)
                {
                    client.Dispose();
                    message = string.IsNullOrEmpty(client.Host) ? $"Failed connection to localhost:{client.Port}. {e.Message}" : $"Failed connection to {client.Host}:{client.Port}. {e.Message}";
                }

                if (state == Mt5ConnectionState.Connected)
                {
                    _client = client;
                    _client.QuoteAdded += _client_QuoteAdded;
                    _client.QuoteRemoved += _client_QuoteRemoved;
                    _client.QuoteUpdated += _client_QuoteUpdated;
                    _client.ServerDisconnected += _client_ServerDisconnected;
                    _client.ServerFailed += _client_ServerFailed;
                    _client.MtEventReceived += _client_MtEventReceived;
                    message = string.IsNullOrEmpty(client.Host) ? $"Connected to localhost:{client.Port}" : $"Connected to  { client.Host}:{client.Port}";
                }

                _connectionState = state;
            }

            ConnectionStateChanged?.Invoke(this, new Mt5ConnectionEventArgs(state, message));

            if (state == Mt5ConnectionState.Connected)
            {
                OnConnected();
            }
        }

        private void _client_MtEventReceived(MtEvent e)
        {
            var eventType = (Mt5EventTypes)e.EventType;

            switch (eventType)
            {
                case Mt5EventTypes.OnTradeTransaction:
                    ReceivedOnTradeTransaction(e.ExpertHandle, e.Payload);
                    break;
                case Mt5EventTypes.OnBookEvent:
                    ReceivedOnBookEvent(e.ExpertHandle, e.Payload);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ReceivedOnTradeTransaction(int expertHandler, string payload)
        {
            var e = JsonConvert.DeserializeObject<OnTradeTransactionEvent>(payload);
            OnTradeTransaction?.Invoke(this, new Mt5TradeTransactionEventArgs
            {
                ExpertHandle = expertHandler,
                Trans = e.Trans,
                Request = e.Request,
                Result = e.Result
            });
        }

        private void ReceivedOnBookEvent(int expertHandler, string payload)
        {
            var e = JsonConvert.DeserializeObject<OnBookEvent>(payload);
            OnBookEvent?.Invoke(this, new Mt5BookEventArgs
            {
                ExpertHandle = expertHandler,
                Symbol = e.Symbol
            });
        }

        private void Connect(string host, int port)
        {
            var client = new MtClient(host, port);
            Connect(client);
        }

        private void Connect(int port)
        {
            var client = new MtClient(port);
            Connect(client);
        }

        private void Disconnect(bool failed)
        {
            var state = failed ? Mt5ConnectionState.Failed : Mt5ConnectionState.Disconnected;
            var message = failed ? "Connection Failed" : "Disconnected";

            lock (_locker)
            {
                if (_connectionState == Mt5ConnectionState.Disconnected
                    || _connectionState == Mt5ConnectionState.Failed)
                    return;

                if (_client != null)
                {
                    _client.QuoteAdded -= _client_QuoteAdded;
                    _client.QuoteRemoved -= _client_QuoteRemoved;
                    _client.QuoteUpdated -= _client_QuoteUpdated;
                    _client.ServerDisconnected -= _client_ServerDisconnected;
                    _client.ServerFailed -= _client_ServerFailed;
                    _client.MtEventReceived -= _client_MtEventReceived;

                    if (!failed)
                    {
                        _client.Disconnect();
                    }

                    _client.Dispose();

                    _client = null;
                }

                _connectionState = state;
            }

            ConnectionStateChanged?.Invoke(this, new Mt5ConnectionEventArgs(state, message));
        }

        private T SendCommand<T>(Mt5CommandType commandType, ArrayList commandParameters, Dictionary<string, object> namedParams = null)
        {
            MtResponse response;

            var client = Client;
            if (client == null)
            {
                throw new Exception("No connection");
            }

            try
            {
                response = client.SendCommand((int)commandType, commandParameters, namedParams, ExecutorHandle);
            }
            catch (CommunicationException ex)
            {
                throw new Exception(ex.Message, ex);
            }

            if (response == null)
            {
                throw new ExecutionException(ErrorCode.ErrCustom, "Response from MetaTrader is null");
            }

            if (response.ErrorCode != 0)
            {
                throw new ExecutionException((ErrorCode)response.ErrorCode, response.ToString());
            }

            var responseValue = response.GetValue();
            return (T) responseValue;
        }

        private T SendRequest<T>(RequestBase request)
        {
            if (request == null)
                return default(T);

            var serializer = JsonConvert.SerializeObject(request, Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });
            var commandParameters = new ArrayList { serializer };

            var res = SendCommand<string>(Mt5CommandType.MtRequest, commandParameters);

            if (res == null)
            {
                throw new ExecutionException(ErrorCode.ErrCustom, "Response from MetaTrader is null");
            }

            var response = JsonConvert.DeserializeObject<Response<T>>(res);
            if (response.ErrorCode != 0)
            {
                throw new ExecutionException((ErrorCode)response.ErrorCode, response.ErrorMessage);
            }

            return response.Value;
        }


        private void _client_QuoteUpdated(MtQuote quote)
        {
            if (quote == null) return;
            QuoteUpdate?.Invoke(this, new Mt5QuoteEventArgs(new Mt5Quote(quote)));
            QuoteUpdated?.Invoke(this, quote.Instrument, quote.Bid, quote.Ask);
        }

        private void _client_ServerDisconnected(object sender, EventArgs e)
        {
            Disconnect(false);
        }

        private void _client_ServerFailed(object sender, EventArgs e)
        {
            Disconnect(true);
        }

        private void _client_QuoteRemoved(MtQuote quote)
        {
            if (quote != null)
            {
                QuoteRemoved?.Invoke(this, new Mt5QuoteEventArgs(new Mt5Quote(quote)));
            }
        }

        private void _client_QuoteAdded(MtQuote quote)
        {
            if (quote != null)
            {
                QuoteAdded?.Invoke(this, new Mt5QuoteEventArgs(new Mt5Quote(quote)));
            }
        }

        private void OnConnected()
        {
            _isBacktestingMode = IsTesting();

            if (_isBacktestingMode)
            {
                BacktestingReady();
            }
        }

        private void BacktestingReady()
        {
            SendCommand<object>(Mt5CommandType.BacktestingReady, null);
        }
        #endregion
    }
}
