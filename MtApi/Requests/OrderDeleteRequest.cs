namespace MtApi.Requests
{
    public class OrderDeleteRequest: RequestBase
    {
        public int Ticket { get; set; }

        public int? ArrowColor { get; set; }

        public override RequestType RequestType
        {
            get { return RequestType.OrderDelete; }
        }
    }
}