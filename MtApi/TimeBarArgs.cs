using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
