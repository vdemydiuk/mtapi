using MtService;

namespace MtClient
{
    /// <summary>
    /// MtService could not deliver the command to MetaTrader, see <see cref="Status"/> for the reason.
    /// </summary>
    public class MtRpcException : Exception
    {
        public MtRpcException(ResponseStatus status, string message)
            : base(message)
        {
            Status = status;
        }

        public ResponseStatus Status { get; }
    }
}