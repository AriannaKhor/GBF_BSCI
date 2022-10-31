using Advantech.Motion;
using AdvanTechAPI;
using ConfigManager;
using System;

namespace MotionManager
{
    public class AdvantechMotion : BaseMotion, IAdvantechMotion
    {
        private AdvantechAPI m_AdvantechAPI;       

        public AdvantechMotion()
        {
            SystemConfig config = SystemConfig.Open(@"..\Config Section\General\System.Config");

            m_AdvantechAPI = new AdvantechAPI();

            TotalCards = m_AdvantechAPI.TotalCards;

            Rev = new double[TotalCards, 4];
            Pitch = new double[TotalCards, 4];
            for (int i = 0; i < config.MotCfgRef.Count; i++)
            {
                MotionConfig motConfig = MotionConfig.Open(config.MotCfgRef[i].Reference);

                Rev[motConfig.Axis.CardID, motConfig.Axis.AxisID] = motConfig.Axis.Revolution;
                Pitch[motConfig.Axis.CardID, motConfig.Axis.AxisID] = motConfig.Axis.Pitch;
            }
        }
      

        public override bool IsConnect(int cardNo)
        {
            if (!m_AdvantechAPI.IsMotionCardConnected[cardNo])
                ErrorMsg = $"Connect Advantech Motion Card #{cardNo} Fail\r\n";

            return m_AdvantechAPI.IsMotionCardConnected[cardNo];
        }

        public override double GetEncoderPosition(int cardNo, int axis)
        {
            double CurCmd = 0;
            Advantech.Motion.Motion.mAcm_AxGetActualPosition(m_AdvantechAPI.m_AxisHandle[cardNo, axis], ref CurCmd);
            return CurCmd;
        }


        public override bool GetMotionDoneStatus(int cardNo, int axis)
        {
            UInt16 AxState = 0;
            Advantech.Motion.Motion.mAcm_AxGetState(m_AdvantechAPI.m_AxisHandle[cardNo, axis], ref AxState);

            return AxState == (uint)GreatechApp.Core.Enums.AxisState.STA_AX_READY;
        }

        public override bool AxisInMotion(int cardNo, int axis)
        {
            UInt16 AxState = 0;
            Advantech.Motion.Motion.mAcm_AxGetState(m_AdvantechAPI.m_AxisHandle[cardNo, axis], ref AxState);

            return AxState == (uint)GreatechApp.Core.Enums.AxisState.STA_AX_CONTI_MOT;
        }

        public override bool GetNegativeLimitStatus(int cardNo, int axis)
        {
            uint IOStatus = 0;
            Advantech.Motion.Motion.mAcm_AxGetMotionIO(m_AdvantechAPI.m_AxisHandle[cardNo, axis], ref IOStatus);

            return (IOStatus & (uint)GreatechApp.Core.Enums.Ax_Motion_IO.AX_MOTION_IO_LMTN) > 0 ? true : false;
        }

        public override bool GetPositiveLimitStatus(int cardNo, int axis)
        {
            uint IOStatus = 0;
            Advantech.Motion.Motion.mAcm_AxGetMotionIO(m_AdvantechAPI.m_AxisHandle[cardNo, axis], ref IOStatus);

            return (IOStatus & (uint)GreatechApp.Core.Enums.Ax_Motion_IO.AX_MOTION_IO_LMTP) > 0 ? true : false;
        }

        public override bool GetHomeLimitStatus(int cardNo, int axis)
        {
            uint IOStatus = 0;
            Advantech.Motion.Motion.mAcm_AxGetMotionIO(m_AdvantechAPI.m_AxisHandle[cardNo, axis], ref IOStatus);

            return (IOStatus & (uint)GreatechApp.Core.Enums.Ax_Motion_IO.AX_MOTION_IO_ORG) > 0 ? true : false;
        }

        public override bool GetServoStatus(int cardNo, int axis)
        {
            uint IOStatus = 0;
            Advantech.Motion.Motion.mAcm_AxGetMotionIO(m_AdvantechAPI.m_AxisHandle[cardNo, axis], ref IOStatus);

            return (IOStatus & (uint)GreatechApp.Core.Enums.Ax_Motion_IO.AX_MOTION_IO_SVON) > 0 ? true : false;
        }

        public bool Jog(int cardNo, int axis, short direction)
        {
            Advantech.Motion.Motion.mAcm_AxSetExtDrive(m_AdvantechAPI.m_AxisHandle[cardNo, axis], 1);
            return Advantech.Motion.Motion.mAcm_AxJog(m_AdvantechAPI.m_AxisHandle[cardNo, axis], (ushort)direction) == (uint)ErrorCode.SUCCESS;
        }

        public override bool Jog(int cardNo, int axis, short direction, double speed)
        {
            return Jog(cardNo, axis, direction);
        }

