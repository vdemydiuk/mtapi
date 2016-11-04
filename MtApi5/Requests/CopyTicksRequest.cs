namespace MtApi5.Requests
{
    internal class CopyTicksRequest: RequestBase
    {
        public override RequestType RequestType => RequestType.CopyTicks;

        public string SymbolName { get; set; }
        public int Flags { get; set; }
        public ulong From { get; set; }
        public uint Count { get; set; }
    }
}