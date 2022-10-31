using System;

namespace Sequence.SeqEventArg
{
    internal class Sample2EventArg: EventArgs
    {
        internal int pickcnt { get; set; }
        internal int placecnt { get; set; }

        private void InitData()
        {
            pickcnt = 0;
            placecnt = 0;
        }

        public Sample2EventArg()
        {
            InitData();
        }
    }
}
