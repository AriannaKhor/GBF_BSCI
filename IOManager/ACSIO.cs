using ACSApi;
using ConfigManager;
using System;
using System.Threading;

namespace IOManager
{
    public class ACSIO : BaseIO, IACSIO
    {
        internal int m_NumOfIOCardInput = 0;
        internal int m_NumOfIOCardOutput = 0;
        internal const int MaxCardSlot = 100;
        internal const int MaxBit = 16;
        Thread[] InRefresh = new Thread[MaxCardSlot];
        Thread[] OutRefresh = new Thread[MaxCardSlot];

        internal bool[,] InputBitStatus = new bool[MaxCardSlot, MaxBit];
        internal bool[,] OutputBitStatus = new bool[MaxCardSlot, MaxBit];

        public ACSLibApi m_ACS;

        public ACSIO()
        {
            m_ACS = new ACSLibApi();
            SystemConfig systemConfig = SystemConfig.Open(@"..\Config Section\General\System.Config");
            m_NumOfIOCardInput = systemConfig.IOInDevices.Count;
            m_NumOfIOCardOutput = systemConfig.IOOutDevices.Count;
            MaxPortNum = systemConfig.DigitalIO.MaxPortNum;
            MaxBitPerDevice = systemConfig.DigitalIO.MaxBitPerPort;
        }

        public override bool OpenDevice()
        {
            return m_ACS.IsConnected(0);
        }

        public override void StartScanIO()
        {
            for (int i = 0; i < m_NumOfIOCardInput; i++)
            {
                InRefresh[i] = new Thread(GetInBitStatus);
                InRefresh[i].Start(i);
            }

            for (int i = 0; i < m_NumOfIOCardOutput; i++)
            {
                OutRefresh[i] = new Thread(GetOutBitStatus);
                OutRefresh[i].Start(i);
            }
        }

        public void GetOutBitStatus(object device)
        {
            int Device = (int)device;
            
            do
            {
                Thread.Sleep(1);

                if (m_ACS.IsConnected(Device))
                {
                    for (int i = 0; i < MaxPortNum / 2; i++)
                    {
                        for (int j = 0; j < MaxBitPerDevice; j++)
                        {
                           int result = m_ACS.GetOutputAsync(Device, i, j);

                            if(result == 1)
                            {
                                OutputBitStatus[i,j] = true;
                            }
                            else
                            {
                                OutputBitStatus[i, j] = false;
                            }
                        }
                    }
                }
            }
            while (true);
        }

        public void GetInBitStatus(object device)
        {
            int Device = (int)device;

            do
            {
                Thread.Sleep(1);

                if (m_ACS.IsConnected(Device))
                {
                    for (int i = 0; i < MaxPortNum / 2; i++)
                    {
                        for (int j = 0; j < MaxBitPerDevice; j++)
                        {
                            int result = m_ACS.GetInputAsync(Device, i, j);

                            if (result == 1)
                            {
                                InputBitStatus[i, j] = true;
                            }
                            else
                            {
                                InputBitStatus[i, j] = false;
                            }
                        }
                    }
                }
            }
            while (true);
        }

        public override bool ReadOutBit(int oNum)
        {
            lock (this)
            {
                int slot = oNum / MaxBitPerDevice;
                int index = oNum % MaxBitPerDevice;
                return OutputBitStatus[slot, index];
            }
        }

        public override bool ReadBit(int iNum)
        {
            lock(this)
            {
                int slot = iNum / MaxBitPerDevice;
                int index = iNum % MaxBitPerDevice;
                return InputBitStatus[slot, index];
            }
        }

        public override bool ReadBit(int iNum, bool invert)
        {
            lock (this)
            {
                int slot = iNum / MaxBitPerDevice;
                int index = iNum % MaxBitPerDevice;
                return invert ? !InputBitStatus[slot, index] : InputBitStatus[slot, index];
            }
        }

        public override bool WriteBit(int? oNum, bool state)
        {
            lock (this)
            {
                try
                {
                    if (oNum == null)
                    {
                        return false;
                    }

                    int device = (int)(oNum / (MaxBitPerDevice * MaxCardSlot));

                    int State = Convert.ToInt16(state);

                    int slot = (int)(oNum / MaxBitPerDevice);

                    byte output = (byte)(oNum % MaxBitPerDevice);

                    m_ACS.SetOutput(device, slot, output, State);

                    return true;

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
