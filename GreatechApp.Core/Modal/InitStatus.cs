using GreatechApp.Core.Enums;
using Prism.Mvvm;

namespace GreatechApp.Core.Modal
{
    public class InitStatus : BindableBase
    {
        private string m_StateIcon;
        private string GrayIcon = "/GreatechApp.Core;component/Icon/GrayIcon.png";
        private string GreenIcon = "/GreatechApp.Core;component/Icon/GreenIcon.png";
        private string RedIcon = "/GreatechApp.Core;component/Icon/RedIcon.png";

        // Constructor
        public InitStatus(SQID seqID)
        {
            SeqID = seqID;
            SeqName = seqID.ToString();
            StateIcon = GrayIcon;
        }

        public void InitData()
        {
            m_StateIcon = GrayIcon;
        }

        public SQID SeqID { get; private set; }
        public string SeqName { get; set; }
        public string StateIcon
        {
            get { return m_StateIcon; }
            set { SetProperty(ref m_StateIcon, value); }
        }
    }
}
