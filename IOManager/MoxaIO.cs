using ConfigManager;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Modal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace IOManager
{
    public class MoxaIO : BaseIO, IMoxaIO
    {
        #region  Variables
        internal int m_NumOfIOCardInput = 0;
        internal int m_NumOfIOCardOutput = 0;
        internal bool m_FailTriggerOutBit;
        internal const ushort Port = 502;
        internal const int MaxCardSlot = 100;
        internal const int MaxBit = 16;
        internal string Password = "";
        internal uint TimeOut = 2000;

        internal int[] DI_hConnection = null;
        internal int[] DO_hConnection = null;

        internal string[] m_DI_IpAddress = new string[MaxCardSlot];
        internal string[] m_DO_IpAddress = new string[MaxCardSlot];

        internal uint[] dwGetDIValue = null;
        internal uint[] dwGetDOValue = null;

        internal uint[] retDIValue = new uint[10];
        internal uint[] retDOValue = new uint[10];

        internal bool[,] InputBitStatus = new bool[MaxCardSlot, MaxBit];
        internal bool[,] OutputBitStatus = new bool[MaxCardSlot, MaxBit];

        static public bool[,] SetOutput = new bool[MaxCardSlot, MaxBit];
        Thread[] InRefresh = new Thread[MaxCardSlot];
        Thread[] OutRefresh = new Thread[MaxCardSlot];

        public bool IsConnected { set; get; } = false;

        #endregion Variables

        public MoxaIO()
        {
            SystemConfig SysCfg = SystemConfig.Open(@"..\Config Section\General\System.Config");

            m_NumOfIOCardInput = SysCfg.IOInDevices.Count;
            m_NumOfIOCardOutput = SysCfg.IOOutDevices.Count;

            for (int i = 0; i < SysCfg.IOInDevices.Count; i++)
                m_DI_IpAddress[i] = SysCfg.IOInDevices[i].DeviceAddress;

            for (int i = 0; i < SysCfg.IOOutDevices.Count; i++)
                m_DO_IpAddress[i] = SysCfg.IOOutDevices[i].DeviceAddress;

            MaxBitPerDevice = 16;
        }

        public override void StartScanIO()
        {
            for (int i = 0; i < m_NumOfIOCardInput; i++)
            {
                InRefresh[i] = new Thread(ReadInput);
                InRefresh[i].Start(i);
            }
            for (int i = 0; i < m_NumOfIOCardOutput; i++)
            {
                OutRefresh[i] = new Thread(ReadOutput);
                OutRefresh[i].Start(i);
            }
        }

        public override bool OpenDevice()
        {
            return InitMoxaIO();
        }

        public override bool ReadBit(int bit)
        {
            return MoxaInputBitStatus(bit);
        }

        public override bool ReadOutBit(int bit)
        {
            return MoxaOutputBitStatus((int)bit);
        }

        public override bool WriteBit(int? bit, bool state)
        {
            lock (this)
            {
                if (bit == null)
                {
                    return false;
                }
                uint _state = Convert.ToUInt32(state);
                int slot = (ushort)bit / MaxBitPerDevice;
                byte _output = (byte)(bit % MaxBitPerDevice);
                SetMoxaOutput(slot, _output, _state);
                return true;
            }
        }

        bool InitMoxaIO()
        {
            int ret;
            IsConnected = true;
            try
            {
                ret = MXIO.MXEIO_Init();

                DI_hConnection = new int[m_NumOfIOCardInput];
                DO_hConnection = new int[m_NumOfIOCardOutput];
                dwGetDIValue = new uint[m_NumOfIOCardInput + 1];
                dwGetDOValue = new uint[m_NumOfIOCardOutput + 1];

                for (int _i = 0; _i < DI_hConnection.Length; _i++)
                {
                    ret = MXIO.MXEIO_E1K_Connect(Encoding.UTF8.GetBytes(m_DI_IpAddress[_i]), Port, TimeOut, out DI_hConnection[_i], Encoding.UTF8.GetBytes(Password));
                    if (ret != MXIO.MXIO_OK)
                    {
                        Debug.WriteLine("InitMoxaIO, MXEIO_E1K_Connect - return code :" + ret);
                        IsConnected = false;
                        ErrorMsg = "Connect Input Module Fail\r\n";
                    }
                    else
                    {
                        Console.WriteLine($"MOXA_DI {_i}: Connected!");
                    }
                }

                for (int _i = 0; _i < DO_hConnection.Length; _i++)
                {
                    ret = MXIO.MXEIO_E1K_Connect(Encoding.UTF8.GetBytes(m_DO_IpAddress[_i]), Port, TimeOut, out DO_hConnection[_i], Encoding.UTF8.GetBytes(Password));
                    if (ret != MXIO.MXIO_OK)
                    {
                        Debug.WriteLine("InitMoxaIO, MXEIO_E1K_Connect - return code :" + ret);
                        IsConnected = false;
                        ErrorMsg = "Connect Output Module Fail\r\n";
                    }
                    else
                    {
                        Console.WriteLine($"MOXA_DO {_i}: Connected!");
                    }
                }

                return IsConnected;
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message.ToString();
                return false;
            }
        }

        private bool MoxaInputBitStatus(int bit)
        {
            lock (this)
            {
                int slot = bit / MaxBitPerDevice;
                int index = bit % MaxBitPerDevice;
                return InputBitStatus[slot, index];
            }
        }

        private bool MoxaOutputBitStatus(int bit)
        {
            lock (this)
            {
                int slot = bit / MaxBitPerDevice;
                int index = bit % MaxBitPerDevice;
                return OutputBitStatus[slot, index];
            }
        }

        void ReadInput(object slot)
        {
            int ret;
            int bitNo = 1;
            int Slot = (int)slot;
            byte[] byteStatus = new byte[1];

            do
            {
                Thread.Sleep(1);
                if (IsConnected)
                {
                    try
                    {
                        ret = MXIO.E1K_DI_Reads(DI_hConnection[Slot], 0, 16, ref dwGetDIValue[Slot]);
                        if (ret != MXIO.MXIO_OK)
                        {
                            ErrorMsg = "ReadMoxaInput(), m_IsConnected = false.";
                            MXIO.MXEIO_CheckConnection(DI_hConnection[Slot], TimeOut, byteStatus);
                            if (byteStatus[0] == 1)
                            {
                                MXIO.MXEIO_E1K_Connect(Encoding.UTF8.GetBytes(m_DI_IpAddress[Slot]), Port, TimeOut, out DI_hConnection[Slot], Encoding.UTF8.GetBytes(Password));
                            }
                        }

                        for (int i = 0; i <= 15; i++)
                        {
                            if ((dwGetDIValue[Slot] & bitNo) == bitNo)
                            {
                                InputBitStatus[Slot, i] = true;
                            }
                            else
                            {
                                InputBitStatus[Slot, i] = false;
                            }
                            bitNo *= 2;
                        }
                        bitNo = 1;
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

        void ReadOutput(object slot)
        {
            int ret;
            int bitNo = 1;
            int Slot = (int)slot;
            byte[] byteStatus = new byte[1];

            do
            {
                Thread.Sleep(1);
                if (IsConnected)
                {
                    try
                    {
                        ret = MXIO.E1K_DO_Reads(DO_hConnection[Slot], 0, 16, dwGetDOValue);
                        if (ret != MXIO.MXIO_OK)
                        {
                            ErrorMsg = "ReadMoxaOutput(), m_IsConnected = false.";
                            MXIO.MXEIO_CheckConnection(DO_hConnection[Slot], TimeOut, byteStatus);
                            if (byteStatus[0] == 1)
                            {
                                MXIO.MXEIO_E1K_Connect(Encoding.UTF8.GetBytes(m_DO_IpAddress[Slot]), Port, TimeOut, out DO_hConnection[Slot], Encoding.UTF8.GetBytes(Password));
                            }
                        }

                        for (int i = 0; i <= 15; i++)
                        {
                            if ((dwGetDOValue[Slot] & bitNo) == bitNo)
                            {
                                OutputBitStatus[Slot, i] = true;
                            }
                            else
                            {
                                OutputBitStatus[Slot, i] = false;
                            }
                            bitNo *= 2;
                        }
                        bitNo = 1;

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

        private void SetMoxaOutput(int slot, byte bit, uint dwSetDOValue)
        {
            try
            {
                int ret = 0;
                ret = MXIO.E1K_DO_Writes(DO_hConnection[slot], bit, 0, dwSetDOValue); //Send Output to MOXA

                if (ret != MXIO.MXIO_OK)
                {
                    ErrorMsg = string.Format("SetMoxaOutput(), m_IsConnected = false, m_Slot= {0},m_Bit= {1}, m_dwSetDOValue ={2}", slot, bit, dwSetDOValue);
                    m_FailTriggerOutBit = true;
                }
                else
                {
                    m_FailTriggerOutBit = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message.ToString();
                return;
            }
        }

    }
}
