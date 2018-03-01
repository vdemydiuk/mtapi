using System;
using Newtonsoft.Json;

namespace MtApi5
{
    public class MqlTradeTransaction
    {
        public ulong Deal { get; set; }                         // Deal ticket 
        public ulong Order { get; set; }                        // Order ticket 
        public string Symbol { get; set; }                      // Trade symbol name
        public ENUM_TRADE_TRANSACTION_TYPE Type { get; set; }   // Trade transaction type
        public ENUM_ORDER_TYPE OrderType { get; set; }          // Order type
        public ENUM_ORDER_STATE OrderState { get; set; }        // Order state
        public ENUM_DEAL_TYPE DealType { get; set; }            // Deal type
        public ENUM_ORDER_TYPE_TIME TimeType { get; set; }      // Order type by action period

        [JsonIgnore]
        public DateTime TimeExpiration                          // Order expiration time 
        {
            get { return Mt5TimeConverter.ConvertFromMtTime(MtTimeExpiration); }
            set { MtTimeExpiration = Mt5TimeConverter.ConvertToMtTime(value); }
        }

        public double Price { get; set; }                       // Price  
        public double PriceTrigger { get; set; }                // Stop limit order activation price 
        public double PriceSl { get; set; }                     // Stop Loss level 
        public double PriceTp { get; set; }                     // Take Profit level 
        public double Volume { get; set; }                      // Volume in lots 
        public ulong Position { get; set; }                     // Position ticket 
        public ulong PositionBy { get; set; }                   // Ticket of an opposite position

        public int MtTimeExpiration { get; private set; }

        public override string ToString()
        {
            return $"Deal={Deal}; Order={Order}; Symbol={Symbol}; Type={Type}; OrderType={OrderType}; OrderState={OrderState}; DealType={DealType}; TimeType={TimeType}; TimeExpiration={TimeExpiration}; Price={Price}; PriceTrigger{PriceTrigger}; PriceSl={PriceSl}; PriceTp={PriceTp}; Volume={Volume}; Position={Position}; PositionBy={PositionBy}";
        }
    }
}