using System;
using System.Threading.Tasks;

namespace MtApi
{
    static class ExtensionMethods
    {
        #region Event Methods

        public static Task FireEventAsync(this MtApiQuoteHandler evenHandler, object sender, string symbol, double bid, double ask)
        {
            return Task.Factory.StartNew(() =>
            {
                evenHandler?.Invoke(sender, symbol, bid, ask);
            });
        }

        public static Task FireEventAsync(this EventHandler eventHandler, object sender)
        {
            return Task.Factory.StartNew(() =>
            {
                eventHandler?.Invoke(sender, EventArgs.Empty);
            });
        }

        public static Task FireEventAsync<T>(this EventHandler<T> eventHandler, object sender, T e)
            where T : EventArgs
        {
            return Task.Factory.StartNew(() =>
            {
                eventHandler?.Invoke(sender, e);
            });
        }

        #endregion

        public static MtQuote Convert(this MTApiService.MtQuote quote)
        {
            return (quote != null) ? new MtQuote(quote.Instrument, quote.Bid, quote.Ask) : null;
        }
    }
}
