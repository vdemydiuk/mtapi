using System;

namespace MtApi
{
    public class TimeBarArgs: EventArgs
    {
        internal TimeBarArgs(int expertHandle, MtTimeBar timeBar)
            : this(timeBar)
        {
            ExpertHandle = expertHandle;
        }

        public TimeBarArgs(MtTimeBar timeBar)
        {
            TimeBar = timeBar;
        }

        public int ExpertHandle { get; }
        public MtTimeBar TimeBar { get; }
    }
}
