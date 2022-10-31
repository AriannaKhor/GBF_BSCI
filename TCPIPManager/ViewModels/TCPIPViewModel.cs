using ConfigManager;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using GreatechApp.Services.UserServices;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace TCPIPManager.ViewModels
{

    public class TCPIPViewModel : BindableBase, IAccessService
    {
        #region Variable

        private AuthService m_authService;

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

        IEventAggregator m_EventAggregator;
        ITCPIP m_TCPIP;
        SystemConfig m_SysCfg;
        CultureResources m_CultureResources;

        public DelegateCommand<TCPIPParameter> OpenCommand { get; private set; }
        public DelegateCommand<TCPIPParameter> CloseCommand { get; private set; }
        public DelegateCommand<TCPIPParameter> SaveCommand { get; private set; }
        public DelegateCommand<TCPIPParameter> SendCommand { get; private set; }
        public DelegateCommand<TCPIPParameter> ClearMsgCommand { get; private set; }
        public DelegateCommand<TCPIPParameter> ClearCmdCommand { get; private set; }
        #endregion

        #region Constructor
        public TCPIPViewModel(IEventAggregator eventAggregator, ITCPIP tcpIP, AuthService authService, SystemConfig sysConfig, CultureResources cultureResources)
        {
            m_authService = authService;
            m_EventAggregator = eventAggregator;
            m_TCPIP = tcpIP;
            m_SysCfg = sysConfig;
            m_CultureResources = cultureResources;

            TCPParamList = new List<TCPIPParameter>();

            for (int i = 0; i < Enum.GetNames(typeof(NetworkDev)).Length; i++)
            {
                TCPParamList.Add(new TCPIPParameter()
                {
                    ID = i,
                    TabTitle = m_TCPIP.clientSockets[i].Name,
                    IPAddress = m_TCPIP.clientSockets[i].IPAdddress,
                    Port = m_TCPIP.clientSockets[i].Port,
                    IsPortOpen = m_TCPIP.clientSockets[i].IsAlive,
                    TCPCommand = string.Empty,
                    TCPMsg = new ObservableCollection<MessageList>(),
                });
            }

            Title = m_CultureResources.GetStringValue("TCPIP");

            OpenCommand = new DelegateCommand<TCPIPParameter>(Open);
            CloseCommand = new DelegateCommand<TCPIPParameter>(Close);
            SaveCommand = new DelegateCommand<TCPIPParameter>(Save);
            SendCommand = new DelegateCommand<TCPIPParameter>(Send);
            ClearMsgCommand = new DelegateCommand<TCPIPParameter>(ClearMsg);
            ClearCmdCommand = new DelegateCommand<TCPIPParameter>(ClearCmd);

            m_EventAggregator.GetEvent<TCPIPMsg>().Subscribe(TCPIPMessageReceived);
            m_EventAggregator.GetEvent<TCPIPStatus>().Subscribe(TCPIPMessageStatus);
            m_EventAggregator.GetEvent<MachineState>().Subscribe(OnMachineState);
            m_EventAggregator.GetEvent<ValidateLogin>().Subscribe(OnValidateLogin);
            m_EventAggregator.GetEvent<CultureChanged>().Subscribe(OnCultureChanged);

            RaisePropertyChanged(nameof(CanAccess));

            OnMachineState(Global.MachineStatus);
        }

        #endregion

        #region Event
        private void OnCultureChanged()
        {
            Title = m_CultureResources.GetStringValue("TCPIP");
        }
        private void OnValidateLogin(bool IsAuthenticated)
        {
            RaisePropertyChanged(nameof(CanAccess));
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
            m_TCPIP.clientSockets[tcpSocketTabList.ID].Connect(tcpSocketTabList.IPAddress, tcpSocketTabList.Port);
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
            sysCfg.Save();
        }

        private void Send(TCPIPParameter tcpSocketTabList)
        {
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
        public IUser CurrentUser { get; }

        public bool CanAccess
        {
            get
            {
                if (m_authService.CheckAccess(ACL.Communication) && m_authService.CurrentUser.IsAuthenticated && Global.MachineStatus != MachineStateType.Running)
                {
                    return true;
                }
                return false;
            }
        }
        #endregion
    }
}
