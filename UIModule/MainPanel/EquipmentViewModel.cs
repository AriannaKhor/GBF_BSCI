using Cognex.DataMan.SDK;
using Cognex.DataMan.SDK.Utils;
using Cognex.InSight;
using Cognex.InSight.Cell;
using ConfigManager;
using DialogManager.ErrorMsg;
using GreatechApp.Core;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Helpers;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using GreatechApp.Services.Utilities;
using IOManager;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Services.Dialogs;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using UIModule.DataMarkers;
using UIModule.DataMarkers.DiagramDesigner;
using UIModule.DataMarkers.Interfaces;

namespace UIModule.MainPanel
{
	public class EquipmentViewModel : BaseUIViewModel
    {
        #region Variable
        private bool allowVisResultchg = false;
        private bool VisionDone = false;
        private bool VisionFail = false;
        private bool CodeReaderDone = false;
        private string topvisIp;
        private IPAddress codereaderIp;
        public static startMachineSeq m_seqNum = startMachineSeq.EOS;
        private IEnumerable<IMachineData> m_IMachineDataCollection;
        private const string MarkerLayoutFile = @"..\AppData\MarkerLayout.dat";
        private static object m_SynMarker = new object();
        private static object m_SynDesignItem = new object();
        private static object m_SyncLog = new object();
        protected DispatcherTimer tmrScanIOEnableLive;
        protected DispatcherTimer tmrScanIOLiveAcquire;
        protected DispatcherTimer tmrVisionResult;
        protected DispatcherTimer tmrScanIOSnapImage;
        protected DispatcherTimer tmrCodeReaderRead;
        CTimer m_timeOut = new CTimer();
        private InSightDisplayControl formVis;
        private CvsInSight m_InsightV1 = new CvsInSight();
        private DataManSystem m_CodeReader = null;
        private ResultCollector m_CodeReaderResults;
        private object _currentResultInfoSyncLock = new object();
        private CodeReaderDisplayControl formCodeReader = new CodeReaderDisplayControl();
        private IBaseIO m_IO;
        private ISystemConnector m_CodeReaderconnector = null;
        private readonly IEventAggregator m_Events;

        public DelegateCommand<string> NavigationCommand { get; set; }
        public DelegateCommand<string> TriggerVisCmd { get; private set; }
        #endregion

        #region Properties
        private string m_TabPageHeader;
        public string TabPageHeader 
        { 
            get { return m_TabPageHeader; }
            set { SetProperty(ref m_TabPageHeader, value); }
        }

        private string m_title = "Top Vision";
        public string Title
        {
            get { return m_title; }
            set { SetProperty(ref m_title, value); }
        }

        private string m_VisionConnStatus;
        public string VisionConnStatus
        {
            get { return m_VisionConnStatus; }
            set { SetProperty(ref m_VisionConnStatus, value); }
        }

        private string m_SelectedVisRcp;
        public string SelectedVisRcp
        {
            get { return m_SelectedVisRcp; }
            set { SetProperty(ref m_SelectedVisRcp, value); }
        }


        private string m_VisInspectResult = resultstatus.PendingResult.ToString();
        public string VisInspectResult
        {
            get { return m_VisInspectResult; }
            set { SetProperty(ref m_VisInspectResult, value); }
        }

        private string m_CodeReaderResult = resultstatus.PendingResult.ToString();
        public string CodeReaderResult
        {
            get { return m_CodeReaderResult; }
            set { SetProperty(ref m_CodeReaderResult, value); }
        }

        private float m_VisProductQuantity;
        public float VisProductQuantity
        {
            get { return m_VisProductQuantity; }
            set { SetProperty(ref m_VisProductQuantity, value); }
        }

        private string m_VisProductCrtOrientation;
        public string VisProductCrtOrientation
        {
            get { return m_VisProductCrtOrientation; }
            set { SetProperty(ref m_VisProductCrtOrientation, value); }
        }

        private string m_VisProductWrgOrientation;
        public string VisProductWrgOrientation
        {
            get { return m_VisProductWrgOrientation; }
            set { SetProperty(ref m_VisProductWrgOrientation, value); }
        }

        private string m_ViewCurrentBatchNumber;
        public string ViewCurrentBatchNumber
        {
            get { return m_ViewCurrentBatchNumber; }
            set { SetProperty(ref m_ViewCurrentBatchNumber, value); }
        }

        private string m_ViewCurrentContainerNumber;
        public string ViewCurrentContainerNumber
        {
            get { return m_ViewCurrentContainerNumber; }
            set { SetProperty(ref m_ViewCurrentContainerNumber, value); }
        }

        private int m_ViewCurrentBatchTotalQuantity;
        public int ViewCurrentBatchTotalQuantity
        {
            get { return m_ViewCurrentBatchTotalQuantity; }
            set { SetProperty(ref m_ViewCurrentBatchTotalQuantity, value); }
        }

