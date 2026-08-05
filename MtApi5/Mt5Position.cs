using System;

namespace MtApi5
{
    ///<summary>
    ///Snapshot of an open position with all its properties.
    ///Returned by MtApi5Client.GetPositions() in a single request instead of the
    ///PositionsTotal/PositionGetTicket/PositionGet* per-property call sequence.
    ///</summary>
    public class Mt5Position
    {
        ///<summary>Position ticket (POSITION_TICKET). Unique number assigned to each newly opened position.</summary>
        public ulong Ticket { get; set; }

        ///<summary>Symbol of the position (POSITION_SYMBOL).</summary>
        public string Symbol { get; set; } = string.Empty;

        ///<summary>Position open time (POSITION_TIME).</summary>
        public DateTime Time => Mt5TimeConverter.ConvertFromMtTime(MtTime);

        ///<summary>Position open time in milliseconds since 01.01.1970 (POSITION_TIME_MSC).</summary>
        public long TimeMsc { get; set; }

        ///<summary>Position changing time (POSITION_TIME_UPDATE).</summary>
        public DateTime TimeUpdate => Mt5TimeConverter.ConvertFromMtTime(MtTimeUpdate);

        ///<summary>Position type (POSITION_TYPE).</summary>
        public ENUM_POSITION_TYPE Type { get; set; }

        ///<summary>Position magic number (POSITION_MAGIC).</summary>
        public long Magic { get; set; }

        ///<summary>Position identifier (POSITION_IDENTIFIER). Doesn't change during the entire lifetime of the position.</summary>
        public long Identifier { get; set; }

        ///<summary>The reason for opening the position (POSITION_REASON).</summary>
        public ENUM_POSITION_REASON Reason { get; set; }

        ///<summary>Position volume (POSITION_VOLUME).</summary>
        public double Volume { get; set; }

        ///<summary>Position open price (POSITION_PRICE_OPEN).</summary>
        public double PriceOpen { get; set; }

        ///<summary>Stop Loss level of the opened position (POSITION_SL).</summary>
        public double StopLoss { get; set; }

        ///<summary>Take Profit level of the opened position (POSITION_TP).</summary>
        public double TakeProfit { get; set; }

        ///<summary>Current price of the position symbol (POSITION_PRICE_CURRENT).</summary>
        public double PriceCurrent { get; set; }

        ///<summary>Cumulative swap (POSITION_SWAP).</summary>
        public double Swap { get; set; }

        ///<summary>Current profit (POSITION_PROFIT).</summary>
        public double Profit { get; set; }

        ///<summary>Position comment (POSITION_COMMENT).</summary>
        public string Comment { get; set; } = string.Empty;

        ///<summary>Position identifier in an external trading system (POSITION_EXTERNAL_ID).</summary>
        public string ExternalId { get; set; } = string.Empty;

        ///<summary>Position open time as seconds since 01.01.1970 (raw MT time).</summary>
        public long MtTime { get; set; }

        ///<summary>Position changing time as seconds since 01.01.1970 (raw MT time).</summary>
        public long MtTimeUpdate { get; set; }

        public override string ToString()
        {
            return $"Ticket={Ticket}; Symbol={Symbol}; Time={Time}; Type={Type}; Volume={Volume}; PriceOpen={PriceOpen}; StopLoss={StopLoss}; TakeProfit={TakeProfit}; PriceCurrent={PriceCurrent}; Swap={Swap}; Profit={Profit}; Magic={Magic}; Identifier={Identifier}; Reason={Reason}; Comment={Comment}; ExternalId={ExternalId}";
        }
    }
}
