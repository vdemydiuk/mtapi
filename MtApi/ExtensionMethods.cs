using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtApi
{
    static class ExtensionMethods
    {
        #region Event Methods

        public static async void FireEvent(this MtApiQuoteHandler evenHandler, object sender, string symbol, double bid, double ask)
        {
            if (evenHandler != null)
            {
                await Task.Factory.StartNew(() =>
                {
                    evenHandler(sender, symbol, bid, ask);
                });
            }
        }

        public static async void FireEvent(this EventHandler eventHandler, object sender)
        {
            if (eventHandler != null)
            {
                await Task.Factory.StartNew(() =>
                {
                    eventHandler(sender, EventArgs.Empty);
                });
            }
        }

        public static async void FireEvent<T>(this EventHandler<T> eventHandler, object sender, T e)
            where T : EventArgs
        {
            if (eventHandler != null)
            {
                await Task.Factory.StartNew(() =>
                {
                    eventHandler(sender, e);    
                });
            }
        }

        #endregion

        public static MtQuote Parse(this MTApiService.MtQuote quote)
        {
            return (quote != null) ? new MtQuote(quote.Instrument, quote.Bid, quote.Ask) : null;
        }
    }
}
