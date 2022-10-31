namespace GreatechApp.Core.Events
{
    using Prism.Events;
    using GreatechApp.Core.Enums;

    public class ValidateLogin : PubSubEvent<bool>
    {

    }

    public class ResetError : PubSubEvent
    {

    }

    public class MachineState : PubSubEvent<MachineStateType>
    {

    }



    public class MachineMode : PubSubEvent<bool>
    {

    }

    public class OEEDataUpdate : PubSubEvent
    {

    }

    public class DataSimulation : PubSubEvent<bool>
    {

    }

    public class UpdateUI : PubSubEvent
    {

    }

    public class UpdateData : PubSubEvent
    {

    }

    public class TCPIPStatus : PubSubEvent<TCPIPStatus>
    {
        public string IpAddress;
        public TCPIPNo TCPIPNo;
        public int Port;
        public ConnectionStatus ConnectionStatus;
    }

    public class TCPIPMsg : PubSubEvent<TCPIPMsg>
    {
        public TCPIPNo TCPIPNo;
        public string IpAddress;
        public int Port;
        public Enums.MessageType _MessageType;
        public string Message;
    }

    public class ErrorMsg : PubSubEvent<ErrorMsg>
    {
        public string[] ErrMsg;
        public ErrListFileNames Module;

    }
    public class MotionStatus : PubSubEvent<MotionStatus>
    {
        public bool[] Alarm;
        public bool[] InPos;
        public bool[] Ready;
        public bool[] PosLmt;
        public bool[] NegLmt;
        public AxisState[] AxisState;
        public double[] CurrentPos;
        public bool[] ServoStatus;

    }

    public class PCICardConnection : PubSubEvent<bool>
    {

    }

    public class IOConnection : PubSubEvent<bool>
    {

    }
    public class IOStatus : PubSubEvent<IOStatus>
    {
        public bool[,] InputList;
        public bool[,] OutputList;
    }

    public class IIOStatus : PubSubEvent<IIOStatus>
    {
        public bool[] InputList;
        public bool[] OutputList;
    }

    public class IOCardConnection : PubSubEvent<bool>
    {

    }
}
