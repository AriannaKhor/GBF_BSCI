using ACS.SPiiPlusNET;
using ACSApi;
using System;

namespace MotionManager
{
    public class ACSMotion : BaseMotion, IACSMotion
    {
        public ACSLibApi m_ACS;

        public ACSMotion()
        {
            m_ACS = new ACSLibApi();
        }

        /// <summary>
        /// Energize / Denergize the motor at a particular Axis. 
        /// Once energize, the motor will have holding torque and 
        /// can't be moved manually by hand.
        /// </summary>
        /// <param name="cardNo"></param>
        /// <param name="axis"></param>
        /// <param name="isEnabled"></param>
        /// <returns></returns>
        public override bool ServoON(int cardNo, int axis)
        {
            lock (this)
            {
                try
                {
                    m_ACS.Enable(cardNo, (Axis)axis);
                   
                    return true;
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        public override bool ServoOFF(int cardNo, int axis)
        {
            lock (this)
            {
                try
                {
                    m_ACS.Disable(cardNo, (Axis)axis);

                    return true;
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        public override bool GetServoStatus(int cardNo, int axis)
        {
            lock (this)
            {
                try
                {
                    MotorStates bMotorState = m_ACS.GetMotorState(cardNo, (Axis)axis);

                    if ((bMotorState & MotorStates.ACSC_MST_ENABLE) != 0)
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
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        /// <summary>
		/// Stop the motion of motor at a particular axis.
		/// </summary>
		/// <param name="cardNo"></param>
		/// <param name="axis"></param>
		/// <returns></returns>
        public override bool StopServo(int cardNo, int axis)
        {
            try
            {
                m_ACS.Halt(cardNo, (Axis)axis);
                return true;
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message.ToString();
                return false;
            }
        }

        /// <summary>
		/// Reset the motor position to zero - Commanded and Actual.
		/// </summary>
		/// <param name="cardNo"></param>
		/// <param name="axis"></param>
		/// <returns></returns>
		public override bool SetZeroPosition(int cardNo, int axis)
        {
            lock (this)
            {
                try
                {
                    m_ACS.SetFPosition(cardNo, (Axis)axis, 0);
                    return true;
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        /// <summary>
		/// Read the Encoder pulse from the controller. This will reflect
		/// the actual position of the motor.
		/// </summary>
		/// <param name="cardNo"></param>
		/// <param name="axis"></param>
		/// <returns></returns>
		public override double GetEncoderPosition(int cardNo, int axis)
        {
            lock (this)
            {
                double FPOS = 0;

                try
                {
                    FPOS = m_ACS.GetFPosition(cardNo, (Axis)axis);
                    return Convert.ToDouble(FPOS);
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return FPOS;
                }
            }
        }

        /// <summary>
		/// The Jog command sets the jog mode and the jog slew speed of the axes. 
		/// </summary>
		/// <param name="cardNo"></param>
		/// <param name="axis"></param>
		/// <param name="dir"></param>
		/// <returns></returns>
		public override bool Jog(int cardNo, int axis, short dir, double speed)
        {
            lock (this)
            {
                try
                {
                    double vel = dir * speed;
                    m_ACS.Jog(cardNo, 0, (Axis)axis, vel);
                    return true;
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        /// <summary>
		/// The Jog command sets the jog mode and the jog slew speed of the axes. 
		/// </summary>
		/// <param name="cardNo"></param>
		/// <param name="direction"></param>
		/// <returns></returns>
		public bool JogM(int cardNo, short direction)
        {
            lock (this)
            {
                try
                {
                    m_ACS.JogM(cardNo, 0, direction);
                    return true;
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        public override bool SetAxisParam(int cardNo, int axis, double drvSpeed, double acc, double dcc, double jerk, double Kdec)
        {
            return m_ACS.SetAxisParam(cardNo, (Axis)axis, drvSpeed, acc, dcc, jerk, Kdec);
        }

        /// <summary>
		/// The MoveRel command sets the incremental distance and direction 
		/// of the next move. 
		/// The move is referenced with respect to the current position. 
		/// </summary>
		/// <param name="cardNo"></param>
		/// <param name="axis"></param>
		/// <param name="dir"></param>
		/// <param name="pulse"></param>
		/// <returns></returns>
		public override bool MoveRel(int cardNo, int axis, int pulse)
        {
            lock (this)
            {
                try
                {
                    double Dpulse = (double)pulse / 1000;

                    m_ACS.ToPoint(cardNo, MotionFlags.ACSC_AMF_RELATIVE, (Axis)axis, Dpulse);
                    return true;

                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        /// <summary>
		/// The MoveRel command sets the incremental distance and direction 
		/// of the next move. 
		/// The move is referenced with respect to the current position. 
		/// </summary>
		/// <param name="cardNo"></param>
		/// <param name="axis"></param>
		/// <param name="pulse"></param>
		/// <returns></returns>
		public bool MoveRelM(int cardNo, short axis, int pulse)
        {
            lock (this)
            {
                try
                {
                    m_ACS.ToPointM(cardNo, MotionFlags.ACSC_AMF_RELATIVE, pulse);
                    return true;
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        /// <summary>
		/// The MoveAbs command will set the final destination of each axis. 
		/// The position is referenced to the absolute zero.
		/// </summary>
		/// <param name="cardNo"></param>
		/// <param name="axis"></param>
		/// <param name="pulse"></param>
		/// <returns></returns>
		public override bool MoveAbs(int cardNo, int axis, int pulse, short direction)
        {
            lock (this)
            {
                try
                {
                    double Dpulse = (double)pulse / 1000;

                    m_ACS.ToPoint(
                        cardNo,
                        0,                  // '0' - Absolute position
                        (Axis)axis,         // Axis number
                        Dpulse              // Target position
                        );
                    return true;

                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        /// <summary>
		/// The MoveAbs command will set the final destination of each axis. 
		/// The position is referenced to the absolute zero.
		/// </summary>
		/// <param name="Revolution"></param>
		/// <param name="Pitch"></param>
		/// <param name="pulse"></param>
		/// <returns></returns>
		public bool MoveAbsM(int cardNo, short axis, int pulse)
        {
            lock (this)
            {
                try
                {
                    m_ACS.ToPointM(
                        cardNo,
                        0,                  // '0' - Absolute position
                        pulse               // Target position
                        );
                    return true;

                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        /// <summary>
		/// The in-position signals INP from the servo motor driver indicate the
		/// deviation error is zero, that is the servo position error is zero.
		/// </summary>
		/// <param name="cardNo"></param>
		/// <param name="axis"></param>
		/// <returns></returns>
		public override bool GetMotionDoneStatus(int cardNo, int axis)
        {
            lock (this)
            {
                try
                {
                    MotorStates bMotorState = m_ACS.GetMotorState(cardNo, (Axis)axis);
                    if ((bMotorState & MotorStates.ACSC_MST_INPOS) != 0)
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
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        /// <summary>
		/// Track the motor whether it is busy rotating or has already stopped.
		/// This method will return true when the motor is moving and return false
		/// when the motor stops.
		/// </summary>
		/// <param name="cardNo"></param>
		/// <param name="axis"></param>
		/// <returns></returns>
		public override bool AxisInMotion(int cardNo, int axis)
        {
            lock (this)
            {
                try
                {
                    MotorStates bMotorState = m_ACS.GetMotorState(cardNo, (Axis)axis);
                    if ((bMotorState & MotorStates.ACSC_MST_MOVE) != 0)
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
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        /// <summary>
        /// When Axis Position Error exceeds error limit, the method will return true.
        /// </summary>
        /// <param name="cardNo"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public override bool GetAlarmStatus(int cardNo, int axis)
        {
            lock (this)
            {
                try
                {
                    //string cmd = string.Format("?sw_axis{0}.3", axis);

                    //bool ReadDriverAlarm = m_ACS.ReadCmd(cardNo, cmd);
                    SafetyControlMasks Fault = m_ACS.GetFault(cardNo, (Axis)axis);

                    if ((Fault == SafetyControlMasks.ACSC_SAFETY_DRIVE || Fault == SafetyControlMasks.ACSC_SAFETY_RL || Fault == SafetyControlMasks.ACSC_SAFETY_LL))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        /// <summary>
        /// When Axis Position Error exceeds error limit, the method will return true.
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public bool GetVariableStatus(int cardNo, string cmd)
        {
            lock (this)
            {
                try
                {
                    bool ReadDriverAlarm = m_ACS.ReadCmd(cardNo, cmd);

                    if (ReadDriverAlarm)
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
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        /// <summary>
        /// Check the Limit Sensor status of a particular axis.
        /// Typically the sensor has a NC connection, i.e. when the limit
        /// is triggered, the signal will turn OFF.
        /// </summary>
        /// <param name="cardNo"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public override bool GetPositiveLimitStatus(int cardNo, int axis)
        {
            lock (this)
            {
                try
                {
                    int intFwdLimit = m_ACS.GetInput(cardNo, axis, 17);

                    if (intFwdLimit == 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        /// <summary>
        /// Check the Limit Sensor status of a particular axis.
        /// Typically the sensor has a NC connection, i.e. when the limit
        /// is triggered, the signal will turn OFF.
        /// </summary>
        /// <param name="cardNo"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public override bool GetNegativeLimitStatus(int cardNo, int axis)
        {
            lock (this)
            {
                try
                {
                    int intRevLimit = m_ACS.GetInput(cardNo, axis, 18);

                    if (intRevLimit == 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        /// <summary>
		/// Check the Home Sensor status of a particular axis.
		/// Typically the sensor has a NO connection, i.e. when the Home
		/// is triggered, the signal will turn ON.
		/// </summary>
		/// <param name="cardNo"></param>
		/// <param name="axis"></param>
		/// <returns></returns>
		public override bool GetHomeLimitStatus(int cardNo, int axis)
        {
            lock (this)
            {
                try
                {
                    int intHome = m_ACS.GetInput(cardNo, axis, 16);

                    if (intHome == 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        #region Master Slave
        /// <summary>
        /// </summary>
        /// <param name="cardNo"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public bool GetSyncStatus(int cardNo, int axis)
        {
            lock (this)
            {
                try
                {
                    bool bSyncStatus = m_ACS.GetMasterSlaveState(cardNo, (Axis)axis);

                    if (bSyncStatus)
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
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="cardNo"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        //public override bool MasterSlave(int cardNo, int axis)
        //{
        //    lock (this)
        //    {
        //        try
        //        {
        //            m_ACS.MasterSlave();

        //            return true;
        //        }
        //        catch (Exception ex)
        //        {
        //            ErrorMsg = ex.Message.ToString();
        //            return false;
        //        }
        //    }
        //}

        /// <summary>
        /// </summary>
        /// <param name="cardNo"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public bool DisableMasterSlave(int cardNo, int axis)
        {
            lock (this)
            {
                try
                {
                    m_ACS.DisableMasterSlave(cardNo);

                    return true;
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }
        #endregion Master Slave

        #region Buffer Program
        /// <summary>
        /// </summary>
        /// <param name="BufferProgram"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public bool RunBufferProgram(int cardNo, int BufferProgram, string label)
        {
            lock (this)
            {
                try
                {
                    m_ACS.RunBuffer(cardNo, (ProgramBuffer)BufferProgram, label);
                    return true;
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }
        #endregion Buffer Program

        #region Alarm
        /// <summary>
        /// Reset Motor Driver Alarm.
        /// </summary>
        /// <param name="cardNo"></param>
        /// <param name="axis"></param>
        /// <param name="isResetAlarm"></param>
        /// <returns></returns>
        public override bool ResetMotorAlarm(int cardNo, int axis, bool isResetAlarm)
        {
            lock (this)
            {
                try
                {
                    if (isResetAlarm)
                    {
                        m_ACS.Disable(cardNo, (Axis)axis);
                        m_ACS.RunBuffer(cardNo, (ProgramBuffer)17, null);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }
        #endregion Alarm

        #region ACS Motion Control
        public bool OpenDevice(int cardNo, string ipAdd, string port)
        {
            return m_ACS.ConnectTCP(cardNo, ipAdd, port);
        }

        public void ConnectSim(int cardNo)
        {
            m_ACS.ConnectSim(cardNo);
        }

        public void Disconnect(int cardNo)
        {
            m_ACS.Disconnect(cardNo);
        }

        public void Enable(int cardNo, Axis axis)
        {
            m_ACS.Enable(cardNo, (Axis)axis);
        }

        public void Disable(int cardNo, Axis axis)
        {
            m_ACS.Disable(cardNo, (Axis)axis);
        }

        public void DisableAll(int cardNo)
        {
            m_ACS.DisableAll(cardNo);
        }

        public void Kill(int cardNo, Axis axis)
        {
            m_ACS.Kill(cardNo, (Axis)axis);
        }

        public string Transaction(int cardNo, string outBuf)
        {
            return m_ACS.Transaction(cardNo, outBuf);
        }

        public bool Jog(int cardNo, MotionFlags flags, Axis axis, double velocity)
        {
            return m_ACS.Jog(cardNo, flags, axis, velocity);
        }

        public void Halt(int cardNo, Axis axis)
        {
            m_ACS.Halt(cardNo, (Axis)axis);
        }

        public void HaltM(int cardNo, Axis[] axes)
        {
            m_ACS.HaltM(cardNo, axes);
        }

        public void ToPoint(int cardNo, MotionFlags flags, Axis axis, double point)
        {
            m_ACS.ToPoint(cardNo, flags, (Axis)axis, point);
        }

        public void SetFPosition(int cardNo, Axis axis, double fPosition)
        {
            m_ACS.SetFPosition(cardNo, (Axis)axis, fPosition);
        }

        public void RunBuffer(int cardNo, ProgramBuffer buffer, string label)
        {
            m_ACS.RunBuffer(cardNo, buffer, label);
        }

        public void StopBuffer(int cardNo, ProgramBuffer buffer)
        {
            m_ACS.StopBuffer(cardNo, buffer);
        }

        public void SendTransaction(int cardNo, string cmd)
        {
            m_ACS.SendTransaction(cardNo, cmd);
        }

        public MotorStates GetMotorState(int cardNo, Axis axis)
        {
            return m_ACS.GetMotorState(cardNo, (Axis)axis);
        }

        public double GetRPosition(int cardNo, Axis axis)
        {
            return m_ACS.GetRPosition(cardNo, (Axis)axis);
        }

        public double GetFPosition(int cardNo, Axis axis)
        {
            return m_ACS.GetFPosition(cardNo, (Axis)axis);
        }

        public object ReadVariable(int cardNo, string variable, ProgramBuffer nBuf = ProgramBuffer.ACSC_NONE, int from1 = -1, int to1 = -1, int from2 = -1, int to2 = -1)
        {
            return m_ACS.ReadVariable(cardNo, variable, nBuf, from1, to1, from2, to2);
        }

        public ProgramStates GetProgramState(int cardNo, ProgramBuffer buffer)
        {
            return m_ACS.GetProgramState(cardNo, buffer);
        }

        public object ReadVariableAsVector(int cardNo, string variable, ProgramBuffer nBuf = ProgramBuffer.ACSC_NONE, int from1 = -1, int to1 = -1, int from2 = -1, int to2 = -1)
        {
            return m_ACS.ReadVariableAsVector(cardNo, variable, nBuf, from1, to1, from2, to2);
        }

        public int GetInputPort(int cardNo, int port)
        {
            return m_ACS.GetInputPort(cardNo, port);
        }

        public int GetOutputPort(int cardNo, int port)
        {
            return m_ACS.GetOutputPort(cardNo, port);
        }

        public double GetVelocity(int cardNo, Axis axis)
        {
            return m_ACS.GetVelocity(cardNo,(Axis)axis);
        }

        public double GetAcceleration(int cardNo, Axis axis)
        {
            return m_ACS.GetAcceleration(cardNo, (Axis)axis);
        }

        public double GetDeceleration(int cardNo, Axis axis)
        {
            return m_ACS.GetDeceleration(cardNo, (Axis)axis);
        }

        public double GetKillDeceleration(int cardNo, Axis axis)
        {
            return m_ACS.GetKillDeceleration(cardNo, (Axis)axis);
        }

        public double GetJerk(int cardNo, Axis axis)
        {
            return m_ACS.GetJerk(cardNo, (Axis)axis);
        }

        public void EnableEvent(int cardNo, Interrupts flags)
        {
            m_ACS.EnableEvent(cardNo, flags);
        }

        public void SetVelocityImm(int cardNo, Axis axis, double velocity)
        {
            m_ACS.SetVelocityImm(cardNo, (Axis)axis, velocity);
        }

        public void SetAccelerationImm(int cardNo, Axis axis, double acceleration)
        {
            m_ACS.SetAccelerationImm(cardNo, (Axis)axis, acceleration);
        }

        public void SetDecelerationImm(int cardNo, Axis axis, double deceleration)
        {
            m_ACS.SetDecelerationImm(cardNo, (Axis)axis, deceleration);
        }

        public void SetKillDecelerationImm(int cardNo, Axis axis, double killDeceleration)
        {
            m_ACS.SetKillDecelerationImm(cardNo, (Axis)axis, killDeceleration);
        }

        public void SetJerkImm(int cardNo,Axis axis, double jerk)
        {
            m_ACS.SetJerkImm(cardNo, (Axis)axis, jerk);
        }

        public bool AxisInMotion(int cardNo, Axis axis)
        {
            return m_ACS.AxisInMotion(cardNo, (Axis)axis);
        }

        public bool AxisInPos(int cardNo, Axis axis)
        {
            return m_ACS.AxisInPos(cardNo, (Axis)axis);
        }
        #endregion ACS Motion Control

        public override bool IsConnect(int cardNo)
        {
            return m_ACS.IsConnected(cardNo);
        }
    }
}
