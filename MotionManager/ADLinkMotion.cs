using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AdlinkApi;
using APS168_W64;
using APS_Define_W32;
using ConfigManager;
using MotionManager;

namespace MotionTest.MotionManager
{
    public class ADLinkMotion : BaseMotion, IADLinkMotion
    {
        private AdlinkAPI m_AdlinkApi;

        public ADLinkMotion()
        {
            SystemConfig config = SystemConfig.Open(@"..\Config Section\General\System.Config");

            m_AdlinkApi = new AdlinkAPI();


        }

        public enum JogMode
        {
            Continuous,
            Step,
        }

        public enum GeneralOpt
        {
            OFF,
            ON,
        }

        private enum AdlinkReturnValue
        {
            True,
        }

        private enum AdlinkReturnBit
        {
            NaN = -1,
            False = 0,
            True,
        }

        private enum MotionIOStatusBit
        {
            ServoAlarm = 0,
            FwdLmt,
            RevLmt,
            HomeLmt,
            EmergencySensor,
            InPos = 6,
            ServoOn,
            Ready,
            SoftwareFwdLmt = 11,
            SoftwareRevLmt = 12,
            EtherCATStatus = 24,           
        }

        private enum MotionStatusBit
        {
            CommandStopped,
            InMaxVelicity,
            InAccelerate,
            InDecelerate,
            MotionDirection,  // 1 = Positive Dir, 0 = Negative Dir
            MotionDone, // 0 = In Motion, 1 = Motion Done
            InHoming,
            InWaiting = 10,
            InPointBufferMoving, //When this bit on, MotionDone and StopStatus will be cleared
            InJogging = 15,
            StopStatus, // 0 = Stop Normally, 1 = Abnormal Stop
            InBlendingMoving, // For interpolation move usage only
            PreDistanceEvent, // 1 = Event Arrived, clear when axis start moving
            PostDistanceEvent, // 1 = Event Arrived, clear when axis start moving
            Backlash = 27, // 0 = In Operation, 1 = IDLE
            Gear, // 1 = In Geared
            PulserStatus, // 0 = Disable, 1 = Enable
            GantryModeStatus, // 0 = when disable, this axis's bit30 (this bit) will depends on his other slaves are in gantry mode or not, 1 = when enable, this axis is master and this axis's bit 30 (this bit) will on)
        }

        private enum ECATSYNCMODE
        {
            DCMode,
            FreeRunMode,
        }

        private enum ServoStatus
        {
            ServoOff,
            ServoOn,
        }

        private enum InitMode
        {
            AutoInit,
        }

        public int SelectedSpeed { get; set; }
        private int m_Board_ID;

        private char[] FalseMtnIOStatus;
        private char[] FalseMtnStatus;

        //Simulation use param
        private bool S_ServoOn = false;
        private bool S_ServoOff = false;
        private bool S_MoveRel = false;
        private bool S_MoveAbs = false;
        private bool S_JogP = false;
        private bool S_JogN = false;
        private bool S_Stop = false;
        private bool S_Open = false;
        private bool S_Close = false;
        private bool S_SetZero = false;
        private bool S_Reset = false;

        private double S_CurrentPos = 0;
        private double S_Speed = 0;
        private int BinaryBaseNum = 2;

        public int Board_ID
        {
            get { return m_Board_ID; }
            set { m_Board_ID = value; }
        }

