using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using MtApi5;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MtApi5TestClient
{
    public class ViewModel : INotifyPropertyChanged
    {
        #region Commands
        public DelegateCommand ConnectCommand { get; private set; }
        public DelegateCommand DisconnectCommand { get; private set; }

        public DelegateCommand OrderSendCommand { get; private set; }

        public DelegateCommand AccountInfoDoubleCommand { get; private set; }
        public DelegateCommand AccountInfoIntegerCommand { get; private set; }
        public DelegateCommand AccountInfoStringCommand { get; private set; }

        public DelegateCommand CopyRatesCommand { get; private set; }
        public DelegateCommand CopyTimesCommand { get; private set; }
        public DelegateCommand CopyOpenCommand { get; private set; }
        public DelegateCommand CopyHighCommand { get; private set; }
        public DelegateCommand CopyLowCommand { get; private set; }
        public DelegateCommand CopyCloseCommand { get; private set; }

        public DelegateCommand CopyTickVolumeCommand { get; private set; }
        public DelegateCommand CopyRealVolumeCommand { get; private set; }
        public DelegateCommand CopySpreadCommand { get; private set; }

        public DelegateCommand SymbolsTotalCommand { get; private set; }
        public DelegateCommand SymbolNameCommand { get; private set; }
        public DelegateCommand SymbolSelectCommand { get; private set; }
        public DelegateCommand SymbolIsSynchronizedCommand { get; private set; }
        public DelegateCommand SymbolInfoDoubleCommand { get; private set; }
        public DelegateCommand SymbolInfoIntegerCommand { get; private set; }
        public DelegateCommand SymbolInfoStringCommand { get; private set; }
        public DelegateCommand SymbolInfoTickCommand { get; private set; }
        public DelegateCommand SymbolInfoSessionQuoteCommand { get; private set; }
        public DelegateCommand SymbolInfoSessionTradeCommand { get; private set; }
        public DelegateCommand MarketBookAddCommand { get; private set; }
        public DelegateCommand MarketBookReleaseCommand { get; private set; }
        public DelegateCommand MarketBookGetCommand { get; private set; }

        public DelegateCommand PositionOpenCommand { get; private set; }
        #endregion

        #region Properties
        private Mt5ConnectionState _connectionState;
        public Mt5ConnectionState ConnectionState
        {
            get { return _connectionState; }
            set
            {
                _connectionState = value;
                OnPropertyChanged("ConnectionState");
            }
        }

        private string _connectionMessage;
        public string ConnectionMessage
        {
            get { return _connectionMessage; }
            set
            {
                _connectionMessage = value;
                OnPropertyChanged("ConnectionMessage");
            }
        }

        private string _host;
        public string Host
        {
            get { return _host; }
            set
            {
                _host = value;
                OnPropertyChanged("Host");
            }
        }

        private int _port;
        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                OnPropertyChanged("Port");
            }
        }

        public ObservableCollection<QuoteViewModel> Quotes { get; } = new ObservableCollection<QuoteViewModel>();

        private QuoteViewModel _selectedQuote;
        public QuoteViewModel SelectedQuote 
        {
            get { return _selectedQuote; }
            set
            {
                _selectedQuote = value;
                OnPropertyChanged("SelectedQuote");
                OnSelectedQuoteChanged();
            }
        }

        public ObservableCollection<string> History { get; } = new ObservableCollection<string>();

        public ObservableCollection<MqlTradeRequestViewModel> TradeRequests { get; } = new ObservableCollection<MqlTradeRequestViewModel>();

        private MqlTradeRequestViewModel _tradeRequest;
        public MqlTradeRequestViewModel TradeRequest
        {
            get { return _tradeRequest; }
            set
            {
                _tradeRequest = value;
                OnPropertyChanged("TradeRequest");
            }
        }

        public ENUM_ACCOUNT_INFO_DOUBLE AccountInfoDoublePropertyId { get; set; }
        public ENUM_ACCOUNT_INFO_INTEGER AccountInfoIntegerPropertyId { get; set; }
        public ENUM_ACCOUNT_INFO_STRING AccountInfoStringPropertyId { get; set; }

        public TimeSeriesValueViewModel TimeSeriesValues { get; set; }

        public ObservableCollection<string> TimeSeriesResults { get; } = new ObservableCollection<string>();

        #endregion

        #region Public Methods
        public ViewModel()
        {
            // Init MtApi client
            _mtApiClient = new MtApi5Client();

            _mtApiClient.ConnectionStateChanged += mMtApiClient_ConnectionStateChanged;
            _mtApiClient.QuoteAdded += mMtApiClient_QuoteAdded;
            _mtApiClient.QuoteRemoved += mMtApiClient_QuoteRemoved;
            _mtApiClient.QuoteUpdated += mMtApiClient_QuoteUpdated;

            _quotesMap = new Dictionary<string, QuoteViewModel>();

            ConnectionState = _mtApiClient.ConnectionState;
            ConnectionMessage = "Disconnected";
            Port = 8228; //default local port

            InitCommands();

            var request = new MqlTradeRequest { Action = ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_DEAL
                , Type = ENUM_ORDER_TYPE.ORDER_TYPE_BUY
                , Volume = 0.1
                , Comment = "Test Trade Request"
            };

            TradeRequest = new MqlTradeRequestViewModel(request);

            TimeSeriesValues = new TimeSeriesValueViewModel { Count = 100 };
        }

        public void Close()
        {
            _mtApiClient.BeginDisconnect();
        }

        #endregion

        #region Private Methods


        private void InitCommands()
        {
            ConnectCommand = new DelegateCommand(ExecuteConnect, CanExecuteConnect);
            DisconnectCommand = new DelegateCommand(ExecuteDisconnect, CanExecuteDisconnect);

            OrderSendCommand = new DelegateCommand(ExecuteOrderSend);

            AccountInfoDoubleCommand = new DelegateCommand(ExecuteAccountInfoDouble);
            AccountInfoIntegerCommand = new DelegateCommand(ExecuteAccountInfoInteger);
            AccountInfoStringCommand = new DelegateCommand(ExecuteAccountInfoString);

            CopyRatesCommand = new DelegateCommand(ExecuteCopyRates);
            CopyTimesCommand = new DelegateCommand(ExecuteCopyTime);
            CopyOpenCommand = new DelegateCommand(ExecuteCopyOpen);
            CopyHighCommand = new DelegateCommand(ExecuteCopyHigh);
            CopyLowCommand = new DelegateCommand(ExecuteCopyLow);
            CopyCloseCommand = new DelegateCommand(ExecuteCopyClose);

            CopyTickVolumeCommand = new DelegateCommand(ExecuteCopyTickVolume);
            CopyRealVolumeCommand = new DelegateCommand(ExecuteCopyRealVolume);
            CopySpreadCommand = new DelegateCommand(ExecuteCopySpread);
            
            SymbolsTotalCommand = new DelegateCommand(ExecuteSymbolsTotal);
            SymbolNameCommand = new DelegateCommand(ExecuteSymbolName);
            SymbolSelectCommand = new DelegateCommand(ExecuteSymbolSelect);
            SymbolIsSynchronizedCommand = new DelegateCommand(ExecuteSymbolIsSynchronized);
            SymbolInfoDoubleCommand = new DelegateCommand(ExecuteSymbolInfoDouble);
            SymbolInfoIntegerCommand = new DelegateCommand(ExecuteSymbolInfoInteger);
            SymbolInfoStringCommand = new DelegateCommand(ExecuteSymbolInfoString);
            SymbolInfoTickCommand = new DelegateCommand(ExecuteSymbolInfoTick);
            SymbolInfoSessionQuoteCommand = new DelegateCommand(ExecuteSymbolInfoSessionQuote);
            SymbolInfoSessionTradeCommand = new DelegateCommand(ExecuteSymbolInfoSessionTrade);
            MarketBookAddCommand = new DelegateCommand(ExecuteMarketBookAdd);   
            MarketBookReleaseCommand = new DelegateCommand(ExecuteMarketBookRelease);
            MarketBookGetCommand = new DelegateCommand(ExecuteMarketBookGet);

            PositionOpenCommand = new DelegateCommand(ExecutePositionOpen);
        }

        private bool CanExecuteConnect(object o)
        {
            return ConnectionState == Mt5ConnectionState.Disconnected || ConnectionState == Mt5ConnectionState.Failed;
        }

        private void ExecuteConnect(object o)
        {
            if (string.IsNullOrEmpty(Host))
            {
                _mtApiClient.BeginConnect(Port);
            }
            else
            {
                _mtApiClient.BeginConnect(Host, Port);
            }            
        }

        private bool CanExecuteDisconnect(object o)
        {
            return ConnectionState == Mt5ConnectionState.Connected;
        }

        private void ExecuteDisconnect(object o)
        {
            _mtApiClient.BeginDisconnect();
        }

        private void ExecuteOrderSend(object o)
        {
            var request = TradeRequest.GetMqlTradeRequest();

            MqlTradeResult result;
            var retVal = _mtApiClient.OrderSend(request, out result);

            string historyItem;

            if (retVal)
            {
                historyItem = "OrderSend successed. " + MqlTradeResultToString(result);
            }
            else
            {
                historyItem = "OrderSend failed. " + MqlTradeResultToString(result);
            }

            History.Add(historyItem);
        }

        private void ExecuteAccountInfoDouble(object o)
        {
            var result = _mtApiClient.AccountInfoDouble(AccountInfoDoublePropertyId);

            var historyItem = $"AccountInfoDouble: property_id = {AccountInfoDoublePropertyId}; result = {result}";
            History.Add(historyItem);
        }

        private void ExecuteAccountInfoInteger(object o)
        {
            var result = _mtApiClient.AccountInfoInteger(AccountInfoIntegerPropertyId);

            var historyItem = $"AccountInfoInteger: property_id = {AccountInfoDoublePropertyId}; result = {result}";
            History.Add(historyItem);
        }

        private void ExecuteAccountInfoString(object o)
        {
            var result = _mtApiClient.AccountInfoString(AccountInfoStringPropertyId);

            var historyItem = $"AccountInfoString: property_id = {AccountInfoDoublePropertyId}; result = {result}";
            History.Add(historyItem);
        }

        private void ExecuteCopyTime(object o)
        {
            if (TimeSeriesValues != null && string.IsNullOrEmpty(TimeSeriesValues.SymbolValue) == false)
            {
                TimeSeriesResults.Clear();

                DateTime[] timesArray;
                var count = _mtApiClient.CopyTime(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out timesArray);
                if (count > 0)
                {
                    foreach (var time in timesArray)
                    {
                        TimeSeriesResults.Add(time.ToString(CultureInfo.CurrentCulture));
                    }

                    History.Add("CopyTime success");
                }
            }
        }

        private void ExecuteCopyOpen(object o)
        {
            if (TimeSeriesValues != null && string.IsNullOrEmpty(TimeSeriesValues.SymbolValue) == false)
            {
                TimeSeriesResults.Clear();

                double[] opensArray;
                var count = _mtApiClient.CopyOpen(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out opensArray);
                if (count > 0)
                {
                    foreach (var open in opensArray)
                    {
                        TimeSeriesResults.Add(open.ToString(CultureInfo.CurrentCulture));
                    }

                    History.Add("CopyOpen success");
                }
            }
        }

        private void ExecuteCopyHigh(object o)
        {
            if (TimeSeriesValues != null && string.IsNullOrEmpty(TimeSeriesValues.SymbolValue) == false)
            {
                TimeSeriesResults.Clear();

                double[] array;
                var count = _mtApiClient.CopyHigh(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out array);
                if (count > 0)
                {
                    foreach (var value in array)
                    {
                        TimeSeriesResults.Add(value.ToString(CultureInfo.CurrentCulture));
                    }

                    History.Add("CopyHigh success");
                }
            }
        }

        private void ExecuteCopyLow(object o)
        {
            if (TimeSeriesValues != null && string.IsNullOrEmpty(TimeSeriesValues.SymbolValue) == false)
            {
                TimeSeriesResults.Clear();

                double[] array;
                var count = _mtApiClient.CopyLow(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out array);
                if (count > 0)
                {
                    foreach (var value in array)
                    {
                        TimeSeriesResults.Add(value.ToString(CultureInfo.CurrentCulture));
                    }

                    History.Add("CopyLow success");
                }
            }
        }

        private void ExecuteCopyClose(object o)
        {
            if (TimeSeriesValues != null && string.IsNullOrEmpty(TimeSeriesValues.SymbolValue) == false)
            {
                TimeSeriesResults.Clear();

                double[] array;
                var count = _mtApiClient.CopyClose(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out array);
                if (count > 0)
                {
                    foreach (var value in array)
                    {
                        TimeSeriesResults.Add(value.ToString(CultureInfo.CurrentCulture));
                    }

                    History.Add("CopyClose success");
                }
            }
        }

        private void ExecuteCopyRates(object o)
        {
            if (TimeSeriesValues != null && string.IsNullOrEmpty(TimeSeriesValues.SymbolValue) == false)
            {
                TimeSeriesResults.Clear();

                MqlRates[] ratesArray;
                var count = _mtApiClient.CopyRates(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out ratesArray);
                if (count > 0)
                {
                    foreach (var rates in ratesArray)
                    {
                        TimeSeriesResults.Add(
                            $"time={rates.time}; open={rates.open}; high={rates.high}; low={rates.low}; close={rates.close}; tick_volume={rates.tick_volume}; spread={rates.spread}; real_volume={rates.tick_volume}");
                    }

                    History.Add("CopyRates success");
                }
            }
        }

        private void ExecuteCopyTickVolume(object o)
        {
            if (TimeSeriesValues != null && string.IsNullOrEmpty(TimeSeriesValues.SymbolValue) == false)
            {
                TimeSeriesResults.Clear();

                long[] array;
                var count = _mtApiClient.CopyTickVolume(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out array);
                if (count > 0)
                {
                    foreach (var value in array)
                    {
                        TimeSeriesResults.Add(value.ToString());
                    }

                    History.Add("CopyTickVolume success");
                }
            }
        }

        private void ExecuteCopyRealVolume(object o)
        {
            if (TimeSeriesValues != null && string.IsNullOrEmpty(TimeSeriesValues.SymbolValue) == false)
            {
                TimeSeriesResults.Clear();

                long[] array;
                var count = _mtApiClient.CopyRealVolume(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out array);
                if (count > 0)
                {
                    foreach (var value in array)
                    {
                        TimeSeriesResults.Add(value.ToString());
                    }

                    History.Add("CopyRealVolume success");
                }
            }
        }

        private void ExecuteCopySpread(object o)
        {
            if (TimeSeriesValues != null && string.IsNullOrEmpty(TimeSeriesValues.SymbolValue) == false)
            {
                TimeSeriesResults.Clear();

                int[] array;
                var count = _mtApiClient.CopySpread(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out array);
                if (count > 0)
                {
                    foreach (var value in array)
                    {
                        TimeSeriesResults.Add(value.ToString());
                    }

                    History.Add("CopySpread success");
                }
            }
        }

        private void ExecuteSymbolsTotal(object o)
        {
            var selectedCount = _mtApiClient.SymbolsTotal(true);
            History.Add($"SymbolsTotal(true) success, result = {selectedCount}");

            var commonCount = _mtApiClient.SymbolsTotal(false);
            History.Add($"SymbolsTotal(false) success, result = {commonCount}");
        }

        private void ExecuteSymbolName(object o)
        {
            var selectedSymbol = _mtApiClient.SymbolName(5, true);
            History.Add("SymbolName(5, true) success, result = " + selectedSymbol);

            var commonSymbol = _mtApiClient.SymbolName(5, false);
            History.Add("SymbolName(5, false) success, result = " + commonSymbol);

        }

        private void ExecuteSymbolSelect(object o)
        {
            var retVal = _mtApiClient.SymbolSelect("AUDJPY", true);
            History.Add("SymbolSelect(AUDJPY, true) success, result = " + retVal);

            //var retVal1 = _mtApiClient.SymbolSelect("AUDJPY", false);
            //History.Add("SymbolSelect(AUDJPY, false) success, result = " + retVal1);
        }

        private void ExecuteSymbolIsSynchronized(object o)
        {
            var retVal = _mtApiClient.SymbolIsSynchronized("EURUSD");
            History.Add("SymbolIsSynchronized(EURUSD) success, result = " + retVal);
        }

        private void ExecuteSymbolInfoDouble(object o)
        {
            var retVal = _mtApiClient.SymbolInfoDouble("EURUSD", ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_BID);
            History.Add("SymbolInfoDouble(EURUSD, ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_BID) success, result = " + retVal);
        }

        private void ExecuteSymbolInfoInteger(object o)
        {
            var retVal = _mtApiClient.SymbolInfoInteger("EURUSD", ENUM_SYMBOL_INFO_INTEGER.SYMBOL_SPREAD);
            History.Add("SymbolInfoInteger(EURUSD, ENUM_SYMBOL_INFO_INTEGER.SYMBOL_SPREAD) success, result = " + retVal);
        }

        private void ExecuteSymbolInfoString(object o)
        {
            var retVal = _mtApiClient.SymbolInfoString("EURUSD", ENUM_SYMBOL_INFO_STRING.SYMBOL_DESCRIPTION);
            History.Add("SymbolInfoString(EURUSD, ENUM_SYMBOL_INFO_STRING.SYMBOL_DESCRIPTION) success, result = " + retVal);
        }

        private void ExecuteSymbolInfoTick(object o)
        {
            MqlTick tick;
            var retVal = _mtApiClient.SymbolInfoTick("EURUSD", out tick);
            History.Add("SymbolInfoTick(EURUSD) success, result = " + retVal);
            History.Add("SymbolInfoTick(EURUSD) tick.time = " + tick.time);
            History.Add("SymbolInfoTick(EURUSD) tick.bid = " + tick.bid);
            History.Add("SymbolInfoTick(EURUSD) tick.ask = " + tick.ask);
            History.Add("SymbolInfoTick(EURUSD) tick.last = " + tick.last);
            History.Add("SymbolInfoTick(EURUSD) tick.volume = " + tick.volume);
        }

        private void ExecuteSymbolInfoSessionQuote(object o)
        {
            DateTime from;
            DateTime to;
            var retVal = _mtApiClient.SymbolInfoSessionQuote("EURUSD", ENUM_DAY_OF_WEEK.MONDAY, 0, out from, out to);
            History.Add("SymbolInfoSessionQuote(EURUSD) success, result = " + retVal);
            History.Add("SymbolInfoSessionQuote(EURUSD) from = " + from);
            History.Add("SymbolInfoSessionQuote(EURUSD) to = " + to);
        }

        private void ExecuteSymbolInfoSessionTrade(object o)
        {
            DateTime from;
            DateTime to;
            var retVal = _mtApiClient.SymbolInfoSessionTrade("EURUSD", ENUM_DAY_OF_WEEK.MONDAY, 0, out from, out to);
            History.Add("SymbolInfoSessionTrade(EURUSD) success, result = " + retVal);
            History.Add("SymbolInfoSessionTrade(EURUSD) from = " + from);
            History.Add("SymbolInfoSessionTrade(EURUSD) to = " + to);
        }

        private void ExecuteMarketBookAdd(object o)
        {
            var retVal = _mtApiClient.MarketBookAdd("CHFJPY");
            History.Add("MarketBookAdd(CHFJPY) success, result = " + retVal);
        }

        private void ExecuteMarketBookRelease(object o)
        {
            var retVal = _mtApiClient.MarketBookRelease("CHFJPY");
            History.Add("MarketBookRelease(CHFJPY) success, result = " + retVal);
        }

        private void ExecuteMarketBookGet(object o)
        {
            MqlBookInfo[] book;
            var retVal = _mtApiClient.MarketBookGet("EURUSD", out book);

            History.Add("MarketBookGet(EURUSD) success, result = " + retVal);

            if (retVal && book != null)
            {
                for (int i = 0; i < book.Length; i++)
                {
                    History.Add($"MarketBookGet: book[{i}].price = {book[i].price}");
                    History.Add($"MarketBookGet: book[{i}].price = {book[i].volume}");
                    History.Add($"MarketBookGet: book[{i}].price = {book[i].type}");
                }
            }
        }

        private void ExecutePositionOpen(object obj)
        {
            const string symbol = "EURUSD";
            const ENUM_ORDER_TYPE orderType = ENUM_ORDER_TYPE.ORDER_TYPE_BUY;
            const double volume = 0.1;
            const double price = 1.013;
            const double sl = 1.00;
            const double tp = 1.020;
            const string comment = "Test PositionOpen";

            var retVal = _mtApiClient.PositionOpen(symbol, orderType, volume, price, sl, tp, comment);
            History.Add("PositionOpen: symbol EURUSD result = " + retVal);
        }

        private static void RunOnUiThread(Action action)
        {
            Application.Current?.Dispatcher.Invoke(action);
        }

        private static void RunOnUiThread<T>(Action<T> action, params object[] args)
        {
            Application.Current?.Dispatcher.Invoke(action, args);
        }

        private void mMtApiClient_QuoteUpdated(object sender, string symbol, double bid, double ask)
        {
            if (string.IsNullOrEmpty(symbol) == false)
            {
                if (_quotesMap.ContainsKey(symbol))
                {
                    var qvm = _quotesMap[symbol];
                    qvm.Bid = bid;
                    qvm.Ask = ask;
                }

                if (string.Equals(symbol, TradeRequest.Symbol))
                {
                    if (TradeRequest.Type == ENUM_ORDER_TYPE.ORDER_TYPE_BUY)
                    {
                        TradeRequest.Price = ask;
                    }
                    else if (TradeRequest.Type == ENUM_ORDER_TYPE.ORDER_TYPE_SELL)
                    {
                        TradeRequest.Price = bid;
                    }
                }
            }
        }

        private void mMtApiClient_QuoteRemoved(object sender, Mt5QuoteEventArgs e)
        {
            RunOnUiThread<Mt5Quote>(RemoveQuote, e.Quote);
        }

        private void mMtApiClient_QuoteAdded(object sender, Mt5QuoteEventArgs e)
        {
            RunOnUiThread<Mt5Quote>(AddQuote, e.Quote);
        }

        private void mMtApiClient_ConnectionStateChanged(object sender, Mt5ConnectionEventArgs e)
        {
            ConnectionState = e.Status;
            ConnectionMessage = e.ConnectionMessage;

            RunOnUiThread(ConnectCommand.RaiseCanExecuteChanged);
            RunOnUiThread(DisconnectCommand.RaiseCanExecuteChanged);

            switch (e.Status)
            {
                case Mt5ConnectionState.Connected:
                    RunOnUiThread(OnConnected);
                    break;
                case Mt5ConnectionState.Disconnected:
                    RunOnUiThread(OnDisconnected);
                    break;
            }
        }

        private void AddQuote(Mt5Quote quote)
        {
            if (quote == null)
                return;

            QuoteViewModel qvm;

            if (_quotesMap.ContainsKey(quote.Instrument) == false)
            {
                qvm = new QuoteViewModel(quote.Instrument);
                _quotesMap[quote.Instrument] = qvm;
                Quotes.Add(qvm);
            }
            else
            {
                qvm = _quotesMap[quote.Instrument];
            }

            qvm.FeedCount++;
            qvm.Bid = quote.Bid;
            qvm.Ask = quote.Ask;
        }

        private void RemoveQuote(Mt5Quote quote)
        {
            if (quote == null)
                return;

            if (_quotesMap.ContainsKey(quote.Instrument))
            {
                var qvm = _quotesMap[quote.Instrument];
                qvm.FeedCount--;

                if (qvm.FeedCount <= 0)
                {
                    _quotesMap.Remove(quote.Instrument);
                    Quotes.Remove(qvm);
                }
            }
        }

        private void OnConnected()
        {
            var quotes = _mtApiClient.GetQuotes();
            if (quotes != null)
            {
                foreach (var quote in quotes)
                {
                    AddQuote(quote);
                }
            }
        }

        private void OnDisconnected()
        {
            _quotesMap.Clear();
            Quotes.Clear();
        }

        private static string MqlTradeResultToString(MqlTradeResult result)
        {
            return result != null ?
                "Retcode = " + result.Retcode + ";"
                + " Comment = " + result.Comment + ";"
                + " Order = " + result.Order + ";"
                + " Volume = " + result.Volume + ";"
                + " Price = " + result.Price + ";"
                + " Deal = " + result.Deal + ";"
                + " Request_id = " + result.Request_id + ";"
                + " Bid = " + result.Bid + ";"
                + " Ask = " + result.Ask + ";" : string.Empty;
        }

        private void OnSelectedQuoteChanged()
        {
            if (SelectedQuote != null)
            {
                TradeRequest.Symbol = SelectedQuote.Instrument;
                TimeSeriesValues.SymbolValue = SelectedQuote.Instrument;
            }            
        }

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Private Fields
        private readonly MtApi5Client _mtApiClient;

        private readonly Dictionary<string, QuoteViewModel> _quotesMap;
        #endregion
    }
}
