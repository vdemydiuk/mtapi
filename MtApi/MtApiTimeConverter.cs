using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtApi
{
    class MtApiTimeConverter
    {
        public static DateTime ConvertFromMtTime(int time)
        {
            DateTime tmpTime = new DateTime(1970, 1, 1);
            return new DateTime(tmpTime.Ticks + (time * 0x989680L));
        }

        public static int ConvertToMtTime(DateTime? time)
        {
            int result = 0;
            if (time != null && time != DateTime.MinValue)
            {
                DateTime tmpTime = new DateTime(1970, 1, 1);
                result = (int)((time.Value.Ticks - tmpTime.Ticks) / 0x989680L);
            }
            return result;
        }
    }
}
