using Cognex.DataMan.SDK;
using Cognex.DataMan.SDK.Utils;
using ConfigManager;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Helpers;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using GreatechApp.Services.Utilities;
using Prism.Events;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Xml;

namespace TCPIPManager
{
    public class CodeReader : ICodeReader
    {
        #region Variable
        private bool canContAnalyse = false;
        public SystemConfig m_SystemConfig;
        private IPAddress codereaderIp;
        private DataManSystem m_CodeReader = null;
        private ResultCollector m_CodeReaderResults;
        private ISystemConnector m_CodeReaderconnector = null;
        private readonly IEventAggregator m_Events;
        private ObservableCollection<string> m_ContainerCollection = new ObservableCollection<string>();
        private FixedSizeObservableCollection<Datalog> m_SoftwareResultCollection = new FixedSizeObservableCollection<Datalog>();
        private object _currentResultInfoSyncLock = new object();
        public IShowDialog m_ShowDialog;
        private CultureResources m_CultureResources;
        public event Action<IDialogResult> RequestClose;
        #endregion

        #region Constructor
        public CodeReader(IEventAggregator eventAggregator)
        {
            m_Events = eventAggregator;

            m_SystemConfig = (SystemConfig)ContainerLocator.Container.Resolve(typeof(SystemConfig));

            m_Events.GetEvent<RequestCodeReaderConnectionEvent>().Subscribe(ConnectCodeReader);
            m_ShowDialog = (IShowDialog)ContainerLocator.Container.Resolve(typeof(IShowDialog));
            m_CultureResources = (CultureResources)ContainerLocator.Container.Resolve(typeof(CultureResources));
        }
        #endregion

        #region Method
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

                //// Subscribe to events that are signalled when the device sends auto-responses.
                ResultTypes requested_result_types = ResultTypes.ReadXml | ResultTypes.Image | ResultTypes.ImageGraphics;
                m_CodeReaderResults = new ResultCollector(m_CodeReader, requested_result_types);
                m_CodeReaderResults.ComplexResultCompleted += Results_ComplexResultCompleted;
                m_CodeReader.SetKeepAliveOptions(false, 3000, 1000);
                m_CodeReader.Connect(); // Uncomment it when connected with the code reader

                //ShowLiveView(m_CodeReader, m_Events);

