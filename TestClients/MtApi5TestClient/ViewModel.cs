﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using MtApi5;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;

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
        public DelegateCommand CopyTicksCommand { get; private set; }

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

        public DelegateCommand PrintCommand { get; private set; }
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

        private string _messageText = "Print some text in MetaTrader expert console";
        public string MessageText
        {
            get { return _messageText; }
            set
            {
                _messageText = value;
                OnPropertyChanged("MessageText");
            }
        }

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
            CopyTicksCommand = new DelegateCommand(ExecuteCopyTicks);

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

            PrintCommand = new DelegateCommand(ExecutePrint);
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

        private async void ExecuteOrderSend(object o)
        {
            var request = TradeRequest.GetMqlTradeRequest();
            
            var retVal = await Execute(() =>
            {
                MqlTradeResult result;
                var ok = _mtApiClient.OrderSend(request, out result);
                return ok ? result : null;
            });

            var message = retVal != null ? $"OrderSend successed. {MqlTradeResultToString(retVal)}" : "OrderSend failed.";
            AddLog(message);
        }

        private async void ExecuteAccountInfoDouble(object o)
        {
            var result = await Execute(() => _mtApiClient.AccountInfoDouble(AccountInfoDoublePropertyId));

            var message = $"AccountInfoDouble: property_id = {AccountInfoDoublePropertyId}; result = {result}";
            AddLog(message);
        }

        private async void ExecuteAccountInfoInteger(object o)
        {
            var result = await Execute(() => _mtApiClient.AccountInfoInteger(AccountInfoIntegerPropertyId));

            var message = $"AccountInfoInteger: property_id = {AccountInfoDoublePropertyId}; result = {result}";
            AddLog(message);
        }

        private async void ExecuteAccountInfoString(object o)
        {
            var result = await Execute(() => _mtApiClient.AccountInfoString(AccountInfoStringPropertyId));

            var message = $"AccountInfoString: property_id = {AccountInfoDoublePropertyId}; result = {result}";
            AddLog(message);
        }

        private async void ExecuteCopyTime(object o)
        {
            if (string.IsNullOrEmpty(TimeSeriesValues?.SymbolValue)) return;

            TimeSeriesResults.Clear();

            var result = await Execute(() =>
            {
                DateTime[] array;
                var count = _mtApiClient.CopyTime(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out array);
                return count > 0 ? array : null;
            });

            if (result == null)
            {
                AddLog("CopyTime: result is null");
                return;
            }

            RunOnUiThread(() =>
            {
                foreach (var time in result)
                {
                    TimeSeriesResults.Add(time.ToString(CultureInfo.CurrentCulture));
                }
            });

            AddLog("CopyTime: success");
        }

        private async void ExecuteCopyOpen(object o)
        {
            if (string.IsNullOrEmpty(TimeSeriesValues?.SymbolValue)) return;

            TimeSeriesResults.Clear();

            var result = await Execute(() =>
            {
                double[] array;
                var count = _mtApiClient.CopyOpen(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out array);
                return count > 0 ? array : null;
            });

            if (result == null)
            {
                AddLog("CopyOpen: result is null");
                return;
            }

            RunOnUiThread(() =>
            {
                foreach (var open in result)
                {
                    TimeSeriesResults.Add(open.ToString(CultureInfo.CurrentCulture));
                }
            });

            AddLog("CopyOpen: success");
        }

        private async void ExecuteCopyHigh(object o)
        {
            if (string.IsNullOrEmpty(TimeSeriesValues?.SymbolValue)) return;

            TimeSeriesResults.Clear();

            var result = await Execute(() =>
            {
                double[] array;
                var count = _mtApiClient.CopyHigh(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out array);
                return count > 0 ? array : null;
            });

            if (result == null)
            {
                AddLog("CopyHigh: result is null");
                return;
            }

            RunOnUiThread(() =>
            {
                foreach (var value in result)
                {
                    TimeSeriesResults.Add(value.ToString(CultureInfo.CurrentCulture));
                }
            });

            AddLog("CopyHigh: success");
        }

        private async void ExecuteCopyLow(object o)
        {
            if (string.IsNullOrEmpty(TimeSeriesValues?.SymbolValue)) return;

            TimeSeriesResults.Clear();

            var result = await Execute(() =>
            {
                double[] array;
                var count = _mtApiClient.CopyLow(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out array);
                return count > 0 ? array : null;
            });

            if (result == null)
            {
                AddLog("CopyLow: result is null");
                return;
            }

            RunOnUiThread(() =>
            {
                foreach (var value in result)
                {
                    TimeSeriesResults.Add(value.ToString(CultureInfo.CurrentCulture));
                }
            });

            AddLog("CopyLow: success");
        }

        private async void ExecuteCopyClose(object o)
        {
            if (string.IsNullOrEmpty(TimeSeriesValues?.SymbolValue)) return;

            TimeSeriesResults.Clear();

            var result = await Execute(() =>
            {
                double[] array;
                var count = _mtApiClient.CopyClose(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame,
                    TimeSeriesValues.StartPos, TimeSeriesValues.Count, out array);
                return count > 0 ? array : null;
            });

            if (result == null)
            {
                AddLog("CopyClose: result is null");
                return;
            }

            RunOnUiThread(() =>
            {
                foreach (var value in result)
                {
                    TimeSeriesResults.Add(value.ToString(CultureInfo.CurrentCulture));
                }
            });

            AddLog("CopyClose: success");
        }

        private async void ExecuteCopyRates(object o)
        {
            if (string.IsNullOrEmpty(TimeSeriesValues?.SymbolValue)) return;

            TimeSeriesResults.Clear();

            var result = await Execute(() =>
            {
                MqlRates[] array;
                var count = _mtApiClient.CopyRates(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame, TimeSeriesValues.StartPos, TimeSeriesValues.Count, out array);
                return count > 0 ? array : null;
            });

            if (result == null)
            {
                AddLog("CopyRates: result is null");
                return;
            }

            RunOnUiThread(() =>
            {
                foreach (var rates in result)
                {
                    TimeSeriesResults.Add(
                        $"time={rates.time}; open={rates.open}; high={rates.high}; low={rates.low}; close={rates.close}; tick_volume={rates.tick_volume}; spread={rates.spread}; real_volume={rates.tick_volume}");
                }
            });

            AddLog("CopyRates: success");

        }

        private async void ExecuteCopyTickVolume(object o)
        {
            if (string.IsNullOrEmpty(TimeSeriesValues?.SymbolValue)) return;

            TimeSeriesResults.Clear();

            var result = await Execute(() =>
            {
                long[] array;
                var count = _mtApiClient.CopyTickVolume(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame,
                    TimeSeriesValues.StartPos, TimeSeriesValues.Count, out array);
                return count > 0 ? array : null;
            });

            if (result == null)
            {
                AddLog("CopyTickVolume: result is null");
                return;
            }

            RunOnUiThread(() =>
            {
                foreach (var value in result)
                {
                    TimeSeriesResults.Add(value.ToString());
                }
            });

            AddLog("CopyTickVolume: success");
        }

        private async void ExecuteCopyRealVolume(object o)
        {
            if (string.IsNullOrEmpty(TimeSeriesValues?.SymbolValue)) return;

            TimeSeriesResults.Clear();

            var result = await Execute(() =>
            {
                long[] array;
                var count = _mtApiClient.CopyRealVolume(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame,
                    TimeSeriesValues.StartPos, TimeSeriesValues.Count, out array);
                return count > 0 ? array : null;
            });

            if (result == null)
            {
                AddLog("CopyRealVolume: result is null");
                return;
            }

            RunOnUiThread(() =>
            {
                foreach (var value in result)
                {
                    TimeSeriesResults.Add(value.ToString());
                }
            });

            AddLog("CopyRealVolume: success");
        }

        private async void ExecuteCopySpread(object o)
        {
            if (string.IsNullOrEmpty(TimeSeriesValues?.SymbolValue)) return;

            TimeSeriesResults.Clear();

            var result = await Execute(() =>
            {
                int[] array;
                var count = _mtApiClient.CopySpread(TimeSeriesValues.SymbolValue, TimeSeriesValues.TimeFrame,
                    TimeSeriesValues.StartPos, TimeSeriesValues.Count, out array);
                return count > 0 ? array : null;
            });

            if (result == null)
            {
                AddLog("CopySpread: result is null");
                return;
            }

            RunOnUiThread(() =>
            {
                foreach (var value in result)
                {
                    TimeSeriesResults.Add(value.ToString());
                }
            });

            AddLog("CopySpread: success");
        }

        private async void ExecuteCopyTicks(object o)
        {
            if (string.IsNullOrEmpty(TimeSeriesValues?.SymbolValue)) return;

            TimeSeriesResults.Clear();

            var result = await Execute(() => _mtApiClient.CopyTicks(TimeSeriesValues.SymbolValue));

            if (result == null)
            {
                AddLog("CopyTicks: result is null");
                return;
            }

            AddLog("CopyTicks: success.");

            RunOnUiThread(() =>
            {
                foreach (var v in result)
                {
                    var tickStr = $"time = {v.time}, bid = {v.bid}, ask = {v.ask}, last = {v.last}, volume = {v.volume}";
                    TimeSeriesResults.Add(tickStr);
                }
            });
        }

        private async void ExecuteSymbolsTotal(object o)
        {
            var selectedCount = await Execute(() => _mtApiClient.SymbolsTotal(true));
            AddLog($"SymbolsTotal(true) success, result = {selectedCount}");

            var commonCount = await Execute(() => _mtApiClient.SymbolsTotal(false));
            AddLog($"SymbolsTotal(false) success, result = {commonCount}");
        }

        private async void ExecuteSymbolName(object o)
        {
            var selectedSymbol = await Execute(() => _mtApiClient.SymbolName(5, true));
            AddLog($"SymbolName(5, true) success, result = {selectedSymbol}");

            var commonSymbol = await Execute(() => _mtApiClient.SymbolName(5, false));
            AddLog($"SymbolName(5, false) success, result = {commonSymbol}");
        }

        private async void ExecuteSymbolSelect(object o)
        {
            var retVal = await Execute(() => _mtApiClient.SymbolSelect("AUDJPY", true));
            AddLog("SymbolSelect(AUDJPY, true) success, result = " + retVal);

            //var retVal1 = _mtApiClient.SymbolSelect("AUDJPY", false);
            //AddLog("SymbolSelect(AUDJPY, false) success, result = " + retVal1);
        }

        private async void ExecuteSymbolIsSynchronized(object o)
        {
            var retVal = await Execute(() => _mtApiClient.SymbolIsSynchronized("EURUSD"));
            AddLog("SymbolIsSynchronized(EURUSD): result = " + retVal);
        }

        private async void ExecuteSymbolInfoDouble(object o)
        {
            var retVal = await Execute(() => _mtApiClient.SymbolInfoDouble("EURUSD", ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_BID));
            AddLog($"SymbolInfoDouble(EURUSD, ENUM_SYMBOL_INFO_DOUBLE.SYMBOL_BID): result = {retVal}");
        }

        private async void ExecuteSymbolInfoInteger(object o)
        {
            var retVal = await Execute(() => _mtApiClient.SymbolInfoInteger("EURUSD", ENUM_SYMBOL_INFO_INTEGER.SYMBOL_SPREAD));
            AddLog($"SymbolInfoInteger(EURUSD, ENUM_SYMBOL_INFO_INTEGER.SYMBOL_SPREAD): result = {retVal}");
        }

        private async void ExecuteSymbolInfoString(object o)
        {
            var retVal = await Execute(() => _mtApiClient.SymbolInfoString("EURUSD", ENUM_SYMBOL_INFO_STRING.SYMBOL_DESCRIPTION));
            AddLog($"SymbolInfoString(EURUSD, ENUM_SYMBOL_INFO_STRING.SYMBOL_DESCRIPTION): result = {retVal}");
        }

        private async void ExecuteSymbolInfoTick(object o)
        {
            var result = await Execute(() =>
            {
                MqlTick tick;
                var ok = _mtApiClient.SymbolInfoTick("EURUSD", out tick);
                return ok ? tick : null;
            });

            if (result == null) return;
            AddLog("SymbolInfoTick(EURUSD) success");
            AddLog($"SymbolInfoTick(EURUSD) tick.time = {result.time}");
            AddLog($"SymbolInfoTick(EURUSD) tick.bid = {result.bid}");
            AddLog($"SymbolInfoTick(EURUSD) tick.ask = {result.ask}");
            AddLog($"SymbolInfoTick(EURUSD) tick.last = {result.last}");
            AddLog($"SymbolInfoTick(EURUSD) tick.volume = {result.volume}");
        }

        private async void ExecuteSymbolInfoSessionQuote(object o)
        {
            var retVal = await Execute(() =>
            {
                DateTime from;
                DateTime to;
                var ok = _mtApiClient.SymbolInfoSessionQuote("EURUSD", ENUM_DAY_OF_WEEK.MONDAY, 0, out from, out to);
                if (ok)
                {
                    AddLog($"SymbolInfoSessionQuote(EURUSD) from = {from}");
                    AddLog($"SymbolInfoSessionQuote(EURUSD) to = {to}");
                }
                return ok;
            });

            AddLog($"SymbolInfoSessionQuote(EURUSD): result = {retVal}");

        }

        private async void ExecuteSymbolInfoSessionTrade(object o)
        {
            var retVal = await Execute(() =>
            {
                DateTime from;
                DateTime to;
                var ok = _mtApiClient.SymbolInfoSessionTrade("EURUSD", ENUM_DAY_OF_WEEK.MONDAY, 0, out from, out to);
                if (ok)
                {
                    AddLog($"SymbolInfoSessionTrade(EURUSD) from = {from}");
                    AddLog($"SymbolInfoSessionTrade(EURUSD) to = {to}");
                }
                return ok;
            });

            AddLog("SymbolInfoSessionTrade(EURUSD): result = " + retVal);
        }

        private async void ExecuteMarketBookAdd(object o)
        {
            var retVal = await Execute(() => _mtApiClient.MarketBookAdd("CHFJPY"));
            AddLog($"MarketBookAdd(CHFJPY): result = {retVal}");
        }

        private async void ExecuteMarketBookRelease(object o)
        {
            var retVal = await Execute(() => _mtApiClient.MarketBookRelease("CHFJPY"));
            AddLog($"MarketBookRelease(CHFJPY): result = {retVal}");
        }

        private async void ExecuteMarketBookGet(object o)
        {
            var result = await Execute(() =>
            {
                MqlBookInfo[] book;
                var ok = _mtApiClient.MarketBookGet("EURUSD", out book);
                return ok ? book : null;
            });

            if (result == null)
            {
                AddLog("MarketBookGet(EURUSD): result is null");
                return;
            }

            AddLog("MarketBookGet(EURUSD): success");

            for (var i = 0; i < result.Length; i++)
            {
                AddLog($"MarketBookGet: book[{i}].price = {result[i].price}");
                AddLog($"MarketBookGet: book[{i}].price = {result[i].volume}");
                AddLog($"MarketBookGet: book[{i}].price = {result[i].type}");
            }
        }

        private async void ExecutePositionOpen(object obj)
        {
            const string symbol = "EURUSD";
            const ENUM_ORDER_TYPE orderType = ENUM_ORDER_TYPE.ORDER_TYPE_BUY;
            const double volume = 0.1;
            const double price = 1.013;
            const double sl = 1.00;
            const double tp = 1.020;
            const string comment = "Test PositionOpen";

            var retVal = await Execute (() => _mtApiClient.PositionOpen(symbol, orderType, volume, price, sl, tp, comment));
            AddLog($"PositionOpen: symbol EURUSD result = {retVal}");
        }

        private async void ExecutePrint(object obj)
        {
            var message = MessageText;

            var retVal = await Execute(() => _mtApiClient.Print(message));
            AddLog($"Print: message print in MetaTrader - {retVal}");
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
            if (quote == null) return;

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
            if (SelectedQuote == null) return;

            TradeRequest.Symbol = SelectedQuote.Instrument;
            TimeSeriesValues.SymbolValue = SelectedQuote.Instrument;
        }

        private async Task<TResult> Execute<TResult>(Func<TResult> func)
        {
            return await Task.Factory.StartNew(() =>
            {
                var result = default(TResult);
                try
                {
                    result = func();
                }
                catch (Exception ex)
                {
                    AddLog($"Exception: {ex.Message}");
                }

                return result;
            });
        }

        private void AddLog(string msg)
        {
            RunOnUiThread(() =>
            {
                var time = DateTime.Now.ToString("h:mm:ss tt");
                History.Add($"[{time}]: {msg}");
            });
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
