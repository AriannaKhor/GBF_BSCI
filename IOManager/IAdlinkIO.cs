using IOManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionTest.IO_Manager
{
    public interface IAdlinkIO : IBaseIO
    {
        bool ScanDevice();
        bool SlaveStatus(int mod_No);
        bool Initialise();
        bool Closing();
        int ModuleNo { get; set; }
        int SubModuleNo { get; set; }
        int Board_ID { get; set; }
    }
}
