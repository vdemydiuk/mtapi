using System.Collections.Generic;

namespace MtApi5.Responses
{
    internal class CopyTicksResponse: ResponseBase
    {
         public List<MqlTick> Ticks { get; set; }
    }
}