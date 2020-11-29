using System;
using MtApi.Events;

namespace MtApi
{
    public class ChartEventArgs : EventArgs
    {
        internal ChartEventArgs(int expertHandle, MtChartEvent chartEvent)
        {
            ExpertHandle = expertHandle;
            ChartId = chartEvent.ChartId;
            EventId = chartEvent.EventId;
            Lparam = chartEvent.Lparam;
            Dparam = chartEvent.Dparam;
            Sparam = chartEvent.Sparam;
        }

        public int ExpertHandle { get; }
        public long ChartId { get; }
        public int EventId { get; }
        public long Lparam { get; }
        public double Dparam { get; }
        public string Sparam { get; }
    }
}