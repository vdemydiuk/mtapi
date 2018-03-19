using System.ComponentModel;

namespace MtApi5TestClient
{
    public class QuoteViewModel: INotifyPropertyChanged
    {
        #region Properties
        
        public string Instrument { get; }

        private double _bid;
        public double Bid
        {
            get { return _bid; }
            set
            {
                _bid = value;
                OnPropertyChanged("Bid");
            }
        }

        private double _ask;
        public double Ask
        {
            get { return _ask; }
            set
            {
                _ask = value;
                OnPropertyChanged("Ask");
            }
        }

        private int _expertHandle;
        public int ExpertHandle
        {
            get { return _expertHandle; }
            set
            {
                _expertHandle = value;
                OnPropertyChanged("ExpertHandle");
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