                try
                {
                    m_CodeReader.SetResultTypes(requested_result_types);
                }
                catch
                { }
            }
            catch (Exception ex)
            {
                Global.CodeReaderConnStatus = ConnectionState.Disconnected.ToString();
                MachineBase.ShowMessage(ex);
            }
        }
        public void AnalyseResult(string returnedresult)
        {
            string[] splitedresult = returnedresult.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            if (splitedresult.Length == 5)
            {
                Global.CurrentContainerNum = splitedresult[0];
                Global.CurrentBatchQuantity = Int32.Parse(splitedresult[1]);
                Global.CurrentMatl = splitedresult[2];
                Global.CurrentBoxQuantity = Int32.Parse(splitedresult[3]);
                Global.CurrentBatchNum = splitedresult[4];


                if (Global.LotInitialTotalBatchQuantity == 0)
                {
                    Global.LotInitialTotalBatchQuantity = Global.CurrentBatchQuantity;
                }
                if (Global.CurrentBatchNum == Global.LotInitialBatchNo)
                {
                    if (Global.CurrentBoxQuantity == Global.VisProductQuantity)
                    {
                        Global.AccumulateCurrentBatchQuantity = Global.AccumulateCurrentBatchQuantity + Global.CurrentBoxQuantity;
                        // Exceed Total Batch Quantity
                        if (Global.AccumulateCurrentBatchQuantity > Global.LotInitialTotalBatchQuantity)
                        {
                            Global.CodeReaderResult = resultstatus.NG.ToString();
                            m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CodeReaderSeq, MachineOpr = MachineOperationType.ProcFail, FailType = "ExceedTotalBatchQty" });
                        }
                        //OK result
                        else if (Global.CodeReaderResult == "OK")
                        {
                            Global.CodeReaderResult = resultstatus.OK.ToString();
                            m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CodeReaderSeq, MachineOpr = MachineOperationType.ProcCont });

                            if (Global.VisOverallResult == "OK" && Global.CodeReaderResult == "OK")
                            {
                                Global.VisOverallResult = resultstatus.OK.ToString();
                                ButtonResult dialogResult = m_ShowDialog.Show(DialogIcon.Question, GetDialogTableValue("PassResult"), GetDialogTableValue("OKResult"), ButtonResult.OK);
                                CloseDialog("");
                                ResetCounter();
                            }
                        }
                    }
                    //Unequal Box Quantity
                    else
                    {
                        Global.CodeReaderResult = resultstatus.NG.ToString();
                        m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CodeReaderSeq, MachineOpr = MachineOperationType.ProcFail, FailType = "BoxQtyNotMatch" });
                    }
                }
                //Incorrect Batch No
                else
                {
                    Global.CodeReaderResult = resultstatus.NG.ToString();
                    m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcFail, FailType = "BatchNotMatch" });
                }
            }
            //Missing Result
            else
            {
                Global.CodeReaderResult = resultstatus.NG.ToString();
                m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CodeReaderSeq, MachineOpr = MachineOperationType.ProcFail, FailType = "MissingResult" });
            }

            m_SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, " Code Reader Result :" + Global.CodeReaderResult + ":" + Global.VisProductQuantity));
            m_SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, " Code Reader Result :" + Global.CodeReaderResult + ":" + Global.CurrentContainerNum));
            m_SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, " Code Reader Result :" + Global.CodeReaderResult + ":" + Global.CurrentBatchQuantity));
            m_SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, " Code Reader Result :" + Global.CodeReaderResult + ":" + Global.AccumulateCurrentBatchQuantity));
            m_SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, " Code Reader Result :" + Global.CodeReaderResult + ":" + Global.CurrentBoxQuantity));

            m_SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, " Code Reader Result :" + Global.VisInspectResult));
            m_Events.GetEvent<OnCodeReaderEndResultEvent>().Publish();
        }
        public void TriggerCodeReader()
        {
            try
            {
                m_CodeReader.SendCommand("TRIGGER ON");
                //Thread.Sleep(2000);
                //m_CodeReader.SendCommand("TRIGGER OFF");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to send TRIGGER ON/OFF commands: " + ex.ToString());
            }
        }

        private string GetReadStringFromResultXml(string resultXml)
        {
            try
            {
                XmlDocument doc = new XmlDocument();

                doc.LoadXml(resultXml);

                XmlNode full_string_node = doc.SelectSingleNode("result/general/full_string");

                if (full_string_node != null && m_CodeReader != null && m_CodeReader.State == Cognex.DataMan.SDK.ConnectionState.Connected)
                {
                    XmlAttribute encoding = full_string_node.Attributes["encoding"];
                    if (encoding != null && encoding.InnerText == "base64")
                    {
                        if (!string.IsNullOrEmpty(full_string_node.InnerText))
                        {
                            byte[] code = Convert.FromBase64String(full_string_node.InnerText);
                            return m_CodeReader.Encoding.GetString(code, 0, code.Length);
                        }
                        else
                        {
                            return "";
                        }
                    }

                    return full_string_node.InnerText;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to send TRIGGER ON/OFF commands: " + ex.ToString());
            }

            return "";
        }

        private void ResetCounter()
        {
            Global.CodeReaderProceedNewBox = true;
            Global.TopVisionProceedNewBox = true;

            #region Code Reader
            Global.CurrentContainerNum = String.Empty;
            Global.CurrentBatchQuantity = 0;
            Global.CurrentBoxQuantity = 0;
            Global.CurrentBatchNum = String.Empty;
            #endregion

            #region Top Vision
            Global.VisProductQuantity = 0f;
            Global.VisProductCrtOrientation = String.Empty;
            Global.VisProductWrgOrientation = String.Empty;
            #endregion

            m_Events.GetEvent<TopVisionResultEvent>().Publish();
            m_Events.GetEvent<OnCodeReaderEndResultEvent>().Publish();
        }

        public void CloseDialog(string parameter)
        {
            //m_TmrButtonMonitor.Stop();
            // Turn off Reset Button LED
            //m_IO.WriteBit(ResetButtonIndic, false);
            RaiseRequestClose(new DialogResult(ButtonResult.OK));
        }
        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }


        private string GetDialogTableValue(string key)
        {
            return m_CultureResources.GetDialogValue(key);
        }

        public string ShowResult(ComplexResult complexResult)
        {
            List<Image> images = new List<Image>();
            List<string> image_graphics = new List<string>();
            string read_result = null;
            int result_id = -1;
            ResultTypes collected_results = ResultTypes.None;

            // Take a reference or copy values from the locked result info object. This is done
            // so that the lock is used only for a short period of time.
            lock (_currentResultInfoSyncLock)
            {
                foreach (var simple_result in complexResult.SimpleResults)
                {
                    collected_results |= simple_result.Id.Type;

                    switch (simple_result.Id.Type)
                    {
                        case ResultTypes.Image:
                            Image image = ImageArrivedEventArgs.GetImageFromImageBytes(simple_result.Data);
                            if (image != null)
                                images.Add(image);
                            break;

                        case ResultTypes.ImageGraphics:
                            image_graphics.Add(simple_result.GetDataAsString());
                            break;

                        case ResultTypes.ReadXml:
                            read_result = GetReadStringFromResultXml(simple_result.GetDataAsString());
                            result_id = simple_result.Id.Id;
                            break;

                        case ResultTypes.ReadString:
                            read_result = simple_result.GetDataAsString();
                            result_id = simple_result.Id.Id;
                            break;
                    }
                }

                if (images.Count > 0)
                {
                    Image first_image = images[0];

                    //original way
                    //Bitmap fitted_image = ResizeImage(first_image, 50, 50); ;
                    //BitmapImage converttobitmapimg = Bitmap2BitmapImage(fitted_image);
                    //m_Events.GetEvent<CodeReaderImage>().Publish(converttobitmapimg);

                    //ky's way
                    BitmapImage liveimage;
                    Bitmap dImg = new Bitmap(first_image);
                    MemoryStream ms = new MemoryStream();
                    dImg.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    BitmapImage bImg = new BitmapImage();
                    bImg.BeginInit();
                    bImg.StreamSource = new MemoryStream(ms.ToArray());
                    bImg.EndInit();
                    bImg.Freeze();
                    liveimage = bImg;
                    m_Events.GetEvent<CodeReaderImage>().Publish(liveimage);
                }

                return read_result;
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

        public static void ShowLiveView(DataManSystem m_CodeReader, IEventAggregator m_Events)
        {
            //m_CodeReader.SendCommand("SET LIVEIMG.MODE 2");

            BitmapImage liveimage;
            Image tempImage = m_CodeReader.GetLiveImage(Cognex.DataMan.SDK.ImageFormat.bitmap, ImageSize.Quarter, ImageQuality.Medium);


            Bitmap dImg = new Bitmap(tempImage);
            MemoryStream ms = new MemoryStream();
            dImg.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            BitmapImage bImg = new BitmapImage();
            bImg.BeginInit();
            bImg.StreamSource = new MemoryStream(ms.ToArray());
            bImg.EndInit();

            liveimage = bImg;

            m_Events.GetEvent<CodeReaderImage>().Publish(liveimage);
        }

        #endregion

        #region Events
        private void OnSystemConnected(object sender, System.EventArgs args)
        {
            m_Events.GetEvent<OnCodeReaderConnectedEvent>().Publish();
        }
        private void OnSystemDisconnected(object sender, System.EventArgs args)
        {
            m_Events.GetEvent<OnCodeReaderDisconnectedEvent>().Publish();
        }
        private void Results_ComplexResultCompleted(object sender, ComplexResult complexResult)
        {
            string returnedresult = ShowResult(complexResult);
            AnalyseResult(returnedresult);
        }
        #endregion
    }
}
