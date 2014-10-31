using System;
using System.Linq;
using System.Text;

namespace MtApi5
{
    public class MqlTradeRequest
    {
        public ENUM_TRADE_REQUEST_ACTIONS Action { get; set; }           // Trade operation type
        public uint Magic { get; set; }                                 // Expert Advisor ID (magic number)
        public uint Order { get; set; }                                 // Order ticket
        public string Symbol { get; set; }                               // Trade symbol
        public double Volume { get; set; }                               // Requested volume for a deal in lots
        public double Price { get; set; }                                // Price
        public double Stoplimit { get; set; }                            // StopLimit level of the order
        public double Sl { get; set; }                                   // Stop Loss level of the order
        public double Tp { get; set; }                                   // Take Profit level of the order
        public uint Deviation { get; set; }                             // Maximal possible deviation from the requested price
        public ENUM_ORDER_TYPE Type { get; set; }                        // Order type
        public ENUM_ORDER_TYPE_FILLING Type_filling { get; set; }        // Order execution type
        public ENUM_ORDER_TYPE_TIME Type_time { get; set; }              // Order expiration type
        public DateTime Expiration { get; set; }                         // Order expiration time (for the orders of ORDER_TIME_SPECIFIED type)
        public string Comment { get; set; }                              // Order comment
    }
}
