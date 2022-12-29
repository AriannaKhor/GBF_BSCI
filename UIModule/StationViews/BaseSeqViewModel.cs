//#define ACSMotion

using ConfigManager;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Resources;
using GreatechApp.Core.Variable;
using GreatechApp.Services.UserServices;
using GreatechApp.Services.Utilities;
using InterlockManager.IO;
using IOManager;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Sequence;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Timers;
using System.Windows;
using System.Windows.Media.Imaging;

namespace UIModule.StationViews
{
    public class BaseSeqViewModel : BindableBase, INavigationAware, IAccessService, IActiveAware
    {
        #region Variables
        private AuthService m_AuthService;

        private string m_Title;
        public string Title
        {
            get { return m_Title; }
            set { SetProperty(ref m_Title, value); }
        }
        private Visibility m_ImageVisibility = Visibility.Collapsed;
        public Visibility ImageVisibility
        {
            get { return m_ImageVisibility; }
            set { SetProperty(ref m_ImageVisibility, value); }
        }

        public SystemConfig m_SystemConfig;
        public CultureResources m_CultureResources;
        //public IBaseMotion m_Motion;
        public IShowDialog m_ShowDialog;
        public IBaseIO m_IIO;
        public IDelegateSeq m_DelegateSeq;
        public IEventAggregator m_EventAggregator;

        //private IEnumerable<IMtrInterlock> m_MtrIntLCollection;
        //private IMtrInterlock m_MtrIntL;
        private IEnumerable<IIOInterlock> m_IOIntLCollection;
        private IIOInterlock m_IOIntL;
        protected SQID CurrentSeq;

        private Type[] m_MCTypes;

        public DelegateCommand<string> ImageCommand { get; set; }

        private Visibility m_PresetVis = Visibility.Collapsed;
        public Visibility PresetVis
        {
            get { return m_PresetVis; }
            set { SetProperty(ref m_PresetVis, value); }
        }

        #region Motion
        //public MotionConfig mtrcfg { get; set; }
        public Timer tmr_UpdateMotionStatus;
        public int SelectedTabIndex { get; set; }
        //public DelegateCommand<TeachingPosition> GoCommand { get; private set; }

