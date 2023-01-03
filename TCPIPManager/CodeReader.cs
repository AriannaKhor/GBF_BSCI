using Cognex.DataMan.SDK;
using Cognex.DataMan.SDK.Utils;
using ConfigManager;
using CsvHelper;
using CsvHelper.Configuration;
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
using System.Globalization;
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
    public class CodeReader :ICodeReader
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
        private object _currentResultInfoSyncLock = new object();
        public IShowDialog m_ShowDialog;
        private CultureResources m_CultureResources;
        public event Action<IDialogResult> RequestClose;
        private bool temp = false;
        private ResultsDatalog m_resultsDatalog = new ResultsDatalog();

        #endregion

        #region Constructor
        public CodeReader(IEventAggregator eventAggregator, ResultsDatalog resultsDatalog)
        {
            m_Events = eventAggregator;

            m_SystemConfig = (SystemConfig)ContainerLocator.Container.Resolve(typeof(SystemConfig));
#if !SIMULATION
            m_Events.GetEvent<RequestCodeReaderConnectionEvent>().Subscribe(ConnectCodeReader);
#endif
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
                m_CodeReader.Disconnect();
                m_CodeReader.Connect(); // Uncomment it when connected with the code reader
                m_CodeReader.SetResultTypes(requested_result_types);
                
            }
            catch (Exception ex)
            {
                m_Events.GetEvent<OnCodeReaderDisconnectedEvent>().Publish();
            }
        }

        public void AnalyseResult(string returnedresult)
        {
            try
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

                    if (Global.LotInitialBatchNo == string.Empty)
                    {
                        Global.LotInitialBatchNo = Global.CurrentBatchNum;
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
                                Global.OverallResult = Global.CodeReaderResult;
                                m_Events.GetEvent<OnCodeReaderEndResultEvent>().Publish();
                                m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcCodeReaderFail, FailType = "ExceedTotalBatchQty" });

                            }
                            //OK result
                            else
                            {
                                Global.CodeReaderResult = resultstatus.OK.ToString();
                                Global.OverallResult = Global.CodeReaderResult;
                                m_Events.GetEvent<OnCodeReaderEndResultEvent>().Publish();

                                if (Global.AccumulateCurrentBatchQuantity == Global.LotInitialTotalBatchQuantity)
                                {
                                    ButtonResult dialogResult = m_ShowDialog.Show(DialogIcon.Question, GetDialogTableValue("EndLot"), GetDialogTableValue("AskConfirmEndLot") + " " + Global.LotInitialBatchNo, ButtonResult.No, ButtonResult.Yes);

                                    if (dialogResult == ButtonResult.Yes)
                                    {
                                        m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.EndLotComp });
                                        m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = "Endlot" + Global.CurrentBatchNum });
                                        m_Events.GetEvent<MachineState>().Publish(MachineStateType.Idle);
                                    }
                                    else if (dialogResult == ButtonResult.No)
                                    {
                                        Global.CodeReaderResult = resultstatus.NG.ToString();
                                        Global.OverallResult = Global.CodeReaderResult;
                                        m_Events.GetEvent<OnCodeReaderEndResultEvent>().Publish();
                                        m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcCodeReaderFail, FailType = "ExceedTotalBatchQty" });
                                    }
                                }
                                else
                                {
                                    SaveGlobalResult();
                                    m_Events.GetEvent<ResultLoggingEvent>().Publish(m_resultsDatalog);
                                    m_resultsDatalog.ClearAll();
                                    ButtonResult dialogResult = m_ShowDialog.Show(DialogIcon.Question, GetDialogTableValue("PassResult"), GetDialogTableValue("OKResult"), ButtonResult.OK, ButtonResult.Cancel);
                                    if (dialogResult == ButtonResult.OK)
                                    {
                                        m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcCodeReaderCont });
                                        CloseDialog("");
                                    }
                                    else if (dialogResult == ButtonResult.Cancel)
                                    {
                                        m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.EndLotComp });
                                        m_Events.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = "Endlot" + Global.CurrentBatchNum });
                                        m_Events.GetEvent<MachineState>().Publish(MachineStateType.Idle);
                                        ResetCounter();
                                        CloseDialog("");
                                    }
                                }
                            }
                        }
                        //Unequal Box Quantity
                        else
                        {
                            Global.CodeReaderResult = resultstatus.NG.ToString();
                            Global.OverallResult = Global.CodeReaderResult;
                            m_Events.GetEvent<OnCodeReaderEndResultEvent>().Publish();
                            m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcCodeReaderFail, FailType = "BoxQtyNotMatch" });
                        }
                    }
                    //Incorrect Batch No
                    else
                    {
                        Global.CodeReaderResult = resultstatus.NG.ToString();
                        Global.OverallResult = Global.CodeReaderResult;
                        m_Events.GetEvent<OnCodeReaderEndResultEvent>().Publish();
                        m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcCodeReaderFail, FailType = "BatchNotMatch" });
                    }
                }
                //Missing Result
                else
                {
                    Global.CodeReaderResult = resultstatus.NG.ToString();
                    Global.OverallResult = Global.CodeReaderResult;
                    m_Events.GetEvent<OnCodeReaderEndResultEvent>().Publish();
                    m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcCodeReaderCont });

                }
                temp = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
            
        }

        private void SaveGlobalResult()
        {
            m_resultsDatalog.UserId = Global.UserId;
            m_resultsDatalog.UserLvl = Global.UserLvl;
            DateTime currentTime = DateTime.Now;
            DateTimeFormatInfo dateFormat = new DateTimeFormatInfo();
            dateFormat.ShortDatePattern = "dd-MM-yyyy";
            m_resultsDatalog.Date = currentTime.ToString("d", dateFormat);
            m_resultsDatalog.Time = currentTime.ToString("HH:mm:ss.fff", DateTimeFormatInfo.InvariantInfo);
            m_resultsDatalog.Timestamp = m_resultsDatalog.Date + " | " + m_resultsDatalog.Time;
            m_resultsDatalog.CodeReader = inspectiontype.CodeReader.ToString();
            m_resultsDatalog.DecodeBatchQuantity = Global.CurrentBatchQuantity;
            m_resultsDatalog.DecodeBoxQuantity = Global.CurrentBoxQuantity;
            m_resultsDatalog.DecodeAccuQuantity = Global.AccumulateCurrentBatchQuantity;
            m_resultsDatalog.OverallResult = Global.OverallResult;
            m_resultsDatalog.TopVision = inspectiontype.TopVision.ToString();
            m_resultsDatalog.VisTotalPrdQty = Global.VisProductQuantity;
            m_resultsDatalog.VisCorrectOrient = Global.VisProductCrtOrientation;
            m_resultsDatalog.VisWrongOrient = Global.VisProductWrgOrientation;
            m_resultsDatalog.ErrorMessage = Global.ErrorMsg;
            m_resultsDatalog.Remarks = Global.Remarks;
            m_resultsDatalog.ApprovedBy = Global.CurrentApprovalLevel;
        }

        public void TriggerCodeReader()
        {
            try
            {
#if SIMULATION
                m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcCodeReaderCont });
#else
                m_CodeReader.SendCommand("TRIGGER ON");
#endif
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
            #region Code Reader
            Global.CurrentContainerNum = String.Empty;
            Global.CurrentBatchQuantity = 0;
            Global.AccumulateCurrentBatchQuantity = 0;
            Global.CurrentBoxQuantity = 0;
            Global.CurrentBatchNum = String.Empty;
            #endregion

            #region Top Vision
            Global.VisProductQuantity = 0f;
            Global.VisProductCrtOrientation = 0f;
            Global.VisProductWrgOrientation = 0f;
            Global.TopVisionEndLot = true;
            Global.CodeReaderEndLot = true;
            m_Events.GetEvent<TopVisionResultEvent>().Publish();
            m_Events.GetEvent<OnCodeReaderEndResultEvent>().Publish();
            #endregion
        }

        public void CloseDialog(string parameter)
        {
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
            if (temp == false)
            {
                temp = true;
                string returnedresult = ShowResult(complexResult);
                AnalyseResult(returnedresult);
            }
        }
#endregion
    }
}
