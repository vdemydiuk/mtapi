using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;
using MtApi;
using System.Net;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TestApiClientUI
{
    public partial class Form1 : Form
    {
        delegate void PerformCommandHandler();

        private List<PerformCommandHandler> GroupOrderCommands = new List<PerformCommandHandler>();

        private MtApiClient apiClient = new MtApiClient();

        public Form1()
        {
            InitializeComponent();

            apiClient.QuoteUpdated += apiClient_QuoteUpdated;
            apiClient.QuoteAdded += apiClient_QuoteAdded;
            apiClient.QuoteRemoved += apiClient_QuoteRemoved;
            apiClient.ConnectionStateChanged += apiClient_ConnectionStateChanged;

            initOrderCommandsGroup();

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
        }

        private void initOrderCommandsGroup()
        {
            GroupOrderCommands.Add(new PerformCommandHandler(closeOrders));
            GroupOrderCommands.Add(new PerformCommandHandler(closeOrdersBy));
            GroupOrderCommands.Add(new PerformCommandHandler(orderClosePrice));
            GroupOrderCommands.Add(new PerformCommandHandler(orderCloseTime));
            GroupOrderCommands.Add(new PerformCommandHandler(orderComment));
            GroupOrderCommands.Add(new PerformCommandHandler(orderCommission));
            GroupOrderCommands.Add(new PerformCommandHandler(orderDelete));
            GroupOrderCommands.Add(new PerformCommandHandler(orderExpiration));
            GroupOrderCommands.Add(new PerformCommandHandler(orderLots));
            GroupOrderCommands.Add(new PerformCommandHandler(orderMagicNumber));
            GroupOrderCommands.Add(new PerformCommandHandler(orderModify));
            GroupOrderCommands.Add(new PerformCommandHandler(orderOpenPrice));
            GroupOrderCommands.Add(new PerformCommandHandler(orderOpenTime));
            GroupOrderCommands.Add(new PerformCommandHandler(orderPrint));
            GroupOrderCommands.Add(new PerformCommandHandler(orderProfit));
            GroupOrderCommands.Add(new PerformCommandHandler(orderSelect));
            GroupOrderCommands.Add(new PerformCommandHandler(ordersHistoryTotal));
            GroupOrderCommands.Add(new PerformCommandHandler(orderStopLoss));
            GroupOrderCommands.Add(new PerformCommandHandler(ordersTotal));
            GroupOrderCommands.Add(new PerformCommandHandler(orderSwap));
            GroupOrderCommands.Add(new PerformCommandHandler(orderSymbol));
            GroupOrderCommands.Add(new PerformCommandHandler(orderTakeProfit));
            GroupOrderCommands.Add(new PerformCommandHandler(orderTicket));
            GroupOrderCommands.Add(new PerformCommandHandler(orderType));
        }

        private void RunOnUiThread(Action action)
        {
            this.BeginInvoke(action);
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
                    RunOnUiThread(onConnected);
                    break;
                case MtConnectionState.Disconnected:
                case MtConnectionState.Failed:
                    RunOnUiThread(onDisconnected);
                    break;
            }
        }

        private void apiClient_QuoteRemoved(object sender, MtQuoteEventArgs e)
        {
            string instrument = e.Quote.Instrument;

            RunOnUiThread(() =>
            {
                removeQuote(e.Quote);
            });
        }

        private void apiClient_QuoteAdded(object sender, MtQuoteEventArgs e)
        {
            RunOnUiThread(() =>
            {
                addNewQuote(e.Quote);
            });
        }

        private void apiClient_QuoteUpdated(object sender, string symbol, double bid, double ask)
        {
            this.BeginInvoke((Action)(() =>
            {
                changeQuote(symbol, bid, ask);
            }));
        }

        private void addNewQuote(MtQuote quote)
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

        private void removeQuote(MtQuote quote)
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

        private void changeQuote(string symbol, double bid, double ask)
        {
            if (string.IsNullOrEmpty(symbol) == false)
            {
                if (listViewQuotes.Items.ContainsKey(symbol) == true)
                {
                    var item = listViewQuotes.Items[symbol];
                    item.SubItems[1].Text = bid.ToString();
                    item.SubItems[2].Text = ask.ToString();
                }
            }
        }

        private void onConnected()
        {
            var quotes = apiClient.GetQuotes();

            if (quotes != null)
            {
                foreach (var quote in quotes)
                {
                    addNewQuote(quote);
                }                    
            }            
        }

        private void onDisconnected()
        {
            listViewQuotes.Items.Clear();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            string serverName = textBoxServerName.Text;

            int port;
            int.TryParse(textBoxPort.Text, out port);

            if (string.IsNullOrEmpty(serverName))
                apiClient.BeginConnect(port);
            else
                apiClient.BeginConnect(serverName, port);

            onConnected();
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            apiClient.BeginDisconnect();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            apiClient.BeginDisconnect();
        }

        private void sendOrder(string symbol, TradeOperation command, double volume, double price, int slippage, double stoploss, double takeprofit
                                , string comment, int magic, DateTime expiration, Color arrow_color)
        {
            int orderId = apiClient.OrderSend(symbol, command, volume, price, slippage, stoploss, takeprofit, comment, magic, expiration, arrow_color);

            RunOnUiThread(() =>
                {
                    if (orderId >= 0)
                    {
                        listBoxSendedOrders.Items.Add(orderId);
                    }

                    addToLog(string.Format("Sended order result: ticketId = {0}, volume - {1}", orderId, volume, slippage));
                });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string symbol = textBoxOrderSymbol.Text;

            var op_command = (TradeOperation)(comboBoxOrderCommand.SelectedIndex);

            double volume;
            double.TryParse(textBoxOrderVolume.Text, out volume);

            double price;
            double.TryParse(textBoxOrderPrice.Text, out price);

            int slippage = (int)numericOrderSlippage.Value;

            double stoploss;
            double.TryParse(textBoxOrderStoploss.Text, out stoploss);

            double takeprofit;
            double.TryParse(textBoxOrderProffit.Text, out takeprofit);

            string comment = textBoxOrderComment.Text;

            int magic;
            int.TryParse(textBoxOrderMagic.Text, out magic);

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

            Action a = () =>
            {
                sendOrder(symbol, op_command, volume, price, slippage, stoploss, takeprofit, comment, magic, expiration, arrow_color);
            };
            a.BeginInvoke(null, null);
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
                    var result = apiClient.OrderClose(orderId, volume, price, slippage);

                    if (result == true)
                    {
                        sendedOrders.Add(orderId);
                    }

                    addToLog(string.Format("Closed order result: {0},  ticketId = {1}, volume - {2}, slippage {3}", result, orderId, volume, slippage));
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

                var result = apiClient.OrderCloseBy(ticket, opposite, color);

                addToLog(string.Format("ClosedBy order result: {0},  ticketId = {1}, opposite {2}", result, ticket, opposite));
            }
        }

        private void orderClosePrice()
        {
            var result = apiClient.OrderClosePrice();
            textBoxOrderPrice.Text = result.ToString();
            addToLog(string.Format("OrderClosePrice result: {0}", result));
        }

        private void orderCloseTime()
        {
            var result = apiClient.OrderCloseTime();
            addToLog(string.Format("OrderCloseTime result: {0}", result));
        }

        private void orderComment()
        {
            var result = apiClient.OrderComment();
            textBoxOrderComment.Text = result.ToString();
            addToLog(string.Format("OrderComment result: {0}", result));
        }

        private void orderCommission()
        {
            var result = apiClient.OrderCommission();
            addToLog(string.Format("OrderCommission result: {0}", result));
        }

        private void orderDelete()
        {
            if (listBoxSendedOrders.SelectedItems.Count >= 1)
            {
                int ticket = (int)listBoxSendedOrders.SelectedItems[0];

                var result = apiClient.OrderDelete(ticket);

                addToLog(string.Format("Delete order result: {0},  ticketId = {1}", result, ticket));
            }
        }

        private void orderExpiration()
        {
            var result = apiClient.OrderExpiration();
            addToLog(string.Format("Expiration order result: {0}", result));
        }

        private void orderLots()
        {
            var result = apiClient.OrderLots();
            addToLog(string.Format("Lots order result: {0}", result));
        }

        private void orderMagicNumber()
        {
            var result = apiClient.OrderMagicNumber();
            addToLog(string.Format("MagicNumber order result: {0}", result));
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

                var result = apiClient.OrderModify(ticket, price, stoploss, takeprofit, expiration, arrow_color);

                addToLog(string.Format("OrderModify result: {0}", result));
            }
        }

        private void orderOpenPrice()
        {
            var result = apiClient.OrderOpenPrice();
            addToLog(string.Format("OrderOpenPrice result: {0}", result));            
        }

        private void orderOpenTime()
        {
            var result = apiClient.OrderOpenTime();
            addToLog(string.Format("OpenTime order result: {0}", result));
        }

        private void orderPrint()
        {
            apiClient.OrderPrint();
        }

        private void orderProfit()
        {
            var result = apiClient.OrderProfit();
            addToLog(string.Format("Profit order result: {0}", result));
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
                var result = apiClient.OrderSelect(ticket, OrderSelectMode.SELECT_BY_POS);

                addToLog(string.Format("OrderSelect result: {0}", result));
            }
        }

        private void ordersHistoryTotal()
        {
            var result = apiClient.OrdersHistoryTotal();
            addToLog(string.Format("OrdersHistoryTotal result: {0}", result));
        }

        private void orderStopLoss()
        {
            var result = apiClient.OrderStopLoss();
            textBoxOrderStoploss.Text = result.ToString();
            addToLog(string.Format("OrderStopLoss result: {0}", result));
        }

        private void ordersTotal()
        {
            var result = apiClient.OrdersTotal();
            addToLog(string.Format("OrdersTotal result: {0}", result));
        }

        private void orderSwap()
        {
            var result = apiClient.OrderSwap();
            addToLog(string.Format("OrderSwap result: {0}", result));
        }

        private void orderSymbol()
        {
            var result = apiClient.OrderSymbol();
            textBoxOrderSymbol.Text = result;
            addToLog(string.Format("OrderSymbol result: {0}", result));
        }

        private void orderTakeProfit()
        {
            var result = apiClient.OrderTakeProfit();
            textBoxOrderProffit.Text = result.ToString();
            addToLog(string.Format("OrderTakeProfit result: {0}", result));
        }

        private void orderTicket()
        {
            var result = apiClient.OrderTicket();
            addToLog(string.Format("OrderTicket result: {0}", result));
        }

        private void orderType()
        {
            var result = apiClient.OrderType();
            comboBoxOrderCommand.SelectedIndex = (int)result;
            addToLog(string.Format("OrderType result: {0}", result));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GroupOrderCommands[comboBoxSelectedCommand.SelectedIndex]();
        }

        private void addToLog(string msg)
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
                        var result = apiClient.GetLastError();
                        addToLog(string.Format("GetLastError result: {0}", result));
                    }
                    break;
                case 1:
                    {
                        var result = apiClient.IsConnected();
                        addToLog(string.Format("IsConnected result: {0}", result));
                    }
                    break;
                case 2:
                    {
                        var result = apiClient.IsDemo();
                        addToLog(string.Format("IsDemo result: {0}", result));
                    }
                    break;
                case 3:
                    {
                        var result = apiClient.IsDllsAllowed();
                        addToLog(string.Format("IsDllsAllowed result: {0}", result));
                    }                    
                    break;
                case 4:
                    {
                        var result = apiClient.IsExpertEnabled();
                        addToLog(string.Format("IsExpertEnabled result: {0}", result));
                    }                    
                    break;
                case 5:
                    {
                        var result = apiClient.IsLibrariesAllowed();
                        addToLog(string.Format("IsLibrariesAllowed result: {0}", result));
                    }                    
                    break;
                case 6:
                    {
                        var result = apiClient.IsOptimization();
                        addToLog(string.Format("IsOptimization result: {0}", result));
                    }                    
                    break;
                case 7:
                    {
                        var result = apiClient.IsStopped();
                        addToLog(string.Format("IsStopped result: {0}", result));
                    }                    
                    break;
                case 8:
                    {
                        var result = apiClient.IsTesting();
                        addToLog(string.Format("IsTesting result: {0}", result));
                    }
                    break;
                case 9:
                    {
                        var result = apiClient.IsTradeAllowed();
                        addToLog(string.Format("IsTradeAllowed result: {0}", result));
                    }                    
                    break;
                case 10:
                    {
                        var result = apiClient.IsTradeContextBusy();
                        addToLog(string.Format("IsTradeContextBusy result: {0}", result));
                    }                    
                    break;
                case 11:
                    {
                        var result = apiClient.IsVisualMode();
                        addToLog(string.Format("IsVisualMode result: {0}", result));
                    }                    
                    break;
                case 12:
                    {
                        var result = apiClient.UninitializeReason();
                        addToLog(string.Format("UninitializeReason result: {0}", result));
                    }                    
                    break;
                case 13:
                    {
                        int errorCode = -1;
                        int.TryParse(textBoxErrorCode.Text, out errorCode);
                        var result = apiClient.ErrorDescription(errorCode);
                        addToLog(string.Format("ErrorDescription result: {0}", result));
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
                        var result = apiClient.AccountBalance();
                        addToLog(string.Format("AccountBalance result: {0}", result));
                    }
                    break;
                case 1:
                    {
                        var result = apiClient.AccountCredit();
                        addToLog(string.Format("AccountCredit result: {0}", result));
                    }
                    break;
                case 2:
                    {
                        var result = apiClient.AccountCompany();
                        addToLog(string.Format("AccountCompany result: {0}", result));
                    }
                    break;
                case 3:
                    {
                        var result = apiClient.AccountCurrency();
                        addToLog(string.Format("AccountCurrency result: {0}", result));
                    }
                    break;
                case 4:
                    {
                        var result = apiClient.AccountEquity();
                        addToLog(string.Format("AccountEquity result: {0}", result));
                    }
                    break;
                case 5:
                    {
                        var result = apiClient.AccountFreeMargin();
                        addToLog(string.Format("AccountFreeMargin result: {0}", result));
                    }
                    break;
                case 6:
                    {
                        var result = apiClient.AccountFreeMarginCheck(textBoxAccountInfoSymbol.Text, (TradeOperation)comboBoxAccountInfoCmd.SelectedIndex, int.Parse(textBoxAccountInfoVolume.Text));
                        addToLog(string.Format("AccountFreeMarginCheck result: {0}", result));
                    }
                    break;
                case 7:
                    {
                        var result = apiClient.AccountFreeMarginMode();
                        addToLog(string.Format("AccountFreeMarginMode result: {0}", result));
                    }
                    break;
                case 8:
                    {
                        var result = apiClient.AccountLeverage();
                        addToLog(string.Format("AccountLeverage result: {0}", result));
                    }
                    break;
                case 9:
                    {
                        var result = apiClient.AccountMargin();
                        addToLog(string.Format("AccountMargin result: {0}", result));
                    }
                    break;
                case 10:
                    {
                        var result = apiClient.AccountName();
                        addToLog(string.Format("AccountName result: {0}", result));
                    }
                    break;
                case 11:
                    {
                        var result = apiClient.AccountNumber();
                        addToLog(string.Format("AccountNumber result: {0}", result));
                    }
                    break;
                case 12:
                    {
                        var result = apiClient.AccountProfit();
                        addToLog(string.Format("AccountProfit result: {0}", result));
                    }
                    break;
                case 13:
                    {
                        var result = apiClient.AccountServer();
                        addToLog(string.Format("AccountServer result: {0}", result));
                    }
                    break;
                case 14:
                    {
                        var result = apiClient.AccountStopoutLevel();
                        addToLog(string.Format("AccountStopoutLevel result: {0}", result));
                    }
                    break;
                case 15:
                    {
                        var result = apiClient.AccountStopoutMode();
                        addToLog(string.Format("AccountStopoutMode result: {0}", result));
                    }
                    break;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (listBoxMarketInfo.SelectedIndex < 0)
                return;

            var result = apiClient.MarketInfo(txtMarketInfoSymbol.Text, (MarketInfoModeType)listBoxMarketInfo.SelectedIndex);
            addToLog(string.Format("MarketInfo result: {0}", result));
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
            //    var price = apiClient.iOpen(symbol, ChartPeriod.PERIOD_M1, i);
            //    openPriceList.Add(price);
            //}

            var prices = apiClient.iCloseArray(symbol, ChartPeriod.PERIOD_M1);

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
            var barCount = apiClient.iBars(symbol, ChartPeriod.PERIOD_M1);
            textBoxTimeframesCount.Text = barCount.ToString();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string symbol = textBoxSelectedSymbol.Text;
            var prices = apiClient.iHighArray(symbol, ChartPeriod.PERIOD_M1);
            var items = new List<double>(prices);
            listBoxProceHistory.DataSource = items;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            string symbol = textBoxSelectedSymbol.Text;
            var prices = apiClient.iLowArray(symbol, ChartPeriod.PERIOD_M1);
            var items = new List<double>(prices);
            listBoxProceHistory.DataSource = items;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            string symbol = textBoxSelectedSymbol.Text;
            var prices = apiClient.iOpenArray(symbol, ChartPeriod.PERIOD_M1);
            var items = new List<double>(prices);
            listBoxProceHistory.DataSource = items;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            string symbol = textBoxSelectedSymbol.Text;
            var prices = apiClient.iVolumeArray(symbol, ChartPeriod.PERIOD_M1);
            var items = new List<double>(prices);
            listBoxProceHistory.DataSource = items;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            string symbol = textBoxSelectedSymbol.Text;
            var times = apiClient.iTimeArray(symbol, ChartPeriod.PERIOD_M1);
            var items = new List<DateTime>(times);
            listBoxProceHistory.DataSource = items;
        }

        private void iCustomBtn_Click(object sender, EventArgs e)
        {
            string symbol = textBoxSelectedSymbol.Text;
            int[] param = {11, 12, 13};
            var retVal = apiClient.iCustom(symbol, (int)ChartPeriod.PERIOD_H1, "Zigzag", param, 1, 0);
            addToLog(string.Format("ICustom result: {0}", retVal));
        }

        private void button13_Click(object sender, EventArgs e)
        {
            var retVal = apiClient.TimeCurrent();
            addToLog(string.Format("TimeCurrent result: {0}", retVal));
        }

        private void button14_Click(object sender, EventArgs e)
        {
            var retVal = apiClient.TimeLocal();
            addToLog(string.Format("TimeLocal result: {0}", retVal));
        }

        private void buttonRefreshRates_Click(object sender, EventArgs e)
        {
            var retVal = apiClient.RefreshRates();
            addToLog(string.Format("RefreshRates result: {0}", retVal));
        }

        private void button15_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < 4; i++)
            {
                var ticket = i;
                Task.Factory.StartNew(() =>
                {
                    MtOrder order = null;
                    try
                    {
                        order = apiClient.GetOrder(ticket, OrderSelectMode.SELECT_BY_POS, OrderSelectSource.MODE_TRADES);
                    }
                    catch (MtConnectionException ex)
                    {
                        addToLog("MtExecutionException: " + ex.Message);
                    }
                    catch (MtExecutionException ex)
                    {
                        addToLog("MtExecutionException: " + ex.Message);
                    }

                    string result;
                    if (order != null)
                    {
                        result =
                            string.Format(
                                "Order: Ticket = {0}, Symbol = {1}, Operation = {2}, OpenPrice = {3}, ClosePrice = {4}, Lots = {5}, Profit = {6}, Comment = {7}, Commission = {8}, MagicNumber = {9}, OpenTime = {10}, CloseTime = {11}",
                                order.Ticket, order.Symbol, order.Operation, order.OpenPrice, order.ClosePrice, order.Lots, order.Profit, order.Comment, order.Commission, order.MagicNumber, order.OpenTime, order.CloseTime);
                    }
                    else
                    {
                        result = "Order is null";
                    }

                    addToLog(result);
                });
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            var ticket = int.Parse(textBoxIndexTicket.Text);
            var selectMode = (OrderSelectMode) comboBox1.SelectedIndex;
            var selectSource = (OrderSelectSource) comboBox2.SelectedIndex;

            MtOrder order = null;
            try
            {
                order = apiClient.GetOrder(ticket, selectMode, selectSource);
            }
            catch (MtConnectionException ex)
            {
                addToLog("MtExecutionException: " + ex.Message);
                return;
            }
            catch (MtExecutionException ex)
            {                
                addToLog("MtExecutionException: " + ex.Message + "; ErrorCode = " + ex.ErrorCode);
                return;
            }

            if (order == null)
            {
                addToLog("Order is null");
                return;
            }

            var result =
                string.Format(
                    "Order: Ticket = {0}, Symbol = {1}, Operation = {2}, OpenPrice = {3}, ClosePrice = {4}, Lots = {5}, Profit = {6}, Comment = {7}, Commission = {8}, MagicNumber = {9}, OpenTime = {10}, CloseTime = {11}",
                    order.Ticket, order.Symbol, order.Operation, order.OpenPrice, order.ClosePrice, order.Lots, order.Profit, order.Comment, order.Commission, order.MagicNumber, order.OpenTime, order.CloseTime);
            addToLog(result);
        }

        private void button17_Click(object sender, EventArgs e)
        {
            var selectSource = (OrderSelectSource)comboBox2.SelectedIndex;


            IEnumerable<MtOrder> orders = null;
            try
            {
                orders = apiClient.GetOrders(selectSource);
            }
            catch (MtConnectionException ex)
            {
                addToLog("MtExecutionException: " + ex.Message);
                return;
            }
            catch (MtExecutionException ex)
            {
                addToLog("MtExecutionException: " + ex.Message + "; ErrorCode = " + ex.ErrorCode);
                return;
            }

            if (orders == null)
            {
                addToLog("Orders is null");
                return;

            }

            foreach (var order in orders)
            {
                var result =
                    string.Format(
                        "Order: Ticket = {0}, Symbol = {1}, Operation = {2}, OpenPrice = {3}, ClosePrice = {4}, Lots = {5}, Profit = {6}, Comment = {7}, Commission = {8}, MagicNumber = {9}, OpenTime = {10}, CloseTime = {11}",
                        order.Ticket, order.Symbol, order.Operation, order.OpenPrice, order.ClosePrice, order.Lots, order.Profit, order.Comment, order.Commission, order.MagicNumber, order.OpenTime, order.CloseTime);
                addToLog(result);                
            }            
        }

        private void listBoxEventLog_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            listBoxEventLog.Items.Clear();
        }
    }
}
