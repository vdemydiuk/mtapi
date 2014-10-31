using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestServer
{
    class MtInstrumentChart
    {
        public MtInstrumentChart(MtInstrument instrument)
        {
            Instrument = instrument;
        }

        #region Public Methods
        public void Start()
        {
            lock (mExpertLocker)
            {
                if (Expert != null)
                {
                    if (Instrument != null)
                        Expert.Symbol = Instrument.Symbol;

                    Expert.Init();
                }
            }

            if (Instrument != null)
            {
                Instrument.InstrumentUpdate += Instrument_InstrumentUpdate;
            }
        }

        public void Stop()
        {
            if (Instrument != null)
            {
                Instrument.InstrumentUpdate -= Instrument_InstrumentUpdate;
            }

            lock (mExpertLocker)
            {
                if (Expert != null)
                {
                    Expert.SetStopped();
                    Expert.Deinit();
                }
            }
        }

        public void AddExpert(MtExpert expert)
        {
            lock (mExpertLocker)
            {
                Expert = expert;

                if (Expert != null)
                {
                    if (Instrument != null)
                        Expert.Symbol = Instrument.Symbol;

                    Expert.Init();
                }
            }
        }
        #endregion

        #region Private Methods

        private void Instrument_InstrumentUpdate(string symbol, double bid, double ask)
        {
            lock (mExpertLocker)
            {
                if (Expert != null)
                {
                    Expert.Symbol = symbol;
                    Expert.Bid = bid;
                    Expert.Ask = ask;

                    Expert.Start();
                }
            }
        }
        #endregion

        #region Properties
        public MtInstrument Instrument { get; private set; }
        public MtExpert Expert { get; private set; }

        private readonly object mExpertLocker = new object();
        #endregion
    }
}
