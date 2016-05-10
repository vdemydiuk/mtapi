using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MtApi5;
using System.ComponentModel;

namespace MtApi5TestClient
{
    public class TimeSeriesValueViewModel : INotifyPropertyChanged
    {
        private string _SymbolValue;
        public string SymbolValue 
        {
            get { return _SymbolValue; }
            set
            {
                _SymbolValue = value;
                OnPropertyChanged("SymbolValue");
            }
        }

        public ENUM_TIMEFRAMES TimeFrame { get; set; }
        public int StartPos { get; set; }
        public int Count { get; set; }

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
