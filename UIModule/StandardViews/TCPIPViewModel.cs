using ConfigManager;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using TCPIPManager;
using UIModule.MainPanel;

namespace UIModule.StandardViews
{
    public class TCPIPViewModel : BaseUIViewModel
    {
        #region Variable
        private string m_title;
        public string Title
        {
            get { return m_title; }
            set { SetProperty(ref m_title, value); }
        }

        private List<TCPIPParameter> m_TCPParamList;
        public List<TCPIPParameter> TCPParamList
        {
            get { return m_TCPParamList; }
            set { SetProperty(ref m_TCPParamList, value); }
        }

        public DelegateCommand<TCPIPParameter> OpenCommand { get; private set; }
        public DelegateCommand<TCPIPParameter> CloseCommand { get; private set; }
        public DelegateCommand<TCPIPParameter> SaveCommand { get; private set; }
        public DelegateCommand<TCPIPParameter> SendCommand { get; private set; }
        public DelegateCommand<TCPIPParameter> ClearMsgCommand { get; private set; }
        public DelegateCommand<TCPIPParameter> ClearCmdCommand { get; private set; }
        #endregion

        #region Constructor
        public TCPIPViewModel()
        {
            TCPParamList = new List<TCPIPParameter>();
            
            for (int i = 0; i < Enum.GetNames(typeof(NetworkDev)).Length; i++)
            {
                TCPParamList.Add(new TCPIPParameter()
                {
                    ID = i,
                    TabTitle = m_TCPIP.clientSockets[i].Name,
                    IPAddress = m_TCPIP.clientSockets[i].IPAdddress,
                    Port = m_TCPIP.clientSockets[i].Port,
                    SendDataFormat = (int)m_TCPIP.clientSockets[i].SendDataFormat,
                    ReceivedDataFormat = (int)m_TCPIP.clientSockets[i].ReceivedDataFormat,
                    IsPortOpen = m_TCPIP.clientSockets[i].IsAlive,
                    TCPCommand = string.Empty,
                    TCPMsg = new ObservableCollection<MessageList>(),
                });
            }

            Title = GetStringTableValue("TCPIP");

            OpenCommand = new DelegateCommand<TCPIPParameter>(Open);
            CloseCommand = new DelegateCommand<TCPIPParameter>(Close);
            SaveCommand = new DelegateCommand<TCPIPParameter>(Save);
            SendCommand = new DelegateCommand<TCPIPParameter>(Send);
            ClearMsgCommand = new DelegateCommand<TCPIPParameter>(ClearMsg);
            ClearCmdCommand = new DelegateCommand<TCPIPParameter>(ClearCmd);

            m_EventAggregator.GetEvent<TCPIPMsg>().Subscribe(TCPIPMessageReceived);
            m_EventAggregator.GetEvent<TCPIPStatus>().Subscribe(TCPIPMessageStatus);
            m_EventAggregator.GetEvent<MachineState>().Subscribe(OnMachineState);

            OnMachineState(Global.MachineStatus);
        }

        #endregion

        #region Event
        public override void OnCultureChanged()
        {
            Title = GetStringTableValue("TCPIP");
        }

        private void OnMachineState(MachineStateType state)
        {
            RaisePropertyChanged(nameof(CanAccess));
        }
        #endregion

        #region Properties
        public bool KeepAlive
        {
            get
            {
                return false;
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            m_EventAggregator = null;
            m_TCPIP = null;
        }

        #endregion

        #region Method
        private bool IsValidHex(IEnumerable<char> chars)
        {
            bool isHex;
            foreach (var c in chars)
            {
                isHex = ((c >= '0' && c <= '9') ||
                         (c >= 'a' && c <= 'f') ||
                         (c >= 'A' && c <= 'F') ||
                         (c == ' '));

                if (!isHex)
                    return false;
            }

            if (chars.ToString().Replace(" ", "").Length % 2 != 0)
                return false;
            return true;
        }

        private void TCPIPMessageReceived(TCPIPMsg _Message)
        {
            if(Global.MachineStatus != MachineStateType.Running)
            {
                for (int i = 0; i < TCPParamList.Count; i++)
                {
                    if (_Message.IpAddress == TCPParamList[i].IPAddress && _Message.Port == TCPParamList[i].Port)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            TCPParamList[i].TCPMsg.Add(new MessageList()
                            {
                                DateTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                                Message = _Message.Message,
                            });
                        });
                    }
                }
            }
        }

        private void TCPIPMessageStatus(TCPIPStatus socketStatus)
        {
            for (int i = 0; i < TCPParamList.Count; i++)
            {
                if (socketStatus.IpAddress == TCPParamList[i].IPAddress && socketStatus.Port == TCPParamList[i].Port)
                {
                    TCPParamList[i].IsPortOpen = socketStatus.IsAlive;
                }
            }
        }

        private void Open(TCPIPParameter tcpSocketTabList)
        {
            m_TCPIP.clientSockets[tcpSocketTabList.ID].Connect(tcpSocketTabList.IPAddress, tcpSocketTabList.Port, (TCPIPDataFormat)tcpSocketTabList.SendDataFormat, (TCPIPDataFormat)tcpSocketTabList.ReceivedDataFormat);
        }

        private void Close(TCPIPParameter tcpSocketTabList)
        {
            m_TCPIP.clientSockets[tcpSocketTabList.ID].Disconnect();
        }

        private void Save(TCPIPParameter tcpSocketTabList)
        {
            SystemConfig sysCfg = SystemConfig.Open();
            sysCfg.NetworkDevices[tcpSocketTabList.ID].IPAddress = tcpSocketTabList.IPAddress;
            sysCfg.NetworkDevices[tcpSocketTabList.ID].Port = tcpSocketTabList.Port;
            sysCfg.NetworkDevices[tcpSocketTabList.ID].ReceivedDataFormat = ((TCPIPDataFormat)tcpSocketTabList.ReceivedDataFormat).ToString();
            sysCfg.NetworkDevices[tcpSocketTabList.ID].SendDataFormat = ((TCPIPDataFormat)tcpSocketTabList.SendDataFormat).ToString();
            sysCfg.Save();
            m_ShowDialog.Show(DialogIcon.Complete, GetDialogTableValue("Saved"));
        }

        private void Send(TCPIPParameter tcpSocketTabList)
        {
            if (m_TCPIP.clientSockets[tcpSocketTabList.ID].SendDataFormat == TCPIPDataFormat.HEX && !IsValidHex(tcpSocketTabList.TCPCommand))
            {
                m_ShowDialog.Show(DialogIcon.Error, GetDialogTableValue("HEXErrorTitle"), GetDialogTableValue("HEXErrorContent"));
                return;
            }
            
            m_TCPIP.clientSockets[tcpSocketTabList.ID].Send(tcpSocketTabList.TCPCommand);
        }

        private void ClearMsg(TCPIPParameter tcpSocketTabList)
        {
            tcpSocketTabList.TCPMsg.Clear();
        }

        private void ClearCmd(TCPIPParameter tcpSocketTabList)
        {
            tcpSocketTabList.TCPCommand = string.Empty;
        }
        #endregion

        #region Access Implementation

        public override bool CanAccess
        {
            get
            {
                if (m_AuthService.CurrentUser.IsAuthenticated && Global.MachineStatus != MachineStateType.Running)
                {
                    return true;
                }
                return false;
            }
        }
        #endregion
    }
}
