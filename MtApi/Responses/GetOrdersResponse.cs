using System.Collections.Generic;

namespace MtApi.Responses
{
    public class GetOrdersResponse: ResponseBase
    {
        public List<MtOrder> Orders { get; set; } 
    }
}