using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialPortManager
{
    public interface ISerialPort
    {
        bool OpenSerialPort();
        void CloseSerialPort();
        bool IsPortOpen();
        bool Send(string Message);
    }
}