        private int m_ViewAccumulateCurrentTotalBatchQuantity;
        public int ViewAccumulateCurrentTotalBatchQuantity
        {
            get { return m_ViewAccumulateCurrentTotalBatchQuantity; }
            set { SetProperty(ref m_ViewAccumulateCurrentTotalBatchQuantity, value); }
        }

        private int m_ViewCurrentBoxQuantity;
        public int ViewCurrentBoxQuantity
        {
            get { return m_ViewCurrentBoxQuantity; }
            set { SetProperty(ref m_ViewCurrentBoxQuantity, value); }
        }

        private string m_TriggerLiveVis = "Trigger Live";
        public string TriggerLiveVis
        {
            get { return m_TriggerLiveVis; }
            set { SetProperty(ref m_TriggerLiveVis, value); }
        }

        private string m_TriggerVis = "Trigger Vision";
        public string TriggerVis
        {
            get { return m_TriggerVis; }
            set { SetProperty(ref m_TriggerVis, value); }
        }

        private string m_TriggerLiveCR = "Trigger Code Reader Live ";
        public string TriggerLiveCR
        {
            get { return m_TriggerLiveCR; }
            set { SetProperty(ref m_TriggerLiveCR, value); }
        }

        private bool m_EnableCodeReader = false;
        public bool EnableCodeReader
        {
            get { return m_EnableCodeReader; }
            set { SetProperty(ref m_EnableCodeReader, value); }
        }

        private ObservableCollection<IMachineData> m_MachineDataCollection;
        public ObservableCollection<IMachineData> MachineDataCollection
        {
            get { return m_MachineDataCollection; }
            set { SetProperty(ref m_MachineDataCollection, value); }
        }

        private SolidColorBrush m_StatusFG;
        public SolidColorBrush StatusFG
        {
            get { return m_StatusFG; }
            set { SetProperty(ref m_StatusFG, value); }
        }

        private SolidColorBrush m_CdStatusFG;
        public SolidColorBrush CdStatusFG
        {
            get { return m_CdStatusFG; }
            set { SetProperty(ref m_CdStatusFG, value); }
        }

        private SolidColorBrush m_VisResultFG = System.Windows.Media.Brushes.Black;
        public SolidColorBrush VisResultFG
        {
            get { return m_VisResultFG; }
            set { SetProperty(ref m_VisResultFG, value); }
        }

        private SolidColorBrush m_CdResultFG = System.Windows.Media.Brushes.Black;
        public SolidColorBrush CdResultFG
        {
            get { return m_CdResultFG; }
            set { SetProperty(ref m_CdResultFG, value); }
        }

        private SolidColorBrush m_VisResultBG = System.Windows.Media.Brushes.Transparent;
        public SolidColorBrush VisResultBG
        {
            get { return m_VisResultBG; }
            set { SetProperty(ref m_VisResultBG, value); }
        }

        private SolidColorBrush m_CdResultBG = System.Windows.Media.Brushes.Transparent;
        public SolidColorBrush CdResultBG
        {
            get { return m_CdResultBG; }
            set { SetProperty(ref m_CdResultBG, value); }
        }

        private SolidColorBrush m_CorrectOrientationFG = System.Windows.Media.Brushes.GreenYellow;
        public SolidColorBrush CorrectOrientationFG
        {
            get { return m_CorrectOrientationFG; }
            set { SetProperty(ref m_CorrectOrientationFG, value); }
        }

        private SolidColorBrush m_WrgOrientationFG = System.Windows.Media.Brushes.OrangeRed;
        public SolidColorBrush WrgOrientationFG
        {
            get { return m_WrgOrientationFG; }
            set { SetProperty(ref m_WrgOrientationFG, value); }
        }

        private FixedSizeObservableCollection<Datalog> m_DataLogCollection;

        public FixedSizeObservableCollection<Datalog> DataLogCollection
        {
            get { return m_DataLogCollection; }
            set { SetProperty(ref m_DataLogCollection, value); }
        }


        private FixedSizeObservableCollection<Datalog> m_SoftwareResultCollection;

        public FixedSizeObservableCollection<Datalog> SoftwareResultCollection
        {
            get { return m_SoftwareResultCollection; }
            set { SetProperty(ref m_SoftwareResultCollection, value); }
        }

        private ObservableCollection<string> m_TopVisRecipeList;
        public ObservableCollection<string> TopVisRecipeList
        {
            get { return m_TopVisRecipeList; }
            set { SetProperty(ref m_TopVisRecipeList, value); }
        }

        private ObservableCollection<string> m_ContainerCollection;
        public ObservableCollection<string> ContainerCollection
        {
            get { return m_ContainerCollection; }
            set
            { SetProperty(ref m_ContainerCollection, value); }
        }

        private Visibility m_IsAllowEditMarker;

        public Visibility IsAllowEditMarker
        {
            get { return m_IsAllowEditMarker; }
            set { SetProperty(ref m_IsAllowEditMarker, value); }
        }

        private bool m_IsEquipViewLoaded;
        public bool IsEquipViewLoaded
        {
            get { return m_IsEquipViewLoaded; }
            set { SetProperty(ref m_IsEquipViewLoaded, value); }
        }
         
        #endregion

