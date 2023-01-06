using ACS.SPiiPlusNET;
using ConfigManager;
using System;
using System.Text;

namespace ACSApi
{
    public class ACSLibApi
    {
        public Api[] m_ACS;

        public int TotalCards { get; set; }

        private StringBuilder m_ErrMsg = new StringBuilder();

        public ACSLibApi()
        {
            SystemConfig systemConfig = SystemConfig.Open(@"..\Config Section\General\System.Config");
            TotalCards = systemConfig.Motion.NumOfController;

            m_ACS = new Api[TotalCards];

            for (int i = 0; i < TotalCards; i++)
            {
                m_ACS[i] = new Api();
#if SIMULATION
                ConnectSim(i);
#else
                ConnectTCP(systemConfig.MotionCards[i].CardID, systemConfig.MotionCards[i].DeviceAddress, systemConfig.MotionCards[i].DevicePort);
#endif
            }
        }

        public bool ConnectTCP(int cardNo, string ipAdd, string port)
        {
            try
            {
                // TCP/IP Port number (default : 701)
                m_ACS[cardNo].OpenCommEthernetTCP(ipAdd, Convert.ToInt32(port.ToString().Trim()));
                return IsConnected(cardNo);
            }
            catch (Exception ex)
            {
                Err_Msg = ex.Message.ToString();
                return false;
            }
        }

        public void ConnectSim(int cardNo)
        {
            m_ACS[cardNo].OpenCommSimulator();
        }

        public void Disconnect(int cardNo)
        {
            m_ACS[cardNo].CloseComm();
        }

        public bool IsConnected(int cardNo)
        {
            return m_ACS[cardNo].IsConnected;
        }

        public void Enable(int cardNo, Axis axis)
        {
            // Enable selected axis
            m_ACS[cardNo].Enable((Axis)axis);
        }

        public void Disable(int cardNo, Axis axis)
        {
            // Disable selected axis
            m_ACS[cardNo].Disable((Axis)axis);
        }

        public void DisableAll(int cardNo)
        {
            // Disable all of axes
            m_ACS[cardNo].DisableAll();
        }

        public void Kill(int cardNo, Axis axis)
        {
            // Disable selected axis
            m_ACS[cardNo].Kill((Axis)axis);
        }

        public string Transaction(int cardNo, string outBuf)
        {
            string temp = m_ACS[cardNo].Transaction(outBuf);
            return temp;
        }

        public bool SetAxisParam(int cardNo, Axis axis, double drvSpeed, double acc, double dcc, double jerk, double Kdec)
        {
            try
            {
                Axis axisVal = (Axis)axis;
                m_ACS[cardNo].SetVelocity(axisVal, drvSpeed);
                m_ACS[cardNo].SetAcceleration(axisVal, acc);
                m_ACS[cardNo].SetDeceleration(axisVal, dcc);
                m_ACS[cardNo].SetJerk(axisVal, jerk);
                m_ACS[cardNo].SetKillDeceleration(axisVal, Kdec);
                return true;
            }
            catch (Exception ex)
            {
                Err_Msg = ex.ToString();
                return false;
            }
        }


        #region Motion
        public bool Jog(int cardNo, MotionFlags flags, Axis axis, double velocity)
        {
            try
            {
                m_ACS[cardNo].Jog(0, (Axis)axis, velocity);

                return true;
            }
            catch (Exception ex)
            {
                Err_Msg = ex.ToString();
                return false;
            }
        }

        public bool JogM(int cardNo, MotionFlags flags, short direction)
        {
            try
            {
                GlobalDirection[] m_Direction = new GlobalDirection[] { (GlobalDirection)direction, (GlobalDirection)direction };
                Axis[] m_arrAxisListTest = new Axis[] { Axis.ACSC_AXIS_0, Axis.ACSC_AXIS_1, Axis.ACSC_NONE }; //hardcode
                m_ACS[cardNo].JogM(flags, m_arrAxisListTest, m_Direction, 0);
                return true;
            }
            catch (Exception ex)
            {
                Err_Msg = ex.ToString();
                return false;
            }
        }

        public void Halt(int cardNo, Axis axis)
        {
            m_ACS[cardNo].Halt((Axis)axis);
        }

