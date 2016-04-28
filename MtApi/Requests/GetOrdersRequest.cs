namespace MtApi.Requests
{
    public class GetOrdersRequest: RequestBase
    {
        public int Pool { get; set; }

        public override RequestType RequestType
        {
            get { return RequestType.GetOrders; }
        }
    }
}