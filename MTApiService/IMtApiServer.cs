using System.Collections.Generic;

namespace MTApiService
{
    public interface IMtApiServer
    {
        MtResponse SendCommand(MtCommand command);
        List<MtQuote> GetQuotes();
    }
}
