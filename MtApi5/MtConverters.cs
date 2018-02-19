using System;
using MTApiService;
using System.Collections;

namespace MtApi5
{
    internal static class MtConverters
    {
        private static readonly MtLog Log = LogConfigurator.GetLogger(typeof(MtConverters));

        #region Values Converters
        public static Mt5Quote Parse(this MtQuote quote)
        {
            return quote != null ? new Mt5Quote(quote.Instrument, quote.Bid, quote.Ask) : null;
        }

        //public static bool ParseResult(this string inputString, char separator, out MqlTradeResult result)
        //{
        //    Log.Debug($"ParseResult: inputString = {inputString}, separator = {separator}");

        //    var retVal = false;
        //    result = null;

        //    if (string.IsNullOrEmpty(inputString) == false)
        //    {
        //        var values = inputString.Split(separator);
        //        if (values.Length == 10)
        //        {
        //            try
        //            {
        //                retVal = int.Parse(values[0]) != 0;

        //                var retcode = uint.Parse(values[1]);
        //                var deal = ulong.Parse(values[2]);
        //                var order = ulong.Parse(values[3]);
        //                var volume = double.Parse(values[4]);
        //                var price = double.Parse(values[5]);
        //                var bid = double.Parse(values[6]);
        //                var ask = double.Parse(values[7]);
        //                var comment = values[8];
        //                var requestId = uint.Parse(values[9]);

        //                result = new MqlTradeResult(retcode, deal, order, volume, price, bid, ask, comment, requestId);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Error($"ParseResult: {ex.Message}");
        //                retVal = false;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        Log.Warn("ParseResult: input srting is null or empty!");
        //    }

        //    return retVal;
        //}

        public static bool ParseResult(this string inputString, char separator, out MqlTradeCheckResult result)
        {
            Log.Debug($"ParseResult: inputString = {inputString}, separator = {separator}");

            var retVal = false;
            result = null;

            if (string.IsNullOrEmpty(inputString) == false)
            {
                var values = inputString.Split(separator);
                if (values.Length == 10)
                {
                    try
                    {
                        retVal = int.Parse(values[0]) != 0;

                        var retcode = uint.Parse(values[1]);
                        var balance = double.Parse(values[2]);
                        var equity = double.Parse(values[3]);
                        var profit = double.Parse(values[4]);
                        var margin = double.Parse(values[5]);
                        var marginFree = double.Parse(values[6]);
                        var marginLevel = double.Parse(values[7]);
                        var comment = values[8];

                        result = new MqlTradeCheckResult(retcode, balance, equity, profit, margin, marginFree, marginLevel, comment);
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