        //private ObservableCollection<MotorTabList> m_MotorList;
        //public ObservableCollection<MotorTabList> MotorList
        //{
        //    get { return m_MotorList; }
        //    set { SetProperty(ref m_MotorList, value); }
        //}
        private BitmapImage m_Alarm = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));
        public BitmapImage Alarm
        {
            get { return m_Alarm; }
            set { SetProperty(ref m_Alarm, value); }
        }

        private BitmapImage m_InPos = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));
        public BitmapImage InPos
        {
            get { return m_InPos; }
            set { SetProperty(ref m_InPos, value); }
        }

        private BitmapImage m_Ready = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));
        public BitmapImage Ready
        {
            get { return m_Ready; }
            set { SetProperty(ref m_Ready, value); }
        }

        private BitmapImage m_AxisHome = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));
        public BitmapImage AxisHome
        {
            get { return m_AxisHome; }
            set { SetProperty(ref m_AxisHome, value); }
        }

        private bool m_ServoOn;
        public bool ServoOn
        {
            get { return m_ServoOn; }
            set { SetProperty(ref m_ServoOn, value); }
        }

        private bool m_IsHome;
        public bool IsHome
        {
            get { return m_IsHome; }
            set { SetProperty(ref m_IsHome, value); }
        }

        private Visibility m_IsReadyVisible;
        public Visibility IsReadyVisible
        {
            get { return m_IsReadyVisible; }
            set { SetProperty(ref m_IsReadyVisible, value); }
        }

        private Visibility m_IsAlarmVisible;
        public Visibility IsAlarmVisible
        {
            get { return m_IsAlarmVisible; }
            set { SetProperty(ref m_IsAlarmVisible, value); }
        }

        private Visibility m_IsInPosVisible;
        public Visibility IsInPosVisible
        {
            get { return m_IsInPosVisible; }
            set { SetProperty(ref m_IsInPosVisible, value); }
        }
        private double m_CurrentPos = 0;
        public double CurrentPos
        {
            get { return m_CurrentPos; }
            set { SetProperty(ref m_CurrentPos, value); }
        }

        private string m_UoM;
        public string UoM
        {
            get { return m_UoM; }
            set { SetProperty(ref m_UoM, value); }
        }

        private Visibility m_MotorVis = Visibility.Collapsed;
        public Visibility MotorVis
        {
            get { return m_MotorVis; }
            set { SetProperty(ref m_MotorVis, value); }
        }
        #endregion

        #region IO
        public Timer tmr_UpdateIOStatus;
        //private List<DictionaryEntry> m_InputResources { get; set; }
        //private List<DictionaryEntry> m_OutputResources { get; set; }

        //private ObservableCollection<CylinderIOParameters> m_CylinderList;
        //public ObservableCollection<CylinderIOParameters> CylinderList
        //{
        //    get { return m_CylinderList; }
        //    set { SetProperty(ref m_CylinderList, value); }
        //}

        //private ObservableCollection<VacuumIOParameters> m_VacuumList;
        //public ObservableCollection<VacuumIOParameters> VacuumList
        //{
        //    get { return m_VacuumList; }
        //    set { SetProperty(ref m_VacuumList, value); }
        //}

        private ObservableCollection<IOList> m_InputList;
        public ObservableCollection<IOList> InputList
        {
            get { return m_InputList; }
            set { SetProperty(ref m_InputList, value); }
        }

        private ObservableCollection<IOList> m_OutputList;
        public ObservableCollection<IOList> OutputList
        {
            get { return m_OutputList; }
            set { SetProperty(ref m_OutputList, value); }
        }

        private Visibility m_VacuumVis = Visibility.Collapsed;
        public Visibility VacuumVis
        {
            get { return m_VacuumVis; }
            set { SetProperty(ref m_VacuumVis, value); }
        }

        private Visibility m_CylinderVis = Visibility.Collapsed;
        public Visibility CylinderVis
        {
            get { return m_CylinderVis; }
            set { SetProperty(ref m_CylinderVis, value); }
        }

        private Visibility m_GInputVis = Visibility.Collapsed;
        public Visibility GInputVis
        {
            get { return m_GInputVis; }
            set { SetProperty(ref m_GInputVis, value); }
        }

        private Visibility m_GOutputVis = Visibility.Collapsed;
        public Visibility GOutputVis
        {
            get { return m_GOutputVis; }
            set { SetProperty(ref m_GOutputVis, value); }
        }

        private Visibility m_IOVis = Visibility.Collapsed;
        public Visibility IOVis
        {
            get { return m_IOVis; }
            set { SetProperty(ref m_IOVis, value); }
        }

        //public DelegateCommand<VacuumIOParameters> VacuumCommand { get; private set; }
        //public DelegateCommand<VacuumIOParameters> PurgeCommand { get; private set; }
        public DelegateCommand<IOList> IOCommand { get; private set; }
        #endregion

        #region Test Run
        public DelegateCommand StartCommand { get; set; }
        public DelegateCommand StopCommand { get; set; }
        public Stopwatch m_StopWatch { get; set; }
        private bool m_IsTesting;

        public bool IsTesting
        {
            get { return m_IsTesting; }
            set { SetProperty(ref m_IsTesting, value); }
        }

        private string m_TestTime;

        public string TestTime
        {
            get { return m_TestTime; }
            set { SetProperty(ref m_TestTime, value); }
        }

        private int m_CycleCount = 1;

        public int CycleCount
        {
            get { return m_CycleCount; }
            set { SetProperty(ref m_CycleCount, value); }
        }

        private ObservableCollection<TestRun> m_TestSeqNumList;

        public ObservableCollection<TestRun> TestSeqNumList
        {
            get { return m_TestSeqNumList; }
            set { SetProperty(ref m_TestSeqNumList, value); }
        }

        //private TestRun m_SelectedTestSeqNum;
        //public TestRun SelectedTestSeqNum
        //{
        //    get { return m_SelectedTestSeqNum; }
        //    set
        //    {
        //        SetProperty(ref m_SelectedTestSeqNum, value);
        //        if (value != null)
        //        {
        //            CanMultipleCycle = TestSeqNumList.Where(x => x.SeqNum == m_SelectedTestSeqNum.SeqNum).Select(x => x.IsMultipleCycle).FirstOrDefault();

        //            if (value.SeqNum == TestRunEnum.SN.HomeMotor)
        //                IsHomeMotorSelected = true;
        //            else
        //                IsHomeMotorSelected = false;
        //        }
        //    }
        //}

        //private MotorTabList m_SelectedAxis;

        //public MotorTabList SelectedAxis
        //{
        //    get { return m_SelectedAxis; }
        //    set { SetProperty(ref m_SelectedAxis, value); }
        //}

        //private bool m_IsHomeMotorSelected;
        //public bool IsHomeMotorSelected
        //{
        //    get { return m_IsHomeMotorSelected; }
        //    set
        //    {
        //        SetProperty(ref m_IsHomeMotorSelected, value);

        //        if (value && MotorList != null && MotorList.Count > 0)
        //        {
        //            SelectedAxis = MotorList[0];
        //        }
        //        else
        //        {
        //            SelectedAxis = null;
        //        }
        //    }
        //}

        private bool m_CanMultipleCycle;
        public bool CanMultipleCycle
        {
            get { return m_CanMultipleCycle; }
            set { SetProperty(ref m_CanMultipleCycle, value); }
        }

        private Visibility m_TestRunVis = Visibility.Collapsed;
        public Visibility TestRunVis
        {
            get { return m_TestRunVis; }
            set { SetProperty(ref m_TestRunVis, value); }
        }
        #endregion
        #endregion

        #region Constructor
        public BaseSeqViewModel()
        {
            m_SystemConfig = (SystemConfig)ContainerLocator.Container.Resolve(typeof(SystemConfig));
            m_CultureResources = (CultureResources)ContainerLocator.Container.Resolve(typeof(CultureResources));
            //m_Motion = (IBaseMotion)ContainerLocator.Container.Resolve(typeof(IBaseMotion));
            m_IIO = (IBaseIO)ContainerLocator.Container.Resolve(typeof(IBaseIO));
            m_DelegateSeq = (IDelegateSeq)ContainerLocator.Container.Resolve(typeof(IDelegateSeq));
            m_EventAggregator = (IEventAggregator)ContainerLocator.Container.Resolve(typeof(IEventAggregator));
            m_AuthService = (AuthService)ContainerLocator.Container.Resolve(typeof(AuthService));
            m_ShowDialog = (IShowDialog)ContainerLocator.Container.Resolve(typeof(IShowDialog));

            //m_MtrIntLCollection = ContainerLocator.Container.Resolve<Func<IEnumerable<IMtrInterlock>>>()();
            //m_MtrIntL = m_MtrIntLCollection.FirstOrDefault();

            m_IOIntLCollection = ContainerLocator.Container.Resolve<Func<IEnumerable<IIOInterlock>>>()();
            m_IOIntL = m_IOIntLCollection.FirstOrDefault();

            ImageCommand = new DelegateCommand<string>(OnImageVisibility);

            m_EventAggregator.GetEvent<MachineState>().Subscribe(OnMachineState);
            m_EventAggregator.GetEvent<ValidateLogin>().Subscribe(OnValidateLogin);

            RaisePropertyChanged(nameof(CanAccess));

            //m_MCTypes = m_Motion.GetType().GetInterfaces();

            //MotorList = new ObservableCollection<MotorTabList>();

            TestTime = "--" + GetStringTableValue("s");
        }
        #endregion

        #region Properties

        public event EventHandler IsActiveChanged;

        private bool _IsActive = false;
        public bool IsActive
        {
            get
            {
                return _IsActive;
            }
            set
            {
                _IsActive = value;
                if (value)
                    OnActivate();
                else
                    OnDeactivate();
            }
        }

        public virtual bool KeepAlive
        {
            get
            {
                return false;
            }
        }

        private void OnActivate()
        {
            ////LoadConfig();
            //m_EventAggregator.GetEvent<TestRunResult>().Subscribe(OnTestRunComplete);
            //if (MotorList.Count > 0) tmr_UpdateMotionStatus.Start();
            //tmr_UpdateIOStatus.Start();
            //if (Global.InitDone && (Global.MachineStatus == MachineStateType.Lot_Ended || Global.MachineStatus == MachineStateType.Ready || Global.MachineStatus == MachineStateType.Init_Done))
            //{
            //    m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.ReInit);
            //}
        }

        private void OnDeactivate()
        {
            m_EventAggregator.GetEvent<TestRunResult>().Unsubscribe(OnTestRunComplete);
            tmr_UpdateMotionStatus.Stop();

            tmr_UpdateIOStatus.Stop();
        }

        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {

        }
        #endregion

        #region Event
        private void OnMachineState(MachineStateType status)
        {
            Global.MachineStatus = status;
            RaisePropertyChanged(nameof(CanAccess));
        }

        private void OnValidateLogin(bool IsAuthenticated)
        {
            RaisePropertyChanged(nameof(CanAccess));
        }

        #region Motion
