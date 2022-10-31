using ConfigManager;
using ConfigManager.Constant;
using GreatechApp.Core.Helpers;
using Sequence.MachineSeq;

namespace InterlockManager.Motion
{
    public class YAxisMtrIntL : BaseMtrIntL
    {
        private int m_Tolerance = 3;

        public YAxisMtrIntL()
        {
            Provider = MotCFG.YAxis;
        }

        public override bool CheckMtrInterlock(BaseCfg cfg, bool isSkipBusy)
        {
            MotionConfig motCfg = cfg as MotionConfig;

            if (motCfg == null)
            {
                return false;
            }

            if (base.CheckMtrInterlock(motCfg, isSkipBusy))
            {
                return true;
            }

            if (m_BaseMotion.AxisInMotion(motCfg.Axis.CardID, motCfg.Axis.AxisID))
            {
                m_IntLMsg.Append("- Y Axis lifter motor is moving.").AppendLine().
                    Append("*** Please stop the motor. Please check motor busy signal from controller.").AppendLine();
            }

            // Check PnPLifter current position
            double curPos = m_BaseMotion.GetEncoderPosition(motCfg.Axis.CardID, motCfg.Axis.AxisID);            // in mm
            if (curPos == 0)
            {
                m_IntLMsg.Append("- Motor position cannot be read.").AppendLine().
                          Append("  System cannot determine the position of Magazine Input motor.").AppendLine().
                          Append("*** Please check motor encoder.").AppendLine();

            }


            double curPnPLifterPos = m_BaseMotion.GetEncoderPosition(motCfg.Axis.CardID, motCfg.Axis.AxisID);   // in mm

            MotionConfig mtrcfg = MotionConfig.Open(m_SystemConfig.MotCfgRef[(int)MotCFG.ZAxis].Reference);


            if (m_BaseMotion.AxisInMotion(mtrcfg.Axis.CardID, mtrcfg.Axis.AxisID))
            {
                m_IntLMsg.Append("- Y Axis Lifter motor is moving.").AppendLine().
                    Append("*** Please stop the motor. Please check motor busy signal from controller.").AppendLine();
            }

            double safeDist = mtrcfg.Position[(int)SampleSeq.P_PRF.Place].Point;
            double min = safeDist - m_Tolerance;
            double max = safeDist + m_Tolerance;
            bool IsInRange = SupportMethod.IsInRange(curPnPLifterPos, min, max, false);

            if (curPnPLifterPos < safeDist)
            {
                // no interlock needed if current position is lesser than safe Dist (toward -limit)
            }
            else
            {
                if (!IsInRange)
                {
                    m_IntLMsg.Append("- Y Axis Lifter not at Up position.").AppendLine().
                        Append("*** Please move PnPLifter to Up position.").AppendLine();
                }
            }

            return Finalize();
        }
    }
}
