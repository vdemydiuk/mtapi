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

        public MqlBookInfo()
        { }

        public ENUM_BOOK_TYPE type { get; set; }    // Order type from ENUM_BOOK_TYPE enumeration
        public double price { get; set; }           // Price
        public long volume { get; set; }            // Volume

        public override string ToString()
        {
            return $"type = {type}; price = {price}; volume = {volume}";
        }
    }
}
