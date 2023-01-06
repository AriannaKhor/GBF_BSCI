using Advantech.Motion;
using ConfigManager;
using GreatechApp.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AdvanTechAPI
{
    public class AdvantechAPI
    {
        public int TotalCards { get; set; }
        public bool[] IsMotionCardConnected;
        public bool[] IsIOCardConnected;

        public int MaxAxisPerCard { get; set; }
        public int MaxBitPerCard { get; set; }
        public int MaxBitPerDevice { get; set; }

        public uint[] m_DeviceID;
        public DEV_LIST[] CurAvailableDevs = new DEV_LIST[Advantech.Motion.Motion.MAX_DEVICES];
        public IntPtr[] m_DeviceHandle;
        public uint[] m_AxisCountPerDevice;
        public IntPtr[,] m_AxisHandle;

        public uint[] DIChanCount, DOChanCount, DIPortCount, DOPortCount;
        public char[,] InputBitStatus, OutputBitStatus;  // Need to put additional array here for index.

        object[] m_Lock;
        Thread[] IOEventThread;

        /* 
          ==============================================================================================
                     Device (Board) Level                              Axis (Motor) Level              
             Get Device ID  =>  Get Device Handle  =>  Get Axis Quantity per Device  =>  Get Axis Handle 
           ============================================================================================== 
        */
        public AdvantechAPI()
        {
            SystemConfig systemConfig = SystemConfig.Open(@"..\Config Section\General\System.Config");

            TotalCards = systemConfig.Motion.NumOfController; /// Need to change using config
            IsMotionCardConnected = Enumerable.Repeat(false, TotalCards).ToArray();
            IsIOCardConnected = Enumerable.Repeat(false, TotalCards).ToArray();

            MaxAxisPerCard = 64; /// Need to change using config
            MaxBitPerCard = 8; /// Need to change using config
            MaxBitPerDevice = 1000;

            m_DeviceID = new uint[TotalCards];
            m_DeviceHandle = Enumerable.Repeat(IntPtr.Zero, TotalCards).ToArray(); //new IntPtr[TotalCards];

            m_AxisCountPerDevice = Enumerable.Repeat((uint)0, TotalCards).ToArray();  //new uint[TotalCards];
            m_AxisHandle = new IntPtr[TotalCards, 64];

            DIChanCount = new uint[TotalCards];
            DOChanCount = new uint[TotalCards];
            DIPortCount = new uint[TotalCards];
            DOPortCount = new uint[TotalCards];

            InputBitStatus = new char[TotalCards, MaxBitPerDevice];
            OutputBitStatus = new char[TotalCards, MaxBitPerDevice];

            IOEventThread = new Thread[TotalCards];
            m_Lock = Enumerable.Repeat(new object(), TotalCards).ToArray();

            for (int i = 0; i < TotalCards; i++)
            {
                // Get Device ID from each card
                uint deviceCount = 0;
                Advantech.Motion.Motion.mAcm_GetAvailableDevs(CurAvailableDevs, Advantech.Motion.Motion.MAX_DEVICES, ref deviceCount);
                m_DeviceID[i] = CurAvailableDevs[i].DeviceNum;

                if (!OpenBoard(m_DeviceID[i], i))
                {
                    Console.WriteLine($"Advantech Board {i} - Failed to Open Board");
                    continue;
                }

                if (!OpenAxis(i))
                {
                    Console.WriteLine($"Advantech Board {i} - Failed to Open Axis");
                    continue;
                }

                if (!LoadMotionConfig(i, systemConfig.MotionCards[i].ConfigFile))
                {
                    Console.WriteLine($"Advantech Board {i} - Failed to Load Motion Config");
                    continue;
                }

                IsMotionCardConnected[i] = true;

                if (!LoadIOConfig(i, systemConfig.IOAdvantechDevices[i].IOFile))
                {
                    Console.WriteLine($"Advantech Board {i} - Failed to Load IO Config");
                    continue;
                }

                if (!GetListOfDOAndDI(i))
                {
                    Console.WriteLine($"Advantech Board {i} - Failed to Get IO List");
                    continue;
                }

                IsIOCardConnected[i] = true;
               
            }
        }

        private bool LoadMotionConfig(int deviceIndex, string motionConfigFile)
        {
            return Advantech.Motion.Motion.mAcm_DevLoadConfig(m_DeviceHandle[deviceIndex], motionConfigFile) == (uint)ErrorCode.SUCCESS ? true : false;
        }

        private bool LoadIOConfig(int deviceIndex, string ioConfigFile)
        {
            return Advantech.Motion.Motion.mAcm_DevLoadMapFile(m_DeviceHandle[deviceIndex], ioConfigFile) == (uint)ErrorCode.SUCCESS ? true : false;
        }

        // Open the device and get its device handle
        private bool OpenBoard(uint DeviceID, int DeviceIndex)
        {
            uint Result;
            uint retry = 0;
            bool rescan;

            if (m_DeviceHandle[DeviceIndex] == IntPtr.Zero)
            {
                do
                {
                    Result = Advantech.Motion.Motion.mAcm_DevOpen(DeviceID, ref m_DeviceHandle[DeviceIndex]);
                    if (Result != (uint)ErrorCode.SUCCESS)
                    {
                        //Open board failed error
                        retry++;
                        rescan = true;
                        if (retry > 10)
                        {
                            return false;
                        }
                        Thread.Sleep(500);
                    }
                    else
                    {
                        rescan = false;
                    }
                }

                while (rescan == true);
            }

            return true;
        }

        // Open axis and get axis handle
        private bool OpenAxis(int DeviceIndex)
        {
            uint Result;
            uint NumberOfAxisPerDevice = new uint();

            // Get number of axis per device
            Result = Advantech.Motion.Motion.mAcm_GetU32Property(m_DeviceHandle[DeviceIndex], (uint)PropertyID.FT_DevAxesCount, ref NumberOfAxisPerDevice);
            if (Result != (int)ErrorCode.SUCCESS)
            {

            }

            m_AxisCountPerDevice[DeviceIndex] = NumberOfAxisPerDevice;

            // Open the axis
            for (int i = 0; i < m_AxisCountPerDevice[DeviceIndex]; i++)
            {
                Result = Advantech.Motion.Motion.mAcm_AxOpen(m_DeviceHandle[DeviceIndex], (UInt16)i, ref m_AxisHandle[DeviceIndex, i]);
            }

            return Result == (uint)ErrorCode.SUCCESS ? true : false;
        }

        // Stop all axis and board
        public void CloseBoard(int DeviceIndex)
        {
            uint AxisNum;

            for (AxisNum = 0; AxisNum < m_AxisCountPerDevice[DeviceIndex]; AxisNum++)
            {
                Advantech.Motion.Motion.mAcm_AxResetError(m_AxisHandle[DeviceIndex, AxisNum]);

                // To command axis to decelerate to stop.
                Advantech.Motion.Motion.mAcm_AxStopDec(m_AxisHandle[DeviceIndex, AxisNum]);
            }

            for (AxisNum = 0; AxisNum < m_AxisCountPerDevice[DeviceIndex]; AxisNum++)
            {
                //Close Axes
                Advantech.Motion.Motion.mAcm_AxClose(ref m_AxisHandle[DeviceIndex, AxisNum]);
            }

            m_AxisCountPerDevice[DeviceIndex] = 0;

            //Close Device
            Advantech.Motion.Motion.mAcm_DevClose(ref m_DeviceHandle[DeviceIndex]);

            m_DeviceHandle[DeviceIndex] = IntPtr.Zero;
        }


        // Get the number of port in each device (each port should have 8-bit / equivalent to MaxBitPerCard)
        private bool GetListOfDOAndDI(int DeviceIndex)
        {
            uint Result;

            Result = Advantech.Motion.Motion.mAcm_GetU32Property(m_DeviceHandle[DeviceIndex], (uint)PropertyID.FT_DaqDiMaxChan, ref DIChanCount[DeviceIndex]);
            if (Result == (uint)ErrorCode.SUCCESS)
            {
                DIPortCount[DeviceIndex] = DIChanCount[DeviceIndex] / (uint)MaxBitPerCard;
            }

            Result = Advantech.Motion.Motion.mAcm_GetU32Property(m_DeviceHandle[DeviceIndex], (uint)PropertyID.FT_DaqDoMaxChan, ref DOChanCount[DeviceIndex]);
            if (Result == (uint)ErrorCode.SUCCESS)
            {
                DOPortCount[DeviceIndex] = DOChanCount[DeviceIndex] / (uint)MaxBitPerCard;
            }

            return Result == (uint)ErrorCode.SUCCESS ? true : false;
        }

        private void GetInputStatus(int DeviceIndex)
        {
            string InputValue = string.Empty;

            if (m_DeviceHandle[DeviceIndex] != IntPtr.Zero)
            {
                for (ushort i = 0; i < DIPortCount[DeviceIndex]; i++)
                {
                    byte DiStatus = 0;
                    uint Result = Advantech.Motion.Motion.mAcm_DaqDiGetByte(m_DeviceHandle[DeviceIndex], i, ref DiStatus);
                    if (Result == (uint)ErrorCode.SUCCESS)
                    {
                        InputValue += DiStatus.ToString("x") + ",";
                    }
                }

                string strByte = string.Empty;
                string[] _strParts = InputValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (_strParts.Length > 1)
                {
                    foreach (string Input in _strParts)
                    {
                        int convertedInt = Convert.ToInt32(int.Parse(Input, System.Globalization.NumberStyles.HexNumber));
                        strByte += Convert.ToString(convertedInt, 2).PadLeft((int)MaxBitPerCard, '0');
                    }
                }

                string reverseBinary = string.Empty;
                char[] tempChar = strByte.ToCharArray();

                for (int j = 0; j <= tempChar.Length - 1; j += MaxBitPerCard)
                {
                    string temp8 = string.Empty;
                    for (int i = j + (MaxBitPerCard - 1); i >= j; i--)
                    {
                        temp8 += tempChar[i];
                    }
                    reverseBinary += temp8;
                }

                int bit = 0;
                foreach (char bitValue in reverseBinary.ToCharArray())
                {
                    InputBitStatus[DeviceIndex, bit] = bitValue;
                    bit++;
                }
            }
        }

        private void GetOutputStatus(int DeviceIndex)
        {
            string OutputValue = string.Empty;

            if (m_DeviceHandle[DeviceIndex] != IntPtr.Zero)
            {
                for (ushort i = 0; i < DOPortCount[DeviceIndex]; i++)
                {
                    byte DoStatus = 0;
                    uint Result = Advantech.Motion.Motion.mAcm_DaqDoGetByte(m_DeviceHandle[DeviceIndex], i, ref DoStatus);
                    if (Result == (uint)ErrorCode.SUCCESS)
                    {
                        OutputValue += DoStatus.ToString("x") + ",";
                    }
                }

                string strByte = string.Empty;
                string[] _strParts = OutputValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (_strParts.Length > 1)
                {
                    foreach (string Input in _strParts)
                    {
                        int convertedInt = Convert.ToInt32(int.Parse(Input, System.Globalization.NumberStyles.HexNumber));
                        strByte += Convert.ToString(convertedInt, 2).PadLeft(MaxBitPerCard, '0');
                    }
                }

                string reverseBinary = string.Empty;
                char[] tempChar = strByte.ToCharArray();

                for (int j = 0; j <= tempChar.Length - 1; j += MaxBitPerCard)
                {
                    string temp8 = string.Empty;
                    for (int i = j + (MaxBitPerCard - 1); i >= j; i--)
                    {
                        temp8 += tempChar[i];
                    }
                    reverseBinary += temp8;
                }

                int bit = 0;
                foreach (char bitValue in reverseBinary.ToCharArray())
                {
                    OutputBitStatus[DeviceIndex, bit] = bitValue;
                    bit++;
                }
            }
        }

        public bool TriggerOutput(int DeviceIndex, int bitIndex, byte value)
        {
            return Advantech.Motion.Motion.mAcm_DaqDoSetBit(m_DeviceHandle[DeviceIndex], (ushort)bitIndex, value) == (uint)ErrorCode.SUCCESS;
        }

        public void StartScanIO()
        {
            for (int i = 0; i < TotalCards; i++)
            {
                IOEventThread[i] = new Thread(() => IOMonitor(i));
                IOEventThread[i].Start();
                Thread.Sleep(200);
            }
        }

        private void IOMonitor(int DeviceIndex)
        {
            while (true)
            {
                lock (m_Lock[DeviceIndex])
                {
                    GetInputStatus(DeviceIndex);
                    GetOutputStatus(DeviceIndex);
                }
            }
        }
    }
}
