using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Advantech.Motion;
using MotionManager;

namespace AdvanTech.PCI1203
{
    //public class PCIMotion : MotionBase
    //{
    //    #region Variable

    //    uint DeviceNum = 0;
    //    IntPtr m_DeviceHandle = IntPtr.Zero;
    //    IntPtr[] m_Axishand = new IntPtr[64];
    //    public uint m_ulAxisCount = 0;

    //    string ConfigFile;

    //    Thread CheckConnectionThread;

    //    public struct MotionIO
    //    {
    //        public bool Alarm;
    //        public bool PositifLimit;
    //        public bool NegativeLimit;
    //        public bool ServoOn;
    //        public bool Ready;
    //        public bool InPos;
    //    }


    //    #endregion

    //    #region Constructor

    //    public PCIMotion()
    //    {

    //    }

    //    public override bool Initialization(string _ConfigFile, int _BoardID)
    //    {
    //        bool result = false;

    //        DeviceNum = GetAvailableDevs(_BoardID);
    //        ConfigFile = _ConfigFile;
    //        m_DeviceHandle = OpenBoard(DeviceNum, _BoardID);

    //        if (m_DeviceHandle != IntPtr.Zero &&  OpenAxis() && LoadConfig())
    //        {
    //            CheckConnectionThread = new Thread(new ThreadStart(ConnectionThread));
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


    //    private bool OpenAxis()
    //    {
    //        uint Result;
    //        uint AxesPerDev = new uint();

    //        Result = Motion.mAcm_GetU32Property(m_DeviceHandle, (uint)PropertyID.FT_DevAxesCount, ref AxesPerDev);
    //        if (Result != (int)ErrorCode.SUCCESS)
    //        {

    //        }

    //        m_ulAxisCount = AxesPerDev;

    //        for (int i = 0; i < m_ulAxisCount; i++)
    //        {
    //            Result = Motion.mAcm_AxOpen(m_DeviceHandle, (UInt16)i, ref m_Axishand[i]);
    //        }


    //        return Result == (uint)ErrorCode.SUCCESS ? true : false;
    //    }

    //    public void CloseBoard()
    //    {
    //        uint AxisNum;
    //        //Stop Every Axes

    //        if (CheckConnectionThread != null)
    //            CheckConnectionThread.Abort();

    //        for (AxisNum = 0; AxisNum < m_ulAxisCount; AxisNum++)
    //        {
    //            Motion.mAcm_AxResetError(m_Axishand[AxisNum]);

    //            // To command axis to decelerate to stop.
    //            Motion.mAcm_AxStopDec(m_Axishand[AxisNum]);
    //        }

    //        for (AxisNum = 0; AxisNum < m_ulAxisCount; AxisNum++)
    //        {
    //            //Close Axes
    //            Motion.mAcm_AxClose(ref m_Axishand[AxisNum]);
    //        }

    //        m_ulAxisCount = 0;

    //        CheckConnectionThread = null;

    //        //Close Device
    //        Motion.mAcm_DevClose(ref m_DeviceHandle);

    //        m_DeviceHandle = IntPtr.Zero;

    //    }

    //    private bool LoadConfig()
    //    {
    //        return Motion.mAcm_DevLoadConfig(m_DeviceHandle, ConfigFile) == (uint)ErrorCode.SUCCESS ? true : false;
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
    //        DevEnableEvt |= (uint)EventType.EVT_DEV_DISCONNET;

    //        Motion.mAcm_DevEnableEvent(m_DeviceHandle, DevEnableEvt);

    //        while (true)
    //        {
    //            UInt32 DevEvtStatus = 0;
    //            result = Motion.mAcm_DevCheckEvent(m_DeviceHandle, ref DevEvtStatus, int.MaxValue);
    //            if (result == (uint)ErrorCode.SUCCESS)
    //            {
    //                if ((DevEvtStatus & (uint)EventType.EVT_DEV_DISCONNET) > 0)
    //                {
    //                    //  OnErrorEvent(this, new OnErrorEventArgs(ErrorType.MotorCardDisc, "Motor Card Disconnected"));
    //                }
    //            }
    //        }
    //    }

    //    #endregion

    //    #region Motion
    //    public override int AxisCount()
    //    {
    //        return (int) m_ulAxisCount;
    //    }

    //    public override bool ServoON(int Axis, int State)
    //    {
    //        return Motion.mAcm_AxSetSvOn(m_Axishand[Axis], (uint)State) == (uint)ErrorCode.SUCCESS ? true : false;
    //    }

    //    public override bool StopAxis(int Axis)
    //    {
    //        Motion.mAcm_AxSetExtDrive(m_Axishand[Axis], 0);
    //        return Motion.mAcm_AxStopEmg(m_Axishand[Axis]) == (uint)ErrorCode.SUCCESS;
    //    }

    //    public override bool ResetError(int Axis)
    //    {
    //        return Motion.mAcm_AxResetError(m_Axishand[Axis]) == (uint)ErrorCode.SUCCESS ? true : false;
    //    }

    //    public override bool SetHomingPos(int Axis)
    //    {
    //        return Motion.mAcm_AxSetActualPosition(m_Axishand[Axis], 0) == (uint)ErrorCode.SUCCESS ? true : false;
    //    }

    //    public override bool MoveAbs(int Axis, double distance)
    //    {
    //        return Motion.mAcm_AxMoveAbs(m_Axishand[Axis], distance) == (uint)ErrorCode.SUCCESS;
    //    }

    //    public override bool MoveRel(int Axis, double distance)
    //    {
    //        return Motion.mAcm_AxMoveRel(m_Axishand[Axis], distance) == (uint)ErrorCode.SUCCESS;
    //    }

    //    public override bool Jog(int Axis, int direction)
    //    {
    //        Motion.mAcm_AxSetExtDrive(m_Axishand[Axis], 1);
    //        return Motion.mAcm_AxJog(m_Axishand[Axis], (ushort)direction) == (uint)ErrorCode.SUCCESS;
    //    }

    //    public override bool GetMotionCompleteStatus(int Axis)
    //    {
    //        UInt16 AxState = 0;
    //        Motion.mAcm_AxGetState(m_Axishand[Axis], ref AxState);
    //        return AxState == (uint)AxisState.STA_AX_READY;
    //    }

    //    public override  UInt16 GetAxisStatus(int Axis)
    //    {
    //        UInt16 AxState = 0;
    //        Motion.mAcm_AxGetState(m_Axishand[Axis], ref AxState);
    //        return AxState;
    //    }

    //    public override double GetEncoderPosition(int Axis)
    //    {
    //        double CurCmd = 0;
    //        Motion.mAcm_AxGetActualPosition(m_Axishand[Axis], ref CurCmd);
    //        return CurCmd;
    //    }

    //    public override uint GetMotionIOStatus(int Axis)
    //    {
    //        uint IOStatus = 0;

    //        Motion.mAcm_AxGetMotionIO(m_Axishand[Axis], ref IOStatus);

    //        return IOStatus;
    //    }

    //    public override bool AxisHoming(int Axis, UInt32 HomeMode, uint Direction)
    //    {
    //        return Motion.mAcm_AxMoveHome(m_Axishand[Axis], HomeMode, Direction) == (uint)ErrorCode.SUCCESS;
    //    }

    //    public override bool AxisSetting(int Axis, uint SettingID, double value)
    //    {
    //        return Motion.mAcm_SetF64Property(m_Axishand[Axis], SettingID, value) == (uint)ErrorCode.SUCCESS;
    //    }

    //    #endregion

    //}
}
