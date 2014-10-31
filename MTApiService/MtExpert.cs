using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTApiService
{
    public class MtExpert
    {
        public delegate void MtQuoteHandler(MtExpert expert, MtQuote quote);

        #region Properties
        private MtQuote _Quote;
        public MtQuote Quote 
        {
            get
            {
                lock (_locker)
                {
                    return _Quote;                         
                }
            }
            set
            {
                lock(_locker)
                {
                    _Quote = value;
                }

                if (QuoteChanged != null)
                {
                    QuoteChanged(this, value);
                }                
            }
        }
        public int Handle { get; private set; }

        private volatile bool _IsEnable = true;
        public bool IsEnable
        {
            get { return _IsEnable; }
            private set { _IsEnable = value; }
        }

        private volatile bool _IsCommandExecutor = true;
        public bool IsCommandExecutor
        {
            get { return _IsCommandExecutor; }
            set { _IsCommandExecutor = value; }
        }

        public ICommandManager CommandManager
        {
            private get
            {
                lock (_locker)
                {
                    return mCommandManager;
                }
            }
            set
            {
                lock (_locker)
                {
                    mCommandManager = value;
                }
            }
        }
        #endregion

        #region Public Methods
        public MtExpert(int handle, MtQuote quote, IMetaTraderHandler mtHandler)
        {
            Quote = quote;
            Handle = handle;
            mMtHadler = mtHandler;
        }

        public void Deinit()
        {
            IsEnable = false;

            if (Deinited != null)
            {
                Deinited(this, EventArgs.Empty);
            }
        }

        public void SendResponse(MtResponse response)
        {
            MtCommand command = mCommand;
            mCommand = null;

            ICommandManager commandManager = CommandManager;
            if (commandManager != null)
            {
                commandManager.OnCommandExecuted(this, command, response);
            }
        }

        public int GetCommandType()
        {
            if (IsCommandExecutor)
            {
                ICommandManager commandManager = CommandManager;
                if (mCommandManager != null)
                {
                    mCommand = mCommandManager.DequeueCommand();
                }
            }

            return mCommand != null ? mCommand.CommandType : 0;
        }

        public object GetCommandParameter(int index)
        {
            if (mCommand != null && mCommand.Parameters != null 
                && index >= 0 && index < mCommand.Parameters.Count)
            {
                return mCommand.Parameters[index];
            }

            return null;
        }
        #endregion

        #region IMtCommandExecutor
        public void NotifyCommandReady()
        {
            SendTickToMetaTrader();
        }
        #endregion


        #region Private Methods
        private void SendTickToMetaTrader()
        {
            if (mMtHadler != null)
            {
                mMtHadler.SendTickToMetaTrader(Handle);
            }
        }
        #endregion

        #region Events
        public event EventHandler Deinited;
        public event MtQuoteHandler QuoteChanged;
        #endregion

        #region Private Fields
        private readonly IMetaTraderHandler mMtHadler;
        private MtCommand mCommand;
        private ICommandManager mCommandManager;
        private readonly object _locker = new object();
        #endregion
    }
}
