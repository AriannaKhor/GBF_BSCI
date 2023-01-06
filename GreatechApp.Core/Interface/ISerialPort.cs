using System.Collections.Generic;
using System.IO.Ports;

namespace GreatechApp.Core.Interface
{
    public interface ISerialPort
    {
        List<SerialPort> serialPortCollection { get; set; }
    }
}
