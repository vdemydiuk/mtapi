using System.Collections.Generic;

namespace MtApi.Responses
{
    public class CopyRatesResponse: ResponseBase
    {
        public List<MqlRates> Rates { get; set; }
    }
}
