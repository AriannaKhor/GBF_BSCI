using Cognex.InSight;
using Cognex.InSight.Cell;
using ConfigManager;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Helpers;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using GreatechApp.Services.Utilities;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace TCPIPManager
{
   public class InSightVision : IInsightVision
    {
        #region Variable
        private string m_topvisIp;
        private string m_Title = "Top Vision";
        private bool m_EnableCodeReader;
        private bool allowVisResultchg = false;
        private static object m_SyncLog = new object();
        public SystemConfig m_SystemConfig;
        private InSightDisplayControl formVis;
        protected DispatcherTimer tmrScanIOEnableLive;
        protected DispatcherTimer tmrScanIOLiveAcquire;
        private FixedSizeObservableCollection<Datalog> m_SoftwareResultCollection;
        private CvsInSight m_InsightV1 = new CvsInSight();
        private readonly IEventAggregator m_Events;
        #endregion



        #region Constructor
        [ImportingConstructor]
        public InSightVision(IEventAggregator eventAggregator)
        {
            m_Events = eventAggregator;

            m_SystemConfig = (SystemConfig)ContainerLocator.Container.Resolve(typeof(SystemConfig));

            // Configure Vision timer object //To Pop Up need 3 seconds
            tmrScanIOEnableLive = new DispatcherTimer();
            tmrScanIOEnableLive.Tick += new System.EventHandler(tmrScanIOEnableLive_Tick);
            tmrScanIOEnableLive.Interval = new TimeSpan(0, 0, 0, 3, 0);
            tmrScanIOEnableLive.IsEnabled = false;

            // Configure Vision timer object //To Enable Live 0.5 seconds
            tmrScanIOLiveAcquire = new DispatcherTimer();
            tmrScanIOLiveAcquire.Tick += new System.EventHandler(tmrScanIOLiveAcquire_Tick);
            tmrScanIOLiveAcquire.Interval = new TimeSpan(0, 0, 0, 0, 500);
            tmrScanIOLiveAcquire.IsEnabled = false;

            m_Events.GetEvent<RequestVisionConnectionEvent>().Subscribe(ConnectVision);
            m_Events.GetEvent<RequestVisionLiveViewEvent>().Subscribe(VisionLive);

            m_InsightV1.ResultsChanged += new System.EventHandler(InsightV1_ResultsChanged);
            m_InsightV1.StateChanged += new Cognex.InSight.CvsStateChangedEventHandler(InsightV1_StateChanged);

            //Software Results Log
            m_SoftwareResultCollection = new FixedSizeObservableCollection<Datalog>();
            m_SoftwareResultCollection.CollectionChanged += this.OnSoftwareResultCollectionChanged;
        }

        
        #endregion

        #region Method
        public void ConnectVision()
        {
            try
            {
                SystemConfig sysCfg = SystemConfig.Open(@"..\Config Section\General\System.Config");
                m_topvisIp = sysCfg.NetworkDevices[0].IPAddress;
                m_InsightV1.Connect(m_topvisIp, "admin", "", true, false);// Determine the state of the sensor
                m_InsightV1.SoftOnline = true;
                switch (m_InsightV1.State)
                {
                    case CvsInSightState.Offline:
                        VisConnectionStatus(true, false, true, "Connected Successfully in Offline mode...");
                        break;
                    case CvsInSightState.Online:
                        VisConnectionStatus(true, false, true, "Connected");
                        break;
                    case CvsInSightState.NotConnected:
                        VisConnectionStatus(false, true, false, "Disconnected");
                        break;
                }
            }
            catch (CvsInSightLockedException ex)
            {
                // The sensor has been software-locked, preventing a connection.
                // Display a message and consume the exception.
                VisConnectionStatus(false, true, false, "Disconnected");
                MachineBase.ShowMessage(ex);
            }
            catch (CvsSensorAlreadyConnectedException ex)
            {
                // Someone is already connected to this sensor.
                // Display a message and consume the exception.
                VisConnectionStatus(false, true, false, "Disconnected");
                MachineBase.ShowMessage(ex);
            }
            catch (CvsInvalidLogonException ex)
            {
                // If we receive an invalid logon, then we can look at the IsInvalidUsername property 
                // to find out more about the exception.
                if (ex.IsInvalidUsername)
                {
                    MachineBase.ShowMessage(ex);
                }
                else
                {
                    VisConnectionStatus(false, true, false, "Disconnected");
                    MachineBase.ShowMessage(ex);
                }
            }
            catch (CvsNetworkException ex)
            {
                // Unable to successfully connect to the sensor.
                // Display a message and consume the exception.
                VisConnectionStatus(false, true, false, "Disconnected");
                MachineBase.ShowMessage(ex);
            }
            catch (Exception ex)
            {
                // Consume any other exception that may occur
                VisConnectionStatus(false, true, false, "Disconnected");
                MachineBase.ShowMessage(ex);
            }

        }

        public void VisConnectionStatus(bool visConnection, bool canConnect, bool canDisconnect, string status)
        {
            Global.VisionConnStatus = status;
            Global.VisConnection = visConnection;
            m_Events.GetEvent<VisionConnectionEvent>().Publish();
        }

        public void TriggerVisCapture()
        {
            try
            {
                m_InsightV1.ManualAcquire(); // Request a new acquisition to generate new results // capture Image *remember to check in-sight whether the spread sheet view is set to "Manual"
                allowVisResultchg = true;
                //formVis = new InSightDisplayControl(m_topvisIp, m_Events);
                //formVis.ShowDialog();
                //formVis.EnableShowImage();

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
                formVis = new InSightDisplayControl(m_topvisIp, m_Events);
                formVis.Show();
                tmrScanIOEnableLive.Start();

            }
            catch (Exception ex)
            {
                MachineBase.ShowMessage(ex);
            }
        }

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
        #endregion

        #region Event
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
        private void tmrScanIOEnableLive_Tick(object sender, System.EventArgs e) //the "EventArgs" need to use System.EventArgs else it will refer to the EventArgs under TCPIPManager which is not compatoble -------------------------------->>
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

        private void tmrScanIOLiveAcquire_Tick(object sender, System.EventArgs e)
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

        private void InsightV1_ResultsChanged(object sender, System.EventArgs e)
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
                        Global.VisProductQuantity = float.Parse(cellResult1.Text);
                        Global.VisProductCrtOrientation = cellResult2.Text;
                        Global.VisProductWrgOrientation = cellResult3.Text;

                        if (Global.VisProductWrgOrientation != "0.000")
                        {
                            Global.VisInspectResult = resultstatus.Fail.ToString();
                            m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcFail});
                        }
                        else
                        {
                            Global.VisInspectResult = resultstatus.Pass.ToString();
                            m_EnableCodeReader = true;
                            m_Events.GetEvent<EnableCodeReaderEvent>().Publish(m_EnableCodeReader);
                            m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcCont});
                        }
                    }
                    m_InsightV1.AcceptUpdate(); // Tell the sensor that the application is ready for new results.
                    m_Events.GetEvent<TopVisionResultEvent>().Publish();
                    m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = m_Title, MsgType = LogMsgType.Info, MsgText = " Product Quantity result:" + " " + Global.VisProductQuantity });
                    m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = m_Title, MsgType = LogMsgType.Info, MsgText = " Product Correct Orientation result:" + " " + Global.VisProductCrtOrientation });
                    m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = m_Title, MsgType = LogMsgType.Info, MsgText = " Product Wrong Orientation Result:" + " " + Global.VisProductWrgOrientation });
                    m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = m_Title, MsgType = LogMsgType.Info, MsgText = " Overall Result:" + " " + Global.VisInspectResult });
                    m_SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, Global.VisInspectResult + ":" + Global.VisProductQuantity + "," + Global.VisProductCrtOrientation + "," + Global.VisProductWrgOrientation));
                }
               
            }
            catch (Exception ex)
            {
                MachineBase.ShowMessage(ex);
            }
        }

        private void InsightV1_StateChanged(object sender, CvsStateChangedEventArgs e)
        {
            switch (m_InsightV1.State)
            {
                case CvsInSightState.Offline:
                    VisConnectionStatus(true, false, true, "Connected Successfully in Offline mode...");
                    break;
                case CvsInSightState.Online:
                    VisConnectionStatus(true, false, true, "Connected");
                    break;
                case CvsInSightState.NotConnected:
                    VisConnectionStatus(false, true, false, "Disconnected");
                    break;
            }
            m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = m_Title, MsgType = LogMsgType.Info, MsgText = " Vision connection state:" + " " + m_InsightV1.State.ToString() });
        }
    
        #endregion


   }
}
