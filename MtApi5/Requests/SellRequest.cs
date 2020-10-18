namespace MtApi5.Requests
{
    internal class SellRequest : RequestBase
    {
        public override RequestType RequestType => RequestType.Sell;

        public double Volume { get; set; }
        public string Symbol { get; set; }
        public double Price { get; set; }
        public double Sl { get; set; }
        public double Tp { get; set; }
        public string Comment { get; set; }
    }
}
