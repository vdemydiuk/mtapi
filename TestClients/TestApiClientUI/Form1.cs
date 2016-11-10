using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using MtApi;
using System.Threading.Tasks;
using System.Linq;
using MtApi.Monitors;

namespace TestApiClientUI
{
    public partial class Form1 : Form
    {
        #region Fields

        private readonly List<Action> _groupOrderCommands = new List<Action>();
        private readonly MtApiClient _apiClient = new MtApiClient();
        private readonly TimerTradeMonitor _timerTradeMonitor;
        private readonly TimeframeTradeMonitor _timeframeTradeMonitor;

        private volatile bool _isUiQuoteUpdateReady = true;

        #endregion

        public Form1()
        {
            InitializeComponent();

            comboBox3.DataSource = Enum.GetNames(typeof(ENUM_TIMEFRAMES));

            _apiClient.QuoteUpdated += apiClient_QuoteUpdated;
            _apiClient.QuoteAdded += apiClient_QuoteAdded;
            _apiClient.QuoteRemoved += apiClient_QuoteRemoved;
            _apiClient.ConnectionStateChanged += apiClient_ConnectionStateChanged;
            _apiClient.OnLastTimeBar += _apiClient_OnLastTimeBar;

            InitOrderCommandsGroup();

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;

            _timerTradeMonitor = new TimerTradeMonitor(_apiClient) { Interval = 10000 }; // 10 sec
            _timerTradeMonitor.AvailabilityOrdersChanged += _tradeMonitor_AvailabilityOrdersChanged;

            _timeframeTradeMonitor = new TimeframeTradeMonitor(_apiClient);
            _timeframeTradeMonitor.AvailabilityOrdersChanged += _tradeMonitor_AvailabilityOrdersChanged;
        }

        private void InitOrderCommandsGroup()
        {
            _groupOrderCommands.Add(CloseOrders);
            _groupOrderCommands.Add(CloseOrdersBy);
            _groupOrderCommands.Add(OrderClosePrice);
            _groupOrderCommands.Add(OrderCloseTime);
            _groupOrderCommands.Add(OrderComment);
            _groupOrderCommands.Add(OrderCommission);
            _groupOrderCommands.Add(OrderDelete);
            _groupOrderCommands.Add(OrderExpiration);
            _groupOrderCommands.Add(OrderLots);
            _groupOrderCommands.Add(OrderMagicNumber);
            _groupOrderCommands.Add(OrderModify);
            _groupOrderCommands.Add(OrderOpenPrice);
            _groupOrderCommands.Add(OrderOpenTime);
            _groupOrderCommands.Add(OrderPrint);
            _groupOrderCommands.Add(OrderProfit);
            _groupOrderCommands.Add(OrderSelect);
            _groupOrderCommands.Add(OrdersHistoryTotal);
            _groupOrderCommands.Add(OrderStopLoss);
            _groupOrderCommands.Add(OrdersTotal);
            _groupOrderCommands.Add(OrderSwap);
            _groupOrderCommands.Add(OrderSymbol);
            _groupOrderCommands.Add(OrderTakeProfit);
            _groupOrderCommands.Add(OrderTicket);
            _groupOrderCommands.Add(OrderType);
        }

        private void RunOnUiThread(Action action)
        {
            if (!IsDisposed)
            {
                BeginInvoke(action);
            }
        }

        private void apiClient_ConnectionStateChanged(object sender, MtConnectionEventArgs e)
        {
            RunOnUiThread(() =>
            {
                toolStripStatusConnection.Text = e.Status.ToString();
            });

            switch (e.Status)
            {
                case MtConnectionState.Connected:
                    RunOnUiThread(OnConnected);
                    break;
                case MtConnectionState.Disconnected:
                case MtConnectionState.Failed:
                    RunOnUiThread(OnDisconnected);
                    break;
            }
        }

        private void apiClient_QuoteRemoved(object sender, MtQuoteEventArgs e)
        {
            RunOnUiThread(() => RemoveQuote(e.Quote) );
        }

        private void apiClient_QuoteAdded(object sender, MtQuoteEventArgs e)
        {
            RunOnUiThread(() => AddNewQuote(e.Quote));
        }

        private void _apiClient_OnLastTimeBar(object sender, TimeBarArgs e)
        {
            var msg =
                $"TimeBar: Symbol = {e.TimeBar.Symbol}, OpenTime = {e.TimeBar.OpenTime}, CloseTime = {e.TimeBar.CloseTime}, Open = {e.TimeBar.Open}, Close = {e.TimeBar.Close}, High = {e.TimeBar.High}, Low = {e.TimeBar.Low}";
            Console.WriteLine(msg);
            AddToLog(msg);
        }

