using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Advantech.Motion;

namespace AdvanTech.PCI1203
{
    public class PCIBase
    {
        #region Variable

         public static DEV_LIST[] CurAvailableDevs = new DEV_LIST[Motion.MAX_DEVICES];
         public static IntPtr[] m_DeviceHandle = new IntPtr[Motion.MAX_DEVICES];

        #endregion
  
    }
}