        public void HaltM(int cardNo, Axis[] axes)
        {
            m_ACS[cardNo].HaltM(axes);
        }

        public void ToPoint(int cardNo, MotionFlags flags, Axis axis, double point)
        {
            m_ACS[cardNo].ToPoint(flags, (Axis)axis, point);
        }

        public void ToPointM(int cardNo, MotionFlags flags, double point)
        {
            Axis[] m_arrAxisListTest = new Axis[] { Axis.ACSC_AXIS_0, Axis.ACSC_AXIS_1, Axis.ACSC_NONE }; //hardcode
            double[] m_Point = new double[] { point, point };
            m_ACS[cardNo].ToPointM(flags, m_arrAxisListTest, m_Point);
        }

        public void SetFPosition(int cardNo, Axis axis, double fPosition)
        {
            m_ACS[cardNo].SetFPosition((Axis)axis, fPosition);
        }


#endregion Motion

        #region Buffer Program
        public void RunBuffer(int cardNo, ProgramBuffer buffer, string label)
        {
            m_ACS[cardNo].RunBuffer(buffer, label);
        }

        public void StopBuffer(int cardNo, ProgramBuffer buffer)
        {
            m_ACS[cardNo].StopBuffer(buffer);
        }

        public void SendTransaction(int cardNo, string cmd)
        {
            if (IsConnected(cardNo))
            {
                string GetSyncStatus = m_ACS[cardNo].Transaction(cmd);
            }
        }
        
        public bool IsBufferRunning(int cardNo, int bufferNum)
        {
            return (m_ACS[cardNo].GetProgramState((ProgramBuffer)bufferNum) & ProgramStates.ACSC_PST_RUN) != 0;
        }
#endregion Buffer Program

        #region Signal / State
        public MotorStates GetMotorState(int cardNo, Axis axis)
        {
            if (IsConnected(cardNo))
            {
                MotorStates m_nMotorState;
                m_nMotorState = m_ACS[cardNo].GetMotorState((Axis)axis);
                return m_nMotorState;
            }
            else
            {
                return MotorStates.ACSC_NONE;
            }

        }

        public SafetyControlMasks GetFault(int cardNo, Axis axis)
        {
            if (IsConnected(cardNo))
            {
                SafetyControlMasks m_GetFault;
                m_GetFault = m_ACS[cardNo].GetFault((Axis)axis);
                return m_GetFault;
            }
            else
            {
                return SafetyControlMasks.ACSC_NONE;
            }
        }

        public double GetRPosition(int cardNo, Axis axis)
        {
            double m_lfFPos;
            m_lfFPos = m_ACS[cardNo].GetRPosition((Axis)axis);
            return m_lfFPos;
        }

        public double GetFPosition(int cardNo, Axis axis)
        {
            if (IsConnected(cardNo))
            {
                double m_lfFPos;
                m_lfFPos = m_ACS[cardNo].GetFPosition((Axis)axis);
                return m_lfFPos;
            }
            else
            {
                return double.NaN;
            }
        }

        public object ReadVariable(int cardNo, string variable, ProgramBuffer nBuf = ProgramBuffer.ACSC_NONE, int from1 = -1, int to1 = -1, int from2 = -1, int to2 = -1)
        {

            object temp = m_ACS[cardNo].ReadVariable(variable, nBuf, from1, to1, from2, to2);
            return temp;
        }

        public ProgramStates GetProgramState(int cardNo, ProgramBuffer buffer)
        {
            ProgramStates m_nProgramState;
            m_nProgramState = m_ACS[cardNo].GetProgramState(buffer);
            return m_nProgramState;
        }

        public object ReadVariableAsVector(int cardNo, string variable, ProgramBuffer nBuf = ProgramBuffer.ACSC_NONE, int from1 = -1, int to1 = -1, int from2 = -1, int to2 = -1)
        {
            object temp = m_ACS[cardNo].ReadVariableAsVector(variable, nBuf, from1, to1, from2, to2);
            return temp;
        }

        public int GetInputPort(int cardNo, int port)
        {
            int m_nValues = m_ACS[cardNo].GetInputPort(port);
            return m_nValues;
        }

