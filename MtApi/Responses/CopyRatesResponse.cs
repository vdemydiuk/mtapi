using System.Collections.Generic;

namespace MtApi.Responses
{
    internal class CopyRatesResponse: ResponseBase
    {
        public List<MqlRates> Rates { get; set; }
    }
}
