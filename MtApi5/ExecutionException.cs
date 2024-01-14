namespace MtApi5
{
    public class ExecutionException(ErrorCode errorCode, string? message) : Exception(message)
    {
        public ErrorCode ErrorCode { get; private set; } = errorCode;
    }
}