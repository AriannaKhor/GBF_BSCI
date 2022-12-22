﻿using ConfigManager;
using ConfigManager.Constant;
using GreatechApp.Core.Helpers;
using Sequence.MachineSeq;
using System;

namespace InterlockManager.Motion
{
    public class XAxisMtrIntL : BaseMtrIntL
    {
        private int m_Tolerance = 3;

        public XAxisMtrIntL()
        {
            Provider = MotCFG.XAxis;
        }

        public override bool CheckMtrInterlock(BaseCfg cfg, bool isSkipBusy)
        {
            MotionConfig motCfg = cfg as MotionConfig;

            if (motCfg == null)
            {
                return false;
            }

            if(base.CheckMtrInterlock(motCfg, isSkipBusy))
            {
                return true;
            }

            if (m_BaseMotion.AxisInMotion(motCfg.Axis.CardID, motCfg.Axis.AxisID))
            {
                m_IntLMsg.Append("- X Axis lifter motor is moving.").AppendLine().
                    Append("*** Please stop the motor. Please check motor busy signal from controller.").AppendLine();
            }

            // Check X Axis current position
            double curPos = m_BaseMotion.Pulse2mm(motCfg.Axis.Revolution, motCfg.Axis.Pitch, Convert.ToInt32(m_BaseMotion.GetEncoderPosition(motCfg.Axis.CardID, motCfg.Axis.AxisID))); // in mm
            if (curPos == 0)
            {
                m_IntLMsg.Append("- Motor position cannot be read.").AppendLine().
                          Append("  System cannot determine the position of X Axis motor.").AppendLine().
                          Append("*** Please check motor encoder.").AppendLine();

            }


            double curPnPLifterPos = m_BaseMotion.Pulse2mm(motCfg.Axis.Revolution, motCfg.Axis.Pitch, Convert.ToInt32(m_BaseMotion.GetEncoderPosition(motCfg.Axis.CardID, motCfg.Axis.AxisID))); // in mm

            MotionConfig mtrcfg = MotionConfig.Open(m_SystemConfig.MotCfgRef[(int)MotCFG.XAxis].Reference);


            //if (m_BaseMotion.AxisInMotion(mtrcfg.Axis.CardID, mtrcfg.Axis.AxisID))
            //{
            //    m_IntLMsg.Append("- X Axis Lifter motor is moving.").AppendLine().
            //        Append("*** Please stop the motor. Please check motor busy signal from controller.").AppendLine();
            //}
           
            //double safeDist = mtrcfg.Position[(int)TopVisionSeq.P_PRF.Pick].Point;
            //double min = safeDist - m_Tolerance;
            //double max = safeDist + m_Tolerance;
            //bool IsInRange = SupportMethod.IsInRange(curPnPLifterPos, min, max, false);

            //if (curPnPLifterPos < safeDist)
            //{
            //    // no interlock needed if current position is lesser than safe Dist (toward -limit)
            //}
            //else
            //{
            //    if (!IsInRange)
            //    {
            //        m_IntLMsg.Append("- X Axis Lifter not at Up position.").AppendLine().
            //            Append("*** Please move X Axis to Up position.").AppendLine();
            //    }
            //}

            return Finalize();
        }
    }
}
