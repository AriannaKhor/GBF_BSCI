using GreatechApp.Core.Enums;
using Prism.Mvvm;
using System;

namespace GreatechApp.Core.Modal
{
    public class AlarmParameter: BindableBase
    {
        private int m_errorCode;
        public int ErrorCode
        {
            get { return m_errorCode; }
            set { SetProperty(ref m_errorCode, value); }
        }

        private SQID m_Module;
        public SQID Module
        {
            get { return m_Module; }
            set { SetProperty(ref m_Module, value); }
        }

        private string m_station;
        public string Station
        {
            get { return m_station; }
            set { SetProperty(ref m_station, value); }
        }

        private string m_causes;
        public string Causes
        {
            get { return m_causes; }
            set { SetProperty(ref m_causes, value); }
        }

        private string m_recovery;
        public string Recovery
        {
            get { return m_recovery; }
            set { SetProperty(ref m_recovery, value); }
        }

        private string m_alarmType;
        public string AlarmType
        {
            get { return m_alarmType; }
            set { SetProperty(ref m_alarmType, value); }
        }

        private bool m_RetestOption;
        public bool RetestOption
        {
            get { return m_RetestOption; }
            set { SetProperty(ref m_RetestOption, value); }
        }

        private bool m_RetestDefault;
        public bool RetestDefault
        {
            get { return m_RetestDefault; }
            set { SetProperty(ref m_RetestDefault, value); }
        }

        private string m_picPath;
        public string Pic_Path
        {
            get { return m_picPath; }
            set { SetProperty(ref m_picPath, value); }
        }

        private DateTime m_date;
        public DateTime Date
        {
            get { return m_date; }
            set { SetProperty(ref m_date, value); }
        }

        private bool m_IsStopPage;
        public bool IsStopPage
        {
            get { return m_IsStopPage; }
            set { SetProperty(ref m_IsStopPage, value); }
        }
    }
}
