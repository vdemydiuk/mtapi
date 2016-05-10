using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace MtApi5TestClient
{
    public class QuoteViewModel: INotifyPropertyChanged
    {
        #region Properties
        
        public string Instrument { get; private set; }

        private double _Bid;
        public double Bid
        {
            get { return _Bid; }
            set
            {
                _Bid = value;
                OnPropertyChanged("Bid");
            }
        }

        private double _Ask;
        public double Ask
        {
            get { return _Ask; }
            set
            {
                _Ask = value;
                OnPropertyChanged("Ask");
            }
        }

        private int _FeedCount = 0;
        public int FeedCount
        {
            get { return _FeedCount; }
            set
            {
                _FeedCount = value;
                OnPropertyChanged("FeedCount");
            }
        }

        #endregion

        #region Public Methods
        public QuoteViewModel(string instrument)
        {
            Instrument = instrument;
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
    }
}
