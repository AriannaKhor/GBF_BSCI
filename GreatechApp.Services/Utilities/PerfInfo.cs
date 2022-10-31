namespace GreatechApp.Services.Utilities
{
    public class PerfInfo
    {
        public void Reset()
        {
            Max = 0;
            Min = 0;
            Avg = 0.0;
            LastCycleTime = 0;
            TotalCycle = 0;
            ArmTime = 0;
            TotalTime = 0;
        }

        public int Max { get; set; }				// Max Value.
        public int Min { get; set; }				// Min Value.
        public double Avg { get; set; }				// Average Value.
        public int LastCycleTime { get; set; }		// Last Cycle Value.
        public int TotalCycle { get; set; }			// Total Cycle.
        public long ArmTime { get; set; }
        public long TotalTime { get; set; }
    }
}
