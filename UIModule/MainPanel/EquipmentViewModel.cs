using Cognex.DataMan.SDK;
using CsvHelper;
using CsvHelper.Configuration;
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UIModule.MainPanel
{
    public class EquipmentViewModel : BaseUIViewModel
    {
        #region Variable
        private static object m_SyncLog = new object();
        CTimer m_timeOut = new CTimer();
        private ResultsDatalog m_resultsDatalog = new ResultsDatalog();
        private float tempvisquantityholder = 0;

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

        private string m_WeighingResult = resultstatus.PendingResult.ToString();
        public string WeighingResult
        {
            get { return m_WeighingResult; }
            set { SetProperty(ref m_WeighingResult, value); }
        }

        private string m_VisClassification;
        public string VisClassification
        {
            get { return m_VisClassification; }
            set { SetProperty(ref m_VisClassification, value); }
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

        private SolidColorBrush m_VisClassificationFG = System.Windows.Media.Brushes.Black;
        public SolidColorBrush VisClassificationFG
        {
            get { return m_VisClassificationFG; }
            set { SetProperty(ref m_VisClassificationFG, value); }
        }

        private SolidColorBrush m_CdResultFG = System.Windows.Media.Brushes.Black;
        public SolidColorBrush CdResultFG
        {
            get { return m_CdResultFG; }
            set { SetProperty(ref m_CdResultFG, value); }
        }

        private SolidColorBrush m_CdResultBG = System.Windows.Media.Brushes.Transparent;
        public SolidColorBrush CdResultBG
        {
            get { return m_CdResultBG; }
            set { SetProperty(ref m_CdResultBG, value); }
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

        private Visibility m_IsAllowEditMarker;

        public Visibility IsAllowEditMarker
        {
            get { return m_IsAllowEditMarker; }
            set { SetProperty(ref m_IsAllowEditMarker, value); }
        }

        private Visibility m_IsAllowEquipment = Visibility.Collapsed;

        public Visibility IsAllowEquipment
        {
            get { return m_IsAllowEquipment; }
            set { SetProperty(ref m_IsAllowEquipment, value); }
        }

        private bool m_IsEquipViewLoaded;
        public bool IsEquipViewLoaded
        {
            get { return m_IsEquipViewLoaded; }
            set { SetProperty(ref m_IsEquipViewLoaded, value); }
        }

        private BitmapImage m_VisLiveImage;
        public BitmapImage VisLiveImage
        {
            get { return m_VisLiveImage; }
            set { SetProperty(ref m_VisLiveImage, value); }
        }

        private BitmapImage m_VisPreviousImage;
        public BitmapImage VisPreviousImage
        {
            get { return m_VisPreviousImage; }
            set { SetProperty(ref m_VisPreviousImage, value); }
        }
        #endregion

        #region Constructor
        public EquipmentViewModel(IEventAggregator eventAggregator)
        {
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
            m_EventAggregator.GetEvent<TopVisionResultEvent>().Subscribe(OnTopVisionResult);//
            m_EventAggregator.GetEvent<VisionConnectionEvent>().Subscribe(OnVisionConnection); //
            m_EventAggregator.GetEvent<TopVisionImage>().Subscribe(OnTopVisionImg);//
            m_EventAggregator.GetEvent<ResultLoggingEvent>().Subscribe(OnResultLog);//


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
            //VisionConnStatus = Global.VisionConnStatus;
        }
        #endregion

        #region Event
        public override void OnCultureChanged()
        {
            TabPageHeader = GetStringTableValue("Equipment");
        }
        #region Logging
        private void OnResultLog(ResultsDatalog resultsDatalog)
        {
            lock (m_SyncLog)
            {
                {
                    //create log directory V2
                    string date = DateTime.Now.ToString("dd-MM-yyyy");
                    string filePath = $"{m_SystemConfig.FolderPath.SoftwareResultLog}Log[{date}]\\";
                    if (!Directory.Exists(filePath))
                        Directory.CreateDirectory(filePath);

                    if (Global.OverallResult != null && Global.CurrentLotBatchNum != null && Global.CurrentLotBatchNum != string.Empty)
                    {
                        string filename = $"Batch {Global.CurrentLotBatchNum}.csv";
                        filename = filePath + filename;

                        var records = new List<ResultsDatalog>();
                        records.Add(resultsDatalog);

                        if (!File.Exists(filename.ToString()))
                        {
                            //create new .csv file and initialize the headers
                            using (var writer = new StreamWriter(filename))
                            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                            {
                                csv.Context.RegisterClassMap<ResultsDatalog.ResultsDatalogMap>();
                                csv.WriteRecords(records);
                            }
                        }
                        else
                        {
                            //append to .csv file
                            var configList = new CsvConfiguration(CultureInfo.InvariantCulture)
                            {
                                HasHeaderRecord = false
                            };

                            using (var stream = File.Open(filename, FileMode.Append))
                            using (var writer = new StreamWriter(stream))
                            using (var csv = new CsvWriter(writer, configList))
                            {
                                csv.Context.RegisterClassMap<ResultsDatalog.ResultsDatalogMap>();
                                csv.WriteRecords(records);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Vision
        private void OnTopVisionResult()
        {
            VisClassification = Global.VisClassification;

            if (Global.VisInspectResult == "OK")
            {
                VisClassificationFG = System.Windows.Media.Brushes.Green;
            }
            else if (Global.VisInspectResult == "NG")
            {
                VisClassificationFG = System.Windows.Media.Brushes.Red;
            }
            else
            {
                VisLiveImage = null;
                VisPreviousImage = null;
                VisClassificationFG = System.Windows.Media.Brushes.Black;
            }
        }
        #region Vision Live Image
        private void OnTopVisionImg(BitmapImage img)
        {
            VisLiveImage = img;
        }
        #endregion
        #region Vision Previous Image

        #endregion
        #endregion

        #region Code Reader
        //New Can Be Use
        private void OnCodeReaderConnected()
        {
            Global.CodeReaderConnStatus = "Connected";
            CdStatusFG = System.Windows.Media.Brushes.Green;
        }
        //New Can Be Use
        private void OnCodeReaderDisconnected()
        {
            Global.CodeReaderConnStatus = "Disconnected";
            CdStatusFG = System.Windows.Media.Brushes.Red;
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
            if (m_AuthService.CurrentUser.IsAuthenticated)
            {
                IsAllowEquipment = Visibility.Visible;
            }
            else
            {
                IsAllowEquipment = Visibility.Collapsed;
            }
        }

        private void OnDatalogEntity(DatalogEntity log)
        {
            if ((log.MsgType == LogMsgType.TCP && !m_SystemConfig.General.IsLogTCPMsg) || (log.MsgType == LogMsgType.SerialPort && !m_SystemConfig.General.IsLogSerialMsg))
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                DataLogCollection.Add(new Datalog(log.MsgType, log.MsgText));
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

        #region Code Reader

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

        #endregion
    }
}
