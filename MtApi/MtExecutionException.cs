namespace MtApi
{
    public class MtExecutionException(MtErrorCode errorCode, string? message) : Exception(message)
    {
        public MtErrorCode ErrorCode { get; private set; } = errorCode;
    }
}