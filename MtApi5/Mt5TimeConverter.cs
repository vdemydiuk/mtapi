using System;

namespace MtApi5
{
    internal class Mt5TimeConverter
    {
        public static DateTime ConvertFromMtTime(int time)
        {
            var tmpTime = new DateTime(1970, 1, 1);
            return new DateTime(tmpTime.Ticks + (time * 0x989680L));
        }

        public static DateTime ConvertFromMtTime(long time)
        {
            var tmpTime = new DateTime(1970, 1, 1);
            return new DateTime(tmpTime.Ticks + (time * 0x989680L));
        }

        public static int ConvertToMtTime(DateTime time)
        {
            var result = 0;
            if (time == DateTime.MinValue) return result;
            var tmpTime = new DateTime(1970, 1, 1);
            result = (int)((time.Ticks - tmpTime.Ticks) / 0x989680L);
            return result;
        }
    }
}
