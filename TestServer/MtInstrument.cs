using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace TestServer
{
    public delegate void InstrumentEventHandler(string symbol, double bid, double ask);

    public class MtInstrument
    {
        public MtInstrument(string symbol, double startBid, double startAsk)
        {
            _symbol = symbol;
            _bid = startBid;
            _ask = startAsk;
        }

        private void work()
        {
            Debug.WriteLine(string.Format("[INFO] MetaTrader Instrument {0} is runned.", _symbol));

            while (_isRunning)
            {
                if (_trendCycle <= 0)
                {
                    Random randomCicle = new Random();
                    _trendCycle = randomCicle.Next(1, 10);

                    Random randomValue = new Random();
                    int tmpValue = randomValue.Next(1, 100);
                    _trend = (tmpValue > 50) ? 1 : -1;
                }

                Random randomCount = new Random();
                int pipsCount = randomCount.Next(1, 4);
                double change = (_trend * pipsCount * _pip);

                lock (_lockPrice)
                {
                    _bid += change;
                    _ask += change;
                }

                if (InstrumentUpdate != null)
                    InstrumentUpdate(_symbol, _bid, _ask);

                _trendCycle--;

                Random randomSleepValue = new Random();
                int sleepValue = randomSleepValue.Next(200, 500);
                Thread.Sleep(sleepValue);
            }

            Debug.WriteLine(string.Format("[INFO] MetaTrader Instrument {0} is stopepd.", _symbol));
        }

        public void Start()
        {
            _isRunning = true;

            Thread thread = new Thread(work);
            thread.Name = "Instrument_" + Symbol;

            thread.Start();
        }

        public void Stop()
        {
            _isRunning = false;
        }

        public double Bid
        {
            get 
            {
                lock (_lockPrice)
                {
                    return _bid; 
                }                
            }
        }

        public double Ask
        {
            get
            {
                lock (_lockPrice)
                {
                    return _ask;
                }
            }
        }

        public string Symbol
        {
            get
            {
                lock (_lockPrice)
                {
                    return _symbol;
                }
            }
        }

        #region Events

        public event InstrumentEventHandler InstrumentUpdate;

        #endregion

        #region Fields

        private volatile bool _isRunning = false;
        private int _trend = 1;
        private int _trendCycle = 0;

        private const double _pip = 0.0001;

        private object _lockPrice = new object();
        private double _bid;
        private double _ask;
        private string _symbol = string.Empty;

        #endregion
    }
}
