using ConfigManager;
using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IOManager
{
    public class WagoIO : BaseIO, IWagoIO
    {
        internal string[] m_DeviceAddress;
        internal string[] m_DevicePort;
        internal string[] m_InputStartAddress;
        internal string[] m_OutputStartAddress;
        internal int[] m_NumOfInputBit;
        internal int[] m_NumOfOutputBit;

        internal bool[][] InputBitStatus;
        internal bool[][] OutputBitStatus;

        ModbusIpMaster[] m_ModBus;
        Thread[] IORefresh;
        Thread[] OutRefresh;

        public bool IsConnected { set; get; }


        public WagoIO()
        {
            SystemConfig SysCfg = SystemConfig.Open(@"..\Config Section\General\System.Config");

            m_DeviceAddress = new string[SysCfg.IOWagoDevices.Count];
            m_DevicePort = new string[SysCfg.IOWagoDevices.Count];
            m_InputStartAddress = new string[SysCfg.IOWagoDevices.Count];
            m_OutputStartAddress = new string[SysCfg.IOWagoDevices.Count];
            m_NumOfInputBit = new int[SysCfg.IOWagoDevices.Count];
            m_NumOfOutputBit = new int[SysCfg.IOWagoDevices.Count];
            InputBitStatus = new bool[SysCfg.IOWagoDevices.Count][];
            OutputBitStatus = new bool[SysCfg.IOWagoDevices.Count][];
            m_ModBus = new ModbusIpMaster[SysCfg.IOWagoDevices.Count];
            IORefresh = new Thread[SysCfg.IOWagoDevices.Count];
            OutRefresh = new Thread[SysCfg.IOWagoDevices.Count];

            for (int i = 0; i < SysCfg.IOWagoDevices.Count; i++)
            {
                m_DeviceAddress[i] = SysCfg.IOWagoDevices[i].DeviceAddress;
                m_DevicePort[i] = SysCfg.IOWagoDevices[i].DevicePort;
                m_InputStartAddress[i] = SysCfg.IOWagoDevices[i].InputStartAddress;
                m_OutputStartAddress[i] = SysCfg.IOWagoDevices[i].OutputStartAddress;
                m_NumOfInputBit[i] = SysCfg.IOWagoDevices[i].NumOfInputModules * SysCfg.DigitalIO.MaxBitPerPort;
                m_NumOfOutputBit[i] = SysCfg.IOWagoDevices[i].NumOfOutputModules * SysCfg.DigitalIO.MaxBitPerPort;

                InputBitStatus[i] = new bool[m_NumOfInputBit[i]];
                OutputBitStatus[i] = new bool[m_NumOfOutputBit[i]];
            }

            MaxBitPerDevice = 1024;
        }

        public override void StartScanIO()
        {
            for (int i = 0; i < m_DeviceAddress.Count(); i++)
            {
                IORefresh[i] = new Thread(ReadInput);
                IORefresh[i].Start(i);

                OutRefresh[i] = new Thread(ReadOutput);
                OutRefresh[i].Start(i);
            }
        }

        public override bool OpenDevice()
        {
            for (int i = 0; i < m_DeviceAddress.Count(); i++)
            {
                TcpClient client = new TcpClient();
                IsConnected = client.ConnectAsync(m_DeviceAddress[i], Convert.ToInt32(m_DevicePort[i])).Wait(1000);

                if (!IsConnected)
                    return false;

                m_ModBus[i] = ModbusIpMaster.CreateIp(client);
            }

            return true;
        }

        void ReadInput(object device)
        {
            do
            {
                Thread.Sleep(1);
                if (IsConnected)
                {
                    try
                    {
                        ushort InStartAddress = Convert.ToUInt16(m_InputStartAddress[(int)device]);
                        ushort InRegisterCount = Convert.ToUInt16(m_NumOfInputBit[(int)device]);
                        InputBitStatus[(int)device] = m_ModBus[(int)device].ReadInputs(InStartAddress, InRegisterCount);
                    }
                    catch (Exception ex)
                    {
                        ErrorMsg = ex.Message.ToString();
                        return;
                    }
                }
            }
            while (true);
        }

        void ReadOutput(object device)
        {
            do
            {
                Thread.Sleep(1);
                if (IsConnected)
                {
                    try
                    {
                        ushort OutStartAddress = Convert.ToUInt16(m_OutputStartAddress[(int)device]);
                        ushort OutRegisterCount = Convert.ToUInt16(m_NumOfOutputBit[(int)device]);
                        OutputBitStatus[(int)device] = m_ModBus[(int)device].ReadInputs(OutStartAddress, OutRegisterCount);
                    }
                    catch (Exception ex)
                    {
                        ErrorMsg = ex.Message.ToString();
                        return;
                    }
                }
            }
            while (true);
        }

        public override bool ReadBit(int bit)
        {
            lock (this)
            {
                int slot = bit / MaxBitPerDevice;
                int index = bit % MaxBitPerDevice;
                return InputBitStatus[slot][index];
            }
        }

        public override bool ReadOutBit(int bit)
        {
            lock (this)
            {
                int slot = bit / MaxBitPerDevice;
                int index = bit % MaxBitPerDevice;
                return OutputBitStatus[slot][index];
            }
        }


        public override bool WriteBit(int? bit, bool state)
        {
            lock (this)
            {
                try
                {
                    if (IsConnected)
                    {
                        if (bit == null)
                        {
                            return false;
                        }

                        int slot = (ushort)bit / MaxBitPerDevice;
                        ushort _output = (ushort)(bit % MaxBitPerDevice + Convert.ToInt32(m_OutputStartAddress[slot]));

                        m_ModBus[slot].WriteSingleCoil(_output, state);
                        return true;
                    }
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }


    }
}
