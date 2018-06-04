// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using MtApi5;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32;

namespace MtApi5TestClient
{
    public class ViewModel : INotifyPropertyChanged
    {
        #region Commands
        public DelegateCommand ConnectCommand { get; private set; }
        public DelegateCommand DisconnectCommand { get; private set; }

        public DelegateCommand OrderSendCommand { get; private set; }
        public DelegateCommand OrderCheckCommand { get; private set; }
        public DelegateCommand PositionGetTicketCommand { get; private set; }

        public DelegateCommand HistoryOrderGetIntegerCommand { get; private set; }
        public DelegateCommand HistoryDealGetDoubleCommand { get; private set; }
        public DelegateCommand HistoryDealGetIntegerCommand { get; private set; }
        public DelegateCommand HistoryDealGetStringCommand { get; private set; }
        public DelegateCommand HistoryDealMethodsCommand { get; private set; }

        public DelegateCommand AccountInfoDoubleCommand { get; private set; }
        public DelegateCommand AccountInfoIntegerCommand { get; private set; }
        public DelegateCommand AccountInfoStringCommand { get; private set; }

        public DelegateCommand TerminalInfoDoubleCommand { get; private set; }
        public DelegateCommand TerminalInfoIntegerCommand { get; private set; }
        public DelegateCommand TerminalInfoStringCommand { get; private set; }

        public DelegateCommand CopyRatesCommand { get; private set; }
        public DelegateCommand CopyTimesCommand { get; private set; }
        public DelegateCommand CopyOpenCommand { get; private set; }
        public DelegateCommand CopyHighCommand { get; private set; }
        public DelegateCommand CopyLowCommand { get; private set; }
        public DelegateCommand CopyCloseCommand { get; private set; }
        public DelegateCommand IndicatorCreateCommand { get; private set; }
        public DelegateCommand IndicatorReleaseCommand { get; private set; }

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
        public DelegateCommand SymbolInfoString2Command { get; private set; }
        public DelegateCommand SymbolInfoTickCommand { get; private set; }
        public DelegateCommand SymbolInfoSessionQuoteCommand { get; private set; }
        public DelegateCommand SymbolInfoSessionTradeCommand { get; private set; }
        public DelegateCommand MarketBookAddCommand { get; private set; }
        public DelegateCommand MarketBookReleaseCommand { get; private set; }
        public DelegateCommand MarketBookGetCommand { get; private set; }

        public DelegateCommand PositionOpenCommand { get; private set; }
        public DelegateCommand PositionCloseCommand { get; private set; }

        public DelegateCommand GetLastErrorCommand { get; private set; }
        public DelegateCommand ResetLastErrorCommand { get; private set; }
        public DelegateCommand PrintCommand { get; private set; }
        public DelegateCommand AlertCommand { get; private set; }

        public DelegateCommand iCustomCommand { get; private set; }

        public DelegateCommand TimeCurrentCommand { get; private set; }

        public DelegateCommand ChartOpenCommand { get; private set; }
        public DelegateCommand ChartTimePriceToXYCommand { get; private set; }
        public DelegateCommand ChartXYToTimePriceCommand { get; private set; }
        public DelegateCommand ChartApplyTemplateCommand { get; private set; }
        public DelegateCommand ChartSaveTemplateCommand { get; private set; }
        public DelegateCommand ChartIdCommand { get; private set; }
        public DelegateCommand ChartRedrawCommand { get; private set; }
        public DelegateCommand ChartWindowFindCommand { get; private set; }
        public DelegateCommand ChartCloseCommand { get; private set; }
        public DelegateCommand ChartPeriodCommand { get; private set; }
        public DelegateCommand ChartSetDoubleCommand { get; private set; }
        public DelegateCommand ChartSetIntegerCommand { get; private set; }
        public DelegateCommand ChartSetStringCommand { get; private set; }
        public DelegateCommand ChartGetDoubleCommand { get; private set; }
        public DelegateCommand ChartGetIntegerCommand { get; private set; }
        public DelegateCommand ChartNavigateCommand { get; private set; }
        public DelegateCommand ChartIndicatorAddCommand { get; private set; }
        public DelegateCommand ChartIndicatorDeleteCommand { get; private set; }
        public DelegateCommand ChartIndicatorGetCommand { get; private set; }
        public DelegateCommand ChartIndicatorNameCommand { get; private set; }
        public DelegateCommand ChartIndicatorsTotalCommand { get; private set; }
        public DelegateCommand ChartWindowOnDroppedCommand { get; private set; }
        public DelegateCommand ChartPriceOnDroppedCommand { get; private set; }
        public DelegateCommand ChartTimeOnDroppedCommand { get; private set; }
        public DelegateCommand ChartXOnDroppedCommand { get; private set; }
        public DelegateCommand ChartYOnDroppedCommand { get; private set; }
        public DelegateCommand ChartSetSymbolPeriodCommand { get; private set; }
        public DelegateCommand ChartScreenShotCommand { get; private set; }

        public DelegateCommand TimeTradeServerCommand { get; private set; }
        public DelegateCommand TimeLocalCommand { get; private set; }
        public DelegateCommand TimeGMTCommand { get; private set; }

