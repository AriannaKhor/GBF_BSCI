using Cognex.InSight;
using Cognex.InSight.Cell;
using Cognex.InSight.Graphic;
using ConfigManager;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using GreatechApp.Services.Utilities;
using Prism.Events;
using Prism.Ioc;
using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Collections.ObjectModel;

namespace TCPIPManager
{
    public class InSightVision : IInsightVision
    {
        #region Variable
        private string m_topvisIp;
        private string m_Title = "Top Vision";
        private bool allowVisResultchg = false;
        public SystemConfig m_SystemConfig;
        public ProductQtyConfig productQtyConfig;
        private CvsInSight m_InsightV1 = new CvsInSight();
        private readonly IEventAggregator m_Events;
        private readonly IContainerProvider m_container;
        private float m_MaxQuantity;
        public ObservableCollection<ProductQuantityParameter> ProductQuantityLimit;


        #endregion

        #region Constructor
        [ImportingConstructor]
        public InSightVision(IEventAggregator eventAggregator)
        {
            m_Events = eventAggregator;
            m_SystemConfig = (SystemConfig)ContainerLocator.Container.Resolve(typeof(SystemConfig));
            m_InsightV1.ResultsChanged += new System.EventHandler(InsightV1_ResultsChanged);
            m_InsightV1.StateChanged += new Cognex.InSight.CvsStateChangedEventHandler(InsightV1_StateChanged);
            OnRequestVisConnEvent();
            //m_Events.GetEvent<RequestVisionConnectionEvent>().Subscribe(OnRequestVisConnEvent);
        }

