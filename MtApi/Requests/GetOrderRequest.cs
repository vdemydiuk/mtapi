namespace MtApi.Requests
{
    public class GetOrderRequest: RequestBase
    {
        public int Index { get; set; }
        public int Select { get; set; }
        public int Pool { get; set; }

        public override RequestType RequestType
        {
            get { return RequestType.GetOrder; }
        }
    }
}