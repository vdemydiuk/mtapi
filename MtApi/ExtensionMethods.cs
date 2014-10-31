using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtApi
{
    static class ExtensionMethods
    {
        #region Event Methods
        public static void FireEvent(this EventHandler eventHandler, object sender)
        {
            if (eventHandler != null)
            {
                eventHandler(sender, EventArgs.Empty);
            }
        }

        public static void FireEvent<T>(this EventHandler<T> eventHandler, object sender, T e)
            where T : EventArgs
        {
            if (eventHandler != null)
            {
                eventHandler(sender, e);
            }
        }

        #endregion

        public static MtQuote Parse(this MTApiService.MtQuote quote)
        {
            return (quote != null) ? new MtQuote(quote.Instrument, quote.Bid, quote.Ask) : null;
        }
    }
}
