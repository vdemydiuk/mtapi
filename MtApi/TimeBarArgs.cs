using System;

namespace MtApi
{
    public class TimeBarArgs: EventArgs
    {
        public TimeBarArgs(MtTimeBar timeBar)
        {
            TimeBar = timeBar;
        }

        public MtTimeBar TimeBar { get; private set; }
    }
}
