using System;
using System.Collections.Generic;
using log4net;

namespace MTApiService
{
    internal class MtExecutorManager : ICommandManager
    {
        #region Private Fields
        private static readonly ILog Log = LogManager.GetLogger(typeof(MtExecutorManager));

        private readonly List<ITaskExecutor> _executorList = new List<ITaskExecutor>();
        private readonly Dictionary<int, ITaskExecutor> _executorMap = new Dictionary<int, ITaskExecutor>();
        private readonly object _locker = new object();
        #endregion

        #region Public Methods

        public void Stop()
        {
            Log.Debug("Stop: begin.");

            lock (_locker)
            {
                _executorList.Clear();
                _executorMap.Clear();
            }

            Log.Debug("Stop: end.");
        }

        public void AddExecutor(ITaskExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException(nameof(executor));

            Log.DebugFormat("AddExecutor: begin. executor = {0}", executor);

            lock (_locker)
            {
                if (_executorList.Contains(executor))
                {
                    Log.Warn("AddExecutor: end. Executor already exist.");
                    return;
                }

                _executorList.Add(executor);
                _executorMap[executor.Handle] = executor;
            }

            Log.Debug("AddCommandExecutor: end.");
        }

        public void RemoveExecutor(ITaskExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException(nameof(executor));

            Log.DebugFormat("RemoveExecutor: begin. executor = {0}", executor);

            lock (_locker)
            {
                if (_executorList.Contains(executor) == false)
                {
                    Log.Warn("RemoveExecutor: end. Executor is not exist in collection.");
                    return;
                }

                _executorList.Remove(executor);
                _executorMap.Remove(executor.Handle);
            }

            Log.Debug("RemoveExecutor: end.");
        }

        public MtCommandTask SendCommand(MtCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var task = new MtCommandTask(command);

            Log.DebugFormat("SendTask: begin. command = {0}", command);

            ITaskExecutor executor = null;

            lock (_locker)
            {
                if (_executorMap.ContainsKey(command.ExpertHandle))
                {
                    executor = _executorMap[command.ExpertHandle];
                }
                else
                {
                    executor = _executorList.Count > 0 ? _executorList[0] : null;
                }
            }

            if (executor == null)
            {
                Log.Error("SendTask: Executor is null!");
            }
            else
            {
                executor.Execute(task);
            }

            Log.Debug("SendTask: end.");

            return task;
        }

        #endregion
    }
}
