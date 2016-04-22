using System;

namespace MtApi
{
    public class MtExecutionException: Exception
    {
        public MtExecutionException(int errorCode, string message)
            :base(message)
        {
            ErrorCode = errorCode;
        }

        public int ErrorCode { get; private set; }
    }
}