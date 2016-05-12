namespace MtApi.Requests
{
    public enum RequestType
    {
        Unknown         = 0,
        GetOrder        = 1,
        GetOrders       = 2,
        OrderSend       = 3,
        OrderClose      = 4,
        OrderCloseBy    = 5,
        OrderDelete     = 6,
        OrderModify     = 7,
        iCustom         = 8
    }
}