        public override bool SetAxisParam(int cardNo, int axis, double drvSpeed, double acc, double dcc, double jerk, double Kdec)
        {
            Advantech.Motion.Motion.mAcm_SetF64Property(m_AdvantechAPI.m_AxisHandle[cardNo, axis], (uint)Advantech.Motion.PropertyID.PAR_AxVelHigh, drvSpeed);
            Advantech.Motion.Motion.mAcm_SetF64Property(m_AdvantechAPI.m_AxisHandle[cardNo, axis], (uint)Advantech.Motion.PropertyID.PAR_AxAcc, acc);
            Advantech.Motion.Motion.mAcm_SetF64Property(m_AdvantechAPI.m_AxisHandle[cardNo, axis], (uint)Advantech.Motion.PropertyID.PAR_AxDec, dcc);
            return true;
        }

        public override bool SetAxisJogParam(int cardNo, int axis, double drvSpeed, double acc, double dcc)
        {
            Advantech.Motion.Motion.mAcm_SetF64Property(m_AdvantechAPI.m_AxisHandle[cardNo, axis], (uint)Advantech.Motion.PropertyID.CFG_AxJogVelHigh, drvSpeed);
            Advantech.Motion.Motion.mAcm_SetF64Property(m_AdvantechAPI.m_AxisHandle[cardNo, axis], (uint)Advantech.Motion.PropertyID.CFG_AxJogAcc, acc);
            Advantech.Motion.Motion.mAcm_SetF64Property(m_AdvantechAPI.m_AxisHandle[cardNo, axis], (uint)Advantech.Motion.PropertyID.CFG_AxJogDec, dcc);
            return true;
        }

        public override bool MoveAbs(int cardNo, int axis, int pulse, short direction)
        {
            return MoveAbs(cardNo, axis, pulse);
        }

        public bool MoveAbs(int cardNo, int axis, int distance)
        {
            return Advantech.Motion.Motion.mAcm_AxMoveAbs(m_AdvantechAPI.m_AxisHandle[cardNo, axis], (double)distance) == (uint)ErrorCode.SUCCESS;
        }

        public override bool MoveRel(int cardNo, int axis, int pulse)
        {
            return Advantech.Motion.Motion.mAcm_AxMoveRel(m_AdvantechAPI.m_AxisHandle[cardNo, axis], (double)pulse) == (uint)ErrorCode.SUCCESS;
        }

        public override bool ServoOFF(int cardNo, int axis)
        {
            return Advantech.Motion.Motion.mAcm_AxSetSvOn(m_AdvantechAPI.m_AxisHandle[cardNo, axis], 0) == (uint)ErrorCode.SUCCESS ? true : false;
        }

        public override bool ServoON(int cardNo, int axis)
        {
            return Advantech.Motion.Motion.mAcm_AxSetSvOn(m_AdvantechAPI.m_AxisHandle[cardNo, axis], 1) == (uint)ErrorCode.SUCCESS ? true : false;
        }

        public override bool StopServo(int cardNo, int axis)
        {
            Advantech.Motion.Motion.mAcm_AxSetExtDrive(m_AdvantechAPI.m_AxisHandle[cardNo, axis], 0);
            return Advantech.Motion.Motion.mAcm_AxStopEmg(m_AdvantechAPI.m_AxisHandle[cardNo, axis]) == (uint)ErrorCode.SUCCESS;
        }

        public override bool GetAlarmStatus(int cardNo, int axis)
        {
            uint IOStatus = 0;
            Advantech.Motion.Motion.mAcm_AxGetMotionIO(m_AdvantechAPI.m_AxisHandle[cardNo, axis], ref IOStatus);
            
            return (IOStatus & (uint)Advantech.Motion.Ax_Motion_IO.AX_MOTION_IO_ALM) > 0 ? true : false;
        }

        public override bool ResetMotorAlarm(int cardNo, int axis, bool isResetAlarm)
        {
            return Advantech.Motion.Motion.mAcm_AxResetError(m_AdvantechAPI.m_AxisHandle[cardNo, axis]) == (uint)ErrorCode.SUCCESS ? true : false;
        }

        public override bool FindEdge(int cardNo, int axis, double speed)
        {
            Advantech.Motion.Motion.mAcm_SetF64Property(m_AdvantechAPI.m_AxisHandle[cardNo, axis], (uint)Advantech.Motion.PropertyID.PAR_AxHomeVelHigh, speed);
            return Advantech.Motion.Motion.mAcm_AxMoveHome(m_AdvantechAPI.m_AxisHandle[cardNo, axis], (uint)Advantech.Motion.HomeMode.MODE12_AbsSearchReFind,(uint)Advantech.Motion.HomeDir.NegDir) == (uint)ErrorCode.SUCCESS ? true : false;
        }

        public override bool SetZeroPosition(int cardNo, int axis)
        {
            return Advantech.Motion.Motion.mAcm_AxSetActualPosition(m_AdvantechAPI.m_AxisHandle[cardNo, axis], 0) == (uint)ErrorCode.SUCCESS ? true : false;
        }

        public override bool SetMoonsMtrZeroPosition(int cardNo, int axis)
        {
            return Advantech.Motion.Motion.mAcm_AxSetCmdPosition(m_AdvantechAPI.m_AxisHandle[cardNo, axis], 0) == (uint)ErrorCode.SUCCESS ? true : false;
        }

        public override bool IsMotorAlarmReset(int cardNo, int axis)
        {
            return GetAlarmStatus(cardNo, axis);
        }
    }
}
