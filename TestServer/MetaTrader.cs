using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TestServer
{
    delegate void MetaTraderInfoHandler(string msg);

    class MetaTrader: IMetaTrader, IDisposable
    {
        public MetaTrader(string accountName, int accountNumber)
        {
            mAccountName = accountName != null ? accountName : string.Empty;
            mAccountNumber = accountNumber;
        }

        #region IMetaTrader
        public void Print(string msg)
        {
            updateMetaTraderInfo(msg);
        }

        public void MessageBoxA(string msg)
        {
            updateMetaTraderInfo("[MessageBox] " + msg);
        }

        public string AccountName()
        {
            var msg = string.Format("MetaTrader: Commmand - AccountName");
            updateMetaTraderInfo(msg);

            return mAccountName;
        }

        public int AccountNumber()
        {
            var msg = string.Format("MetaTrader: Commmand - AccountNumber");
            updateMetaTraderInfo(msg);

            return mAccountNumber;
        }

        public bool IsDemo()
        {
            var msg = string.Format("MetaTrader: Commmand - IsDemo");
            updateMetaTraderInfo(msg);

            return mIsDemo;
        }

        public bool IsConnected()
        {
            var msg = string.Format("MetaTrader: Commmand - IsConnected");
            updateMetaTraderInfo(msg);

            return true;
        }

        public int OrderSend(string symbol, int cmd, double volume, double price, int slippage, double stoploss, double takeprofit
            , string comment, int magic, int expiration, int arrow_color)
        {
            var msg = string.Format("MetaTrader: Commmand - OrderSend: cmd = {0}, symbol = {1}, volume = {2}, price = {3}, slippage = {4}, stoploss = {5}, takeprofit = {6}, comment = {7}, magic = {8}, expiration = {9}, arrow_color = {10}"
                            , cmd, symbol, volume, price, slippage, stoploss, takeprofit, comment, magic, expiration, arrow_color);
            updateMetaTraderInfo(msg);

            Thread.Sleep(5000);

            return ++mOrderCount;
        }

        public bool OrderClose(int ticket, double lots, double price, int slippage, int color)
        {
            var msg = string.Format("MetaTrader: Commmand - OrderClose, ticket = {0}, lots = {1}, price = {2}, slippage = {3}", ticket, lots, price, slippage);
            updateMetaTraderInfo(msg);

            return true;
        }

        public string ErrorDescription(int errorCode)
        {
            var msg = string.Format("MetaTrader: Commmand - ErrorDescription: errorCode = {0}", errorCode);
            updateMetaTraderInfo(msg);

            CodeErrorTypes errorType = (CodeErrorTypes)errorCode;

            return errorType.ToString();
        }

        #endregion

        #region Public Methods
        public void Start()
        {
            foreach(var instrument in mInstruments)
            {
                instrument.Start();
            }

            mIsStarted = true;
        }

        public void Stop()
        {
            mIsStarted = false;

            foreach (var chart in mInstrumentCharts)
            {
                chart.Stop();
            }

            foreach(var instrument in mInstruments)
            {
                instrument.Stop();
            }
        }

        public void AddInstrument(MtInstrument instrument)
        {
            if (instrument != null && mInstruments.Contains(instrument) == false)
            {
                mInstruments.Add(instrument);
                instrument.InstrumentUpdate += instrument_InstrumentUpdate;

                if (mIsStarted == true)
                    instrument.Start();
            }
        }

        public void RemoveInstrument(MtInstrument instrument)
        {
            if (instrument != null && mInstruments.Contains(instrument) == true)
            {
                mInstruments.Remove(instrument);
                instrument.InstrumentUpdate -= instrument_InstrumentUpdate;

                instrument.Stop();
            }
        }

        public void AddInstrumentChart(MtInstrumentChart instrumentChart)
        {
            if (instrumentChart != null && mInstrumentCharts.Contains(instrumentChart) == false)
            {
                mInstrumentCharts.Add(instrumentChart);

                instrumentChart.Start();
            }
        }

        public void RemoveInstrumentChart(MtInstrumentChart instrumentChart)
        {
            if (instrumentChart != null && mInstrumentCharts.Contains(instrumentChart) == true)
            {
                mInstrumentCharts.Remove(instrumentChart);

                instrumentChart.Stop();
            }
        }

        public void SetDemo(bool isDemo)
        {
            mIsDemo = isDemo;
        }
        #endregion

        #region Properties
        
        public List<MtInstrument> Instruments
        {
            get { return mInstruments; }
        }


        public List<MtInstrumentChart> InstrumentCharts
        {
            get { return mInstrumentCharts; }
        }

        #endregion

        #region Private Methods
        void instrument_InstrumentUpdate(string symbol, double bid, double ask)
        {
            
        }

        void updateMetaTraderInfo(string msg)
        {
            if (InfoUpdated != null)
                InfoUpdated(msg);
        }

        #endregion

        #region Events
        public event MetaTraderInfoHandler InfoUpdated;
        #endregion

        #region Fields

        private readonly string mAccountName;
        private readonly int mAccountNumber;

        private int mOrderCount = 0;

        private readonly List<MtInstrument> mInstruments = new List<MtInstrument>();
        private readonly List<MtInstrumentChart> mInstrumentCharts = new List<MtInstrumentChart>();

        private bool mIsStarted = false;

        private static List<string> OpTypes = new List<string> { "OP_BUY", "OP_SELL", "OP_BUYLIMIT", "OP_SELLLIMIT", "OP_BUYSTOP", "OP_SELLSTOP" };

        private bool mIsDemo = true;

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            
        }

        #endregion
    }
}
