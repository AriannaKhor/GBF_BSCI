using Cognex.DataMan.SDK;
using GreatechApp.Core;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Helpers;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using GreatechApp.Services.Utilities;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UIModule.MainPanel
{
    public class OperatorViewModel : BaseUIViewModel
    {
        #region Variable
        private const string MarkerLayoutFile = @"..\AppData\MarkerLayout.dat";
        private static object m_SynMarker = new object();
        private static object m_SynDesignItem = new object();
        private static object m_SyncLog = new object();
        CTimer m_timeOut = new CTimer();
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

        private float m_VisProductQuantity;
        public float VisProductQuantity
        {
            get { return m_VisProductQuantity; }
            set { SetProperty(ref m_VisProductQuantity, value); }
        }

        private float m_VisProductCrtOrientation;
        public float VisProductCrtOrientation
        {
            get { return m_VisProductCrtOrientation; }
            set { SetProperty(ref m_VisProductCrtOrientation, value); }
        }

        private float m_VisProductWrgOrientation;
        public float VisProductWrgOrientation
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

        private SolidColorBrush m_ProductQuantityFG = System.Windows.Media.Brushes.Black;
        public SolidColorBrush ProductQuantityFG
        {
            get { return m_ProductQuantityFG; }
            set { SetProperty(ref m_ProductQuantityFG, value); }
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

        private SolidColorBrush m_CorrectOrientationFG = System.Windows.Media.Brushes.Black;
        public SolidColorBrush CorrectOrientationFG
        {
            get { return m_CorrectOrientationFG; }
            set { SetProperty(ref m_CorrectOrientationFG, value); }
        }

        private SolidColorBrush m_WrgOrientationFG = System.Windows.Media.Brushes.Black;
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

        private Visibility m_IsAllowOperator = Visibility.Collapsed;

        public Visibility IsAllowOperator
        {
            get { return m_IsAllowOperator; }
            set { SetProperty(ref m_IsAllowOperator, value); }
        }

        private bool m_IsEquipViewLoaded;
        public bool IsEquipViewLoaded
        {
            get { return m_IsEquipViewLoaded; }
            set { SetProperty(ref m_IsEquipViewLoaded, value); }
        }

        private BitmapImage m_VisImage;
        public BitmapImage VisImage
        {
            get { return m_VisImage; }
            set { SetProperty(ref m_VisImage, value); }
        }
        #endregion

        #region Constructor
        public OperatorViewModel(IEventAggregator eventAggregator)
        {
            TabPageHeader = GetStringTableValue("Operator");

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

            m_EventAggregator.GetEvent<ResultlogEntity>().Subscribe(OnResultlogEntity);
            m_EventAggregator.GetEvent<TopVisionResultEvent>().Subscribe(OnTopVisionResult);//
            m_EventAggregator.GetEvent<VisionConnectionEvent>().Subscribe(OnVisionConnection); //
            m_EventAggregator.GetEvent<TopVisionImage>().Subscribe(OnTopVisionImg);//

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
            DataLogCollection.Add(new Datalog(LogMsgType.Info, $"--- {GetStringTableValue("CompileMode")} : {GetStringTableValue("Release")}"));
#endif
            DataLogCollection.Add(new Datalog(LogMsgType.Info, $"--- {GetStringTableValue("SerialNo")} : {Environment.MachineName}"));
            DataLogCollection.Add(new Datalog(LogMsgType.Info, $"--- {GetStringTableValue("MachName")} : {m_SystemConfig.Machine.EquipName}"));
            DataLogCollection.Add(new Datalog(LogMsgType.Info, $"--- {GetStringTableValue("MachID")} : {m_SystemConfig.Machine.MachineID}"));

            //m_EventAggregator.GetEvent<RequestVisionConnectionEvent>().Publish();
        }

        #endregion

        #region Event
        public override void OnCultureChanged()
        {
            TabPageHeader = GetStringTableValue("Operator");
        }

        #region Vision
        private void OnTopVisionResult()
        {
            VisInspectResult = Global.VisInspectResult;
            VisProductQuantity = Global.VisProductQuantity;
            VisProductCrtOrientation = Global.VisProductCrtOrientation;
            VisProductWrgOrientation = Global.VisProductWrgOrientation;

            if (Global.VisInspectResult == "OK")
            {
                VisResultBG = System.Windows.Media.Brushes.Green;
                ProductQuantityFG = System.Windows.Media.Brushes.Green;
                CorrectOrientationFG = System.Windows.Media.Brushes.Green;
                WrgOrientationFG = System.Windows.Media.Brushes.Green;
            }
            else if (Global.VisInspectResult == "NG")
            {
                VisResultBG = System.Windows.Media.Brushes.Red;
                ProductQuantityFG = System.Windows.Media.Brushes.Red;
                CorrectOrientationFG = System.Windows.Media.Brushes.Red;
                WrgOrientationFG = System.Windows.Media.Brushes.Red;
            }
            else
            {
                VisResultBG = System.Windows.Media.Brushes.Transparent;
                ProductQuantityFG = System.Windows.Media.Brushes.Black;
                CorrectOrientationFG = System.Windows.Media.Brushes.Black;
                WrgOrientationFG = System.Windows.Media.Brushes.Black;
            }
        }

        private void OnTopVisionImg(BitmapImage img)
        {
            VisImage = img;
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
            //IsAllowEditMarker = m_AuthService.CurrentUser.UserLevel == ACL.UserLevel.Admin && m_AuthService.CurrentUser.IsAuthenticated ? Visibility.Visible : Visibility.Collapsed;
            //IsAllowAccessOperator = m_AuthService.CurrentUser.UserLevel == ACL.UserLevel.Operator && m_AuthService.CurrentUser.IsAuthenticated ? Visibility.Visible : Visibility.Collapsed;

            if (m_AuthService.CurrentUser.UserLevel == ACL.UserLevel.Operator && m_AuthService.CurrentUser.IsAuthenticated)
            {
                IsAllowOperator = Visibility.Visible;

            }
            else
            {
                IsAllowOperator = Visibility.Collapsed;

            }
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

        private void OnDatalogCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    Datalog dtLog = e.NewItems[0] as Datalog;

                    if (dtLog == null)
                    {
                        return;
                    }
                    WriteLog(dtLog);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }
        #endregion

        #region Method

        #region Vision
        //New Can Be Use
        public void OnVisionConnection()
        {
            VisionConnStatus = Global.VisionConnStatus;
            if (Global.VisConnection == true)
            {
                StatusFG = System.Windows.Media.Brushes.Green;
            }
            else
            {
                StatusFG = System.Windows.Media.Brushes.Red;
            }
        }
        //New Can Be Use
        private void VisOperation(string Command)
        {
            try
            {
                if (Command == "Trigger Vision Live")
                {
                    m_EventAggregator.GetEvent<RequestVisionLiveViewEvent>().Publish();
                }
            }
            catch (Exception ex)
            {
                MachineBase.ShowMessage(ex);
            }
        }

        #endregion

        #region Log
        private void WriteLog(Datalog log)
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
        #endregion
    }
}
