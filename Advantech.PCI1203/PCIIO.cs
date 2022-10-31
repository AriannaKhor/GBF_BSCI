using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Advantech.Motion;
using IOManager;


namespace AdvanTech.PCI1203
{
    //public class PCIIO :IOBase
    //{
    //    #region Variable

    //    uint DeviceNum = 0;
    //    int _BoardID;
    //    IntPtr m_DeviceHandle = IntPtr.Zero;

    //    string IOFile;
    //    int MaxBitPerCard;

    //    public uint DOChanNum;
    //    public uint DIChanNum;

    //    public uint DIPortNumber, DOPortNumber;

    //    Thread IOEventThread;
    //    public Thread CheckConnectionThread;

    //    char[] InputStatus, OutputStatus;

    //    object m_Lock = new object();
    //    object m_StatusLock = new object();

    //    #endregion

    //    #region Constructor

    //    public PCIIO()
    //    {
           
    //    }

    //    public override bool Initialization(string _IOFile, int _MaxBitPerCard, int _BoardID)
    //    {
    //        bool result = false;

    //        MaxBitPerCard = _MaxBitPerCard;
    //        IOFile = _IOFile;
    //        DeviceNum = GetAvailableDevs(_BoardID);
    //        m_DeviceHandle = OpenBoard(DeviceNum, _BoardID);

    //        if (m_DeviceHandle != IntPtr.Zero && LoadIOMap() && GetListOfDOAndDI())
    //        {

    //            IOEventThread = new Thread(new ThreadStart(IOMonitor));
    //            CheckConnectionThread = new Thread(new ThreadStart(ConnectionThread));
    //            IOEventThread.Start();
    //            CheckConnectionThread.Start();
    //            result = true;
    //        }

    //        return result;
    //    }


    //    public uint GetAvailableDevs(int BoardID)
    //    {
    //        uint deviceCount = 0;
    //        Motion.mAcm_GetAvailableDevs(PCIBase.CurAvailableDevs, Motion.MAX_DEVICES, ref deviceCount);
    //        return PCIBase.CurAvailableDevs[BoardID].DeviceNum;
    //    }

    //    public IntPtr OpenBoard(uint DeviceNum, int BoardID)
    //    {
    //        uint Result;
    //        uint retry = 0;
    //        bool rescan;

    //        if (PCIBase.m_DeviceHandle[BoardID] == IntPtr.Zero)
    //        {
    //            do
    //            {
    //                Result = Motion.mAcm_DevOpen(DeviceNum, ref PCIBase.m_DeviceHandle[BoardID]);
    //                if (Result != (uint)ErrorCode.SUCCESS)
    //                {
    //                    //Open board failed error
    //                    retry++;
    //                    rescan = true;
    //                    if (retry > 10)
    //                        break;
    //                    System.Threading.Thread.Sleep(1000);
    //                }
    //                else
    //                {
    //                    rescan = false;
    //                }
    //            }

    //            while (rescan == true);
    //        }

    //        return PCIBase.m_DeviceHandle[BoardID];
    //    }

    //    public override void Close()
    //    {

    //        if (IOEventThread != null)
    //            IOEventThread.Abort();
    //        if (CheckConnectionThread != null)
    //            CheckConnectionThread.Abort();

    //        IOEventThread = null;
    //        CheckConnectionThread = null;

    //        //Close Device
    //        Motion.mAcm_DevClose(ref m_DeviceHandle);

    //        m_DeviceHandle = IntPtr.Zero;

    //    }

    //    private bool LoadIOMap()
    //    {
    //        return Motion.mAcm_DevLoadMapFile(m_DeviceHandle, IOFile) == (uint)ErrorCode.SUCCESS ? true : false;
    //    }

    //    private bool GetListOfDOAndDI()
    //    {
    //        uint Result;

    //        Result = Motion.mAcm_GetU32Property(m_DeviceHandle, (uint)PropertyID.FT_DaqDiMaxChan, ref DIChanNum);
    //        if (Result == (uint)ErrorCode.SUCCESS)
    //        {
    //            DIPortNumber = DIChanNum / (uint)MaxBitPerCard;
    //        }

    //        Result = Motion.mAcm_GetU32Property(m_DeviceHandle, (uint)PropertyID.FT_DaqDoMaxChan, ref DOChanNum);
    //        if (Result == (uint)ErrorCode.SUCCESS)
    //        {
    //            DOPortNumber = DOChanNum / (uint)MaxBitPerCard;
    //        }

    //        return Result == (uint)ErrorCode.SUCCESS ? true : false;
    //    }

    //    public string GetErrorMessage(uint errorCode)
    //    {
    //        StringBuilder ErrorMsg = new StringBuilder("", 100);
    //        string ErrorMessage = "";

    //        if (Motion.mAcm_GetErrorMessage(errorCode, ErrorMsg, 100))
    //        {
    //            ErrorMessage = ErrorMsg.ToString();
    //        }

    //        return ErrorMessage;
    //    }

    //    #endregion

    //    #region Events

    //    private void ConnectionThread()
    //    {
    //        uint result;
    //        uint DevEnableEvt = 0;
    //        DevEnableEvt |= (uint)EventType.EVT_DEV_IO_DISCONNET;
    //        Motion.mAcm_DevEnableEvent(m_DeviceHandle, DevEnableEvt);

