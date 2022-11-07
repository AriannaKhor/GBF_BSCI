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
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TCPIPManager
{
    public class CodeReader :ICodeReader
    {
        #region Variable
        public SystemConfig m_SystemConfig;
        private IPAddress codereaderIp;
        private CodeReaderDisplayControl formCodeReader;
        private DataManSystem m_CodeReader = null;
        private ResultCollector m_CodeReaderResults;
        private ISystemConnector m_CodeReaderconnector = null;
        private readonly IEventAggregator m_Events;
        private ObservableCollection<string> m_ContainerCollection;
        private FixedSizeObservableCollection<Datalog> m_SoftwareResultCollection;
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
                Global.CurrentBoxQuantity = Int32.Parse(splitedresult[4]);

                if (Global.LotInitialTotalBatchQuantity == 0)
                {
                    Global.LotInitialTotalBatchQuantity = Global.CurrentBatchQuantity;
                }
                else if (Global.CurrentBatchNum == Global.LotInitialBatchNo)
                {
                    string Container = m_ContainerCollection.Where(key => key == Global.CurrentContainerNum).FirstOrDefault();

                    if (Container == null)
                    {
                        m_ContainerCollection.Add(Global.CurrentContainerNum);

                        if (Global.CurrentBoxQuantity == Global.VisProductQuantity)
                        {
                            Global.AccumulateCurrentBatchQuantity = Global.AccumulateCurrentBatchQuantity + Global.CurrentBoxQuantity;

                            if (Global.AccumulateCurrentBatchQuantity > Global.LotInitialTotalBatchQuantity)
                            {
                                MachineBase.ShowMessage("Current Total Batch Quantity Does Not Match Total Batch Quantity Entered", MachineBase.MessageIcon.Error);
                                Global.CodeReaderResult = resultstatus.Fail.ToString();
                                checkresult = false;
                            }
                            else
                            {
                                checkresult = true;
                                Global.CodeReaderResult = resultstatus.Pass.ToString();
                            }
                        }
                        else
                        {
                            MachineBase.ShowMessage("Current Box Quantity Does Not Match Vision Result", MachineBase.MessageIcon.Error);
                            Global.CodeReaderResult = resultstatus.Fail.ToString();
                            checkresult = false;
                        }
                    }
                    else
                    {
                        MachineBase.ShowMessage("Container Number already Exist", MachineBase.MessageIcon.Error);
                        Global.CodeReaderResult = resultstatus.Fail.ToString();
                        checkresult = false;
                    }
                }
                else
                {
                    MachineBase.ShowMessage("Batch Number does not match", MachineBase.MessageIcon.Error); ;
                    Global.CodeReaderResult = resultstatus.Fail.ToString();
                    checkresult = false;
                }
            }
            else
            {
                MachineBase.ShowMessage("Missing Result", MachineBase.MessageIcon.Error);
                Global.CodeReaderResult = resultstatus.Fail.ToString();
                checkresult = false;
            }

            if (checkresult)
            {
                m_Events.GetEvent<OnCodeReaderEndResultEvent>().Publish();
                m_SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, "Code Reader Result :" + Global.CodeReaderResult + ":" + Global.VisProductQuantity));
                m_SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, "Code Reader Result :" + Global.CodeReaderResult + ":" + Global.CurrentContainerNum));
                m_SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, "Code Reader Result :" + Global.CodeReaderResult + ":" + Global.CurrentBatchQuantity));
                m_SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, "Code Reader Result :" + Global.CodeReaderResult + ":" + Global.AccumulateCurrentBatchQuantity));
                m_SoftwareResultCollection.Add(new Datalog(LogMsgType.Info, "Code Reader Result :" + Global.CodeReaderResult + ":" + Global.CurrentBoxQuantity));
            }
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
            string returnedresult = formCodeReader.ShowResult(complexResult);
            AnalyseResult(returnedresult);
        }


        #endregion
    }
}
