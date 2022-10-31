namespace GreatechApp.Services.Utilities
{
    using System;
    using System.Runtime.InteropServices;
    using System.ComponentModel;
	using GreatechApp.Core.Variable;

	public class CTimer
    {
        #region import DLL
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);
        #endregion 

        internal const double MILI_FACTOR = 1000;
        internal const double MICRO_FACTOR = 1000000;

        private long m_ClockFreq = 0;
        private long m_StartTime = 0;
        private long m_DelayTime = 0;

        public CTimer()
        {
            // Constructor
            // Get the tick frequency
            if (QueryPerformanceFrequency(out m_ClockFreq) == false)
            {
                // high-performance counter not supported
                throw new Win32Exception();
            }
        }

        public bool TimeOut()
        {
            if (Global.SeqStop && Global.AutoMode && Global.MachineStatus != Core.Enums.MachineStateType.Recovery)
            {
                m_StartTime = Mili_Time;
                return false;
            }
            // Timeout will not occur if machine is not running. (Prevents error timeout when process not running)
            return CalculateRunTime();
        }

        public float Time_Out
        {
            set
            {
                // Set the delay start time
                m_StartTime = Mili_Time;
                // Set the total delay time - UoM for value is second.
                // Convert to milisecond.
                m_DelayTime = Convert.ToInt32(value * 1000f);
            }
            // This will return the value in seconds.
            get { return Convert.ToSingle(m_DelayTime) / 1000f; }
        }

        public long Mili_Time
        {
            get { return ClockCnt2Time(MILI_FACTOR); }
        }

        private bool CalculateRunTime()
        {
            long current_time = 0;
            current_time = Mili_Time;
            if (current_time - m_StartTime >= m_DelayTime)
            {
                m_StartTime = current_time;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Convert the Performance Counter into time unit
        /// milisecond (factor = 1000); microsecond (factor = 1000000)
        /// </summary>
        /// <param name="factor"></param>
        /// <returns></returns>
        private long ClockCnt2Time(double factor)
        {
            long highres_time;
            // Get current Clock
            QueryPerformanceCounter(out highres_time);

            return (long)Math.Round(((double)highres_time * factor / (double)m_ClockFreq), 0);
        }
    }
}