        #region Constructor
        public EquipmentViewModel(IEventAggregator eventAggregator)
        {
            m_Events = eventAggregator;

            m_IMachineDataCollection = ContainerLocator.Container.Resolve<Func<IEnumerable<IMachineData>>>()();

            TabPageHeader = GetStringTableValue("Equipment");

            Assembly asm = Assembly.GetEntryAssembly();
            AssemblyName asmName = asm.GetName();
            string version = asmName.Version.ToString();
            string exePath = Assembly.GetEntryAssembly().GetName().CodeBase;
            exePath = exePath.Replace("file:///", string.Empty);
            exePath = exePath.Replace("file://", string.Empty);
            exePath = exePath.Replace("/", @"\");
            FileInfo fi = new FileInfo(exePath);
            string buildDate = fi.LastWriteTime.ToLongDateString();
            string buildTime = fi.LastWriteTime.ToShortTimeString();

            m_EventAggregator.GetEvent<DatalogEntity>().Subscribe(OnDatalogEntity);
            m_EventAggregator.GetEvent<ResultlogEntity>().Subscribe(OnResultlogEntity);
            m_EventAggregator.GetEvent<CountingScaleOperation>().Subscribe(OnCountingScaleOperation);
            m_Events.GetEvent<FormCloseConnection>().Subscribe(OnFormCloseConnection);

            // Configure Vision timer object //To Pop Up need 3 seconds
            tmrScanIOEnableLive = new DispatcherTimer();
            tmrScanIOEnableLive.Tick += new EventHandler(tmrScanIOEnableLive_Tick);
            tmrScanIOEnableLive.Interval = new TimeSpan(0, 0, 0,3,0); 
            tmrScanIOEnableLive.IsEnabled = false;

            // Configure Vision timer object //To Enable Live 0.5 seconds
            tmrScanIOLiveAcquire = new DispatcherTimer();
            tmrScanIOLiveAcquire.Tick += new EventHandler(tmrScanIOLiveAcquire_Tick);
            tmrScanIOLiveAcquire.Interval = new TimeSpan(0, 0, 0, 0, 500);
            tmrScanIOLiveAcquire.IsEnabled = false;

            //Configure Vision timer object
            tmrVisionResult = new DispatcherTimer();
            tmrVisionResult.Tick += new EventHandler(tmrVisionResult_Tick);
            tmrVisionResult.Interval = new TimeSpan(0, 0, 0, 5, 0);

            // Configure CodeReader timer object
            tmrCodeReaderRead = new DispatcherTimer();
            tmrCodeReaderRead.Tick += new EventHandler(tmrCodeReaderRead_Tick);
            tmrCodeReaderRead.Interval = new TimeSpan(0, 0, 0, 5, 0);

            m_InsightV1.ResultsChanged += new System.EventHandler(InsightV1_ResultsChanged);
            m_InsightV1.StateChanged += new Cognex.InSight.CvsStateChangedEventHandler(InsightV1_StateChanged);
            m_EventAggregator.GetEvent<MachineState>().Subscribe(OnMachineStateChange);

            //Button Command
            NavigationCommand = new DelegateCommand<string>(OnNavigation);
            TriggerVisCmd = new DelegateCommand<string>(VisOperation);

            //DataLogging
            DataLogCollection = new FixedSizeObservableCollection<Datalog>(m_SystemConfig.General.MaxLogItem);
            DataLogCollection.CollectionChanged += this.OnDatalogCollectionChanged;

            DataLogCollection.Add(new Datalog(LogMsgType.Info, $"--- {GetStringTableValue("SoftwareVer")} : {version}"));
            DataLogCollection.Add(new Datalog(LogMsgType.Info, $"--- {GetStringTableValue("SoftwareBuildDate")} : {string.Format("{0}, {1}", buildDate, buildTime)}"));
#if DEBUG
            DataLogCollection.Add(new Datalog(LogMsgType.Info, $"--- {GetStringTableValue("CompileMode")} : {GetStringTableValue("Debug")}"));
#elif RELEASE
            DataLogCollection.Add(new Datalog(LogMsgType.Info, $"--- {ModulesTable.CompileMode} : {ModulesTable.Release}"));
#endif
            DataLogCollection.Add(new Datalog(LogMsgType.Info, $"--- {GetStringTableValue("SerialNo")} : {Environment.MachineName}"));
            DataLogCollection.Add(new Datalog(LogMsgType.Info, $"--- {GetStringTableValue("MachName")} : {m_SystemConfig.Machine.EquipName}"));
            DataLogCollection.Add(new Datalog(LogMsgType.Info, $"--- {GetStringTableValue("MachID")} : {m_SystemConfig.Machine.MachineID}"));

            //Software Results Log
            SoftwareResultCollection = new FixedSizeObservableCollection<Datalog>();
            SoftwareResultCollection.CollectionChanged += this.OnSoftwareResultCollectionChanged;

            MachineDataCollection = new ObservableCollection<IMachineData>();
            var Seq3 = m_IMachineDataCollection.FirstOrDefault(x => x.DataMarkerType == MarkerType.CircularDataMarker) as CircularDataMarkerViewModel;
            Seq3.BuildDataMarker("SampleSeq 3", SQID.SampleSeq3, UnitFlowDir.CW);
            Seq3.SetSlotName(0, "In");
            Seq3.SetSlotName(1, "Top Vision");
            Seq3.SetSlotName(4, "Bottom Vision");
            Seq3.SetSlotName(5, "Out");
            Seq3.NavigateView = "SampleSeqView";
            MachineDataCollection.Add(Seq3);

            var Sseq4 = m_IMachineDataCollection.FirstOrDefault(x => x.DataMarkerType == MarkerType.LinearDataMarker) as LinearDataMarkerViewModel;
            Sseq4.BuildDataMarker("SampleSeq 4", SQID.SampleSeq4);
            Sseq4.SetSlotName(0, "Vision4S");
            Sseq4.SetSlotName(1, "Tester1");
            Sseq4.SetSlotName(2, "Out");
            Sseq4.NavigateView = "SampleSeqView";
            MachineDataCollection.Add(Sseq4);

            var Sseq5 = m_IMachineDataCollection.FirstOrDefault(x => x.DataMarkerType == MarkerType.CircularDataMarker) as CircularDataMarkerViewModel;
            Sseq5.BuildDataMarker("SampleSeq 5", SQID.SampleSeq5, UnitFlowDir.CCW);
            Sseq5.SetSlotName(0, "In");
            Sseq5.SetSlotName(1, "Tester2");
            Sseq5.SetSlotName(2, "Out");
            Sseq5.NavigateView = "SampleSeqView";
            MachineDataCollection.Add(Sseq5);

            var tray = m_IMachineDataCollection.FirstOrDefault(x => x.DataMarkerType == MarkerType.TrayDataMarker) as TrayDataMarkerViewModel;
            tray.BuildDataMarker("InputTray", SQID.BarcodeScanner, 14, 25);
            MachineDataCollection.Add(tray);

            ConnectVision();
            ConnectCodeReader();
            VisionDone = false;
            CodeReaderDone = false;
            m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Idle);

            if (CanAccess)
            {
                m_Events.GetEvent<OpenLotEntryView>().Publish(true);
            }

            //StartOperation Thread
            Thread StartLot_Operation = new Thread(StartMachineSeq);
            StartLot_Operation.Start();
        }

