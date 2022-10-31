namespace Sequence.Framework
{
    public class TestEventArg
    {
        public enum Run_Mode
        {
            None = -1,
            Stop = 0,
            TestRun,
            HomeMotor,
            Custom1,
            Custom2,
            Custom3,
            Custom4,
            Custom5,
        }

        public TestEventArg()
        {
            RunMode = Run_Mode.None;
            ErrMsg = string.Empty;
            CycleCnt = 0;
        }

        public Run_Mode RunMode { get; set; }
        public int AxisID { get; set; }
        public string ErrMsg { get; set; }
        public int CycleCnt { get; set; }
        public int MotorIndex { get; set; }
        public string FirstSN { get; set; }
        public string LastSN { get; set; }
    }
}
