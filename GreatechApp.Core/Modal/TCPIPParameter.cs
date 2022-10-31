using GreatechApp.Core.Enums;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace GreatechApp.Core.Modal
{
    public class TCPIPParameter : BindableBase
    {
        private int m_ID;
        public int ID
        {
            get { return m_ID; }
            set { SetProperty(ref m_ID, value); }
        }

        private string m_TabTitle;
        public string TabTitle
        {
            get { return m_TabTitle; }
            set { SetProperty(ref m_TabTitle, value); }
        }

        private string m_Title;
        public string Title
        {
            get { return m_Title; }
            set { SetProperty(ref m_Title, value); }
        }

        private string m_IPAddress;
        public string IPAddress
        {
            get { return m_IPAddress; }
            set { SetProperty(ref m_IPAddress, value); }
        }

        private int m_Port;
        public int Port
        {
            get { return m_Port; }
            set { SetProperty(ref m_Port, value); }
        }

        private int m_SendDataFormat;
        public int SendDataFormat
        {
            get { return m_SendDataFormat; }
            set { SetProperty(ref m_SendDataFormat, value); }
        }

        private int m_ReceivedDataFormat;
        public int ReceivedDataFormat
        {
            get { return m_ReceivedDataFormat; }
            set { SetProperty(ref m_ReceivedDataFormat, value); }
        }

        private bool m_IsPortOpen;
        public bool IsPortOpen
        {
            get { return m_IsPortOpen; }
            set
            {
                SetProperty(ref m_IsPortOpen, value);
                RaisePropertyChanged(nameof(StatusIcon));
                RaisePropertyChanged(nameof(IsOnline));
                RaisePropertyChanged(nameof(IsOffline));
            }
        }

        public bool IsOnline => IsPortOpen;

        public bool IsOffline => !IsPortOpen;

        public string StatusIcon
        {
            get
            {
                if (IsPortOpen)
                {
                    return "/GreatechApp.Core;component/Icon/GreenIcon.png";
                }
                return "/GreatechApp.Core;component/Icon/GrayIcon.png";
            }
        }

        private string m_TCPCommand;

        public string TCPCommand
        {
            get { return m_TCPCommand; }
            set { SetProperty(ref m_TCPCommand, value); }
        }

        private ObservableCollection<MessageList> m_TCPMsg;

        public ObservableCollection<MessageList> TCPMsg
        {
            get { return m_TCPMsg; }
            set { SetProperty(ref m_TCPMsg, value); }
        }
    }

    public class MessageList : BindableBase
    {
        private string m_DateTime;
        public string DateTime
        {
            get { return m_DateTime; }
            set { SetProperty(ref m_DateTime, value); }
        }

        private string m_Message;
        public string Message
        {
            get { return m_Message; }
            set { SetProperty(ref m_Message, value); }
        }
    }
}