//        private void UpdateMotionStatus(object sender, EventArgs e)
//        {
//            try
//            {
//                MotorTabList motor = MotorList[SelectedTabIndex];

//                IsReadyVisible = motor.IsChkReady ? Visibility.Visible : Visibility.Collapsed;
//                IsAlarmVisible = motor.IsChkAlarm ? Visibility.Visible : Visibility.Collapsed;
//                IsInPosVisible = motor.IsChkInPos ? Visibility.Visible : Visibility.Collapsed;
//                IsHome = mtrcfg.Axis.IsHome;

//#if !SIMULATION || ACSMotion

//                Application.Current.Dispatcher.Invoke(() =>
//                {
//                    Alarm = m_Motion.GetAlarmStatus(motor.CardID, motor.AxisID) ?
//                    new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GreenIcon.png", uriKind: System.UriKind.RelativeOrAbsolute)) :
//                    new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));

//                    InPos = m_Motion.GetMotionDoneStatus(motor.CardID, motor.AxisID) ?
//                    new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GreenIcon.png", uriKind: System.UriKind.RelativeOrAbsolute)) :
//                    new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));

//                    Ready = m_Motion.GetServoStatus(motor.CardID, motor.AxisID) && m_Motion.GetMotionDoneStatus(motor.CardID, motor.AxisID) ?
//                    new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GreenIcon.png", uriKind: System.UriKind.RelativeOrAbsolute)) :
//                    new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));

//                    ServoOn = m_Motion.GetServoStatus(motor.CardID, motor.AxisID);

//                    AxisHome = IsAxisHome(motor.IsChkAxisHome) ?
//                                 new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GreenIcon.png", uriKind: System.UriKind.RelativeOrAbsolute)) :
//                                 new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));

//                    double CurPulse = EnumHelper.GetValueFromDescription<MotorType>(motor.Type) == MotAxis.MotorType.Servo ? m_Motion.GetEncoderPosition(motor.CardID, motor.AxisID) : m_Motion.GetLogCnt(motor.CardID, motor.AxisID);

