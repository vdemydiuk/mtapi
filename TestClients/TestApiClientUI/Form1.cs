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
            comboBox6.DataSource = Enum.GetNames(typeof(ENUM_TIMEFRAMES));
            comboBox7.DataSource = Enum.GetNames(typeof(EnumSymbolInfoDouble));
            comboBox8.DataSource = Enum.GetNames(typeof(MarketInfoModeType));
            comboBox9.DataSource = Enum.GetNames(typeof(EnumTerminalInfoInteger));
            comboBox10.DataSource = Enum.GetNames(typeof(EnumTerminalInfoDouble));
            comboBox11.DataSource = Enum.GetNames(typeof(EnumObject));
            comboBoxAccountInfoCmd.DataSource = Enum.GetNames(typeof(TradeOperation));

            _apiClient.QuoteUpdated += apiClient_QuoteUpdated;
            _apiClient.QuoteUpdate += _apiClient_QuoteUpdate;
            _apiClient.QuoteAdded += apiClient_QuoteAdded;
            _apiClient.QuoteRemoved += apiClient_QuoteRemoved;
            _apiClient.ConnectionStateChanged += apiClient_ConnectionStateChanged;
            _apiClient.OnLastTimeBar += _apiClient_OnLastTimeBar;
            _apiClient.OnChartEvent += _apiClient_OnChartEvent;

            InitOrderCommandsGroup();

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
            comboBox5.SelectedIndex = 0;
            comboBox11.SelectedIndex = 0;
            comboBoxAccountInfoCmd.SelectedIndex = 0;

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
            if (e.Quote != null)
            {
                RunOnUiThread(() => RemoveQuote(e.Quote));
            }
        }

        private void apiClient_QuoteAdded(object sender, MtQuoteEventArgs e)
        {
            if (e.Quote != null)
            {
                RunOnUiThread(() => AddNewQuote(e.Quote));
            }
        }

        private void _apiClient_OnLastTimeBar(object sender, TimeBarArgs e)
        {
            var msg =
                $"TimeBar: ExpertHandle = {e.ExpertHandle}, Symbol = {e.TimeBar.Symbol}, OpenTime = {e.TimeBar.OpenTime}, CloseTime = {e.TimeBar.CloseTime}, Open = {e.TimeBar.Open}, Close = {e.TimeBar.Close}, High = {e.TimeBar.High}, Low = {e.TimeBar.Low}";
            Console.WriteLine(msg);
            PrintLog(msg);
        }

        private void _apiClient_OnChartEvent(object sender, ChartEventArgs e)
        {
            var msg =
                $"OnChartEvent: ExpertHandle = {e.ExpertHandle}, ChartId = {e.ChartId}, EventId = {e.EventId}, Lparam = {e.Lparam}, Dparam = {e.Dparam}, Sparam = {e.Sparam}";
            Console.WriteLine(msg);
            PrintLog(msg);
        }

        private void apiClient_QuoteUpdated(object sender, string symbol, double bid, double ask)
        {
            Console.WriteLine(@"Quote: Symbol = {0}, Bid = {1}, Ask = {2}", symbol, bid, ask);
        }

        private void _apiClient_QuoteUpdate(object sender, MtQuoteEventArgs e)
        {
            //if UI of quite is busy we are skipping this update
            if (_isUiQuoteUpdateReady)
            {
                RunOnUiThread(() => ChangeQuote(e.Quote));
            }
        }

        private void AddNewQuote(MtQuote quote)
        {
            var key = quote.ExpertHandle.ToString();

            if (listViewQuotes.Items.ContainsKey(key) == false)
            {
                var item = new ListViewItem(quote.Instrument) { Name = key };
                item.SubItems.Add(quote.Bid.ToString(CultureInfo.CurrentCulture));
                item.SubItems.Add(quote.Ask.ToString(CultureInfo.CurrentCulture));
                item.SubItems.Add(key);
                listViewQuotes.Items.Add(item);
            }
        }

        private void RemoveQuote(MtQuote quote)
        {
            var key = quote.ExpertHandle.ToString();

            if (listViewQuotes.Items.ContainsKey(key))
            {
                listViewQuotes.Items.RemoveByKey(key);
            }
        }

        private void ChangeQuote(MtQuote quote)
        {
            _isUiQuoteUpdateReady = false;

            var key = quote.ExpertHandle.ToString();

            if (listViewQuotes.Items.ContainsKey(key))
            {
                var item = listViewQuotes.Items[key];
                item.SubItems[1].Text = quote.Bid.ToString(CultureInfo.CurrentCulture);
                item.SubItems[2].Text = quote.Ask.ToString(CultureInfo.CurrentCulture);
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
                textBoxAccountInfoSymbol.Text = listViewQuotes.SelectedItems[0].Text;

                if (checkBox2.Checked)
                {
                    _apiClient.ExecutorHandle = int.Parse(listViewQuotes.SelectedItems[0].SubItems[3].Text);
                }
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

                    PrintLog($"Closed order result: {result},  ticketId = {orderId}, volume - {volume}, slippage {slippage}");
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

                PrintLog($"ClosedBy order result: {result},  ticketId = {ticket}, opposite {opposite}");
            }
        }

        private void OrderClosePrice()
        {
            var result = _apiClient.OrderClosePrice();
            textBoxOrderPrice.Text = result.ToString(CultureInfo.CurrentCulture);
            PrintLog($"OrderClosePrice result: {result}");
        }

        private void OrderCloseTime()
        {
            var result = _apiClient.OrderCloseTime();
            PrintLog($"OrderCloseTime result: {result}");
        }

        private void OrderComment()
        {
            var result = _apiClient.OrderComment();
            textBoxOrderComment.Text = result;
            PrintLog($"OrderComment result: {result}");
        }

        private void OrderCommission()
        {
            var result = _apiClient.OrderCommission();
            PrintLog($"OrderCommission result: {result}");
        }

        private void OrderDelete()
        {
            if (listBoxSendedOrders.SelectedItems.Count >= 1)
            {
                var ticket = (int)listBoxSendedOrders.SelectedItems[0];
                var result = _apiClient.OrderDelete(ticket);
                PrintLog($"Delete order result: {result},  ticketId = {ticket}");
            }
        }

        private void OrderExpiration()
        {
            var result = _apiClient.OrderExpiration();
            PrintLog($"Expiration order result: {result}");
        }

        private void OrderLots()
        {
            var result = _apiClient.OrderLots();
            PrintLog($"Lots order result: {result}");
        }

        private void OrderMagicNumber()
        {
            var result = _apiClient.OrderMagicNumber();
            PrintLog($"MagicNumber order result: {result}");
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

                PrintLog($"OrderModify result: {result}");
            }
        }

        private void OrderOpenPrice()
        {
            var result = _apiClient.OrderOpenPrice();
            PrintLog($"OrderOpenPrice result: {result}");
        }

        private void OrderOpenTime()
        {
            var result = _apiClient.OrderOpenTime();
            PrintLog($"OpenTime order result: {result}");
        }

        private void OrderPrint()
        {
            _apiClient.OrderPrint();
        }

        private void OrderProfit()
        {
            var result = _apiClient.OrderProfit();
            PrintLog($"Profit order result: {result}");
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

                PrintLog($"OrderSelect result: {result}");
            }
        }

        private void OrdersHistoryTotal()
        {
            var result = _apiClient.OrdersHistoryTotal();
            PrintLog($"OrdersHistoryTotal result: {result}");
        }

        private void OrderStopLoss()
        {
            var result = _apiClient.OrderStopLoss();
            textBoxOrderStoploss.Text = result.ToString(CultureInfo.CurrentCulture);
            PrintLog($"OrderStopLoss result: {result}");
        }

        private void OrdersTotal()
        {
            var result = _apiClient.OrdersTotal();
            PrintLog($"OrdersTotal result: {result}");
        }

        private void OrderSwap()
        {
            var result = _apiClient.OrderSwap();
            PrintLog($"OrderSwap result: {result}");
        }

        private void OrderSymbol()
        {
            var result = _apiClient.OrderSymbol();
            textBoxOrderSymbol.Text = result;
            PrintLog($"OrderSymbol result: {result}");
        }

        private void OrderTakeProfit()
        {
            var result = _apiClient.OrderTakeProfit();
            textBoxOrderProffit.Text = result.ToString(CultureInfo.CurrentCulture);
            PrintLog($"OrderTakeProfit result: {result}");
        }

        private void OrderTicket()
        {
            var result = _apiClient.OrderTicket();
            PrintLog($"OrderTicket result: {result}");
        }

        private void OrderType()
        {
            var result = _apiClient.OrderType();
            comboBoxOrderCommand.SelectedIndex = (int)result;
            PrintLog($"OrderType result: {result}");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _groupOrderCommands[comboBoxSelectedCommand.SelectedIndex]();
        }

        private void PrintLog(string msg)
        {
            RunOnUiThread(() =>
            {
                var time = DateTime.Now.ToString("h:mm:ss tt");
                listBoxEventLog.Items.Add($"[{time}]: {msg}");
                listBoxEventLog.SetSelected(listBoxEventLog.Items.Count - 1, true);
                listBoxEventLog.SetSelected(listBoxEventLog.Items.Count - 1, false);
            });
        }

        //MarketInfo
        private async void button5_Click(object sender, EventArgs e)
        {
            var symbol = txtMarketInfoSymbol.Text;
            MarketInfoModeType propId;
            Enum.TryParse(comboBox8.Text, out propId);

            var result = await Execute(() => _apiClient.MarketInfo(symbol, propId));
            PrintLog($"MarketInfo result: {result}");
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

            //using (var file = new System.IO.StreamWriter($@"{System.IO.Path.GetTempPath()}\MtApi\test.txt"))
            //{
            //    foreach (var value in openPriceList)
            //    {
            //        file.WriteLine(value);
            //    }
            //}
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
            PrintLog($"TimeCurrent result: {retVal}");
        }

        private void button14_Click(object sender, EventArgs e)
        {
            var retVal = _apiClient.TimeLocal();
            PrintLog($"TimeLocal result: {retVal}");
        }

        private void buttonRefreshRates_Click(object sender, EventArgs e)
        {
            var retVal = _apiClient.RefreshRates();
            PrintLog($"RefreshRates result: {retVal}");
        }

        private void listBoxEventLog_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            listBoxEventLog.Items.Clear();
        }

        private Task<TResult> Execute<TResult>(Func<TResult> func)
        {
            return Task.Factory.StartNew(() =>
            {
                var result = default(TResult);
                try
                {
                    result = func();
                }
                catch (MtConnectionException ex)
                {
                    PrintLog("MtExecutionException: " + ex.Message);
                }
                catch (MtExecutionException ex)
                {
                    PrintLog("MtExecutionException: " + ex.Message + "; ErrorCode = " + ex.ErrorCode);
                }

                return result;
            });
        }

        private Task Execute(Action action)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    action();
                }
                catch (MtConnectionException ex)
                {
                    PrintLog("MtExecutionException: " + ex.Message);
                }
                catch (MtExecutionException ex)
                {
                    PrintLog("MtExecutionException: " + ex.Message + "; ErrorCode = " + ex.ErrorCode);
                }
            });
        }

        //OrderSend
        private async void button1_Click(object sender, EventArgs e)
        {
            var ticket = -1;

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

            if (string.IsNullOrEmpty(comment))
            {
                ticket = await Execute(() => _apiClient.OrderSend(symbol, cmd, volume, price, slippage, stoploss, takeprofit));
            }
            else
            {
                int magic;
                if (!int.TryParse(textBoxOrderMagic.Text, out magic))
                {
                    ticket = await Execute(() => _apiClient.OrderSend(symbol, cmd, volume, price, slippage, stoploss, takeprofit, comment));
                }
                else
                {
                    if (comboBoxOrderColor.SelectedIndex < 0 || comboBoxOrderColor.SelectedIndex > 2)
                    {
                        ticket = await Execute(() => _apiClient.OrderSend(symbol, cmd, volume, price, slippage, stoploss, takeprofit, comment, magic));
                    }
                    else
                    {
                        var expiration = DateTime.Now.AddDays(1);
                        Color arrowColor = Color.White;
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
                        }
                        ticket = await Execute(() => _apiClient.OrderSend(symbol, cmd, volume, price, slippage, stoploss, takeprofit, comment, magic, expiration, arrowColor));
                    }
                }
            }

            PrintLog($"Sended order result: ticket = {ticket}");
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
                PrintLog(result);
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
                    PrintLog(result);
                }
            }
            else
            {
                PrintLog("GetOrders: 0 orders");
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

            PrintLog(
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

            PrintLog($"Sended order result: ticketId = {ticket}, symbol = {symbol}, volume = {volume}, slippage = {slippage}");
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

            PrintLog($"Close order result: {closed}, ticket = {ticket}");
        }

        //OrderCloseBy
        private async void button15_Click(object sender, EventArgs e)
        {
            var ticket = int.Parse(textBoxIndexTicket.Text);
            var opposite = int.Parse(textBoxOppositeTicket.Text);

            var closed = await Execute(() => _apiClient.OrderCloseBy(ticket, opposite));

            PrintLog($"Close order result: {closed}, ticket = {ticket}");
        }

        //OrderDelete
        private async void button21_Click(object sender, EventArgs e)
        {
            var ticket = int.Parse(textBoxIndexTicket.Text);

            var deleted = await Execute(() => _apiClient.OrderDelete(ticket));

            PrintLog($"Delete order result: {deleted}, ticket = {ticket}");
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

            PrintLog($"Modify order result: {modified}, ticket = {ticket}");
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
            PrintLog($"ICustom result: {retVal}");
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
            PrintLog($"ICustom result: {retVal}");
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
                    PrintLog(result);
                }
            }
            else
            {
                PrintLog("CopyRates: 0 rates");
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
                    PrintLog(result);
                }
            }
            else
            {
                PrintLog("CopyRates: 0 rates");
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
                    PrintLog(result);
                }
            }
            else
            {
                PrintLog("CopyRates: 0 rates");
            }
        }

        //Print
        private void button27_Click(object sender, EventArgs e)
        {
            var msg = textBoxPrint.Text;
            if (!string.IsNullOrEmpty(msg))
            {
                _apiClient.Print(msg);
                PrintLog("Print executed");
            }
        }

        //SymbolsTotal
        private async void button28_Click(object sender, EventArgs e)
        {
            var resultTrue = await Execute(() => _apiClient.SymbolsTotal(true));
            PrintLog($"SymbolsTotal [true]: result = {resultTrue}");

            var resultFalse = await Execute(() => _apiClient.SymbolsTotal(false));
            PrintLog($"SymbolsTotal [false]: result = {resultFalse}");
        }

        //SymbolName
        private async void button29_Click(object sender, EventArgs e)
        {
            const int pos = 1;
            const bool selected = false;

            var result = await Execute(() => _apiClient.SymbolName(pos, selected));
            PrintLog($"SymbolName [true]: result = {result}");
        }

        //SymbolSelect
        private async void button30_Click(object sender, EventArgs e)
        {
            var symbol = textBox1.Text;
            const bool @select = true;

            var result = await Execute(() => _apiClient.SymbolSelect(symbol, select));
            PrintLog($"SymbolSelect [true]: result = {result}");
        }

        //SymbolInfoInteger
        private async void button32_Click(object sender, EventArgs e)
        {
            var symbol = textBox1.Text;
            EnumSymbolInfoInteger propId;
            Enum.TryParse(comboBox4.Text, out propId);

            var result = await Execute(() => _apiClient.SymbolInfoInteger(symbol, propId));
            PrintLog($"SymbolInfoInteger [true]: result = {result}");
        }

        private void _tradeMonitor_AvailabilityOrdersChanged(object sender, AvailabilityOrdersEventArgs e)
        {
            if (e.Opened != null)
            {
                PrintLog($"{sender.GetType()}: Opened orders - {string.Join(", ", e.Opened.Select(o => o.Ticket).ToList())}");
            }

            if (e.Closed != null)
            {
                PrintLog($"{sender.GetType()}: Closed orders - {string.Join(", ", e.Closed.Select(o => o.Ticket).ToList())}");
            }
        }

        //SeriesInfoInteger
        private async void button31_Click(object sender, EventArgs e)
        {
            var symbol = txtMarketInfoSymbol.Text;
            ENUM_TIMEFRAMES timeframes;
            Enum.TryParse(comboBox6.Text, out timeframes);
            EnumSeriesInfoInteger propId;
            Enum.TryParse(comboBox5.Text, out propId);

            var result = await Execute(() => _apiClient.SeriesInfoInteger(symbol, timeframes, propId));
            PrintLog($"SeriesInfoInteger: result = {result}");
        }

        //SymbolInfoDouble
        private async void button33_Click(object sender, EventArgs e)
        {
            var symbol = txtMarketInfoSymbol.Text;
            EnumSymbolInfoDouble propId;
            Enum.TryParse(comboBox7.Text, out propId);

            var result = await Execute(() => _apiClient.SymbolInfoDouble(symbol, propId));
            PrintLog($"SymbolInfoDouble: result = {result}");
        }

        //SymbolInfoTick
        private async void button34_Click(object sender, EventArgs e)
        {
            var symbol = txtMarketInfoSymbol.Text;

            var result = await Execute(() => _apiClient.SymbolInfoTick(symbol));
            if (result != null)
            {
                PrintLog($"SymbolInfoTick: Tick - time = {result.Time}, Bid = {result.Bid}, Ask = {result.Ask}, Last = {result.Last}, Volume = {result.Volume}");
            }
        }

        //TerminalInfoInteger
        private async void button35_Click(object sender, EventArgs e)
        {
            EnumTerminalInfoInteger propId;
            Enum.TryParse(comboBox9.Text, out propId);

            var result = await Execute(() => _apiClient.TerminalInfoInteger(propId));
            PrintLog($"TerminalInfoInteger: result = {result}");
        }

        //TerminalInfoDouble
        private async void button36_Click(object sender, EventArgs e)
        {
            EnumTerminalInfoDouble propId;
            Enum.TryParse(comboBox10.Text, out propId);

            var result = await Execute(() => _apiClient.TerminalInfoDouble(propId));
            PrintLog($"TerminalInfoDouble: result = {result}");
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            var checkbox = sender as CheckBox;
            if (checkbox == null)
                return;

            int expertHandle = 0;
            if (checkbox.Checked)
            {
                if (listViewQuotes.SelectedItems.Count > 0)
                {
                    expertHandle = int.Parse(listViewQuotes.SelectedItems[0].SubItems[3].Text);
                }
            }

            _apiClient.ExecutorHandle = expertHandle;
        }

        //GetLastError
        private async void button40_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.GetLastError);
            PrintLog($"GetLastError: result = {result}");
            RunOnUiThread(() => { textBoxErrorCode.Text = result.ToString(); });
        }

        //ErrorDescription
        private async void button39_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxErrorCode.Text))
            {
                MessageBox.Show(@"Error code is not defined!");
                textBoxErrorCode.Focus();
                return;
            }

            int errorCode;
            if (int.TryParse(textBoxErrorCode.Text, out errorCode) == false)
            {
                MessageBox.Show(@"Failed to parse error code!");
                textBoxErrorCode.Focus();
                return;
            }

            var result = await Execute(() => _apiClient.ErrorDescription(errorCode));
            PrintLog($"ErrorDescription: code = {errorCode}, description = {result}");
        }

        //IsConnected
        private async void button41_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.IsConnected);
            PrintLog($"IsConnected: result = {result}");
        }

        //IsDemo
        private async void button42_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.IsDemo);
            PrintLog($"IsDemo: result = {result}");
        }

        //IsDllsAllowed
        private async void button43_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.IsDllsAllowed);
            PrintLog($"IsDllsAllowed: result = {result}");
        }

        //IsExpertEnabled
        private async void button44_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.IsExpertEnabled);
            PrintLog($"IsExpertEnabled: result = {result}");
        }

        //IsLibrariesAllowed
        private async void button45_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.IsLibrariesAllowed);
            PrintLog($"IsLibrariesAllowed: result = {result}");
        }

        //IsOptimization
        private async void button46_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.IsOptimization);
            PrintLog($"IsOptimization: result = {result}");
        }

        //IsStopped
        private async void button47_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.IsStopped);
            PrintLog($"IsStopped: result = {result}");
        }

        //IsTesting
        private async void button48_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.IsTesting);
            PrintLog($"IsTesting: result = {result}");
        }

        //IsTradeAllowed
        private async void button49_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.IsTradeAllowed);
            PrintLog($"IsTradeAllowed: result = {result}");
        }

        //IsTradeContextBusy
        private async void button50_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.IsTradeContextBusy);
            PrintLog($"IsTradeContextBusy: result = {result}");
        }

        //IsVisualMode
        private async void button51_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.IsVisualMode);
            PrintLog($"IsVisualMode: result = {result}");
        }

        //UninitializeReason
        private async void button52_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.UninitializeReason);
            PrintLog($"UninitializeReason: result = {result}");
        }

        //AccountBalance
        private async void button3_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.AccountBalance);
            PrintLog($"AccountBalance result: {result}");
        }

        //AccountCredit
        private async void button53_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.AccountCredit);
            PrintLog($"AccountCredit result: {result}");
        }

        //AccountCompany
        private async void button54_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.AccountCompany);
            PrintLog($"AccountCompany result: {result}");
        }

        //AccountCurrency
        private async void button55_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.AccountCurrency);
            PrintLog($"AccountCurrency result: {result}");
        }

        //AccountEquity
        private async void button56_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.AccountEquity);
            PrintLog($"AccountEquity result: {result}");
        }

        //AccountFreeMargin
        private async void button57_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.AccountFreeMargin);
            PrintLog($"AccountFreeMargin result: {result}");
        }

        //AccountFreeMarginCheck
        private async void button58_Click(object sender, EventArgs e)
        {
            var symbol = textBoxAccountInfoSymbol.Text;
            if (string.IsNullOrEmpty(symbol))
            {
                MessageBox.Show(@"Symbol is not defined!");
                textBoxAccountInfoSymbol.Focus();
                return;
            }

            double volume;
            if (double.TryParse(textBoxAccountInfoVolume.Text, out volume) == false)
            {
                MessageBox.Show(@"Failed to parse volume value!");
                textBoxAccountInfoVolume.Focus();
                return;
            }

            TradeOperation cmd;
            Enum.TryParse(comboBoxAccountInfoCmd.Text, out cmd);

            var result = await Execute(() => _apiClient.AccountFreeMarginCheck(symbol, cmd, volume));
            PrintLog($"AccountFreeMarginCheck result: {result}");
        }

        //AccountFreeMarginMode
        private async void button59_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.AccountFreeMarginMode);
            PrintLog($"AccountFreeMarginMode result: {result}");
        }

        //AccountLeverage
        private async void button60_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.AccountLeverage);
            PrintLog($"AccountLeverage result: {result}");
        }

        //AccountMargin
        private async void button61_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.AccountMargin);
            PrintLog($"AccountMargin result: {result}");
        }

        //AccountName
        private async void button62_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.AccountName);
            PrintLog($"AccountName result: {result}");
        }

        //AccountNumber
        private async void button63_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.AccountNumber);
            PrintLog($"AccountNumber result: {result}");
        }

        //AccountProfit
        private async void button64_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.AccountProfit);
            PrintLog($"AccountProfit result: {result}");
        }

        //AccountServer
        private async void button65_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.AccountServer);
            PrintLog($"AccountServer result: {result}");
        }

        //AccountStopoutLevel
        private async void button66_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.AccountStopoutLevel);
            PrintLog($"AccountStopoutLevel result: {result}");
        }

        //AccountStopoutMode
        private async void button67_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.AccountStopoutMode);
            PrintLog($"AccountStopoutMode result: {result}");
        }

        //CharID
        private async void button37_Click(object sender, EventArgs e)
        {
            var result = await Execute(_apiClient.ChartId);
            PrintLog($"CharID: result = {result}");
            RunOnUiThread(() => textBoxChartId.Text = result.ToString());
        }

        //ChartRedraw
        private async void button38_Click(object sender, EventArgs e)
        {
            await Execute(() => _apiClient.ChartRedraw());
            PrintLog($"ChartRedraw: called.");
        }

        //ObjectCreate
        private async void button4_Click(object sender, EventArgs e)
        {
            const long chartId = 0;
            const string objectName = "label_object";
            EnumObject objectType;
            Enum.TryParse(comboBox11.Text, out objectType);

            var result = await Execute(() => _apiClient.ObjectCreate(chartId, objectName, objectType, 0, null, 0));
            PrintLog($"ObjectCreate result: {result}");
        }

        //ObjectName
        private async void button68_Click(object sender, EventArgs e)
        {
            const long chartId = 0;
            const int objectIndex = 0;

            var result = await Execute(() => _apiClient.ObjectName(chartId, objectIndex));
            PrintLog($"ObjectName result: {result}");
        }

        private void button69_Click(object sender, EventArgs e)
        {
            _apiClient.UnlockTicks();
        }

        private async void button70_Click(object sender, EventArgs e)
        {
            var login = textBoxAccountLogin.Text;
            var password = textBoxAccountPassword.Text;
            var host = textBoxAccountHost.Text;

            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show(@"Login is not defined!", @"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxAccountLogin.Focus();
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show(@"Password is not defined!", @"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxAccountPassword.Focus();
                return;
            }
            if (string.IsNullOrEmpty(host))
            {
                MessageBox.Show(@"Host is not defined!", @"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxAccountHost.Focus();
                return;
            }

            var result = await Execute(() => _apiClient.ChangeAccount(login, password, host));
            PrintLog($"ChangeAccount result: {result}");
        }
    }
}