        public override bool IsConnect(int cardNo)
        {
            try
            {
#if !Simulation
                // Card(Board) initia
                int board_id = 0;
                bool _init = APS168.APS_initial(ref board_id, (int)InitMode.AutoInit) == (int)AdlinkReturnValue.True ? true : false;
                //Board_ID = board_id;
                int rem = APS168.APS_set_board_param(Board_ID, (int)APS_Define.PRB_ECAT_SYNC_MODE, (int)ECATSYNCMODE.FreeRunMode); // Set EtherCAT Sync Mode
                return _init;
#else   
                return true;
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public void Close()
        {
            try
            {
#if !Simulation
                // Card(Board) initial
                bool _Closedevice = APS168.APS_close() == (int)AdlinkReturnValue.True ? true : false;
#else
                
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public override bool ServoON(int cardNo, int axis)
        {
            try
            {
#if !Simulation
                bool _EnableMotor = APS168.APS_set_servo_on(axis, (int)ServoStatus.ServoOn) == (int)AdlinkReturnValue.True ? true : false;

                return _EnableMotor;
#else
                if (isEnabled)
                {
                    S_Stop = false;
                    S_ServoOn = true;
                    S_ServoOff = false;
                }
                else
                {
                    S_Stop = false;
                    S_ServoOn = false;
                    S_ServoOff = true;
                }

                return true;
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public override bool ServoOFF(int cardNo, int axis)
        {
            bool _DisableMotor = APS168.APS_set_servo_on(axis, (int)ServoStatus.ServoOff) == (int)AdlinkReturnValue.True ? true : false;

            return _DisableMotor;
        }

        public override bool StopServo(int cardNo, int axis)
        {
            try
            {
#if !Simulation
                bool _StopMotion = APS168.APS_stop_move(axis) == (int)AdlinkReturnValue.True ? true : false;

                return _StopMotion;
#else
                S_Stop = true;
                S_JogP = false;
                S_JogN = false;

                return true;
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }      
        }

        public override bool Jog(int cardNo, int axis, short direction, double speed)
        {
            try
            {
#if !Simulation
                APS168.APS_set_axis_param(axis, (int)APS_Define.PRA_JG_MODE, (int)JogMode.Continuous); //Set Jog mode   
                //APS168.APS_set_axis_param(axis, (int)APS_Define.PRA_JG_VM, SelectedSpeed); // Motion Speed
                APS168.APS_set_axis_param(axis, (int)APS_Define.PRA_JG_DIR, Convert.ToInt32(direction)); //Set Jog direction
            
                //Jog need to be Off first before On
                APS168.APS_jog_start(axis, (int)GeneralOpt.OFF);  //Jog Off
                bool _Jog = APS168.APS_jog_start(axis, (int)GeneralOpt.ON) == (int)AdlinkReturnValue.True ? true : false; //Start jog
            
            return _Jog;
#else
                bool S_Direction = (direction == 0) ? true : false;
                if (S_Direction)
                {
                    S_Stop = false;
                    S_JogP = true;
                    S_JogN = false;
                }
                else
                {
                    S_Stop = false;
                    S_JogP = false;
                    S_JogN = true;
                }
                return true;
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public override bool MoveAbs(int cardNo, int axis, int pulse, short direction)
        {
            try
            {
#if !Simulation
                bool _MoveAbs = APS168.APS_absolute_move(axis, Convert.ToInt32(pulse), SelectedSpeed) == (int)AdlinkReturnValue.True ? true : false;
                
                return _MoveAbs;
#else
                return true;
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
        public override bool MoveRel(int cardNo, int axis, int pulse)
        {
            try
            {
#if !Simulation
                bool _MoveRel = APS168.APS_relative_move(axis, Convert.ToInt32(pulse), SelectedSpeed) == (int)AdlinkReturnValue.True ? true : false;
                return _MoveRel;
#else
                return true;
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public override bool ResetMotorAlarm(int cardNo, int axis, bool isResetAlarm)
        {
            try
            {
#if !Simulation
                bool _resetMotorAlarm = false;
                _resetMotorAlarm = APS168.APS_reset_field_bus_alarm(axis) == (int)AdlinkReturnValue.True ? true : false;
                //int res = APS168.APS_reset_field_bus_alarm(axis);
                //bool _resetMotorAlarm = false;
                //if (res == 0)
                //{
                //    _resetMotorAlarm = true;
                //}
                return _resetMotorAlarm;
#else
                S_Stop = false;
                return true;
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            
        }
        public override bool SetZeroPosition(int cardNo, int axis)
        {
            try
            {
#if !Simulation
                double ZeroPosf = 0;
                bool _SetZeroPos = APS168.APS_set_position_f(axis, ZeroPosf) == (int)AdlinkReturnValue.True ? true : false;
                return _SetZeroPos;
#else
                S_Stop = false;
                return true;
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public override bool GetMotionDoneStatus(int cardNo, int axis)
        {
            try
            {
#if !Simulation
                char[] _signal = Signal(Convert.ToByte(axis));
                bool _AxisInPos = (_signal[(int)MotionIOStatusBit.InPos]).ToString() == ((int)AdlinkReturnBit.True).ToString() ? true : false;

                return _AxisInPos;
#else         
                if (S_Stop || !S_JogP && !S_JogN)
                {
                    return true;
                }
                else
                {
                    return false;
                }
                
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public override bool AxisInMotion(int cardNo, int axis)
        {
            try
            {
#if !Simulation
                char[] _signal = MotionSignal(Convert.ToByte(axis));
                bool _AxisInMotion = (_signal[(int)MotionStatusBit.MotionDone]).ToString() == ((int)AdlinkReturnBit.False).ToString() ? true : false;

                return _AxisInMotion;
#else
                if (S_JogP || S_JogN)
                {
                    return true;
                }
                else
                {
                    return false;
                }               
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public override bool GetServoStatus(int cardNo, int axis)
        {
            try
            {
#if !Simulation
                char[] _signal = Signal(Convert.ToByte(axis));
                bool _ServoOn = (_signal[(int)MotionIOStatusBit.ServoOn]).ToString() == ((int)AdlinkReturnBit.True).ToString() ? true : false;

                return _ServoOn;
#else
                if (S_ServoOn)
                {
                    return true;
                }
                else
                {
                    return false;
                }
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public override bool GetAlarmStatus(int cardNo, int axis)
        {
            try
            {
#if !Simulation
                char[] _signal = Signal(Convert.ToByte(axis));
                bool _ServoAlarm = (_signal[(int)MotionIOStatusBit.ServoAlarm]).ToString() == ((int)AdlinkReturnBit.False).ToString() ? true : false;

                return _ServoAlarm;
#else
                return false;
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public override double GetEncoderPosition(int cardNo, int axis)
        {
            try
            {
#if !Simulation
                double Posf = 0;
                APS168.APS_get_position_f(axis, ref Posf);
                return Posf;
#else  
                if (!S_Stop)
                {
                    if (S_JogP)
                    {
                        S_CurrentPos += S_Speed;
                    }
                    else if (S_JogN)
                    {
                        S_CurrentPos -= S_Speed;
                    }
                }

                return S_CurrentPos.ToString();
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return 0;
            }
        }

        public override bool GetPositiveLimitStatus(int cardNo, int axis)
        {
            try
            {
#if !Simulation
                char[] _signal = Signal(Convert.ToByte(axis));
                bool _ChkFwdLmt = (_signal[(int)MotionIOStatusBit.FwdLmt]).ToString() == ((int)AdlinkReturnBit.True).ToString() ? true : false;

                return _ChkFwdLmt;
#else
                return false;
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public override bool GetNegativeLimitStatus(int cardNo, int axis)
        {
            try
            {
#if !Simulation
                char[] _signal = Signal(Convert.ToByte(axis));
                bool _ChkrevLmt = (_signal[(int)MotionIOStatusBit.RevLmt]).ToString() == ((int)AdlinkReturnBit.True).ToString() ? true : false;

                return _ChkrevLmt;
#else
                return false;
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public override bool GetHomeLimitStatus(int cardNo, int axis)
        {
            try
            {
#if !Simulation
                char[] _signal = Signal(Convert.ToByte(axis));
                bool _ChkHomeLmt = (_signal[(int)MotionIOStatusBit.HomeLmt]).ToString() == ((int)AdlinkReturnBit.True).ToString() ? true : false;

                return _ChkHomeLmt;
#else
                return false;
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public override bool SetAxisParam(int cardNo, int axis, double drvSpeed, double acc, double dcc, double jerk, double Kdec)
        {
            try
            {
#if !Simulation
                bool Jdcc = APS168.APS_set_axis_param(axis, (int)APS_Define.PRA_JG_DEC, Convert.ToInt32(dcc)) == (int)AdlinkReturnValue.True ? true : false; // Jog Deccelerate
                bool Jacc = APS168.APS_set_axis_param(axis, (int)APS_Define.PRA_JG_ACC, Convert.ToInt32(acc)) == (int)AdlinkReturnValue.True ? true : false; // Jog Accelerate

                bool Mdcc = APS168.APS_set_axis_param(axis, (int)APS_Define.PRA_DEC, Convert.ToInt32(dcc)) == (int)AdlinkReturnValue.True ? true : false; // Deccelerate
                bool Macc = APS168.APS_set_axis_param(axis, (int)APS_Define.PRA_ACC, Convert.ToInt32(acc)) == (int)AdlinkReturnValue.True ? true : false; // Accelerate

                bool JSpeed = APS168.APS_set_axis_param(axis, (int)APS_Define.PRA_JG_VM, Convert.ToInt32(drvSpeed)) == (int)AdlinkReturnValue.True ? true : false; // Motion Speed
                bool MSpeed = APS168.APS_set_axis_param(axis, (int)APS_Define.PRA_VM, Convert.ToInt32(drvSpeed)) == (int)AdlinkReturnValue.True ? true : false; // Motion Speed

                SelectedSpeed = Convert.ToInt32(drvSpeed);

                if (Jdcc && Jacc && Mdcc && Macc && JSpeed && MSpeed)
                {
                    return true;
                }
                else
                {
                    return false;
                }
#else
                S_Speed = drvSpeed;
                return true;
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public override bool SetAxisJogParam(int cardNo, int axis, double drvSpeed, double acc, double dcc)
        {
            try
            {
#if !Simulation
                bool Jdcc = APS168.APS_set_axis_param(axis, (int)APS_Define.PRA_JG_DEC, Convert.ToInt32(dcc)) == (int)AdlinkReturnValue.True ? true : false; // Jog Deccelerate
                bool Jacc = APS168.APS_set_axis_param(axis, (int)APS_Define.PRA_JG_ACC, Convert.ToInt32(acc)) == (int)AdlinkReturnValue.True ? true : false; // Jog Accelerate

                bool Mdcc = APS168.APS_set_axis_param(axis, (int)APS_Define.PRA_DEC, Convert.ToInt32(dcc)) == (int)AdlinkReturnValue.True ? true : false; // Deccelerate
                bool Macc = APS168.APS_set_axis_param(axis, (int)APS_Define.PRA_ACC, Convert.ToInt32(acc)) == (int)AdlinkReturnValue.True ? true : false; // Accelerate

                bool JSpeed = APS168.APS_set_axis_param(axis, (int)APS_Define.PRA_JG_VM, Convert.ToInt32(drvSpeed)) == (int)AdlinkReturnValue.True ? true : false; // Motion Speed
                bool MSpeed = APS168.APS_set_axis_param(axis, (int)APS_Define.PRA_VM, Convert.ToInt32(drvSpeed)) == (int)AdlinkReturnValue.True ? true : false; // Motion Speed

                SelectedSpeed = Convert.ToInt32(drvSpeed);

                if (Jdcc && Jacc && Mdcc && Macc && JSpeed && MSpeed)
                {
                    return true;
                }
                else
                {
                    return false;
                }
#else
                S_Speed = drvSpeed;
                return true;
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        private char[] Signal(byte axis) 
        {
            try
            {
#if !Simulation
                int AxisSignals = APS168.APS_motion_io_status(axis); //return the positive decimal value, while code success execute; return negative value, if error 

                string BinarySignal = Convert.ToString(AxisSignals, 2); //Convert to binary

                //Convert to array
                char[] iostatus = new char[BinarySignal.Length];
                char[] falseMtnIoStatus = new char[BinarySignal.Length];
                for (int i = 0; i < BinarySignal.Length; i++)
                {
                    iostatus[i] = BinarySignal[BinarySignal.Length-i-1]; //The binary is in reverse sequence with the bit in encoder, hence need to reverse the signal to get correct sequence with encoder
                    falseMtnIoStatus[i] = '-';
                }

                FalseMtnIOStatus = falseMtnIoStatus;

                #region Bit Desc
                //[0] ALM Servo alarm
                //[1] PEL Plus end limit
                //[2] MEL Minus end limit
                //[3] ORG Original position sensor(home sensor)
                //[4] EMG EMG sensor
                //[5] EZ EZ passed
                //[6] INP In position (0: Not In Pos; 1: In Pos)
                //[7] SVON Servo ON (0: Servo OFF; 1: Servo ON)
                //[8] RDY Ready 
                //[9] Reserved Reserved, always be 0
                //[10] SCL Software circular limit
                //[11] SPEL Software plus end limit
                //[12] SMEL Software minus end limit
                //[13~23] Reserved Reserved, always be 0
                //[24] OP EtherCAT online status (0: EtherCAT slave is offline; 1: EtherCAT slave is online)
                #endregion
            
                return iostatus;
#else
                int arraysize = 1;
                char[] S_ioStatus = new char[arraysize];
                for (int i = 0; i < arraysize; i++)
                {
                    S_ioStatus[i] = '1';
                }
                return S_ioStatus;
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return FalseMtnIOStatus;
            }
        }

        private char[] MotionSignal(byte axis)
        {
            try
            {
#if !Simulation
                int AxisSignals = APS168.APS_motion_status(axis);

                string BinarySignal = Convert.ToString(AxisSignals, BinaryBaseNum); //Convert to binary

                //Convert to array
                char[] status = new char[BinarySignal.Length];
                char[] falseMtnstatus = new char[BinarySignal.Length];
                for (int i = 0; i < BinarySignal.Length; i++)
                {
                    status[i] = BinarySignal[BinarySignal.Length-i-1];
                    falseMtnstatus[i] = '-';
                }

                FalseMtnStatus = falseMtnstatus;

                #region Bit Desc
                //[0] CSTP Commad stooped
                //[1] VM (In maximum velocity) 
                //[2] ACC (In Accelerate)
                //[3] DEC (In Decelerate)
                //[4] DIR (0: Neg direction, 1 : Pos direction)
                //[5] MDN (Motion done (0: in motion, 1: done motion))
                //[6] HMV (In homing)
                //[7-9] Reserved Reserved, always be 0
                //[10] WAIT (Axis is waiting stats)
                //[11] PTB (In point buffer moving)
                //[12-14] Reserved Reserved, always be 0
                //[15] JOG (In jogging)
                //[16] ASTP (0: Stop normally, 1: abnormal stop)
                //[17] BLD (Axis in blending moving)
                //[18] PRED (Pre-distance event 1 good is better for you)
                //[19] POSTD (Axis in blending movingPre-distance event 1 good is better for you
                //[20-26] Reserved
                //[27] BACKLASH (0: In Operation, 1: IDLE)
                //[28] GER (1: Ib geared condition)
                //[29] PSR (0: Disable, 1:Enable)
                //[30] GRY (1: When gantry mode is enabled, this axis is master and his motion status bit 30(GRY) will be turned on. 0: When gantry mode is disable, turning this axis's motion status bit 30(GRY)off will depends on his other slaves are in gantry mode or not)
                #endregion

                return status;
#else
                int arraysize = 1;
                char[] S_Status = new char[arraysize];
                for (int i = 0; i < arraysize; i++)
                {
                    S_Status[i] = '1';
                }
                return S_Status;
 #endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return FalseMtnStatus;
            }
        }
    }
}
