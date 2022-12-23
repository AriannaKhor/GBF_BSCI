using Cognex.InSight;
using Cognex.InSight.Cell;
using Cognex.InSight.Graphic;
using Cognex.InSight.Controls.Display;
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Threading;

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
        private FixedSizeObservableCollection<Datalog> m_SoftwareResultCollection;
        private CvsInSight m_InsightV1 = new CvsInSight();
        private CvsInSightDisplay m_CvsInSightDisplay = new CvsInSightDisplay();
        private readonly IEventAggregator m_Events;
        #endregion

        #region Constructor
        [ImportingConstructor]
        public InSightVision(IEventAggregator eventAggregator)
        {
            m_Events = eventAggregator;

            m_SystemConfig = (SystemConfig)ContainerLocator.Container.Resolve(typeof(SystemConfig));

            m_Events.GetEvent<RequestVisionConnectionEvent>().Subscribe(ConnectVision);
            //m_Events.GetEvent<RequestVisionLiveViewEvent>().Subscribe(VisionLive);

            //m_Events.GetEvent<TopVisionResultEvent>().Subscribe(VisionLive);

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
                //m_CvsInSightDisplay.InSight = m_InsightV1;
                //m_CvsInSightDisplay.InSight.Connect(m_topvisIp, "admin", "", true, false);// Determine the state of the sensor
                m_InsightV1.Connect(m_topvisIp, "admin", "", true, false);
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

            //try
            //{
            //    if (m_CvsInSightDisplay.InvokeRequired)
            //    {
            //        m_CvsInSightDisplay.Invoke(new Action(ConnectVision));
            //        return;
            //    }
            //    m_CvsInSightDisplay.InSight = m_InsightV1;
            //}
            //catch (Exception ex)
            //{
            //    MachineBase.ShowMessage(ex);
            //}
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
                //formVis = new InSightDisplayControl(m_topvisIp, m_Events);
                //formVis.Show();
                if (m_InsightV1.State == CvsInSightState.NotConnected)
                {
                    ConnectVision();
                }
                Thread.Sleep(500);
                m_InsightV1.ManualAcquire(); // Request a new acquisition to generate new results // capture Image *remember to check in-sight whether the spread sheet view is set to "Manual"
                allowVisResultchg = true;

                //VisionLive();
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
                //formVis = new InSightDisplayControl(m_topvisIp, m_Events);
                //formVis.Show();
                //tmrScanIOEnableLive.Start();

                //add implementation for trigger button here
                BitmapImage VisionImage;
                //m_CvsInSightDisplay.ShowImage = true;
                //m_CvsInSightDisplay.ShowGraphics = true;
                //m_CvsInSightDisplay.Edit.ZoomImageToFit.Execute();
                //m_CvsInSightDisplay.Edit.ManualAcquire.Execute();
                //m_CvsInSightDisplay.Edit.LiveAcquire.Execute();
                // m_CvsInSightDisplay.Edit.RepeatingTrigger.Execute();

                CvsImage cvsImage = m_InsightV1.Results.GetImage(0);
                CvsGraphicImage gimage = new CvsGraphicImage(cvsImage);


                Bitmap dImg = cvsImage.ToBitmap();
                MemoryStream ms = new MemoryStream();
                dImg.Save(ms, ImageFormat.Jpeg);
                BitmapImage bImg = new BitmapImage();
                bImg.BeginInit();
                bImg.StreamSource = new MemoryStream(ms.ToArray());
                bImg.EndInit();
                bImg.Freeze();

                VisionImage = bImg;

                m_Events.GetEvent<TopVisionImage>().Publish(VisionImage);
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
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private static BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            BitmapImage retval = null;

            try
            {
                retval = (BitmapImage)Imaging.CreateBitmapSourceFromHBitmap(
                         hBitmap,
                         IntPtr.Zero,
                         Int32Rect.Empty,
                         BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return retval;
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

        private void InsightV1_ResultsChanged(object sender, System.EventArgs e)
        {
            try
                {
                if (allowVisResultchg)
                {
                    allowVisResultchg = false;
                    CvsCell cellResult1 = m_InsightV1.Results.Cells["D20"];
                    CvsCell cellResult2 = m_InsightV1.Results.Cells["D21"];
                    CvsCell cellResult3 = m_InsightV1.Results.Cells["D22"];
                    CvsCell cellResult4 = m_InsightV1.Results.Cells["I23"];

                    if (!string.IsNullOrEmpty(cellResult1.Text) && cellResult1.Text.ToUpper() != "NULL" && cellResult1.Text.ToUpper() != "ERR" &&
                                !string.IsNullOrEmpty(cellResult2.Text) && cellResult2.Text.ToUpper() != "NULL" && cellResult2.Text.ToUpper() != "ERR" &&
                                !string.IsNullOrEmpty(cellResult3.Text) && cellResult3.Text.ToUpper() != "NULL" && cellResult3.Text.ToUpper() != "ERR" &&
                                !string.IsNullOrEmpty(cellResult4.Text) && cellResult4.Text.ToUpper() != "NULL" && cellResult4.Text.ToUpper() != "ERR")
                    {
                        Global.VisProductQuantity = float.Parse(cellResult1.Text);
                        Global.VisProductCrtOrientation = cellResult2.Text;
                        Global.VisProductWrgOrientation = cellResult3.Text;
                        Global.VisOverallResult = cellResult4.Text;

                        if (Global.VisOverallResult == "OK")
                        {
                            if (Global.VisProductQuantity == 0.000 && Global.VisProductCrtOrientation == "0.000" && Global.VisProductCrtOrientation == "0.000")
                            {
                                Global.VisInspectResult = resultstatus.NoBoxDetected.ToString();
                                m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.TopVisionSeq, MachineOpr = MachineOperationType.ProcCont });
                            }
                            else
                            {
                                Global.VisInspectResult = resultstatus.OK.ToString();
                                m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.TopVisionSeq, MachineOpr = MachineOperationType.ProcCont });
                            }
                        }
                        else
                        {
                            Global.VisInspectResult = resultstatus.NG.ToString();
                            m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.TopVisionSeq, MachineOpr = MachineOperationType.ProcFail, FailType = "WrongOrientation" });
                        }

                    }

                    VisionLive();
                    //m_InsightV1.AcceptUpdate(); // Tell the sensor that the application is ready for new results.
                    //m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = m_Title, MsgType = LogMsgType.Info, MsgText = " Product Quantity result:" + " " + Global.VisProductQuantity });
                    //m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = m_Title, MsgType = LogMsgType.Info, MsgText = " Product Correct Orientation result:" + " " + Global.VisProductCrtOrientation });
                    //m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = m_Title, MsgType = LogMsgType.Info, MsgText = " Product Wrong Orientation Result:" + " " + Global.VisProductWrgOrientation });
                    //m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = m_Title, MsgType = LogMsgType.Info, MsgText = " Overall Result:" + " " + Global.VisInspectResult });
                    //m_SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, " Vision Result :" + Global.VisInspectResult + "<" + "Total Quantity per box :" + Global.VisProductQuantity + ", Correct Orientation :" + Global.VisProductCrtOrientation + ", Wrong Orientation" + Global.VisProductWrgOrientation + ">"));
                    m_Events.GetEvent<TopVisionResultEvent>().Publish(); //Publish Vision Result
                    //m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.TopVisionSeq, MachineOpr = MachineOperationType.ProcUpdate });
                    //m_CvsInSightDisplay.ShowImage = true;
                    //Bitmap fitted_image = m_CvsInSightDisplay.GetBitmap();
                    //BitmapImage converttobitmapimg = Bitmap2BitmapImage(fitted_image);
                    //m_Events.GetEvent<TopVisionImage>().Publish(converttobitmapimg);

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