//                    if (m_MCTypes[1].Name == "IACSMotion")
//                    {
//                        CurrentPos = Math.Round(CurPulse, 4);
//                    }
//                    else if (m_MCTypes[1].Name == "IAdvantechMotion")
//                    {
//                        CurrentPos = EnumHelper.GetValueFromDescription<DriveMethod>(motor.System) == MotAxis.DriveMethod.Linear ? Math.Round(m_Motion.Pulse2mm(motor.Revolution, motor.Pitch, (float)CurPulse), 4) : Math.Round(m_Motion.Pulse2degree(motor.Revolution, (float)CurPulse), 4);
//                    }
//                    else
//                    {
//                        CurrentPos = EnumHelper.GetValueFromDescription<DriveMethod>(motor.System) == MotAxis.DriveMethod.Linear ? Math.Round(m_Motion.Pulse2mm(motor.Revolution, motor.Pitch, (int)CurPulse), 4) : Math.Round(m_Motion.Pulse2degree(motor.Revolution, (float)CurPulse), 4);
//                    }
//                });
//#endif
//                UoM = motor.UoM;
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(ex.Message, "Station View Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }
        #endregion

        #region IO
        private void UpdateIOList(object sender, ElapsedEventArgs e)
        {
            try
            {
                //foreach (CylinderIOParameters cylinder in CylinderList)
                //{
                //    if (cylinder.RestSns1 != null)
                //        cylinder.IsRestSns1 = m_IIO.ReadBit((int)cylinder.RestSns1);
                //    if (cylinder.WorkSns1 != null)
                //        cylinder.IsWorkSns1 = m_IIO.ReadBit((int)cylinder.WorkSns1);
                //    if (cylinder.RestSns2 != null)
                //        cylinder.IsRestSns2 = m_IIO.ReadBit((int)cylinder.RestSns2);
                //    if (cylinder.WorkSns2 != null)
                //        cylinder.IsWorkSns2 = m_IIO.ReadBit((int)cylinder.WorkSns2);
                //    Application.Current.Dispatcher.Invoke(() =>
                //    {
                //        cylinder.IsCylinderWork = m_IIO.ReadOutBit((int)cylinder.Work);
                //    });
                //}

                //foreach (VacuumIOParameters vacuum in VacuumList)
                //{
                //    Application.Current.Dispatcher.Invoke(() =>
                //    {
                //        vacuum.VacuumImage = m_IIO.ReadOutBit((int)vacuum.Vacuum) ?
                //            new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/R_ON.png", uriKind: UriKind.RelativeOrAbsolute)) :
                //            new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/R_OFF.png", uriKind: UriKind.RelativeOrAbsolute));

                //        if (vacuum.IsPurgeAvail)
                //        {
                //            vacuum.PurgeImage = m_IIO.ReadOutBit((int)vacuum.Purge) ?
                //            new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/R_ON.png", uriKind: UriKind.RelativeOrAbsolute)) :
                //            new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/R_OFF.png", uriKind: UriKind.RelativeOrAbsolute));
                //            vacuum.IsPurgeOn = m_IIO.ReadOutBit((int)vacuum.Purge);

                //        }

                //        vacuum.IsVacuumOn = m_IIO.ReadOutBit((int)vacuum.Vacuum);
                //    });


                //    if (vacuum.PickedUpSns != null)
                //    {
                //        Application.Current.Dispatcher.Invoke(() =>
                //        {
                //            vacuum.PickedUpImage = m_IIO.ReadBit((int)vacuum.PickedUpSns) ?
                //            new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GreenIcon.png", uriKind: UriKind.RelativeOrAbsolute)) :
                //            new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/G_OFF.png", uriKind: UriKind.RelativeOrAbsolute));
                //        });
                //    }
                //    if (vacuum.VacuumSns != null)
                //    {
                //        Application.Current.Dispatcher.Invoke(() =>
                //        {
                //            vacuum.VacOnImage = m_IIO.ReadBit((int)vacuum.VacuumSns) ?
                //            new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GreenIcon.png", uriKind: UriKind.RelativeOrAbsolute)) :
                //            new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/G_OFF.png", uriKind: UriKind.RelativeOrAbsolute));
                //        });
                //    }
                //}

                foreach (IOList io in InputList)
                {
                    io.Status = m_IIO.ReadBit(io.Tag);
                }

                foreach (IOList io in OutputList)
                {
                    io.Status = m_IIO.ReadOutBit(io.Tag);
                }
            }
            catch (Exception ex)
            {
                tmr_UpdateIOStatus.Stop();
                MessageBox.Show(ex.Message, "IO Error");
            }
        }
        #endregion

        #region Test Run
        private void OnTestRunComplete(TestRunResult obj)
        {
            IsTesting = false;
            m_StopWatch.Stop();
            if (obj.result)
            {
                TimeSpan timeTaken = m_StopWatch.Elapsed;
                TestTime = $"{timeTaken.TotalSeconds:N2}" + GetStringTableValue("s");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    m_ShowDialog.Show(DialogIcon.Complete, GetDialogTableValue("TestRunCompleted"));

                });
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    m_ShowDialog.Show(DialogIcon.Error, GetDialogTableValue("TestRunError") + obj.ErrMsg);

                });
                TestTime = GetStringTableValue("Error");
            }
        }
        #endregion
        #endregion

        #region Method
        //private bool IsAxisHome(bool chkAxisHome)
        //{
        //    MotionConfig mtrcfg = MotionConfig.Open(MotorList[SelectedTabIndex].MotReference);

        //    if (!chkAxisHome)
        //    {
        //        for (int i = 0; i < MotorList[SelectedTabIndex].Positions.Count; i++)
        //        {
        //            MotorList[SelectedTabIndex].Positions[i].IsAllowMoveMtr = true;
        //        }
        //        return true;
        //    }

        //    for (int i = 0; i < MotorList[SelectedTabIndex].Positions.Count; i++)
        //    {
        //        MotorList[SelectedTabIndex].Positions[i].IsAllowMoveMtr = mtrcfg.Axis.IsHome;
        //    }

        //    return mtrcfg.Axis.IsHome;
        //}

        public void Init(string title, SQID sqid)
        {
            Title = title;
            CurrentSeq = sqid;

            Init();

            GetConfig(sqid);
        }

        public void Init()
        {
            #region Motion
            //LoadConfig();

            //tmr_UpdateMotionStatus = new Timer();
            //tmr_UpdateMotionStatus.Interval = 100;
            //tmr_UpdateMotionStatus.Elapsed += UpdateMotionStatus;
            //GoCommand = new DelegateCommand<TeachingPosition>(GoMethod);
            #endregion

            #region IO
            //CylinderList = new ObservableCollection<CylinderIOParameters>();
            //VacuumList = new ObservableCollection<VacuumIOParameters>();

            //VacuumCommand = new DelegateCommand<VacuumIOParameters>(VacuumMethod);
            //PurgeCommand = new DelegateCommand<VacuumIOParameters>(PurgeMethod);
            //IOCommand = new DelegateCommand<IOList>(IOMethod);

            //List<object> CylinderVacuumInputList = new List<object>();
            //List<object> CylinderVacuumOutputList = new List<object>();

            //var ioMappingList = m_IIO.VacuumCylinderList.Where(x => x.SeqName == CurrentSeq).ToList();
            //foreach (VacuumCylinderIO item in ioMappingList)
            //{
            //    // If Cylinder name has value, add into cylinder list
            //    if (!String.IsNullOrEmpty(item.CylinderName) && String.IsNullOrEmpty(item.VacuumName))
            //    {
            //        CylinderList.Add(new CylinderIOParameters()
            //        {
            //            CylinderName = item.CylinderName,
            //            WorkSns1 = item.WorkSns1,
            //            WorkSns2 = item.WorkSns2,
            //            RestSns1 = item.RestSns1,
            //            RestSns2 = item.RestSns2,
            //            Work = (OUT)item.Work,
            //            Rest = item.Rest,
            //            IsCylinderWork = m_IIO.ReadOutBit((int)item.Work),
            //            IsWorkSns1 = GetBoolResult(item.WorkSns1),
            //            IsWorkSns2 = GetBoolResult(item.WorkSns2),
            //            IsRestSns1 = GetBoolResult(item.RestSns1),
            //            IsRestSns2 = GetBoolResult(item.RestSns2),
            //            WorkCommand = new DelegateCommand<CylinderIOParameters>(WorkMethod),
            //            RestCommand = new DelegateCommand<CylinderIOParameters>(RestMethod),
            //        });

            //        // add input into general input except list
            //        CylinderVacuumInputList.Add(item.WorkSns1);
            //        CylinderVacuumInputList.Add(item.WorkSns2);
            //        CylinderVacuumInputList.Add(item.RestSns1);
            //        CylinderVacuumInputList.Add(item.RestSns2);

            //        // add output into general output except list
            //        CylinderVacuumOutputList.Add(item.Work);
            //        CylinderVacuumOutputList.Add(item.Rest);

            //    }

            //    // if Vacuum name has value, add into vacuum list
            //    else if (!String.IsNullOrEmpty(item.VacuumName) && String.IsNullOrEmpty(item.CylinderName))
            //    {
            //        VacuumList.Add(new VacuumIOParameters()
            //        {
            //            VacuumName = item.VacuumName,
            //            Vacuum = (OUT)item.Vacuum,
            //            Purge = item.Purge ?? null,
            //            VacuumSns = item.VacuumPressureSns1,
            //            PickedUpSns = item.VacuumPickedUpSns1,
            //            IsVacuumSnsOn = GetBoolResult(item.VacuumPressureSns1),
            //            IsPickedUpSnsOn = GetBoolResult(item.VacuumPickedUpSns1),
            //            IsVacuumOn = m_IIO.ReadOutBit((int)item.Vacuum),
            //            IsPurgeOn = GetBoolResult(item.Purge)
            //        });

            //        // add input into general input except list
            //        CylinderVacuumInputList.Add(item.VacuumPressureSns1);
            //        CylinderVacuumInputList.Add(item.VacuumPickedUpSns1);

            //        // add output into general output except list
            //        CylinderVacuumOutputList.Add(item.Vacuum);
            //        CylinderVacuumOutputList.Add(item.Purge);
            //    }
            //}

            //try
            //{
            //    // General Input
            //    InputList = new ObservableCollection<IOList>();

            //    ResourceManager inputResources = new ResourceManager(typeof(InputTable));
            //    ResourceSet inputResourceSet = inputResources.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            //    m_InputResources = new List<DictionaryEntry>();
            //    foreach (DictionaryEntry entry in inputResourceSet)
            //        m_InputResources.Add(entry);
            //    m_InputResources = m_InputResources.OrderBy(x => x.Key).Select(x => x).ToList();

            //    if (m_IIO.InputMapList.ContainsKey(CurrentSeq))
            //    {
            //        List<object> inputList = m_IIO.InputMapList.Where(key => key.Key == CurrentSeq).First().Value;
            //        var inputs = inputList.Except(CylinderVacuumInputList);

            //        foreach (object inputObj in inputs)
            //        {
            //            IN input = (IN)Enum.Parse(typeof(IN), inputObj.ToString());

            //            InputList.Add(new IOList()
            //            {
            //                Assignment = (string)m_InputResources.Where(x => x.Key.ToString() == input.ToString()).FirstOrDefault().Value,
            //                Description = (string)m_InputResources.Where(x => x.Key.ToString() == input.ToString()).FirstOrDefault().Value,
            //                Tag = (int)input,
            //                Status = m_IIO.ReadBit((int)input),
            //            });
            //        }
            //    }

            //    // Genral Output
            //    OutputList = new ObservableCollection<IOList>();

            //    ResourceManager outputResources = new ResourceManager(typeof(OutputTable));
            //    ResourceSet outputResourceSet = outputResources.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            //    m_OutputResources = new List<DictionaryEntry>();
            //    foreach (DictionaryEntry entry in outputResourceSet)
            //        m_OutputResources.Add(entry);
            //    m_OutputResources = m_OutputResources.OrderBy(x => x.Key).Select(x => x).ToList();

            //    if (m_IIO.OutputMapList.ContainsKey(CurrentSeq))
            //    {
            //        List<object> outputList = m_IIO.OutputMapList.Where(key => key.Key == CurrentSeq).First().Value;
            //        var outputs = outputList.Except(CylinderVacuumOutputList);

            //        foreach (object outputObj in outputs)
            //        {
            //            OUT output = (OUT)Enum.Parse(typeof(OUT), outputObj.ToString());

            //            OutputList.Add(new IOList()
            //            {
            //                Assignment = (string)m_OutputResources.Where(x => x.Key.ToString() == output.ToString()).FirstOrDefault().Value,
            //                Description = (string)m_OutputResources.Where(x => x.Key.ToString() == output.ToString()).FirstOrDefault().Value,
            //                Tag = (int)output,
            //                Status = m_IIO.ReadOutBit((int)output),
            //            });
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message, "IO Mapping Error");
            //}

            //CylinderVis = CylinderList.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            //VacuumVis = VacuumList.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            //GInputVis = InputList.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            //GOutputVis = OutputList.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            //if (CylinderVis == Visibility.Visible || VacuumVis == Visibility.Visible || GInputVis == Visibility.Visible || GOutputVis == Visibility.Visible)
            //{
            //    IOVis = Visibility.Visible;
            //}
            //else
            //{
            //    IOVis = Visibility.Collapsed;
            //}

            tmr_UpdateIOStatus = new Timer();
            tmr_UpdateIOStatus.Interval = m_SystemConfig.General.IOScanRate;
            tmr_UpdateIOStatus.Elapsed += UpdateIOList;

            #endregion

            #region Test Run
            IsTesting = false;
            //StartCommand = new DelegateCommand(StartTest);
            //StopCommand = new DelegateCommand(StopTest);
            #endregion
        }

        public void OnImageVisibility(string command)
        {
            if (command == "Visible")
            {
                ImageVisibility = Visibility.Visible;
            }
            else if (command == "Collapsed")
            {
                ImageVisibility = Visibility.Collapsed;
            }
        }

        #region Motion
        //public void LoadConfig()
        //{
        //    MotorList.Clear();

        //    foreach (Cfg item in m_SystemConfig.MotCfgRef)
        //    {
        //        var motCfg = m_DelegateSeq.GetMotCfg(CurrentSeq);

        //        if (motCfg.ContainsKey(item.ID))
        //        {
        //            string motReference = item.Reference;
        //            mtrcfg = MotionConfig.Open(motReference);
        //            List<TeachingPosition> pos = new List<TeachingPosition>();
        //            List<Velocity> vel = new List<Velocity>();

        //            for (int j = 0; j < mtrcfg.Position.Count; j++)
        //            {
        //                if (mtrcfg.Position[j].IsVisible)
        //                {
        //                    pos.Add(new TeachingPosition()
        //                    {
        //                        ID = mtrcfg.Position[j].ID,
        //                        TeachingPointName = mtrcfg.Position[j].Description,
        //                        TeachingPointValue = mtrcfg.Position[j].Point,
        //                        TeachingPointUOM = mtrcfg.Position[j].UoM,
        //                    });
        //                }
        //            }

        //            for (int j = 0; j < mtrcfg.Velocity.Count; j++)
        //            {
        //                vel.Add(new Velocity()
        //                {
        //                    ID = mtrcfg.Velocity[j].ID,
        //                    RowNum = (int)(j / 2),
        //                    ColNum = j % 2,
        //                    ProfileName = mtrcfg.Velocity[j].ProfileName,
        //                    DriveVel = mtrcfg.Velocity[j].DriveVel,
        //                    Acc = mtrcfg.Velocity[j].Acc,
        //                    Dcc = mtrcfg.Velocity[j].Dcc,
        //                    MaxVel = mtrcfg.Velocity[j].MaxVel,
        //                    MaxAcc = mtrcfg.Velocity[j].MaxAcc,
        //                });
        //            }

        //            MotorList.Add(new MotorTabList()
        //            {
        //                AxisName = mtrcfg.Axis.Name,
        //                MotorIndex = item.ID,
        //                AxisID = mtrcfg.Axis.AxisID,
        //                CardID = mtrcfg.Axis.CardID,
        //                Positions = pos,
        //                Velocities = vel,
        //                Revolution = mtrcfg.Axis.Revolution,
        //                Pitch = mtrcfg.Axis.Pitch,
        //                UoM = mtrcfg.Axis.UoM,
        //                Type = mtrcfg.Axis.Type.ToString(),
        //                System = mtrcfg.Axis.System.ToString(),
        //                // Option
        //                IsChkAlarm = mtrcfg.Option.ChkAlarm,
        //                IsChkReady = mtrcfg.Option.ChkReady,
        //                IsChkInPos = mtrcfg.Option.ChkInPos,
        //                IsChkFwdLmt = mtrcfg.Option.ChkFwdLimit,
        //                IsChkRevLmt = mtrcfg.Option.ChkRevLimit,
        //                IsChkAxisHome = mtrcfg.Option.ChkAxisHome,
        //                AlarmContact = mtrcfg.Option.AlarmContact,
        //                ReadyContact = mtrcfg.Option.ReadyContact,

        //                SetZeroPosAfterGoLoad = mtrcfg.Axis.SetZeroPosAfterGoLoad,
        //                Icon1 = mtrcfg.ViewCfg.Icon1,
        //                Icon2 = mtrcfg.ViewCfg.Icon2,
        //                Dir1 = mtrcfg.ViewCfg.Dir1,
        //                Dir2 = mtrcfg.ViewCfg.Dir2,
        //                Sign1 = mtrcfg.ViewCfg.Sign1,
        //                Sign2 = mtrcfg.ViewCfg.Sign2,

        //            });

        //        }
        //    }

        //    SelectedTabIndex = 0;

        //    MotorVis = MotorList.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        //}

        //private void GoMethod(TeachingPosition parameter)
        //{
        //    int motorIndex = MotorList[SelectedTabIndex].MotorIndex;
        //    MotionConfig mtrcfg = MotionConfig.Open(m_SystemConfig.MotCfgRef[motorIndex].Reference);

        //    // Check Interlock
        //    m_MtrIntL = m_MtrIntLCollection.FirstOrDefault(x => x.Provider == motorIndex);

        //    if (m_MtrIntL != null && !m_MtrIntL.CheckMtrInterlock(mtrcfg)) return;

        //    if (ServoOn)
        //    {
        //        m_Motion.SetAxisParam(mtrcfg.Axis.CardID, mtrcfg.Axis.AxisID,
        //                            mtrcfg.Velocity[MotCFG.Medium].DriveVel, mtrcfg.Velocity[MotCFG.Medium].Acc, mtrcfg.Velocity[MotCFG.Medium].Dcc,
        //                            mtrcfg.Velocity[MotCFG.Medium].JerkTime, mtrcfg.Velocity[MotCFG.Medium].KillDcc);

        //        int pulse = 0;

        //        if (m_MCTypes[1].Name == "IACSMotion")
        //        {
        //            pulse = (int)(parameter.TeachingPointValue * 1000);
        //        }
        //        else
        //        {
        //            if (mtrcfg.Axis.System == DriveMethod.Linear)
        //            {
        //                pulse = m_MCTypes[1].Name == "IAdvantechMotion" ? m_Motion.mm2Pulse(mtrcfg.Axis.Revolution, mtrcfg.Axis.Pitch, (float)parameter.TeachingPointValue, true) :
        //                                                                  m_Motion.mm2Pulse(mtrcfg.Axis.Revolution, mtrcfg.Axis.Pitch, (float)parameter.TeachingPointValue);
        //            }
        //            else if (mtrcfg.Axis.System == DriveMethod.Rotary)
        //            {
        //                pulse = m_Motion.degree2pulse(mtrcfg.Axis.Revolution, (float)parameter.TeachingPointValue);
        //            }
        //        }

        //        m_Motion.MoveAbs(MotorList[SelectedTabIndex].CardID, MotorList[SelectedTabIndex].AxisID, pulse, mtrcfg.Dir.Opr);
        //    }
        //    else
        //    {
        //        ButtonResult buttonResult = m_ShowDialog.Show(DialogIcon.Question, GetDialogTableValue("TurnOnServo"));

        //        if (buttonResult == ButtonResult.Yes)
        //        {
        //            m_Motion.ServoON(MotorList[SelectedTabIndex].CardID, MotorList[SelectedTabIndex].AxisID);
        //        }
        //        CheckServo();
        //    }
        //}
        //private void CheckServo()
        //{
        //    ServoOn = m_Motion.GetServoStatus(MotorList[SelectedTabIndex].CardID, MotorList[SelectedTabIndex].AxisID);
        //    if (ServoOn)
        //    {
        //        m_ShowDialog.Show(DialogIcon.Complete, GetDialogTableValue("ServoOn"));
        //    }
        //    else
        //    {
        //        m_ShowDialog.Show(DialogIcon.Stop, GetDialogTableValue("ServoOff"));
        //    }
        //}
        #endregion

        #region IO
        private bool? GetBoolResult(IN? data)
        {
            if (data.HasValue)
                return m_IIO.ReadBit((int)data);

            else
                return null;
        }
        private bool? GetBoolResult(OUT? data)
        {
            if (data.HasValue)
                return m_IIO.ReadOutBit((int)data);

            else
                return null;
        }

        //private void RestMethod(CylinderIOParameters obj)
        //{
        //    if (obj.Rest != null)
        //    {
        //        WriteBit(obj.Work, false);
        //        WriteBit((OUT)obj.Rest, true);
        //    }
        //    else
        //    {
        //        WriteBit(obj.Work, false);
        //    }
        //}

        //private void WorkMethod(CylinderIOParameters obj)
        //{
        //    if (obj.Rest != null)
        //    {
        //        WriteBit(obj.Work, true);
        //        WriteBit((OUT)obj.Rest, false);

        //    }
        //    else
        //    {
        //        WriteBit(obj.Work, true);
        //    }
        //}

        //private void VacuumMethod(VacuumIOParameters obj)
        //{
        //    if (obj.IsVacuumOn)
        //        if (obj.Purge.HasValue)
        //            WriteBit(obj.Purge.Value, false);

        //    WriteBit(obj.Vacuum, obj.IsVacuumOn);
        //}

        //private void PurgeMethod(VacuumIOParameters obj)
        //{
        //    if (obj.Purge.HasValue)
        //    {

        //        if (obj.IsPurgeOn.Value)
        //            WriteBit(obj.Vacuum, false);


        //        WriteBit(obj.Purge.Value, obj.IsPurgeOn.Value);
        //    }
        //}

        private void IOMethod(IOList ioParam)
        {
            WriteBit((OUT)ioParam.Tag, ioParam.Status);
        }

        public void WriteBit(OUT outbit, bool state)
        {
            m_IOIntL = m_IOIntLCollection.FirstOrDefault();

            string seqName = m_IOIntL.GetSeqName(outbit);

            m_IOIntL = m_IOIntLCollection.FirstOrDefault(x => x.Provider == EnumHelper.GetValueFromDescription<SQID>(seqName));

            //if (m_IOIntL != null && !m_IOIntL.CheckIOInterlock((int)outbit, state, m_IOIntL.Provider != SQID.CriticalScan))
            //{
            //    return;
            //}

            m_IIO.WriteBit((int)outbit, state);
        }

        #endregion

        #region Test Run
        internal void GetConfig(SQID currentSeq)
        {
            //SeqConfig seqcfg = SeqConfig.Open(m_SystemConfig.SeqCfgRef[(int)currentSeq].Reference);
            //TestSeqNumList = new ObservableCollection<TestRun>();
            //foreach (TestRun testRun in seqcfg.Test)
            //{
            //    if (testRun.IsActive)
            //    {
            //        TestSeqNumList.Add(testRun);
            //    }
            //    CanMultipleCycle = testRun.IsMultipleCycle;
            //}
            //if (TestSeqNumList.Count > 0)
            //    SelectedTestSeqNum = TestSeqNumList[0];
            //// TestRun is visible if list is not empty
            //TestRunVis = TestSeqNumList.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        //private void StartTest()
        //{
        //    IsTesting = true;
        //    TestRunEvent parameter = new TestRunEvent { SeqName = CurrentSeq, TestRunSeq = SelectedTestSeqNum.SeqNum, TestRunCycle = CycleCount };
        //    if (SelectedAxis != null)
        //        parameter.MtrIdx = SelectedAxis.MotorIndex;
        //    m_StopWatch = new Stopwatch();
        //    m_StopWatch.Start();
        //    m_EventAggregator.GetEvent<TestRunEvent>().Publish(parameter);
        //}

        private void StopTest()
        {
            m_EventAggregator.GetEvent<TestRunEvent>().Publish(new TestRunEvent { SeqName = CurrentSeq, TestRunSeq = TestRunEnum.SN.EndSampleTest });
            IsTesting = false;
            m_StopWatch.Stop();
            TestTime = GetStringTableValue("Stop");
        }
        #endregion

        #region Culture
        public string GetStringTableValue(string key)
        {
            return m_CultureResources.GetStringValue(key);
        }
        public string GetDialogTableValue(string key)
        {
            return m_CultureResources.GetDialogValue(key);
        }
        #endregion
        #endregion

        #region Access Implementation
        public IUser CurrentUser { get; }

        public bool CanAccess
        {
            get
            {
                if (m_AuthService.CurrentUser.IsAuthenticated && m_AuthService.CheckAccess(ACL.Motion) && m_AuthService.CheckAccess(ACL.IO)
                    && Global.MachineStatus != MachineStateType.Running && Global.MachineStatus != MachineStateType.Stopped && Global.MachineStatus != MachineStateType.Initializing)
                {
                    return true;
                }
                return false;
            }
        }
        #endregion
    }
}
