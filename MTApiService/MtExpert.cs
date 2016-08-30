using System;

namespace MTApiService
{
    public class MtExpert
    {
        public delegate void MtQuoteHandler(MtExpert expert, MtQuote quote);

        #region Properties
        private MtQuote _quote;
        public MtQuote Quote 
        {
            get
            {
                lock (_locker)
                {
                    return _quote;                         
                }
            }
            set
            {
                lock(_locker)
                {
                    _quote = value;
                }

                FireOnQuoteChanged(value);
            }
        }

        public int Handle { get; private set; }

        private bool _isEnable = true;
        public bool IsEnable
        {
            get
            {
                lock (_locker)
                {
                    return _isEnable;
                }
            }
            private set
            {
                lock (_locker)
                {
                    _isEnable = value;
                }
            }
        }

        public ICommandManager CommandManager
        {
            private get
            {
                lock (_locker)
                {
                    return _commandManager;
                }
            }
            set
            {
                lock (_locker)
                {
                    _commandManager = value;
                }
            }
        }

        #endregion

        #region Public Methods
        public MtExpert(int handle, MtQuote quote, IMetaTraderHandler mtHandler)
        {
            Quote = quote;
            Handle = handle;
            _mtHadler = mtHandler;
        }

        public void Deinit()
        {
            IsEnable = false;
            FireOnDeinited();
        }

        public void SendResponse(MtResponse response)
        {
            _commandTask.SetResult(response);
            _commandTask = null;
            FireOnCommandExecuted();
        }

        public int GetCommandType()
        {
            var commandManager = CommandManager;
            if (commandManager != null)
            {
                _commandTask = commandManager.DequeueCommandTask();
            }

            return _commandTask != null && _commandTask.Command != null ? _commandTask.Command.CommandType : 0;
        }

        public object GetCommandParameter(int index)
        {
            if (_commandTask != null)
            {
                var command = _commandTask.Command;
                if (command != null && command.Parameters != null
                    && index >= 0 && index < command.Parameters.Count)
                {
                    return command.Parameters[index];
                }
            }

            return null;
        }

        public void SendEvent(MtEvent mtEvent)
        {
            FireOnMtEvent(mtEvent);
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
            if (_mtHadler != null)
            {
                _mtHadler.SendTickToMetaTrader(Handle);
            }
        }

        private void FireOnQuoteChanged(MtQuote quote)
        {
            QuoteChanged?.Invoke(this, quote);
        }

        private void FireOnDeinited()
        {
            Deinited?.Invoke(this, EventArgs.Empty);
        }

        private void FireOnCommandExecuted()
        {
            CommandExecuted?.Invoke(this, EventArgs.Empty);
        }

        private void FireOnMtEvent(MtEvent mtEvent)
        {
            OnMtEvent?.Invoke(this, new MtEventArgs(mtEvent));
        }
        #endregion

        #region Events
        public event EventHandler Deinited;
        public event MtQuoteHandler QuoteChanged;
        public event EventHandler CommandExecuted;
        public event EventHandler<MtEventArgs> OnMtEvent;
        #endregion

        #region Private Fields
        private readonly IMetaTraderHandler _mtHadler;
        private MtCommandTask _commandTask;
        private ICommandManager _commandManager;
        private readonly object _locker = new object();
        #endregion
    }
}
