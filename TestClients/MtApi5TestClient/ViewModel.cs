using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows;
using MtApi5;
using System.Collections.ObjectModel;

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
        #endregion

        #region Properties
        private Mt5ConnectionState _ConnectionState;
        public Mt5ConnectionState ConnectionState
        {
            get { return _ConnectionState; }
            set
            {
                _ConnectionState = value;
                OnPropertyChanged("ConnectionState");
            }
        }

        private string _ConnectionMessage;
        public string ConnectionMessage
        {
            get { return _ConnectionMessage; }
            set
            {
                _ConnectionMessage = value;
                OnPropertyChanged("ConnectionMessage");
            }
        }

        private string _Host;
        public string Host
        {
            get { return _Host; }
            set
            {
                _Host = value;
                OnPropertyChanged("Host");
            }
        }

        private int _Port;
        public int Port
        {
            get { return _Port; }
            set
            {
                _Port = value;
                OnPropertyChanged("Port");
            }
        }

        private ObservableCollection<QuoteViewModel> _Quotes = new ObservableCollection<QuoteViewModel>();
        public ObservableCollection<QuoteViewModel> Quotes
        {
            get { return _Quotes; }
        }

        private QuoteViewModel _SelectedQuote;
        public QuoteViewModel SelectedQuote 
        {
            get { return _SelectedQuote; }
            set
            {
                _SelectedQuote = value;
                OnPropertyChanged("SelectedQuote");
                OnSelectedQuoteChanged();
            }
        }

        private ObservableCollection<string> _History = new ObservableCollection<string>();
        public ObservableCollection<string> History
        {
            get { return _History; }
        }

        private ObservableCollection<MqlTradeRequestViewModel> _TradeRequests = new ObservableCollection<MqlTradeRequestViewModel>();
        public ObservableCollection<MqlTradeRequestViewModel> TradeRequests
        {
            get { return _TradeRequests; }
        }

        private MqlTradeRequestViewModel _TradeRequest;
        public MqlTradeRequestViewModel TradeRequest
        {
            get { return _TradeRequest; }
            set
            {
                _TradeRequest = value;
                OnPropertyChanged("TradeRequest");
            }
        }

        public ENUM_ACCOUNT_INFO_DOUBLE AccountInfoDoublePropertyId { get; set; }
        public ENUM_ACCOUNT_INFO_INTEGER AccountInfoIntegerPropertyId { get; set; }
        public ENUM_ACCOUNT_INFO_STRING AccountInfoStringPropertyId { get; set; }

        public TimeSeriesValueViewModel TimeSeriesValues { get; set; }

        private ObservableCollection<string> _TimeSeriesResults = new ObservableCollection<string>();
        public ObservableCollection<string> TimeSeriesResults
        {
            get { return _TimeSeriesResults; }
        }
        #endregion

        #region Public Methods
        public ViewModel()
        {
            // Init MtApi client
            mMtApiClient = new MtApi5Client();

            mMtApiClient.ConnectionStateChanged += new EventHandler<Mt5ConnectionEventArgs>(mMtApiClient_ConnectionStateChanged);
            mMtApiClient.QuoteAdded += new EventHandler<Mt5QuoteEventArgs>(mMtApiClient_QuoteAdded);
            mMtApiClient.QuoteRemoved += new EventHandler<Mt5QuoteEventArgs>(mMtApiClient_QuoteRemoved);
            mMtApiClient.QuoteUpdated += new MtApi5Client.QuoteHandler(mMtApiClient_QuoteUpdated);

            _quotesMap = new Dictionary<string, QuoteViewModel>();

            ConnectionState = mMtApiClient.ConnectionState;
            ConnectionMessage = "Disconnected";
            Port = 8228; //default local port

            InitCommands();

            var request = new MqlTradeRequest { Action = ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_DEAL
                , Type = ENUM_ORDER_TYPE.ORDER_TYPE_BUY
                , Volume = 0.1
                , Comment = "Test Trade Request"
            };

            TradeRequest = new MqlTradeRequestViewModel(request);

            TimeSeriesValues = new TimeSeriesValueViewModel();
            TimeSeriesValues.Count = 100;
        }

        public void Close()
        {
            mMtApiClient.BeginDisconnect();
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
        }

        private bool CanExecuteConnect(object o)
        {
            return ConnectionState == Mt5ConnectionState.Disconnected || ConnectionState == Mt5ConnectionState.Failed;
        }

        private void ExecuteConnect(object o)
        {
            if (string.IsNullOrEmpty(Host))
            {
                mMtApiClient.BeginConnect(Port);
            }
            else
            {
                mMtApiClient.BeginConnect(Host, Port);
            }            
        }

        private bool CanExecuteDisconnect(object o)
        {
            return ConnectionState == Mt5ConnectionState.Connected;
        }

        private void ExecuteDisconnect(object o)
        {
            mMtApiClient.BeginDisconnect();
        }

        private void ExecuteOrderSend(object o)
        {
            var request = TradeRequest.GetMqlTradeRequest();

            MqlTradeResult result;
            bool retVal = mMtApiClient.OrderSend(request, out result);

            string historyItem;

            if (retVal == true)
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
            var result = mMtApiClient.AccountInfoDouble(AccountInfoDoublePropertyId);

            var historyItem = string.Format("AccountInfoDouble: property_id = {0}; result = {1}", AccountInfoDoublePropertyId, result);
            History.Add(historyItem);
        }

        private void ExecuteAccountInfoInteger(object o)
        {
            var result = mMtApiClient.AccountInfoInteger(AccountInfoIntegerPropertyId);

            var historyItem = string.Format("AccountInfoInteger: property_id = {0}; result = {1}", AccountInfoDoublePropertyId, result);
            History.Add(historyItem);
        }

        private void ExecuteAccountInfoString(object o)
        {
            var result = mMtApiClient.AccountInfoString(AccountInfoStringPropertyId);

            var historyItem = string.Format("AccountInfoString: property_id = {0}; result = {1}", AccountInfoDoublePropertyId, result);
            History.Add(historyItem);
        }

        private void ExecuteCopyTime(object o)
        {
            if (TimeSeriesValues != null && string.IsNullOrEmpty(TimeSeriesValues.SymbolValue) == false)
            {
                TimeSeriesResults.Clear();

                DateTime[] timesArray;
                var count = mMtApiClient.CopyTime(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out timesArray);
                if (count > 0)
                {
                    foreach (var time in timesArray)
                    {
                        TimeSeriesResults.Add(time.ToString());
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
                var count = mMtApiClient.CopyOpen(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out opensArray);
                if (count > 0)
                {
                    foreach (var open in opensArray)
                    {
                        TimeSeriesResults.Add(open.ToString());
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
                var count = mMtApiClient.CopyHigh(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out array);
                if (count > 0)
                {
                    foreach (var value in array)
                    {
                        TimeSeriesResults.Add(value.ToString());
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
                var count = mMtApiClient.CopyLow(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out array);
                if (count > 0)
                {
                    foreach (var value in array)
                    {
                        TimeSeriesResults.Add(value.ToString());
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
                var count = mMtApiClient.CopyClose(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out array);
                if (count > 0)
                {
                    foreach (var value in array)
                    {
                        TimeSeriesResults.Add(value.ToString());
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
                var count = mMtApiClient.CopyRates(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out ratesArray);
                if (count > 0)
                {
                    foreach (var rates in ratesArray)
                    {
                        TimeSeriesResults.Add(string.Format("time={0}; open={1}; high={2}; low={3}; close={4}; tick_volume={5}; spread={6}; real_volume={7}",
                            rates.time, rates.open, rates.high, rates.low, rates.close, rates.tick_volume, rates.spread, rates.tick_volume));
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
                var count = mMtApiClient.CopyTickVolume(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out array);
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
                var count = mMtApiClient.CopyRealVolume(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out array);
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
                var count = mMtApiClient.CopySpread(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out array);
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
            var selectedCount = mMtApiClient.SymbolsTotal(true);
            History.Add("SymbolsTotal(true) success, result = " + selectedCount.ToString());

            var commonCount = mMtApiClient.SymbolsTotal(false);
            History.Add("SymbolsTotal(false) success, result = " + selectedCount.ToString());
        }

        private void ExecuteSymbolName(object o)
        {
            var selectedSymbol = mMtApiClient.SymbolName(5, true);
            History.Add("SymbolName(5, true) success, result = " + selectedSymbol);

            var commonSymbol = mMtApiClient.SymbolName(5, false);
            History.Add("SymbolName(5, false) success, result = " + commonSymbol);

        }

        private void ExecuteSymbolSelect(object o)
        {
            var retVal = mMtApiClient.SymbolSelect("AUDJPY", true);
            History.Add("SymbolSelect(AUDJPY, true) success, result = " + retVal);

            //var retVal1 = mMtApiClient.SymbolSelect("AUDJPY", false);
            //History.Add("SymbolSelect(AUDJPY, false) success, result = " + retVal1);
        }

        private void ExecuteSymbolIsSynchronized(object o)
        {
            var retVal = mMtApiClient.SymbolIsSynchronized("EURUSD");
            History.Add("SymbolIsSynchronized(EURUSD) success, result = " + retVal);
        }

        private void ExecuteSymbolInfoDouble(object o)
        {
            var retVal = mMtApiClient.SymbolInfoDouble("EURUSD", ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_BID);
            History.Add("SymbolInfoDouble(EURUSD, ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_BID) success, result = " + retVal);
        }

        private void ExecuteSymbolInfoInteger(object o)
        {
            var retVal = mMtApiClient.SymbolInfoInteger("EURUSD", ENUM_SYMBOL_INFO_INTEGER.SYMBOL_SPREAD);
            History.Add("SymbolInfoInteger(EURUSD, ENUM_SYMBOL_INFO_INTEGER.SYMBOL_SPREAD) success, result = " + retVal);
        }

        private void ExecuteSymbolInfoString(object o)
        {
            var retVal = mMtApiClient.SymbolInfoString("EURUSD", ENUM_SYMBOL_INFO_STRING.SYMBOL_DESCRIPTION);
            History.Add("SymbolInfoString(EURUSD, ENUM_SYMBOL_INFO_STRING.SYMBOL_DESCRIPTION) success, result = " + retVal);
        }

        private void ExecuteSymbolInfoTick(object o)
        {
            MqlTick tick;
            var retVal = mMtApiClient.SymbolInfoTick("EURUSD", out tick);
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
            var retVal = mMtApiClient.SymbolInfoSessionQuote("EURUSD", ENUM_DAY_OF_WEEK.MONDAY, 0, out from, out to);
            History.Add("SymbolInfoSessionQuote(EURUSD) success, result = " + retVal);
            History.Add("SymbolInfoSessionQuote(EURUSD) from = " + from);
            History.Add("SymbolInfoSessionQuote(EURUSD) to = " + to);
        }

        private void ExecuteSymbolInfoSessionTrade(object o)
        {
            DateTime from;
            DateTime to;
            var retVal = mMtApiClient.SymbolInfoSessionTrade("EURUSD", ENUM_DAY_OF_WEEK.MONDAY, 0, out from, out to);
            History.Add("SymbolInfoSessionTrade(EURUSD) success, result = " + retVal);
            History.Add("SymbolInfoSessionTrade(EURUSD) from = " + from);
            History.Add("SymbolInfoSessionTrade(EURUSD) to = " + to);
        }

        private void ExecuteMarketBookAdd(object o)
        {
            var retVal = mMtApiClient.MarketBookAdd("CHFJPY");
            History.Add("MarketBookAdd(CHFJPY) success, result = " + retVal);
        }

        private void ExecuteMarketBookRelease(object o)
        {
            var retVal = mMtApiClient.MarketBookRelease("CHFJPY");
            History.Add("MarketBookRelease(CHFJPY) success, result = " + retVal);
        }

        private void ExecuteMarketBookGet(object o)
        {
            MqlBookInfo[] book;
            var retVal = mMtApiClient.MarketBookGet("EURUSD", out book);
            History.Add("MarketBookGet(EURUSD) success, result = " + retVal);
            if (retVal == true && book != null)
            {
                for (int i = 0; i < book.Length; i++)
                {
                    History.Add(String.Format("MarketBookGet: book[{0}].price = {1}", i, book[i].price));
                    History.Add(String.Format("MarketBookGet: book[{0}].price = {1}", i, book[i].volume));
                    History.Add(String.Format("MarketBookGet: book[{0}].price = {1}", i, book[i].type));
                }
            }
        }

        private void runOnUIThread(Action action)
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(action);
            }            
        }

        private void runOnUIThread<T>(Action<T> action, params Object[] args)
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(action, args);
            }            
        }

        private object mLocker = new object();

        private void mMtApiClient_QuoteUpdated(object sender, string symbol, double bid, double ask)
        {
            if (string.IsNullOrEmpty(symbol) == false)
            {
                if (_quotesMap.ContainsKey(symbol) == true)
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
            runOnUIThread<Mt5Quote>(RemoveQuote, e.Quote);
        }

        private void mMtApiClient_QuoteAdded(object sender, Mt5QuoteEventArgs e)
        {
            runOnUIThread<Mt5Quote>(AddQuote, e.Quote);
        }

        private void mMtApiClient_ConnectionStateChanged(object sender, Mt5ConnectionEventArgs e)
        {
            ConnectionState = e.Status;
            ConnectionMessage = e.ConnectionMessage;

            runOnUIThread(ConnectCommand.RaiseCanExecuteChanged);
            runOnUIThread(DisconnectCommand.RaiseCanExecuteChanged);

            switch (e.Status)
            {
                case Mt5ConnectionState.Connected:
                    runOnUIThread(OnConnected);
                    break;
                case Mt5ConnectionState.Disconnected:
                    runOnUIThread(OnDisconnected);
                    break;
            }
        }

        private void AddQuote(Mt5Quote quote)
        {
            if (quote == null)
                return;

            QuoteViewModel qvm = null;

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

            if (_quotesMap.ContainsKey(quote.Instrument) == true)
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
            var quotes = mMtApiClient.GetQuotes();
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
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Private Fields
        private readonly MtApi5Client mMtApiClient;

        private Dictionary<string, QuoteViewModel> _quotesMap;        
        #endregion
    }
}
