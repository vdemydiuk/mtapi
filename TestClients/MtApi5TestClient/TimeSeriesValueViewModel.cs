using MtApi5;
using System.ComponentModel;

namespace MtApi5TestClient
{
    public class TimeSeriesValueViewModel : INotifyPropertyChanged
    {
        private string _symbolValue;
        public string SymbolValue 
        {
            get { return _symbolValue; }
            set
            {
                _symbolValue = value;
                OnPropertyChanged("SymbolValue");
            }
        }

        public ENUM_TIMEFRAMES TimeFrame { get; set; }
        public int StartPos { get; set; }
        public int Count { get; set; }


        private int _indicatorHandle;
        public int IndicatorHandle
        {
            get { return _indicatorHandle; }
            set
            {
                _indicatorHandle = value;
                OnPropertyChanged("IndicatorHandle");
            }
        }

        private ENUM_INDICATOR _indicatorType = ENUM_INDICATOR.IND_MACD;
        public ENUM_INDICATOR IndicatorType
        {
            get { return _indicatorType; }
            set
            {
                _indicatorType = value;
                OnPropertyChanged("IndicatorType");
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