        public DelegateCommand GlobalVariableCheckCommand { get; private set; }
        public DelegateCommand GlobalVariableTimeCommand { get; private set; }
        public DelegateCommand GlobalVariableDelCommand { get; private set; }
        public DelegateCommand GlobalVariableGetCommand { get; private set; }
        public DelegateCommand GlobalVariableNameCommand { get; private set; }
        public DelegateCommand GlobalVariableSetCommand { get; private set; }
        public DelegateCommand GlobalVariablesFlushCommand { get; private set; }
        public DelegateCommand GlobalVariableTempCommand { get; private set; }
        public DelegateCommand GlobalVariableSetOnConditionCommand { get; private set; }
        public DelegateCommand GlobalVariablesDeleteAllCommand { get; private set; }
        public DelegateCommand GlobalVariablesTotalCommand { get; private set; }
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

        public ENUM_TERMINAL_INFO_DOUBLE TerminalInfoDoublePropertyId { get; set; }
        public ENUM_TERMINAL_INFO_INTEGER TerminalInfoIntegerPropertyId { get; set; }
        public ENUM_TERMINAL_INFO_STRING TerminalInfoStringPropertyId { get; set; }


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

        private string _chartFunctionsSymbolValue = "EURUSD";
        public string ChartFunctionsSymbolValue
        {
            get { return _chartFunctionsSymbolValue; }
            set
            {
                _chartFunctionsSymbolValue = value;
                OnPropertyChanged("ChartFunctionsSymbolValue");
            }
        }

        private long _chartFunctionsChartIdValue;
        public long ChartFunctionsChartIdValue
        {
            get { return _chartFunctionsChartIdValue; }
            set
            {
                _chartFunctionsChartIdValue = value;
                OnPropertyChanged("ChartFunctionsChartIdValue");
            }
        }

        private string _globalVarName;
        public string GlobalVarName
        {
            get { return _globalVarName; }
            set
            {
                _globalVarName = value;
                OnPropertyChanged("GlobalVarName");
            }
        }

        private double _globalVarValue;
        public double GlobalVarValue
        {
            get { return _globalVarValue; }
            set
            {
                _globalVarValue = value;
                OnPropertyChanged("GlobalVarValue");
            }
        }