    //        while (true)
    //        {
    //            UInt32 DevEvtStatus = 0;
    //            result = Motion.mAcm_DevCheckEvent(m_DeviceHandle, ref DevEvtStatus, int.MaxValue);
    //            if (result == (uint)ErrorCode.SUCCESS)
    //            {
    //                if ((DevEvtStatus & (uint)EventType.EVT_DEV_IO_DISCONNET) > 0)
    //                {
    //                    //   OnErrorEvent(this, new OnErrorEventArgs(ErrorType.IOCardDisc, "IO Card Disconnected"));
    //                }
    //            }
    //        }
    //    }

    //    private void IOMonitor()
    //    {
    //        while (true)
    //        {
    //            lock (m_Lock)
    //            {
    //                AssignInputVariable(GetInputStatus());
    //                AssignOutputVariable(GetOutputStatus());
    //            }

    //            Thread.Sleep(100);
    //        }
    //    }

    //    #endregion

    //    #region IO

    //    public string GetInputStatus()
    //    {
    //        string InputValue = string.Empty;

    //        if (m_DeviceHandle != IntPtr.Zero)
    //        {
    //            for (ushort i = 0; i < DIPortNumber; i++)
    //            {
    //                byte DiStatus = 0;
    //                uint Result = Motion.mAcm_DaqDiGetByte(m_DeviceHandle, i, ref DiStatus);
    //                if (Result == (uint)ErrorCode.SUCCESS)
    //                {
    //                    InputValue += DiStatus.ToString("x") + ",";
    //                }
    //            }
    //        }

    //        return InputValue;
    //    }

    //    public string GetOutputStatus()
    //    {
    //        string OutputValue = string.Empty;

    //        if (m_DeviceHandle != IntPtr.Zero)
    //        {
    //            for (ushort i = 0; i < DOPortNumber; i++)
    //            {
    //                byte DoStatus = 0;
    //                uint Result = Motion.mAcm_DaqDoGetByte(m_DeviceHandle, i, ref DoStatus);
    //                if (Result == (uint)ErrorCode.SUCCESS)
    //                {

    //                    OutputValue += DoStatus.ToString("x") + ",";
    //                }
    //            }
    //        }

    //        return OutputValue;
    //    }

    //    private void AssignInputVariable(string value)
    //    {
    //        string strByte = string.Empty;
    //        string[] _strParts = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
    //        if (_strParts.Length > 1)
    //        {
    //            foreach (string Input in _strParts)
    //            {
    //                int convertedInt = Convert.ToInt32(int.Parse(Input, System.Globalization.NumberStyles.HexNumber));
    //                strByte += Convert.ToString(convertedInt, 2).PadLeft((int)MaxBitPerCard, '0');
    //            }
    //        }

    //        string reverseBinary = string.Empty;
    //        char[] tempChar = strByte.ToCharArray();

    //        for (int j = 0; j <= tempChar.Length - 1; j += MaxBitPerCard)
    //        {
    //            string temp8 = string.Empty;
    //            for (int i = j + (MaxBitPerCard - 1); i >= j; i--)
    //            {
    //                temp8 += tempChar[i];
    //            }
    //            reverseBinary += temp8;
    //        }

    //        InputStatus = reverseBinary.ToCharArray();

    //    }

    //    private void AssignOutputVariable(string value)
    //    {
    //        string strByte = string.Empty;
    //        string[] _strParts = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
    //        if (_strParts.Length > 1)
    //        {
    //            foreach (string Input in _strParts)
    //            {
    //                int convertedInt = Convert.ToInt32(int.Parse(Input, System.Globalization.NumberStyles.HexNumber));
    //                strByte += Convert.ToString(convertedInt, 2).PadLeft(MaxBitPerCard, '0');
    //            }
    //        }

    //        string reverseBinary = string.Empty;
    //        char[] tempChar = strByte.ToCharArray();

    //        for (int j = 0; j <= tempChar.Length - 1; j += MaxBitPerCard)
    //        {
    //            string temp8 = string.Empty;
    //            for (int i = j + (MaxBitPerCard - 1); i >= j; i--)
    //            {
    //                temp8 += tempChar[i];
    //            }
    //            reverseBinary += temp8;
    //        }

    //        OutputStatus = reverseBinary.ToCharArray();
    //    }

    //    public override bool TriggerOutput(int IO, bool Condition)
    //    {
    //        return Motion.mAcm_DaqDoSetBit(m_DeviceHandle, (ushort)IO, Convert.ToByte(Condition)) == (uint)ErrorCode.SUCCESS;
    //    }

    //    public override bool GetInputStatus(int index)
    //    {
    //        return false;
    //    }

    //    public override bool GetOutputStatus(int index)
    //    {
    //        return OutputStatus[index].ToString() == "1";
    //    }

    //    public override char[] GetInput()
    //    {
    //        lock (m_StatusLock)
    //            return InputStatus;
    //    }

    //    public override char[] GetOutput()
    //    {
    //        lock (m_StatusLock)
    //            return OutputStatus;
    //    }

 

    //    #endregion
    //}
}
