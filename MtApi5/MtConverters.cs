using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTApiService;
using System.Collections;

namespace MtApi5
{
    static class MtConverters
    {
        #region Values Converters
        public static Mt5Quote Parse(this MtQuote quote)
        {
            return (quote != null) ? new Mt5Quote(quote.Instrument, quote.Bid, quote.Ask) : null;
        }

        public static bool ParseResult(this string inputString, char separator, out MqlTradeResult result)
        {
            bool retVal = false;
            result = null;

            if (string.IsNullOrEmpty(inputString) == false)
            {
                string[] values = inputString.Split(separator);
                if (values.Length == 10)
                {
                    try
                    {
                        retVal = int.Parse(values[0]) != 0;

                        uint retcode = uint.Parse(values[1]);
                        ulong deal = ulong.Parse(values[2]);
                        ulong order = ulong.Parse(values[3]);
                        double volume = double.Parse(values[4]);
                        double price = double.Parse(values[5]);
                        double bid = double.Parse(values[6]);
                        double ask = double.Parse(values[7]);
                        string comment = values[8];
                        uint request_id = uint.Parse(values[9]);

                        result = new MqlTradeResult(retcode, deal, order, volume, price, bid, ask, comment, request_id);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return retVal;
        }

        public static bool ParseResult(this string inputString, char separator, out MqlTradeCheckResult result)
        {
            bool retVal = false;
            result = null;

            if (string.IsNullOrEmpty(inputString) == false)
            {
                string[] values = inputString.Split(separator);
                if (values.Length == 10)
                {
                    try
                    {
                        retVal = int.Parse(values[0]) != 0;

                        uint retcode = uint.Parse(values[1]);
                        double balance = double.Parse(values[2]);
                        double equity = double.Parse(values[3]);
                        double profit = double.Parse(values[4]);
                        double margin = double.Parse(values[5]);
                        double margin_free = double.Parse(values[6]);
                        double margin_level = double.Parse(values[7]);
                        string comment = values[8];

                        result = new MqlTradeCheckResult(retcode, balance, equity, profit, margin, margin_free, margin_level, comment);
                    }
                    catch (Exception)
                    {
                        retVal = false;
                    }
                }
            }

            return retVal;
        }

        public static bool ParseResult(this string inputString, char separator, out double result)
        {
            bool retVal = false;
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
                    catch (Exception)
                    {
                        retVal = false;
                    }
                }
            }

            return retVal;
        }

        public static bool ParseResult(this string inputString, char separator, out DateTime from, out DateTime to)
        {
            bool retVal = false;

            from = new DateTime();
            to = new DateTime();

            if (string.IsNullOrEmpty(inputString) == false)
            {
                string[] values = inputString.Split(separator);
                if (values.Length == 3)
                {
                    try
                    {
                        retVal = int.Parse(values[0]) != 0;

                        int iFrom = int.Parse(values[1]);
                        from = Mt5TimeConverter.ConvertFromMtTime(iFrom);

                        int iTo= int.Parse(values[2]);
                        to = Mt5TimeConverter.ConvertFromMtTime(iTo);
                    }
                    catch (Exception)
                    {
                        retVal = false;
                    }
                }
            }

            return retVal;
        }

        public static ArrayList ToArrayList(this MqlTradeRequest request)
        {
            if (request == null)
                throw new ArgumentNullException();

            int exp = Mt5TimeConverter.ConvertToMtTime(request.Expiration);

            return new ArrayList { (int)request.Action, request.Magic, request.Order, request.Symbol, request.Volume
                , request.Price, request.Stoplimit, request.Sl, request.Tp, request.Deviation, (int)request.Type
                , (int)request.Type_filling, (int)request.Type_time, exp, request.Comment };
        }

        #endregion
    }
}
