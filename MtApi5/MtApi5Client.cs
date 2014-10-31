using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTApiService;
using System.Collections;

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

        private const char PARAM_SEPARATOR = ';';

        public delegate void QuoteHandler(object sender, string symbol, double bid, double ask);

        #region Public Methods
        public MtApi5Client()
        {
            ConnectionState = Mt5ConnectionState.Disconnected;

            mClient.QuoteAdded += new MtClient.MtQuoteHandler(mClient_QuoteAdded);
            mClient.QuoteRemoved += new MtClient.MtQuoteHandler(mClient_QuoteRemoved);
            mClient.QuoteUpdated += new MtClient.MtQuoteHandler(mClient_QuoteUpdated);
            mClient.ServerDisconnected += new EventHandler(mClient_ServerDisconnected);
            mClient.ServerFailed += new EventHandler(mClient_ServerFailed);
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
            var quotes = mClient.GetQuotes();
            return quotes != null ? (from q in quotes select q.Parse()) : null;
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

            string strResult = sendCommand<string>(Mt5CommandType.OrderSend, commandParameters);

            return strResult.ParseResult(PARAM_SEPARATOR, out result); 
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

            string strResult = sendCommand<string>(Mt5CommandType.OrderCalcMargin, commandParameters);

            return strResult.ParseResult(PARAM_SEPARATOR, out margin);
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
        ///<param name="price_open">Open price.</param>
        ///<param name="price_close">Close price.</param>
        ///<param name="profit">The variable, to which the calculated value of the profit will be written in case the function is successfully executed. 
        ///The estimated profit value depends on many factors, and can differ in different market environments.</param>
        /// <returns>
        /// The function returns true in case of success; otherwise it returns false. If an invalid order type is specified, the function will return false.
        /// </returns>
        public bool OrderCalcProfit(ENUM_ORDER_TYPE action, string symbol, double volume, double price_open, double price_close, out double profit)
        {
            var commandParameters = new ArrayList { (int)action, symbol, volume, price_open, price_close };

            string strResult = sendCommand<string>(Mt5CommandType.OrderCalcProfit, commandParameters);

            return strResult.ParseResult(PARAM_SEPARATOR, out profit);
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

            string strResult = sendCommand<string>(Mt5CommandType.OrderSend, commandParameters);

            return strResult.ParseResult(PARAM_SEPARATOR, out result); 
        }

        ///<summary>
        ///Returns the number of open positions.
        ///</summary>
        public int PositionsTotal()
        {
            return sendCommand<int>(Mt5CommandType.PositionsTotal, null);
        }

        ///<summary>
        ///Returns the symbol corresponding to the open position and automatically selects the position for further working with it using functions PositionGetDouble, PositionGetInteger, PositionGetString.
        ///</summary>
        ///<param name="index">Number of the position in the list of open positions.</param>
        public string PositionGetSymbol(int index)
        {
            var commandParameters = new ArrayList { index };

            return sendCommand<string>(Mt5CommandType.PositionGetSymbol, commandParameters);
        }

        ///<summary>
        ///Chooses an open position for further working with it. Returns true if the function is successfully completed. Returns false in case of failure.
        ///</summary>
        ///<param name="symbol">Name of the financial security.</param>
        public bool PositionSelect(string symbol)
        {
            var commandParameters = new ArrayList { symbol };

            return sendCommand<bool>(Mt5CommandType.PositionSelect, commandParameters);
        }

        ///<summary>
        ///The function returns the requested property of an open position, pre-selected using PositionGetSymbol or PositionSelect.
        ///</summary>
        ///<param name="property_id">Identifier of a position property.</param>
        public double PositionGetDouble(ENUM_POSITION_PROPERTY_DOUBLE property_id)
        {
            var commandParameters = new ArrayList { (int)property_id };

            return sendCommand<double>(Mt5CommandType.PositionGetDouble, commandParameters);
        }

        ///<summary>
        ///The function returns the requested property of an open position, pre-selected using PositionGetSymbol or PositionSelect.
        ///</summary>
        ///<param name="property_id">Identifier of a position property.</param>
        public long PositionGetInteger(ENUM_POSITION_PROPERTY_INTEGER property_id)
        {
            var commandParameters = new ArrayList { (int)property_id };

            return sendCommand<long>(Mt5CommandType.PositionGetInteger, commandParameters);
        }

        ///<summary>
        ///The function returns the requested property of an open position, pre-selected using PositionGetSymbol or PositionSelect.
        ///</summary>
        ///<param name="property_id">Identifier of a position property.</param>
        public string PositionGetString(ENUM_POSITION_PROPERTY_STRING property_id)
        {
            var commandParameters = new ArrayList { (int)property_id };

            return sendCommand<string>(Mt5CommandType.PositionGetString, commandParameters);
        }

        ///<summary>
        ///Returns the number of current orders.
        ///</summary>
        public int OrdersTotal()
        {
            return sendCommand<int>(Mt5CommandType.OrdersTotal, null);
        }

        ///<summary>
        ///Returns the number of current orders.
        ///</summary>
        ///<param name="index">Number of an order in the list of current orders.</param>
        public ulong OrderGetTicket(int index)
        {
            var commandParameters = new ArrayList { index };

            return sendCommand<ulong>(Mt5CommandType.OrderGetTicket, commandParameters);
        }

        ///<summary>
        ///Selects an order to work with. Returns true if the function has been successfully completed. 
        ///</summary>
        ///<param name="ticket">Order ticket.</param>
        public bool OrderSelect(ulong ticket)
        {
            var commandParameters = new ArrayList { ticket };

            return sendCommand<bool>(Mt5CommandType.OrderSelect, commandParameters);
        }

        ///<summary>
        ///Returns the requested property of an order, pre-selected using OrderGetTicket or OrderSelect.
        ///</summary>
        ///<param name="property_id"> Identifier of the order property.</param>
        public double OrderGetDouble(ENUM_ORDER_PROPERTY_DOUBLE property_id)
        {
            var commandParameters = new ArrayList { (int)property_id };

            return sendCommand<double>(Mt5CommandType.OrderGetDouble, commandParameters);
        }

        ///<summary>
        ///Returns the requested property of an order, pre-selected using OrderGetTicket or OrderSelect.
        ///</summary>
        ///<param name="property_id"> Identifier of the order property.</param>
        public long OrderGetInteger(ENUM_ORDER_PROPERTY_INTEGER property_id)
        {
            var commandParameters = new ArrayList { (int)property_id };

            return sendCommand<long>(Mt5CommandType.OrderGetInteger, commandParameters);
        }

        ///<summary>
        ///Returns the requested property of an order, pre-selected using OrderGetTicket or OrderSelect.
        ///</summary>
        ///<param name="property_id"> Identifier of the order property.</param>
        public string OrderGetString(ENUM_ORDER_PROPERTY_STRING property_id)
        {
            var commandParameters = new ArrayList { (int)property_id };

            return sendCommand<string>(Mt5CommandType.OrderGetString, commandParameters);
        }

        ///<summary>
        ///Retrieves the history of deals and orders for the specified period of server time.
        ///</summary>
        ///<param name="from_date">Start date of the request.</param>
        ///<param name="to_date">End date of the request.</param>
        public bool HistorySelect(DateTime from_date, DateTime to_date)
        {
            var commandParameters = new ArrayList { Mt5TimeConverter.ConvertToMtTime(from_date), Mt5TimeConverter.ConvertToMtTime(to_date) };

            return sendCommand<bool>(Mt5CommandType.HistorySelect, commandParameters);
        }

        ///<summary>
        ///Retrieves the history of deals and orders having the specified position identifier.
        ///</summary>
        ///<param name="position_id">Position identifier that is set to every executed order and every deal.</param>
        public bool HistorySelectByPosition(long position_id)
        {
            var commandParameters = new ArrayList { position_id };

            return sendCommand<bool>(Mt5CommandType.HistorySelectByPosition, commandParameters);
        }

        ///<summary>
        ///Selects an order from the history for further calling it through appropriate functions. 
        ///</summary>
        ///<param name="ticket">Order ticket.</param>
        public bool HistoryOrderSelect(ulong ticket)
        {
            var commandParameters = new ArrayList { ticket };

            return sendCommand<bool>(Mt5CommandType.HistoryOrderSelect, commandParameters);
        }

        ///<summary>
        ///Returns the number of orders in the history.
        ///</summary>
        public int HistoryOrdersTotal()
        {
            return sendCommand<int>(Mt5CommandType.HistoryOrdersTotal, null);
        }

        ///<summary>
        ///Return the ticket of a corresponding order in the history.
        ///</summary>
        ///<param name="index">Number of the order in the list of orders.</param>
        public ulong HistoryOrderGetTicket(int index)
        {
            var commandParameters = new ArrayList { index };

            return sendCommand<ulong>(Mt5CommandType.HistoryOrderGetTicket, commandParameters);
        }

        ///<summary>
        ///Returns the requested order property.
        ///</summary>
        ///<param name="ticket_number">Order ticket.</param>
        ///<param name="property_id">Identifier of the order property.</param>
        public double HistoryOrderGetDouble(ulong ticket_number, ENUM_ORDER_PROPERTY_DOUBLE property_id)
        {
            var commandParameters = new ArrayList { ticket_number, (int)property_id };

            return sendCommand<double>(Mt5CommandType.HistoryOrderGetDouble, commandParameters);
        }

        ///<summary>
        ///Returns the requested property of an order. 
        ///</summary>
        ///<param name="ticket_number">Order ticket.</param>
        ///<param name="property_id">Identifier of the order property.</param>
        public long HistoryOrderGetInteger(ulong ticket_number, ENUM_ORDER_PROPERTY_INTEGER property_id)
        {
            var commandParameters = new ArrayList { ticket_number, (int)property_id };

            return sendCommand<long>(Mt5CommandType.HistoryOrderGetInteger, commandParameters);
        }

        ///<summary>
        ///Returns the requested property of an order. 
        ///</summary>
        ///<param name="ticket_number">Order ticket.</param>
        ///<param name="property_id">Identifier of the order property.</param>
        public string HistoryOrderGetString(ulong ticket_number, ENUM_ORDER_PROPERTY_STRING property_id)
        {
            var commandParameters = new ArrayList { ticket_number, (int)property_id };

            return sendCommand<string>(Mt5CommandType.HistoryOrderGetString, commandParameters);
        }

        ///<summary>
        ///Selects a deal in the history for further calling it through appropriate functions. 
        ///</summary>
        ///<param name="ticket">Deal ticket.</param>
        public bool HistoryDealSelect(ulong ticket)
        {
            var commandParameters = new ArrayList { ticket };

            return sendCommand<bool>(Mt5CommandType.HistoryDealSelect, commandParameters);
        }

        ///<summary>
        ///Returns the number of deal in history.
        ///</summary>
        public int HistoryDealsTotal()
        {
            return sendCommand<int>(Mt5CommandType.HistoryDealsTotal, null);
        }

        ///<summary>
        ///The function selects a deal for further processing and returns the deal ticket in history. 
        ///</summary>
        ///<param name="index">Number of a deal in the list of deals.</param>
        public ulong HistoryDealGetTicket(int index)
        {
            var commandParameters = new ArrayList { index };

            return sendCommand<ulong>(Mt5CommandType.HistoryDealGetTicket, commandParameters);
        }

        ///<summary>
        ///Returns the requested property of a deal. 
        ///</summary>
        ///<param name="ticket_number">Deal ticket.</param>
        ///<param name="property_id"> Identifier of a deal property.</param>
        public double HistoryDealGetDouble(ulong ticket_number, ENUM_DEAL_PROPERTY_DOUBLE property_id)
        {
            var commandParameters = new ArrayList { ticket_number, property_id };

            return sendCommand<double>(Mt5CommandType.HistoryDealGetDouble, commandParameters);
        }

        ///<summary>
        ///Returns the requested property of a deal. 
        ///</summary>
        ///<param name="ticket_number">Deal ticket.</param>
        ///<param name="property_id"> Identifier of a deal property.</param>
        public long HistoryDealGetInteger(ulong ticket_number, ENUM_DEAL_PROPERTY_INTEGER property_id)
        {
            var commandParameters = new ArrayList { ticket_number, property_id };

            return sendCommand<long>(Mt5CommandType.HistoryDealGetInteger, commandParameters);
        }

        ///<summary>
        ///Returns the requested property of a deal. 
        ///</summary>
        ///<param name="ticket_number">Deal ticket.</param>
        ///<param name="property_id"> Identifier of a deal property.</param>
        public string HistoryDealGetString(ulong ticket_number, ENUM_DEAL_PROPERTY_STRING property_id)
        {
            var commandParameters = new ArrayList { ticket_number, property_id };

            return sendCommand<string>(Mt5CommandType.HistoryDealGetString, commandParameters);
        }
        #endregion

        #region Account Information functions

        ///<summary>
        ///Returns the value of the corresponding account property. 
        ///</summary>
        ///<param name="property_id">Identifier of the property.</param>
        public double AccountInfoDouble(ENUM_ACCOUNT_INFO_DOUBLE property_id)
        {
            var commandParameters = new ArrayList { (int)property_id };

            return sendCommand<double>(Mt5CommandType.AccountInfoDouble, commandParameters);
        }

        ///<summary>
        ///Returns the value of the corresponding account property. 
        ///</summary>
        ///<param name="property_id">Identifier of the property.</param>
        public long AccountInfoInteger(ENUM_ACCOUNT_INFO_INTEGER property_id)
        {
            var commandParameters = new ArrayList { (int)property_id };

            return sendCommand<long>(Mt5CommandType.AccountInfoInteger, commandParameters);
        }

        ///<summary>
        ///Returns the value of the corresponding account property. 
        ///</summary>
        ///<param name="property_id">Identifier of the property.</param>
        public string AccountInfoString(ENUM_ACCOUNT_INFO_STRING property_id)
        {
            var commandParameters = new ArrayList { (int)property_id };

            return sendCommand<string>(Mt5CommandType.AccountInfoString, commandParameters);
        }
        #endregion

        #region Timeseries and Indicators Access
        ///<summary>
        ///Returns information about the state of historical data.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe"> Period.</param>
        ///<param name="prop_id">Identifier of the requested property, value of the ENUM_SERIES_INFO_INTEGER enumeration.</param>
        public int SeriesInfoInteger(string symbol_name, ENUM_TIMEFRAMES timeframe, ENUM_SERIES_INFO_INTEGER prop_id)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, (int)prop_id };

            return sendCommand<int>(Mt5CommandType.SeriesInfoInteger, commandParameters);
        }

        ///<summary>
        ///Returns the number of bars count in the history for a specified symbol and period.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe"> Period.</param>
        public int Bars(string symbol_name, ENUM_TIMEFRAMES timeframe)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe };

            return sendCommand<int>(Mt5CommandType.Bars, commandParameters);
        }

        ///<summary>
        ///Returns the number of bars count in the history for a specified symbol and period.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_time">Bar time corresponding to the first element.</param>
        ///<param name="stop_time">Bar time corresponding to the last element.</param>
        public int Bars(string symbol_name, ENUM_TIMEFRAMES timeframe, DateTime start_time, DateTime stop_time)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(start_time), Mt5TimeConverter.ConvertToMtTime(stop_time) };

            return sendCommand<int>(Mt5CommandType.Bars2, commandParameters);
        }

        ///<summary>
        ///Returns the number of calculated data for the specified indicator.
        ///</summary>
        ///<param name="indicator_handle">The indicator handle, returned by the corresponding indicator function.</param>
        public int BarsCalculated(int indicator_handle)
        {
            var commandParameters = new ArrayList { indicator_handle };

            return sendCommand<int>(Mt5CommandType.BarsCalculated, commandParameters);
        }

        ///<summary>
        ///Gets data of a specified buffer of a certain indicator in the necessary quantity.
        ///</summary>
        ///<param name="indicator_handle">The indicator handle, returned by the corresponding indicator function.</param>
        ///<param name="buffer_num">The indicator buffer number.</param>
        ///<param name="start_pos">The position of the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="buffer">Array of double type.</param>
        public int CopyBuffer(int indicator_handle, int buffer_num, int start_pos, int count, out double[] buffer)
        {
            var commandParameters = new ArrayList { indicator_handle, buffer_num, start_pos, count };
            buffer = sendCommand<double[]>(Mt5CommandType.CopyBuffer, commandParameters);
            return buffer != null ? buffer.Length : 0;
        }

        ///<summary>
        ///Gets data of a specified buffer of a certain indicator in the necessary quantity.
        ///</summary>
        ///<param name="indicator_handle">The indicator handle, returned by the corresponding indicator function.</param>
        ///<param name="buffer_num">The indicator buffer number.</param>
        ///<param name="start_time">Bar time, corresponding to the first element.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="buffer">Array of double type.</param>
        public int CopyBuffer(int indicator_handle, int buffer_num, DateTime start_time, int count, out double[] buffer)
        {
            var commandParameters = new ArrayList { indicator_handle, buffer_num, Mt5TimeConverter.ConvertToMtTime(start_time), count };
            buffer = sendCommand<double[]>(Mt5CommandType.CopyBuffer1, commandParameters);
            return buffer != null ? buffer.Length : 0;
        }

        ///<summary>
        ///Gets data of a specified buffer of a certain indicator in the necessary quantity.
        ///</summary>
        ///<param name="indicator_handle">The indicator handle, returned by the corresponding indicator function.</param>
        ///<param name="buffer_num">The indicator buffer number.</param>
        ///<param name="start_time">Bar time, corresponding to the first element.</param>
        ///<param name="stop_time">Bar time, corresponding to the last element.</param>
        ///<param name="buffer">Array of double type.</param>
        public int CopyBuffer(int indicator_handle, int buffer_num, DateTime start_time, DateTime stop_time, out double[] buffer)
        {
            var commandParameters = new ArrayList { indicator_handle, buffer_num, Mt5TimeConverter.ConvertToMtTime(start_time), Mt5TimeConverter.ConvertToMtTime(stop_time) };
            buffer = sendCommand<double[]>(Mt5CommandType.CopyBuffer1, commandParameters);
            return buffer != null ? buffer.Length : 0;
        }

        ///<summary>
        ///Gets history data of MqlRates structure of a specified symbol-period in specified quantity into the rates_array array. The elements ordering of the copied data is from present to the past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_pos">The start position for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="rates_array">Array of MqlRates type.</param>
        public int CopyRates(string symbol_name, ENUM_TIMEFRAMES timeframe, int start_pos, int count, out MqlRates[] rates_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, start_pos, count };

            rates_array = null;

            var retVal = sendCommand<MtMqlRates[]>(Mt5CommandType.CopyRates, commandParameters);
            if (retVal != null)
            {
                rates_array = new MqlRates[retVal.Length];
                for(int i = 0; i < retVal.Length; i++)
                {
                    rates_array[i] = new MqlRates(Mt5TimeConverter.ConvertFromMtTime(retVal[i].time)
                        , retVal[i].open
                        , retVal[i].high
                        , retVal[i].low
                        , retVal[i].close
                        , retVal[i].tick_volume
                        , retVal[i].spread
                        , retVal[i].real_volume);
                }
            }

            return rates_array != null ? rates_array.Length : 0;
        }

        ///<summary>
        ///Gets history data of MqlRates structure of a specified symbol-period in specified quantity into the rates_array array. The elements ordering of the copied data is from present to the past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_time">The start time for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="rates_array">Array of MqlRates type.</param>
        public int CopyRates(string symbol_name, ENUM_TIMEFRAMES timeframe, DateTime start_time, int count, out MqlRates[] rates_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(start_time), count };

            rates_array = null;

            var retVal = sendCommand<MtMqlRates[]>(Mt5CommandType.CopyRates1, commandParameters);
            if (retVal != null)
            {
                rates_array = new MqlRates[retVal.Length];
                for (int i = 0; i < retVal.Length; i++)
                {
                    rates_array[i] = new MqlRates(Mt5TimeConverter.ConvertFromMtTime(retVal[i].time)
                        , retVal[i].open
                        , retVal[i].high
                        , retVal[i].low
                        , retVal[i].close
                        , retVal[i].tick_volume
                        , retVal[i].spread
                        , retVal[i].real_volume);
                }
            }

            return rates_array != null ? rates_array.Length : 0;
        }

        ///<summary>
        ///Gets history data of MqlRates structure of a specified symbol-period in specified quantity into the rates_array array. The elements ordering of the copied data is from present to the past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_time">The start time for the first element to copy.</param>
        ///<param name="stop_time">Bar time, corresponding to the last element to copy.</param>
        ///<param name="rates_array">Array of MqlRates type.</param>
        public int CopyRates(string symbol_name, ENUM_TIMEFRAMES timeframe, DateTime start_time, DateTime stop_time, out MqlRates[] rates_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(start_time), Mt5TimeConverter.ConvertToMtTime(stop_time) };

            rates_array = null;

            var retVal = sendCommand<MtMqlRates[]>(Mt5CommandType.CopyRates2, commandParameters);
            if (retVal != null)
            {
                rates_array = new MqlRates[retVal.Length];
                for (int i = 0; i < retVal.Length; i++)
                {
                    rates_array[i] = new MqlRates(Mt5TimeConverter.ConvertFromMtTime(retVal[i].time)
                        , retVal[i].open
                        , retVal[i].high
                        , retVal[i].low
                        , retVal[i].close
                        , retVal[i].tick_volume
                        , retVal[i].spread
                        , retVal[i].real_volume);
                }
            }

            return rates_array != null ? rates_array.Length : 0;
        }

        ///<summary>
        ///The function gets to time_array history data of bar opening time for the specified symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_pos">The start position for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="rates_array">Array of DatetTme type.</param>
        public int CopyTime(string symbol_name, ENUM_TIMEFRAMES timeframe, int start_pos, int count, out DateTime[] time_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, start_pos, count };

            time_array = null;

            var retVal = sendCommand<long[]>(Mt5CommandType.CopyTime, commandParameters);
            if (retVal != null)
            {
                time_array = new DateTime[retVal.Length];
                for (int i = 0; i < retVal.Length; i++)
                {
                    time_array[i] = Mt5TimeConverter.ConvertFromMtTime(retVal[i]);
                }
            }

            return time_array != null ? time_array.Length : 0;
        }

        ///<summary>
        ///The function gets to time_array history data of bar opening time for the specified symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_time">The start time for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="rates_array">Array of DatetTme type.</param>
        public int CopyTime(string symbol_name, ENUM_TIMEFRAMES timeframe, DateTime start_time, int count, out DateTime[] time_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(start_time), count };

            time_array = null;

            var retVal = sendCommand<long[]>(Mt5CommandType.CopyTime1, commandParameters);
            if (retVal != null)
            {
                time_array = new DateTime[retVal.Length];
                for (int i = 0; i < retVal.Length; i++)
                {
                    time_array[i] = Mt5TimeConverter.ConvertFromMtTime(retVal[i]);
                }
            }

            return time_array != null ? time_array.Length : 0;
        }

        ///<summary>
        ///The function gets to time_array history data of bar opening time for the specified symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_pos">The start time for the first element to copy.</param>
        ///<param name="stop_time">Bar time corresponding to the last element to copy.</param>
        ///<param name="rates_array">Array of DatetTme type.</param>
        public int CopyTime(string symbol_name, ENUM_TIMEFRAMES timeframe, DateTime start_time, DateTime stop_time, out DateTime[] time_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(start_time), Mt5TimeConverter.ConvertToMtTime(stop_time) };

            time_array = null;

            var retVal = sendCommand<long[]>(Mt5CommandType.CopyTime2, commandParameters);
            if (retVal != null)
            {
                time_array = new DateTime[retVal.Length];
                for (int i = 0; i < retVal.Length; i++)
                {
                    time_array[i] = Mt5TimeConverter.ConvertFromMtTime(retVal[i]);
                }
            }

            return time_array != null ? time_array.Length : 0;
        }

        ///<summary>
        ///The function gets into open_array the history data of bar open prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_pos">The start position for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="rates_array">Array of double type.</param>
        public int CopyOpen(string symbol_name, ENUM_TIMEFRAMES timeframe, int start_pos, int count, out double[] open_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, start_pos, count };

            open_array = sendCommand<double[]>(Mt5CommandType.CopyOpen, commandParameters);

            return open_array != null ? open_array.Length : 0;
        }

        ///<summary>
        ///The function gets into open_array the history data of bar open prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_time">The start time for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="rates_array">Array of double type.</param>
        public int CopyOpen(string symbol_name, ENUM_TIMEFRAMES timeframe, DateTime start_time, int count, out double[] open_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(start_time), count };

            open_array = sendCommand<double[]>(Mt5CommandType.CopyOpen1, commandParameters);

            return open_array != null ? open_array.Length : 0;
        }

        ///<summary>
        ///The function gets into open_array the history data of bar open prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_time">The start time for the first element to copy.</param>
        ///<param name="stop_time">The start time for the last element to copy.</param>
        ///<param name="rates_array">Array of double type.</param>
        public int CopyOpen(string symbol_name, ENUM_TIMEFRAMES timeframe, DateTime start_time, DateTime stop_time, out double[] open_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(start_time), Mt5TimeConverter.ConvertToMtTime(stop_time) };

            open_array = sendCommand<double[]>(Mt5CommandType.CopyOpen2, commandParameters);

            return open_array != null ? open_array.Length : 0;
        }

        ///<summary>
        ///The function gets into high_array the history data of highest bar prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_pos">The start position for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="rates_array">Array of double type.</param>
        public int CopyHigh(string symbol_name, ENUM_TIMEFRAMES timeframe, int start_pos, int count, out double[] high_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, start_pos, count };

            high_array = sendCommand<double[]>(Mt5CommandType.CopyHigh, commandParameters);

            return high_array != null ? high_array.Length : 0;
        }

        ///<summary>
        ///The function gets into high_array the history data of highest bar prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_time">The start time for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="rates_array">Array of double type.</param>
        public int CopyHigh(string symbol_name, ENUM_TIMEFRAMES timeframe, DateTime start_time, int count, out double[] high_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(start_time), count };

            high_array = sendCommand<double[]>(Mt5CommandType.CopyHigh1, commandParameters);

            return high_array != null ? high_array.Length : 0;
        }

        ///<summary>
        ///The function gets into high_array the history data of highest bar prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_time">The start time for the first element to copy.</param>
        ///<param name="stop_time">The start time for the last element to copy.</param>
        ///<param name="rates_array">Array of double type.</param>
        public int CopyHigh(string symbol_name, ENUM_TIMEFRAMES timeframe, DateTime start_time, DateTime stop_time, out double[] high_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(start_time), Mt5TimeConverter.ConvertToMtTime(stop_time) };

            high_array = sendCommand<double[]>(Mt5CommandType.CopyHigh2, commandParameters);

            return high_array != null ? high_array.Length : 0;
        }

        ///<summary>
        ///The function gets into low_array the history data of minimal bar prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_pos">The start position for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="rates_array">Array of double type.</param>
        public int CopyLow(string symbol_name, ENUM_TIMEFRAMES timeframe, int start_pos, int count, out double[] low_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, start_pos, count };

            low_array = sendCommand<double[]>(Mt5CommandType.CopyLow, commandParameters);

            return low_array != null ? low_array.Length : 0;
        }

        ///<summary>
        ///The function gets into low_array the history data of minimal bar prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_time">The start time for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="rates_array">Array of double type.</param>
        public int CopyLow(string symbol_name, ENUM_TIMEFRAMES timeframe, DateTime start_time, int count, out double[] low_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(start_time), count };

            low_array = sendCommand<double[]>(Mt5CommandType.CopyLow1, commandParameters);

            return low_array != null ? low_array.Length : 0;
        }

        ///<summary>
        ///The function gets into low_array the history data of minimal bar prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_time">The start time for the first element to copy.</param>
        ///<param name="stop_time">The start time for the last element to copy.</param>
        ///<param name="rates_array">Array of double type.</param>
        public int CopyLow(string symbol_name, ENUM_TIMEFRAMES timeframe, DateTime start_time, DateTime stop_time, out double[] low_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(start_time), Mt5TimeConverter.ConvertToMtTime(stop_time) };

            low_array = sendCommand<double[]>(Mt5CommandType.CopyLow2, commandParameters);

            return low_array != null ? low_array.Length : 0;
        }

        ///<summary>
        ///The function gets into close_array the history data of bar close prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_pos">The start position for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="rates_array">Array of double type.</param>
        public int CopyClose(string symbol_name, ENUM_TIMEFRAMES timeframe, int start_pos, int count, out double[] close_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, start_pos, count };

            close_array = sendCommand<double[]>(Mt5CommandType.CopyClose, commandParameters);

            return close_array != null ? close_array.Length : 0;
        }

        ///<summary>
        ///The function gets into close_array the history data of bar close prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_time">The start time for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="rates_array">Array of double type.</param>
        public int CopyClose(string symbol_name, ENUM_TIMEFRAMES timeframe, DateTime start_time, int count, out double[] close_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(start_time), count };

            close_array = sendCommand<double[]>(Mt5CommandType.CopyClose1, commandParameters);

            return close_array != null ? close_array.Length : 0;
        }

        ///<summary>
        ///The function gets into close_array the history data of bar close prices for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_time">The start time for the first element to copy.</param>
        ///<param name="stop_time">The start time for the last element to copy.</param>
        ///<param name="rates_array">Array of double type.</param>
        public int CopyClose(string symbol_name, ENUM_TIMEFRAMES timeframe, DateTime start_time, DateTime stop_time, out double[] close_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(start_time), Mt5TimeConverter.ConvertToMtTime(stop_time) };

            close_array = sendCommand<double[]>(Mt5CommandType.CopyClose2, commandParameters);

            return close_array != null ? close_array.Length : 0;
        }

        ///<summary>
        ///The function gets into volume_array the history data of tick volumes for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_pos">The start position for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="rates_array">Array of long type.</param>
        public int CopyTickVolume(string symbol_name, ENUM_TIMEFRAMES timeframe, int start_pos, int count, out long[] volume_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, start_pos, count };

            volume_array = sendCommand<long[]>(Mt5CommandType.CopyTickVolume, commandParameters);

            return volume_array != null ? volume_array.Length : 0;
        }

        ///<summary>
        ///The function gets into volume_array the history data of tick volumes for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_time">The start time for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="rates_array">Array of long type.</param>
        public int CopyTickVolume(string symbol_name, ENUM_TIMEFRAMES timeframe, DateTime start_time, int count, out long[] volume_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(start_time), count };

            volume_array = sendCommand<long[]>(Mt5CommandType.CopyTickVolume1, commandParameters);

            return volume_array != null ? volume_array.Length : 0;
        }

        ///<summary>
        ///The function gets into volume_array the history data of tick volumes for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_time">The start time for the first element to copy.</param>
        ///<param name="stop_time">The start time for the last element to copy.</param>
        ///<param name="rates_array">Array of long type.</param>
        public int CopyTickVolume(string symbol_name, ENUM_TIMEFRAMES timeframe, DateTime start_time, DateTime stop_time, out long[] volume_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(start_time), Mt5TimeConverter.ConvertToMtTime(stop_time) };

            volume_array = sendCommand<long[]>(Mt5CommandType.CopyTickVolume2, commandParameters);

            return volume_array != null ? volume_array.Length : 0;
        }

        ///<summary>
        ///The function gets into volume_array the history data of trade volumes for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_pos">The start position for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="rates_array">Array of long type.</param>
        public int CopyRealVolume(string symbol_name, ENUM_TIMEFRAMES timeframe, int start_pos, int count, out long[] volume_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, start_pos, count };

            volume_array = sendCommand<long[]>(Mt5CommandType.CopyRealVolume, commandParameters);

            return volume_array != null ? volume_array.Length : 0;
        }

        ///<summary>
        ///The function gets into volume_array the history data of trade volumes for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_time">The start time for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="rates_array">Array of long type.</param>
        public int CopyRealVolume(string symbol_name, ENUM_TIMEFRAMES timeframe, DateTime start_time, int count, out long[] volume_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(start_time), count };

            volume_array = sendCommand<long[]>(Mt5CommandType.CopyRealVolume1, commandParameters);

            return volume_array != null ? volume_array.Length : 0;
        }

        ///<summary>
        ///The function gets into volume_array the history data of trade volumes for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_time">The start time for the first element to copy.</param>
        ///<param name="stop_time">The start time for the last element to copy.</param>
        ///<param name="rates_array">Array of long type.</param>
        public int CopyRealVolume(string symbol_name, ENUM_TIMEFRAMES timeframe, DateTime start_time, DateTime stop_time, out long[] volume_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(start_time), Mt5TimeConverter.ConvertToMtTime(stop_time) };

            volume_array = sendCommand<long[]>(Mt5CommandType.CopyRealVolume2, commandParameters);

            return volume_array != null ? volume_array.Length : 0;
        }

        ///<summary>
        ///The function gets into spread_array the history data of spread values for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_pos">The start position for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="rates_array">Array of long type.</param>
        public int CopySpread(string symbol_name, ENUM_TIMEFRAMES timeframe, int start_pos, int count, out int[] spread_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, start_pos, count };

            spread_array = sendCommand<int[]>(Mt5CommandType.CopySpread, commandParameters);

            return spread_array != null ? spread_array.Length : 0;
        }

        ///<summary>
        ///The function gets into spread_array the history data of spread values for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_time">The start time for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="rates_array">Array of long type.</param>
        public int CopySpread(string symbol_name, ENUM_TIMEFRAMES timeframe, DateTime start_time, int count, out int[] spread_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(start_time), count };

            spread_array = sendCommand<int[]>(Mt5CommandType.CopySpread1, commandParameters);

            return spread_array != null ? spread_array.Length : 0;
        }

        ///<summary>
        ///The function gets into spread_array the history data of spread values for the selected symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbol_name">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="start_time">The start time for the first element to copy.</param>
        ///<param name="stop_time">The start time for the last element to copy.</param>
        ///<param name="rates_array">Array of long type.</param>
        public int CopySpread(string symbol_name, ENUM_TIMEFRAMES timeframe, DateTime start_time, DateTime stop_time, out int[] spread_array)
        {
            var commandParameters = new ArrayList { symbol_name, (int)timeframe, Mt5TimeConverter.ConvertToMtTime(start_time), Mt5TimeConverter.ConvertToMtTime(stop_time) };

            spread_array = sendCommand<int[]>(Mt5CommandType.CopySpread2, commandParameters);

            return spread_array != null ? spread_array.Length : 0;
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

            return sendCommand<int>(Mt5CommandType.SymbolsTotal, commandParameters);
        }

        ///<summary>
        ///Returns the name of a symbol.
        ///</summary>
        ///<param name="pos">Order number of a symbol.</param>
        ///<param name="selected">Request mode. If the value is true, the symbol is taken from the list of symbols selected in MarketWatch. If the value is false, the symbol is taken from the general list.</param>

        public string SymbolName(int pos, bool selected)
        {
            var commandParameters = new ArrayList { pos, selected };

            return sendCommand<string>(Mt5CommandType.SymbolName, commandParameters);
        }

        ///<summary>
        ///Selects a symbol in the Market Watch window or removes a symbol from the window.
        ///</summary>
        ///<param name="name">Symbol name.</param>
        ///<param name="selected">Switch. If the value is false, a symbol should be removed from MarketWatch, otherwise a symbol should be selected in this window. A symbol can't be removed if the symbol chart is open, or there are open positions for this symbol.</param>

        public bool SymbolSelect(string name, bool selected)
        {
            var commandParameters = new ArrayList { name, selected };

            return sendCommand<bool>(Mt5CommandType.SymbolSelect, commandParameters);
        }

        ///<summary>
        ///The function checks whether data of a selected symbol in the terminal are synchronized with data on the trade server.
        ///</summary>
        ///<param name="name">Symbol name.</param>

        public bool SymbolIsSynchronized(string name)
        {
            var commandParameters = new ArrayList { name };

            return sendCommand<bool>(Mt5CommandType.SymbolIsSynchronized, commandParameters);
        }

        ///<summary>
        ///Returns the corresponding property of a specified symbol.
        ///</summary>
        ///<param name="name">Symbol name.</param>
        ///<param name="prop_id">Identifier of a symbol property.</param>

        public double SymbolInfoDouble(string name, ENUM_SYMBOL_INFO_DOUBLE prop_id)
        {
            var commandParameters = new ArrayList { name, (int)prop_id };

            return sendCommand<double>(Mt5CommandType.SymbolInfoDouble, commandParameters);
        }

        ///<summary>
        ///Returns the corresponding property of a specified symbol.
        ///</summary>
        ///<param name="name">Symbol name.</param>
        ///<param name="prop_id">Identifier of a symbol property.</param>

        public long SymbolInfoInteger(string name, ENUM_SYMBOL_INFO_INTEGER prop_id)
        {
            var commandParameters = new ArrayList { name, (int)prop_id };

            return sendCommand<long>(Mt5CommandType.SymbolInfoInteger, commandParameters);
        }

        ///<summary>
        ///Returns the corresponding property of a specified symbol. 
        ///</summary>
        ///<param name="name">Symbol name.</param>
        ///<param name="prop_id">Identifier of a symbol property.</param>

        public string SymbolInfoString(string name, ENUM_SYMBOL_INFO_STRING prop_id)
        {
            var commandParameters = new ArrayList { name, (int)prop_id };

            return sendCommand<string>(Mt5CommandType.SymbolInfoString, commandParameters);
        }


        ///<summary>
        ///The function returns current prices of a specified symbol in a variable of the MqlTick type.
        ///The function returns true if successful, otherwise returns false.
        ///</summary>
        ///<param name="name">Symbol name.</param>
        ///<param name="prop_id"> Link to the structure of the MqlTick type, to which the current prices and time of the last price update will be placed.</param>

        public bool SymbolInfoTick(string symbol, out MqlTick  tick)
        {
            var commandParameters = new ArrayList { symbol };

            var retVal = sendCommand<MtMqlTick>(Mt5CommandType.SymbolInfoTick, commandParameters);

            tick = null;
            if (retVal != null)
            {
                tick = new MqlTick(Mt5TimeConverter.ConvertFromMtTime(retVal.time), retVal.bid, retVal.ask, retVal.last, retVal.volume);
            }

            return tick != null;
        }


        ///<summary>
        ///Allows receiving time of beginning and end of the specified quoting sessions for a specified symbol and weekday.
        ///</summary>
        ///<param name="name">Symbol name.</param>
        ///<param name="day_of_week">Day of the week</param>
        ///<param name="session_index">Ordinal number of a session, whose beginning and end time we want to receive. Indexing of sessions starts with 0.</param>
        ///<param name="from">Session beginning time in seconds from 00 hours 00 minutes, in the returned value date should be ignored.</param>
        ///<param name="to">Session end time in seconds from 00 hours 00 minutes, in the returned value date should be ignored.</param>
        public bool SymbolInfoSessionQuote(string name, ENUM_DAY_OF_WEEK day_of_week, uint session_index, out DateTime from, out DateTime to)
        {
            var commandParameters = new ArrayList { name, (int)day_of_week, session_index };

            string strResult = sendCommand<string>(Mt5CommandType.SymbolInfoSessionQuote, commandParameters);

            return strResult.ParseResult(PARAM_SEPARATOR, out from, out to); 
        }

        ///<summary>
        ///Allows receiving time of beginning and end of the specified trading sessions for a specified symbol and weekday.
        ///</summary>
        ///<param name="name">Symbol name.</param>
        ///<param name="day_of_week">Day of the week</param>
        ///<param name="session_index">Ordinal number of a session, whose beginning and end time we want to receive. Indexing of sessions starts with 0.</param>
        ///<param name="from">Session beginning time in seconds from 00 hours 00 minutes, in the returned value date should be ignored.</param>
        ///<param name="to">Session end time in seconds from 00 hours 00 minutes, in the returned value date should be ignored.</param>
        public bool SymbolInfoSessionTrade(string name, ENUM_DAY_OF_WEEK day_of_week, uint session_index, out DateTime from, out DateTime to)
        {
            var commandParameters = new ArrayList { name, (int)day_of_week, session_index };

            string strResult = sendCommand<string>(Mt5CommandType.SymbolInfoSessionTrade, commandParameters);

            return strResult.ParseResult(PARAM_SEPARATOR, out from, out to);
        }

        ///<summary>
        ///Provides opening of Depth of Market for a selected symbol, and subscribes for receiving notifications of the DOM changes.
        ///</summary>
        ///<param name="name">Symbol name.</param>
        public bool MarketBookAdd(string symbol)
        {
            var commandParameters = new ArrayList { symbol };

            return sendCommand<bool>(Mt5CommandType.MarketBookAdd, commandParameters);
        }

        ///<summary>
        ///Provides closing of Depth of Market for a selected symbol, and cancels the subscription for receiving notifications of the DOM changes.
        ///</summary>
        ///<param name="name">Symbol name.</param>
        public bool MarketBookRelease(string symbol)
        {
            var commandParameters = new ArrayList { symbol };

            return sendCommand<bool>(Mt5CommandType.MarketBookRelease, commandParameters);
        }

        ///<summary>
        ///Returns a structure array MqlBookInfo containing records of the Depth of Market of a specified symbol.
        ///</summary>
        ///<param name="name">Symbol name.</param>
        ///<param name="book">Reference to an array of Depth of Market records.</param>        
        public bool MarketBookGet(string symbol, out MqlBookInfo[] book)
        {
            var commandParameters = new ArrayList { symbol };

            var retVal = sendCommand<MtMqlBookInfo[]>(Mt5CommandType.MarketBookGet, commandParameters);

            book = null; 
            if (retVal != null)
            {
                book = new MqlBookInfo[retVal.Length];

                for (int i = 0; i < retVal.Length; i++)
                {
                    book[0] = new MqlBookInfo((ENUM_BOOK_TYPE)retVal[i].type, retVal[i].price, retVal[i].volume);                        
                }
            }

            return book != null;
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
                mClient.Open(host, port);
                mClient.Connect();
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
        }

        private void Connect(int port)
        {
            ConnectionState = Mt5ConnectionState.Connecting;
            ConnectionStateChanged.FireEvent(this
                , new Mt5ConnectionEventArgs(Mt5ConnectionState.Connecting, "Connecting to 'localhost':" + port));

            try
            {
                mClient.Open(port);
                mClient.Connect();
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
        }

        private void Disconnect()
        {
            mClient.Disconnect();
            mClient.Close();

            ConnectionState = Mt5ConnectionState.Disconnected;
            ConnectionStateChanged.FireEvent(this, new Mt5ConnectionEventArgs(Mt5ConnectionState.Disconnected, "Disconnected"));
        }

        private T sendCommand<T>(Mt5CommandType commandType, ArrayList commandParameters)
        {
            var response = mClient.SendCommand((int)commandType, commandParameters);

            if (response is MtResponseDouble)
                return (T)Convert.ChangeType(((MtResponseDouble)response).Value, typeof(T));

            if (response is MtResponseInt)
                return (T)Convert.ChangeType(((MtResponseInt)response).Value, typeof(T));

            if (response is MtResponseLong)
                return (T)Convert.ChangeType(((MtResponseLong)response).Value, typeof(T));

            if (response is MtResponseULong)
                return (T)Convert.ChangeType(((MtResponseULong)response).Value, typeof(T));

            if (response is MtResponseBool)
                return (T)Convert.ChangeType(((MtResponseBool)response).Value, typeof(T));

            if (response is MtResponseString)
                return (T)Convert.ChangeType(((MtResponseString)response).Value, typeof(T));

            if (response is MtResponseDoubleArray)
                return (T)Convert.ChangeType(((MtResponseDoubleArray)response).Value, typeof(T));

            if (response is MtResponseIntArray)
                return (T)Convert.ChangeType(((MtResponseIntArray)response).Value, typeof(T));

            if (response is MtResponseLongArray)
                return (T)Convert.ChangeType(((MtResponseLongArray)response).Value, typeof(T));

            if (response is MtResponseArrayList)
                return (T)Convert.ChangeType(((MtResponseArrayList)response).Value, typeof(T));

            if (response is MtResponseMqlRatesArray)
                return (T)Convert.ChangeType(((MtResponseMqlRatesArray)response).Value, typeof(T));

            if (response is MtResponseMqlTick)
                return (T)Convert.ChangeType(((MtResponseMqlTick)response).Value, typeof(T));

            if (response is MtResponseMqlBookInfoArray)
                return (T)Convert.ChangeType(((MtResponseMqlBookInfoArray)response).Value, typeof(T));

            return default(T);
        }

        private void mClient_QuoteUpdated(MtQuote quote)
        {
            if (quote != null)
            {
                if (QuoteUpdated != null)
                {
                    QuoteUpdated(this, quote.Instrument, quote.Bid, quote.Ask);
                }
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

        #endregion

        #region Private Fields
        private readonly MtClient mClient = new MtClient();
        #endregion
    }
}
