using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public ENUM_BOOK_TYPE type { get; private set; }    // Order type from ENUM_BOOK_TYPE enumeration
        public double price { get; private set; }           // Price
        public long volume { get; private set; }            // Volume
    }
}
