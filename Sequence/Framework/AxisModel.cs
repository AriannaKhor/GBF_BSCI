using ConfigManager;
using System;
using System.Collections.Generic;

namespace Sequence.Framework
{
    public class AxisModel
    {
        public AxisModel()
        {
            MotCfgs = new List<MotionConfig>();
            Status = new List<AxisStatus>();
            AlarmInputs = new List<Enum>();
        }

        public void ClearAll()
        {
            MotCfgs.Clear();
            Status.Clear();
            AlarmInputs.Clear();
        }

        public List<MotionConfig> MotCfgs { get; set; }

        public List<AxisStatus> Status { get; set; }

        public List<Enum> AlarmInputs { get; set; }

        public int NumOfAxis
        {
            get { return MotCfgs.Count; }
        }
    }
}
