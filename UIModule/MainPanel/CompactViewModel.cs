using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using UIModule.StandardViews.Services;
using ApplicationCommands = GreatechApp.Core.Command.ApplicationCommands;

namespace UIModule.MainPanel
{
    public class CompactViewModel : BaseUIViewModel
    {
        private Thread EStopWinThread;
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
        #endregion

        #region User Info
        private string m_UserInfo;
        public string UserInfo
        {
            get { return m_UserInfo; }
            set { SetProperty(ref m_UserInfo, value); }
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
        #endregion

        #region SSR Guarding
        private bool m_IsAllowInit;
        public bool IsAllowInit
        {
            get { return m_IsAllowInit; }
            set { SetProperty(ref m_IsAllowInit, value); }
        }

        private bool m_IsAllowStart;
        public bool IsAllowStart
        {
            get { return m_IsAllowStart; }
            set
            {
                SetProperty(ref m_IsAllowStart, value);
            }
        }

        private bool m_IsAllowStop;
        public bool IsAllowStop
        {
            get { return m_IsAllowStop; }
            set
            {
                SetProperty(ref m_IsAllowStop, value);
            }
        }

        private bool m_IsAllowReset;
        public bool IsAllowReset
        {
            get { return m_IsAllowReset; }
            set { SetProperty(ref m_IsAllowReset, value); }
        }

        private bool m_IsEndingLot = false;
        public bool IsEndingLot
        {
            get { return m_IsEndingLot; }
            set { SetProperty(ref m_IsEndingLot, value); }
        }
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

        #region Equipment State
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

        private string m_CycleTime;
        public string CycleTime
        {
            get { return m_CycleTime; }
            set { SetProperty(ref m_CycleTime, value); }
        }

        private string m_UPH;
        public string UPH
        {
            get { return m_UPH; }
            set { SetProperty(ref m_UPH, value); }
        }

        private string m_TotalInput;
        public string TotalInput
        {
            get { return m_TotalInput; }
            set { SetProperty(ref m_TotalInput, value); }
        }

        private string m_TotalOutput;
        public string TotalOutput
        {
            get { return m_TotalOutput; }
            set { SetProperty(ref m_TotalOutput, value); }
        }

        private double m_Availability;
        public double Availability
        {
            get { return m_Availability; }
            set { SetProperty(ref m_Availability, value); }
        }

        private double m_Performance;
        public double Performance
        {
            get { return m_Performance; }
            set { SetProperty(ref m_Performance, value); }
        }

        private double m_Quality;
        public double Quality
        {
            get { return m_Quality; }
            set { SetProperty(ref m_Quality, value); }
        }

        private double m_OEE;
        public double OEE
        {
            get { return m_OEE; }
            set { SetProperty(ref m_OEE, value); }
        }

        private bool m_CanAccess;
        public bool CanAccess
        {
            get { return m_CanAccess; }
            set { SetProperty(ref m_CanAccess, value); }
        }
        #endregion

        #region Performance
        private string m_Throughput;
        public string Throughput
        {
            get { return m_Throughput; }
            set { SetProperty(ref m_Throughput, value); }
        }
        private string m_StopPages;
        public string StopPages
        {
            get { return m_StopPages; }
            set { SetProperty(ref m_StopPages, value); }
        }
        private string m_ProdStartTime;
        public string ProdStartTime
        {
            get { return m_ProdStartTime; }
            set { SetProperty(ref m_ProdStartTime, value); }
        }
        private string m_ProdElapsedTime;
        public string ProdElapsedTime
        {
            get { return m_ProdElapsedTime; }
            set { SetProperty(ref m_ProdElapsedTime, value); }
        }
        private string m_DownTime;
        public string DownTime
        {
            get { return m_DownTime; }
            set { SetProperty(ref m_DownTime, value); }
        }
        private string m_LotStartTime;
        public string LotStartTime
        {
            get { return m_LotStartTime; }
            set { SetProperty(ref m_LotStartTime, value); }
        }
        private string m_LotElapsedTime;
        public string LotElapsedTime
        {
            get { return m_LotElapsedTime; }
            set { SetProperty(ref m_LotElapsedTime, value); }
        }
        private string m_LotFinishTime;
        public string LotFinishTime
        {
            get { return m_LotFinishTime; }
            set { SetProperty(ref m_LotFinishTime, value); }
        }
        private string m_OverallYield;
        public string OverallYield
        {
            get { return m_OverallYield; }
            set { SetProperty(ref m_OverallYield, value); }
        }
        private string m_MTBA;
        public string MTBA
        {
            get { return m_MTBA; }
            set { SetProperty(ref m_MTBA, value); }
        }
        private string m_MTTA;
        public string MTTA
        {
            get { return m_MTTA; }
            set { SetProperty(ref m_MTTA, value); }
        }
        private string m_MTTR;
        public string MTTR
        {
            get { return m_MTTR; }
            set { SetProperty(ref m_MTTR, value); }
        }
        private string m_MTBF;
        public string MTBF
        {
            get { return m_MTBF; }
            set { SetProperty(ref m_MTBF, value); }
        }
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

        private string m_AlarmMsg;
        public string AlarmMsg
        {
            get { return m_AlarmMsg; }
            set { SetProperty(ref m_AlarmMsg, value); }
        }

        public DelegateCommand<string> InitCommand { get; set; }
        public DelegateCommand<string> OperationCommand { get; set; }
        public DelegateCommand<string> TCPIPListCommand { get; set; }
        public DelegateCommand LoginDialogCommand { get; set; }
        public DelegateCommand<TCPDisplay> ReconnectTCP { get; set; }
        public DelegateCommand ReconnectAllTCP { get; set; }

        private readonly IDialogService m_DialogService;
        private static object m_SyncOEE = new object();
        private DispatcherTimer tmrOEEUpdate;

        public CompactViewModel(IDialogService dialogService)
        {
            m_DialogService = dialogService;

            m_EventAggregator.GetEvent<MachineState>().Subscribe(OnMachineStateChange);
            m_EventAggregator.GetEvent<PerformanceEntity>().Subscribe(OnPerformanceChange);
            m_EventAggregator.GetEvent<PerformanceCompact>().Subscribe(OnPerformanceCompactChange);
            m_EventAggregator.GetEvent<DatalogEntity>().Subscribe(OnDatalogEntity, filter => filter.MsgType == LogMsgType.Error || filter.MsgType == LogMsgType.Warning);


            OperationCommand = new DelegateCommand<string>(OperationMethod);
            LoginDialogCommand = new DelegateCommand(RaiseLoginPopup);
            TCPIPListCommand = new DelegateCommand<string>(OnTCPIPList);
            ReconnectTCP = new DelegateCommand<TCPDisplay>(OnReconnectTCP);
            ReconnectAllTCP = new DelegateCommand(OnReconnectAllTCP);

            ApplicationCommands.OperationCommand.RegisterCommand(OperationCommand);

            IsLogin = Visibility.Visible;
            IsLogout = Visibility.Collapsed;
            LoginStatus = "Login";

            // System Clock
            tmrSysClock = new DispatcherTimer();
            tmrSysClock.Interval = new TimeSpan(0, 0, 0, 1);
            tmrSysClock.Tick += new EventHandler(tmrSysClock_Tick);
            tmrSysClock.Start();

            IsTCPIPListOpen = false;
            DisableAllBtn();

            EquipStateIcon = GrayIcon;
            EquipStatus = "Idle";

            UPH = "0";
            CycleTime = "0";
            TotalInput = "0";
            TotalOutput = "0";
            Throughput = "0";

            OnMachineStateChange(Global.MachineStatus);
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
            IsAllowInit = false;
            IsAllowStart = false;
            IsAllowStop = false;
            IsAllowReset = false;
        }

        public void ProductionMode()
        {
            IsAllowInit = false;
            IsAllowStart = false;
            IsAllowStop = true;
            IsAllowReset = false;
        }

        public void ErrorMode()
        {
            IsAllowInit = false;
            IsAllowStop = false;
            IsAllowReset = true;
        }
        public void RecoveryMode()
        {
            IsAllowStart = true;
            IsAllowInit = true; ;
            IsAllowStop = false;
            IsAllowReset = false;
        }

        public void StopMode()
        {
            IsAllowInit = true;
            IsAllowStart = true;
            IsAllowStop = false;
            IsAllowReset = false;
        }

        public void IdleMode()
        {

            IsAllowInit = true;
            IsAllowStart = false;
            IsAllowStop = false;
            IsAllowReset = false;      
        }

        public void ReadyMode()
        {
            IsAllowInit = true;
            IsAllowStart = true;
            IsAllowStop = false;
            IsAllowReset = false;

            CycleTime = "0";
            UPH = "0";
        }
        #endregion

        #region Event

        private void m_tmrTCPMonitor_Tick(object sender, EventArgs e)
        {
            try
            {
                List<string> tooltip = new List<string>();
                foreach (TCPDisplay tcpip in TCPCollection)
                {
                    tcpip.IsConnected = m_TCPIP.clientSockets[tcpip.ID].IsAlive;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    TCPIPStateIcon = TCPCollection.All(x => x.IsConnected) ? Connected : Disconnected;
                });
                TCPIPStatus = TCPCollection.All(x => x.IsConnected) ? "Connected" : "Disconnected";
            }
            catch (Exception ex)
            {

            }
        }

