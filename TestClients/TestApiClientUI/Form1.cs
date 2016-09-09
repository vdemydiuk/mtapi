using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MtApi;
using System.Threading.Tasks;
using System.Linq;
using MtApi.Monitors;

namespace TestApiClientUI
{
    public partial class Form1 : Form
    {
        private readonly List<Action> _groupOrderCommands = new List<Action>();
        private readonly MtApiClient _apiClient = new MtApiClient();
        private readonly TimerTradeMonitor _timerTradeMonitor;
        private readonly TimeframeTradeMonitor _timeframeTradeMonitor;

        public Form1()
        {
            InitializeComponent();

            comboBox3.DataSource = Enum.GetNames(typeof(ENUM_TIMEFRAMES));

            _apiClient.QuoteUpdated += apiClient_QuoteUpdated;
            _apiClient.QuoteAdded += apiClient_QuoteAdded;
            _apiClient.QuoteRemoved += apiClient_QuoteRemoved;
            _apiClient.ConnectionStateChanged += apiClient_ConnectionStateChanged;
            _apiClient.OnLastTimeBar += _apiClient_OnLastTimeBar;

            initOrderCommandsGroup();

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;

            _timerTradeMonitor = new TimerTradeMonitor(_apiClient);
            _timerTradeMonitor.Interval = 1000; // 1 sec
            _timerTradeMonitor.AvailabilityOrdersChanged += _tradeMonitor_AvailabilityOrdersChanged;

            _timeframeTradeMonitor = new TimeframeTradeMonitor(_apiClient);
            _timeframeTradeMonitor.AvailabilityOrdersChanged += _tradeMonitor_AvailabilityOrdersChanged;
        }

        private void initOrderCommandsGroup()
        {
            _groupOrderCommands.Add(closeOrders);
            _groupOrderCommands.Add(closeOrdersBy);
            _groupOrderCommands.Add(orderClosePrice);
            _groupOrderCommands.Add(orderCloseTime);
            _groupOrderCommands.Add(orderComment);
            _groupOrderCommands.Add(orderCommission);
            _groupOrderCommands.Add(orderDelete);
            _groupOrderCommands.Add(orderExpiration);
            _groupOrderCommands.Add(orderLots);
            _groupOrderCommands.Add(orderMagicNumber);
            _groupOrderCommands.Add(orderModify);
            _groupOrderCommands.Add(orderOpenPrice);
            _groupOrderCommands.Add(orderOpenTime);
            _groupOrderCommands.Add(orderPrint);
            _groupOrderCommands.Add(orderProfit);
            _groupOrderCommands.Add(orderSelect);
            _groupOrderCommands.Add(ordersHistoryTotal);
            _groupOrderCommands.Add(orderStopLoss);
            _groupOrderCommands.Add(ordersTotal);
            _groupOrderCommands.Add(orderSwap);
            _groupOrderCommands.Add(orderSymbol);
            _groupOrderCommands.Add(orderTakeProfit);
            _groupOrderCommands.Add(orderTicket);
            _groupOrderCommands.Add(orderType);
        }

