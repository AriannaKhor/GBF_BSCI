using System;
using System.Text;

namespace MotionManager
{
    public class BaseMotion : IBaseMotion
    {
        private int m_TotalCards;
        public virtual int TotalCards 
        {
            get { return m_TotalCards; }
            set { m_TotalCards = value; }
        }

        private int m_MaxAxisPerCard;
        public int MaxAxisPerCard
        {
            get { return m_MaxAxisPerCard; }
            set { m_MaxAxisPerCard = value; }
        }

        protected StringBuilder _ErrMsg = new StringBuilder();
        public virtual string ErrorMsg
        {
            set
            {
                _ErrMsg.Remove(0, _ErrMsg.Length);
                _ErrMsg.Append(value);
            }
            get { return _ErrMsg.ToString(); }
        }

        private double[,] m_Rev;
        public virtual double[,] Rev
        {
            get { return m_Rev; }
            set { m_Rev = value; }
        }

        private double[,] m_Pitch;
        public virtual double[,] Pitch
        {
            get { return m_Pitch; }
            set { m_Pitch = value; }
        }

        public virtual bool IsConnect(int cardNo)
        {
            return false;
        }

        public virtual bool FindEdge(int cardNo, int axis, double speed)
        {
            return false;
        }

        public virtual bool GetAlarmStatus(int cardNo, int axis)
        {
            return false;
        }

        public virtual double GetEncoderPosition(int cardNo, int axis)
        {
            return 0;
        }

        public virtual double GetLogCnt(int cardNo, int axis)
        {
            return 0;
        }

        public virtual bool GetHomeLimitStatus(int cardNo, int axis)
        {
            return false;
        }

        public virtual bool GetMotionDoneStatus(int cardNo, int axis)
        {
            return false;
        }

        public virtual bool GetNegativeLimitStatus(int cardNo, int axis)
        {
            return false;
        }

        public virtual bool GetPositiveLimitStatus(int cardNo, int axis)
        {
            return false;
        }

        public virtual bool GetServoStatus(int cardNo, int axis)
        {
            return false;
        }

        public virtual bool SetAxisParam(int cardNo, int axis, double drvSpeed, double acc, double dcc, double jerk, double Kdec)
        {
            return false;
        }

        public virtual bool SetAxisJogParam(int cardNo, int axis, double drvSpeed, double acc, double dcc)
        {
            return false;
        }

        public virtual bool Jog(int cardNo, int axis, short direction, double speed)
        {
            return false;
        }

        public virtual bool MoveAbs(int cardNo, int axis, int pulse, short direction)
        {
            return false;
        }

        public virtual bool MoveRel(int cardNo, int axis, int pulse)
        {
            return false;
        }

        public virtual bool AxisInMotion(int cardNo, int axis)
        {
            return false;
        }

        public float Pulse2mm(int pulseRev, float pitch, int pulseDist)
        {
            float distance = 0;

            if (pulseRev > 0)
            {
                distance = (pitch * pulseDist) / (float)pulseRev;
            }
            return distance;
        }

        public float Pulse2mm(int pulseRev, float pitch, float pulseDist)
        {
            float distance = 0;

            if (pulseRev > 0)
            {
                distance = (pitch * pulseDist) / (float)pulseRev;
            }
            return distance;
        }

        public int mm2Pulse(int pulseRev, float pitch, float mmDist, bool directReturnEncoderPos)
        {
            // Convert the motion distance from mm to pulse
            // 10mm (pitch) = 400 pulse (pulseRev)
            // cal_pulse = 400 * mmDist / 10

            double cal_pulse = 0;

            if (pitch > 0)
            {
                //cal_pulse = directReturnEncoderPos ? Math.Round((pulseRev * mmDist / 1000) / pitch, 0) : Math.Round((pulseRev * mmDist) / pitch, 0);
                cal_pulse = Math.Round((pulseRev * mmDist) / pitch, 4);
            }
            return Convert.ToInt32(cal_pulse);
        }

        public float Pulse2degree(int pulseRev, float pulseDegree)
        {
            float degree = 0;

            if (pulseRev > 0)
            {
                double result = (double)360 * (double)pulseDegree / (double)pulseRev;
                degree = Convert.ToSingle(Math.IEEERemainder(result, 360.0));
            }

            return Convert.ToSingle(degree);
        }

        public int degree2pulse(int pulseRev, float degree)
        {
            // Convert the motion distance from degree to pulse
            // 360 degree = 600 pulse (pulseRev)
            // x degree = 600 * x/360

            double cal_pulse = 0;
            cal_pulse = Math.Round(((float)pulseRev * degree) / 360.0, 0);
            return Convert.ToInt32(cal_pulse);
        }

        public virtual bool ResetMotorAlarm(int cardNo, int axis, bool isResetMtr)
        {
            return false;
        }

        public virtual bool IsMotorAlarmReset(int cardNo, int axis)
        {
            return false;
        }

        public virtual bool ServoOFF(int cardNo, int axis)
        {
            return false;
        }

        public virtual bool ServoON(int cardNo, int axis)
        {
            return false;
        }

        public virtual bool SetZeroPosition(int cardNo, int axis)
        {
            return false;
        }

        public virtual bool SetMoonsMtrZeroPosition(int cardNo, int axis)
        {
            return false;
        }

        public virtual bool StopServo(int cardNo, int axis)
        {
            return false;
        }

        #region Properties
        public double Resolution(int cardNo, int axis)
        {
            return Rev[cardNo, axis] / Pitch[cardNo, axis];
        }
        #endregion
    }
}
