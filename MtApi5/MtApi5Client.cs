using System;
using System.Collections.Generic;
using System.Linq;
using MTApiService;
using System.Collections;
using System.ServiceModel;
using MtApi5.Requests;
using MtApi5.Responses;
using Newtonsoft.Json;

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
        private readonly MtClient _client = new MtClient();
        private volatile bool _isBacktestingMode = false;
        #endregion

        #region Public Methods
        public MtApi5Client()
        {
            LogConfigurator.Setup(LogProfileName);

            ConnectionState = Mt5ConnectionState.Disconnected;

            _client.QuoteAdded += mClient_QuoteAdded;
            _client.QuoteRemoved += mClient_QuoteRemoved;
            _client.QuoteUpdated += mClient_QuoteUpdated;
            _client.ServerDisconnected += mClient_ServerDisconnected;
            _client.ServerFailed += mClient_ServerFailed;
        }

        ///<summary>
        ///Connect with MetaTrader API. Async method.
        ///</summary>
        ///<param name="host">Address of MetaTrader host (ex. 192.168.1.2)</param>
        ///<param name="port">Port of host connection (default 8222) </param>
        public void BeginConnect(string host, int port)
        {
            //if (string.IsNullOrEmpty(host) == false && (host.Equals("localhost") || host.Equals("127.0.0.1")))
            //{
            //    this.BeginConnect(port);
            //}
            //else
            //{
                Action<string, int> connectAction = Connect;
                connectAction.BeginInvoke(host, port, null, null);
            //}
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
        public IEnumerable<Mt5Quote> GetQuotes()
        {
            IEnumerable<MtQuote> quotes;
            lock (_client)
            {
                quotes = _client.GetQuotes();
            }
            return quotes?.Select(q => q.Parse());
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
            if (request == null)
            {
                result = null;
                return false;
            }

            var commandParameters = request.ToArrayList();

            var strResult = SendCommand<string>(Mt5CommandType.OrderSend, commandParameters);

            return strResult.ParseResult(ParamSeparator, out result); 
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
            if (request == null)
            {
                result = null;
                return false;
            }

            var commandParameters = request.ToArrayList();

            var strResult = SendCommand<string>(Mt5CommandType.OrderSend, commandParameters);

            return strResult.ParseResult(ParamSeparator, out result); 
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
            var commandParameters = new ArrayList { ticketNumber, propertyId };

            return SendCommand<double>(Mt5CommandType.HistoryDealGetDouble, commandParameters);
        }

        ///<summary>
        ///Returns the requested property of a deal. 
        ///</summary>
        ///<param name="ticketNumber">Deal ticket.</param>
        ///<param name="propertyId"> Identifier of a deal property.</param>
        public long HistoryDealGetInteger(ulong ticketNumber, ENUM_DEAL_PROPERTY_INTEGER propertyId)
        {
            var commandParameters = new ArrayList { ticketNumber, propertyId };

            return SendCommand<long>(Mt5CommandType.HistoryDealGetInteger, commandParameters);
        }

        ///<summary>
        ///Returns the requested property of a deal. 
        ///</summary>
        ///<param name="ticketNumber">Deal ticket.</param>
        ///<param name="propertyId"> Identifier of a deal property.</param>
        public string HistoryDealGetString(ulong ticketNumber, ENUM_DEAL_PROPERTY_STRING propertyId)
        {
            var commandParameters = new ArrayList { ticketNumber, propertyId };

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
        public bool PositionClose(int ticket)
        {
            var commandParameters = new ArrayList { ticket};

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
        public int SeriesInfoInteger(string symbolName, ENUM_TIMEFRAMES timeframe, ENUM_SERIES_INFO_INTEGER propId)
        {
            var commandParameters = new ArrayList { symbolName, (int)timeframe, (int)propId };

            return SendCommand<int>(Mt5CommandType.SeriesInfoInteger, commandParameters);
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
            var response = SendRequest<CopyTicksResponse>(new CopyTicksRequest
            {
                SymbolName = symbolName, Flags = (int)flags, From = from, Count = count
            });
            return response.Ticks;
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
            var commandParameters = new ArrayList { symbol };

            var retVal = SendCommand<MtMqlBookInfo[]>(Mt5CommandType.MarketBookGet, commandParameters);

            book = null; 
            if (retVal != null)
            {
                book = new MqlBookInfo[retVal.Length];

                foreach (var t in retVal)
                {
                    book[0] = new MqlBookInfo((ENUM_BOOK_TYPE)t.type, t.price, t.volume);
                }
            }

            return book != null;
        }

        #endregion

        #region Common Functions
        ///<summary>
        ///It enters a message in the Expert Advisor log.
        ///</summary>
        ///<param name="message">Symbol name.</param>
        public bool Print(string message)
        {
            var commandParameters = new ArrayList { message };

            return SendCommand<bool>(Mt5CommandType.Print, commandParameters);
        }
        #endregion

        #endregion

        #region Properties
        ///<summary>
        ///Connection status of MetaTrader API.
        ///</summary>
        public Mt5ConnectionState ConnectionState { get; private set; }
        #endregion

        #region Events
        public event QuoteHandler QuoteUpdated;
        public event EventHandler<Mt5QuoteEventArgs> QuoteAdded;
        public event EventHandler<Mt5QuoteEventArgs> QuoteRemoved;
        public event EventHandler<Mt5ConnectionEventArgs> ConnectionStateChanged;
        #endregion

        #region Private Methods
        private void Connect(string host, int port)
        {
            ConnectionState = Mt5ConnectionState.Connecting;
            ConnectionStateChanged.FireEvent(this
                , new Mt5ConnectionEventArgs(Mt5ConnectionState.Connecting, "Connecting to " + host + ":" + port));

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
                ConnectionState = Mt5ConnectionState.Failed;
                ConnectionStateChanged.FireEvent(this
                    , new Mt5ConnectionEventArgs(Mt5ConnectionState.Failed, "Failed connection to " + host + ":" + port + ". " + e.Message));
                return;
            }

            ConnectionState = Mt5ConnectionState.Connected;
            ConnectionStateChanged.FireEvent(this
                , new Mt5ConnectionEventArgs(Mt5ConnectionState.Connected, "Connected  to " + host + ":" + port));

            OnConnected();
        }

        private void Connect(int port)
        {
            ConnectionState = Mt5ConnectionState.Connecting;
            ConnectionStateChanged.FireEvent(this
                , new Mt5ConnectionEventArgs(Mt5ConnectionState.Connecting, "Connecting to 'localhost':" + port));

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
                ConnectionState = Mt5ConnectionState.Failed;
                ConnectionStateChanged.FireEvent(this
                    , new Mt5ConnectionEventArgs(Mt5ConnectionState.Failed, "Failed connection  to 'localhost':" + port + ". " + e.Message));

                return;
            }

            ConnectionState = Mt5ConnectionState.Connected;
            ConnectionStateChanged.FireEvent(this
                , new Mt5ConnectionEventArgs(Mt5ConnectionState.Connected, "Connected to 'localhost':" + port));

            OnConnected();
        }

        private void Disconnect()
        {
            lock (_client)
            {
                _client.Disconnect();
                _client.Close();
            }

            ConnectionState = Mt5ConnectionState.Disconnected;
            ConnectionStateChanged.FireEvent(this, new Mt5ConnectionEventArgs(Mt5ConnectionState.Disconnected, "Disconnected"));
        }

        private T SendCommand<T>(Mt5CommandType commandType, ArrayList commandParameters)
        {
            MtResponse response;
            try
            {
                lock (_client)
                {
                    response = _client.SendCommand((int) commandType, commandParameters);
                }
            }
            catch (CommunicationException ex)
            {
                throw new Exception(ex.Message, ex);
            }

            var responseValue = response?.GetValue();
            return responseValue != null ? (T) responseValue : default(T);
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
                    res = (MtResponseString)_client.SendCommand((int)Mt5CommandType.MtRequest, commandParameters);
                }
            }
            catch (CommunicationException ex)
            {
                throw new Exception(ex.Message, ex);
            }

            if (res == null)
            {
                throw new ExecutionException(ErrorCode.ErrCustom, "Response from MetaTrader is null");
            }

            var response = JsonConvert.DeserializeObject<T>(res.Value);
            if (response.ErrorCode != 0)
            {
                throw new ExecutionException((ErrorCode)response.ErrorCode, response.ErrorMessage);
            }

            return response;
        }


        private void mClient_QuoteUpdated(MtQuote quote)
        {
            if (quote != null)
            {
                QuoteUpdated?.Invoke(this, quote.Instrument, quote.Bid, quote.Ask);
            }
        }

        private void mClient_ServerDisconnected(object sender, EventArgs e)
        {
            ConnectionState = Mt5ConnectionState.Disconnected;
            ConnectionStateChanged.FireEvent(this
                , new Mt5ConnectionEventArgs(Mt5ConnectionState.Disconnected, "MtApi is disconnected"));
        }

        private void mClient_ServerFailed(object sender, EventArgs e)
        {
            ConnectionState = Mt5ConnectionState.Failed;
            ConnectionStateChanged.FireEvent(this
                , new Mt5ConnectionEventArgs(Mt5ConnectionState.Failed, "Failed connection with MtApi"));
        }

        private void mClient_QuoteRemoved(MtQuote quote)
        {
            QuoteRemoved.FireEvent(this, new Mt5QuoteEventArgs(quote.Parse()));
        }

        private void mClient_QuoteAdded(MtQuote quote)
        {
            QuoteAdded.FireEvent(this, new Mt5QuoteEventArgs(quote.Parse()));
        }

        private void OnConnected()
        {
            // INFO: disabled backtesting mode while solution of window handle in testing mode is not found
            //_isBacktestingMode = IsTesting();

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
