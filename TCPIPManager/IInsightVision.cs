using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPIPManager
{
    public interface IInsightVision
    {
        void TriggerVisCapture();
        bool VisConnectionStatus();
        void ConnectVision();
    }
}
