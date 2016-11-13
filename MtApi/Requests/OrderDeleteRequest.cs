namespace MtApi.Requests
{
    internal class OrderDeleteRequest: RequestBase
    {
        public int Ticket { get; set; }

        public int? ArrowColor { get; set; }

        public override RequestType RequestType => RequestType.OrderDelete;
    }
}