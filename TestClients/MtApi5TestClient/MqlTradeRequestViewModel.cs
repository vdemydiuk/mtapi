﻿using System;
using System.ComponentModel;
using MtApi5;

namespace MtApi5TestClient
{
    public class MqlTradeRequestViewModel : INotifyPropertyChanged
    {
        public MqlTradeRequestViewModel(MqlTradeRequest reqest)
        {
            TradeRequest = reqest ?? throw new ArgumentNullException();
        }

        private MqlTradeRequest TradeRequest { get; set; }

        public ENUM_TRADE_REQUEST_ACTIONS Action 
        {
            get { return TradeRequest.Action; }
            set 
            {
                TradeRequest.Action = value;
                OnPropertyChanged("Action");
            }
        }

        public ulong Magic
        {
            get { return TradeRequest.Magic; }
            set
            {
                TradeRequest.Magic = value;
                OnPropertyChanged("Magic");
            }
        }

        public ulong Order
        {
            get { return TradeRequest.Order; }
            set
            {
                TradeRequest.Order = value;
                OnPropertyChanged("Order");
            }
        }

        public string Symbol
        {
            get { return TradeRequest.Symbol; }
            set
            {
                TradeRequest.Symbol = value;
                OnPropertyChanged("Symbol");
            }
        }

        public double Volume
        {
            get { return TradeRequest.Volume; }
            set
            {
                TradeRequest.Volume = value;
                OnPropertyChanged("Volume");
            }
        }

        public double Price
        {
            get { return TradeRequest.Price; }
            set
            {
                TradeRequest.Price = value;
                OnPropertyChanged("Price");
            }
        }

        public double Stoplimit
        {
            get { return TradeRequest.Stoplimit; }
            set
            {
                TradeRequest.Stoplimit = value;
                OnPropertyChanged("Stoplimit");
            }
        }

        public double Sl
        {
            get { return TradeRequest.Sl; }
            set
            {
                TradeRequest.Sl = value;
                OnPropertyChanged("Sl");
            }
        }

        public double Tp
        {
            get { return TradeRequest.Tp; }
            set
            {
                TradeRequest.Tp = value;
                OnPropertyChanged("Tp");
            }
        }

        public ulong Deviation
        {
            get { return TradeRequest.Deviation; }
            set
            {
                TradeRequest.Deviation = value;
                OnPropertyChanged("Deviation");
            }
        }

        public ENUM_ORDER_TYPE Type
        {
            get { return TradeRequest.Type; }
            set
            {
                TradeRequest.Type = value;
                OnPropertyChanged("Type");
            }
        }

        public ENUM_ORDER_TYPE_FILLING Type_filling
        {
            get { return TradeRequest.Type_filling; }
            set
            {
                TradeRequest.Type_filling = value;
                OnPropertyChanged("Type_filling");
            }
        }

        public ENUM_ORDER_TYPE_TIME Type_time
        {
            get { return TradeRequest.Type_time; }
            set
            {
                TradeRequest.Type_time = value;
                OnPropertyChanged("Type_time");
            }
        }

        public DateTime Expiration
        {
            get { return TradeRequest.Expiration; }
            set
            {
                TradeRequest.Expiration = value;
                OnPropertyChanged("Expiration");
            }
        }

        public string Comment
        {
            get { return TradeRequest.Comment; }
            set
            {
                TradeRequest.Comment = value;
                OnPropertyChanged("Comment");
            }
        }

        public MqlTradeRequest GetMqlTradeRequest()
        {
            return TradeRequest;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
