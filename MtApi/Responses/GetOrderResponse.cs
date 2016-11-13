namespace MtApi.Responses
{
    internal class GetOrderResponse: ResponseBase
    {
        public MtOrder Order { get; set; }
    }
}