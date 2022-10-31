using System;

namespace Sequence.UIEventArg
{
    public class SampleSeq1UIEventArg : EventArgs
    {
        public int TotalInput { get; set; }

        public SampleSeq1UIEventArg()
        {
            TotalInput = 0;
        }
    }
}
