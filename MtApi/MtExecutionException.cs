using System;

namespace MtApi
{
    public class MtExecutionException: Exception
    {
        public MtExecutionException(MtErrorCode errorCode, string message)
            :base(message)
        {
            ErrorCode = errorCode;
        }

        public MtErrorCode ErrorCode { get; private set; }
    }
}