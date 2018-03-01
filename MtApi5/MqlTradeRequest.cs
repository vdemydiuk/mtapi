// ReSharper disable InconsistentNaming
using System;
using Newtonsoft.Json;

namespace MtApi5
{
    public class MqlTradeRequest
    {
        public ENUM_TRADE_REQUEST_ACTIONS Action { get; set; }           // Trade operation type
        public ulong Magic { get; set; }                                 // Expert Advisor ID (magic number)
        public ulong Order { get; set; }                                 // Order ticket
        public string Symbol { get; set; }                               // Trade symbol
        public double Volume { get; set; }                               // Requested volume for a deal in lots
        public double Price { get; set; }                                // Price
        public double Stoplimit { get; set; }                            // StopLimit level of the order
        public double Sl { get; set; }                                   // Stop Loss level of the order
        public double Tp { get; set; }                                   // Take Profit level of the order
        public ulong Deviation { get; set; }                             // Maximal possible deviation from the requested price
        public ENUM_ORDER_TYPE Type { get; set; }                        // Order type
        public ENUM_ORDER_TYPE_FILLING Type_filling { get; set; }        // Order execution type
        public ENUM_ORDER_TYPE_TIME Type_time { get; set; }              // Order expiration type

        [JsonIgnore]
        public DateTime Expiration                                       // Order expiration time (for the orders of ORDER_TIME_SPECIFIED type)
        {
            get { return Mt5TimeConverter.ConvertFromMtTime(MtExpiration); }
            set { MtExpiration =  Mt5TimeConverter.ConvertToMtTime(value); } 
        }

        public string Comment { get; set; }                              // Order comment
        public ulong Position { get; set; }                              // Position ticket
        public ulong PositionBy { get; set; }                            // The ticket of an opposite position

        public int MtExpiration { get; private set; }

        public override string ToString()
        {
            return $"Action={Action}; Magic={Magic}; Order={Order}; Symbol={Symbol}; Volume={Volume}; Price={Price}; Stoplimit={Stoplimit}; Sl={Sl}; Tp={Tp}; Deviation={Deviation}; Type={Type}; Type_filling={Type_filling}; Type_time={Type_time}; Expiration={Expiration}; Comment={Comment}; Position={Position}; PositionBy={PositionBy}";
        }
    }
}