        private void apiClient_QuoteUpdated(object sender, string symbol, double bid, double ask)
        {
            Console.WriteLine(@"Quote: Symbol = {0}, Bid = {1}, Ask = {2}", symbol, bid, ask);
            //if UI of quite is busy we are skipping this update
            if (_isUiQuoteUpdateReady)
            {
                RunOnUiThread(() => ChangeQuote(symbol, bid, ask));
            }
        }

        private void AddNewQuote(MtQuote quote)
        {
            if (quote == null)
                return;

            if (string.IsNullOrEmpty(quote.Instrument) == false
                && listViewQuotes.Items.ContainsKey(quote.Instrument) == false)
            {
                var item = new ListViewItem(quote.Instrument) { Name = quote.Instrument };
                item.SubItems.Add(quote.Bid.ToString(CultureInfo.CurrentCulture));
                item.SubItems.Add(quote.Ask.ToString(CultureInfo.CurrentCulture));
                item.SubItems.Add("1");
                listViewQuotes.Items.Add(item);
            }
            else
            {
                var item = listViewQuotes.Items[quote.Instrument];
                int feedCount;
                int.TryParse(item.SubItems[3].Text, out feedCount);
                feedCount++;
                item.SubItems[3].Text = feedCount.ToString();
            }
        }

        private void RemoveQuote(MtQuote quote)
        {
            if (quote == null)
                return;

            if (string.IsNullOrEmpty(quote.Instrument) == false
                && listViewQuotes.Items.ContainsKey(quote.Instrument))
            {
                var item = listViewQuotes.Items[quote.Instrument];
                int feedCount;
                int.TryParse(item.SubItems[3].Text, out feedCount);
                feedCount--;
                if (feedCount <= 0)
                {
                    listViewQuotes.Items.RemoveByKey(quote.Instrument);
                }
                else
                {
                    item.SubItems[3].Text = feedCount.ToString();
                }
            }
        }

        private void ChangeQuote(string symbol, double bid, double ask)
        {
            _isUiQuoteUpdateReady = false;

            if (string.IsNullOrEmpty(symbol) == false)
            {
                if (listViewQuotes.Items.ContainsKey(symbol))
                {
                    var item = listViewQuotes.Items[symbol];
                    item.SubItems[1].Text = bid.ToString(CultureInfo.CurrentCulture);
                    item.SubItems[2].Text = ask.ToString(CultureInfo.CurrentCulture);
                }
            }

            _isUiQuoteUpdateReady = true;
        }

        private void OnConnected()
        {
            var quotes = _apiClient.GetQuotes();

            if (quotes != null)
            {
                foreach (var quote in quotes)
                {
                    AddNewQuote(quote);
                }                    
            }            
        }

        private void OnDisconnected()
        {
            listViewQuotes.Items.Clear();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            var serverName = textBoxServerName.Text;

            int port;
            int.TryParse(textBoxPort.Text, out port);

            _timerTradeMonitor.Start();
            _timeframeTradeMonitor.Start();

            if (string.IsNullOrEmpty(serverName))
                _apiClient.BeginConnect(port);
            else
                _apiClient.BeginConnect(serverName, port);
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            _timerTradeMonitor.Stop();
            _timeframeTradeMonitor.Stop();

            _apiClient.BeginDisconnect();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _apiClient.BeginDisconnect();
        }

