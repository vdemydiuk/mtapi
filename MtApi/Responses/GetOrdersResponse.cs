using System.Collections.Generic;

namespace MtApi.Responses
{
    internal class GetOrdersResponse: ResponseBase
    {
        public List<MtOrder> Orders { get; set; } 
    }
}