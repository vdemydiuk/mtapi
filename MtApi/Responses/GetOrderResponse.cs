namespace MtApi.Responses
{
    public class GetOrderResponse: ResponseBase
    {
        public MtOrder Order { get; set; }
    }
}