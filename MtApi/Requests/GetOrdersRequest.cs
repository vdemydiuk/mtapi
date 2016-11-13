namespace MtApi.Requests
{
    internal class GetOrdersRequest: RequestBase
    {
        public int Pool { get; set; }

        public override RequestType RequestType => RequestType.GetOrders;
    }
}