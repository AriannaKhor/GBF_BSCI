using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreatechApp.Core.Interlock
{
    public interface IInterlock
    {
        bool CheckIOInterlock(int IONum);
       
    }
}
