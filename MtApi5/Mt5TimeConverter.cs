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

        public static long ConvertToMtTime(DateTime? time)
        {
            long result = 0;
            if (time == null || time == DateTime.MinValue)
                return result;
            var tmpTime = new DateTime(1970, 1, 1);
            result = ((time.Value.Ticks - tmpTime.Ticks) / 0x989680L);
            return result;
        }
    }
}
