using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtApi5.Requests
{
    internal class SymbolInfoTickRequest : RequestBase
    {
        public override RequestType RequestType => RequestType.SymbolInfoTick;

        public string SymbolName { get; set; }
    }
}
