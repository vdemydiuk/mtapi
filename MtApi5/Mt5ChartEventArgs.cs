using System;

namespace MtApi5
{
    public class Mt5ChartEventArgs: EventArgs
    {
        internal Mt5ChartEventArgs(int expertHandle, int eventId, long lparam, double dparam, string sparam)
        {
            ExpertHandle = expertHandle;
            EventId = eventId;
            Lparam = lparam;
            Dparam = dparam;
            Sparam = sparam;
        }

        public int ExpertHandle { get; }
        public int EventId { get; }
        public long Lparam { get; }
        public double Dparam { get; }
        public string Sparam { get; }
    }
}
