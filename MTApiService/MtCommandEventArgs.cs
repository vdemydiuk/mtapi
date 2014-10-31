using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTApiService
{
    public class MtCommandExecuteEventArgs: EventArgs
    {
        public MtCommand Command { get; private set; }
        public MtResponse Response { get; private set; }

        public MtCommandExecuteEventArgs(MtCommand command, MtResponse response)
        {
            Command = command;
            Response = response;
        }
    }
}
