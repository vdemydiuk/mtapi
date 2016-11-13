namespace MtApi.Requests
{
    internal class GetOrderRequest: RequestBase
    {
        public int Index { get; set; }
        public int Select { get; set; }
        public int Pool { get; set; }

        public override RequestType RequestType => RequestType.GetOrder;
    }
}