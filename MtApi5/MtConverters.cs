using System;
using MTApiService;
using System.Collections;

namespace MtApi5
{
    internal static class MtConverters
    {
        private static readonly MtLog Log = LogConfigurator.GetLogger(typeof(MtConverters));

        #region Values Converters

        public static bool ParseResult(this string inputString, char separator, out double result)
        {
            Log.Debug($"ParseResult: inputString = {inputString}, separator = {separator}");

            var retVal = false;
            result = 0;

            if (string.IsNullOrEmpty(inputString) == false)
            {
                string[] values = inputString.Split(separator);
                if (values.Length == 2)
                {
                    try
                    {
                        retVal = int.Parse(values[0]) != 0;

                        result = double.Parse(values[1]);
                    }
                    catch (Exception ex)
                    {
                        retVal = false;
                        Log.Error($"ParseResult: {ex.Message}");
                    }
                }
            }
            else
            {
                Log.Warn("ParseResult: input srting is null or empty!");
            }

            return retVal;
        }

        public static bool ParseResult(this string inputString, char separator, out DateTime from, out DateTime to)
        {
            Log.Debug($"ParseResult: inputString = {inputString}, separator = {separator}");

            var retVal = false;

            from = new DateTime();
            to = new DateTime();

            if (string.IsNullOrEmpty(inputString) == false)
            {
                var values = inputString.Split(separator);
                if (values.Length == 3)
                {
                    try
                    {
                        retVal = int.Parse(values[0]) != 0;

                        var iFrom = int.Parse(values[1]);
                        from = Mt5TimeConverter.ConvertFromMtTime(iFrom);

                        var iTo= int.Parse(values[2]);
                        to = Mt5TimeConverter.ConvertFromMtTime(iTo);
                    }
                    catch (Exception ex)
                    {
                        retVal = false;
                        Log.Error($"ParseResult: {ex.Message}");
                    }
                }
            }
            else
            {
                Log.Warn("ParseResult: input srting is null or empty!");
            }

            return retVal;
        }

        public static ArrayList ToArrayList(this MqlTradeRequest request)
        {
            if (request == null)
                throw new ArgumentNullException();

            var exp = Mt5TimeConverter.ConvertToMtTime(request.Expiration);

            return new ArrayList { (int)request.Action, request.Magic, request.Order, request.Symbol, request.Volume
                , request.Price, request.Stoplimit, request.Sl, request.Tp, request.Deviation, (int)request.Type
                , (int)request.Type_filling, (int)request.Type_time, exp, request.Comment, request.Position, request.PositionBy };
        }

        #endregion
    }
}
