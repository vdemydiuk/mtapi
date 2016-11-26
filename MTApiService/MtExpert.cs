using System;
using log4net;
using System.Collections.Generic;

namespace MTApiService
{
    internal class MtExpert: ITaskExecutor
    {
        public delegate void MtQuoteHandler(MtExpert expert, MtQuote quote);
        public delegate void MtEventHandler(MtExpert expert, MtEvent e);

        #region Private Fields
        private static readonly ILog Log = LogManager.GetLogger(typeof(MtExpert));

        private readonly IMetaTraderHandler _mtHadler;
        private MtCommandTask _currentTask;
        private readonly Queue<MtCommandTask> _taskQueue = new Queue<MtCommandTask>();
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

            _currentTask.SetResult(response);
            _currentTask = null;

            Log.Debug("SendResponse: end.");
        }

        public int GetCommandType()
        {
            Log.Debug("GetCommandType: called.");

            _currentTask = DequeueTask();

            return _currentTask?.Command?.CommandType ?? 0;
        }

        public object GetCommandParameter(int index)
        {
            Log.DebugFormat("GetCommandType: called. index = {0}", index);

            var command = _currentTask?.Command;
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

        #region ITaskExecutor

        public void Execute(MtCommandTask task)
        {
            lock (_taskQueue)
            {
                _taskQueue.Enqueue(task);
            }

            NotifyCommandReady();
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

        #endregion

        #region Private Methods
        private MtCommandTask DequeueTask()
        {
            Log.Debug("DequeueTask: called.");

            MtCommandTask task;
            int count;

            lock (_locker)
            {
                count = _taskQueue.Count;
                task = _taskQueue.Count > 0 ? _taskQueue.Dequeue() : null;
            }

            Log.DebugFormat("DequeueTask: end. left task count = {0}.", count);

            return task;
        }

        private void NotifyCommandReady()
        {
            Log.Debug("NotifyCommandReady: begin.");

            _mtHadler.SendTickToMetaTrader(Handle);

            Log.Debug("NotifyCommandReady: end.");
        }

        private void FireOnQuoteChanged(MtQuote quote)
        {
            QuoteChanged?.Invoke(this, quote);
        }

        private void FireOnDeinited()
        {
            Deinited?.Invoke(this, EventArgs.Empty);
        }

        private void FireOnMtEvent(MtEvent mtEvent)
        {
            OnMtEvent?.Invoke(this, mtEvent);
        }
        #endregion

        #region Events
        public event EventHandler Deinited;
        public event MtQuoteHandler QuoteChanged;
        public event MtEventHandler OnMtEvent;
        #endregion
    }
}
