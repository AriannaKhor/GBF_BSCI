namespace Sequence.Framework
{
    public class AxisStatus
    {
        public AxisStatus()
        {
            Ready = true;
            InPos = true;
            Busy = false;
            Alarm = false;
            FwdLmt = false;
            RevLmt = false;
        }

        public bool AllPass()
        {
            return Ready && InPos && !Busy && !Alarm && !FwdLmt && !RevLmt;
        }

        public bool Ready { get; set; }

        public bool InPos { get; set; }

        public bool Busy { get; set; }

        public bool Alarm { get; set; }

        public bool FwdLmt { get; set; }

        public bool RevLmt { get; set; }
    }
}
