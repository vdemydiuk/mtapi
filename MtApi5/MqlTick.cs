using MtApi5.MtProtocol;

namespace MtApi5
{
    public class MqlTick
    {
        public MqlTick(DateTime time, double bid, double ask, double last, ulong volume)
            : this(time, bid, ask, last, volume, 0)
        {
        }

        public MqlTick(DateTime time, double bid, double ask, double last, ulong volume, ENUM_TICK_FLAGS flags)
        {
            MtTime = Mt5TimeConverter.ConvertToMtTime(time);
            this.bid = bid;
            this.ask = ask;
            this.last = last;
            this.volume = volume;
            this.flags = flags;
        }

        public MqlTick()
        {
        }

        internal MqlTick(MtTick? tick)
        {
            if (tick != null)
            {
                MtTime = tick.Time;
                bid = tick.Bid;
                ask = tick.Ask;
                last = tick.Last;
                volume = tick.Volume;
                volume_real = tick.VolumeReal;
                flags = (ENUM_TICK_FLAGS)tick.Flags;
            }
        }

        public long MtTime { get; set; }          // Time of the last prices update

        public double bid { get; set; }           // Current Bid price
        public double ask { get; set; }           // Current Ask price
        public double last { get; set; }          // Price of the last deal (Last)
        public ulong volume { get; set; }         // Volume for the current Last price
        public double volume_real { get; set; }   // Volume for the current Last price with greater accuracy 
        public ENUM_TICK_FLAGS flags { get; set; } // Tick flags (used for analyzing to find out what data have been changed)

        public DateTime time => Mt5TimeConverter.ConvertFromMtTime(MtTime);
    }
}
