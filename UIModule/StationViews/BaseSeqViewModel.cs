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
        public IShowDialog m_ShowDialog;
        public IBaseIO m_IIO;
        public IDelegateSeq m_DelegateSeq;
        public IEventAggregator m_EventAggregator;
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
        public Timer tmr_UpdateMotionStatus;
        public int SelectedTabIndex { get; set; }

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
            m_IIO = (IBaseIO)ContainerLocator.Container.Resolve(typeof(IBaseIO));
            m_DelegateSeq = (IDelegateSeq)ContainerLocator.Container.Resolve(typeof(IDelegateSeq));
            m_EventAggregator = (IEventAggregator)ContainerLocator.Container.Resolve(typeof(IEventAggregator));
            m_AuthService = (AuthService)ContainerLocator.Container.Resolve(typeof(AuthService));
            m_ShowDialog = (IShowDialog)ContainerLocator.Container.Resolve(typeof(IShowDialog));
            m_IOIntLCollection = ContainerLocator.Container.Resolve<Func<IEnumerable<IIOInterlock>>>()();
            m_IOIntL = m_IOIntLCollection.FirstOrDefault();

            ImageCommand = new DelegateCommand<string>(OnImageVisibility);

            m_EventAggregator.GetEvent<MachineState>().Subscribe(OnMachineState);
            m_EventAggregator.GetEvent<ValidateLogin>().Subscribe(OnValidateLogin);

            RaisePropertyChanged(nameof(CanAccess));
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

        #region IO
        private void UpdateIOList(object sender, ElapsedEventArgs e)
        {
            try
            {
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

        public void Init(string title, SQID sqid)
        {
            Title = title;
            CurrentSeq = sqid;

            Init();

            GetConfig(sqid);
        }

        public void Init()
        {
            #region IO
            tmr_UpdateIOStatus = new Timer();
            tmr_UpdateIOStatus.Interval = m_SystemConfig.General.IOScanRate;
            tmr_UpdateIOStatus.Elapsed += UpdateIOList;

            #endregion

            #region Test Run
            IsTesting = false;
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

        #region IO
        private bool? GetBoolResult(OUT? data)
        {
            if (data.HasValue)
                return m_IIO.ReadOutBit((int)data);

            else
                return null;
        }

        private void IOMethod(IOList ioParam)
        {
            WriteBit((OUT)ioParam.Tag, ioParam.Status);
        }

        public void WriteBit(OUT outbit, bool state)
        {
            m_IOIntL = m_IOIntLCollection.FirstOrDefault();

            string seqName = m_IOIntL.GetSeqName(outbit);

            m_IOIntL = m_IOIntLCollection.FirstOrDefault(x => x.Provider == EnumHelper.GetValueFromDescription<SQID>(seqName));
            m_IIO.WriteBit((int)outbit, state);
        }

        #endregion

        #region Test Run
        internal void GetConfig(SQID currentSeq)
        {
           
        }

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
                if (m_AuthService.CurrentUser.IsAuthenticated && m_AuthService.CheckAccess(ACL.IO)
                    && Global.MachineStatus != MachineStateType.Running)
                {
                    return true;
                }
                return false;
            }
        }
        #endregion
    }
}
