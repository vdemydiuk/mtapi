namespace MtApi5.Requests
{
    internal class SymbolInfoStringRequest : RequestBase
    {
        public override RequestType RequestType => RequestType.SymbolInfoString;

        public string SymbolName { get; set; }
        public ENUM_SYMBOL_INFO_STRING PropId { get; set; }
    }
}