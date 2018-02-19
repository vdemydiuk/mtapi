namespace MtApi5.Requests
{
    internal class PositionOpenRequest: RequestBase
    {
        public override RequestType RequestType => RequestType.PositionOpen;

        public string Symbol { get; set; }
        public ENUM_ORDER_TYPE OrderType { get; set; }
        public double Volume { get; set; }
        public double Price { get; set; }
        public double Sl { get; set; }
        public double Tp { get; set; }
        public string Comment { get; set; }
    }
}