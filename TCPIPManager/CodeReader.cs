using Cognex.DataMan.SDK;
using Cognex.DataMan.SDK.Utils;
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
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace TCPIPManager
{
    public class CodeReader : ICodeReader
    {
        #region Variable
        private bool canContAnalyse = false;
        public SystemConfig m_SystemConfig;
        private IPAddress codereaderIp;
        private CodeReaderDisplayControl formCodeReader = new CodeReaderDisplayControl();
        private DataManSystem m_CodeReader = null;
        private ResultCollector m_CodeReaderResults;
        private ISystemConnector m_CodeReaderconnector = null;
        private readonly IEventAggregator m_Events;
        private ObservableCollection<string> m_ContainerCollection = new ObservableCollection<string>();
        private FixedSizeObservableCollection<Datalog> m_SoftwareResultCollection = new FixedSizeObservableCollection<Datalog>();
        private object _currentResultInfoSyncLock = new object();
        #endregion

        #region Constructor
        public CodeReader(IEventAggregator eventAggregator)
        {
            m_Events = eventAggregator;

            m_SystemConfig = (SystemConfig)ContainerLocator.Container.Resolve(typeof(SystemConfig));

            m_Events.GetEvent<RequestCodeReaderConnectionEvent>().Subscribe(ConnectCodeReader);


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
                    string Container = m_ContainerCollection.Where(key => key == Global.CurrentContainerNum).FirstOrDefault();

                    if (Container == null || Global.CodeReaderRetry)
                    {
                        if (!Global.CodeReaderRetry)
                        {
                            m_ContainerCollection.Add(Global.CurrentContainerNum);
                        }

                        if (Global.CurrentBoxQuantity == Global.VisProductQuantity)
                        {
                            Global.AccumulateCurrentBatchQuantity = Global.AccumulateCurrentBatchQuantity + Global.CurrentBoxQuantity;

                            if (Global.AccumulateCurrentBatchQuantity > Global.LotInitialTotalBatchQuantity)
                            {
                                Global.CodeReaderResult = resultstatus.Fail.ToString();
                                m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcFail, FailType = "ExceedTotalBatchQty" });
                            }
                            else
                            {
                                Global.CodeReaderResult = resultstatus.Pass.ToString();
                                m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcCont });
                            }
                        }
                        else
                        {
                            Global.CodeReaderResult = resultstatus.Fail.ToString();
                            m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcFail, FailType = "BoxQtyNotMatch" });
                        }
                    }
                    else
                    {
                        Global.CodeReaderResult = resultstatus.Fail.ToString();
                        m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcFail, FailType = "ContainerNumberExist" });
                    }
                }
                else
                {
                    Global.CodeReaderResult = resultstatus.Fail.ToString();
                    m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcFail, FailType = "BatchNotMatch" });
                }
            }
            else
            {
                Global.CodeReaderResult = resultstatus.Fail.ToString();
                m_Events.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcFail, FailType = "MissingResult" });
            }

            m_SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, " Code Reader Result :" + Global.CodeReaderResult + ":" + Global.VisProductQuantity));
            m_SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, " Code Reader Result :" + Global.CodeReaderResult + ":" + Global.CurrentContainerNum));
            m_SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, " Code Reader Result :" + Global.CodeReaderResult + ":" + Global.CurrentBatchQuantity));
            m_SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, " Code Reader Result :" + Global.CodeReaderResult + ":" + Global.AccumulateCurrentBatchQuantity));
            m_SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, " Code Reader Result :" + Global.CodeReaderResult + ":" + Global.CurrentBoxQuantity));
            m_Events.GetEvent<OnCodeReaderEndResultEvent>().Publish();

        }
        public void TriggerCodeReader()
        {
            try
            {
                m_CodeReader.SendCommand("TRIGGER ON");
                Thread.Sleep(2000);
                m_CodeReader.SendCommand("TRIGGER OFF");
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
                return read_result;

            }

            if (images.Count > 0)
            {
                Image first_image = images[0];
                //Image fitted_image = Gui.ResizeImageToBitmap(first_image, );
            }


            //if (images.Count > 0)
            //{
            //    Image first_image = images[0];

            //    Size image_size = Gui.FitImageInControl(first_image.Size, picResultImage.Size);
            //    Image fitted_image = Gui.ResizeImageToBitmap(first_image, image_size);

            //    if (image_graphics.Count > 0)
            //    {
            //        using (Graphics g = Graphics.FromImage(fitted_image))
            //        {
            //            foreach (var graphics in image_graphics)
            //            {
            //                ResultGraphics rg = GraphicsResultParser.Parse(graphics, new Rectangle(0, 0, image_size.Width, image_size.Height));
            //                ResultGraphicsRenderer.PaintResults(g, rg);
            //            }
            //        }
            //    }

            //    if (picResultImage.Image != null)
            //    {
            //        var image = picResultImage.Image;
            //        picResultImage.Image = null;
            //        image.Dispose();
            //    }

            //    picResultImage.Image = fitted_image;
            //    picResultImage.Invalidate();
            //}
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
