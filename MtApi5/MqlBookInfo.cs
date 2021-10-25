// ReSharper disable InconsistentNaming
namespace MtApi5
{
    public class MqlBookInfo
    {
        public MqlBookInfo(ENUM_BOOK_TYPE type, double price, long volume)
        {
            this.Type = type;
            this.Price = price;
            this.Volume = volume;
        }

        public MqlBookInfo()
        { }

        public ENUM_BOOK_TYPE Type { get; set; }    // Order type from ENUM_BOOK_TYPE enumeration
        public double Price { get; set; }           // Price
        public long Volume { get; set; }            // Volume
        public double Volume_real { get; set; }   // Volume for the current Last price with greater accuracy 

        public override string ToString()
        {
            return $"type = {Type}; price = {Price}; volume = {Volume}";
        }
    }
}
