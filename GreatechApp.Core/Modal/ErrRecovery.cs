using GreatechApp.Core.Enums;
using Prism.Mvvm;

namespace GreatechApp.Core.Modal
{
    public class ErrRecovery : BindableBase
    {
        private SQID m_AlarmModule;
        public SQID AlarmModule
        {
            get { return m_AlarmModule; }
            set { SetProperty(ref m_AlarmModule, value); }
        }

        private bool m_IsSkipRetest;
        public bool IsSkipRetest
        {
            get { return m_IsSkipRetest; }
            set { SetProperty(ref m_IsSkipRetest, value); }
        }
    }
}
