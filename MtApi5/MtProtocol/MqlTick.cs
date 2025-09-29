namespace MtApi5.MtProtocol
{
    public class MtTick
    {
        public double Bid { get; set; }           // Current Bid price
        public double Ask { get; set; }           // Current Ask price
        public long Time { get; set; }            // Time of the last prices update
        public double Last { get; set; }          // Price of the last deal (Last)
        public ulong Volume { get; set; }         // Volume for the current Last price
        public double VolumeReal { get; set; }    // Volume for the current Last price with greater accuracy
        public uint Flags { get; set; }           // Tick flags
    }

}
