// ReSharper disable InconsistentNaming
namespace MtApi5
{
    public class MqlBookInfo
    {
        public MqlBookInfo(ENUM_BOOK_TYPE type, double price, long volume)
        {
            this.type = type;
            this.price = price;
            this.volume = volume;
        }

        public ENUM_BOOK_TYPE type { get; }    // Order type from ENUM_BOOK_TYPE enumeration
        public double price { get; }           // Price
        public long volume { get; }            // Volume

        public override string ToString()
        {
            return $"{type}|{price}|{volume}";
        }
    }
}
