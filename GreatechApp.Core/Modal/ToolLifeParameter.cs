using Prism.Mvvm;

namespace GreatechApp.Core.Modal
{
    public class ToolLifeParameter : BindableBase
    {
        private int m_ID;
        public int ID
        {
            get { return m_ID; }
            set { SetProperty(ref m_ID, value); }
        }

        private string m_ToolName;
        public string ToolName
        {
            get { return m_ToolName; }
            set { SetProperty(ref m_ToolName, value); }
        }

        private string m_ToolTip;
        public string ToolTip
        {
            get { return m_ToolTip; }
            set { SetProperty(ref m_ToolTip, value); }
        }

        private int m_CleaningValue;
        public int CleaningValue
        {
            get { return m_CleaningValue; }
            set { SetProperty(ref m_CleaningValue, value); }
        }

        private int m_MinCleaning;
        public int MinCleaning
        {
            get { return m_MinCleaning; }
            set { SetProperty(ref m_MinCleaning, value); }
        }

        private int m_MaxCleaning;
        public int MaxCleaning
        {
            get { return m_MaxCleaning; }
            set { SetProperty(ref m_MaxCleaning, value); }
        }

        private int m_PrevMaxCleaning;
        public int PrevMaxCleaning
        {
            get { return m_PrevMaxCleaning; }
            set { SetProperty(ref m_PrevMaxCleaning, value); }
        }

        private int m_ToolLifeValue;
        public int ToolLifeValue
        {
            get { return m_ToolLifeValue; }
            set { SetProperty(ref m_ToolLifeValue, value); }
        }

        private int m_MinToolLife;
        public int MinToolLife
        {
            get { return m_MinToolLife; }
            set { SetProperty(ref m_MinToolLife, value); }
        }

        private int m_MaxToolLife;
        public int MaxToolLife
        {
            get { return m_MaxToolLife; }
            set { SetProperty(ref m_MaxToolLife, value); }
        }

        private int m_PrevMaxToolLife;
        public int PrevMaxToolLife
        {
            get { return m_PrevMaxToolLife; }
            set { SetProperty(ref m_PrevMaxToolLife, value); }
        }

        private bool m_IsCleaningEnable;
        public bool IsCleaningEnable
        {
            get { return m_IsCleaningEnable; }
            set { SetProperty(ref m_IsCleaningEnable, value); }
        }

        private bool m_IsToolLifeEnable;
        public bool IsToolLifeEnable
        {
            get { return m_IsToolLifeEnable; }
            set { SetProperty(ref m_IsToolLifeEnable, value); }
        }
    }
}
