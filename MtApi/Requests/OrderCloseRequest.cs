namespace MtApi.Requests
{
    public class OrderCloseRequest: RequestBase
    {
        public int Ticket { get; set; }

        public double? Lots { get; set; }
        public double? Price { get; set; }
        public int? Slippage { get; set; }
        public int? ArrowColor { get; set; }

        public override RequestType RequestType
        {
            get { return RequestType.OrderClose; }
        }
    }
}