        //Sequence 
        private void StartMachineSeq()
        {
            try
            {
                switch (m_seqNum)
                {
                    case startMachineSeq.TrgVision:

                        if (!VisionDone)
                        {
                            TriggerVisCapture();
                            m_timeOut.Time_Out = 2f;
                            m_seqNum = startMachineSeq.GetVisResultTimeOut;
                        }
                        break;

                    case startMachineSeq.GetVisResultTimeOut:

                        if (m_timeOut.TimeOut())
                        {
                            m_seqNum = startMachineSeq.TrgVision;
                        }
                        else
                        {
                            m_seqNum = startMachineSeq.TrgCodeReader;
                        }
                        break;

                    case startMachineSeq.TrgCodeReader:


                        break;
                }
            }
            catch (Exception ex)
            {
                MachineBase.ShowMessage(ex);
            }
        }
        #endregion

        #region Event
        public override void OnCultureChanged()
        {
            TabPageHeader = GetStringTableValue("Equipment");
        }

        private void OnMachineStateChange(MachineStateType stateType)
        {
            if (stateType == MachineStateType.Lot_Ended)
            {
                VisionDone = false;
                CodeReaderDone = false;
            }
        }

        #region Vision
        public void OnFormCloseConnection()
        {
            ConnectVision();

            if(VisInspectResult == resultstatus.Pass.ToString())
            {
                if (Global.LotInitialBatchNo == null)
                {
                    m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Idle);
                }
                else
                {
                    m_Events.GetEvent<OpenLotEntryView>().Publish(false);
                    m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Ready);
                }

            }else if (VisInspectResult == resultstatus.Fail.ToString())
            {
                m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Error);

                ButtonResult dialogResult = m_ShowDialog.Show(DialogIcon.Error, "Top Vision Fail", ButtonResult.OK);

