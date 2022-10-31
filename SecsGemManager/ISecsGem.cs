using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecsGemManager
{
    public interface ISecsGem
    {
       void UpdateConnectionState(bool Connected);
        
       void UpdateControlState(string ControlState);

       void UpdateProcessState(string ProcID);

        void UpdateReceivedRecipe(string StreamFunction, string RecipeName);

        void UpdateReceivedTerminalTextBox(string Message);

        void LoadInitSpoolingState();








    }
}
