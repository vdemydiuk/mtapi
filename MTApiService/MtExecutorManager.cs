using System;
using System.Collections.Generic;

namespace MTApiService
{
    internal class MtCommandExecutorManager : ICommandManager
    {
        #region Public Methods

        public void Stop()
        {
            lock (_locker)
            {
                _commandExecutors.Clear();
                _commandTasks.Clear();
            }
        }

        public void AddCommandExecutor(MtExpert commandExecutor)
        {
            if (commandExecutor == null)
                return;

            var notify = false;
            lock (_locker)
            {
                if (_commandExecutors.Contains(commandExecutor))
                    return;

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
        }

        public void RemoveCommandExecutor(MtExpert commandExecutor)
        {
            if (commandExecutor == null)
                return;

            var notify = false;
            lock (_locker)
            {
                if (_commandExecutors.Contains(commandExecutor) == false)
                    return;

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
        }

        public void EnqueueCommandTask(MtCommandTask task)
        {
            if (task == null)
                return;

            lock (_locker)
            {
                _commandTasks.Enqueue(task);                
            }

            NotifyCommandReady();
        }

        public MtCommandTask DequeueCommandTask()
        {
            lock (_locker)
            {
                return _commandTasks.Count > 0 ? _commandTasks.Dequeue() : null;
            }
        }

        #endregion

        #region Private Methods
        private void NotifyCommandReady()
        {
            var commandExecutors = new List<MtExpert>();
            lock (_locker)
            {
                commandExecutors.AddRange(_commandExecutors);
            }

            foreach (var executor in commandExecutors)
            {
                executor.NotifyCommandReady();
            }
        }

        private void CommandExecutor_CommandExecuted(object sender, EventArgs e)
        {
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

        }
        #endregion

        #region Private Fields
        private readonly List<MtExpert> _commandExecutors = new List<MtExpert>();
        private readonly Queue<MtCommandTask> _commandTasks = new Queue<MtCommandTask>();

        private readonly object _locker = new object();
        #endregion
    }
}