                if (dialogResult == ButtonResult.OK)
                {
                    m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Ready);
                }
                m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = Title, MsgType = LogMsgType.Info, MsgText = " Top Vision Fail result:" });
            }

        }
        private void tmrScanIOEnableLive_Tick(object sender, EventArgs e)
        {
            try
            {
                formVis.EnableLive();
                tmrScanIOLiveAcquire.Start();
                tmrScanIOEnableLive.Stop();
            }
            catch (Exception ex)
            {
                MachineBase.ShowMessage(ex);
            }
        }

        private void tmrScanIOLiveAcquire_Tick(object sender, EventArgs e)
        {
            try
            {
                formVis.LiveAcquire();
                tmrScanIOLiveAcquire.Stop();
            }
            catch (Exception ex)
            {
                MachineBase.ShowMessage(ex);
            }
        }

        private void tmrVisionResult_Tick(object sender, EventArgs e)
        {
            try
            {
                allowVisResultchg = false;
            }
            catch (Exception ex)
            {
                MachineBase.ShowMessage(ex);
            }
        }


        private void InsightV1_ResultsChanged(object sender, EventArgs e)
        {
            try
            {   
                if (allowVisResultchg)
                {
                    allowVisResultchg = false;
                    CvsCell cellResult1 = m_InsightV1.Results.Cells["D91"]; 
                    CvsCell cellResult2 = m_InsightV1.Results.Cells["D79"]; 
                    CvsCell cellResult3 = m_InsightV1.Results.Cells["D67"]; 

                    if (!string.IsNullOrEmpty(cellResult1.Text) && cellResult1.Text.ToUpper() != "NULL" && cellResult1.Text.ToUpper() != "ERR" &&
                                !string.IsNullOrEmpty(cellResult2.Text) && cellResult2.Text.ToUpper() != "NULL" && cellResult2.Text.ToUpper() != "ERR" &&
                                !string.IsNullOrEmpty(cellResult3.Text) && cellResult3.Text.ToUpper() != "NULL" && cellResult3.Text.ToUpper() != "ERR")
                    {
                        VisProductQuantity = float.Parse(cellResult1.Text);
                        VisProductCrtOrientation = cellResult2.Text;
                        VisProductWrgOrientation = cellResult3.Text;

                        if (VisProductWrgOrientation != "0.000")
                        {
                            VisInspectResult = resultstatus.Fail.ToString();
                            VisionFail = true;
                        }
                        else
                        {
                            VisInspectResult = resultstatus.Pass.ToString();
                            EnableCodeReader = true;
                            VisionDone = true;
                            CodeReaderDone = false;
                        }
                    }
                    tmrVisionResult.Stop();
                    m_InsightV1.AcceptUpdate(); // Tell the sensor that the application is ready for new results.

                    m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = Title, MsgType = LogMsgType.Info, MsgText = " Product Quantity result:" + " " + VisProductQuantity });
                    m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = Title, MsgType = LogMsgType.Info, MsgText = " Product Correct Orientation result:" + " " + VisProductCrtOrientation });
                    m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = Title, MsgType = LogMsgType.Info, MsgText = " Product Wrong Orientation Result:" + " " + VisProductWrgOrientation });
                    m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = Title, MsgType = LogMsgType.Info, MsgText = " Overall Result:" + " " + VisInspectResult });
                    SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, VisInspectResult + ":" + VisProductQuantity + "," + VisProductCrtOrientation + "," + VisProductWrgOrientation));
                }
            }
            catch (Exception ex)
            {
                MachineBase.ShowMessage(ex);
            }
        }

        private void InsightV1_StateChanged(object sender, EventArgs e)
        {
            switch (m_InsightV1.State)
            {
                case CvsInSightState.Offline:
                    VisConnectionStatus(true, false, true, "Connected Successfully in Offline mode...");
                    Console.WriteLine("The sensor is offline.");
                    break;
                case CvsInSightState.Online:
                    VisConnectionStatus(true, false, true, "Connected");
                    Console.WriteLine("The sensor is online.");
                    break;
                case CvsInSightState.NotConnected:
                    VisConnectionStatus(false, true, false, "Disconnected");
                    Console.WriteLine("The sensor is not connected.");
                    break;
            }
            m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = Title, MsgType = LogMsgType.Info, MsgText = " Vision connection state:" + " " + m_InsightV1.State.ToString() });
        }

        #endregion

        #region Code Reader

        private void tmrCodeReaderRead_Tick(object sender, EventArgs e)
        {
            formCodeReader.TriggerCodeReader();
        }

        private void OnCountingScaleOperation(string onCountingScaleOperation)
        {
            switch (onCountingScaleOperation)
            {
                case "Start":
                    m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Running);
                    m_seqNum = startMachineSeq.TrgVision;
                   
                    //else if (!CodeReaderDone)
                    //{
                    //    formCodeReader.Show();
                    //    tmrCodeReaderRead.Start();
                    //}
                    break;

                case "Stop":
                    m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Stopped);
                    tmrCodeReaderRead.Stop();
                    break;
            }
        }

        private void Results_ComplexResultCompleted(object sender, ComplexResult complexResult)
        {
            tmrCodeReaderRead.Stop();
            string returnedresult = formCodeReader.ShowResult(complexResult);
            AnalyseResult(returnedresult);
        }

        private void OnSystemConnected(object sender, EventArgs args)
        {
            Global.CodeReaderConnStatus = ConnectionState.Connected.ToString();
            CdStatusFG = System.Windows.Media.Brushes.GreenYellow;
        }

        private void OnSystemDisconnected(object sender, EventArgs args)
        {
            Global.CodeReaderConnStatus = ConnectionState.Disconnected.ToString();
            CdStatusFG = System.Windows.Media.Brushes.DeepSkyBlue;
        }
        #endregion

        private void OnNavigation(string page)
        {
            if (page != null)
                m_RegionManager.RequestNavigate(RegionNames.CenterContentRegion, page);
        }

        public override void OnValidateLogin(bool IsAuthenticated)
        {
            base.OnValidateLogin(IsAuthenticated);
            IsAllowEditMarker = m_AuthService.CurrentUser.UserLevel == ACL.UserLevel.Admin && m_AuthService.CurrentUser.IsAuthenticated ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnResultlogEntity(ResultlogEntity log)
        {
            if ((log.MsgType == LogMsgType.TCP && !m_SystemConfig.General.IsLogTCPMsg) || (log.MsgType == LogMsgType.SerialPort && !m_SystemConfig.General.IsLogSerialMsg))
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                SoftwareResultCollection.Add(new Datalog(log.MsgType, log.MsgText));
            });
        }
        

        private void OnDatalogEntity(DatalogEntity log)
        {
            if((log.MsgType == LogMsgType.TCP && !m_SystemConfig.General.IsLogTCPMsg) || (log.MsgType == LogMsgType.SerialPort && !m_SystemConfig.General.IsLogSerialMsg))
			{
                return;
			}

            Application.Current.Dispatcher.Invoke(() =>
            {
                DataLogCollection.Add(new Datalog(log.MsgType, log.MsgText));
            });
        }

        private void OnSoftwareResultCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    Datalog resultLog = e.NewItems[0] as Datalog;

                    if (resultLog == null)
                    {
                        return;
                    }
                    WriteSoftwareResultLog(resultLog);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        private void OnDatalogCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    Datalog dtLog = e.NewItems[0] as Datalog;

                    if(dtLog == null)
                    {
                        return;
                    }
                    WriteLog(dtLog);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }
        #endregion

        #region Method

        #region Vision
        public void ConnectVision()
        {
            try
            {
                SystemConfig sysCfg = SystemConfig.Open(@"..\Config Section\General\System.Config");
                topvisIp = sysCfg.NetworkDevices[0].IPAddress;
                m_InsightV1.Connect(topvisIp, "admin", "", true, false);// Determine the state of the sensor
                m_InsightV1.SoftOnline = true;
                switch (m_InsightV1.State)
                {
                    case CvsInSightState.Offline:
                        VisConnectionStatus(true, false, true, "Connected Successfully in Offline mode...");
                        Console.WriteLine("The sensor is offline.");
                        break;
                    case CvsInSightState.Online:
                        VisConnectionStatus(true, false, true, "Connected");
                        Console.WriteLine("The sensor is online.");
                        break;
                    case CvsInSightState.NotConnected:
                        VisConnectionStatus(false, true, false, "Disconnected");
                        Console.WriteLine("The sensor is not connected.");
                        break;
                }
            }
            catch (CvsInSightLockedException ex)
            {
                // The sensor has been software-locked, preventing a connection.
                // Display a message and consume the exception.
                Console.WriteLine("The sensor is currently locked.");
                VisConnectionStatus(false, true, false, "Disconnected");
                MachineBase.ShowMessage(ex);
            }

            catch (CvsSensorAlreadyConnectedException ex)
            {
                // Someone is already connected to this sensor.
                // Display a message and consume the exception.
                Console.WriteLine("A user currently has an open connection to the sensor.");
                VisConnectionStatus(false, true, false, "Disconnected");
                MachineBase.ShowMessage(ex);
            }

            catch (CvsInvalidLogonException ex)
            {
                // If we receive an invalid logon, then we can look at the IsInvalidUsername property 
                // to find out more about the exception.
                if (ex.IsInvalidUsername)
                {
                    Console.WriteLine("Invalid Username");
                    MachineBase.ShowMessage(ex);
                }
                else
                {
                    Console.WriteLine("Invalid Password");
                    VisConnectionStatus(false, true, false, "Disconnected");
                    MachineBase.ShowMessage(ex);
                }
            }

            catch (CvsNetworkException ex)
            {
                // Unable to successfully connect to the sensor.
                // Display a message and consume the exception.
                Console.WriteLine("A network error occurred while connecting to the sensor: " + ex.Message);
                VisConnectionStatus(false, true, false, "Disconnected");
                MachineBase.ShowMessage(ex);
            }

            catch (Exception ex)
            {
                // Consume any other exception that may occur
                Console.WriteLine("Error: " + ex.Message);
                VisConnectionStatus(false, true, false, "Disconnected");
                MachineBase.ShowMessage(ex);
            }

        }

        public void VisConnectionStatus(bool visConnection, bool canConnect, bool canDisconnect, string status)
        {
            VisionConnStatus = status;
            if (visConnection == true)
            {
                StatusFG = System.Windows.Media.Brushes.GreenYellow;
            }
            else
            {
                StatusFG = System.Windows.Media.Brushes.Red;

            }
        }

        private void VisOperation(string Command)
        {
            try
            {
                if (Command == "Trigger Vision Live")
                {
                    VisionLive();
                }
            }
            catch (Exception ex)
            {
                MachineBase.ShowMessage(ex);
            }
        }

        public void VisionLive()
        {
            try
            {
                formVis = new InSightDisplayControl(topvisIp, m_Events);
                formVis.Show();
                tmrScanIOEnableLive.Start();

            }
            catch (Exception ex)
            {
                MachineBase.ShowMessage(ex);
            }
        }

        public void TriggerVisCapture()
        {
            try
            {
                m_InsightV1.ManualAcquire(); // Request a new acquisition to generate new results // capture Image *remember to check in-sight whether the spread sheet view is set to "Manual"
                allowVisResultchg = true;
                formVis = new InSightDisplayControl(topvisIp, m_Events);
                formVis.ShowDialog();
                ConnectVision();
                formVis.EnableShowImage();
                tmrVisionResult.Start();


            }
            catch (Exception ex)
            {
                MachineBase.ShowMessage(ex);
            }
        }

        #endregion

        #region Code Reader
        public void ConnectCodeReader()
        {
            try
            {
                SystemConfig sysCfg = SystemConfig.Open(@"..\Config Section\General\System.Config");
                codereaderIp = IPAddress.Parse(sysCfg.NetworkDevices[1].IPAddress);

                EthSystemConnector myConn = new EthSystemConnector(codereaderIp);

                myConn.UserName = "admin";
                myConn.Password = "";

                m_CodeReaderconnector = myConn;

                m_CodeReader = new DataManSystem(m_CodeReaderconnector);

                m_CodeReader.DefaultTimeout = 5000;

                // Subscribe to events that are signalled when the system is connected / disconnected.
                m_CodeReader.SystemConnected += new SystemConnectedHandler(OnSystemConnected);
                m_CodeReader.SystemDisconnected += new SystemDisconnectedHandler(OnSystemDisconnected);
            
                // Subscribe to events that are signalled when the device sends auto-responses.
                ResultTypes requested_result_types = ResultTypes.ReadXml | ResultTypes.Image | ResultTypes.ImageGraphics;
                m_CodeReaderResults = new ResultCollector(m_CodeReader, requested_result_types);
                m_CodeReaderResults.ComplexResultCompleted += Results_ComplexResultCompleted;
                m_CodeReader.SetKeepAliveOptions(true, 3000, 1000);
               // m_CodeReader.Connect();
            }
            catch (Exception ex)
            {
                Global.CodeReaderConnStatus = ConnectionState.Disconnected.ToString();
                CdStatusFG = System.Windows.Media.Brushes.OrangeRed;
                MachineBase.ShowMessage(ex);
            }
        }

        public void AnalyseResult(string returnedresult)
        {
            string[] splitedresult = returnedresult.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            bool checkresult = false;
            if (splitedresult.Length == 5)
            {
                Global.CurrentContainerNum = splitedresult[0];
                Global.CurrentBatchQuantity = Int32.Parse(splitedresult[1]);
                Global.CurrentMatl = Int32.Parse(splitedresult[2]);
                Global.CurrentBatchNum = splitedresult[3];
                Global.CurrentBoxQuantity = Int32.Parse( splitedresult[4]);

                if(Global.CurrentBatchNum == Global.LotInitialBatchNo)
                {
                    string Container = ContainerCollection.Where(key=>key == Global.CurrentContainerNum).FirstOrDefault();
                 
                    if (Container == null)
                    {
                        ContainerCollection.Add(Global.CurrentContainerNum);

                        if (Global.CurrentBoxQuantity == VisProductQuantity)
                        {
                            Global.AccumulateCurrentBatchQuantity = Global.AccumulateCurrentBatchQuantity + Global.CurrentBoxQuantity;

                            if (Global.AccumulateCurrentBatchQuantity > Global.LotInitialTotalBatchQuantity)
                            {
                                MachineBase.ShowMessage("Current Total Batch Quantity Does Not Match Total Batch Quantity Entered", MachineBase.MessageIcon.Error);
                                CodeReaderResult = resultstatus.Fail.ToString();
                                tmrCodeReaderRead.Start();
                                checkresult = false;

                            }
                            else
                            {
                                checkresult = true;
                                CodeReaderResult = resultstatus.Pass.ToString();
                            }
                        }
                        else
                        {
                            MachineBase.ShowMessage("Current Box Quantity Does Not Match Vision Result", MachineBase.MessageIcon.Error);
                            tmrCodeReaderRead.Start();
                            CodeReaderResult = resultstatus.Fail.ToString();
                            checkresult = false;
                        }
                    }
                    else
                    {
                        MachineBase.ShowMessage("Container Number already Exist", MachineBase.MessageIcon.Error);
                        tmrCodeReaderRead.Start();
                        CodeReaderResult = resultstatus.Fail.ToString();
                        checkresult = false;
                    }
                }
                else
                {
                    MachineBase.ShowMessage("Batch Number does not match", MachineBase.MessageIcon.Error);
                    tmrCodeReaderRead.Start();
                    CodeReaderResult = resultstatus.Fail.ToString();
                    checkresult = false;
                }
            }
            else
            {
                MachineBase.ShowMessage("Missing Result", MachineBase.MessageIcon.Error);
                tmrCodeReaderRead.Start();
                CodeReaderResult = resultstatus.Fail.ToString();
                checkresult = false;
            }

            if (checkresult)
            {
                ViewCurrentBatchNumber = Global.CurrentBatchNum;
                ViewCurrentContainerNumber = Global.CurrentContainerNum;
                ViewCurrentBatchTotalQuantity = Global.CurrentBatchQuantity;
                ViewAccumulateCurrentTotalBatchQuantity = Global.AccumulateCurrentBatchQuantity;
                ViewCurrentBoxQuantity = Global.CurrentBoxQuantity;
                CodeReaderDone = true;
                VisionDone = false; 
                
            }
            SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, "Code Reader Result :"+ CodeReaderResult + ":" + VisProductQuantity));
            SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, "Code Reader Result :" + CodeReaderResult + ":" + ViewCurrentContainerNumber));
            SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, "Code Reader Result :" + CodeReaderResult + ":" + ViewCurrentBatchTotalQuantity));
            SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, "Code Reader Result :" + CodeReaderResult + ":" + ViewAccumulateCurrentTotalBatchQuantity));
            SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, "Code Reader Result :" + CodeReaderResult + ":" + ViewCurrentBoxQuantity));
        }

        #endregion

        #region Log
        private void WriteSoftwareResultLog(Datalog log)
        {
            lock (m_SyncLog)
            {
                string executableName = System.IO.Path.GetDirectoryName(
                         System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                executableName = executableName.Replace("file:\\", string.Empty);
                FileInfo executableFileInfo = new FileInfo(executableName);
                string logDirectory = executableFileInfo.DirectoryName +
                    m_SystemConfig.FolderPath.AppLog.Replace(@"..", string.Empty);
                if (!Directory.Exists(logDirectory))
                {
                    // Create the station folder in AppData directory
                    Directory.CreateDirectory(logDirectory);
                }

                // Write the log information to the file
                FileStream fs = null;
                StringBuilder fileName = new StringBuilder();
                fileName.Append(m_SystemConfig.FolderPath.SoftwareResultLog).
                    Append("Log[").Append(log.Date).Append("].log");
                // Check whether this file exist.
                // A new datalog file will be created for each day.
                if (!File.Exists(fileName.ToString()))
                {
                    fs = new FileStream(fileName.ToString(), FileMode.Create, FileAccess.Write);
                }
                else
                {
                    fs = new FileStream(fileName.ToString(), FileMode.Append, FileAccess.Write);
                }
                using (StreamWriter logWriter = new StreamWriter(fs))
                {
                    StringBuilder logData = new StringBuilder();
                    logData.Append(log.Date).Append(" | ").
                        Append(log.Time).Append(" |").
                        Append(log.MsgType).Append("| ").
                        Append(log.MsgText);
                    logWriter.WriteLine(logData.ToString());
                    logWriter.Close();
                }
            }
        }


        private void WriteLog(Datalog log)
        {
            lock(m_SyncLog)
            {
                string executableName = System.IO.Path.GetDirectoryName(
                         System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                executableName = executableName.Replace("file:\\", string.Empty);
                FileInfo executableFileInfo = new FileInfo(executableName);
                string logDirectory = executableFileInfo.DirectoryName +
                    m_SystemConfig.FolderPath.AppLog.Replace(@"..", string.Empty);
                if (!Directory.Exists(logDirectory))
                {
                    // Create the station folder in AppData directory
                    Directory.CreateDirectory(logDirectory);
                }

                // Write the log information to the file
                FileStream fs = null;
                StringBuilder fileName = new StringBuilder();
                fileName.Append(m_SystemConfig.FolderPath.AppLog).
                    Append("Log[").Append(log.Date).Append("].log");
                // Check whether this file exist.
                // A new datalog file will be created for each day.
                if (!File.Exists(fileName.ToString()))
                {
                    fs = new FileStream(fileName.ToString(), FileMode.Create, FileAccess.Write);
                }
                else
                {
                    fs = new FileStream(fileName.ToString(), FileMode.Append, FileAccess.Write);
                }
                using (StreamWriter logWriter = new StreamWriter(fs))
                {
                    StringBuilder logData = new StringBuilder();
                    logData.Append(log.Date).Append(" | ").
                        Append(log.Time).Append(" |").
                        Append(log.MsgType).Append("| ").
                        Append(log.MsgText);
                    logWriter.WriteLine(logData.ToString());
                    logWriter.Close();
                }
            }
        }
        #endregion

        #region Others

        public void OnDeactivate()
        {
            IsEquipViewLoaded = false;
        }

        public bool KeepAlive
        {
            get
            {
                return true;
            }
        }

        #endregion

        #endregion

        #region Enum 
        public enum resultstatus
        {
            PendingResult,
            Pass,
            Fail,
        }

        public enum startMachineSeq
        {
            EOS,
            TrgVision,
            TrgCodeReader,
            GetVisResultTimeOut,
            GetCodeReaderResultTimeOut,
        }

        #endregion
    }
}
