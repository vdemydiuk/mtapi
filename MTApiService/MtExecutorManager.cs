using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MTApiService
{
    class MtCommandExecutorManager : ICommandManager
    {
        #region Public Methods

        public void Stop()
        {
            lock (_locker)
            {
                mCommandExecutors.Clear();
                mCommands.Clear();
            }
        }

        public void AddCommandExecutor(MtExpert commandExecutor)
        {
            if (commandExecutor == null)
                return;

            lock (_locker)
            {
                if (mCommandExecutors.Contains(commandExecutor) == true)
                    return;

                mCommandExecutors.Add(commandExecutor);

                commandExecutor.CommandManager = this;

                if (mCurrentExecutor == null)
                {
                    mCurrentExecutor = commandExecutor;
                    mCurrentExecutor.IsCommandExecutor = true;

                    if (mCommands.Count > 0)
                    {
                        mCurrentExecutor.NotifyCommandReady();
                    }
                }
            }
        }

        public void RemoveCommandExecutor(MtExpert commandExecutor)
        {
            if (commandExecutor == null)
                return;

            lock (_locker)
            {
                if (mCommandExecutors.Contains(commandExecutor) == false)
                    return;

                mCommandExecutors.Remove(commandExecutor);

                if (mCurrentExecutor == commandExecutor)
                {
                    mCurrentExecutor.IsCommandExecutor = false;
                    mCurrentExecutor = mCommandExecutors.Count > 0 ? mCommandExecutors[0] : null;

                    if (mCommands.Count > 0)
                    {
                        mCurrentExecutor.NotifyCommandReady();
                    }
                }
            }
        }

        public void EnqueueCommand(MtCommand command)
        {
            if (command == null)
                return;

            lock (_locker)
            {
                mCommands.Enqueue(command);

                mCurrentExecutor.NotifyCommandReady();
            }
        }

        public MtCommand DequeueCommand()
        {
            lock (_locker)
            {
                return mCommands.Count > 0 ? mCommands.Dequeue() : null;
            }
        }

        public void OnCommandExecuted(MtExpert expert, MtCommand command, MtResponse response)
        {
            if (expert == null)
                return;

            if (CommandExecuted != null)
            {
                CommandExecuted(this, new MtCommandExecuteEventArgs(command, response));
            }

            lock (_locker)
            {
                if (expert == mCurrentExecutor)
                {
                    if (mCommands.Count > 0)
                    {
                        mCurrentExecutor.NotifyCommandReady();
                    }
                }
            }
        }

        #endregion

        #region Events
        public event EventHandler<MtCommandExecuteEventArgs> CommandExecuted;
        #endregion

        #region Private Fields
        private MtExpert mCurrentExecutor;

        private List<MtExpert> mCommandExecutors = new List<MtExpert>();
        private Queue<MtCommand> mCommands = new Queue<MtCommand>();

        private readonly object _locker = new object();
        #endregion
    }
}
