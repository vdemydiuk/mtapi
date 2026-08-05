using System;

namespace MtApi5
{
    ///<summary>
    ///Snapshot of a current (pending) order with all its properties.
    ///Returned by MtApi5Client.GetOrders() in a single request instead of the
    ///OrdersTotal/OrderGetTicket/OrderGet* per-property call sequence.
    ///</summary>
    public class Mt5Order
    {
        ///<summary>Order ticket (ORDER_TICKET). Unique number assigned to each order.</summary>
        public ulong Ticket { get; set; }

        ///<summary>Symbol of the order (ORDER_SYMBOL).</summary>
        public string Symbol { get; set; } = string.Empty;

        ///<summary>Order setup time (ORDER_TIME_SETUP).</summary>
        public DateTime TimeSetup => Mt5TimeConverter.ConvertFromMtTime(MtTimeSetup);

        ///<summary>The time of placing the order in milliseconds since 01.01.1970 (ORDER_TIME_SETUP_MSC).</summary>
        public long TimeSetupMsc { get; set; }

        ///<summary>Order expiration time (ORDER_TIME_EXPIRATION).</summary>
        public DateTime TimeExpiration => Mt5TimeConverter.ConvertFromMtTime(MtTimeExpiration);

        ///<summary>Order type (ORDER_TYPE).</summary>
        public ENUM_ORDER_TYPE Type { get; set; }

        ///<summary>Order lifetime (ORDER_TYPE_TIME).</summary>
        public ENUM_ORDER_TYPE_TIME TypeTime { get; set; }

        ///<summary>Order filling type (ORDER_TYPE_FILLING).</summary>
        public ENUM_ORDER_TYPE_FILLING TypeFilling { get; set; }

        ///<summary>Order state (ORDER_STATE).</summary>
        public ENUM_ORDER_STATE State { get; set; }

        ///<summary>Order magic number (ORDER_MAGIC).</summary>
        public long Magic { get; set; }

        ///<summary>Position identifier that is set to an order as soon as it is executed (ORDER_POSITION_ID).</summary>
        public long PositionId { get; set; }

        ///<summary>Identifier of an opposite position used for closing by order (ORDER_POSITION_BY_ID).</summary>
        public long PositionById { get; set; }

        ///<summary>Order initial volume (ORDER_VOLUME_INITIAL).</summary>
        public double VolumeInitial { get; set; }

        ///<summary>Order current volume (ORDER_VOLUME_CURRENT).</summary>
        public double VolumeCurrent { get; set; }

        ///<summary>Price specified in the order (ORDER_PRICE_OPEN).</summary>
        public double PriceOpen { get; set; }

        ///<summary>Stop Loss value (ORDER_SL).</summary>
        public double StopLoss { get; set; }

        ///<summary>Take Profit value (ORDER_TP).</summary>
        public double TakeProfit { get; set; }

        ///<summary>The current price of the order symbol (ORDER_PRICE_CURRENT).</summary>
        public double PriceCurrent { get; set; }

        ///<summary>The limit order price for the StopLimit order (ORDER_PRICE_STOPLIMIT).</summary>
        public double PriceStopLimit { get; set; }

        ///<summary>Order comment (ORDER_COMMENT).</summary>
        public string Comment { get; set; } = string.Empty;

        ///<summary>Order identifier in an external trading system (ORDER_EXTERNAL_ID).</summary>
        public string ExternalId { get; set; } = string.Empty;

        ///<summary>The reason for order placing (ORDER_REASON).</summary>
        public ENUM_ORDER_REASON Reason { get; set; }

        ///<summary>Order setup time as seconds since 01.01.1970 (raw MT time).</summary>
        public long MtTimeSetup { get; set; }

        ///<summary>Order expiration time as seconds since 01.01.1970 (raw MT time).</summary>
        public long MtTimeExpiration { get; set; }

        public override string ToString()
        {
            return $"Ticket={Ticket}; Symbol={Symbol}; TimeSetup={TimeSetup}; Type={Type}; State={State}; TypeTime={TypeTime}; TypeFilling={TypeFilling}; TimeExpiration={TimeExpiration}; VolumeInitial={VolumeInitial}; VolumeCurrent={VolumeCurrent}; PriceOpen={PriceOpen}; StopLoss={StopLoss}; TakeProfit={TakeProfit}; PriceCurrent={PriceCurrent}; PriceStopLimit={PriceStopLimit}; Magic={Magic}; PositionId={PositionId}; PositionById={PositionById}; Reason={Reason}; Comment={Comment}; ExternalId={ExternalId}";
        }
    }
}