        private void RunOnUiThread(Action action)
        {
            if (!IsDisposed)
            {
                this.BeginInvoke(action);   
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
            string msg = string.Format("TimeBar: Symbol = {0}, OpenTime = {1}, CloseTime = {2}, Open = {3}, Close = {4}, High = {5}, Low = {6}",
                e.TimeBar.Symbol, e.TimeBar.OpenTime, e.TimeBar.CloseTime, e.TimeBar.Open, e.TimeBar.Close, e.TimeBar.High, e.TimeBar.Low);
            Console.WriteLine(msg);
            AddToLog(msg);
        }

        private volatile bool IsUiQuoteUpdateReady = true;

        private void apiClient_QuoteUpdated(object sender, string symbol, double bid, double ask)
        {
            Console.WriteLine("Quote: Symbol = {0}, Bid = {1}, Ask = {2}", symbol, bid, ask);
            //if UI of quite is busy we are skipping this update
            if (IsUiQuoteUpdateReady)
            {
                RunOnUiThread(() => ChangeQuote(symbol, bid, ask));
            }
        }

        private void AddNewQuote(MtQuote quote)
        {
            if (quote != null
                && string.IsNullOrEmpty(quote.Instrument) == false
                && listViewQuotes.Items.ContainsKey(quote.Instrument) == false)
            {
                var item = new ListViewItem(quote.Instrument);
                item.Name = quote.Instrument;
                item.SubItems.Add(quote.Bid.ToString());
                item.SubItems.Add(quote.Ask.ToString());
                item.SubItems.Add("1");
                listViewQuotes.Items.Add(item);
            }
            else
            {
                var item = listViewQuotes.Items[quote.Instrument];
                int feedCount = 0;
                int.TryParse(item.SubItems[3].Text, out feedCount);
                feedCount++;
                item.SubItems[3].Text = feedCount.ToString();
            }
        }

        private void RemoveQuote(MtQuote quote)
        {
            if (quote != null
                && string.IsNullOrEmpty(quote.Instrument) == false
                && listViewQuotes.Items.ContainsKey(quote.Instrument) == true)
            {
                var item = listViewQuotes.Items[quote.Instrument];
                int feedCount = 0;
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
            IsUiQuoteUpdateReady = false;
            if (string.IsNullOrEmpty(symbol) == false)
            {
                if (listViewQuotes.Items.ContainsKey(symbol) == true)
                {
                    var item = listViewQuotes.Items[symbol];
                    item.SubItems[1].Text = bid.ToString();
                    item.SubItems[2].Text = ask.ToString();
                }
            }
            IsUiQuoteUpdateReady = true;
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
            string serverName = textBoxServerName.Text;

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

        private void closeOrders()
        {
            if (listBoxSendedOrders.SelectedItems.Count > 0)
            {
                double volume;
                double.TryParse(textBoxOrderVolume.Text, out volume);

                double price;
                double.TryParse(textBoxOrderPrice.Text, out price);

                int slippage = (int)numericOrderSlippage.Value;

                List<int> sendedOrders = new List<int>();

                foreach (var item in listBoxSendedOrders.SelectedItems)
                {
                    int orderId = (int)item;
                    var result = _apiClient.OrderClose(orderId, volume, price, slippage);

                    if (result == true)
                    {
                        sendedOrders.Add(orderId);
                    }

                    AddToLog(string.Format("Closed order result: {0},  ticketId = {1}, volume - {2}, slippage {3}", result, orderId, volume, slippage));
                }

                foreach (var orderId in sendedOrders)
                {
                    listBoxClosedOrders.Items.Add(orderId);
                    listBoxSendedOrders.Items.Remove(orderId);
                }
            }
        }

        private void closeOrdersBy()
        {
            if (listBoxSendedOrders.SelectedItems.Count >= 1)
            {
                int ticket = int.Parse(textBoxIndexTicket.Text);
                int opposite = (int)listBoxSendedOrders.SelectedItems[0];

                Color color = new Color();
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
                }

                var result = _apiClient.OrderCloseBy(ticket, opposite, color);

                AddToLog(string.Format("ClosedBy order result: {0},  ticketId = {1}, opposite {2}", result, ticket, opposite));
            }
        }

        private void orderClosePrice()
        {
            var result = _apiClient.OrderClosePrice();
            textBoxOrderPrice.Text = result.ToString();
            AddToLog(string.Format("OrderClosePrice result: {0}", result));
        }

        private void orderCloseTime()
        {
            var result = _apiClient.OrderCloseTime();
            AddToLog(string.Format("OrderCloseTime result: {0}", result));
        }

        private void orderComment()
        {
            var result = _apiClient.OrderComment();
            textBoxOrderComment.Text = result.ToString();
            AddToLog(string.Format("OrderComment result: {0}", result));
        }

        private void orderCommission()
        {
            var result = _apiClient.OrderCommission();
            AddToLog(string.Format("OrderCommission result: {0}", result));
        }

        private void orderDelete()
        {
            if (listBoxSendedOrders.SelectedItems.Count >= 1)
            {
                int ticket = (int)listBoxSendedOrders.SelectedItems[0];

                var result = _apiClient.OrderDelete(ticket);

                AddToLog(string.Format("Delete order result: {0},  ticketId = {1}", result, ticket));
            }
        }

        private void orderExpiration()
        {
            var result = _apiClient.OrderExpiration();
            AddToLog(string.Format("Expiration order result: {0}", result));
        }

        private void orderLots()
        {
            var result = _apiClient.OrderLots();
            AddToLog(string.Format("Lots order result: {0}", result));
        }

        private void orderMagicNumber()
        {
            var result = _apiClient.OrderMagicNumber();
            AddToLog(string.Format("MagicNumber order result: {0}", result));
        }

        private void orderModify()
        {
            if (listBoxSendedOrders.SelectedItems.Count >= 1)
            {
                int ticket = (int)listBoxSendedOrders.SelectedItems[0];

                double price;
                double.TryParse(textBoxOrderPrice.Text, out price);

                double stoploss;
                double.TryParse(textBoxOrderStoploss.Text, out stoploss);

                double takeprofit;
                double.TryParse(textBoxOrderProffit.Text, out takeprofit);

                DateTime expiration = DateTime.Now;

                Color arrow_color = new Color();
                switch (comboBoxOrderColor.SelectedIndex)
                {
                    case 0:
                        arrow_color = Color.Green;
                        break;
                    case 1:
                        arrow_color = Color.Blue;
                        break;
                    case 2:
                        arrow_color = Color.Red;
                        break;
                }

                var result = _apiClient.OrderModify(ticket, price, stoploss, takeprofit, expiration, arrow_color);

                AddToLog(string.Format("OrderModify result: {0}", result));
            }
        }

        private void orderOpenPrice()
        {
            var result = _apiClient.OrderOpenPrice();
            AddToLog(string.Format("OrderOpenPrice result: {0}", result));            
        }

        private void orderOpenTime()
        {
            var result = _apiClient.OrderOpenTime();
            AddToLog(string.Format("OpenTime order result: {0}", result));
        }

        private void orderPrint()
        {
            _apiClient.OrderPrint();
        }

        private void orderProfit()
        {
            var result = _apiClient.OrderProfit();
            AddToLog(string.Format("Profit order result: {0}", result));
        }

        private void orderSelect()
        {
            int ticket = -1;

            if (string.IsNullOrEmpty(textBoxIndexTicket.Text) == false)
                ticket = int.Parse(textBoxIndexTicket.Text);
            else if (listBoxSendedOrders.SelectedItems.Count > 0)
                ticket = (int)listBoxSendedOrders.SelectedItems[0];
            else if (listBoxClosedOrders.SelectedItems.Count > 0)
                ticket = (int)listBoxClosedOrders.SelectedItems[0];
            
            if (ticket >= 0)
            {
                var result = _apiClient.OrderSelect(ticket, OrderSelectMode.SELECT_BY_POS);

                AddToLog(string.Format("OrderSelect result: {0}", result));
            }
        }

        private void ordersHistoryTotal()
        {
            var result = _apiClient.OrdersHistoryTotal();
            AddToLog(string.Format("OrdersHistoryTotal result: {0}", result));
        }

        private void orderStopLoss()
        {
            var result = _apiClient.OrderStopLoss();
            textBoxOrderStoploss.Text = result.ToString();
            AddToLog(string.Format("OrderStopLoss result: {0}", result));
        }

        private void ordersTotal()
        {
            var result = _apiClient.OrdersTotal();
            AddToLog(string.Format("OrdersTotal result: {0}", result));
        }

        private void orderSwap()
        {
            var result = _apiClient.OrderSwap();
            AddToLog(string.Format("OrderSwap result: {0}", result));
        }

        private void orderSymbol()
        {
            var result = _apiClient.OrderSymbol();
            textBoxOrderSymbol.Text = result;
            AddToLog(string.Format("OrderSymbol result: {0}", result));
        }

        private void orderTakeProfit()
        {
            var result = _apiClient.OrderTakeProfit();
            textBoxOrderProffit.Text = result.ToString();
            AddToLog(string.Format("OrderTakeProfit result: {0}", result));
        }

        private void orderTicket()
        {
            var result = _apiClient.OrderTicket();
            AddToLog(string.Format("OrderTicket result: {0}", result));
        }

        private void orderType()
        {
            var result = _apiClient.OrderType();
            comboBoxOrderCommand.SelectedIndex = (int)result;
            AddToLog(string.Format("OrderType result: {0}", result));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _groupOrderCommands[comboBoxSelectedCommand.SelectedIndex]();
        }

        private void AddToLog(string msg)
        {
            RunOnUiThread(() =>
            {
                listBoxEventLog.Items.Add(msg);
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
                        AddToLog(string.Format("GetLastError result: {0}", result));
                    }
                    break;
                case 1:
                    {
                        var result = _apiClient.IsConnected();
                        AddToLog(string.Format("IsConnected result: {0}", result));
                    }
                    break;
                case 2:
                    {
                        var result = _apiClient.IsDemo();
                        AddToLog(string.Format("IsDemo result: {0}", result));
                    }
                    break;
                case 3:
                    {
                        var result = _apiClient.IsDllsAllowed();
                        AddToLog(string.Format("IsDllsAllowed result: {0}", result));
                    }                    
                    break;
                case 4:
                    {
                        var result = _apiClient.IsExpertEnabled();
                        AddToLog(string.Format("IsExpertEnabled result: {0}", result));
                    }                    
                    break;
                case 5:
                    {
                        var result = _apiClient.IsLibrariesAllowed();
                        AddToLog(string.Format("IsLibrariesAllowed result: {0}", result));
                    }                    
                    break;
                case 6:
                    {
                        var result = _apiClient.IsOptimization();
                        AddToLog(string.Format("IsOptimization result: {0}", result));
                    }                    
                    break;
                case 7:
                    {
                        var result = _apiClient.IsStopped();
                        AddToLog(string.Format("IsStopped result: {0}", result));
                    }                    
                    break;
                case 8:
                    {
                        var result = _apiClient.IsTesting();
                        AddToLog(string.Format("IsTesting result: {0}", result));
                    }
                    break;
                case 9:
                    {
                        var result = _apiClient.IsTradeAllowed();
                        AddToLog(string.Format("IsTradeAllowed result: {0}", result));
                    }                    
                    break;
                case 10:
                    {
                        var result = _apiClient.IsTradeContextBusy();
                        AddToLog(string.Format("IsTradeContextBusy result: {0}", result));
                    }                    
                    break;
                case 11:
                    {
                        var result = _apiClient.IsVisualMode();
                        AddToLog(string.Format("IsVisualMode result: {0}", result));
                    }                    
                    break;
                case 12:
                    {
                        var result = _apiClient.UninitializeReason();
                        AddToLog(string.Format("UninitializeReason result: {0}", result));
                    }                    
                    break;
                case 13:
                    {
                        int errorCode = -1;
                        int.TryParse(textBoxErrorCode.Text, out errorCode);
                        var result = _apiClient.ErrorDescription(errorCode);
                        AddToLog(string.Format("ErrorDescription result: {0}", result));
                    }                    
                    break;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            switch (listBoxAccountInfo.SelectedIndex)
            {
                case 0:
                    {
                        var result = _apiClient.AccountBalance();
                        AddToLog(string.Format("AccountBalance result: {0}", result));
                    }
                    break;
                case 1:
                    {
                        var result = _apiClient.AccountCredit();
                        AddToLog(string.Format("AccountCredit result: {0}", result));
                    }
                    break;
                case 2:
                    {
                        var result = _apiClient.AccountCompany();
                        AddToLog(string.Format("AccountCompany result: {0}", result));
                    }
                    break;
                case 3:
                    {
                        var result = _apiClient.AccountCurrency();
                        AddToLog(string.Format("AccountCurrency result: {0}", result));
                    }
                    break;
                case 4:
                    {
                        var result = _apiClient.AccountEquity();
                        AddToLog(string.Format("AccountEquity result: {0}", result));
                    }
                    break;
                case 5:
                    {
                        var result = _apiClient.AccountFreeMargin();
                        AddToLog(string.Format("AccountFreeMargin result: {0}", result));
                    }
                    break;
                case 6:
                    {
                        var result = _apiClient.AccountFreeMarginCheck(textBoxAccountInfoSymbol.Text, (TradeOperation)comboBoxAccountInfoCmd.SelectedIndex, int.Parse(textBoxAccountInfoVolume.Text));
                        AddToLog(string.Format("AccountFreeMarginCheck result: {0}", result));
                    }
                    break;
                case 7:
                    {
                        var result = _apiClient.AccountFreeMarginMode();
                        AddToLog(string.Format("AccountFreeMarginMode result: {0}", result));
                    }
                    break;
                case 8:
                    {
                        var result = _apiClient.AccountLeverage();
                        AddToLog(string.Format("AccountLeverage result: {0}", result));
                    }
                    break;
                case 9:
                    {
                        var result = _apiClient.AccountMargin();
                        AddToLog(string.Format("AccountMargin result: {0}", result));
                    }
                    break;
                case 10:
                    {
                        var result = _apiClient.AccountName();
                        AddToLog(string.Format("AccountName result: {0}", result));
                    }
                    break;
                case 11:
                    {
                        var result = _apiClient.AccountNumber();
                        AddToLog(string.Format("AccountNumber result: {0}", result));
                    }
                    break;
                case 12:
                    {
                        var result = _apiClient.AccountProfit();
                        AddToLog(string.Format("AccountProfit result: {0}", result));
                    }
                    break;
                case 13:
                    {
                        var result = _apiClient.AccountServer();
                        AddToLog(string.Format("AccountServer result: {0}", result));
                    }
                    break;
                case 14:
                    {
                        var result = _apiClient.AccountStopoutLevel();
                        AddToLog(string.Format("AccountStopoutLevel result: {0}", result));
                    }
                    break;
                case 15:
                    {
                        var result = _apiClient.AccountStopoutMode();
                        AddToLog(string.Format("AccountStopoutMode result: {0}", result));
                    }
                    break;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (listBoxMarketInfo.SelectedIndex < 0)
                return;

            var result = _apiClient.MarketInfo(txtMarketInfoSymbol.Text, (MarketInfoModeType)listBoxMarketInfo.SelectedIndex);
            AddToLog(string.Format("MarketInfo result: {0}", result));
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string symbol = textBoxSelectedSymbol.Text;
            int timeframeCount = 1;
            int.TryParse(textBoxTimeframesCount.Text, out timeframeCount);

            Console.WriteLine("Started time: " + DateTime.Now.ToString());

            List<double> openPriceList = new List<double>();

            //for (int i = 0; i < COUNT; i++)
            //{
            //    var price = _apiClient.iOpen(symbol, ChartPeriod.PERIOD_M1, i);
            //    openPriceList.Add(price);
            //}

            var prices = _apiClient.iCloseArray(symbol, ChartPeriod.PERIOD_M1);

            openPriceList = new List<double>(prices);

            listBoxProceHistory.DataSource = openPriceList;

            Console.WriteLine("Finished time: " + DateTime.Now.ToString());

            using (System.IO.StreamWriter file = new System.IO.StreamWriter("d:\\test.txt"))
            {
                foreach (var value in openPriceList)
                {
                    file.WriteLine(value.ToString());
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string symbol = textBoxSelectedSymbol.Text;
            var barCount = _apiClient.iBars(symbol, ChartPeriod.PERIOD_M1);
            textBoxTimeframesCount.Text = barCount.ToString();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string symbol = textBoxSelectedSymbol.Text;
            var prices = _apiClient.iHighArray(symbol, ChartPeriod.PERIOD_M1);
            var items = new List<double>(prices);
            listBoxProceHistory.DataSource = items;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            string symbol = textBoxSelectedSymbol.Text;
            var prices = _apiClient.iLowArray(symbol, ChartPeriod.PERIOD_M1);
            var items = new List<double>(prices);
            listBoxProceHistory.DataSource = items;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            string symbol = textBoxSelectedSymbol.Text;
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
            AddToLog(string.Format("TimeCurrent result: {0}", retVal));
        }

        private void button14_Click(object sender, EventArgs e)
        {
            var retVal = _apiClient.TimeLocal();
            AddToLog(string.Format("TimeLocal result: {0}", retVal));
        }

        private void buttonRefreshRates_Click(object sender, EventArgs e)
        {
            var retVal = _apiClient.RefreshRates();
            AddToLog(string.Format("RefreshRates result: {0}", retVal));
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
        private void button1_Click(object sender, EventArgs e)
        {
            var symbol = textBoxOrderSymbol.Text;

            var cmd = (TradeOperation)(comboBoxOrderCommand.SelectedIndex);

            double volume;
            double.TryParse(textBoxOrderVolume.Text, out volume);

            double price;
            double.TryParse(textBoxOrderPrice.Text, out price);

            var slippage = (int)numericOrderSlippage.Value;

            double stoploss;
            double.TryParse(textBoxOrderStoploss.Text, out stoploss);

            double takeprofit;
            double.TryParse(textBoxOrderProffit.Text, out takeprofit);

            var comment = textBoxOrderComment.Text;

            int magic;
            int.TryParse(textBoxOrderMagic.Text, out magic);

            var expiration = DateTime.Now;

            var arrowColor = new Color();
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

            var ticket = Execute(() => _apiClient.OrderSend(symbol, cmd, volume, price, slippage, stoploss, takeprofit, comment, magic, expiration, arrowColor));

            AddToLog(string.Format("Sended order result: ticket = {0}", ticket));
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
                    string.Format(
                        "Order: Ticket = {0}, Symbol = {1}, Operation = {2}, OpenPrice = {3}, ClosePrice = {4}, Lots = {5}, Profit = {6}, Comment = {7}, Commission = {8}, MagicNumber = {9}, OpenTime = {10}, CloseTime = {11}, Swap = {12}, Expiration = {13}, TakeProfit = {14}, StopLoss = {15}",
                        order.Ticket, order.Symbol, order.Operation, order.OpenPrice, order.ClosePrice, order.Lots, order.Profit, order.Comment, order.Commission, order.MagicNumber, order.OpenTime, order.CloseTime, order.Swap, order.Expiration, order.TakeProfit, order.StopLoss);
                AddToLog(result);                
            }
        }

        //GetOrders
        private async void button17_Click(object sender, EventArgs e)
        {
            var selectSource = (OrderSelectSource)comboBox2.SelectedIndex;


            var orders = await Execute(() => _apiClient.GetOrders(selectSource)); ;

            if (orders != null && orders.Count() > 0)
            {
                foreach (var order in orders)
                {
                    var result =
                        string.Format(
                            "Order: Ticket = {0}, Symbol = {1}, Operation = {2}, OpenPrice = {3}, ClosePrice = {4}, Lots = {5}, Profit = {6}, Comment = {7}, Commission = {8}, MagicNumber = {9}, OpenTime = {10}, CloseTime = {11}, Swap = {12}, Expiration = {13}, TakeProfit = {14}, StopLoss = {15}",
                            order.Ticket, order.Symbol, order.Operation, order.OpenPrice, order.ClosePrice, order.Lots, order.Profit, order.Comment, order.Commission, order.MagicNumber, order.OpenTime, order.CloseTime, order.Swap, order.Expiration, order.TakeProfit, order.StopLoss);
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

            AddToLog(string.Format("Sended order result: ticketId = {0}, symbol = {1}, volume = {2}, slippage = {3}",
                ticket, symbol, volume, slippage));
        }

        //OrderSendSell
        private async void button19_Click(object sender, EventArgs e)
        {
            var symbol = textBoxOrderSymbol.Text;

            double volume;
            double.TryParse(textBoxOrderVolume.Text, out volume);

            var slippage = (int)numericOrderSlippage.Value;

            var ticket = await Execute(() => _apiClient.OrderSendSell(symbol, volume, slippage));

            AddToLog(string.Format("Sended order result: ticketId = {0}, symbol = {1}, volume = {2}, slippage = {3}",
                ticket, symbol, volume, slippage));
        }

        //OrderClose
        private async void button20_Click(object sender, EventArgs e)
        {
            var ticket = int.Parse(textBoxIndexTicket.Text);
            var slippage = (int)numericOrderSlippage.Value;

            var closed = false;

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

            AddToLog(string.Format("Close order result: {0}, ticket = {1}", closed, ticket));
        }

        //OrderCloseBy
        private async void button15_Click(object sender, EventArgs e)
        {
            var ticket = int.Parse(textBoxIndexTicket.Text);
            var opposite = int.Parse(textBoxOppositeTicket.Text);

            var closed = await Execute(() => _apiClient.OrderCloseBy(ticket, opposite)); ;

            AddToLog(string.Format("Close order result: {0}, ticket = {1}", closed, ticket));
        }

        //OrderDelete
        private async void button21_Click(object sender, EventArgs e)
        {
            var ticket = int.Parse(textBoxIndexTicket.Text);

            var deleted = await Execute(() => _apiClient.OrderDelete(ticket));

            AddToLog(string.Format("Delete order result: {0}, ticket = {1}", deleted, ticket));
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

            Color color = new Color();
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
            }

            var modified = await Execute(() => _apiClient.OrderModify(ticket, price, stoploss, takeprofit, expiration, color));

            AddToLog(string.Format("Modify order result: {0}, ticket = {1}", modified, ticket));
        }

        //iCustom (ZigZag)
        private async void iCustomBtn_Click(object sender, EventArgs e)
        {
            string symbol = "EURUSD";
            var timeframe = ChartPeriod.PERIOD_H1;
            string name = "ZigZag";
            int[] parameters = { 12, 5, 4 };
            int mode = 0;
            int shift = 0;
            var retVal = await Execute(() => _apiClient.iCustom(symbol, (int)timeframe, name, parameters, mode, shift));
            AddToLog(string.Format("ICustom result: {0}", retVal));
        }

        //iCustom (Parabolic)
        private async void button23_Click(object sender, EventArgs e)
        {
            string symbol = "EURUSD";
            var timeframe = ChartPeriod.PERIOD_H1;
            string name = "Parabolic";
            double[] parameters = { 0.02, 0.2 };
            int mode = 0;
            int shift = 1;
            var retVal = await Execute(() => _apiClient.iCustom(symbol, (int)timeframe, name, parameters, mode, shift));
            AddToLog(string.Format("ICustom result: {0}", retVal));
        }

        //CopyRates
        private async void button24_Click(object sender, EventArgs e)
        {
            string symbol = textBoxSelectedSymbol.Text;
            ENUM_TIMEFRAMES timeframes;
            Enum.TryParse<ENUM_TIMEFRAMES>(comboBox3.SelectedValue.ToString(), out timeframes);

            int startPos = Convert.ToInt32(numericUpDown1.Value);
            int count = Convert.ToInt32(numericUpDown2.Value);

            var rates = await Execute(() => _apiClient.CopyRates(symbol, timeframes, startPos, count));

            if (rates != null)
            {
                foreach (var r in rates)
                {
                    var result = string.Format("Rate: Time = {0}, Open = {1}, High = {2}, Low = {3}, Close = {4}, TickVolume = {5}, Spread = {6}, RealVolume = {7}",
                        r.Time, r.Open, r.High, r.Low, r.Close, r.TickVolume, r.Spread, r.RealVolume);
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
            string symbol = textBoxSelectedSymbol.Text;
            ENUM_TIMEFRAMES timeframes;
            Enum.TryParse<ENUM_TIMEFRAMES>(comboBox3.SelectedValue.ToString(), out timeframes);

            DateTime startTime = dateTimePicker1.Value;
            int count = Convert.ToInt32(numericUpDown2.Value);

            var rates = await Execute(() => _apiClient.CopyRates(symbol, timeframes, startTime, count));

            if (rates != null)
            {
                foreach (var r in rates)
                {
                    var result = string.Format("Rate: Time = {0}, Open = {1}, High = {2}, Low = {3}, Close = {4}, TickVolume = {5}, Spread = {6}, RealVolume = {7}",
                        r.Time, r.Open, r.High, r.Low, r.Close, r.TickVolume, r.Spread, r.RealVolume);
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
            string symbol = textBoxSelectedSymbol.Text;
            ENUM_TIMEFRAMES timeframes;
            Enum.TryParse<ENUM_TIMEFRAMES>(comboBox3.SelectedValue.ToString(), out timeframes);

            DateTime startTime = dateTimePicker1.Value;
            DateTime stopTime = dateTimePicker1.Value;

            var rates = await Execute(() => _apiClient.CopyRates(symbol, timeframes, startTime, stopTime));

            if (rates != null)
            {
                foreach (var r in rates)
                {
                    var result = string.Format("Rate: Time = {0}, Open = {1}, High = {2}, Low = {3}, Close = {4}, TickVolume = {5}, Spread = {6}, RealVolume = {7}",
                        r.Time, r.Open, r.High, r.Low, r.Close, r.TickVolume, r.Spread, r.RealVolume);
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
                AddToLog(string.Format("Print executed"));
            }
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
