using Newtonsoft.Json;
using MtClient;
using MtApi5.MtProtocol;
using MtApi5.MtProtocol.ICustomRequest;

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

        public delegate void QuoteHandler(object sender, string symbol, double bid, double ask);


        #region Private Fields
        private MtRpcClient? _client;
        private readonly object _locker = new();
        private Mt5ConnectionState _connectionState = Mt5ConnectionState.Disconnected;
        private int _executorHandle;
        private readonly Dictionary<Mt5EventTypes, Action<int, string>> _mtEventHandlers = [];
        
        private HashSet<int> _experts = [];
        private Dictionary<int, Mt5Quote> _quotes = [];
        #endregion

        #region Public Methods
        private IMtLogger Log { get; }

        public MtApi5Client(IMtLogger? log = null)
        {
            _mtEventHandlers[Mt5EventTypes.OnBookEvent] = ReceivedOnBookEvent;
            _mtEventHandlers[Mt5EventTypes.OnTick] = ReceivedOnTickEvent;
            _mtEventHandlers[Mt5EventTypes.OnTradeTransaction] = ReceivedOnTradeTransactionEvent;
            _mtEventHandlers[Mt5EventTypes.OnLastTimeBar] = ReceivedOnLastTimeBarEvent;
            _mtEventHandlers[Mt5EventTypes.OnLockTicks] = ReceivedOnLockTicksEvent;

            Log = log ?? new StubMtLogger();
        }

        ///<summary>
        ///Connect with MetaTrader API. Async method.
        ///</summary>
        ///<param name="host">Address of MetaTrader host (ex. 192.168.1.2, localhost)</param>
        ///<param name="port">Port of host connection (default 8222) </param>
        public void BeginConnect(string host, int port)
        {
            Log.Info($"BeginConnect: host = {host}, port = {port}");
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await Connect(host, port);
                }
                catch (Exception)
                {
                }
            });
        }

        ///<summary>
        ///Connect with MetaTrader API. Async method.
        ///</summary>
        ///<param name="port">Port of host connection (default 8222) </param>
        public void BeginConnect(int port)
        {
            BeginConnect("localhost", port);
        }

        ///<summary>
        ///Connect with MetaTrader API.
        ///</summary>
        ///<param name="host">Address of MetaTrader host (ex. 192.168.1.2, localhost)</param>
        ///<param name="port">Port of host connection (default 8222) </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when host is null or empty.
        /// </exception>
        public async Task Connect(string host, int port)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentNullException(nameof(host));

            lock (_locker)
            {
                if (_connectionState == Mt5ConnectionState.Connected
                    || _connectionState == Mt5ConnectionState.Connecting)
                    return;
                _connectionState = Mt5ConnectionState.Connecting;
            }

            ConnectionStateChanged?.Invoke(this, new Mt5ConnectionEventArgs(Mt5ConnectionState.Connecting, $"Connecting to {host}:{port}"));

            var client = new MtRpcClient(host, port, new RpcClientLogger(Log));
            client.ExpertAdded += Client_ExpertAdded;
            client.ExpertRemoved += Client_ExpertRemoved;
            client.MtEventReceived += Client_MtEventReceived;
            client.ConnectionFailed += Client_OnConnectionFailed;
            client.Disconnected += Client_Disconnected;

            try
            {
                await client.Connect();
                Log.Info($"Connected to {host}:{port}");

                var experts = client.RequestExpertsList();
                if (experts == null || experts.Count == 0)
                {
                    var errorMessage = "Failed to load expert list";
                    Log.Error(errorMessage);
                    client.Disconnect();

                    ConnectionStateChanged?.Invoke(this, new Mt5ConnectionEventArgs(Mt5ConnectionState.Failed, errorMessage));
                    throw new Exception($"Connection to {host}:{port} failed. Error: {errorMessage}");
                }

                // Load quotes
                Dictionary<int, Mt5Quote> quotes = [];
                foreach (var handle in experts)
                {
                    var quote = GetQuote(client, handle);
                    if (quote != null)
                        quotes[handle] = quote;
                }

                lock (_locker)
                {
                    _client = client;
                    _experts = experts;
                    _quotes = quotes;
                    if (_executorHandle == 0)
                        _executorHandle = (_experts.Count > 0) ? _experts.ElementAt(0) : 0;
                    _connectionState = Mt5ConnectionState.Connected;
                }

                if (IsTesting())
                    BacktestingReady();

                ConnectionStateChanged?.Invoke(this, new Mt5ConnectionEventArgs(Mt5ConnectionState.Connected, $"Connected to {host}:{port}"));
                QuoteList?.Invoke(this, new(quotes.Values.ToList()));
            }
            catch (Exception e)
            {
                Log.Error($"Connect: Failed connection to {host}:{port}. Error: {e.Message}");
                lock (_locker)
                {
                    _connectionState = Mt5ConnectionState.Failed;
                }
                ConnectionStateChanged?.Invoke(this, new Mt5ConnectionEventArgs(Mt5ConnectionState.Failed, e.Message));
                throw new Exception($"Connection to {host}:{port} failed. Error: {e.Message}");
            }
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
        ///Disconnect from MetaTrader API.
        ///</summary>
        public void Disconnect()
        {
            Log.Info("Disconnect called.");
            Disconnect(false);
        }

        ///<summary>
        ///Load quotes connected into MetaTrader API
        ///</summary>
        public IEnumerable<Mt5Quote> GetQuotes()
        {
            lock (_locker)
            {
                return _quotes.Values.ToList();
            }
        }

        ///<summary>
        ///Checks if the Expert Advisor runs in the testing mode..
        ///</summary>
        public bool IsTesting()
        {
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.IsTesting);
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
        public bool OrderSend(MqlTradeRequest request, out MqlTradeResult? result)
        {
            Log.Debug($"OrderSend: request = {request}");

            if (request == null)
            {
                Log.Warn("OrderSend: request is not defined!");
                result = null;
                return false;
            }

            Dictionary<string, object> commandParameters = new() { { "TradeRequest", request } };
            var response = SendCommand<FuncResult<MqlTradeResult>>(ExecutorHandle, Mt5CommandType.OrderSend, commandParameters);
            result = response?.Result;
            return response != null && response.RetVal;
        }

        ///<summary>
        ///Function is used for conducting asynchronous trade operations without waiting for the trade server's response to a sent request.
        ///</summary>
        ///<param name="request">Reference to a object of MqlTradeRequest type describing the trade activity of the client.</param>
        ///<param name="result">Reference to a object of MqlTradeResult type describing the result of trade operation in case of a successful completion (if true is returned).</param>
        /// <returns>
        /// Returns true if the request is sent to a trade server. In case the request is not sent, it returns false. 
        /// In case the request is sent, in the result variable the response code contains TRADE_RETCODE_PLACED value (code 10008) – "order placed". 
        /// Successful execution means only the fact of sending, but does not give any guarantee that the request has reached the trade server and has been accepted for processing. 
        /// When processing the received request, a trade server sends a reply to a client terminal notifying of change in the current state of positions, 
        /// orders and deals, which leads to the generation of the Trade event.
        /// </returns>
        public bool OrderSendAsync(MqlTradeRequest request, out MqlTradeResult? result)
        {
            Log.Debug($"OrderSend: request = {request}");

            if (request == null)
            {
                Log.Warn("OrderSend: request is not defined!");
                result = null;
                return false;
            }

            Dictionary<string, object> commandParameters = new() { { "TradeRequest", request } };
            var response = SendCommand<FuncResult<MqlTradeResult>>(ExecutorHandle, Mt5CommandType.OrderSendAsync, commandParameters);
            result = response?.Result;
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
            Dictionary<string, object> cmdParams = new() { { "Action", (int)action }, { "Symbol", symbol },
                { "Volume", volume }, { "Price", price } };

            var response = SendCommand<FuncResult<double>>(ExecutorHandle, Mt5CommandType.OrderCalcMargin, cmdParams);
            margin = response != null ? response.Result : double.NaN;
            return response != null && response.RetVal;
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
            Dictionary<string, object> cmdParams = new() { { "Action", (int)action }, { "Symbol", symbol ?? string.Empty },
                { "Volume", volume }, { "PriceOpen", priceOpen }, { "PriceClose", priceClose} };

            var response = SendCommand<FuncResult<double>>(ExecutorHandle, Mt5CommandType.OrderCalcProfit, cmdParams);
            profit = response != null ? response.Result : double.NaN;
            return response != null && response.RetVal;
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
        public bool OrderCheck(MqlTradeRequest request, out MqlTradeCheckResult? result)
        {
            Log.Debug($"OrderCheck: request = {request}");

            if (request == null)
            {
                Log.Warn("OrderCheck: request is not defined!");
                result = null;
                return false;
            }

            Dictionary<string, object> commandParameters = new() { { "TradeRequest", request } };
            var response = SendCommand<FuncResult<MqlTradeCheckResult>>(ExecutorHandle, Mt5CommandType.OrderCheck, commandParameters);
            result = response?.Result;
            return response != null && response.RetVal;
        }

        ///<summary>
        ///Returns the number of open positions.
        ///</summary>
        public int PositionsTotal()
        {
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.PositionsTotal);
        }

        ///<summary>
        ///Returns the symbol corresponding to the open position and automatically selects the position for further working with it using functions PositionGetDouble, PositionGetInteger, PositionGetString.
        ///</summary>
        ///<param name="index">Number of the position in the list of open positions.</param>
        public string? PositionGetSymbol(int index)
        {
            Dictionary<string, object> commandParameters = new() { { "Index", index } };
            return SendCommand<string>(ExecutorHandle, Mt5CommandType.PositionGetSymbol, commandParameters);
        }

        ///<summary>
        ///Chooses an open position for further working with it. Returns true if the function is successfully completed. Returns false in case of failure.
        ///</summary>
        ///<param name="symbol">Name of the financial security.</param>
        public bool PositionSelect(string symbol)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.PositionSelect, cmdParams);
        }

        ///<summary>
        ///Selects an open position to work with based on the ticket number specified in the position. If successful, returns true. Returns false if the function failed.
        ///</summary>
        ///<param name="ticket">Position ticket.</param>
        public bool PositionSelectByTicket(ulong ticket)
        {
            Dictionary<string, object> cmdParams = new() { { "Ticket", ticket } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.PositionSelectByTicket, cmdParams);
        }

        ///<summary>
        ///The function returns the requested property of an open position, pre-selected using PositionGetSymbol or PositionSelect.
        ///</summary>
        ///<param name="propertyId">Identifier of a position property.</param>
        public double PositionGetDouble(ENUM_POSITION_PROPERTY_DOUBLE propertyId)
        {
            Dictionary<string, object> cmdParams = new() { { "PropertyId", (int)propertyId } };
            return SendCommand<double>(ExecutorHandle, Mt5CommandType.PositionGetDouble, cmdParams);
        }

        ///<summary>
        ///The function returns the requested property of an open position, pre-selected using PositionGetSymbol or PositionSelect.
        ///</summary>
        ///<param name="propertyId">Identifier of a position property.</param>
        public long PositionGetInteger(ENUM_POSITION_PROPERTY_INTEGER propertyId)
        {
            Dictionary<string, object> cmdParams = new() { { "PropertyId", (int)propertyId } };
            return SendCommand<long>(ExecutorHandle, Mt5CommandType.PositionGetInteger, cmdParams);
        }

        ///<summary>
        ///The function returns the requested property of an open position, pre-selected using PositionGetSymbol or PositionSelect.
        ///</summary>
        ///<param name="propertyId">Identifier of a position property.</param>
        public string? PositionGetString(ENUM_POSITION_PROPERTY_STRING propertyId)
        {
            Dictionary<string, object> cmdParams = new() { { "PropertyId", (int)propertyId } };
            return SendCommand<string>(ExecutorHandle, Mt5CommandType.PositionGetString, cmdParams);
        }

        ///<summary>
        ///The function returns the ticket of a position with the specified index in the list of open positions and automatically selects the position to work with using functions PositionGetDouble, PositionGetInteger, PositionGetString.
        ///</summary>
        ///<param name="index">Identifier of a position property.</param>
        public ulong PositionGetTicket(int index)
        {
            Dictionary<string, object> cmdParams = new() { { "Index", index } };
            return SendCommand<ulong>(ExecutorHandle, Mt5CommandType.PositionGetTicket, cmdParams);
        }

        ///<summary>
        ///Returns the number of current orders.
        ///</summary>
        public int OrdersTotal()
        {
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.OrdersTotal);
        }

        ///<summary>
        ///Returns the number of current orders.
        ///</summary>
        ///<param name="index">Number of an order in the list of current orders.</param>
        public ulong OrderGetTicket(int index)
        {
            Dictionary<string, object> cmdParams = new() { { "Index", index } };
            return SendCommand<ulong>(ExecutorHandle, Mt5CommandType.OrderGetTicket, cmdParams);
        }

        ///<summary>
        ///Selects an order to work with. Returns true if the function has been successfully completed. 
        ///</summary>
        ///<param name="ticket">Order ticket.</param>
        public bool OrderSelect(ulong ticket)
        {
            Dictionary<string, object> cmdParams = new() { { "Ticket", ticket } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.OrderSelect, cmdParams);
        }

        ///<summary>
        ///Returns the requested property of an order, pre-selected using OrderGetTicket or OrderSelect.
        ///</summary>
        ///<param name="propertyId"> Identifier of the order property.</param>
        public double OrderGetDouble(ENUM_ORDER_PROPERTY_DOUBLE propertyId)
        {
            Dictionary<string, object> cmdParams = new() { { "PropertyId", (int)propertyId } };
            return SendCommand<double>(ExecutorHandle, Mt5CommandType.OrderGetDouble, cmdParams);
        }

        ///<summary>
        ///Returns the requested property of an order, pre-selected using OrderGetTicket or OrderSelect.
        ///</summary>
        ///<param name="propertyId"> Identifier of the order property.</param>
        public long OrderGetInteger(ENUM_ORDER_PROPERTY_INTEGER propertyId)
        {
            Dictionary<string, object> cmdParams = new() { { "PropertyId", (int)propertyId } };
            return SendCommand<long>(ExecutorHandle, Mt5CommandType.OrderGetInteger, cmdParams);
        }

        ///<summary>
        ///Returns the requested property of an order, pre-selected using OrderGetTicket or OrderSelect.
        ///</summary>
        ///<param name="propertyId"> Identifier of the order property.</param>
        public string? OrderGetString(ENUM_ORDER_PROPERTY_STRING propertyId)
        {
            Dictionary<string, object> cmdParams = new() { { "PropertyId", (int)propertyId } };
            return SendCommand<string>(ExecutorHandle, Mt5CommandType.OrderGetString, cmdParams);
        }

        ///<summary>
        ///Retrieves the history of deals and orders for the specified period of server time.
        ///</summary>
        ///<param name="fromDate">Start date of the request.</param>
        ///<param name="toDate">End date of the request.</param>
        public bool HistorySelect(DateTime fromDate, DateTime toDate)
        {
            Dictionary<string, object> cmdParams = new() { { "FromDate", Mt5TimeConverter.ConvertToMtTime(fromDate) },
                { "ToDate", Mt5TimeConverter.ConvertToMtTime(toDate) } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.HistorySelect, cmdParams);
        }

        ///<summary>
        ///Retrieves the history of deals and orders having the specified position identifier.
        ///</summary>
        ///<param name="positionId">Position identifier that is set to every executed order and every deal.</param>
        public bool HistorySelectByPosition(long positionId)
        {
            Dictionary<string, object> cmdParams = new() { { "PositionId", positionId } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.HistorySelectByPosition, cmdParams);
        }

        ///<summary>
        ///Selects an order from the history for further calling it through appropriate functions. 
        ///</summary>
        ///<param name="ticket">Order ticket.</param>
        public bool HistoryOrderSelect(ulong ticket)
        {
            Dictionary<string, object> cmdParams = new() { { "Ticket", ticket } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.HistoryOrderSelect, cmdParams);
        }

        ///<summary>
        ///Returns the number of orders in the history.
        ///</summary>
        public int HistoryOrdersTotal()
        {
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.HistoryOrdersTotal);
        }

        ///<summary>
        ///Return the ticket of a corresponding order in the history.
        ///</summary>
        ///<param name="index">Number of the order in the list of orders.</param>
        public ulong HistoryOrderGetTicket(int index)
        {
            Dictionary<string, object> cmdParams = new() { { "Index", index } };
            return SendCommand<ulong>(ExecutorHandle, Mt5CommandType.HistoryOrderGetTicket, cmdParams);
        }

        ///<summary>
        ///Returns the requested order property.
        ///</summary>
        ///<param name="ticketNumber">Order ticket.</param>
        ///<param name="propertyId">Identifier of the order property.</param>
        public double HistoryOrderGetDouble(ulong ticketNumber, ENUM_ORDER_PROPERTY_DOUBLE propertyId)
        {
            Dictionary<string, object> cmdParams = new() { { "TicketNumber", ticketNumber },
                { "PropertyId", (int)propertyId } };
            return SendCommand<double>(ExecutorHandle, Mt5CommandType.HistoryOrderGetDouble, cmdParams);
        }

        ///<summary>
        ///Returns the requested property of an order. 
        ///</summary>
        ///<param name="ticketNumber">Order ticket.</param>
        ///<param name="propertyId">Identifier of the order property.</param>
        public long HistoryOrderGetInteger(ulong ticketNumber, ENUM_ORDER_PROPERTY_INTEGER propertyId)
        {
            Dictionary<string, object> cmdParams = new() { { "TicketNumber", ticketNumber },
                { "PropertyId", (int)propertyId } };
            return SendCommand<long>(ExecutorHandle, Mt5CommandType.HistoryOrderGetInteger, cmdParams);
        }

        ///<summary>
        ///Returns the requested property of an order. 
        ///</summary>
        ///<param name="ticketNumber">Order ticket.</param>
        ///<param name="propertyId">Identifier of the order property.</param>
        public string? HistoryOrderGetString(ulong ticketNumber, ENUM_ORDER_PROPERTY_STRING propertyId)
        {
            Dictionary<string, object> cmdParams = new() { { "TicketNumber", ticketNumber },
                { "PropertyId", (int)propertyId } };
            return SendCommand<string>(ExecutorHandle, Mt5CommandType.HistoryOrderGetString, cmdParams);
        }

        ///<summary>
        ///Selects a deal in the history for further calling it through appropriate functions. 
        ///</summary>
        ///<param name="ticket">Deal ticket.</param>
        public bool HistoryDealSelect(ulong ticket)
        {
            Dictionary<string, object> cmdParams = new() { { "Ticket", ticket } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.HistoryDealSelect, cmdParams);
        }

        ///<summary>
        ///Returns the number of deal in history.
        ///</summary>
        public int HistoryDealsTotal()
        {
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.HistoryDealsTotal);
        }

        ///<summary>
        ///The function selects a deal for further processing and returns the deal ticket in history. 
        ///</summary>
        ///<param name="index">Number of a deal in the list of deals.</param>
        public ulong HistoryDealGetTicket(int index)
        {
            Dictionary<string, object> cmdParams = new() { { "Index", index } };
            return SendCommand<ulong>(ExecutorHandle, Mt5CommandType.HistoryDealGetTicket, cmdParams);
        }

        ///<summary>
        ///Returns the requested property of a deal. 
        ///</summary>
        ///<param name="ticketNumber">Deal ticket.</param>
        ///<param name="propertyId"> Identifier of a deal property.</param>
        public double HistoryDealGetDouble(ulong ticketNumber, ENUM_DEAL_PROPERTY_DOUBLE propertyId)
        {
            Dictionary<string, object> cmdParams = new() { { "TicketNumber", ticketNumber },
                { "PropertyId", (int)propertyId } };
            return SendCommand<double>(ExecutorHandle, Mt5CommandType.HistoryDealGetDouble, cmdParams);
        }

        ///<summary>
        ///Returns the requested property of a deal. 
        ///</summary>
        ///<param name="ticketNumber">Deal ticket.</param>
        ///<param name="propertyId"> Identifier of a deal property.</param>
        public long HistoryDealGetInteger(ulong ticketNumber, ENUM_DEAL_PROPERTY_INTEGER propertyId)
        {
            Dictionary<string, object> cmdParams = new() { { "TicketNumber", ticketNumber },
                { "PropertyId", (int)propertyId } };
            return SendCommand<long>(ExecutorHandle, Mt5CommandType.HistoryDealGetInteger, cmdParams);
        }

        ///<summary>
        ///Returns the requested property of a deal. 
        ///</summary>
        ///<param name="ticketNumber">Deal ticket.</param>
        ///<param name="propertyId"> Identifier of a deal property.</param>
        public string? HistoryDealGetString(ulong ticketNumber, ENUM_DEAL_PROPERTY_STRING propertyId)
        {
            Dictionary<string, object> cmdParams = new() { { "TicketNumber", ticketNumber },
                { "PropertyId", (int)propertyId } };
            return SendCommand<string>(ExecutorHandle, Mt5CommandType.HistoryDealGetString, cmdParams);
        }

        ///<summary>
        ///Close all open positions. 
        ///</summary>
        [Obsolete("OrderCloseAll is deprecated, please use PositionCloseAll instead.")]
        public bool OrderCloseAll()
        {
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.OrderCloseAll);
        }

        ///<summary>
        ///Close all open positions. Returns count of closed positions.
        ///</summary>
        public int PositionCloseAll()
        {
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.PositionCloseAll);
        }

        ///<summary>
        ///Closes a position with the specified ticket.
        ///</summary>
        ///<param name="ticket">Ticket of the closed position.</param>
        ///<param name="deviation">Maximal deviation from the current price (in points).</param>
        public bool PositionClose(ulong ticket, ulong deviation = ulong.MaxValue)
        {
            return PositionClose(ticket, deviation, out MqlTradeResult? result);
        }

        /// <summary>
        /// Modifies existing position
        /// </summary>
        /// <param name="ticket">>Ticket of the position</param>
        /// <param name="sl">Stop loss</param>
        /// <param name="tp">Take profit</param>
        /// <returns></returns>
        public bool PositionModify(ulong ticket, double sl, double tp)
        {
            Dictionary<string, object> cmdParams = new() { { "Ticket", ticket }, { "Sl", sl }, { "Tp", tp } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.PositionModify, cmdParams);
        }

        ///<summary>
        ///Closes a position with the specified ticket.
        ///</summary>
        ///<param name="ticket">Ticket of the closed position.</param>
        ///<param name="deviation">Maximal deviation from the current price (in points).</param>
        /// <param name="result">output result</param>
        public bool PositionClose(ulong ticket, ulong deviation, out MqlTradeResult? result)
        {
            Dictionary<string, object> cmdParams = new() { { "Ticket", ticket }, { "Deviation", deviation } };
            var response = SendCommand<FuncResult<MqlTradeResult>>(ExecutorHandle, Mt5CommandType.PositionClose, cmdParams);

            result = response?.Result;
            return response != null && response.RetVal;
        }

        ///<summary>
        ///Closes a position with the specified ticket.
        ///</summary>
        ///<param name="ticket">Ticket of the closed position.</param>
        /// <param name="result">output result</param>
        public bool PositionClose(ulong ticket, out MqlTradeResult? result)
        {
            return PositionClose(ticket, ulong.MaxValue, out result);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "OrderType", (int)orderType },
                { "Volume", volume }, { "Price", price }, { "Sl", sl }, { "Tp", tp }, { "Comment", comment ?? string.Empty } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.PositionOpen, cmdParams);
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
        public bool PositionOpen(string symbol, ENUM_ORDER_TYPE orderType, double volume, double price, double sl, double tp, string? comment, out MqlTradeResult? result)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty}, { "OrderType", (int)orderType },
                { "Volume", volume }, { "Price", price }, { "Sl", sl }, { "Tp", tp } };
            if (comment != null)
                cmdParams["Comment"] = comment;
            var response = SendCommand<FuncResult<MqlTradeResult>>(ExecutorHandle, Mt5CommandType.PositionOpen2, cmdParams);

            result = response?.Result;
            return response != null && response.RetVal;
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
        /// <param name="result">output result</param>
        /// <returns>true - successful check of the basic structures, otherwise - false.</returns>
        public bool PositionOpen(string symbol, ENUM_ORDER_TYPE orderType, double volume, double price, double sl, double tp, out MqlTradeResult? result)
        {
            return PositionOpen(symbol, orderType, volume, price, sl, tp, "", out result);
        }

        /// <summary>
        /// Partially closes a position on a specified symbol in case of a "hedging" accounting.
        /// </summary>
        /// <param name="symbol">Name of a trading instrument, on which a position is closed partially.</param>
        /// <param name="volume"> Volume, by which a position should be decreased. If the value exceeds the volume of a partially closed position, it is closed in full. No position in the opposite direction is opened.</param>
        /// <param name="deviation">The maximum deviation from the current price (in points).</param>
        /// <returns>true if the basic check of structures is successful, otherwise false.</returns>
        public bool PositionClosePartial(string symbol, double volume, ulong deviation = ulong.MaxValue)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol }, { "Volume", volume },
                { "Deviation", deviation} };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.PositionClosePartial_bySymbol, cmdParams);
        }

        /// <summary>
        /// Partially closes a position on a specified symbol in case of a "hedging" accounting.
        /// </summary>
        /// <param name="ticket">Closed position ticket.</param>
        /// <param name="volume"> Volume, by which a position should be decreased. If the value exceeds the volume of a partially closed position, it is closed in full. No position in the opposite direction is opened.</param>
        /// <param name="deviation">The maximum deviation from the current price (in points).</param>
        /// <returns>true if the basic check of structures is successful, otherwise false.</returns>
        public bool PositionClosePartial(ulong ticket, double volume, ulong deviation = ulong.MaxValue)
        {
            Dictionary<string, object> cmdParams = new() { { "Ticket", ticket }, { "Volume", volume },
                { "Deviation", deviation} };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.PositionClosePartial_byTicket, cmdParams);
        }

        /// <summary>
        /// Opens a long position with specified parameters with current market Ask price
        /// </summary>
        /// <param name="result">output result</param>
        /// <param name="volume">Requested position volume.</param>
        /// <param name="symbol">Position symbol. If it is not specified, the current symbol will be used.</param>
        /// <param name="price">Execution price.</param>
        /// <param name="sl">Stop Loss price.</param>
        /// <param name="tp">Take Profit price.</param>
        /// <param name="comment">Comment.</param>
        /// <returns>true - successful check of the structures, otherwise - false.</returns>
        public bool Buy(out MqlTradeResult? result, double volume, string? symbol = null, double price = 0.0, double sl = 0.0, double tp = 0.0, string? comment = null)
        {
            Dictionary<string, object> cmdParams = new() { { "Volume", volume }, { "Price", price }, { "Sl", sl }, { "Tp", tp } };
            if (symbol != null)
                cmdParams["Symbol"] = symbol;
            if (comment != null)
                cmdParams["Comment"] = comment;

            var response = SendCommand<FuncResult<MqlTradeResult>>(ExecutorHandle, Mt5CommandType.Buy, cmdParams);

            result = response?.Result;
            return response != null && response.RetVal;
        }

        /// <summary>
        /// Opens a short position with specified parameters with current market Bid price
        /// </summary>
        /// <param name="result">output result</param>
        /// <param name="volume">Requested position volume.</param>
        /// <param name="symbol">Position symbol. If it is not specified, the current symbol will be used.</param>
        /// <param name="price">Execution price.</param>
        /// <param name="sl">Stop Loss price.</param>
        /// <param name="tp">Take Profit price.</param>
        /// <param name="comment">Comment.</param>
        /// <returns>true - successful check of the structures, otherwise - false.</returns>
        public bool Sell(out MqlTradeResult? result, double volume, string? symbol = null, double price = 0.0, double sl = 0.0, double tp = 0.0, string? comment = null)
        {
            Dictionary<string, object> cmdParams = new() { { "Volume", volume }, { "Price", price }, { "Sl", sl }, { "Tp", tp } };
            if (symbol != null)
                cmdParams["Symbol"] = symbol;
            if (comment != null)
                cmdParams["Comment"] = comment;

            var response = SendCommand<FuncResult<MqlTradeResult>>(ExecutorHandle, Mt5CommandType.Sell, cmdParams);

            result = response?.Result;
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
            Dictionary<string, object> cmdParams = new() { { "PropertyId", propertyId } };
            return SendCommand<double>(ExecutorHandle, Mt5CommandType.AccountInfoDouble, cmdParams);
        }

        ///<summary>
        ///Returns the value of the corresponding account property. 
        ///</summary>
        ///<param name="propertyId">Identifier of the property.</param>
        public long AccountInfoInteger(ENUM_ACCOUNT_INFO_INTEGER propertyId)
        {
            Dictionary<string, object> cmdParams = new() { { "PropertyId", propertyId } };
            return SendCommand<long>(ExecutorHandle, Mt5CommandType.AccountInfoInteger, cmdParams);
        }

        ///<summary>
        ///Returns the value of the corresponding account property. 
        ///</summary>
        ///<param name="propertyId">Identifier of the property.</param>
        public string? AccountInfoString(ENUM_ACCOUNT_INFO_STRING propertyId)
        {
            Dictionary<string, object> cmdParams = new() { { "PropertyId", propertyId } };
            return SendCommand<string>(ExecutorHandle, Mt5CommandType.AccountInfoString, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty },
                { "Timeframe", (int)timeframe }, { "PropId", (int)propId } };
            return SendCommand<long>(ExecutorHandle, Mt5CommandType.SeriesInfoInteger, cmdParams);
        }

        ///<summary>
        ///Returns the number of bars count in the history for a specified symbol and period.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe"> Period.</param>
        public int Bars(string symbolName, ENUM_TIMEFRAMES timeframe)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.Bars, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartTime",  Mt5TimeConverter.ConvertToMtTime(startTime) },
                { "StopTime",  Mt5TimeConverter.ConvertToMtTime(stopTime)} };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.Bars2, cmdParams);
        }

        ///<summary>
        ///Returns the number of calculated data for the specified indicator.
        ///</summary>
        ///<param name="indicatorHandle">The indicator handle, returned by the corresponding indicator function.</param>
        public int BarsCalculated(int indicatorHandle)
        {
            Dictionary<string, object> cmdParams = new() { { "IndicatorHandle", indicatorHandle } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.BarsCalculated, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "IndicatorHandle", indicatorHandle },
                { "BufferNum", bufferNum }, { "StartPos", startPos }, { "Count", count } };
            var response = SendCommand<List<double>>(ExecutorHandle, Mt5CommandType.CopyBuffer, cmdParams);
            buffer = response?.ToArray() ?? [];
            return response?.Count ?? 0;
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
            Dictionary<string, object> cmdParams = new() { { "IndicatorHandle", indicatorHandle },
                { "BufferNum", bufferNum }, { "StartTime", Mt5TimeConverter.ConvertToMtTime(startTime) }, { "Count", count } };
            var response = SendCommand<List<double>>(ExecutorHandle, Mt5CommandType.CopyBuffer1, cmdParams);
            buffer = response?.ToArray() ?? [];
            return response?.Count ?? 0;
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
            Dictionary<string, object> cmdParams = new() { { "IndicatorHandle", indicatorHandle },
                { "BufferNum", bufferNum }, { "StartTime", Mt5TimeConverter.ConvertToMtTime(startTime) },
                 { "StopTime", Mt5TimeConverter.ConvertToMtTime(stopTime) } };
            var response = SendCommand<List<double>>(ExecutorHandle, Mt5CommandType.CopyBuffer2, cmdParams);
            buffer = response?.ToArray() ?? [];
            return response?.Count ?? 0;
        }

        ///<summary>
        ///Gets history data of MqlRates structure of a specified symbol-period in specified quantity into the ratesArray array. The elements ordering of the copied data is from present to the past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startPos">The start position for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="ratesArray">Array of MqlRates type.</param>
        public int CopyRates(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count, out MqlRates[]? ratesArray)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartPos", startPos }, { "Count", count } };
            var response = SendCommand<List<MqlRates>>(ExecutorHandle, Mt5CommandType.CopyRates, cmdParams);
            ratesArray = response?.ToArray() ?? [];
            return response?.Count ?? 0;
        }
            
        ///<summary>
        ///Gets history data of MqlRates structure of a specified symbol-period in specified quantity into the ratesArray array. The elements ordering of the copied data is from present to the past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">The start time for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="ratesArray">Array of MqlRates type.</param>
        public int CopyRates(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, int count, out MqlRates[]? ratesArray)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartTime",  Mt5TimeConverter.ConvertToMtTime(startTime) }, { "Count", count } };
            var response = SendCommand<List<MqlRates>>(ExecutorHandle, Mt5CommandType.CopyRates1, cmdParams);
            ratesArray = response?.ToArray() ?? [];
            return response?.Count ?? 0;
        }

        ///<summary>
        ///Gets history data of MqlRates structure of a specified symbol-period in specified quantity into the ratesArray array. The elements ordering of the copied data is from present to the past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">The start time for the first element to copy.</param>
        ///<param name="stopTime">Bar time, corresponding to the last element to copy.</param>
        ///<param name="ratesArray">Array of MqlRates type.</param>
        public int CopyRates(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, DateTime stopTime, out MqlRates[]? ratesArray)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartTime",  Mt5TimeConverter.ConvertToMtTime(startTime) },
                { "StopTime", Mt5TimeConverter.ConvertToMtTime(stopTime) } };
            var response = SendCommand<List<MqlRates>>(ExecutorHandle, Mt5CommandType.CopyRates2, cmdParams);
            ratesArray = response?.ToArray() ?? [];
            return response?.Count ?? 0;
        }

        ///<summary>
        ///The function gets to time_array history data of bar opening time for the specified symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startPos">The start position for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="timeArray">Array of DatetTme type.</param>
        public int CopyTime(string symbolName, ENUM_TIMEFRAMES timeframe, int startPos, int count, out DateTime[]? timeArray)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartPos", startPos }, { "Count", count } };
            var response = SendCommand<List<long>>(ExecutorHandle, Mt5CommandType.CopyTime, cmdParams);
            if (response != null)
            {
                timeArray = new DateTime[response.Count];
                for (var i = 0; i < response.Count; i++)
                    timeArray[i] = Mt5TimeConverter.ConvertFromMtTime(response[i]);
            }
            else
                timeArray = [];
            return response?.Count ?? 0;
        }

        ///<summary>
        ///The function gets to time_array history data of bar opening time for the specified symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">The start time for the first element to copy.</param>
        ///<param name="count">Data count to copy.</param>
        ///<param name="timeArray">Array of DatetTme type.</param>
        public int CopyTime(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, int count, out DateTime[]? timeArray)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartTime", Mt5TimeConverter.ConvertToMtTime(startTime) }, { "Count", count } };
            var response = SendCommand<List<long>>(ExecutorHandle, Mt5CommandType.CopyTime1, cmdParams);
            if (response != null)
            {
                timeArray = new DateTime[response.Count];
                for (var i = 0; i < response.Count; i++)
                    timeArray[i] = Mt5TimeConverter.ConvertFromMtTime(response[i]);
            }
            else
                timeArray = [];
            return response?.Count ?? 0;
        }

        ///<summary>
        ///The function gets to time_array history data of bar opening time for the specified symbol-period pair in the specified quantity. It should be noted that elements ordering is from present to past, i.e., starting position of 0 means the current bar.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="timeframe">Period.</param>
        ///<param name="startTime">The start time for the first element to copy.</param>
        ///<param name="stopTime">Bar time corresponding to the last element to copy.</param>
        ///<param name="timeArray">Array of DatetTme type.</param>
        public int CopyTime(string symbolName, ENUM_TIMEFRAMES timeframe, DateTime startTime, DateTime stopTime, out DateTime[]? timeArray)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartTime", Mt5TimeConverter.ConvertToMtTime(startTime) },
                { "StopTime", Mt5TimeConverter.ConvertToMtTime(stopTime) } };
            var response = SendCommand<List<long>>(ExecutorHandle, Mt5CommandType.CopyTime2, cmdParams);
            if (response != null)
            {
                timeArray = new DateTime[response.Count];
                for (var i = 0; i < response.Count; i++)
                    timeArray[i] = Mt5TimeConverter.ConvertFromMtTime(response[i]);
            }
            else
                timeArray = [];
            return response?.Count ?? 0;
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartPos", startPos }, { "Count", count } };
            var response = SendCommand<List<double>>(ExecutorHandle, Mt5CommandType.CopyOpen, cmdParams);
            openArray = response != null ? response.ToArray() : [];
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartTime", Mt5TimeConverter.ConvertToMtTime(startTime) }, { "Count", count } };
            var response = SendCommand<List<double>>(ExecutorHandle, Mt5CommandType.CopyOpen1, cmdParams);
            openArray = response != null ? response.ToArray() : [];
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartTime", Mt5TimeConverter.ConvertToMtTime(startTime) },
                { "StopTime", Mt5TimeConverter.ConvertToMtTime(stopTime) } };
            var response = SendCommand<List<double>>(ExecutorHandle, Mt5CommandType.CopyOpen2, cmdParams);
            openArray = response != null ? response.ToArray() : [];
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartPos", startPos }, { "Count", count } };
            var response = SendCommand<List<double>>(ExecutorHandle, Mt5CommandType.CopyHigh, cmdParams);
            highArray = response != null ? response.ToArray() : [];
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                 { "StartTime", Mt5TimeConverter.ConvertToMtTime(startTime) }, { "Count", count } };
            var response = SendCommand<List<double>>(ExecutorHandle, Mt5CommandType.CopyHigh1, cmdParams);
            highArray = response != null ? response.ToArray() : [];
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                 { "StartTime", Mt5TimeConverter.ConvertToMtTime(startTime) }, 
                 { "StopTime", Mt5TimeConverter.ConvertToMtTime(stopTime) } };
            var response = SendCommand<List<double>>(ExecutorHandle, Mt5CommandType.CopyHigh2, cmdParams);
            highArray = response != null ? response.ToArray() : [];
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartPos", startPos }, { "Count", count } };
            var response = SendCommand<List<double>>(ExecutorHandle, Mt5CommandType.CopyLow, cmdParams);
            lowArray = response != null ? response.ToArray() : [];
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartTime", Mt5TimeConverter.ConvertToMtTime(startTime) }, { "Count", count } };
            var response = SendCommand<List<double>>(ExecutorHandle, Mt5CommandType.CopyLow1, cmdParams);
            lowArray = response != null ? response.ToArray() : [];
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartTime", Mt5TimeConverter.ConvertToMtTime(startTime) },
                { "StopTime", Mt5TimeConverter.ConvertToMtTime(stopTime) } };
            var response = SendCommand<List<double>>(ExecutorHandle, Mt5CommandType.CopyLow2, cmdParams);
            lowArray = response != null ? response.ToArray() : [];
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartPos", startPos }, { "Count", count } };
            var response = SendCommand<List<double>>(ExecutorHandle, Mt5CommandType.CopyClose, cmdParams);
            closeArray = response != null ? response.ToArray() : [];
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartTime", Mt5TimeConverter.ConvertToMtTime(startTime) }, { "Count", count } };
            var response = SendCommand<List<double>>(ExecutorHandle, Mt5CommandType.CopyClose1, cmdParams);
            closeArray = response != null ? response.ToArray() : [];
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartTime", Mt5TimeConverter.ConvertToMtTime(startTime) },
                { "StopTime", Mt5TimeConverter.ConvertToMtTime(stopTime) } };
            var response = SendCommand<List<double>>(ExecutorHandle, Mt5CommandType.CopyClose2, cmdParams);
            closeArray = response != null ? response.ToArray() : [];
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartPos", startPos }, { "Count", count } };
            var response = SendCommand<List<long>>(ExecutorHandle, Mt5CommandType.CopyTickVolume, cmdParams);
            volumeArray = response != null ? response.ToArray() : [];
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                 { "StartTime", Mt5TimeConverter.ConvertToMtTime(startTime) }, { "Count", count } };
            var response = SendCommand<List<long>>(ExecutorHandle, Mt5CommandType.CopyTickVolume1, cmdParams);
            volumeArray = response != null ? response.ToArray() : [];
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                 { "StartTime", Mt5TimeConverter.ConvertToMtTime(startTime) },
                 { "StopTime", Mt5TimeConverter.ConvertToMtTime(stopTime) } };
            var response = SendCommand<List<long>>(ExecutorHandle, Mt5CommandType.CopyTickVolume2, cmdParams);
            volumeArray = response != null ? response.ToArray() : [];
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartPos", startPos }, { "Count", count } };
            var response = SendCommand<List<long>>(ExecutorHandle, Mt5CommandType.CopyRealVolume, cmdParams);
            volumeArray = response != null ? response.ToArray() : [];
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartTime", Mt5TimeConverter.ConvertToMtTime(startTime) }, { "Count", count } };
            var response = SendCommand<List<long>>(ExecutorHandle, Mt5CommandType.CopyRealVolume1, cmdParams);
            volumeArray = response != null ? response.ToArray() : [];
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartTime", Mt5TimeConverter.ConvertToMtTime(startTime) },
                { "StopTime", Mt5TimeConverter.ConvertToMtTime(stopTime) } };
            var response = SendCommand<List<long>>(ExecutorHandle, Mt5CommandType.CopyRealVolume2, cmdParams);
            volumeArray = response != null ? response.ToArray() : [];
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartPos", startPos }, { "Count", count } };
            var response = SendCommand<List<int>>(ExecutorHandle, Mt5CommandType.CopySpread, cmdParams);
            spreadArray = response != null ? response.ToArray() : [];
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
               { "StartTime", Mt5TimeConverter.ConvertToMtTime(startTime) }, { "Count", count } };
            var response = SendCommand<List<int>>(ExecutorHandle, Mt5CommandType.CopySpread1, cmdParams);
            spreadArray = response != null ? response.ToArray() : [];
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Timeframe", (int)timeframe },
                { "StartTime", Mt5TimeConverter.ConvertToMtTime(startTime) },
                { "StopTime", Mt5TimeConverter.ConvertToMtTime(stopTime) } };
            var response = SendCommand<List<int>>(ExecutorHandle, Mt5CommandType.CopySpread2, cmdParams);
            spreadArray = response != null ? response.ToArray() : [];
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
        public List<MqlTick>? CopyTicks(string symbolName, CopyTicksFlag flags = CopyTicksFlag.All, ulong from = 0, uint count = 0)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Flags", flags },
                { "From", from }, { "Count", count }  };
            var response = SendCommand<List<MtTick>>(ExecutorHandle, Mt5CommandType.CopyTicks, cmdParams);
            List<MqlTick>? ticks = response?.Select(t => new MqlTick(t)).ToList();
            return ticks;
        }

        ///<summary>
        ///The function returns the handle of a specified technical indicator created based on the array of parameters of MqlParam type.
        ///</summary>
        ///<param name="symbol">Name of a symbol, on data of which the indicator is calculated. NULL means the current symbol.</param>
        ///<param name="period">The value of the timeframe can be one of values of the ENUM_TIMEFRAMES enumeration, 0 means the current timeframe.</param>
        ///<param name="indicatorType">Indicator type, can be one of values of the ENUM_INDICATOR enumeration.</param>
        ///<param name="parameters">An array of MqlParam type, whose elements contain the type and value of each input parameter of a technical indicator.</param>
        public int IndicatorCreate(string? symbol, ENUM_TIMEFRAMES period, ENUM_INDICATOR indicatorType, List<MqlParam>? parameters = null)
        {
            Dictionary<string, object> cmdParams = new() { { "Period", (int)period }, { "IndicatorType", (int)indicatorType } };
            if (symbol != null)
                cmdParams["Symbol"] = symbol;
            if (parameters != null)
                cmdParams["Parameters"] = parameters;
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.IndicatorCreate, cmdParams);
        }

        public bool IndicatorRelease(int indicatorHandle)
        {
            Dictionary<string, object> cmdParams = new() { { "IndicatorHandle", indicatorHandle } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.IndicatorRelease, cmdParams);
        }
        #endregion

        #region Market Info

        ///<summary>
        ///Returns the number of available (selected in Market Watch or all) symbols.
        ///</summary>
        ///<param name="selected">Request mode. Can be true or false.</param>
        public int SymbolsTotal(bool selected)
        {
            Dictionary<string, object> cmdParams = new() { { "Selected", selected } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.SymbolsTotal, cmdParams);
        }

        ///<summary>
        ///Returns the name of a symbol.
        ///</summary>
        ///<param name="pos">Order number of a symbol.</param>
        ///<param name="selected">Request mode. If the value is true, the symbol is taken from the list of symbols selected in MarketWatch. If the value is false, the symbol is taken from the general list.</param>
        public string? SymbolName(int pos, bool selected)
        {
            Dictionary<string, object> cmdParams = new() { { "Pos", pos }, { "Selected", selected } };
            return SendCommand<string>(ExecutorHandle, Mt5CommandType.SymbolName, cmdParams);
        }

        ///<summary>
        ///Selects a symbol in the Market Watch window or removes a symbol from the window.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="selected">Switch. If the value is false, a symbol should be removed from MarketWatch, otherwise a symbol should be selected in this window. A symbol can't be removed if the symbol chart is open, or there are open positions for this symbol.</param>
        public bool SymbolSelect(string symbolName, bool selected)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "Selected", selected } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.SymbolSelect, cmdParams);
        }

        ///<summary>
        ///The function checks whether data of a selected symbol in the terminal are synchronized with data on the trade server.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        public bool SymbolIsSynchronized(string symbolName)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.SymbolIsSynchronized, cmdParams);
        }

        ///<summary>
        ///Returns the corresponding property of a specified symbol.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="propId">Identifier of a symbol property.</param>
        public double SymbolInfoDouble(string symbolName, ENUM_SYMBOL_INFO_DOUBLE propId)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "PropId", (int)propId } };
            return SendCommand<double>(ExecutorHandle, Mt5CommandType.SymbolInfoDouble, cmdParams);
        }

        ///<summary>
        ///Returns the corresponding property of a specified symbol.
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="propId">Identifier of a symbol property.</param>
        public long SymbolInfoInteger(string symbolName, ENUM_SYMBOL_INFO_INTEGER propId)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "PropId", (int)propId } };
            return SendCommand<long>(ExecutorHandle, Mt5CommandType.SymbolInfoInteger, cmdParams);
        }

        ///<summary>
        ///Returns the corresponding property of a specified symbol. 
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="propId">Identifier of a symbol property.</param>
        public string? SymbolInfoString(string symbolName, ENUM_SYMBOL_INFO_STRING propId)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "PropId", (int)propId } };
            var response = SendCommand<string>(ExecutorHandle, Mt5CommandType.SymbolInfoString, cmdParams);
            return response;
        }

        ///<summary>
        ///Returns the corresponding property of a specified symbol. 
        ///</summary>
        ///<param name="symbolName">Symbol name.</param>
        ///<param name="propId">Identifier of a symbol property.</param>
        ///<param name="value">Variable of the string type receiving the value of the requested property.</param>
        public bool SymbolInfoString(string symbolName, ENUM_SYMBOL_INFO_STRING propId, out string? value)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbolName ?? string.Empty }, { "PropId", (int)propId } };
            var response = SendCommand<FuncResult<string>>(ExecutorHandle, Mt5CommandType.SymbolInfoString2, cmdParams);
            value = response?.Result;
            return response?.RetVal ?? false;
        }

        ///<summary>
        ///The function returns current prices of a specified symbol in a variable of the MqlTick type.
        ///The function returns true if successful, otherwise returns false.
        ///</summary>
        ///<param name="symbol">Symbol name.</param>
        ///<param name="tick"> Link to the structure of the MqlTick type, to which the current prices and time of the last price update will be placed.</param>
        public bool SymbolInfoTick(string symbol, out MqlTick? tick)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty } };
            var response = SendCommand<FuncResult<MtTick>>(ExecutorHandle, Mt5CommandType.SymbolInfoTick, cmdParams);
            tick = response != null && response.Result != null ? new MqlTick(response.Result) : null;
            return response?.RetVal ?? false;
        }

        ///<summary>
        ///The function returns current prices of a specified symbol in a variable of the MqlTick type.
        ///</summary>
        ///<param name="symbol">Symbol name.</param>
        public MqlTick? SymbolInfoTick(string symbol)
        {
            if (SymbolInfoTick(symbol, out MqlTick? tick))
                return tick;
            return null;
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", name ?? string.Empty }, { "DayOfWeek", dayOfWeek },
                { "SessionIndex", sessionIndex } };

            var response = SendCommand<FuncResult<Dictionary<string,int>>>(ExecutorHandle,
                Mt5CommandType.SymbolInfoSessionQuote, cmdParams);
            if (response != null && response.Result != null
                && response.Result.TryGetValue("From", out int mtFrom)
                && response.Result.TryGetValue("To", out int mtTo))
            {
                from = Mt5TimeConverter.ConvertFromMtTime(mtFrom);
                to = Mt5TimeConverter.ConvertFromMtTime(mtTo);
                return response.RetVal;
            }
            from = DateTime.MinValue;
            to = DateTime.MinValue;
            return false;
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", name ?? string.Empty }, { "DayOfWeek", dayOfWeek },
                { "SessionIndex", sessionIndex } };

            var response = SendCommand<FuncResult<Dictionary<string,int>>>(ExecutorHandle,
                Mt5CommandType.SymbolInfoSessionTrade, cmdParams);
            if (response != null && response.Result != null
                && response.Result.TryGetValue("From", out int mtFrom)
                && response.Result.TryGetValue("To", out int mtTo))
            {
                from = Mt5TimeConverter.ConvertFromMtTime(mtFrom);
                to = Mt5TimeConverter.ConvertFromMtTime(mtTo);
                return response.RetVal;
            }
            from = DateTime.MinValue;
            to = DateTime.MinValue;
            return false;
        }

        ///<summary>
        ///Provides opening of Depth of Market for a selected symbol, and subscribes for receiving notifications of the DOM changes.
        ///</summary>
        ///<param name="symbol">Symbol name.</param>
        public bool MarketBookAdd(string symbol)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.MarketBookAdd, cmdParams);
        }

        ///<summary>
        ///Provides closing of Depth of Market for a selected symbol, and cancels the subscription for receiving notifications of the DOM changes.
        ///</summary>
        ///<param name="symbol">Symbol name.</param>
        public bool MarketBookRelease(string symbol)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.MarketBookRelease, cmdParams);
        }

        ///<summary>
        ///Returns a structure array MqlBookInfo containing records of the Depth of Market of a specified symbol.
        ///</summary>
        ///<param name="symbol">Symbol name.</param>
        ///<param name="book">Reference to an array of Depth of Market records.</param>        
        public bool MarketBookGet(string symbol, out MqlBookInfo[]? book)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty } };
            var response = SendCommand<List<MqlBookInfo>>(ExecutorHandle, Mt5CommandType.MarketBookGet, cmdParams);
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
            return ChartId(ExecutorHandle);
        }

        ///<summary>
        ///Returns the ID of the chart.
        ///</summary>
        ///<param name="expertHandle">Handle of expert linked to the chart.</param>
        ///<returns>
        /// Value of long type.
        ///</returns>
        public long ChartId(int expertHandle)
        {
            return SendCommand<long>(ExecutorHandle, Mt5CommandType.ChartId, expertHandle);
        }

        ///<summary>
        ///This function calls a forced redrawing of a specified chart.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        public void ChartRedraw(long chartId = 0)
        {
            Dictionary<string, long> cmdParams = new() { { "ChartId", chartId } };
            SendCommand<object>(ExecutorHandle, Mt5CommandType.ChartRedraw, cmdParams);
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
                { "TemplateFileName", filename ?? string.Empty } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.ChartApplyTemplate, cmdParams);
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
                { "TemplateFileName", filename ?? string.Empty } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.ChartSaveTemplate, cmdParams);
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
                { "IndicatorShortname", indicatorShortname ?? string.Empty } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.ChartWindowFind, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId }, { "SubWindow", subWindow },
                { "Time", Mt5TimeConverter.ConvertToMtTime(time) }, { "Price", price } };

            var response = SendCommand<FuncResult<Dictionary<string,int>>>(ExecutorHandle, Mt5CommandType.ChartTimePriceToXY, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId }, { "X", x }, { "Y", y } };
            var response = SendCommand<FuncResult<Dictionary<string,object>>>(ExecutorHandle, Mt5CommandType.ChartXYToTimePrice, cmdParams);
            if (response != null && response.Result != null
                && response.Result.TryGetValue("SubWindow", out object? mtSubWindow)
                && response.Result.TryGetValue("Time", out object? mtTime)
                && response.Result.TryGetValue("Price", out object? mtPrice))
            {
                subWindow = Convert.ToInt32(mtSubWindow);
                time = Mt5TimeConverter.ConvertFromMtTime(Convert.ToInt32(mtTime));
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Timeframe", (int)period } };
            return SendCommand<long>(ExecutorHandle, Mt5CommandType.ChartOpen, cmdParams);
        }

        ///<summary>
        ///Returns the ID of the first chart of the client terminal.
        ///</summary>
        public long ChartFirst()
        {
            return SendCommand<long>(ExecutorHandle, Mt5CommandType.ChartFirst);
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
            Dictionary<string, long> cmdParams = new() { { "ChartId", chartId } };
            return SendCommand<long>(ExecutorHandle, Mt5CommandType.ChartNext, cmdParams);
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
            Dictionary<string, long> cmdParams = new() { { "ChartId", chartId } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.ChartClose, cmdParams);
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
            Dictionary<string, long> cmdParams = new() { { "ChartId", chartId } };
            return SendCommand<string>(ExecutorHandle, Mt5CommandType.ChartSymbol, cmdParams);
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
            Dictionary<string, long> cmdParams = new() { { "ChartId", chartId } };
            return (ENUM_TIMEFRAMES)SendCommand<int>(ExecutorHandle, Mt5CommandType.ChartPeriod, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "PropId", (int)propId }, { "Value", value } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.ChartSetDouble, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "PropId", (int)propId }, { "Value", value } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.ChartSetInteger, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "PropId", (int)propId }, { "Value", value } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.ChartSetString, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "PropId", (int)propId }, { "SubWindow", subWindow } };
            return SendCommand<double>(ExecutorHandle, Mt5CommandType.ChartGetDouble, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "PropId", (int)propId }, { "SubWindow", subWindow } };
            return SendCommand<long>(ExecutorHandle, Mt5CommandType.ChartGetInteger, cmdParams);
        }

        ///<summary>
        ///Returns the value of a corresponding property of the specified chart. Chart property must be of string type.
        ///</summary>
        ///<param name="chartId">Chart ID. 0 means the current chart.</param>
        ///<param name="propId">Chart property ID. This value can be one of the ENUM_CHART_PROPERTY_STRING values.</param>
        ///<returns>
        ///The value of string type.
        ///</returns>
        public string? ChartGetString(long chartId, ENUM_CHART_PROPERTY_STRING propId)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "PropId", (int)propId } };
            return SendCommand<string>(ExecutorHandle, Mt5CommandType.ChartGetString, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "Position", (int)position }, { "Shift", shift } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.ChartNavigate, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "SubWindow", subWindow }, { "IndicatorHandle", indicatorHandle } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.ChartIndicatorAdd, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "SubWindow", subWindow }, { "IndicatorShortname", indicatorShortname ?? string.Empty } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.ChartIndicatorDelete, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "SubWindow", subWindow }, { "IndicatorShortname", indicatorShortname ?? string.Empty } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.ChartIndicatorGet, cmdParams);
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
                { "SubWindow", subWindow }, { "Index", index } };
            return SendCommand<string>(ExecutorHandle, Mt5CommandType.ChartIndicatorName, cmdParams);
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
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.ChartIndicatorsTotal, cmdParams);
        }

        ///<summary>
        ///Returns the number (index) of the chart subwindow the Expert Advisor or script has been dropped to. 0 means the main chart window.
        ///</summary>
        public int ChartWindowOnDropped()
        {
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.ChartWindowOnDropped);
        }

        ///<summary>
        ///Returns the price coordinate corresponding to the chart point the Expert Advisor or script has been dropped to.
        ///</summary>
        public double ChartPriceOnDropped()
        {
            return SendCommand<double>(ExecutorHandle, Mt5CommandType.ChartPriceOnDropped);
        }

        ///<summary>
        ///Returns the time coordinate corresponding to the chart point the Expert Advisor or script has been dropped to.
        ///</summary>
        public DateTime ChartTimeOnDropped()
        {
            var res = SendCommand<int>(ExecutorHandle, Mt5CommandType.ChartTimeOnDropped);
            return Mt5TimeConverter.ConvertFromMtTime(res);
        }

        ///<summary>
        ///Returns the X coordinate of the chart point the Expert Advisor or script has been dropped to.
        ///</summary>
        public int ChartXOnDropped()
        {
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.ChartXOnDropped);
        }

        ///<summary>
        ///Returns the Y coordinateof the chart point the Expert Advisor or script has been dropped to.
        ///</summary>
        public int ChartYOnDropped()
        {
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.ChartYOnDropped);
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
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId }, { "Symbol", symbol ?? string.Empty },
                { "Period", (int)period } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.ChartSetSymbolPeriod, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId }, { "Filename", filename ?? string.Empty },
                { "Width", width }, { "Height", height }, { "AlignMode", (int)alignMode } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.ChartScreenShot, cmdParams);
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
        public string? TerminalInfoString(ENUM_TERMINAL_INFO_STRING propertyId)
        {
            Dictionary<string, int> cmdParams = new() { { "PropertyId", (int)propertyId } };
            return SendCommand<string>(ExecutorHandle, Mt5CommandType.TerminalInfoString, cmdParams);
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
            Dictionary<string, int> cmdParams = new() { { "PropertyId", (int)propertyId } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.TerminalInfoInteger, cmdParams);
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
            Dictionary<string, int> cmdParams = new() { { "PropertyId", (int)propertyId } };
            return SendCommand<double>(ExecutorHandle, Mt5CommandType.TerminalInfoDouble, cmdParams);
        }
        #endregion 


        #region Common Functions

        ///<summary>
        ///It enters a message in the Expert Advisor
        ///</summary>
        ///<param name="message">Message</param>
        public bool Print(string message)
        {
            Dictionary<string, object> cmdParams = new() { { "PrintMsg", message ?? string.Empty } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.Print, cmdParams);
        }

        ///<summary>
        ///Displays a message in a separate window.
        ///</summary>
        ///<param name="message">Message</param>
        public void Alert(string message)
        {
            Dictionary<string, object> cmdParams = new() { { "Message", message ?? string.Empty } };
            SendCommand<object>(ExecutorHandle, Mt5CommandType.Alert, cmdParams);
        }

        ///<summary>
        ///Gives program operation completion command when testing.
        ///</summary>
        public void TesterStop()
        {
            SendCommand<object>(ExecutorHandle, Mt5CommandType.TesterStop);
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
        ///<param name="listOfCoordinates">List of further anchor points (tuple of time and price).</param>
        public bool ObjectCreate(long chartId, string name, ENUM_OBJECT type, int nwin, DateTime time, double price, List<Tuple<DateTime, double>>? listOfCoordinates = null)
        {
            //Count the additional coordinates
            int iAdditionalCoordinates = (listOfCoordinates != null) ? listOfCoordinates.Count : 0;
            if(iAdditionalCoordinates > 29)
                throw new ArgumentOutOfRangeException(nameof(listOfCoordinates), "The maximum amount of coordinates in 30.");

            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId }, { "Name", name ?? string.Empty },
                { "Type", (int)type }, { "Nwin", nwin } };

            List<int> times = [];
            List<double> prices = [];

            if (iAdditionalCoordinates > 0 && listOfCoordinates != null)
            {
                foreach (var coordinateTuple in listOfCoordinates)
                {
                    times.Add(Mt5TimeConverter.ConvertToMtTime(coordinateTuple.Item1));
                    prices.Add(coordinateTuple.Item2);
                }
            }

            cmdParams["Times"] = times;
            cmdParams["Prices"] = prices;

            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.ObjectCreate, cmdParams);
        }

        ///<summary>
        ///The function returns the name of the corresponding object in the specified chart, in the specified subwindow, of the specified type.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="pos">Ordinal number of the object according to the specified filter by the number and type of the subwindow.</param>
        ///<param name="subWindow">umber of the chart subwindow. 0 means the main chart window, -1 means all the subwindows of the chart, including the main window.</param>
        ///<param name="type">Type of the object. The value can be one of the values of the ENUM_OBJECT enumeration. -1 means all types.</param>
        public string? ObjectName(long chartId, int pos, int subWindow = -1, int type = -1)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId }, { "Pos", pos },
                { "SubWindow", subWindow }, { "Type", type } };
            return SendCommand<string>(ExecutorHandle, Mt5CommandType.ObjectName, cmdParams);
        }

        ///<summary>
        ///The function removes the object with the specified name from the specified chart.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="name">Name of object to be deleted.</param>
        public bool ObjectDelete(long chartId, string name)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId }, { "Name", name ?? string.Empty } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.ObjectDelete, cmdParams);
        }

        ///<summary>
        ///The function removes the object with the specified name from the specified chart.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="subWindow">Number of the chart subwindow. 0 means the main chart window, -1 means all the subwindows of the chart, including the main window.</param>
        ///<param name="type">Type of the object. The value can be one of the values of the ENUM_OBJECT enumeration. -1 means all types.</param>
        public int ObjectsDeleteAll(long chartId, int subWindow = -1, int type = -1)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "SubWindow", subWindow }, { "Type", type } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.ObjectsDeleteAll, cmdParams);
        }

        ///<summary>
        ///The function searches for an object with the specified name in the chart with the specified ID.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="name">The name of the searched object.</param>
        public int ObjectFind(long chartId, string name)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId }, { "Name", name ?? string.Empty } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.ObjectFind, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId }, { "Name", name ?? string.Empty },
                { "Value", value }, { "LineId", lineId } };
            var res = SendCommand<int>(ExecutorHandle, Mt5CommandType.ObjectGetTimeByValue, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId }, { "Name", name ?? string.Empty },
                { "Time",  Mt5TimeConverter.ConvertToMtTime(time) }, { "LineId", lineId } };
            return SendCommand<double>(ExecutorHandle, Mt5CommandType.ObjectGetValueByTime, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId }, { "Name", name ?? string.Empty }, { "PointIndex", pointIndex },
                { "Time",  Mt5TimeConverter.ConvertToMtTime(time) }, { "Price", price } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.ObjectMove, cmdParams);
        }

        ///<summary>
        ///The function returns the number of objects in the specified chart, specified subwindow, of the specified type.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="subWindow">Number of the chart subwindow. 0 means the main chart window, -1 means all the subwindows of the chart, including the main window.</param>
        ///<param name="type">Type of the object. The value can be one of the values of the ENUM_OBJECT enumeration. -1 means all types.</param>
        public int ObjectsTotal(long chartId, int subWindow = -1, int type = -1)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "SubWindow", subWindow }, { "Type", type } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.ObjectsTotal, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "Name", name ?? string.Empty }, { "PropId", (int)propId }, { "PropValue", propValue} };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.ObjectSetDouble, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "Name", name ?? string.Empty }, { "PropId", (int)propId }, { "PropValue", propValue} };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.ObjectSetInteger, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "Name", name ?? string.Empty }, { "PropId", (int)propId }, { "PropValue", propValue} };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.ObjectSetString, cmdParams);
        }

        ///<summary>
        ///The function returns the value of the corresponding object property. The object property must be of the double type.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="name">Name of the object.</param>
        ///<param name="propId">ID of the object property. The value can be one of the values of the ENUM_OBJECT_PROPERTY_DOUBLE enumeration.</param>
        public double ObjectGetDouble(long chartId, string name, ENUM_OBJECT_PROPERTY_DOUBLE propId)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "Name", name ?? string.Empty }, { "PropId", (int)propId } };
            return SendCommand<double>(ExecutorHandle, Mt5CommandType.ObjectGetDouble, cmdParams);
        }

        ///<summary>
        ///he function returns the value of the corresponding object property. The object property must be of the datetime, int, color, bool or char type.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="name">Name of the object.</param>
        ///<param name="propId">ID of the object property. The value can be one of the values of the ENUM_OBJECT_PROPERTY_INTEGER enumeration.</param>
        public long ObjectGetInteger(long chartId, string name, ENUM_OBJECT_PROPERTY_INTEGER propId)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "Name", name ?? string.Empty }, { "PropId", (int)propId } };
            return SendCommand<long>(ExecutorHandle, Mt5CommandType.ObjectGetInteger, cmdParams);
        }

        ///<summary>
        ///The function returns the value of the corresponding object property. The object property must be of the string type.
        ///</summary>
        ///<param name="chartId">Chart identifier. 0 means the current chart.</param>
        ///<param name="name">Name of the object.</param>
        ///<param name="propId">ID of the object property. The value can be one of the values of the ENUM_OBJECT_PROPERTY_STRING enumeration.</param>
        public string? ObjectGetString(long chartId, string name, ENUM_OBJECT_PROPERTY_STRING propId)
        {
            Dictionary<string, object> cmdParams = new() { { "ChartId", chartId },
                { "Name", name ?? string.Empty }, { "PropId", (int)propId } };
            return SendCommand<string>(ExecutorHandle, Mt5CommandType.ObjectGetString, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iAC, cmdParams);
        }

        ///<summary>
        ///The function returns the handle of the Accumulation/Distribution indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="appliedVolume">The volume used. Can be any of ENUM_APPLIED_VOLUME values.</param>
        public int iAD(string symbol, ENUM_TIMEFRAMES period, ENUM_APPLIED_VOLUME appliedVolume)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "AppliedVolume", appliedVolume } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iAD, cmdParams);
        }

        ///<summary>
        ///The function returns the handle of the Average Directional Movement Index indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="adxPeriod">Period to calculate the index.</param>
        public int iADX(string symbol, ENUM_TIMEFRAMES period, int adxPeriod)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "AdxPeriod", adxPeriod } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iADX, cmdParams);
        }

        ///<summary>
        ///The function returns the handle of Average Directional Movement Index by Welles Wilder.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="adxPeriod">Period to calculate the index.</param>
        public int iADXWilder(string symbol, ENUM_TIMEFRAMES period, int adxPeriod)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "AdxPeriod", adxPeriod } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iADXWilder, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "JawPeriod", jawPeriod }, { "JawShift", jawShift }, { "TeethPeriod", teethPeriod },
                { "TeethShift", teethShift }, { "LipsPeriod", lipsPeriod }, { "LipsShift", lipsShift },
                { "MaMethod", maMethod}, { "AppliedPrice", appliedPrice } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iAlligator, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "AmaPeriod", amaPeriod }, { "FastMaPeriod", fastMaPeriod }, { "SlowMaPeriod", slowMaPeriod },
                { "AmaShift", amaShift }, { "AppliedPrice", appliedPrice} };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iAMA, cmdParams);
        }

        ///<summary>
        ///The function returns the handle of the Awesome Oscillator indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        public int iAO(string symbol, ENUM_TIMEFRAMES period)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iAO, cmdParams);
        }

        ///<summary>
        ///The function returns the handle of the Average True Range indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="maPeriod">The value of the averaging period for the indicator calculation.</param>
        public int iATR(string symbol, ENUM_TIMEFRAMES period, int maPeriod)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "MaPeriod", maPeriod } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iATR, cmdParams);
        }

        ///<summary>
        ///The function returns the handle of the Bears Power indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="maPeriod">The value of the averaging period for the indicator calculation.</param>
        public int iBearsPower(string symbol, ENUM_TIMEFRAMES period, int maPeriod)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "MaPeriod", maPeriod } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iBearsPower, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "BandsPeriod", bandsPeriod }, { "BandsShift", bandsShift }, { "Deviation", deviation},
                { "AppliedPrice", (int)appliedPrice } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iBands, cmdParams);
        }

        ///<summary>
        ///The function returns the handle of the Bulls Power indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="maPeriod">The averaging period for the indicator calculation.</param>
        public int iBullsPower(string symbol, ENUM_TIMEFRAMES period, int maPeriod)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "MaPeriod", maPeriod } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iBullsPower, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "MaPeriod", maPeriod }, { "AppliedPrice", appliedPrice } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iCCI, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "FastMaPeriod", fastMaPeriod }, { "SlowMaPeriod", slowMaPeriod},
                { "MaMethod", (int)maMethod }, { "appliedVolume", (int)appliedVolume } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iChaikin, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "MaPeriod", maPeriod }, { "MaShift", maShift}, { "AppliedPrice", (int)appliedPrice } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iDEMA, cmdParams);
        }

        ///<summary>
        ///The function returns the handle of the DeMarker indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="maPeriod">Averaging period (bars count) for calculations.</param>
        public int iDeMarker(string symbol, ENUM_TIMEFRAMES period, int maPeriod)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty },
                { "Period", (int)period }, { "MaPeriod", maPeriod } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iDeMarker, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "MaPeriod", maPeriod },  { "MaShift", maShift }, { "MaMethod", (int)maMethod },
                { "AppliedPrice", (int)appliedPrice }, { "Deviation", deviation } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iEnvelopes, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "MaPeriod", maPeriod }, { "MaMethod", (int)maMethod }, { "AppliedVolume", (int)appliedVolume } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iForce, cmdParams);
        }

        ///<summary>
        ///The function returns the handle of the Force Index indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        public int iForce(string symbol, ENUM_TIMEFRAMES period)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iForce, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "MaPeriod", maPeriod }, { "MaShift", maShift }, { "AppliedPrice", (int)appliedPrice } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iFrAMA, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "JawPeriod", jawPeriod }, { "JawShift", jawShift }, { "TeethPeriod", teethPeriod },
                { "TeethShift", teethShift }, { "LipsPeriod", lipsPeriod }, { "LipsShift", lipsShift },
                { "MaMethod", maMethod }, { "AppliedPrice", (int)appliedPrice } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iGator, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "TenkanSen", tenkanSen }, { "KijunSen", kijunSen }, { "SenkouSpanB", senkouSpanB } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iIchimoku, cmdParams);
        }

        ///<summary>
        ///The function returns the handle of the Market Facilitation Index indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="appliedVolume">The volume used. Can be one of the constants of ENUM_APPLIED_VOLUME.</param>
        public int iBWMFI(string symbol, ENUM_TIMEFRAMES period, ENUM_APPLIED_VOLUME appliedVolume)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty },
                { "Period", (int)period }, { "AppliedVolume", (int)appliedVolume } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iBWMFI, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "MomPeriod", momPeriod }, { "AppliedPrice", (int)appliedPrice } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iMomentum, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "MaPeriod", maPeriod }, { "AppliedVolume", (int)appliedVolume } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iMFI, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "MaPeriod", maPeriod }, { "MaShift", maShift }, { "MaMethod", (int)maMethod }, { "AppliedPrice", (int)appliedPrice} };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iMA, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "FastEmaPeriod", fastEmaPeriod }, { "SlowEmaPeriod", slowEmaPeriod }, { "SignalPeriod", signalPeriod },
                { "AppliedPrice", (int)appliedPrice} };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iOsMA, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "FastEmaPeriod", fastEmaPeriod }, { "SlowEmaPeriod", slowEmaPeriod }, { "SignalPeriod", signalPeriod },
                { "AppliedPrice", (int)appliedPrice} };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iMACD, cmdParams);
        }

        ///<summary>
        ///The function returns the handle of the On Balance Volume indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="appliedVolume">The volume used. Can be any of the ENUM_APPLIED_VOLUME values.</param>
        public int iOBV(string symbol, ENUM_TIMEFRAMES period, ENUM_APPLIED_VOLUME appliedVolume)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty },
                { "Period", (int)period }, { "AppliedVolume", (int)appliedVolume} };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iOBV, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty },
                { "Period", (int)period }, { "Step", (int)step}, { "Maximum", maximum } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iSAR, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "MaPeriod", maPeriod }, { "AppliedPrice", (int)appliedPrice } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iRSI, cmdParams);
        }

        ///<summary>
        ///The function returns the handle of the Relative Vigor Index indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="maPeriod">Averaging period for the RVI calculation.</param>
        public int iRVI(string symbol, ENUM_TIMEFRAMES period, int maPeriod)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty },
                { "Period", (int)period }, { "MaPeriod", maPeriod } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iRVI, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "MaPeriod", maPeriod }, { "MaShift", maShift }, { "MaMethod", (int)maMethod },
                { "AppliedPrice", (int)appliedPrice } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iStdDev, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "Kperiod", Kperiod }, { "Dperiod", Dperiod }, { "Slowing", slowing },
                { "MaMethod", (int)maMethod }, { "priceField", (int)priceField } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iStochastic, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol }, { "Period", (int)period },
                { "MaPeriod", maPeriod }, { "MaShift", maShift }, { "AppliedPrice", (int)appliedPrice } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iTEMA, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "MaPeriod", maPeriod }, { "AppliedPrice", (int)appliedPrice } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iTriX, cmdParams);
        }

        ///<summary>
        ///The function returns the handle of the Larry Williams' Percent Range indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="calcPeriod">Period (bars count) for the indicator calculation.</param>
        public int iWPR(string symbol, ENUM_TIMEFRAMES period, int calcPeriod)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty },
                { "Period", (int)period }, { "CalcPeriod", calcPeriod } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iWPR, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty }, { "Period", (int)period },
                { "CmoPeriod", cmoPeriod }, { "EmaPeriod", emaPeriod }, { "MaShift", maShift },
                { "AppliedPrice", (int)appliedPrice } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iVIDyA, cmdParams);
        }

        ///<summary>
        ///The function returns the handle of the Volumes indicator.
        ///</summary>
        ///<param name="symbol">The symbol name of the security, the data of which should be used to calculate the indicator.</param>
        ///<param name="period">The value of the period can be one of the ENUM_TIMEFRAMES enumeration values, 0 means the current timeframe.</param>
        ///<param name="appliedVolume">The volume used. Can be any of the ENUM_APPLIED_VOLUME values.</param>
        public int iVolumes(string symbol, ENUM_TIMEFRAMES period, ENUM_APPLIED_VOLUME appliedVolume)
        {
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty },
                { "Period", (int)period }, { "AppliedVolume", (int)appliedVolume } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iVolumes, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty },
                { "Timeframe", (int)period }, { "Name", name ?? string.Empty}, 
                { "Params", parameters }, { "ParamsType", ParametersType.Double} };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iCustom, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty },
                { "Timeframe", (int)period }, { "Name", name  ?? string.Empty }, { "Parameters", parameters },
                { "Params", ParametersType.Int } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iCustom, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty },
                { "Timeframe", (int)period }, { "Name", name ?? string.Empty },
                { "Params", parameters }, { "ParamsType", ParametersType.String } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iCustom, cmdParams);
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
            Dictionary<string, object> cmdParams = new() { { "Symbol", symbol ?? string.Empty },
                { "Timeframe", (int)period }, { "Name", name ?? string.Empty },
                { "Params", parameters }, { "ParamsType", ParametersType.Boolean } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.iCustom, cmdParams);
        }

        #endregion //Technical Indicators

        #region Date and Time

        ///<summary>
        ///Returns the last known server time, time of the last quote receipt for one of the symbols selected in the "Market Watch" window.
        ///</summary>
        public DateTime TimeCurrent()
        {
            var response = SendCommand<long>(ExecutorHandle, Mt5CommandType.TimeCurrent);
            return Mt5TimeConverter.ConvertFromMtTime(response);
        }

        ///<summary>
        ///Returns the calculated current time of the trade server. Unlike TimeCurrent(), the calculation of the time value is performed in the client terminal and depends on the time settings on your computer.
        ///</summary>
        public DateTime TimeTradeServer()
        {
            var response = SendCommand<long>(ExecutorHandle, Mt5CommandType.TimeTradeServer);
            return Mt5TimeConverter.ConvertFromMtTime(response);
        }

        ///<summary>
        ///Returns the local time of a computer, where the client terminal is running.
        ///</summary>
        public DateTime TimeLocal()
        {
            var response = SendCommand<long>(ExecutorHandle, Mt5CommandType.TimeLocal);
            return Mt5TimeConverter.ConvertFromMtTime(response);
        }

        ///<summary>
        ///Returns the GMT, which is calculated taking into account the DST switch by the local time on the computer where the client terminal is running.
        ///</summary>
        public DateTime TimeGMT()
        {
            var response = SendCommand<long>(ExecutorHandle, Mt5CommandType.TimeGMT);
            return Mt5TimeConverter.ConvertFromMtTime(response);
        }

        #endregion //Date and Time

        #region Checkup

        ///<summary>
        ///Returns the value of the last error that occurred during the execution of an mql5 program.
        ///</summary>
        public int GetLastError()
        {
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.GetLastError);
        }

        ///<summary>
        ///Sets the value of the predefined variable _LastError into zero.
        ///</summary>
        public void ResetLastError()
        {
            SendCommand<object>(ExecutorHandle, Mt5CommandType.ResetLastError);
        }

        #endregion

        #region Global Variables

        ///<summary>
        ///Checks the existence of a global variable with the specified name.
        ///</summary>
        ///<param name="name">Global variable name.</param>
        public bool GlobalVariableCheck(string name)
        {
            Dictionary<string, string> cmdParams = new() { { "Name", name ?? string.Empty } }; 
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.GlobalVariableCheck, cmdParams);
        }

        ///<summary>
        ///Returns the time when the global variable was last accessed.
        ///</summary>
        ///<param name="name">Name of the global variable.</param>
        public DateTime GlobalVariableTime(string name)
        {
            Dictionary<string, string> cmdParams = new() { { "Name", name ?? string.Empty } };
            var res = SendCommand<int>(ExecutorHandle, Mt5CommandType.GlobalVariableTime, cmdParams);
            return Mt5TimeConverter.ConvertFromMtTime(res);
        }

        ///<summary>
        ///Deletes a global variable from the client terminal.
        ///</summary>
        ///<param name="name">Name of the global variable.</param>
        public bool GlobalVariableDel(string name)
        {
            Dictionary<string, string> cmdParams = new() { { "Name", name ?? string.Empty } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.GlobalVariableDel, cmdParams);
        }

        ///<summary>
        ///Returns the value of an existing global variable of the client terminal.
        ///</summary>
        ///<param name="name">Global variable name.</param>
        public double GlobalVariableGet(string name)
        {
            Dictionary<string, string> cmdParams = new() { { "Name", name ?? string.Empty } };
            return SendCommand<double>(ExecutorHandle, Mt5CommandType.GlobalVariableGet, cmdParams);
        }

        ///<summary>
        ///Returns the name of a global variable by its ordinal number.
        ///</summary>
        ///<param name="index">Sequence number in the list of global variables. It should be greater than or equal to 0 and less than GlobalVariablesTotal().</param>
        public string? GlobalVariableName(int index)
        {
            Dictionary<string, int> cmdParams = new() { { "Index", index } };
            return SendCommand<string>(ExecutorHandle, Mt5CommandType.GlobalVariableName, cmdParams);
        }

        ///<summary>
        ///Sets a new value for a global variable. If the variable does not exist, the system creates a new global variable.
        ///</summary>
        ///<param name="name">Global variable name.</param>
        ///<param name="value">The new numerical value.</param>
        public DateTime GlobalVariableSet(string name, double value)
        {
            Dictionary<string, object> cmdParams = new() { { "Name", name ?? string.Empty }, { "Value", value } };
            var res = SendCommand<int>(ExecutorHandle, Mt5CommandType.GlobalVariableSet, cmdParams);
            return Mt5TimeConverter.ConvertFromMtTime(res);
        }

        ///<summary>
        ///Forcibly saves contents of all global variables to a disk.
        ///</summary>
        public void GlobalVariablesFlush()
        {
            SendCommand<object>(ExecutorHandle, Mt5CommandType.GlobalVariablesFlush);
        }

        ///<summary>
        ///The function attempts to create a temporary global variable. If the variable doesn't exist, the system creates a new temporary global variable.
        ///</summary>
        ///<param name="name">The name of a temporary global variable.</param>
        public bool GlobalVariableTemp(string name)
        {
            Dictionary<string, string> cmdParams = new() { { "Name", name ?? string.Empty } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.GlobalVariableTemp, cmdParams);
        }

        ///<summary>
        ///Sets the new value of the existing global variable if the current value equals to the third parameter check_value. If there is no global variable, the function will generate an error ERR_GLOBALVARIABLE_NOT_FOUND (4501) and return false.
        ///</summary>
        ///<param name="name">The name of a global variable.</param>
        ///<param name="value">New value.</param>
        ///<param name="checkValue">The value to check the current value of the global variable.</param>
        public bool GlobalVariableSetOnCondition(string name, double value, double checkValue)
        {
            Dictionary<string, object> cmdParams = new() { { "Name", name ?? string.Empty },
                { "Value", value }, { "CheckValue", checkValue } };
            return SendCommand<bool>(ExecutorHandle, Mt5CommandType.GlobalVariableSetOnCondition, cmdParams);
        }

        ///<summary>
        ///Deletes global variables of the client terminal.
        ///</summary>
        ///<param name="prefixName">Name prefix global variables to remove. If you specify a prefix NULL or empty string, then all variables that meet the data criterion will be deleted.</param>
        ///<param name="limitData">Date to select global variables by the time of their last modification. The function removes global variables, which were changed before this date. If the parameter is zero, then all variables that meet the first criterion (prefix) are deleted.</param>
        public int GlobalVariablesDeleteAll(string prefixName = "", DateTime? limitData = null)
        {
            Dictionary<string, object> cmdParams = new() { { "PrefixName", prefixName ?? string.Empty },
                { "LimitData", Mt5TimeConverter.ConvertToMtTime(limitData) } };
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.GlobalVariablesDeleteAll, cmdParams);
        }

        ///<summary>
        ///Returns the total number of global variables of the client terminal.
        ///</summary>
        public int GlobalVariablesTotal()
        {
            return SendCommand<int>(ExecutorHandle, Mt5CommandType.GlobalVariablesTotal);
        }
        #endregion

        #region Backtesting functions

        ///<summary>
        ///The function unlock ticks in backtesting mode.
        ///</summary>
        public void UnlockTicks()
        {
            SendCommand<object>(ExecutorHandle, Mt5CommandType.UnlockTicks);
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
        public event QuoteHandler? QuoteUpdated;
        public event EventHandler<Mt5QuoteEventArgs>? QuoteUpdate;
        public event EventHandler<Mt5QuoteEventArgs>? QuoteAdded;
        public event EventHandler<Mt5QuoteEventArgs>? QuoteRemoved;
        public event EventHandler<Mt5ConnectionEventArgs>? ConnectionStateChanged;
        public event EventHandler<Mt5TradeTransactionEventArgs>? OnTradeTransaction;
        public event EventHandler<Mt5BookEventArgs>? OnBookEvent;
        public event EventHandler<Mt5TimeBarArgs>? OnLastTimeBar;
        public event EventHandler<Mt5LockTicksEventArgs>? OnLockTicks;
        public event EventHandler<Mt5QuotesEventArgs>? QuoteList;
        #endregion

        #region Private Methods
        private MtRpcClient? Client
        {
            get
            {
                lock (_locker)
                {
                    return _client;
                }
            }
        }

        private void Client_MtEventReceived(object? sender, MtEventArgs e)
        {
            Task.Run(() => _mtEventHandlers[(Mt5EventTypes)e.EventType](e.ExpertHandle, e.Payload));
        }

        private void Client_ExpertAdded(object? sender, MtExpertEventArgs e)
        {
            Task.Run(() => ProcessExpertAdded(e.Expert));
        }

        private void Client_ExpertRemoved(object? sender, MtExpertEventArgs e)
        {
            Task.Run(() => ProcessExpertRemoved(e.Expert));
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
                var quote = GetQuote(Client, handle);
                if (quote != null)
                {
                    lock (_locker)
                    {
                        _quotes[handle] = quote;
                    }

                    QuoteAdded?.Invoke(this, new Mt5QuoteEventArgs(quote));
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

            Mt5Quote? quote = null;
            lock (_locker)
            {
                _experts.Remove(handle);
                if (_quotes.TryGetValue(handle, out quote))
                    _quotes.Remove(handle);
                if (_executorHandle == handle)
                    _executorHandle = (_experts.Count > 0) ? _experts.ElementAt(0) : 0;

            }

            if (quote != null)
                QuoteRemoved?.Invoke(this, new Mt5QuoteEventArgs(quote));
        }

        private Mt5Quote? GetQuote(MtRpcClient? client, int expertHandle)
        {
            Log.Debug($"GetQuote: expertHandle = {expertHandle}");

            var q = SendCommand<MtQuote>(client, expertHandle, Mt5CommandType.GetQuote);
            if (q == null || string.IsNullOrEmpty(q.Instrument) || q.Tick == null)
                return null;

            Mt5Quote quote = new()
            {
                Instrument = q.Instrument,
                Bid = q.Tick.Bid,
                Ask = q.Tick.Ask,
                ExpertHandle = expertHandle,
                Volume = q.Tick.Volume,
                Time = Mt5TimeConverter.ConvertFromMtTime(q.Tick.Time),
                Last = q.Tick.Last
            };

            return quote;
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

        private void ReceivedOnTradeTransactionEvent(int expertHandle, string payload)
        {
            var e = JsonConvert.DeserializeObject<OnTradeTransactionEvent>(payload);
            if (e == null)
                return;
            OnTradeTransaction?.Invoke(this, new Mt5TradeTransactionEventArgs
            {
                ExpertHandle = expertHandle,
                Trans = e.Trans,
                Request = e.Request,
                Result = e.Result
            });
        }

        private void ReceivedOnBookEvent(int expertHandle, string payload)
        {
            var e = JsonConvert.DeserializeObject<OnBookEvent>(payload);
            if (e == null || string.IsNullOrEmpty(e.Symbol))
                return;
            OnBookEvent?.Invoke(this, new Mt5BookEventArgs
            {
                ExpertHandle = expertHandle,
                Symbol = e.Symbol
            });
        }

        private void ReceivedOnTickEvent(int expertHandle, string payload)
        {
            var e = JsonConvert.DeserializeObject<MtQuote>(payload);
            if (e == null || string.IsNullOrEmpty(e.Instrument) || e.Tick == null)
                return;

            QuoteUpdated?.Invoke(this, e.Instrument, e.Tick.Bid, e.Tick.Ask);

            Mt5Quote quote = new()
            {
                Instrument = e.Instrument,
                Bid = e.Tick.Bid,
                Ask = e.Tick.Ask,
                ExpertHandle = expertHandle,
                Volume = e.Tick.Volume,
                Time = Mt5TimeConverter.ConvertFromMtTime(e.Tick.Time),
                Last = e.Tick.Last
            };
            QuoteUpdate?.Invoke(this, new Mt5QuoteEventArgs(quote));
        }

        private void ReceivedOnLastTimeBarEvent(int expertHandle, string payload)
        {
            var e = JsonConvert.DeserializeObject<OnLastTimeBarEvent>(payload);
            if (e == null || string.IsNullOrEmpty(e.Instrument) || e.Rates == null)
                return;
            OnLastTimeBar?.Invoke(this, new Mt5TimeBarArgs(expertHandle, e.Instrument, e.Rates));
        }

        private void ReceivedOnLockTicksEvent(int expertHandle, string payload)
        {
            var e = JsonConvert.DeserializeObject<OnLockTicksEvent>(payload);
            if (e == null || string.IsNullOrEmpty(e.Instrument))
                return;
            OnLockTicks?.Invoke(this, new Mt5LockTicksEventArgs(e.Instrument));
        }

        private void Disconnect(bool failed)
        {
            var state = failed ? Mt5ConnectionState.Failed : Mt5ConnectionState.Disconnected;
            var message = failed ? "Connection Failed" : "Disconnected";

            MtRpcClient? client;

            lock (_locker)
            {
                if (_connectionState == Mt5ConnectionState.Disconnected
                    || _connectionState == Mt5ConnectionState.Failed)
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

            ConnectionStateChanged?.Invoke(this, new Mt5ConnectionEventArgs(state, message));
        }

        private T? SendCommand<T>(int expertHandle, Mt5CommandType commandType, object? payload = null)
        {
            return SendCommand<T>(Client, expertHandle, commandType, payload);
        }

        private T? SendCommand<T>(MtRpcClient? client, int expertHandle, Mt5CommandType commandType, object? payload = null)
        {
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
                throw new ExecutionException(ErrorCode.ErrCustom, "Response from MetaTrader is null");
            }

            var response = JsonConvert.DeserializeObject<Response<T>>(responseJson);
            if (response == null)
            {
                Log.Warn("SendCommand: Failed to deserialize response from JSON");
                throw new ExecutionException(ErrorCode.ErrCustom, "Response from MetaTrader is null");
            }

            if (response.ErrorCode != 0)
            {
                Log.Warn($"SendCommand: ErrorCode = {response.ErrorCode}. {response.ErrorMessage}");
                throw new ExecutionException((ErrorCode)response.ErrorCode, response.ErrorMessage);
            }

            return (response.Value == null) ? default : response.Value;
        }

        private void BacktestingReady()
        {
            SendCommand<object>(ExecutorHandle, Mt5CommandType.BacktestingReady);
        }
        #endregion
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
