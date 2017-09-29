namespace MtApi5
{
    public class MqlTradeResult
    {
        public MqlTradeResult(uint retcode, ulong deal, ulong order, double volume, double price, double bid, double ask, string comment, uint request_id)
        {
            Retcode = retcode;
            Deal = deal;
            Order = order;
            Volume = volume;
            Price = price;
            Bid = bid;
            Ask = ask;
            Comment = comment;
            Request_id = request_id;
        }

        public uint Retcode { get; private set; }          // Operation return code
        public ulong Deal { get; private set; }            // Deal ticket, if it is performed
        public ulong Order { get; private set; }           // Order ticket, if it is placed
        public double Volume { get; private set; }         // Deal volume, confirmed by broker
        public double Price { get; private set; }          // Deal price, confirmed by broker
        public double Bid { get; private set; }            // Current Bid price
        public double Ask { get; private set; }            // Current Ask price
        public string Comment { get; private set; }        // Broker comment to operation (by default it is filled by the operation description)
        public uint Request_id { get; private set; }       // Request ID set by the terminal during the dispatch 
    }
}