        private void listViewQuotes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewQuotes.SelectedItems.Count > 0)
            {
                textBoxOrderSymbol.Text = listViewQuotes.SelectedItems[0].Text;
                txtMarketInfoSymbol.Text = listViewQuotes.SelectedItems[0].Text;
                textBoxSelectedSymbol.Text = listViewQuotes.SelectedItems[0].Text;
            }
        }

        private void CloseOrders()
        {
            if (listBoxSendedOrders.SelectedItems.Count > 0)
            {
                double volume;
                double.TryParse(textBoxOrderVolume.Text, out volume);

                double price;
                double.TryParse(textBoxOrderPrice.Text, out price);

                var slippage = (int)numericOrderSlippage.Value;

                var sendedOrders = new List<int>();

                foreach (var item in listBoxSendedOrders.SelectedItems)
                {
                    var orderId = (int)item;
                    var result = _apiClient.OrderClose(orderId, volume, price, slippage);

                    if (result)
                    {
                        sendedOrders.Add(orderId);
                    }

                    AddToLog($"Closed order result: {result},  ticketId = {orderId}, volume - {volume}, slippage {slippage}");
                }

                foreach (var orderId in sendedOrders)
                {
                    listBoxClosedOrders.Items.Add(orderId);
                    listBoxSendedOrders.Items.Remove(orderId);
                }
            }
        }

        private void CloseOrdersBy()
        {
            if (listBoxSendedOrders.SelectedItems.Count >= 1)
            {
                var ticket = int.Parse(textBoxIndexTicket.Text);
                var opposite = (int)listBoxSendedOrders.SelectedItems[0];

                Color color;
                switch (comboBoxOrderColor.SelectedIndex)
                {
                    case 0:
                        color = Color.Green;
                        break;
                    case 1:
                        color = Color.Blue;
                        break;
                    case 2:
                        color = Color.Red;
                        break;
                    default:
                        return;
                }

                var result = _apiClient.OrderCloseBy(ticket, opposite, color);

                AddToLog($"ClosedBy order result: {result},  ticketId = {ticket}, opposite {opposite}");
            }
        }

        private void OrderClosePrice()
        {
            var result = _apiClient.OrderClosePrice();
            textBoxOrderPrice.Text = result.ToString(CultureInfo.CurrentCulture);
            AddToLog($"OrderClosePrice result: {result}");
        }

        private void OrderCloseTime()
        {
            var result = _apiClient.OrderCloseTime();
            AddToLog($"OrderCloseTime result: {result}");
        }

        private void OrderComment()
        {
            var result = _apiClient.OrderComment();
            textBoxOrderComment.Text = result;
            AddToLog($"OrderComment result: {result}");
        }

        private void OrderCommission()
        {
            var result = _apiClient.OrderCommission();
            AddToLog($"OrderCommission result: {result}");
        }

        private void OrderDelete()
        {
            if (listBoxSendedOrders.SelectedItems.Count >= 1)
            {
                var ticket = (int)listBoxSendedOrders.SelectedItems[0];
                var result = _apiClient.OrderDelete(ticket);
                AddToLog($"Delete order result: {result},  ticketId = {ticket}");
            }
        }

        private void OrderExpiration()
        {
            var result = _apiClient.OrderExpiration();
            AddToLog($"Expiration order result: {result}");
        }

        private void OrderLots()
        {
            var result = _apiClient.OrderLots();
            AddToLog($"Lots order result: {result}");
        }

        private void OrderMagicNumber()
        {
            var result = _apiClient.OrderMagicNumber();
            AddToLog($"MagicNumber order result: {result}");
        }

        private void OrderModify()
        {
            if (listBoxSendedOrders.SelectedItems.Count >= 1)
            {
                var ticket = (int)listBoxSendedOrders.SelectedItems[0];

                double price;
                double.TryParse(textBoxOrderPrice.Text, out price);

                double stoploss;
                double.TryParse(textBoxOrderStoploss.Text, out stoploss);

                double takeprofit;
                double.TryParse(textBoxOrderProffit.Text, out takeprofit);

                var expiration = DateTime.Now;

                Color arrowColor;
                switch (comboBoxOrderColor.SelectedIndex)
                {
                    case 0:
                        arrowColor = Color.Green;
                        break;
                    case 1:
                        arrowColor = Color.Blue;
                        break;
                    case 2:
                        arrowColor = Color.Red;
                        break;
                    default:
                        return;
                }

                var result = _apiClient.OrderModify(ticket, price, stoploss, takeprofit, expiration, arrowColor);

                AddToLog($"OrderModify result: {result}");
            }
        }

        private void OrderOpenPrice()
        {
            var result = _apiClient.OrderOpenPrice();
            AddToLog($"OrderOpenPrice result: {result}");
        }

        private void OrderOpenTime()
        {
            var result = _apiClient.OrderOpenTime();
            AddToLog($"OpenTime order result: {result}");
        }

        private void OrderPrint()
        {
            _apiClient.OrderPrint();
        }

        private void OrderProfit()
        {
            var result = _apiClient.OrderProfit();
            AddToLog($"Profit order result: {result}");
        }

        private void OrderSelect()
        {
            var ticket = -1;

            if (string.IsNullOrEmpty(textBoxIndexTicket.Text) == false)
                ticket = int.Parse(textBoxIndexTicket.Text);
            else if (listBoxSendedOrders.SelectedItems.Count > 0)
                ticket = (int)listBoxSendedOrders.SelectedItems[0];
            else if (listBoxClosedOrders.SelectedItems.Count > 0)
                ticket = (int)listBoxClosedOrders.SelectedItems[0];
            
            if (ticket >= 0)
            {
                var result = _apiClient.OrderSelect(ticket, OrderSelectMode.SELECT_BY_POS);

                AddToLog($"OrderSelect result: {result}");
            }
        }

        private void OrdersHistoryTotal()
        {
            var result = _apiClient.OrdersHistoryTotal();
            AddToLog($"OrdersHistoryTotal result: {result}");
        }

        private void OrderStopLoss()
        {
            var result = _apiClient.OrderStopLoss();
            textBoxOrderStoploss.Text = result.ToString(CultureInfo.CurrentCulture);
            AddToLog($"OrderStopLoss result: {result}");
        }

        private void OrdersTotal()
        {
            var result = _apiClient.OrdersTotal();
            AddToLog($"OrdersTotal result: {result}");
        }

        private void OrderSwap()
        {
            var result = _apiClient.OrderSwap();
            AddToLog($"OrderSwap result: {result}");
        }

        private void OrderSymbol()
        {
            var result = _apiClient.OrderSymbol();
            textBoxOrderSymbol.Text = result;
            AddToLog($"OrderSymbol result: {result}");
        }

        private void OrderTakeProfit()
        {
            var result = _apiClient.OrderTakeProfit();
            textBoxOrderProffit.Text = result.ToString(CultureInfo.CurrentCulture);
            AddToLog($"OrderTakeProfit result: {result}");
        }

        private void OrderTicket()
        {
            var result = _apiClient.OrderTicket();
            AddToLog($"OrderTicket result: {result}");
        }

        private void OrderType()
        {
            var result = _apiClient.OrderType();
            comboBoxOrderCommand.SelectedIndex = (int)result;
            AddToLog($"OrderType result: {result}");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _groupOrderCommands[comboBoxSelectedCommand.SelectedIndex]();
        }

        private void AddToLog(string msg)
        {
            RunOnUiThread(() =>
            {
                var time = DateTime.Now.ToString("h:mm:ss tt");
                listBoxEventLog.Items.Add($"[{time}]: {msg}");
                listBoxEventLog.SetSelected(listBoxEventLog.Items.Count - 1, true);
                listBoxEventLog.SetSelected(listBoxEventLog.Items.Count - 1, false);
            });
        }

        private void button3_Click(object sender, EventArgs e)
        {
            switch (comboBoxStatusCommand.SelectedIndex)
            {
                case 0:
                    {
                        var result = _apiClient.GetLastError();
                        AddToLog($"GetLastError result: {result}");
                    }
                    break;
                case 1:
                    {
                        var result = _apiClient.IsConnected();
                        AddToLog($"IsConnected result: {result}");
                    }
                    break;
                case 2:
                    {
                        var result = _apiClient.IsDemo();
                        AddToLog($"IsDemo result: {result}");
                    }
                    break;
                case 3:
                    {
                        var result = _apiClient.IsDllsAllowed();
                        AddToLog($"IsDllsAllowed result: {result}");
                    }
                    break;
                case 4:
                    {
                        var result = _apiClient.IsExpertEnabled();
                        AddToLog($"IsExpertEnabled result: {result}");
                    }
                    break;
                case 5:
                    {
                        var result = _apiClient.IsLibrariesAllowed();
                        AddToLog($"IsLibrariesAllowed result: {result}");
                    }
                    break;
                case 6:
                    {
                        var result = _apiClient.IsOptimization();
                        AddToLog($"IsOptimization result: {result}");
                    }
                    break;
                case 7:
                    {
                        var result = _apiClient.IsStopped();
                        AddToLog($"IsStopped result: {result}");
                    }
                    break;
                case 8:
                    {
                        var result = _apiClient.IsTesting();
                        AddToLog($"IsTesting result: {result}");
                    }
                    break;
                case 9:
                    {
                        var result = _apiClient.IsTradeAllowed();
                        AddToLog($"IsTradeAllowed result: {result}");
                    }
                    break;
                case 10:
                    {
                        var result = _apiClient.IsTradeContextBusy();
                        AddToLog($"IsTradeContextBusy result: {result}");
                    }
                    break;
                case 11:
                    {
                        var result = _apiClient.IsVisualMode();
                        AddToLog($"IsVisualMode result: {result}");
                    }
                    break;
                case 12:
                    {
                        var result = _apiClient.UninitializeReason();
                        AddToLog($"UninitializeReason result: {result}");
                    }
                    break;
                case 13:
                    {
                        int errorCode;
                        int.TryParse(textBoxErrorCode.Text, out errorCode);
                        var result = _apiClient.ErrorDescription(errorCode);
                        AddToLog($"ErrorDescription result: {result}");
                    }
                    break;
                default:
                    return;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            switch (listBoxAccountInfo.SelectedIndex)
            {
                case 0:
                    {
                        var result = _apiClient.AccountBalance();
                        AddToLog($"AccountBalance result: {result}");
                    }
                    break;
                case 1:
                    {
                        var result = _apiClient.AccountCredit();
                        AddToLog($"AccountCredit result: {result}");
                    }
                    break;
                case 2:
                    {
                        var result = _apiClient.AccountCompany();
                        AddToLog($"AccountCompany result: {result}");
                    }
                    break;
                case 3:
                    {
                        var result = _apiClient.AccountCurrency();
                        AddToLog($"AccountCurrency result: {result}");
                    }
                    break;
                case 4:
                    {
                        var result = _apiClient.AccountEquity();
                        AddToLog($"AccountEquity result: {result}");
                    }
                    break;
                case 5:
                    {
                        var result = _apiClient.AccountFreeMargin();
                        AddToLog($"AccountFreeMargin result: {result}");
                    }
                    break;
                case 6:
                    {
                        var result = _apiClient.AccountFreeMarginCheck(textBoxAccountInfoSymbol.Text, (TradeOperation)comboBoxAccountInfoCmd.SelectedIndex, int.Parse(textBoxAccountInfoVolume.Text));
                        AddToLog($"AccountFreeMarginCheck result: {result}");
                    }
                    break;
                case 7:
                    {
                        var result = _apiClient.AccountFreeMarginMode();
                        AddToLog($"AccountFreeMarginMode result: {result}");
                    }
                    break;
                case 8:
                    {
                        var result = _apiClient.AccountLeverage();
                        AddToLog($"AccountLeverage result: {result}");
                    }
                    break;
                case 9:
                    {
                        var result = _apiClient.AccountMargin();
                        AddToLog($"AccountMargin result: {result}");
                    }
                    break;
                case 10:
                    {
                        var result = _apiClient.AccountName();
                        AddToLog($"AccountName result: {result}");
                    }
                    break;
                case 11:
                    {
                        var result = _apiClient.AccountNumber();
                        AddToLog($"AccountNumber result: {result}");
                    }
                    break;
                case 12:
                    {
                        var result = _apiClient.AccountProfit();
                        AddToLog($"AccountProfit result: {result}");
                    }
                    break;
                case 13:
                    {
                        var result = _apiClient.AccountServer();
                        AddToLog($"AccountServer result: {result}");
                    }
                    break;
                case 14:
                    {
                        var result = _apiClient.AccountStopoutLevel();
                        AddToLog($"AccountStopoutLevel result: {result}");
                    }
                    break;
                case 15:
                    {
                        var result = _apiClient.AccountStopoutMode();
                        AddToLog($"AccountStopoutMode result: {result}");
                    }
                    break;
                default:
                    return;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (listBoxMarketInfo.SelectedIndex < 0)
                return;

            var result = _apiClient.MarketInfo(txtMarketInfoSymbol.Text, (MarketInfoModeType)listBoxMarketInfo.SelectedIndex);
            AddToLog($"MarketInfo result: {result}");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var symbol = textBoxSelectedSymbol.Text;
            int timeframeCount;
            int.TryParse(textBoxTimeframesCount.Text, out timeframeCount);

            Console.WriteLine($"Started time: {DateTime.Now}");

            var prices = _apiClient.iCloseArray(symbol, ChartPeriod.PERIOD_M1);

            var openPriceList = new List<double>(prices);

            listBoxProceHistory.DataSource = openPriceList;

            Console.WriteLine($"Finished time: {DateTime.Now}");

            using (var file = new System.IO.StreamWriter($@"{System.IO.Path.GetTempPath()}\MtApi\test.txt"))
            {
                foreach (var value in openPriceList)
                {
                    file.WriteLine(value);
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            var symbol = textBoxSelectedSymbol.Text;
            var barCount = _apiClient.iBars(symbol, ChartPeriod.PERIOD_M1);
            textBoxTimeframesCount.Text = barCount.ToString();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            var symbol = textBoxSelectedSymbol.Text;
            var prices = _apiClient.iHighArray(symbol, ChartPeriod.PERIOD_M1);
            var items = new List<double>(prices);
            listBoxProceHistory.DataSource = items;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            var symbol = textBoxSelectedSymbol.Text;
            var prices = _apiClient.iLowArray(symbol, ChartPeriod.PERIOD_M1);
            var items = new List<double>(prices);
            listBoxProceHistory.DataSource = items;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            var symbol = textBoxSelectedSymbol.Text;
            var prices = _apiClient.iOpenArray(symbol, ChartPeriod.PERIOD_M1);
            var items = new List<double>(prices);
            listBoxProceHistory.DataSource = items;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            string symbol = textBoxSelectedSymbol.Text;
            var prices = _apiClient.iVolumeArray(symbol, ChartPeriod.PERIOD_M1);
            var items = new List<double>(prices);
            listBoxProceHistory.DataSource = items;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            string symbol = textBoxSelectedSymbol.Text;
            var times = _apiClient.iTimeArray(symbol, ChartPeriod.PERIOD_M1);
            var items = new List<DateTime>(times);
            listBoxProceHistory.DataSource = items;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            var retVal = _apiClient.TimeCurrent();
            AddToLog($"TimeCurrent result: {retVal}");
        }

        private void button14_Click(object sender, EventArgs e)
        {
            var retVal = _apiClient.TimeLocal();
            AddToLog($"TimeLocal result: {retVal}");
        }

        private void buttonRefreshRates_Click(object sender, EventArgs e)
        {
            var retVal = _apiClient.RefreshRates();
            AddToLog($"RefreshRates result: {retVal}");
        }

        private void listBoxEventLog_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            listBoxEventLog.Items.Clear();
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
                catch (MtConnectionException ex)
                {
                    AddToLog("MtExecutionException: " + ex.Message);
                }
                catch (MtExecutionException ex)
                {
                    AddToLog("MtExecutionException: " + ex.Message + "; ErrorCode = " + ex.ErrorCode);
                }

                return result;
            });
        }

        //OrderSend
        private async void button1_Click(object sender, EventArgs e)
        {
            var symbol = textBoxOrderSymbol.Text;

            var cmd = (TradeOperation) comboBoxOrderCommand.SelectedIndex;

            double volume;
            double.TryParse(textBoxOrderVolume.Text, out volume);

            double price;
            double.TryParse(textBoxOrderPrice.Text, out price);

            var slippage = (int) numericOrderSlippage.Value;

            double stoploss;
            double.TryParse(textBoxOrderStoploss.Text, out stoploss);

            double takeprofit;
            double.TryParse(textBoxOrderProffit.Text, out takeprofit);

            var comment = textBoxOrderComment.Text;

            int magic;
            int.TryParse(textBoxOrderMagic.Text, out magic);

            var expiration = DateTime.Now;

            Color arrowColor;
            switch (comboBoxOrderColor.SelectedIndex)
            {
                case 0:
                    arrowColor = Color.Green;
                    break;
                case 1:
                    arrowColor = Color.Blue;
                    break;
                case 2:
                    arrowColor = Color.Red;
                    break;
                default:
                    return;
            }

            var ticket = await Execute(() => _apiClient.OrderSend(symbol, cmd, volume, price, slippage, stoploss, takeprofit, comment, magic, expiration, arrowColor));

            AddToLog($"Sended order result: ticket = {ticket}");
        }

        //GetOrder
        private async void button16_Click(object sender, EventArgs e)
        {
            var ticket = int.Parse(textBoxIndexTicket.Text);
            var selectMode = (OrderSelectMode) comboBox1.SelectedIndex;
            var selectSource = (OrderSelectSource) comboBox2.SelectedIndex;

            var order = await Execute(() => _apiClient.GetOrder(ticket, selectMode, selectSource));

            if (order != null)
            {
                var result =
                    $"Order: Ticket = {order.Ticket}, Symbol = {order.Symbol}, Operation = {order.Operation}, OpenPrice = {order.OpenPrice}, ClosePrice = {order.ClosePrice}, Lots = {order.Lots}, Profit = {order.Profit}, Comment = {order.Comment}, Commission = {order.Commission}, MagicNumber = {order.MagicNumber}, OpenTime = {order.OpenTime}, CloseTime = {order.CloseTime}, Swap = {order.Swap}, Expiration = {order.Expiration}, TakeProfit = {order.TakeProfit}, StopLoss = {order.StopLoss}";
                AddToLog(result);
            }
        }

        //GetOrders
        private async void button17_Click(object sender, EventArgs e)
        {
            var selectSource = (OrderSelectSource)comboBox2.SelectedIndex;


            var orders = await Execute(() => _apiClient.GetOrders(selectSource));

            if (orders != null && orders.Any())
            {
                foreach (var order in orders)
                {
                    var result =
                        $"Order: Ticket = {order.Ticket}, Symbol = {order.Symbol}, Operation = {order.Operation}, OpenPrice = {order.OpenPrice}, ClosePrice = {order.ClosePrice}, Lots = {order.Lots}, Profit = {order.Profit}, Comment = {order.Comment}, Commission = {order.Commission}, MagicNumber = {order.MagicNumber}, OpenTime = {order.OpenTime}, CloseTime = {order.CloseTime}, Swap = {order.Swap}, Expiration = {order.Expiration}, TakeProfit = {order.TakeProfit}, StopLoss = {order.StopLoss}";
                    AddToLog(result);
                }
            }
            else
            {
                AddToLog("GetOrders: 0 orders");
            }
        }

        //OrderSendBuy
        private async void button18_Click(object sender, EventArgs e)
        {
            var symbol = textBoxOrderSymbol.Text;

            double volume;
            double.TryParse(textBoxOrderVolume.Text, out volume);

            var slippage = (int)numericOrderSlippage.Value;

            var ticket = await Execute(() => _apiClient.OrderSendBuy(symbol, volume, slippage));

            AddToLog(
                $"Sended order result: ticketId = {ticket}, symbol = {symbol}, volume = {volume}, slippage = {slippage}");
        }

        //OrderSendSell
        private async void button19_Click(object sender, EventArgs e)
        {
            var symbol = textBoxOrderSymbol.Text;

            double volume;
            double.TryParse(textBoxOrderVolume.Text, out volume);

            var slippage = (int)numericOrderSlippage.Value;

            var ticket = await Execute(() => _apiClient.OrderSendSell(symbol, volume, slippage));

            AddToLog($"Sended order result: ticketId = {ticket}, symbol = {symbol}, volume = {volume}, slippage = {slippage}");
        }

        //OrderClose
        private async void button20_Click(object sender, EventArgs e)
        {
            var ticket = int.Parse(textBoxIndexTicket.Text);
            var slippage = (int)numericOrderSlippage.Value;

            bool closed;

            if (checkBox1.Checked)
            {
                closed = await Execute(() => _apiClient.OrderClose(ticket, slippage));
            }
            else
            {
                double volume;
                double.TryParse(textBoxOrderVolume.Text, out volume);

                double price;
                double.TryParse(textBoxOrderPrice.Text, out price);
                closed = await Execute(() => _apiClient.OrderClose(ticket, volume, price, slippage));
            }

            AddToLog($"Close order result: {closed}, ticket = {ticket}");
        }

        //OrderCloseBy
        private async void button15_Click(object sender, EventArgs e)
        {
            var ticket = int.Parse(textBoxIndexTicket.Text);
            var opposite = int.Parse(textBoxOppositeTicket.Text);

            var closed = await Execute(() => _apiClient.OrderCloseBy(ticket, opposite));

            AddToLog($"Close order result: {closed}, ticket = {ticket}");
        }

        //OrderDelete
        private async void button21_Click(object sender, EventArgs e)
        {
            var ticket = int.Parse(textBoxIndexTicket.Text);

            var deleted = await Execute(() => _apiClient.OrderDelete(ticket));

            AddToLog($"Delete order result: {deleted}, ticket = {ticket}");
        }

        //OrderModify
        private async void button22_Click(object sender, EventArgs e)
        {
            var ticket = int.Parse(textBoxIndexTicket.Text);

            double price;
            double.TryParse(textBoxOrderPrice.Text, out price);

            double stoploss;
            double.TryParse(textBoxOrderStoploss.Text, out stoploss);

            double takeprofit;
            double.TryParse(textBoxOrderProffit.Text, out takeprofit);

            var expiration = DateTime.MinValue;

            Color color;
            switch (comboBoxOrderColor.SelectedIndex)
            {
                case 0:
                    color = Color.Green;
                    break;
                case 1:
                    color = Color.Blue;
                    break;
                case 2:
                    color = Color.Red;
                    break;
                default:
                    return;
            }

            var modified = await Execute(() => _apiClient.OrderModify(ticket, price, stoploss, takeprofit, expiration, color));

            AddToLog($"Modify order result: {modified}, ticket = {ticket}");
        }

        //iCustom (ZigZag)
        private async void iCustomBtn_Click(object sender, EventArgs e)
        {
            const string symbol = "EURUSD";
            const ChartPeriod timeframe = ChartPeriod.PERIOD_H1;
            const string name = "ZigZag";
            int[] parameters = { 12, 5, 4 };
            const int mode = 0;
            const int shift = 0;

            var retVal = await Execute(() => _apiClient.iCustom(symbol, (int)timeframe, name, parameters, mode, shift));
            AddToLog($"ICustom result: {retVal}");
        }

        //iCustom (Parabolic)
        private async void button23_Click(object sender, EventArgs e)
        {
            const string symbol = "EURUSD";
            const ChartPeriod timeframe = ChartPeriod.PERIOD_H1;
            const string name = "Parabolic";
            double[] parameters = { 0.02, 0.2 };
            const int mode = 0;
            const int shift = 1;

            var retVal = await Execute(() => _apiClient.iCustom(symbol, (int)timeframe, name, parameters, mode, shift));
            AddToLog($"ICustom result: {retVal}");
        }

        //CopyRates
        private async void button24_Click(object sender, EventArgs e)
        {
            var symbol = textBoxSelectedSymbol.Text;
            ENUM_TIMEFRAMES timeframes;
            Enum.TryParse(comboBox3.SelectedValue.ToString(), out timeframes);

            var startPos = Convert.ToInt32(numericUpDown1.Value);
            var count = Convert.ToInt32(numericUpDown2.Value);

            var rates = await Execute(() => _apiClient.CopyRates(symbol, timeframes, startPos, count));

            if (rates != null)
            {
                foreach (var r in rates)
                {
                    var result =
                        $"Rate: Time = {r.Time}, Open = {r.Open}, High = {r.High}, Low = {r.Low}, Close = {r.Close}, TickVolume = {r.TickVolume}, Spread = {r.Spread}, RealVolume = {r.RealVolume}";
                    AddToLog(result);
                }
            }
            else
            {
                AddToLog("CopyRates: 0 rates");
            }
        }

        //CopyRates
        private async void button25_Click(object sender, EventArgs e)
        {
            var symbol = textBoxSelectedSymbol.Text;
            ENUM_TIMEFRAMES timeframes;
            Enum.TryParse(comboBox3.SelectedValue.ToString(), out timeframes);

            var startTime = dateTimePicker1.Value;
            var count = Convert.ToInt32(numericUpDown2.Value);

            var rates = await Execute(() => _apiClient.CopyRates(symbol, timeframes, startTime, count));

            if (rates != null)
            {
                foreach (var r in rates)
                {
                    var result =
                        $"Rate: Time = {r.Time}, Open = {r.Open}, High = {r.High}, Low = {r.Low}, Close = {r.Close}, TickVolume = {r.TickVolume}, Spread = {r.Spread}, RealVolume = {r.RealVolume}";
                    AddToLog(result);
                }
            }
            else
            {
                AddToLog("CopyRates: 0 rates");
            }
        }

        //CopyRates
        private async void button26_Click(object sender, EventArgs e)
        {
            var symbol = textBoxSelectedSymbol.Text;
            ENUM_TIMEFRAMES timeframes;
            Enum.TryParse(comboBox3.SelectedValue.ToString(), out timeframes);

            var startTime = dateTimePicker1.Value;
            var stopTime = dateTimePicker1.Value;

            var rates = await Execute(() => _apiClient.CopyRates(symbol, timeframes, startTime, stopTime));

            if (rates != null)
            {
                foreach (var r in rates)
                {
                    var result =
                        $"Rate: Time = {r.Time}, Open = {r.Open}, High = {r.High}, Low = {r.Low}, Close = {r.Close}, TickVolume = {r.TickVolume}, Spread = {r.Spread}, RealVolume = {r.RealVolume}";
                    AddToLog(result);
                }
            }
            else
            {
                AddToLog("CopyRates: 0 rates");
            }
        }

        //Print
        private void button27_Click(object sender, EventArgs e)
        {
            var msg = textBoxPrint.Text;
            if (!string.IsNullOrEmpty(msg))
            {
                _apiClient.Print(msg);
                AddToLog("Print executed");
            }
        }

        //SymbolsTotal
        private async void button28_Click(object sender, EventArgs e)
        {
            var resultTrue = await Execute(() => _apiClient.SymbolsTotal(true));
            AddToLog($"SymbolsTotal [true]: result = {resultTrue}");

            var resultFalse = await Execute(() => _apiClient.SymbolsTotal(false));
            AddToLog($"SymbolsTotal [false]: result = {resultFalse}");
        }

        //SymbolName
        private async void button29_Click(object sender, EventArgs e)
        {
            const int pos = 1;
            const bool selected = false;

            var result = await Execute(() => _apiClient.SymbolName(pos, selected));
            AddToLog($"SymbolName [true]: result = {result}");
        }

        //SymbolSelect
        private async void button30_Click(object sender, EventArgs e)
        {
            var symbol = textBox1.Text;
            const bool @select = true;

            var result = await Execute(() => _apiClient.SymbolSelect(symbol, select));
            AddToLog($"SymbolSelect [true]: result = {result}");
        }

        //SymbolInfoInteger
        private async void button32_Click(object sender, EventArgs e)
        {
            var symbol = textBox1.Text;
            EnumSymbolInfoInteger propId;
            Enum.TryParse(comboBox4.Text, out propId);

            var result = await Execute(() => _apiClient.SymbolInfoInteger(symbol, propId));
            AddToLog($"SymbolInfoInteger [true]: result = {result}");
        }

        private void _tradeMonitor_AvailabilityOrdersChanged(object sender, AvailabilityOrdersEventArgs e)
        {
            if (e.Opened != null)
            {
                AddToLog($"{sender.GetType()}: Opened orders - {string.Join(", ", e.Opened.Select(o => o.Ticket).ToList())}");
            }

            if (e.Closed != null)
            {
                AddToLog($"{sender.GetType()}: Closed orders - {string.Join(", ", e.Closed.Select(o => o.Ticket).ToList())}");
            }
        }
    }
}
