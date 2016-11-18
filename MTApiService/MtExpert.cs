using System;
using log4net;

namespace MTApiService
{
    public class MtExpert
    {
        public delegate void MtQuoteHandler(MtExpert expert, MtQuote quote);
        public delegate void MtEventHandler(MtExpert expert, MtEvent e);

        #region Private Fields
        private static readonly ILog Log = LogManager.GetLogger(typeof(MtExpert));

        private readonly IMetaTraderHandler _mtHadler;
        private MtCommandTask _commandTask;
        private ICommandManager _commandManager;
        private readonly object _locker = new object();
        #endregion

        #region Public Methods
        public MtExpert(int handle, MtQuote quote, IMetaTraderHandler mtHandler)
        {
            if (mtHandler == null)
                throw new ArgumentNullException(nameof(mtHandler));

            Quote = quote;
            Handle = handle;
            _mtHadler = mtHandler;
        }

        public void Deinit()
        {
            Log.Debug("Deinit: begin.");

            IsEnable = false;
            FireOnDeinited();

            Log.Debug("Deinit: end.");
        }

        public void SendResponse(MtResponse response)
        {
            Log.DebugFormat("SendResponse: begin. response = {0}", response);

            _commandTask.SetResult(response);
            _commandTask = null;
            FireOnCommandExecuted();

            Log.Debug("SendResponse: end.");
        }

        public int GetCommandType()
        {
            Log.Debug("GetCommandType: called.");

            var commandManager = CommandManager;
            if (commandManager != null)
            {
                _commandTask = commandManager.DequeueCommandTask();
            }

            return _commandTask?.Command?.CommandType ?? 0;
        }

        public object GetCommandParameter(int index)
        {
            Log.DebugFormat("GetCommandType: called. index = {0}", index);

            var command = _commandTask?.Command;
            if (command?.Parameters != null && index >= 0 && index < command.Parameters.Count)
            {
                return command.Parameters[index];
            }

            return null;
        }

        public void SendEvent(MtEvent mtEvent)
        {
            Log.DebugFormat("SendEvent: begin. event = {0}", mtEvent);

            FireOnMtEvent(mtEvent);

            Log.Debug("SendEvent: end.");
        }

        public override string ToString()
        {
            return $"ExpertHandle = {Handle}";
        }

        #endregion

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
                lock (_locker)
                {
                    _quote = value;
                }

                FireOnQuoteChanged(value);
            }
        }

        public int Handle { get; }

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

        #region IMtCommandExecutor
        public void NotifyCommandReady()
        {
            Log.Debug("NotifyCommandReady: begin.");

            SendTickToMetaTrader();

            Log.Debug("NotifyCommandReady: end.");
        }
        #endregion

        #region Private Methods
        private void SendTickToMetaTrader()
        {
            _mtHadler.SendTickToMetaTrader(Handle);
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
            OnMtEvent?.Invoke(this, mtEvent);
        }
        #endregion

        #region Events
        public event EventHandler Deinited;
        public event MtQuoteHandler QuoteChanged;
        public event EventHandler CommandExecuted;
        public event MtEventHandler OnMtEvent;
        #endregion
    }
}
