
namespace MotionManager
{
    public interface IBaseMotion
    {
        int TotalCards { get; set; }
        int MaxAxisPerCard { get; set; }
        string ErrorMsg { get; set; }

        double[,] Rev { get; set; }
        double[,] Pitch { get; set; }

        bool IsConnect(int cardNo);
        bool ServoON(int cardNo, int axis);
        bool ServoOFF(int cardNo, int axis);
        bool GetServoStatus(int cardNo, int axis);

        bool GetAlarmStatus(int cardNo, int axis);
        bool ResetMotorAlarm(int cardNo, int axis, bool isResetMtr);
        bool IsMotorAlarmReset(int cardNo, int axis);

        bool SetAxisParam(int cardNo, int axis, double drvSpeed, double acc, double dcc, double jerk, double Kdec);

        bool SetAxisJogParam(int cardNo, int axis, double drvSpeed, double acc, double dcc);

        bool FindEdge(int cardNo, int axis, double speed);

        bool SetZeroPosition(int cardNo, int axis);

        bool Jog(int cardNo, int axis, short direction, double speed);

        bool MoveAbs(int cardNo, int axis, int pulse, short direction);

        bool MoveRel(int cardNo, int axis, int distance);

        bool StopServo(int cardNo, int axis);

        bool AxisInMotion(int cardNo, int axis);

        double GetEncoderPosition(int cardNo, int axis);

        double GetLogCnt(int cardNo, int axis);

        bool GetMotionDoneStatus(int cardNo, int axis);

        bool GetPositiveLimitStatus(int cardNo, int axis);

        bool GetNegativeLimitStatus(int cardNo, int axis);

        bool GetHomeLimitStatus(int cardNo, int axis);

        float Pulse2mm(int pulseRev, float pitch, int pulseDist);

        float Pulse2mm(int pulseRev, float pitch, float pulseDist);

        int mm2Pulse(int pulseRev, float pitch, float mmDist, bool returnEncoderPos = false);

        float Pulse2degree(int pulseRev, float pulseDegree);

        int degree2pulse(int pulseRev, float degree);
    }
}
