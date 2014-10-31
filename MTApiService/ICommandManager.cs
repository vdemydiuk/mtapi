using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTApiService
{
    public interface ICommandManager
    {
        void EnqueueCommand(MtCommand command);
        MtCommand DequeueCommand();

        void OnCommandExecuted(MtExpert expert, MtCommand command, MtResponse response);
    }
}
