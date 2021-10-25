﻿using System;
using System.Threading;

namespace MTApiService
{
    public class MtCommandTask
    {
        private readonly EventWaitHandle _responseWaiter = new AutoResetEvent(false);
        private MtResponse _result;
        private readonly object _locker = new object();

        public MtCommandTask(MtCommand command)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        public MtCommand Command { get; }

        public MtResponse WaitResult(int time)
        {
            _responseWaiter.WaitOne(time);
            lock (_locker)
            {
                return _result;
            }
        }

        public void SetResult(MtResponse result)
        {
            lock (_locker)
            {
                _result = result;
            }
            _responseWaiter.Set();
        }

        public override string ToString()
        {
            return $"Command = {Command}";
        }
    }
}
