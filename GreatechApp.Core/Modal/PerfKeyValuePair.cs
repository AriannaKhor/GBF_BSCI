using Prism.Mvvm;

namespace GreatechApp.Core.Modal
{
    public class PerfKeyValuePair : BindableBase
    {
        public string m_Value;
        public string m_MinCycleTime;
        public string m_AvgCycleTime;
        public string m_MaxCycleTime;

        public PerfKeyValuePair()
        {
            Title = string.Empty;
            Value = "-";
        }

        public PerfKeyValuePair(string title)
        {
            Title = title;
            Value = "-";
        }

        public string Title { get; set; }

        public string Value
        {
            get { return m_Value; }
            set
            {
                SetProperty(ref m_Value, value);
            }
        }

        public string MinCycleTime
        {
            get { return m_MinCycleTime; }
            set
            {
                SetProperty(ref m_MinCycleTime, value);
            }
        }

        public string AvgCycleTime
        {
            get { return m_AvgCycleTime; }
            set
            {
                SetProperty(ref m_AvgCycleTime, value);
            }
        }

        public string MaxCycleTime
        {
            get { return m_MaxCycleTime; }
            set
            {
                SetProperty(ref m_MaxCycleTime, value);
            }
        }
    }
}
