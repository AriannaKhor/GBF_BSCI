using ConfigManager;
using GreatechApp.Core.Events;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialPortManager
{
    public class SerialPortBase : BindableBase , ISerialPort
    {
        #region Variable
        private string BufferSTR;
        public bool IsDataReceived = false;
        IEventAggregator m_Event;
        private Parity SerialPortParity;
        private StopBits SerialPortStopBits;
        private Handshake SerialPortHandshake;
        private SerialPortInfo m_serialPortInfo;
        private SerialPortMsg m_SerialPortMsg;
        private StringBuilder m_ErrMsg = new StringBuilder();
        private SystemConfig m_sysCfg = SystemConfig.Open(@"..\Config Section\General\System.Config");
        private SerialPort m_SerialPort = new SerialPort();
        #endregion

        #region Properties
        public string Err_Msg
        {
            set
            {
                m_ErrMsg.Remove(0, m_ErrMsg.Length);
                m_ErrMsg.Append(value);
            }
            get { return m_ErrMsg.ToString(); }
        }
        private ObservableCollection<SerialPortInfo> m_SerialPortBaseinfo;
        public ObservableCollection<SerialPortInfo> SerialPortBaseinfo
        {
            get { return m_SerialPortBaseinfo; }
            set
            {
                SetProperty(ref m_SerialPortBaseinfo, value);
            }
        }
        #endregion

        #region Constructor
        public SerialPortBase(IEventAggregator eventAggregator)
        {
            m_Event = eventAggregator;
            GetSerialPortInfo();
        }
        #endregion

        #region Method

        #region Connect Serial Port
        public void Connect()
        {
            try
            {
                if (!m_SerialPort.IsOpen)
                {
                    m_SerialPort.Open();
                }
            }
            catch
            {
                m_SerialPort.Close();
            }
        }
        #endregion

        #region Get Serial Port Info
        public void GetSerialPortInfo()
        {
            int SerialPortCount = m_sysCfg.SerialPortDevices.Count;
            Int32.TryParse(m_sysCfg.SerialPortDevices[SerialPortCount].BaudRate, out int baudrate);
            Int32.TryParse(m_sysCfg.SerialPortDevices[SerialPortCount].Parity, out int parity);
            Int32.TryParse(m_sysCfg.SerialPortDevices[SerialPortCount].StopBit, out int stopbit);
            Int32.TryParse(m_sysCfg.SerialPortDevices[SerialPortCount].Handshake, out int handshake);

            #region Check for Parity
            switch (parity)
            {
                case (int)Parity.None:
                    SerialPortParity = Parity.None;
                    break;
                case (int)Parity.Odd:
                    SerialPortParity = Parity.Odd;
                    break;
                case (int)Parity.Even:
                    SerialPortParity = Parity.Even;
                    break;
                case (int)Parity.Mark:
                    SerialPortParity = Parity.Mark;
                    break;
                case (int)Parity.Space:
                    SerialPortParity = Parity.Space;
                    break;
            }
            #endregion

            #region Check For Stop Bits
            switch (stopbit)
            {
                case (int)StopBits.None:
                    SerialPortStopBits = StopBits.None;
                    break;
                case (int)StopBits.One:
                    SerialPortStopBits = StopBits.One;
                    break;
                case (int)StopBits.Two:
                    SerialPortStopBits = StopBits.Two;
                    break;
                case (int)StopBits.OnePointFive:
                    SerialPortStopBits = StopBits.OnePointFive;
                    break;
            }
            #endregion

            #region Check for Handshake
            switch (handshake)
            {
                case (int)Handshake.None:
                    SerialPortHandshake = Handshake.None;
                    break;
                case (int)Handshake.XOnXOff:
                    SerialPortHandshake = Handshake.XOnXOff;
                    break;
                case (int)Handshake.RequestToSendXOnXOff:
                    SerialPortHandshake = Handshake.RequestToSendXOnXOff;
                    break;
                case (int)Handshake.RequestToSend:
                    SerialPortHandshake = Handshake.RequestToSend;
                    break;
            }
            #endregion

            m_SerialPortBaseinfo.Add(new SerialPortInfo
            {
                ID = m_sysCfg.SerialPortDevices[SerialPortCount].ID,
                Name = m_sysCfg.SerialPortDevices[SerialPortCount].Name,
                BaudRate = baudrate,
                Parity = SerialPortParity,
                PortName = m_sysCfg.SerialPortDevices[SerialPortCount].PortName,
                StopBit = SerialPortStopBits,
                Handshake = SerialPortHandshake,
                Format = m_sysCfg.SerialPortDevices[SerialPortCount].Format,
                Type = m_sysCfg.SerialPortDevices[SerialPortCount].Type,
            });
        }
        #endregion

        #region Open Serial Port
        public bool OpenSerialPort()
        {
            try
            {
                GetSerialPortInfo();
                m_SerialPort.PortName = m_serialPortInfo.PortName;
                m_SerialPort.BaudRate = m_serialPortInfo.BaudRate;
                m_SerialPort.Parity = m_serialPortInfo.Parity;
                m_SerialPort.StopBits = m_serialPortInfo.StopBit;
                m_SerialPort.Handshake = m_serialPortInfo.Handshake;
                m_SerialPort.DataReceived += Serial_Port_DataReceived;
                Connect();
                return true;
            }
            catch (Exception ex)
            {
                Err_Msg = ex.Message;
                return false;
            }
        }
        #endregion

        #region Close Serial Port
        public void CloseSerialPort()
        {
            m_SerialPort.Close();
        }
        #endregion

        #region Check Port Open 
        public bool IsPortOpen()
        {
            return m_SerialPort.IsOpen;
        }
        #endregion

        #region Send Command
        public bool Send(string Message)
        {
            try
            {
                //byte[] Command = Encoding.ASCII.GetBytes(Message);
                //byte[] Command = StringToByteArray(Message);
                //m_SerialPort.Write(Command, 0, Message.Length);
                m_SerialPort.Write(Message + "<CR><LF>");
                return true;
            }
            catch (Exception ex)
            {
                Err_Msg = ex.Message;
                return false;
            }
        }
        #endregion

        #region Convert String to Byte Array
        //public byte[] StringToByteArray(string hex)
        //{
        //    return Enumerable.Range(0, hex.Length)
        //                     .Where(x => x % 2 == 0)
        //                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
        //                     .ToArray();
        //}
        #endregion

        #endregion

        #region Event

        #region On Data Received
        private void Serial_Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] Buffer = new byte[m_SerialPort.BytesToRead];
            m_SerialPort.Read(Buffer, 0, Buffer.Length);

            try
            {
                if (Buffer.Length > 0)
                {
                    IsDataReceived = true;

                    BufferSTR = BufferSTR + Encoding.UTF8.GetString(Buffer);
                    string[] _BufferSTR = BufferSTR.Split('\r');
                    RaiseOnDataReceived(_BufferSTR[0]);

                    BufferSTR = null;
                }
            }
            catch (Exception ex)
            {
                Err_Msg = ex.Message;
            }
        }

        public virtual void RaiseOnDataReceived(string Message)
        {
            m_SerialPortMsg.PortName = m_serialPortInfo.PortName;
            m_SerialPortMsg.Message = Message;
            m_Event.GetEvent<SerialPortMsg>().Publish(m_SerialPortMsg);
        }
        #endregion

        #endregion
    }

    public class SerialPortInfo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int BaudRate { get; set; }
        public Parity Parity { get; set; }
        public string PortName { get; set; }
        public StopBits StopBit { get; set; }
        public Handshake Handshake { get; set; }
        public string Format { get; set; }
        public string Type { get; set; }
    }
}
