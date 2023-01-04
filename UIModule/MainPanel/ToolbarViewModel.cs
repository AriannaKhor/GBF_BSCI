using GreatechApp.Core;
using GreatechApp.Core.Command;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace UIModule.MainPanel
{
    public class ToolbarViewModel : BaseUIViewModel
    {
        private Thread EStopWinThread;
        private EStopView eStopView;
        //private CvsInSight m_InsightV1 = new CvsInSight();
      

        public const string GrayIcon = "/GreatechApp.Core;component/Icon/GrayIcon.png";
        public const string GreenIcon = "/GreatechApp.Core;component/Icon/GreenIcon.png";
        public const string RedIcon = "/GreatechApp.Core;component/Icon/RedIcon.png";
        public const string YellowIcon = "/GreatechApp.Core;component/Icon/YellowIcon.png";

        public const string Connected = "/GreatechApp.Core;component/Icon/TCPConnected.png";
        public const string Disconnected = "/GreatechApp.Core;component/Icon/TCPDisconnected.png";

        #region Control Visibility
        private Visibility m_IsLogin = Visibility.Collapsed;
        public Visibility IsLogin
        {
            get { return m_IsLogin; }
            set { SetProperty(ref m_IsLogin, value); }
        }

        private Visibility m_IsLogout = Visibility.Collapsed;
        public Visibility IsLogout
        {
            get { return m_IsLogout; }
            set { SetProperty(ref m_IsLogout, value); }
        }

        private Visibility m_MenuVisibility = Visibility.Collapsed;
        public Visibility MenuVisibility
        {
            get { return m_MenuVisibility; }
            set { SetProperty(ref m_MenuVisibility, value); }
        }
        private Visibility m_PerformanceMenuVisibility = Visibility.Collapsed;
        public Visibility PerformanceMenuVisibility
        {
            get { return m_PerformanceMenuVisibility; }
            set { SetProperty(ref m_PerformanceMenuVisibility, value); }
        }
        #endregion

        #region User Info
        private string m_UserInfo;
        public string UserInfo
        {
            get { return m_UserInfo; }
            set { SetProperty(ref m_UserInfo, value); }
        }

        private string m_UserId;
        public string UserId
        {
            get { return m_UserId; }
            set { SetProperty(ref m_UserId, value); }
        }

        private string m_UserLvl;
        public string UserLvl
        {
            get { return m_UserLvl; }
            set { SetProperty(ref m_UserLvl, value); }
        }

        private string m_LoginStatus;
        public string LoginStatus
        {
            get { return m_LoginStatus; }
            set
            {
                SetProperty(ref m_LoginStatus, value);
                LoginStatusWithCulture = GetStringTableValue(value);
            }
        }

        private string m_LoginStatusWithCulture;
        public string LoginStatusWithCulture
        {
            get { return m_LoginStatusWithCulture; }
            set { SetProperty(ref m_LoginStatusWithCulture, value); }
        }

        private bool m_CanAccess;
        public bool CanAccess
        {
            get { return m_CanAccess; }
            set { SetProperty(ref m_CanAccess, value); }
        }
        #endregion

        #region SSR Guarding

        private bool m_IsAllowStart;
        public bool IsAllowStart
        {
            get { return m_IsAllowStart; }
            set
            {
                SetProperty(ref m_IsAllowStart, value);
                //CheckSSRButtonAvail();
            }
        }

        private bool m_IsAllowStop;
        public bool IsAllowStop
        {
            get { return m_IsAllowStop; }
            set
            {
                SetProperty(ref m_IsAllowStop, value);
                //CheckSSRButtonAvail();
            }
        }

        private bool m_IsEndingLot = false;
        public bool IsEndingLot
        {
            get { return m_IsEndingLot; }
            set { SetProperty(ref m_IsEndingLot, value); }
        }
        #endregion

        #region Hardware SSR Button
        private DispatcherTimer tmrButtonMonitor;

        //private int StartButton = (int)IN.DI0104_Input5; // Assign Start Button Input
        //private int StopButton = (int)IN.DI0105_Input6; // Assign Stop Button Input

        //private int StartButtonIndic = (int)OUT.DO0104_Output5; // Assign Start button indicator output
        //private int StopButtonIndic = (int)OUT.DO0105_Output6; // Assign stop button indicator output
        #endregion

        #region System Clock
        private DateTime m_dateTime;
        public DateTime DateTime
        {
            get { return m_dateTime; }
            set { SetProperty(ref m_dateTime, value); }
        }

        private DispatcherTimer tmrSysClock;
        #endregion

        #region TCP/IP State
        private DispatcherTimer m_tmrTCPMonitor;

        private ObservableCollection<TCPDisplay> m_TCPCollection;
        public ObservableCollection<TCPDisplay> TCPCollection
        {
            get { return m_TCPCollection; }
            set { SetProperty(ref m_TCPCollection, value); }
        }

        private string m_TCPIPStateIcon = Disconnected;
        public string TCPIPStateIcon
        {
            get { return m_TCPIPStateIcon; }
            set { SetProperty(ref m_TCPIPStateIcon, value); }
        }

        private string m_TCPIPStatus;
        public string TCPIPStatus
        {
            get { return m_TCPIPStatus; }
            set
            {
                SetProperty(ref m_TCPIPStatus, value);
                TCPIPStatusWithCulture = GetStringTableValue(value);
            }
        }

        private string m_TCPIPStatusWithCulture;
        public string TCPIPStatusWithCulture
        {
            get { return m_TCPIPStatusWithCulture; }
            set { SetProperty(ref m_TCPIPStatusWithCulture, value); }
        }

        private bool m_IsTCPIPListOpen = false;
        public bool IsTCPIPListOpen
        {
            get { return m_IsTCPIPListOpen; }
            set { SetProperty(ref m_IsTCPIPListOpen, value); }
        }

        #endregion

        #region Machine Status
        private string m_EquipStateIcon;
        public string EquipStateIcon
        {
            get { return m_EquipStateIcon; }
            set { SetProperty(ref m_EquipStateIcon, value); }
        }

        private string m_EquipStatus;
        public string EquipStatus
        {
            get { return m_EquipStatus; }
            set
            {
                SetProperty(ref m_EquipStatus, value);
                EquipStatusWithCulture = GetStringTableValue(value);
            }
        }

        private string m_EquipStatusWithCulture;
        public string EquipStatusWithCulture
        {
            get { return m_EquipStatusWithCulture; }
            set { SetProperty(ref m_EquipStatusWithCulture, value); }
        }
        #endregion

        public event Action<IDialogResult> RequestClose;

        public DelegateCommand<string> OperationCommand { get; set; }
        public DelegateCommand RaiseMenuCommand { get; set; }
        public DelegateCommand<string> CloseMenuCommand { get; set; }
        public DelegateCommand<string> TCPIPListCommand { get; set; }
        public DelegateCommand<string> NavigateCommand { get; set; }
        public DelegateCommand LoginDialogCommand { get; set; }
        public DelegateCommand<TCPDisplay> ReconnectTCP { get; set; }
        public DelegateCommand ReconnectAllTCP { get; set; }
        public DelegateCommand<string> ControlStateCheck { get; set; }

        public event Action<string> EStopWindEvent;

        private readonly IDialogService m_DialogService;

        public ToolbarViewModel(IDialogService dialogService)
        {
            m_DialogService = dialogService;

            OperationCommand = new DelegateCommand<string>(OperationMethod);
            NavigateCommand = new DelegateCommand<string>(Navigate); //For Lot Entry 
            LoginDialogCommand = new DelegateCommand(RaiseLoginPopup);
            RaiseMenuCommand = new DelegateCommand(RaiseMenuPopup);
            CloseMenuCommand = new DelegateCommand<string>(CloseMenuPopup);
            TCPIPListCommand = new DelegateCommand<string>(OnTCPIPList);
            ReconnectTCP = new DelegateCommand<TCPDisplay>(OnReconnectTCP);
            ReconnectAllTCP = new DelegateCommand(OnReconnectAllTCP);

            m_EventAggregator.GetEvent<MachineState>().Subscribe(OnMachineStateChange);

            ApplicationCommands.OperationCommand.RegisterCommand(OperationCommand);

            IsLogin = Visibility.Visible;
            IsLogout = Visibility.Collapsed;
            LoginStatus = "Login";

            // System Clock
            tmrSysClock = new DispatcherTimer();
            tmrSysClock.Interval = new TimeSpan(0, 0, 0, 1);
            tmrSysClock.Tick += new EventHandler(tmrSysClock_Tick);
            tmrSysClock.Start();

            //// TCP/IP Monitor
            //m_tmrTCPMonitor = new DispatcherTimer();
            //m_tmrTCPMonitor.Interval = new TimeSpan(0, 0, 0, 0, 500);
            //m_tmrTCPMonitor.Tick += new EventHandler(m_tmrTCPMonitor_Tick);
            //m_tmrTCPMonitor.Start();

            MenuVisibility = Visibility.Collapsed;
            IsTCPIPListOpen = false;
            DisableAllBtn();

            EquipStateIcon = GrayIcon;
            EquipStatus = "Idle";
            m_EventAggregator.GetEvent<MachineState>().Publish(Global.MachineStatus);
        }

        #region System Clock
        public string SysDate { get; private set; }
        public string SysTime { get; private set; }
        void tmrSysClock_Tick(object sender, EventArgs e)
        {
            try
            {
                SysDate = DateTime.Now.ToString("dd-MMM-yyyy", DateTimeFormatInfo.InvariantInfo);
                SysTime = DateTime.Now.ToString("HH:mm:ss", DateTimeFormatInfo.InvariantInfo);
                RaisePropertyChanged(nameof(SysDate));
                RaisePropertyChanged(nameof(SysTime));
            }
            catch (Exception ex)
            {
                tmrSysClock.Stop();
                MessageBox.Show(ex.Message, ex.Source);
            }
        }
        #endregion

        #region SSR button availability
        public void DisableAllBtn()
        {
            IsAllowStart = false;
            IsAllowStop = false;
        }

        public void AbleAllButton()
        {
            IsAllowStart = true;
            IsAllowStop = true;
        }

        public void ProductionMode()
        {
            IsAllowStart = false;
            IsAllowStop = false;
         
        }

        public void StopMode()
        {
            IsAllowStart = true;
            IsAllowStop = false;
        }

        public void ErrorMode()
        {
            IsAllowStop = false;
        }

        public void IdleMode()
        {
            if (CanAccess)
            {
                IsAllowStart = true;
                //if (m_InsightV1.State == CvsInSightState.Online || Global.CodeReaderConnStatus == ConnectionState.Connected.ToString())
                //{
                //    IsAllowStart = true;
                //    IsAllowStop = false;
            }
        }

        public void ReadyMode()
        {
            IsAllowStart = true;
            IsAllowStop = false;
        }

        #endregion

        #region Event
        private void OnMachineStateChange(MachineStateType state)
        {
            switch (state)
            {
                case MachineStateType.Ready:
                    Global.MachineStatus = MachineStateType.Ready;
                    ReadyMode();
                    EquipStateIcon = GreenIcon;
                    EquipStatus = "Ready";
                    break;

                case MachineStateType.Running:
                    ProductionMode();
                    EquipStateIcon = GreenIcon;

                    if (IsEndingLot)
                        EquipStatus = "LotEnding";
                    else
                        EquipStatus = "Running";
                    Global.MachineStatus = MachineStateType.Running;
                    break;

                case MachineStateType.Stopped:
                    Global.MachineStatus = MachineStateType.Stopped;
                    StopMode();
                    EquipStateIcon = RedIcon;
                    EquipStatus = "Stopped";
                    break;

                case MachineStateType.Error:
                    Global.MachineStatus = MachineStateType.Error;
                    ErrorMode();
                    EquipStateIcon = RedIcon;
                    EquipStatus = "Error";
                    break;

                case MachineStateType.Lot_Ended:
                    Global.MachineStatus = MachineStateType.Lot_Ended;
                    IsEndingLot = false;
                    IdleMode();
                    EquipStateIcon = GrayIcon;
                    EquipStatus = "LotEnded";
                    break;

                case MachineStateType.Idle:
                    Global.MachineStatus = MachineStateType.Idle;
                    if (LoginStatus == "Logout")
                    {
                        IdleMode();
                        EquipStateIcon = GrayIcon;
                        EquipStatus = "Idle";
                    }
                    break;
            }
        }
        public override void OnValidateLogin(bool IsAuthenticated)
        {
            if (IsAuthenticated)
            {
                CanAccess = true;
                UserInfo = $"User ID : {m_CurrentUser.Username} {Environment.NewLine}User Level : {m_CurrentUser.UserLevel}";
                UserId = m_CurrentUser.Username;
                UserLvl = m_CurrentUser.UserLevel.ToString();
                Global.UserId = UserId;
                Global.UserLvl = UserLvl;
                IsLogin = Visibility.Collapsed;
                IsLogout = Visibility.Visible;
                LoginStatus = "Logout";
                if (m_CurrentUser.UserLevel == ACL.UserLevel.Admin && m_SystemConfig.Machine.AdminEStop) 
                {
                    EStopWinThread = new Thread(new ThreadStart(SetupEStopWindow));
                    EStopWinThread.SetApartmentState(ApartmentState.STA);
                    EStopWinThread.Name = "EStop Window";
                    EStopWinThread.IsBackground = true;
                    EStopWinThread.Start();
                }
                OnMachineStateChange(Global.MachineStatus);
                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("User")} {m_CurrentUser.Username} {GetStringTableValue("LoggedIn")}" });
            }
            else
            {
                CanAccess = false;
            }
        }

        private void SetupEStopWindow()
		{
            eStopView = new EStopView();
            eStopView.Show();
            Dispatcher.Run();
        }
  
        public override void OnCultureChanged()
        {
            TCPIPStatusWithCulture = GetStringTableValue(TCPIPStatus);
            LoginStatusWithCulture = GetStringTableValue(LoginStatus);
        }
        #endregion

        #region Method
        private void Navigate(string navigatePath)
        {
            CloseMenuPopup();
            if (navigatePath != null)
            {
                m_RegionManager.RequestNavigate(RegionNames.CenterContentRegion, navigatePath);
            }
        }

        private void Reset()
        {
            m_EventAggregator.GetEvent<CheckOperation>().Publish(true);
            m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Running);
            CloseDialog("");
        }

        void RaiseLoginPopup()
        {
            m_DialogService.ShowDialog(DialogList.LoginView.ToString(),
                                      new DialogParameters($"message={""}"),
                                      null);
        }

        void RaiseMenuPopup()
        {
            MenuVisibility = Visibility.Visible;
            IsTCPIPListOpen = false;
        }

        void CloseMenuPopup(string Command)
        {
            if (Command == "SecondRow")
            {
                // Collapsae all second row canvas
                PerformanceMenuVisibility = Visibility.Collapsed;
                IsTCPIPListOpen = false;
            }
            else if (Command == "Menu")
            {
                MenuVisibility = Visibility.Collapsed;
                PerformanceMenuVisibility = Visibility.Collapsed;
                IsTCPIPListOpen = false;
            }
        }

        void OnTCPIPList(string command)
        {
            if(command == "Open")
            {
                if(!IsTCPIPListOpen)
                    IsTCPIPListOpen = true;
                CloseMenuPopup();
            }
            else if(command == "Close")
            {
                IsTCPIPListOpen = false;
            }
        }

        void CloseMenuPopup()
        {
            PerformanceMenuVisibility = Visibility.Collapsed;
            MenuVisibility = Visibility.Collapsed;
        }

        private void OperationMethod(string Command)
        {
            if (Command == "Logout")
            {
                DisableAllBtn();
                IsTCPIPListOpen = false;
                LoginStatus = "Login";
                UserInfo = $"User ID : {m_CurrentUser.Username} {Environment.NewLine}User Level : {m_CurrentUser.UserLevel}";
                UserId = m_CurrentUser.Username;
                UserLvl = m_CurrentUser.UserLevel.ToString();
                UserId = " ";
                UserLvl = " ";
                IsLogin = Visibility.Visible;
                IsLogout = Visibility.Collapsed;
                if(EStopWinThread != null)
				{
                    if(EStopWinThread.IsAlive)
					{
                        Dispatcher.FromThread(EStopWinThread).Invoke(() =>
                        {
                            eStopView.Close();
                        });
                    }

                    EStopWinThread.Abort();
                }
                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("User")} {m_CurrentUser.Username} {GetStringTableValue("LoggedOut")}." });
                m_AuthService.CurrentUser.IsAuthenticated = false;
                m_EventAggregator.GetEvent<ValidateLogin>().Publish(false);
            }
            else if (Command == "Start")
            {
                StartOperation();
            }
            else if (Command == "Stop")
            {
                CloseDialog("");
                if (Global.AccumulateCurrentBatchQuantity == Global.LotInitialTotalBatchQuantity)
                {
                    ButtonResult dialogResult = m_ShowDialog.Show(DialogIcon.Question, GetDialogTableValue("EndLot"), GetDialogTableValue("AskConfirmEndLot") + " " + Global.LotInitialBatchNo, ButtonResult.No, ButtonResult.Yes);

                    if (dialogResult == ButtonResult.Yes)
                    {
                        Global.AccumulateCurrentBatchQuantity = Global.LotInitialTotalBatchQuantity = 0;
                        m_EventAggregator.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.EndLotComp });
                        m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = "Endlot" + Global.CurrentBatchNum });
                        m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Idle);
                        ResetCounter();

                    }
                    else if (dialogResult == ButtonResult.No)
                    {
                        Reset();
                        m_EventAggregator.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcContErrRtn });
                    }
                }
                else
                {
                    m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Error);
                    RaiseEndLotPopup();
                }
                CloseDialog("");
            }
        }

        private void ResetCounter()
        {
            #region Code Reader
            Global.CurrentContainerNum = String.Empty;
            Global.CurrentBatchQuantity = 0;
            Global.AccumulateCurrentBatchQuantity = 0;
            Global.CurrentBoxQuantity = 0;
            Global.CurrentBatchNum = String.Empty;
            Global.CurrentLotBatchNum = String.Empty;
            #endregion

            #region Top Vision
            Global.VisProductQuantity = 0f;
            Global.VisProductCrtOrientation =0f;
            Global.VisProductWrgOrientation = 0f;
            Global.TopVisionEndLot = true;
            Global.CodeReaderEndLot = true;
            m_EventAggregator.GetEvent<TopVisionResultEvent>().Publish();
            m_EventAggregator.GetEvent<OnCodeReaderEndResultEvent>().Publish();
            #endregion
        }

        private void OnReconnectTCP(TCPDisplay tcp)
        {
            m_TCPIP.clientSockets[tcp.ID].Disconnect();
            m_TCPIP.clientSockets[tcp.ID].Reconnect();
            if(!m_TCPIP.clientSockets[tcp.ID].IsAlive)
                m_ShowDialog.Show(DialogIcon.Error, m_TCPIP.clientSockets[tcp.ID].Name + " " + GetDialogTableValue("FailReconnect"));

        }
        void RaiseEndLotPopup()
        {
            m_DialogService.ShowDialog(DialogList.ForcedEndLotView.ToString(),
                                      new DialogParameters($"message={""}"),
                                      null);
        }

        protected virtual void CloseDialog(string parameter)
        {
            //m_TmrButtonMonitor.Stop();
            // Turn off Reset Button LED
            //m_IO.WriteBit(ResetButtonIndic, false);
            RaiseRequestClose(new DialogResult(ButtonResult.OK));
        }
        #endregion

        #region Properties
        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }
        private void OnReconnectAllTCP()
        {
            foreach(TCPDisplay tcpip in TCPCollection)
            {
                if (!m_TCPIP.clientSockets[tcpip.ID].IsAlive)
                {
                    m_TCPIP.clientSockets[tcpip.ID].Disconnect();
                    m_TCPIP.clientSockets[tcpip.ID].Reconnect();
                }
            }

            if (!m_TCPIP.clientSockets.All(x => x.IsAlive))
                m_ShowDialog.Show(DialogIcon.Error, GetDialogTableValue("FailReconnectSomeTCP"));
        }

        private void StartOperation()
        {
            IsAllowStart = false;
            m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Running);
        }
        #endregion
    }
}