        public int GetOutputPort(int cardNo, int port)
        {
            int m_nOutputState = m_ACS[cardNo].GetOutputPort(port);
            return m_nOutputState;
        }

        public double GetVelocity(int cardNo, Axis axis)
        {
            double txtVel = m_ACS[cardNo].GetVelocity((Axis)axis);
            return txtVel;
        }

        public double GetAcceleration(int cardNo, Axis axis)
        {
            double txtAcc = m_ACS[cardNo].GetAcceleration((Axis)axis);
            return txtAcc;
        }

        public double GetDeceleration(int cardNo, Axis axis)
        {
            double txtDec = m_ACS[cardNo].GetDeceleration((Axis)axis);
            return txtDec;
        }

        public double GetKillDeceleration(int cardNo, Axis axis)
        {
            double txtKdec = m_ACS[cardNo].GetKillDeceleration((Axis)axis);
            return txtKdec;
        }

        public double GetJerk(int cardNo, Axis axis)
        {
            double txtJerk = m_ACS[cardNo].GetJerk((Axis)axis);
            return txtJerk;
        }

        public void SetVelocityImm(int cardNo, Axis axis, double velocity)
        {
            m_ACS[cardNo].SetVelocityImm((Axis)axis, velocity);
        }

        public void SetAccelerationImm(int cardNo, Axis axis, double acceleration)
        {
            m_ACS[cardNo].SetAccelerationImm((Axis)axis, acceleration);
        }

        public void SetDecelerationImm(int cardNo, Axis axis, double deceleration)
        {
            m_ACS[cardNo].SetDecelerationImm((Axis)axis, deceleration);
        }

        public void SetKillDecelerationImm(int cardNo, Axis axis, double killDeceleration)
        {
            m_ACS[cardNo].SetKillDecelerationImm((Axis)axis, killDeceleration);
        }

        public void SetJerkImm(int cardNo, Axis axis, double jerk)
        {
            m_ACS[cardNo].SetJerkImm((Axis)axis, jerk);
        }

        public int GetInput(int cardNo, int port, int bit)
        {
            if (IsConnected(cardNo))
            {
                return m_ACS[cardNo].GetInput(port, bit);
            }
            else
            {
                return 0;
            }
        }

        public int GetInputAsync(int cardNo, int port, int bit)
        {
            if (IsConnected(cardNo))
            {
                ACSC_WAITBLOCK wb = m_ACS[cardNo].GetInputAsync(port, bit);
                var retVal = m_ACS[cardNo].GetResult(wb, 1000);
                return (int)retVal;
            }
            else
            {
                return 0;
            }
        }

        public int GetOutput(int cardNo, int port, int bit)
        {
            if (IsConnected(cardNo))
            {
                return m_ACS[cardNo].GetOutput(port, bit);
            }
            else
            {
                return 0;
            }
        }

        public int GetOutputAsync(int cardNo, int port, int bit)
        {
            if (IsConnected(cardNo))
            {
                ACSC_WAITBLOCK wb = m_ACS[cardNo].GetOutputAsync(port, bit);
                var retVal = m_ACS[cardNo].GetResult(wb, 1000);
                return (int)retVal;
            }
            else
            {
                return 0;
            }
        }

        public void SetOutput(int cardNo, int port, int bit, int status)
        {
            if (IsConnected(cardNo))
            {
                m_ACS[cardNo].SetOutput(port, bit, status);
            }
        }

        #endregion Signal / State

        #region Event
        public void EnableEvent(int cardNo, Interrupts flags)
        {
            m_ACS[cardNo].EnableEvent(flags);
        }
        #endregion Event

