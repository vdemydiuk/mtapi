using System;

namespace MtApi5
{
    public class ExecutionException: Exception
    {
        public ExecutionException(ErrorCode errorCode, string message)
            :base(message)
        {
            ErrorCode = errorCode;
        }

        public ErrorCode ErrorCode { get; private set; }
    }
}