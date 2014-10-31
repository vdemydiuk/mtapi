using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTApiService
{
    public interface IMtApiServer
    {
        MtResponse SendCommand(MtCommand command);
        IEnumerable<MtQuote> GetQuotes();
    }
}