        #region AxisInMotion
        /// <summary>
        /// Track the motor whether it is busy rotating or has already stopped.
        /// This method will return true when the motor is moving and return false
        /// when the motor stops.
        /// </summary>
        /// <param name="boardID"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public bool AxisInMotion(int cardNo, Axis axis)
        {
            MotorStates m_nMotorState = GetMotorState(cardNo, (Axis)axis);

            if ((m_nMotorState & MotorStates.ACSC_MST_MOVE) != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

#endregion

        #region In Position Signal
        /// <summary>
        /// The in-position signals INP from the servo motor driver indicate the
        /// deviation error is zero, that is the servo position error is zero.
        /// </summary>
        /// <param name="boardID"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public bool AxisInPos(int cardNo, Axis axis)
        {
            try
            {
                MotorStates m_nMotorState = GetMotorState(cardNo, (Axis)axis);

                if ((m_nMotorState & MotorStates.ACSC_MST_INPOS) != 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Err_Msg = ex.Message.ToString();
                return false;
            }
        }
#endregion

        #region Master Slave
        public void MasterSlave(int cardNo)
        {
            //Stop Buffer Program
            //m_ACS.StopBuffer(ProgramBuffer.ACSC_BUFFER_58);
            //Run Buffer Program
            string temp = "Limitsetting_Masterslave";
            // Run buffer program from label position
            //m_ACS.RunBuffer(ProgramBuffer.ACSC_BUFFER_58, temp);

            //test Parallel Move
            int timeout = 5000;
            string mFormula = "RPOS(0)";
            Axis[] m_arrAxisListTest = new Axis[] { Axis.ACSC_AXIS_0, Axis.ACSC_AXIS_1, Axis.ACSC_NONE };
            m_ACS[cardNo].EnableM(m_arrAxisListTest);

            // Wait axis 0 enabled during 5 sec
            m_ACS[cardNo].WaitMotorEnabled(Axis.ACSC_AXIS_0, 1, timeout);
            // Wait till axis 1 is enabled during 5 sec
            m_ACS[cardNo].WaitMotorEnabled(Axis.ACSC_AXIS_1, 1, timeout);

            ProgramBuffer SetMasterSlave = (ProgramBuffer)2;
            //m_ACS.SetMaster(Axis.ACSC_AXIS_1, mFormula);
            //m_ACS.Slave(MotionFlags.ACSC_NONE, Axis.ACSC_AXIS_1);
            m_ACS[cardNo].RunBuffer((ProgramBuffer)SetMasterSlave, null);
        }

        public bool GetMasterSlaveState(int cardNo, Axis axis)
        {
            if (IsConnected(cardNo))
            {
                string GetSyncStatusCmd = "?MFLAGS(1).17";
                string GetSyncStatus = m_ACS[cardNo].Transaction(GetSyncStatusCmd);
                if (GetSyncStatus.Contains("0"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

        public void DisableMasterSlave(int cardNo)
        {
            //Run Buffer Program
            ProgramBuffer DisableMasterSlave = (ProgramBuffer)3;
            m_ACS[cardNo].RunBuffer((ProgramBuffer)DisableMasterSlave, null);
        }
#endregion Master Slave

        #region IO
        public void SendCmd(int cardNo, string cmd, out bool isSendOK, out UInt32 value)
        {
            value = 0;

            if (IsConnected(cardNo))
            {
                ACSC_WAITBLOCK wb = m_ACS[cardNo].TransactionAsync(cmd.Trim());
                var retVal = m_ACS[cardNo].GetResult(wb, 1000);

                if (int.TryParse(retVal.ToString().Trim('\r'), out int conVert))
                {
                    value = (UInt32)conVert;
                    isSendOK = true;
                }
                else
                {
                    isSendOK = false;
                }
            }
            else
            {
                isSendOK = false;
            }
        }

        public bool ReadCmd(int cardNo, string cmd)
        {
            if (IsConnected(cardNo))
            {
                try
                {
                    ACSC_WAITBLOCK wb = m_ACS[cardNo].TransactionAsync(cmd.Trim());
                    var retVal = m_ACS[cardNo].GetResult(wb, 1000);

                    if (retVal.ToString().Contains("1"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch(Exception ex)
                {
                    throw ex;
                }
              
            }
            else
            {
                return false;
            }
        }

        public void SendCmd(int cardNo, string cmd)
        {
            if (IsConnected(cardNo))
            {
                m_ACS[cardNo].Command(cmd.Trim());
            }
        }
        #endregion IO

        public string Err_Msg
        {
            set
            {
                m_ErrMsg.Remove(0, m_ErrMsg.Length);
                m_ErrMsg.Append(value);
            }
            get { return m_ErrMsg.ToString(); }
        }
    }
}
