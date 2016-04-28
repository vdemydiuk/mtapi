using Newtonsoft.Json;

namespace MtApi.Requests
{
    public abstract class RequestBase
    {
        public abstract RequestType RequestType { get; }
    }
}