        private void OnRequestVisConnEvent()
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

#if !SIMULATION
                    VisConnectionStatus(false, true, false, "Disconnected");
                    ConnectVision();
                    break;
#else
                    VisConnectionStatus(true, false, true, "Connected");
                    break;
#endif  
            }
        }

        #endregion

        #region Method
        #region Connect Vision
        public void ConnectVision()
        {
            try
            {
                SystemConfig sysCfg = SystemConfig.Open(@"..\Config Section\General\System.Config");
                int visDevicesCount = sysCfg.VisionDevices.Count;
                foreach(VisionDevices devices in sysCfg.VisionDevices)
                {
                    m_InsightV1.Connect(devices.IPAddress, "admin", "", true, false);
                    m_InsightV1.SoftOnline = true;
                }
                //OnRequestVisConnEvent();
            }
            catch (CvsInSightLockedException ex)
            {
                // The sensor has been software-locked, preventing a connection.
                // Display a message and consume the exception.
                VisConnectionStatus(false, true, false, "Disconnected");
                MachineBase.ShowMessage(ex);
                m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = m_Title, MsgType = LogMsgType.Info, MsgText = " Exception1 Vision connection state:" + " " + m_InsightV1.State.ToString() });
            }
            catch (CvsSensorAlreadyConnectedException ex)
            {
                // Someone is already connected to this sensor.
                // Display a message and consume the exception.
                VisConnectionStatus(false, true, false, "Disconnected");
                MachineBase.ShowMessage(ex);
                m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = m_Title, MsgType = LogMsgType.Info, MsgText = " Exception2 Vision connection state:" + " " + m_InsightV1.State.ToString() });
            }
            catch (CvsInvalidLogonException ex)
            {
                // If we receive an invalid logon, then we can look at the IsInvalidUsername property 
                // to find out more about the exception.
                if (ex.IsInvalidUsername)
                {
                    MachineBase.ShowMessage(ex);
                    m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = m_Title, MsgType = LogMsgType.Info, MsgText = " Exception3a Vision connection state:" + " " + m_InsightV1.State.ToString() });
                }
                else
                {
                    VisConnectionStatus(false, true, false, "Disconnected");
                    MachineBase.ShowMessage(ex);
                    m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = m_Title, MsgType = LogMsgType.Info, MsgText = " Exception3b Vision connection state:" + " " + m_InsightV1.State.ToString() });
                }
            }
            catch (CvsNetworkException ex)
            {
                // Unable to successfully connect to the sensor.
                // Display a message and consume the exception.
                VisConnectionStatus(false, true, false, "Disconnected");
                MachineBase.ShowMessage(ex);
                m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = m_Title, MsgType = LogMsgType.Info, MsgText = " Exception4 Vision connection state:" + " " + m_InsightV1.State.ToString() });
            }
            catch (Exception ex)
            {
                // Consume any other exception that may occur
                VisConnectionStatus(false, true, false, "Disconnected");
                MachineBase.ShowMessage(ex);
                m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = m_Title, MsgType = LogMsgType.Info, MsgText = " Exception5 Vision connection state:" + " " + m_InsightV1.State.ToString() });
            }
        }
        #endregion
        public void EditCell(string colorRcp)
        {
            // Define which cell you want to modify
            CvsCellLocation cell = new CvsCellLocation(0,'B');
            //Modify the cell with the specific value
            m_InsightV1.SetExpression(cell,colorRcp,true);
        }
        public void VisConnectionStatus(bool visConnection, bool canConnect, bool canDisconnect, string status)
        {
            Global.VisionConnStatus = status;
            Global.VisConnection = visConnection;
            m_Events.GetEvent<VisionConnectionEvent>().Publish();
        }

        public bool VisConnectionStatus()
        {
            if (Global.VisConnection)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void GetProductQuantityConfig()
        {
            ProductQtyConfig m_ProductQttSettingsysCfg = ProductQtyConfig.Open(@"..\Config Section\Product Quantity Setting\QtyLimit.Config");
            for (int i = 0; i < m_ProductQttSettingsysCfg.Setting.Count; i++)
            {
                m_MaxQuantity = m_ProductQttSettingsysCfg.Setting[i].MaxQuantity;
            }
        }

        public void TriggerVisCapture()
        {
            try
            {
                if (m_InsightV1.State == CvsInSightState.NotConnected)
                {
#if !SIMULATION
                    ConnectVision();
#endif
                }
                Thread.Sleep(300);
#if SIMULATION
                m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcVisCont, ContType = "TriggerCodeReader" });
#else
                m_InsightV1.ManualAcquire(); // Request a new acquisition to generate new results // capture Image *remember to check in-sight whether the spread sheet view is set to "Manual"
#endif
                allowVisResultchg = true;
            }
            catch (Exception ex)
            {
                MachineBase.ShowMessage(ex);
                m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = m_Title, MsgType = LogMsgType.Info, MsgText = " Exception during TriggerVisCapture Vision connection state:" + " " + m_InsightV1.State.ToString() });
            }
        }

        public void VisionImg()
        {
            try
            {
                BitmapImage VisionImage;
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
        #endregion

        #region Event
        private void InsightV1_ResultsChanged(object sender, System.EventArgs e)
        {
            try
            {
#if !SIMULATION
                if (allowVisResultchg)
                {
                    allowVisResultchg = false;
                    CvsCell cellResult1 = m_InsightV1.Results.Cells["D20"];
                    CvsCell cellResult2 = m_InsightV1.Results.Cells["D21"];
                    CvsCell cellResult3 = m_InsightV1.Results.Cells["D22"];
                    CvsCell cellResult4 = m_InsightV1.Results.Cells["I23"];
                    CvsCell cellResult5 = m_InsightV1.Results.Cells["I23"];

                    if (!string.IsNullOrEmpty(cellResult1.Text) && cellResult1.Text.ToUpper() != "NULL" && cellResult1.Text.ToUpper() != "ERR" &&
                                !string.IsNullOrEmpty(cellResult2.Text) && cellResult2.Text.ToUpper() != "NULL" && cellResult2.Text.ToUpper() != "ERR" &&
                                !string.IsNullOrEmpty(cellResult3.Text) && cellResult3.Text.ToUpper() != "NULL" && cellResult3.Text.ToUpper() != "ERR" &&
                                !string.IsNullOrEmpty(cellResult4.Text) && cellResult4.Text.ToUpper() != "NULL" && cellResult4.Text.ToUpper() != "ERR" &&
                                !string.IsNullOrEmpty(cellResult5.Text) && cellResult4.Text.ToUpper() != "NULL" && cellResult5.Text.ToUpper() != "ERR")
                    {
                        Global.VisSlipSheet = cellResult1.Text;
                        Global.VisReversePouch = cellResult2.Text;
                        Global.VisColorPouch = cellResult3.Text;
                        Global.VisInvertColorPouch = cellResult4.Text;
                        Global.VisDFU = cellResult5.Text;
                    }
#else
                
#endif
                }
            }
            catch (Exception ex)
            {

            }
        }
        //private void InsightV1_ResultsChanged(object sender, System.EventArgs e)
        //{
        //    try
        //    {
        //        GetProductQuantityConfig();

        //        if (allowVisResultchg)
        //        {
        //            allowVisResultchg = false;
        //            CvsCell cellResult1 = m_InsightV1.Results.Cells["D20"];
        //            CvsCell cellResult2 = m_InsightV1.Results.Cells["D21"];
        //            CvsCell cellResult3 = m_InsightV1.Results.Cells["D22"];
        //            CvsCell cellResult4 = m_InsightV1.Results.Cells["I23"];

        //            if (!string.IsNullOrEmpty(cellResult1.Text) && cellResult1.Text.ToUpper() != "NULL" && cellResult1.Text.ToUpper() != "ERR" &&
        //                        !string.IsNullOrEmpty(cellResult2.Text) && cellResult2.Text.ToUpper() != "NULL" && cellResult2.Text.ToUpper() != "ERR" &&
        //                        !string.IsNullOrEmpty(cellResult3.Text) && cellResult3.Text.ToUpper() != "NULL" && cellResult3.Text.ToUpper() != "ERR" &&
        //                        !string.IsNullOrEmpty(cellResult4.Text) && cellResult4.Text.ToUpper() != "NULL" && cellResult4.Text.ToUpper() != "ERR")
        //            {
        //                Global.VisProductQuantity = float.Parse(cellResult1.Text);
        //                Global.VisProductCrtOrientation = float.Parse(cellResult2.Text);
        //                Global.VisProductWrgOrientation = float.Parse(cellResult3.Text);
        //                Global.VisOverallResult = cellResult4.Text;

        //                if (Global.VisOverallResult == "OK")
        //                {
        //                    if (Global.VisProductQuantity == 0 && Global.VisProductCrtOrientation == 0 && Global.VisProductCrtOrientation == 0)
        //                    {
        //                        Global.VisInspectResult = resultstatus.NoBoxDetected.ToString();
        //                        if (!Global.EndTrigger)
        //                        {
        //                             m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcVisCont, ContType = "ReTriggerVis" });
        //                        }
        //                        else
        //                        {
        //                            Global.EndTrigger = false;
        //                        }
        //                    }
        //                    else if (Global.VisProductQuantity > m_MaxQuantity)
        //                    {
        //                        Global.VisInspectResult = resultstatus.NG.ToString();
        //                        m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcVisFail, FailType = "ExceedUpperLimit" });
        //                    }
        //                    else
        //                    {
        //                        Global.VisInspectResult = resultstatus.OK.ToString();
        //                        m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcVisCont, ContType = "TriggerCodeReader" });
        //                    }
        //                }
        //                else
        //                {
        //                    Global.VisInspectResult = resultstatus.NG.ToString();
        //                    m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcVisFail, FailType = "WrongOrientation" });
        //                }
        //            }
        //            VisionImg();
        //            m_Events.GetEvent<TopVisionResultEvent>().Publish(); //Publish Vision Result
        //            m_Events.GetEvent<BoxChecking>().Publish();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MachineBase.ShowMessage(ex);
        //        m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = m_Title, MsgType = LogMsgType.Info, MsgText = " Exception during Insight-ResultChanged Vision connection state:" + " " + m_InsightV1.State.ToString() });
        //    }
        //}

        private void InsightV1_StateChanged(object sender, CvsStateChangedEventArgs e)
        {
            OnRequestVisConnEvent();
            m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity { DisplayView = m_Title, MsgType = LogMsgType.Info, MsgText = " Vision connection state:" + " " + m_InsightV1.State.ToString() });
        }
#endregion
    }
}
