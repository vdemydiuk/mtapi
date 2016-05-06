namespace MtApi.Requests
{
    public class OrderModifyRequest: RequestBase
    {
        public int Ticket { get; set; }
        public double Price { get; set; }
        public double Stoploss { get; set; }
        public double Takeprofit { get; set; }
        public int Expiration { get; set; }

        public int? ArrowColor { get; set; }

        public override RequestType RequestType
        {
            get { return RequestType.OrderModify; }
        }
    }
}