        private void OnMachineStateChange(MachineStateType state)
        {
            switch (state)
            {
                case MachineStateType.Running:
                    ProductionMode();
                    EquipStateIcon = GreenIcon;

                    if (IsEndingLot)
                        EquipStatus = "LotEnding";
                    else
                        EquipStatus = "Running";
                    Global.MachineStatus = MachineStateType.Running;
                    break;
                    
                case MachineStateType.Error:
                    Global.MachineStatus = MachineStateType.Error;
                    ErrorMode();
                    EquipStateIcon = RedIcon;
                    EquipStatus = "Error";
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
            Dispatcher.Run();
        }

        private void OnPerformanceChange(PerformanceEntity performanceEntity)
        {
            CycleTime = performanceEntity.CycleTime.ToString();
            UPH = performanceEntity.UPH.ToString();
        }

        private void OnPerformanceCompactChange(PerformanceCompact performanceEntity)
        {
            Throughput = performanceEntity.Throughput;
            StopPages = performanceEntity.StopPages;
            ProdStartTime = performanceEntity.StartTime;
            ProdElapsedTime = performanceEntity.ElapsedTime;
            DownTime = performanceEntity.DownTime;
            LotStartTime = performanceEntity.LotStartTime;
            LotElapsedTime = performanceEntity.LotElapsedTime;
            LotFinishTime = performanceEntity.LotFinishTime;
            OverallYield = performanceEntity.OverallYield;
            MTBA = performanceEntity.MTBA;
            MTTA = performanceEntity.MTTA;
            MTTR = performanceEntity.MTTR;
            MTBF = performanceEntity.MTBF;
        }

        public override void OnCultureChanged()
        {
            EquipStatusWithCulture = GetStringTableValue(EquipStatus);
            TCPIPStatusWithCulture = GetStringTableValue(TCPIPStatus);
            LoginStatusWithCulture = GetStringTableValue(LoginStatus);
        }

        private void OnDatalogEntity(DatalogEntity log)
        {
            DateTime currentTime = DateTime.Now;
            DateTimeFormatInfo dateFormat = new DateTimeFormatInfo();
            dateFormat.ShortDatePattern = "dd-MM-yyyy";
            string date = currentTime.ToString("d", dateFormat);
            string time = currentTime.ToString("HH:mm:ss.fff", DateTimeFormatInfo.InvariantInfo);
            AlarmMsg = date + " " + time + "   \t|  " + log.MsgText;
        }
        #endregion

        #region Method
        void RaiseLoginPopup()
        {
            m_DialogService.ShowDialog(DialogList.LoginView.ToString(),
                                      new DialogParameters($"message={""}"),
                                      null);
        }

        void OnTCPIPList(string command)
        {
            if (command == "Open")
            {
                if (!IsTCPIPListOpen)
                    IsTCPIPListOpen = true;
            }
            else if (command == "Close")
            {
                IsTCPIPListOpen = false;
            }
        }

        private void OperationMethod(string Command)
        {
            if (Command == "Logout")
            {
                DisableAllBtn();
                IsTCPIPListOpen = false;
                LoginStatus = "Login";
                IsLogin = Visibility.Visible;
                IsLogout = Visibility.Collapsed;
                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("User")} {m_CurrentUser.Username} {GetStringTableValue("LoggedOut")}." });
                m_AuthService.CurrentUser.IsAuthenticated = false;
                m_EventAggregator.GetEvent<ValidateLogin>().Publish(false);
            }
        }

        private void OnReconnectTCP(TCPDisplay tcp)
        {
            m_TCPIP.clientSockets[tcp.ID].Disconnect();
            m_TCPIP.clientSockets[tcp.ID].Reconnect();
            if (!m_TCPIP.clientSockets[tcp.ID].IsAlive)
                m_ShowDialog.Show(DialogIcon.Error, m_TCPIP.clientSockets[tcp.ID].Name + " " + GetDialogTableValue("FailReconnect"));

        }

        private void OnReconnectAllTCP()
        {
            foreach (TCPDisplay tcpip in TCPCollection)
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
        #endregion
    }
}
