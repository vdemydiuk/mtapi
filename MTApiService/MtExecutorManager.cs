using System;
using System.Collections.Generic;
using log4net;

namespace MTApiService
{
    internal class MtCommandExecutorManager : ICommandManager
    {
        #region Private Fields
        private static readonly ILog Log = LogManager.GetLogger(typeof(MtCommandExecutorManager));

        private readonly List<MtExpert> _commandExecutors = new List<MtExpert>();
        private readonly Queue<MtCommandTask> _commandTasks = new Queue<MtCommandTask>();

        private readonly object _locker = new object();
        #endregion

        #region Public Methods

        public void Stop()
        {
            Log.Debug("Stop: begin.");

            lock (_locker)
            {
                _commandExecutors.Clear();
                _commandTasks.Clear();
            }

            Log.Debug("Stop: end.");
        }

        public void AddCommandExecutor(MtExpert commandExecutor)
        {
            if (commandExecutor == null)
                throw new ArgumentNullException(nameof(commandExecutor));

            Log.DebugFormat("AddCommandExecutor: begin. commandExecutor = {0}", commandExecutor);

            var notify = false;
            lock (_locker)
            {
                if (_commandExecutors.Contains(commandExecutor))
                {
                    Log.Warn("AddCommandExecutor: end. Command executor already exist.");
                    return;
                }

                _commandExecutors.Add(commandExecutor);
                if (_commandTasks.Count > 0)
                {
                    notify = true;
                }
            }

            commandExecutor.CommandExecuted += CommandExecutor_CommandExecuted;
            commandExecutor.CommandManager = this;

            if (notify)
            {
                NotifyCommandReady();
            }

            Log.Debug("AddCommandExecutor: end.");
        }

        public void RemoveCommandExecutor(MtExpert commandExecutor)
        {
            if (commandExecutor == null)
                throw new ArgumentNullException(nameof(commandExecutor));

            Log.DebugFormat("RemoveCommandExecutor: begin. commandExecutor = {0}", commandExecutor);

            var notify = false;
            lock (_locker)
            {
                if (_commandExecutors.Contains(commandExecutor) == false)
                {
                    Log.Warn("RemoveCommandExecutor: end. Command executor is not exist in collection.");
                    return;
                }

                _commandExecutors.Remove(commandExecutor);
                if (_commandTasks.Count > 0)
                {
                    notify = true;
                }
            }

            commandExecutor.CommandExecuted -= CommandExecutor_CommandExecuted;
            commandExecutor.CommandManager = null;

            if (notify)
            {
                NotifyCommandReady();
            }

            Log.Debug("RemoveCommandExecutor: end.");
        }

        public void EnqueueCommandTask(MtCommandTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            Log.DebugFormat("EnqueueCommandTask: begin. task = {0}", task);

            lock (_locker)
            {
                _commandTasks.Enqueue(task);
            }

            NotifyCommandReady();

            Log.Debug("EnqueueCommandTask: end.");
        }

        public MtCommandTask DequeueCommandTask()
        {
            Log.Debug("DequeueCommandTask: called.");

            lock (_locker)
            {
                return _commandTasks.Count > 0 ? _commandTasks.Dequeue() : null;
            }
        }

        #endregion

        #region Private Methods
        private void NotifyCommandReady()
        {
            Log.Debug("NotifyCommandReady: begin.");

            var commandExecutors = new List<MtExpert>();
            lock (_locker)
            {
                commandExecutors.AddRange(_commandExecutors);
            }

            foreach (var executor in commandExecutors)
            {
                executor.NotifyCommandReady();
            }

            Log.DebugFormat("NotifyCommandReady: end. Notified executor count = {0}", commandExecutors.Count);
        }

        private void CommandExecutor_CommandExecuted(object sender, EventArgs e)
        {
            Log.Debug("CommandExecutor_CommandExecuted: begin.");

            var notify = false;
            lock (_locker)
            {
                if (_commandTasks.Count > 0)
                {
                    notify = true;
                }
            }

            if (notify)
            {
                NotifyCommandReady();
            }

            Log.Debug("CommandExecutor_CommandExecuted: end.");
        }
        #endregion
    }
}