        private ulong _positionTicketValue;
        public ulong PositionTicketValue
        {
            get { return _positionTicketValue; }
            set
            {
                _positionTicketValue = value;
                OnPropertyChanged("PositionTicketValue");
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
            _mtApiClient.QuoteUpdate += mMtApiClient_QuoteUpdate;
            _mtApiClient.OnTradeTransaction += mMtApiClient_OnTradeTransaction;
            _mtApiClient.OnBookEvent += _mtApiClient_OnBookEvent;

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
            OrderCheckCommand = new DelegateCommand(ExecuteOrderCheck);
            PositionGetTicketCommand = new DelegateCommand(ExecutePositionGetTicket);

            HistoryOrderGetIntegerCommand = new DelegateCommand(ExecuteHistoryOrderGetInteger);
            HistoryDealGetDoubleCommand = new DelegateCommand(ExecuteHistoryDealGetDouble);
            HistoryDealGetIntegerCommand = new DelegateCommand(ExecuteHistoryDealGetInteger);
            HistoryDealGetStringCommand = new DelegateCommand(ExecuteHistoryDealGetString);
            HistoryDealMethodsCommand = new DelegateCommand(ExecuteHistoryDealMethods);

            AccountInfoDoubleCommand = new DelegateCommand(ExecuteAccountInfoDouble);
            AccountInfoIntegerCommand = new DelegateCommand(ExecuteAccountInfoInteger);
            AccountInfoStringCommand = new DelegateCommand(ExecuteAccountInfoString);

            TerminalInfoDoubleCommand = new DelegateCommand(ExecuteTerminalInfoDouble);
            TerminalInfoIntegerCommand = new DelegateCommand(ExecuteTerminalInfoInteger);
            TerminalInfoStringCommand = new DelegateCommand(ExecuteTerminalInfoString);

            CopyRatesCommand = new DelegateCommand(ExecuteCopyRates);
            CopyTimesCommand = new DelegateCommand(ExecuteCopyTime);
            CopyOpenCommand = new DelegateCommand(ExecuteCopyOpen);
            CopyHighCommand = new DelegateCommand(ExecuteCopyHigh);
            CopyLowCommand = new DelegateCommand(ExecuteCopyLow);
            CopyCloseCommand = new DelegateCommand(ExecuteCopyClose);
            IndicatorCreateCommand = new DelegateCommand(ExecuteIndicatorCreate);
            IndicatorReleaseCommand = new DelegateCommand(ExecuteIndicatorRelease);

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
            SymbolInfoString2Command = new DelegateCommand(ExecuteSymbolInfoString2);
            SymbolInfoTickCommand = new DelegateCommand(ExecuteSymbolInfoTick);
            SymbolInfoSessionQuoteCommand = new DelegateCommand(ExecuteSymbolInfoSessionQuote);
            SymbolInfoSessionTradeCommand = new DelegateCommand(ExecuteSymbolInfoSessionTrade);
            MarketBookAddCommand = new DelegateCommand(ExecuteMarketBookAdd);   
            MarketBookReleaseCommand = new DelegateCommand(ExecuteMarketBookRelease);
            MarketBookGetCommand = new DelegateCommand(ExecuteMarketBookGet);

            PositionOpenCommand = new DelegateCommand(ExecutePositionOpen);
            PositionCloseCommand = new DelegateCommand(ExecutePositionClose);

            PrintCommand = new DelegateCommand(ExecutePrint);
            AlertCommand = new DelegateCommand(ExecuteAlert);
            GetLastErrorCommand = new DelegateCommand(ExecuteGetLastError);
            ResetLastErrorCommand = new DelegateCommand(ExecuteResetLastError);

            iCustomCommand = new DelegateCommand(ExecuteICustom);

            ChartOpenCommand = new DelegateCommand(ExecuteChartOpen);
            ChartTimePriceToXYCommand = new DelegateCommand(ExecuteChartTimePriceToXY);
            ChartXYToTimePriceCommand = new DelegateCommand(ExecuteChartXYToTimePrice);
            ChartApplyTemplateCommand = new DelegateCommand(ExecuteChartApplyTemplate);
            ChartSaveTemplateCommand = new DelegateCommand(ExecuteChartSaveTemplate);
            ChartIdCommand = new DelegateCommand(ExecuteChartId);
            ChartRedrawCommand = new DelegateCommand(ExecuteChartRedraw);
            ChartWindowFindCommand = new DelegateCommand(ExecuteChartWindowFind);
            ChartCloseCommand = new DelegateCommand(ExecuteChartClose);
            ChartPeriodCommand = new DelegateCommand(ExecuteChartPeriod);
            ChartSetDoubleCommand = new DelegateCommand(ExecuteChartSetDouble);
            ChartSetIntegerCommand = new DelegateCommand(ExecuteChartSetInteger);
            ChartSetStringCommand = new DelegateCommand(ExecuteChartSetString);
            ChartGetDoubleCommand = new DelegateCommand(ExecuteChartGetDouble);
            ChartGetIntegerCommand = new DelegateCommand(ExecuteChartGetInteger);
            ChartNavigateCommand = new DelegateCommand(ExecuteChartNavigate);
            ChartIndicatorAddCommand = new DelegateCommand(ExecuteChartIndicatorAdd);
            ChartIndicatorDeleteCommand = new DelegateCommand(ExecuteChartIndicatorDelete);
            ChartIndicatorGetCommand = new DelegateCommand(ExecuteChartIndicatorGet);
            ChartIndicatorNameCommand = new DelegateCommand(ExecuteChartIndicatorName);
            ChartIndicatorsTotalCommand = new DelegateCommand(ExecuteChartIndicatorsTotal);
            ChartWindowOnDroppedCommand = new DelegateCommand(ExecuteChartWindowOnDropped);
            ChartPriceOnDroppedCommand = new DelegateCommand(ExecuteChartPriceOnDropped);
            ChartTimeOnDroppedCommand = new DelegateCommand(ExecuteChartTimeOnDropped);
            ChartXOnDroppedCommand = new DelegateCommand(ExecuteChartXOnDropped);
            ChartYOnDroppedCommand = new DelegateCommand(ExecuteChartYOnDropped);
            ChartSetSymbolPeriodCommand = new DelegateCommand(ExecuteChartSetSymbolPeriod);
            ChartScreenShotCommand = new DelegateCommand(ExecuteChartScreenShot);

            TimeCurrentCommand = new DelegateCommand(ExecuteTimeCurrent);
            TimeTradeServerCommand = new DelegateCommand(ExecuteTimeTradeServer);
            TimeLocalCommand = new DelegateCommand(ExecuteTimeLocal);
            TimeGMTCommand = new DelegateCommand(ExecuteTimeGMT);

            GlobalVariableCheckCommand = new DelegateCommand(ExecuteGlobalVariableCheck);
            GlobalVariableTimeCommand = new DelegateCommand(ExecuteGlobalVariableTime);
            GlobalVariableDelCommand = new DelegateCommand(ExecuteGlobalVariableDel);
            GlobalVariableGetCommand = new DelegateCommand(ExecuteGlobalVariableGet);
            GlobalVariableNameCommand = new DelegateCommand(ExecuteGlobalVariableName);
            GlobalVariableSetCommand = new DelegateCommand(ExecuteGlobalVariableSet);
            GlobalVariablesFlushCommand = new DelegateCommand(ExecuteGlobalVariablesFlush);
            GlobalVariableTempCommand = new DelegateCommand(ExecuteGlobalVariableTemp);
            GlobalVariableSetOnConditionCommand = new DelegateCommand(ExecuteGlobalVariableSetOnCondition);
            GlobalVariablesDeleteAllCommand = new DelegateCommand(ExecuteGlobalVariablesDeleteAll);
            GlobalVariablesTotalCommand = new DelegateCommand(ExecuteGlobalVariablesTotal);
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
            MqlTradeResult result = null;
            var retVal = await Execute(() =>
            {
                var ok = _mtApiClient.OrderSend(request, out result);
                return ok;
            });

            var message = retVal ? $"OrderSend: success. {result}" : $"OrderSend: fail. {result}";
            AddLog(message);
        }


        private async void ExecuteOrderCheck(object obj)
        {
            var request = TradeRequest.GetMqlTradeRequest();
            MqlTradeCheckResult result = null;
            var retVal = await Execute(() =>
            {
                var ok = _mtApiClient.OrderCheck(request, out result);
                return ok;
            });

            var message = retVal ? $"OrderCheck: success. {result}" : $"OrderCheck: fail. {result}";
            AddLog(message);
        }

        private async void ExecutePositionGetTicket(object obj)
        {
            const int index = 0;
            var retVal = await Execute(() =>
            {
                var ok = _mtApiClient.PositionGetTicket(index);
                return ok;
            });

            var message = $"PositionGetTicket: result = {retVal}";
            AddLog(message);
        }

        private async void ExecuteHistoryOrderGetInteger(object o)
        {
            const ulong ticket = 12345;
            const ENUM_ORDER_PROPERTY_INTEGER propertyId = ENUM_ORDER_PROPERTY_INTEGER.ORDER_POSITION_ID;

            var retVal = await Execute(() => _mtApiClient.HistoryOrderGetInteger(ticket, propertyId));

            AddLog($"HistoryOrderGetInteger: {retVal}");
        }

        private async void ExecuteHistoryDealGetDouble(object o)
        {
            const ulong ticket = 12345;
            const ENUM_DEAL_PROPERTY_DOUBLE propertyId = ENUM_DEAL_PROPERTY_DOUBLE.DEAL_PROFIT;

            var retVal = await Execute(() => _mtApiClient.HistoryDealGetDouble(ticket, propertyId));

            AddLog($"HistoryDealGetDouble: {retVal}");
        }

        private async void ExecuteHistoryDealGetInteger(object o)
        {
            const ulong ticket = 12345;
            const ENUM_DEAL_PROPERTY_INTEGER propertyId = ENUM_DEAL_PROPERTY_INTEGER.DEAL_TICKET;

            var retVal = await Execute(() => _mtApiClient.HistoryDealGetInteger(ticket, propertyId));

            AddLog($"HistoryDealGetInteger: {retVal}");
        }

        private async void ExecuteHistoryDealGetString(object o)
        {
            const ulong ticket = 12345;
            const ENUM_DEAL_PROPERTY_STRING propertyId = ENUM_DEAL_PROPERTY_STRING.DEAL_SYMBOL;

            var retVal = await Execute(() => _mtApiClient.HistoryDealGetString(ticket, propertyId));

            AddLog($"HistoryDealGetString: {retVal}");
        }

        private async void ExecuteHistoryDealMethods(object o)
        {
            try
            {
                var posId = await Execute(() => _mtApiClient.PositionGetInteger(ENUM_POSITION_PROPERTY_INTEGER.POSITION_IDENTIFIER));
                var history = await Execute(() => _mtApiClient.HistorySelectByPosition(posId));
                var historyDealsTotal = await Execute(() => _mtApiClient.HistoryDealsTotal());
                var histDealTicket = await Execute(() => _mtApiClient.HistoryDealGetTicket(0));
                var histDealPrice = await Execute(() => _mtApiClient.HistoryDealGetDouble(histDealTicket, ENUM_DEAL_PROPERTY_DOUBLE.DEAL_PRICE));
            }
            catch (Exception ex)
            {
                AddLog(ex.Message);
                return;
            }

            AddLog("ExecuteHistoryDealMethods: success.");
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

            var message = $"AccountInfoInteger: property_id = {AccountInfoIntegerPropertyId}; result = {result}";
            AddLog(message);
        }

        private async void ExecuteAccountInfoString(object o)
        {
            var result = await Execute(() => _mtApiClient.AccountInfoString(AccountInfoStringPropertyId));

            var message = $"AccountInfoString: property_id = {AccountInfoStringPropertyId}; result = {result}";
            AddLog(message);
        }

        private async void ExecuteTerminalInfoDouble(object o)
        {
            var result = await Execute(() => _mtApiClient.TerminalInfoDouble(TerminalInfoDoublePropertyId));

            var message = $"TerminalInfoDouble: property_id = {TerminalInfoDoublePropertyId}; result = {result}";
            AddLog(message);
        }

        private async void ExecuteTerminalInfoInteger(object o)
        {
            var result = await Execute(() => _mtApiClient.TerminalInfoInteger(TerminalInfoIntegerPropertyId));

            var message = $"TerminalInfoInteger: property_id = {TerminalInfoIntegerPropertyId}; result = {result}";
            AddLog(message);
        }

        private async void ExecuteTerminalInfoString(object o)
        {
            var result = await Execute(() => _mtApiClient.TerminalInfoString(TerminalInfoStringPropertyId));

            var message = $"TerminalInfoString: property_id = {TerminalInfoStringPropertyId}; result = {result}";
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

        private async void ExecuteIndicatorCreate(object o)
        {
            var parameters = new List<MqlParam>
            {
                new MqlParam
                {
                    DataType = ENUM_DATATYPE.TYPE_INT,
                    IntegerValue = 12
                },
                new MqlParam
                {
                    DataType = ENUM_DATATYPE.TYPE_INT,
                    IntegerValue = 26
                },
                new MqlParam
                {
                    DataType = ENUM_DATATYPE.TYPE_INT,
                    IntegerValue = 9
                },
                new MqlParam
                {
                    DataType = ENUM_DATATYPE.TYPE_INT,
                    IntegerValue = (int)ENUM_APPLIED_PRICE.PRICE_CLOSE
                }
            };

            var retVal = await Execute(() => 
                _mtApiClient.IndicatorCreate(TimeSeriesValues.SymbolValue, 
                TimeSeriesValues.TimeFrame, TimeSeriesValues.IndicatorType, parameters));

            TimeSeriesValues.IndicatorHandle = retVal;
            AddLog($"IndicatorCreate [IND_MA]: result - {retVal}");
        }

        private async void ExecuteIndicatorRelease(object o)
        {
            var indicatorHandle = TimeSeriesValues.IndicatorHandle;
   
            var retVal = await Execute(() => _mtApiClient.IndicatorRelease(indicatorHandle));
            AddLog($"IndicatorRelease [{indicatorHandle}]: result - {retVal}");
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
                        $"time={rates.time}; mt_time={rates.mt_time}; open={rates.open}; high={rates.high}; low={rates.low}; close={rates.close}; tick_volume={rates.tick_volume}; spread={rates.spread}; real_volume={rates.tick_volume}");
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

        private async void ExecuteSymbolInfoString2(object o)
        {
            string stringVar = null;

            var retVal = await Execute(() => _mtApiClient.SymbolInfoString("EURUSD", ENUM_SYMBOL_INFO_STRING.SYMBOL_DESCRIPTION, out stringVar));
            AddLog($"SymbolInfoString-2 (EURUSD, ENUM_SYMBOL_INFO_STRING.SYMBOL_DESCRIPTION): result = {retVal}, stringVar = {stringVar}");
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
            var retVal = await Execute(() => _mtApiClient.MarketBookAdd("EURUSD"));
            AddLog($"MarketBookAdd(EURUSD): result = {retVal}");
        }

        private async void ExecuteMarketBookRelease(object o)
        {
            var retVal = await Execute(() => _mtApiClient.MarketBookRelease("EURUSD"));
            AddLog($"MarketBookRelease(EURUSD): result = {retVal}");
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

            AddLog($"MarketBookGet(EURUSD): success. Count = {result.Length}");

            for (var i = 0; i < result.Length; i++)
            {
                AddLog($"MarketBookGet: [{i}] - {result[i].price} | {result[i].volume} | {result[i].type}");
            }
        }

        private async void ExecutePositionOpen(object obj)
        {
            const string symbol = "EURUSD";
            const ENUM_ORDER_TYPE orderType = ENUM_ORDER_TYPE.ORDER_TYPE_BUY;
            const double volume = 0.1;
            const double price = 1.18129;
            const double sl = 1.1811;
            const double tp = 1.1814;
            const string comment = "Test PositionOpen";
            MqlTradeResult tradeResult = null;

            var retVal = await Execute (() => _mtApiClient.PositionOpen(symbol, orderType, volume, price, sl, tp, comment, out tradeResult));
            AddLog($"PositionOpen: symbol EURUSD retVal = {retVal}, result = {tradeResult}");
        }

        private async void ExecutePositionClose(object obj)
        {
            var ticket = PositionTicketValue;
            MqlTradeResult tradeResult = null;

            var retVal = await Execute(() => _mtApiClient.PositionClose(ticket, out tradeResult));
            AddLog($"PositionClose: ticket {ticket} retVal = {retVal}, result = {tradeResult}");
        }

        private async void ExecutePrint(object obj)
        {
            var message = MessageText;

            var retVal = await Execute(() => _mtApiClient.Print(message));
            AddLog($"Print: message print in MetaTrader - {retVal}");
        }

        private void ExecuteAlert(object obj)
        {
            var message = MessageText;

            _mtApiClient.Alert(message);
            AddLog($"Alert: send alert to MetaTrader - {message}.");
        }

        private async void ExecuteGetLastError(object obj)
        {
            var retVal = await Execute(() => _mtApiClient.GetLastError());
            AddLog($"GetLastError: last error = {retVal}");
        }

        private void ExecuteResetLastError(object obj)
        {
            _mtApiClient.ResetLastError();
            AddLog("ResetLastError: executed.");
        }

        private async void ExecuteICustom(object o)
        {
            const string symbol = "EURUSD";
            const ENUM_TIMEFRAMES timeframe = ENUM_TIMEFRAMES.PERIOD_H1;
            const string name = @"Examples\Custom Moving Average";
            int[] parameters = { 0, 21, (int)ENUM_APPLIED_PRICE.PRICE_CLOSE };

            var retVal = await Execute(() => _mtApiClient.iCustom(symbol, timeframe, name, parameters));
            AddLog($"Custom Moving Average: result - {retVal}");
        }

        private async void ExecuteTimeCurrent(object o)
        {
            var retVal = await Execute(() => _mtApiClient.TimeCurrent());
            AddLog($"TimeCurrent: {retVal}");
        }

        private async void ExecuteTimeTradeServer(object o)
        {
            var retVal = await Execute(() => _mtApiClient.TimeTradeServer());
            AddLog($"TimeTradeServer: {retVal}");
        }

        private async void ExecuteTimeLocal(object o)
        {
            var retVal = await Execute(() => _mtApiClient.TimeLocal());
            AddLog($"TimeLocal: {retVal}");
        }

        private async void ExecuteTimeGMT(object o)
        {
            var retVal = await Execute(() => _mtApiClient.TimeGMT());
            AddLog($"TimeGMT: {retVal}");
        }

        #region Global Variable Commands
        private async void ExecuteGlobalVariableCheck(object obj)
        {
            var name = GlobalVarName;
            var retVal = await Execute(() => _mtApiClient.GlobalVariableCheck(name));
            AddLog($"GlobalVariableCheck: {retVal}");
        }

        private async void ExecuteGlobalVariableTime(object obj)
        {
            var name = GlobalVarName;
            var retVal = await Execute(() => _mtApiClient.GlobalVariableTime(name));
            AddLog($"GlobalVariableTime: {retVal}");
        }

        private async void ExecuteGlobalVariableDel(object obj)
        {
            var name = GlobalVarName;
            var retVal = await Execute(() => _mtApiClient.GlobalVariableDel(name));
            AddLog($"GlobalVariableDel: {retVal}");
        }

        private async void ExecuteGlobalVariableGet(object obj)
        {
            var name = GlobalVarName;
            var retVal = await Execute(() => _mtApiClient.GlobalVariableGet(name));
            GlobalVarValue = retVal;
            AddLog($"GlobalVariableGet: {retVal}");
        }

        private async void ExecuteGlobalVariableName(object obj)
        {
            var retVal = await Execute(() => _mtApiClient.GlobalVariableName(0));
            GlobalVarName = retVal;
            AddLog($"GlobalVariableName: {retVal}");
        }

        private async void ExecuteGlobalVariableSet(object obj)
        {
            var name = GlobalVarName;
            var value = GlobalVarValue;
            var retVal = await Execute(() => _mtApiClient.GlobalVariableSet(name, value));
            AddLog($"GlobalVariableSet: {retVal}");
        }

        private void ExecuteGlobalVariablesFlush(object obj)
        {
            _mtApiClient.GlobalVariablesFlush();
            AddLog("GlobalVariablesFlush: executed.");
        }

        private async void ExecuteGlobalVariableTemp(object obj)
        {
            var name = GlobalVarName;
            var retVal = await Execute(() => _mtApiClient.GlobalVariableTemp(name));
            AddLog($"GlobalVariableTemp: {retVal}");
        }

        private async void ExecuteGlobalVariableSetOnCondition(object obj)
        {
            var name = GlobalVarName;
            var value = GlobalVarValue;
            const double checkValue = 2;
            var retVal = await Execute(() => _mtApiClient.GlobalVariableSetOnCondition(name, value, checkValue));
            AddLog($"GlobalVariableSetOnCondition: {retVal}");
        }

        private async void ExecuteGlobalVariablesDeleteAll(object obj)
        {
            var retVal = await Execute(() => _mtApiClient.GlobalVariablesDeleteAll());
            AddLog($"GlobalVariablesDeleteAll: {retVal}");
        }

        private async void ExecuteGlobalVariablesTotal(object obj)
        {
            var retVal = await Execute(() => _mtApiClient.GlobalVariablesTotal());
            AddLog($"GlobalVariablesTotal: {retVal}");
        }
        #endregion

        #region Chart Commands
        private async void ExecuteChartOpen(object o)
        {
            AddLog("Executed #1");
            if (string.IsNullOrEmpty(ChartFunctionsSymbolValue))
            {
                AddLog("ChartOpen [ERROR]: Symbol is not defined!");
                return;
            }

            AddLog($"Executed #2 s:{ChartFunctionsSymbolValue}");


            var result = await Execute(() =>
            {
                var SymbolAddReturn = _mtApiClient.SymbolSelect(ChartFunctionsSymbolValue, true);
                var ChartId = _mtApiClient.ChartOpen(ChartFunctionsSymbolValue, TimeSeriesValues.TimeFrame);
                return ChartId;
            });

            if (result == -1)
            {
                AddLog("ChartOpen: result is null");
                return;
            }

            AddLog($"ChartOpen: success chartid=>{result}");
        }

        private async void ExecuteChartTimePriceToXY(object o)
        {
            const long chartId = 0;
            const int subWindow = 0;
            var time = DateTime.Now;
            const double price = 1.131;
            var x = 0;
            var y = 0;

            var result = await Execute(() => _mtApiClient.ChartTimePriceToXY(chartId, subWindow, time, price, out x, out y));
            if (result == false)
            {
                AddLog("ChartTimePriceToXY: result is false");
                return;
            }

            AddLog($"ChartTimePriceToXY: success. x = {x}; Y = {y}");
        }

        private async void ExecuteChartXYToTimePrice(object o)
        {
            const long chartId = 0;
            const int x = 0;
            const int y = 0;

            var subWindow = 0;
            DateTime? time = null;
            double price = double.NaN;

            var result = await Execute(() => _mtApiClient.ChartXYToTimePrice(chartId, x, y, out subWindow, out time, out price));
            if (result == false)
            {
                AddLog("ChartXYToTimePrice: result is false");
                return;
            }

            AddLog($"ChartXYToTimePrice: success. subWindow = {subWindow}; time = {time}; price = {price}");
        }

        private async void ExecuteChartApplyTemplate(object o)
        {
            if (string.IsNullOrEmpty(TimeSeriesValues?.SymbolValue)) return;

            AddLog($"ExecuteChartApplyTemplate #2 s:{TimeSeriesValues?.SymbolValue}");


            var result = await Execute(() =>
            {
                var SymbolAddReturn = _mtApiClient.SymbolSelect(TimeSeriesValues?.SymbolValue, true);
                var ChartId = _mtApiClient.ChartOpen(TimeSeriesValues?.SymbolValue, TimeSeriesValues.TimeFrame);

                var MT5Path = _mtApiClient.TerminalInfoString(ENUM_TERMINAL_INFO_STRING.TERMINAL_DATA_PATH);

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Template File (*.tpl)|*.tpl|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == true)
                {
                    var TemplateName = "\\Files\\mt5api_copy.tpl";
                    var TemplateStringContent = File.ReadAllLines(openFileDialog.FileName);
                    var DestPath = $"{MT5Path}\\MQL5{TemplateName}";
                    File.WriteAllLines($"{MT5Path}\\MQL5{TemplateName}", TemplateStringContent);
                    _mtApiClient.ChartApplyTemplate(ChartId, TemplateName);
                }
                return ChartId;
            });

            if (result == -1)
            {
                AddLog("ExecuteChartApplyTemplate: result is null");
                return;
            }

            AddLog($"ExecuteChartApplyTemplate: success chartid=>{result}");
        }

        private async void ExecuteChartSaveTemplate(object o)
        {

            AddLog("ExecuteSaveApplyTemplate #1");


            var result = await Execute(() =>
            {

                var MT5Path = _mtApiClient.TerminalInfoString(ENUM_TERMINAL_INFO_STRING.TERMINAL_DATA_PATH);
                int ChartId = 0;  // Actual Chart
                var TemplateName = "\\Files\\exported.tpl";
                _mtApiClient.ChartSaveTemplate(ChartId, TemplateName);
                var DestPath = $"{MT5Path}\\MQL5{TemplateName}";
                AddLog($"Destination: {TemplateName}");
                return ChartId;
            });

            if (result == -1)
            {
                AddLog("ChartOpen: result is null");
                return;
            }

            AddLog($"ChartOpen: success chartid=>{result}");
        }

        private async void ExecuteChartId(object o)
        {
            var result = await Execute(() => _mtApiClient.ChartId());
            RunOnUiThread(() =>
            {
                ChartFunctionsChartIdValue = result;
            });
            AddLog($"ChartId: chartid = {result}");
        }

        private void ExecuteChartRedraw(object o)
        {
            var chartId = ChartFunctionsChartIdValue;
            _mtApiClient.ChartRedraw(chartId);
            AddLog($"ChartRedraw: executed for chartid = {chartId}");
        }

        private async void ExecuteChartWindowFind(object o)
        {
            const string shortname = "MACD(12,26,9)";
            var chartId = ChartFunctionsChartIdValue;
            var result = await Execute(() => _mtApiClient.ChartWindowFind(chartId, shortname));
            AddLog($"ChartRedraw: result {result} for chartid {chartId}");
        }

        private async void ExecuteChartClose(object o)
        {
            var chartId = ChartFunctionsChartIdValue;
            var result = await Execute(() => _mtApiClient.ChartClose(chartId));
            AddLog($"ChartClose: result {result} for chartid {chartId}");
        }

        private async void ExecuteChartPeriod(object o)
        {
            var chartId = ChartFunctionsChartIdValue;
            var result = await Execute(() => _mtApiClient.ChartPeriod(chartId));
            AddLog($"ChartPeriod: result {result} for chartid {chartId}");
        }

        private async void ExecuteChartSetDouble(object o)
        {
            var chartId = ChartFunctionsChartIdValue;
            var result = await Execute(() => _mtApiClient.ChartSetDouble(chartId, ENUM_CHART_PROPERTY_DOUBLE.CHART_PRICE_MAX, 1.13));
            AddLog($"ChartSetDouble: result {result} for chartid {chartId}");
        }

        private async void ExecuteChartSetInteger(object o)
        {
            var chartId = ChartFunctionsChartIdValue;
            var result = await Execute(() => _mtApiClient.ChartSetInteger(chartId, ENUM_CHART_PROPERTY_INTEGER.CHART_SHOW_GRID, 0));
            AddLog($"ChartSetInteger: result {result} for chartid {chartId}");
        }

        private async void ExecuteChartSetString(object o)
        {
            var chartId = ChartFunctionsChartIdValue;
            var result = await Execute(() => _mtApiClient.ChartSetString(chartId, ENUM_CHART_PROPERTY_STRING.CHART_COMMENT, "This is test comment from MtApi5"));
            AddLog($"ChartSetString: result {result} for chartid {chartId}");
        }

        private async void ExecuteChartGetDouble(object o)
        {
            var chartId = ChartFunctionsChartIdValue;
            var result = await Execute(() => _mtApiClient.ChartGetDouble(chartId, ENUM_CHART_PROPERTY_DOUBLE.CHART_PRICE_MAX, 0));
            AddLog($"ChartGetDouble: result {result} for chartid {chartId}");
        }

        private async void ExecuteChartGetInteger(object o)
        {
            var chartId = ChartFunctionsChartIdValue;
            var result = await Execute(() => _mtApiClient.ChartGetInteger(chartId, ENUM_CHART_PROPERTY_INTEGER.CHART_VISIBLE_BARS, 0));
            AddLog($"ChartGetInteger: result {result} for chartid {chartId}");
        }

        private async void ExecuteChartNavigate(object o)
        {
            var chartId = ChartFunctionsChartIdValue;
            var result = await Execute(() => _mtApiClient.ChartNavigate(chartId, ENUM_CHART_POSITION.CHART_BEGIN, 0));
            AddLog($"ChartNavigate: result {result} for chartid {chartId}");
        }

        private async void ExecuteChartIndicatorAdd(object obj)
        {
            var chartId = ChartFunctionsChartIdValue;
            var symbol = ChartFunctionsSymbolValue;

            var indicatorHandle = await Execute(() => _mtApiClient.iMACD(symbol, ENUM_TIMEFRAMES.PERIOD_CURRENT, 12, 26, 9, ENUM_APPLIED_PRICE.PRICE_CLOSE));
            var result = await Execute(() => _mtApiClient.ChartIndicatorAdd(chartId, 1, indicatorHandle));
            AddLog($"ChartIndicatorAdd: result {result} for chartid {chartId} with indicator {indicatorHandle}");
        }

        private async void ExecuteChartIndicatorDelete(object o)
        {
            const string shortname = "MACD(12,26,9)";
            var chartId = ChartFunctionsChartIdValue;
            var result = await Execute(() => _mtApiClient.ChartIndicatorDelete(chartId, 1, shortname));
            AddLog($"ChartIndicatorDelete: result {result} for chartid {chartId}");
        }

        private async void ExecuteChartIndicatorGet(object obj)
        {
            const string shortname = "MACD(12,26,9)";
            var chartId = ChartFunctionsChartIdValue;
            var result = await Execute(() => _mtApiClient.ChartIndicatorGet(chartId, 1, shortname));
            AddLog($"hartIndicatorGet: result {result} for chartid {chartId}");
        }

        private async void ExecuteChartIndicatorName(object obj)
        {
            var chartId = ChartFunctionsChartIdValue;
            var result = await Execute(() => _mtApiClient.ChartIndicatorName(chartId, 1, 0));
            AddLog($"ChartIndicatorName: result {result} for chartid {chartId}");
        }

        private async void ExecuteChartIndicatorsTotal(object obj)
        {
            var chartId = ChartFunctionsChartIdValue;
            var result = await Execute(() => _mtApiClient.ChartIndicatorsTotal(chartId, 1));
            AddLog($"ChartIndicatorsTotal: result {result} for chartid {chartId}");
        }

        private async void ExecuteChartWindowOnDropped(object obj)
        {
            var result = await Execute(() => _mtApiClient.ChartWindowOnDropped());
            AddLog($"ChartWindowOnDropped: result {result}");
        }

        private async void ExecuteChartPriceOnDropped(object obj)
        {
            var result = await Execute(() => _mtApiClient.ChartPriceOnDropped());
            AddLog($"ChartPriceOnDropped: result {result}");
        }

        private async void ExecuteChartTimeOnDropped(object obj)
        {
            var result = await Execute(() => _mtApiClient.ChartTimeOnDropped());
            AddLog($"ChartTimeOnDropped: result {result}");
        }

        private async void ExecuteChartXOnDropped(object obj)
        {
            var result = await Execute(() => _mtApiClient.ChartXOnDropped());
            AddLog($"ChartXOnDropped: result {result}");
        }

        private async void ExecuteChartYOnDropped(object obj)
        {
            var result = await Execute(() => _mtApiClient.ChartYOnDropped());
            AddLog($"ChartYOnDropped: result {result}");
        }

        private async void ExecuteChartSetSymbolPeriod(object obj)
        {
            var chartId = ChartFunctionsChartIdValue;
            var symbol = ChartFunctionsSymbolValue;
            var result = await Execute(() => _mtApiClient.ChartSetSymbolPeriod(chartId, symbol, ENUM_TIMEFRAMES.PERIOD_M5));
            AddLog($"ChartSetSymbolPeriod: result {result} for chartid {chartId}");
        }

        private async void ExecuteChartScreenShot(object obj)
        {
            var chartId = ChartFunctionsChartIdValue;
            var filename = "ChartScreenShot_TestMtApi.gif";
            const int width = 800;
            const int height = 600;
            var result = await Execute(() => _mtApiClient.ChartScreenShot(chartId, filename, width, height));
            AddLog($"ChartScreenShot: result {result} for chartid {chartId}. Filename {filename}");
        }
        #endregion

        private static void RunOnUiThread(Action action)
        {
            Application.Current?.Dispatcher.Invoke(action);
        }

        private static void RunOnUiThread<T>(Action<T> action, params object[] args)
        {
            Application.Current?.Dispatcher.Invoke(action, args);
        }

        private void mMtApiClient_QuoteUpdate(object sender, Mt5QuoteEventArgs e)
        {
            var q = e.Quote;

            Console.WriteLine(@"Quote: Symbol = {0}, Bid = {1}, Ask = {2}, Volume = {3}, Time = {4}, Last = {5}"
                , q.Instrument, q.Bid, q.Ask, q.Volume, q.Time, q.Last);

            if (_quotesMap.ContainsKey(e.Quote.ExpertHandle))
            {
                var qvm = _quotesMap[e.Quote.ExpertHandle];
                qvm.Bid = e.Quote.Bid;
                qvm.Ask = e.Quote.Ask;
            }

            if (string.Equals(e.Quote.Instrument, TradeRequest.Symbol))
            {
                if (TradeRequest.Type == ENUM_ORDER_TYPE.ORDER_TYPE_BUY)
                {
                    TradeRequest.Price = e.Quote.Ask;
                }
                else if (TradeRequest.Type == ENUM_ORDER_TYPE.ORDER_TYPE_SELL)
                {
                    TradeRequest.Price = e.Quote.Bid;
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

        private void mMtApiClient_OnTradeTransaction(object sender, Mt5TradeTransactionEventArgs e)
        {
            AddLog($"OnTradeTransaction: ExpertHandle = {e.ExpertHandle}.{Environment.NewLine}Transaction = {e.Trans}.{Environment.NewLine}Request = {e.Request}.{Environment.NewLine}Result = {e.Result}.");
        }

        private void _mtApiClient_OnBookEvent(object sender, Mt5BookEventArgs e)
        {
            AddLog($"OnBookEvent: ExpertHandle = {e.ExpertHandle}, Symbol = {e.Symbol}");
        }

        private void AddQuote(Mt5Quote quote)
        {
            if (_quotesMap.ContainsKey(quote.ExpertHandle))
            {
                AddLog($"AddQuote: Quote {quote.Instrument} with handle {quote.ExpertHandle} is present list. Skipped!");
                return;
            }

            var qvm = new QuoteViewModel(quote.Instrument)
            {
                ExpertHandle = quote.ExpertHandle,
                Bid = quote.Bid,
                Ask = quote.Ask
            };

            _quotesMap[quote.ExpertHandle] = qvm;
            Quotes.Add(qvm);
        }

        private void RemoveQuote(Mt5Quote quote)
        {
            if (_quotesMap.ContainsKey(quote.ExpertHandle) == false)
            {
                AddLog($"RemoveQuote: Quote {quote.Instrument} with handle {quote.ExpertHandle} is NOT present list. Skipped!");
                return;
            }

            var qvm = _quotesMap[quote.ExpertHandle];
            _quotesMap.Remove(quote.ExpertHandle);
            Quotes.Remove(qvm);
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
                catch (ExecutionException ex)
                {
                    AddLog($"Exception: {ex.ErrorCode} - {ex.Message}");
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

        private readonly Dictionary<int, QuoteViewModel> _quotesMap = new Dictionary<int, QuoteViewModel>();
        #endregion
    }
}
