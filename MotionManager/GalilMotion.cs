using ConfigManager;
using System;

namespace MotionManager
{
    public class GalilMotion : BaseMotion, IGalilMotion
    {
        private Galil.Galil[] m_Galil;
        private bool[] m_IsConnected;

        public GalilMotion()
        {
            SystemConfig config = SystemConfig.Open(@"..\Config Section\General\System.Config");

            TotalCards = config.Motion.NumOfController;
            MaxAxisPerCard = config.Motion.NumOfAxis;

            m_Galil = new Galil.Galil[TotalCards];
            m_IsConnected = new bool[TotalCards];
            for (int i = 0; i < TotalCards; i++)
            {
                m_Galil[i] = new Galil.Galil();
                m_IsConnected[i] = ConnectDevice(config.MotionCards[i].CardID, config.MotionCards[i].DeviceAddress);

                if (m_IsConnected[i])
                    Console.WriteLine($"Galil Card {i}: Connected!");

                else
                    Console.WriteLine($"Galil Card {i}: Not Connected!");
            }
        }

        private bool ConnectDevice(int cardNo, string ipAddress)
        {
            try
            {
                if (m_Galil != null)
                {
                    if (m_IsConnected[cardNo]) return true;
                    m_Galil[cardNo].address = ipAddress;
                    m_Galil[cardNo].timeout_ms = 500;
                    m_IsConnected[cardNo] = true;

                    m_Galil[cardNo].command("SH");

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                SetAllConnectionToFalse();
                ErrorMsg = ex.Message.ToString();
                return false;
            }
        }


        public override bool FindEdge(int cardNo, int axis, double speed)
        {
            lock (this)
            {
                try
                {
                    char mtrAxis = Convert.ToChar(axis);

                    //string cmd = $"SP{mtrAxis}={drvSpeed};AC{mtrAxis}={acc};DC{mtrAxis}={dcc};";
                    string cmd = "SP" + mtrAxis + "=" + speed + ";" + "FE" + mtrAxis + ";" + "BG" + mtrAxis + ";";

                    m_Galil[cardNo].command(cmd);

                    return true;
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        public bool FindIndex(int cardNo, int axis, double speed)
        {
            lock (this)
            {
                try
                {
                    char mtrAxis = Convert.ToChar(axis);

                    string cmd = "FI" + mtrAxis + ";" + "BG" + mtrAxis + ";";

                    m_Galil[cardNo].command(cmd);

                    return true;
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        public override bool SetZeroPosition(int cardNo, int axis)
        {
            lock (this)
            {
                try
                {
                    char mtrAxis = Convert.ToChar(axis);

                    string cmd = "DP" + mtrAxis + "=" + "0" + ";" + "DE" + mtrAxis + "=" + "0" + ";";

                    m_Galil[cardNo].command(cmd);

                    return true;
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        public override double GetEncoderPosition(int cardNo, int axis)
        {
            lock (this)
            {
                double Value = 0;

                if (!m_IsConnected[cardNo]) { return Value; }
                try
                {
                    char mtrAxis = Convert.ToChar(axis);

                    string cmd = "MG_TP" + mtrAxis;

                    Value = m_Galil[cardNo].commandValue(cmd);

                    return Convert.ToDouble(Value);
                }

                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return Value;
                }
            }
        }

        public override double GetLogCnt(int cardNo, int axis)
        {
            lock (this)
            {
                double Value = 0;

                if (!m_IsConnected[cardNo]) { return Value; }

                try
                {
                    char mtrAxis = Convert.ToChar(axis);

                    string cmd = "MG_TD" + mtrAxis;

                    Value = m_Galil[cardNo].commandValue(cmd);

                    return Convert.ToDouble(Value);
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return Value;
                }
            }
        }

        public override bool GetMotionDoneStatus(int cardNo, int axis)
        {
            lock (this)
            {
                if (!m_IsConnected[cardNo]) { return false; }
                double Value = 0;
                bool Status;

                try
                {
                    char mtrAxis = Convert.ToChar(axis);

                    string cmd = "MG_BG" + mtrAxis;

                    Value = m_Galil[cardNo].commandValue(cmd);

                    Status = !Convert.ToBoolean(Value);// Value = 1 is Done, Value = 0 is not Done

                    return Status;
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        public override bool GetNegativeLimitStatus(int cardNo, int axis)
        {
            lock (this)
            {
                if (!m_IsConnected[cardNo]) { return false; }

                double Value = 0;
                bool Status;

                try
                {
                    char mtrAxis = Convert.ToChar(axis);

                    string cmd = "MG_LR" + mtrAxis;

                    Value = m_Galil[cardNo].commandValue(cmd);

                    Status = !Convert.ToBoolean(Value);

                    return Status;
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        public override bool GetPositiveLimitStatus(int cardNo, int axis)
        {
            lock (this)
            {
                if (!m_IsConnected[cardNo]) { return false; }

                double Value = 0;
                bool Status;

                try
                {
                    char mtrAxis = Convert.ToChar(axis);

                    string cmd = "MG_LF" + mtrAxis;

                    Value = m_Galil[cardNo].commandValue(cmd);

                    Status = !Convert.ToBoolean(Value);

                    return Status;
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        public override bool GetHomeLimitStatus(int cardNo, int axis)
        {
            lock (this)
            {
                if (!m_IsConnected[cardNo]) { return false; }

                double Value = 0;
                bool Status;

                try
                {
                    char mtrAxis = Convert.ToChar(axis);

                    string cmd = "MG_HM" + mtrAxis;

                    Value = m_Galil[cardNo].commandValue(cmd);

                    Status = !Convert.ToBoolean(Value);

                    return Status;
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
                if (!m_IsConnected[cardNo]) { return false; }

                double Value = 0;
                bool Status;
                try
                {
                    char mtrAxis = Convert.ToChar(axis);

                    string cmd = "MG_MO" + mtrAxis;

                    Value = m_Galil[cardNo].commandValue(cmd);

                    Status = !Convert.ToBoolean(Value);

                    return Status;
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
            lock (this)
            {
                try
                {
                    char mtrAxis = System.Convert.ToChar(axis);

                    string cmd = $"SP{mtrAxis}={drvSpeed};AC{mtrAxis}={acc};DC{mtrAxis}={dcc};";

                    string CMDs = "SP" + mtrAxis + "=" + drvSpeed + ";" + "AC" + mtrAxis + "=" + acc + ";" + "DC" + mtrAxis + "=" + dcc + ";";

                    m_Galil[cardNo].command(cmd);

                    return true;
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        public override bool AxisInMotion(int cardNo, int axis)
        {
            lock (this)
            {
                try
                {
                    char mtrAxis = Convert.ToChar(axis);

                    string cmd = "TS" + mtrAxis;

                    string resp = m_Galil[cardNo].command(cmd);

                    byte _axisSwBits = System.Convert.ToByte(resp);

                    // Mask bit 7 - Axis in motion if high
                    if ((_axisSwBits & 0x80) == 0x80)
                    {
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message.ToString();
                    return false;
                }
            }
        }

        public override bool Jog(int cardNo, int axis, short direction, double speed)
        {
            if (!m_IsConnected[cardNo]) { return false; }

            try
            {
                char mtrAxis = Convert.ToChar(axis);

                string cmd = "JG" + mtrAxis + "=" + (speed * direction) + ";" + "BG" + mtrAxis;

                m_Galil[cardNo].command(cmd);

                return true;
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message.ToString();
                return false;
            }
        }

        public override bool MoveAbs(int cardNo, int axis, int pulse, short direction)
        {
            if (!m_IsConnected[cardNo]) { return false; }

            try
            {
                char mtrAxis = Convert.ToChar(axis);

                string cmd = "PA" + mtrAxis + "=" + (pulse * direction) + ";" + "BG" + mtrAxis;

                m_Galil[cardNo].command(cmd);

                return true;
            }

            catch (Exception ex)
            {
                ErrorMsg = ex.Message.ToString();
                return false;
            }
        }

        public override bool MoveRel(int cardNo, int axis, int pulse)
        {
            if (!m_IsConnected[cardNo]) { return false; }

            try
            {
                char mtrAxis = Convert.ToChar(axis);

                string cmd = "PR" + mtrAxis + "=" + pulse + ";" + "BG" + mtrAxis;

                m_Galil[cardNo].command(cmd);

                return true;
            }

            catch (Exception ex)
            {
                ErrorMsg = ex.Message.ToString();
                return false;
            }
        }

        public override bool ServoOFF(int cardNo, int axis)
        {
            if (!m_IsConnected[cardNo]) { return false; }

            try
            {

                char mtrAxis = Convert.ToChar(axis);

                string cmd = "MO" + mtrAxis;

                m_Galil[cardNo].command(cmd);

                return true;
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message.ToString();
                return false;
            }
        }

        public override bool ServoON(int cardNo, int axis)
        {
            if (!m_IsConnected[cardNo]) { return false; }

            try
            {
                char mtrAxis = Convert.ToChar(axis);

                string cmd = "SH" + mtrAxis;

                m_Galil[cardNo].command(cmd);

                return true;
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message.ToString();
                return false;
            }
        }

        public override bool StopServo(int cardNo, int axis)
        {
            if (!m_IsConnected[cardNo]) { return false; }

            try
            {
                char mtrAxis = Convert.ToChar(axis);

                string cmd = "ST" + mtrAxis;

                m_Galil[cardNo].command(cmd);

                return true;
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message.ToString();
                return false;
            }
        }

        public override bool IsConnect(int cardID)
        {
            return m_IsConnected[cardID];
        }

        private void SetAllConnectionToFalse()
        {
            for (int i = 0; i < TotalCards; i++)
            {
                m_IsConnected[i] = false;
            }
        }

        public override bool GetAlarmStatus(int cardNo, int axis)
        {
            try
            {
                #region Controller Alarm
                string _resp = "";

                char mtrAxis = Convert.ToChar(axis);

                string cmd = "TS" + mtrAxis;

                _resp = m_Galil[cardNo].command(cmd);

                byte _axisSwBits = System.Convert.ToByte(_resp);
                // Mask bit 6 - Axis error exceeds error limit if high
                if ((_axisSwBits & 0x40) == 0x40)
                {
                    return true;
                }
                #endregion

                #region Driver Alarm
                // Axis 1 :
                // Bit 1 - In Position
                // Bit 2 - Motor / Driver Alarm
                // Axis 2 :
                // Bit 3 - In Position
                // Bit 4 - Motor / Driver Alarm
                // :
                // :
                int _io_bit = GetIOBit(mtrAxis);

                if (_io_bit >= 0)
                {
                    // 1st 8 Bits = TI0 : 0 - 7
                    // 2nd 8 Bits = TI1 : 8 - 15
                    string _cmd = "TI" + (_io_bit < 8 ? "0" : "1");
                    _resp = m_Galil[cardNo].command(_cmd);
                    byte _input_bits = System.Convert.ToByte(_resp);
                    // Mask motor alarm bit 2, 4, 6, 8, 10, 12, 14 & 16

                    if ((_input_bits & (1 << ((_io_bit + 1) % 8))) == 0x00)
                    {
                        return true;
                    }
                }
                #endregion

                return false;
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message.ToString();
                return false;
            }
        }

        public override bool ResetMotorAlarm(int cardNo, int axis, bool isResetAlarm)
        {

            // 1 axis 1 output bit.
            // Axis 1 :
            // Bit 1 - Alarm Reset
            // Axis 2 :
            // Bit 2 - Alarm Reset
            char _motorAxis = Convert.ToChar(axis);
            int _io_bit = GetOutBit(_motorAxis);
            if (_io_bit >= 0)
            {
                // ON / OFF Output Alarm Rset Bit
                if (AssignOutputBit(cardNo, _io_bit + 1, isResetAlarm))
                {
                    //AssignOutputBit(cardNo, _io_bit + 1, !isResetAlarm);
                }
                return false;
            }
            return false;
        }

        public override bool IsMotorAlarmReset(int cardNo, int axis)
        {
            return GetAlarmStatus(cardNo, axis);
        }

        private int GetAxis(char axis)
        {
            int _axis = -1;
            switch (axis)
            {
                case 'A': // 65
                case 'X': // 88
                    _axis = 0;
                    break;



                case 'B': // 66
                case 'Y': // 89
                    _axis = 1;
                    break;



                case 'C': // 67
                case 'Z': // 90
                    _axis = 2;
                    break;



                case 'D': // 68
                case 'W': // 87
                    _axis = 3;
                    break;



                case 'E': // 69
                    _axis = 4;
                    break;



                case 'F': // 70
                    _axis = 5;
                    break;



                case 'G': // 71
                    _axis = 6;
                    break;



                case 'H': // 72
                    _axis = 7;
                    break;
            }
            return _axis;
        }

        private int GetIOBit(char axis)
        {
            try
            {
                // 1 - 2 : Axis 1
                // 3 - 4 : Axis 2
                // 5 - 6 : Axis 3
                // 7 - 8 : Axis 4
                // 9 - 10 : Axis 5
                // 11 - 12 : Axis 6
                // 13 - 14 : Axis 7
                // 15 - 16 : Axis 8
                int _io_bit = -1;
                int _axis = GetAxis(axis);
                if (_axis >= 0 && _axis < 8)
                {
                    _io_bit = _axis * 2;
                }
                return _io_bit;
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message.ToString();
                throw ex;
            }
        }

        private int GetOutBit(char axis)
        {
            try
            {
                // Maximum Output Bit contains 4 bit. 1 axis have 1 output bit
                // 1  : Axis 1
                // 2  : Axis 2
                // 3  : Axis 3
                // 4  : Axis 4
                int _io_bit = -1;
                int _axis = GetAxis(axis);
                if (_axis >= 0 && _axis < 8)
                {
                    _io_bit = _axis;
                }
                return _io_bit;
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message.ToString();
                throw ex;
            }
        }

        private bool AssignOutputBit(int cardNo, int bit, bool isON)
        {
            try
            {
                string _cmd = (isON ? "SB" : "CB") + bit.ToString();

                m_Galil[cardNo].command(_cmd);
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
