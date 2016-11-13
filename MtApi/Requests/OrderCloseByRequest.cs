namespace MtApi.Requests
{
    internal class OrderCloseByRequest: RequestBase
    {
        public int Ticket { get; set; }
        public int Opposite { get; set; }

        public int? ArrowColor { get; set; }

        public override RequestType RequestType => RequestType.OrderCloseBy;
    }
}