using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestServer
{
    enum MtExpertType
    {
        MtApiSymbol,
        MtApiControl,
        MtQuoteExpert
    }

    abstract class MtExpert
    {
        public MtExpert(MtExpertType expertType, IMetaTrader metatrader)
        {
            ExpertType = expertType;
            mMetaTrader = metatrader;
        }

        #region Public Methods

        public void Init()
        {
            Action a = () =>
                {
                    mIsStartFuncBlocked = true;
                    
                    init();

                    mIsStartFuncBlocked = false;
                };
            a.BeginInvoke(null, null);
        }

        public void Deinit()
        {
            Action a = () =>
            {
                deinit();
            };
            a.BeginInvoke(null, null);
        }

        public void Start()
        {
            Action a = () =>
            {
                if (mIsStartFuncBlocked == true)
                    return;

                mIsStartFuncBlocked = true;

                start();

                mIsStartFuncBlocked = false;

            };
            a.BeginInvoke(null, null);
        }

        public void SetStopped()
        {
            mIsStopped = true;
        }

        public bool IsStopped()
        {
            return mIsStopped;
        }
        #endregion

        #region Expert Methods
        protected abstract void init();
        protected abstract void deinit();
        protected abstract void start();
        #endregion

        #region MetaTrader Functions
        protected void Print(string msg)
        {
            if (mMetaTrader != null)
                mMetaTrader.Print(msg);
        }

        protected void MessageBoxA(string msg)
        {
            if (mMetaTrader != null)
                mMetaTrader.MessageBoxA(msg);
        }

        protected string AccountName()
        {
            return (mMetaTrader != null) ? mMetaTrader.AccountName() : string.Empty;
        }

        protected int AccountNumber()
        {
            return (mMetaTrader != null) ? mMetaTrader.AccountNumber() : -1;
        }

        protected bool IsDemo()
        {
            return (mMetaTrader != null) ? mMetaTrader.IsDemo() : false;
        }

        public bool IsConnected()
        {
            return (mMetaTrader != null) ? mMetaTrader.IsConnected() : false;
        }

        protected int OrderSend(string symbol, int cmd, double volume, double price, int slippage, double stoploss, double takeprofit
            , string comment, int magic, int expiration, int arrow_color)
        {
            return (mMetaTrader != null) ? mMetaTrader.OrderSend(symbol, cmd, volume, price, slippage, stoploss, takeprofit, comment, magic, expiration, arrow_color) : -1;
        }

        protected bool OrderClose(int ticket, double lots, double price, int slippage, int color)
        {
            return (mMetaTrader != null) ? mMetaTrader.OrderClose(ticket, lots, price, slippage, color) : false;
        }

        protected string ErrorDescription(int errorCode)
        {
            return (mMetaTrader != null) ? mMetaTrader.ErrorDescription(errorCode) : string.Empty;
        }
        #endregion

        #region Properties
        public MtExpertType ExpertType { get; private set; }

        public string Symbol { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
        #endregion

        #region Fields
        private readonly IMetaTrader mMetaTrader;
        private volatile bool mIsStopped = false;
        private volatile bool mIsStartFuncBlocked = false;
        #endregion
    }
}
