using ConfigManager;
using GreatechApp.Core.Events;
using GreatechApp.Core.Modal;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecsGemManager
{
    public class SecsGemBase : ISecsGem
    {
        GemMsgQue m_MsgQue = new GemMsgQue();
        GemDatabase m_DataBase = new GemDatabase();

        private readonly IEventAggregator m_eventAggregator;
        public SystemConfig m_SystemConfig;
        public SecsGemConfig m_GemCfg;
        public SecsGemRecipeConfig m_SecsGemrcpCfg;
        public SecsGemBase m_SesGemControl;
        public SecsGem m_SGVM;

        private string ConnPath;
        private bool EnableSGComm;
        private bool IsConnected;


        

        public SecsGemBase(IEventAggregator eventAggregator, SecsGemConfig GemCfg, SystemConfig SystemConfig, SecsGem SGVM)
        {
            m_eventAggregator = eventAggregator;
            m_GemCfg = GemCfg;
            m_SystemConfig = SystemConfig;
            m_SGVM = SGVM;

            m_MsgQue.ReceiveQueMessage += MsgQue_ReceiveQueMessage;
            m_MsgQue.Start();

            m_MsgQue.SendMessage("S1F13", "");
            m_MsgQue.SendMessage("S1F1", "");

            m_GemCfg = SecsGemConfig.Open(m_SystemConfig.GemRef[0].Reference);
            ConnPath = m_GemCfg.GPCollection[0].DatabaseDir;

            LoadData();
            LoadInitCommunicationState();
            LoadInitControlState();
            LoadInitProcessState();
            LoadInitSpoolingState();

            if (EnableSGComm && IsConnected)
            {
                m_eventAggregator.GetEvent<SecsGemCommState>().Publish("Communicating");
            }
            else
            {
                m_eventAggregator.GetEvent<SecsGemCommState>().Publish("Not Communicating");
            }
        }

        private bool CheckSecsGemDatabasePath()
        {
            if (File.Exists(ConnPath))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        private bool CheckSecsGemConfigPath()
        {
            if (File.Exists(ConnPath))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        private void LoadData()
        {
            if (CheckSecsGemDatabasePath())
            {
                LoadDatabase();
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Database Path Cannot Found" + Environment.NewLine + "Please Click on 'Load Database' to Load the Database", "Equipment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };

            if (CheckSecsGemConfigPath())
            {
                LoadConfig();
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Secs Gem Recipe Path Cannot Found" + Environment.NewLine + "Please Click on 'Load Database' to Load the Database", "Equipment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };
        }

        public void LoadDatabase()
        {
            m_DataBase = (GemDatabase)ContainerLocator.Container.Resolve(typeof(GemDatabase));
            SecsGemConfig.Open(m_SystemConfig.GemRef[0].Reference).ToString();

            m_SGVM = new SecsGem();

            m_SGVM.DbDir = m_SystemConfig.GemRef[0].Reference;
            
            m_DataBase.LoadConnProperties();
            m_DataBase.LoadSGConnection();
            m_DataBase.LoadTimeOuts();

            EnableSGComm = m_DataBase.enablesgconn;
            m_SGVM.DeviceID = Convert.ToInt32(m_DataBase.deviceId);
            m_SGVM.IpAddress = m_DataBase.IpAddress;
            m_SGVM.PortID = Convert.ToInt32(m_DataBase.portID);
            m_SGVM.t3 = Convert.ToInt32(m_DataBase.t3);
            m_SGVM.t5 = Convert.ToInt32(m_DataBase.t5);
            m_SGVM.t6 = Convert.ToInt32(m_DataBase.t6);
            m_SGVM.t7 = Convert.ToInt32(m_DataBase.t7);
            m_SGVM.t8 = Convert.ToInt32(m_DataBase.t8);
            m_eventAggregator.GetEvent<LoadSecsGem>().Publish(m_SGVM);

        }
        public void LoadConfig()
        {
            SecsGemRecipeConfig.Open(m_SystemConfig.SecsGemRcpRef[0].Reference).ToString();
            m_SGVM.RcpDir = m_SystemConfig.SecsGemRcpRef[0].Reference;
            m_eventAggregator.GetEvent<LoadSecsGem>().Publish(m_SGVM);
        }
        public void MsgQue_ReceiveQueMessage(string label, string body)
        {
            string[] splittedMsg = label.Split(':');
            switch (splittedMsg[0])
            {
                case "MIntStart":
                    // Update equipment variable data into HostDataBase.mdb
                    break;
                case "S1F13":
                    ReceivedS1F13(splittedMsg[1]);
                    break;
                case "S1F14":
                    ReceivedS1F14(splittedMsg[1]);
                    break;
                case "S1F15":
                    UpdateControlState("Offline");
                    break;
                case "S1F17":
                    if (m_DataBase.ReadVIDValue("ControlState") == "5")
                        UpdateControlState("OnlineRemote");
                    else
                        UpdateControlState("OnlineLocal");
                    break;
                case "S2F15":
                    ReceivedEC_Changes(body);
                    break;
                case "S2F21":
                    ReceivedRemoteCommand(splittedMsg[1]);
                    break;
                case "S2F41":
                    ReceivedHostCommand(splittedMsg[1]);
                    break;
                case "S2F49":
                    ReceivedEnhancedCommand(splittedMsg[1]);
                    break;
                case "S7F3":
                    UpdateReceivedRecipe("S7F3", splittedMsg[1]);
                    break;
                case "S7F6":
                    UpdateReceivedRecipe("S7F6", splittedMsg[1]);
                    break;
                case "S7F17":
                    ReceivedRecipeDeleteRequest(splittedMsg[1]);
                    break;
                case "S10F3":
                    UpdateReceivedTerminalTextBox(body);
                    break;
                case "S10F5":
                    UpdateReceivedTerminalTextBox(body);
                    break;
                default:
                    break;
            }
        }

        private void ReceivedS1F13(string Connect)
        {
            if (Connect == "TRUE")
            {
                UpdateConnectionState(true);
                IsConnected = true;
            }

            else
            {
                UpdateConnectionState(false);
                IsConnected = false;
            }
              
        }
        private void ReceivedS1F14(string Connect)
        {
            if (Connect == "TRUE")
            {
                UpdateConnectionState(true);
                IsConnected = true;
            }

            else
            {
                UpdateConnectionState(false);
                IsConnected = false;
            }
               
        }

        public void LoadInitCommunicationState()
        {
            string CommState = m_DataBase.ReadVIDValue("CommState");
            switch (CommState)
            {
                case "2":
                    UpdateConnectionState(true);
                    break;
                default:
                    UpdateConnectionState(false);
                    break;
            }
        }

        public void LoadInitProcessState()
        {
            string ProcID = m_DataBase.ReadVIDValue("ProcessState");
            UpdateProcessState(ProcID);
        }

        public void LoadInitControlState()
        {
            string ControlState = m_DataBase.ReadVIDValue("ControlState");
            switch (ControlState)
            {
                case "3":
                    // RadioButton_CS_Offline.Checked = true;
                    m_eventAggregator.GetEvent<SecsGemCntrlState>().Publish("Offline");
                    m_eventAggregator.GetEvent<SecsGemCommState>().Publish("Not Communicating");
                    IsConnected = false;

                    break;
                case "4":
                    // RadioButton_CS_OnlineLocal.Checked = true;
                    m_eventAggregator.GetEvent<SecsGemCntrlState>().Publish("OnlineLocal");
                    IsConnected = true;
                    break;
                case "5":
                    // RadioButton_CS_OnlineRemote.Checked = true;
                    m_eventAggregator.GetEvent<SecsGemCntrlState>().Publish("OnlineRemote");
                    IsConnected = true;
                    break;
                default:
                    break;
            }
        }

        public void LoadInitSpoolingState()
        {
            string SpoolingState = m_DataBase.ReadVIDValue("ConfigSpool");
            switch (SpoolingState)
            {
                case "0":
                    // RadioButton_SpoolingDisable.Checked = true;
                    break;
                case "1":
                    // RadioButton_SpoolingEnable.Checked = true;
                    break;
                default:
                    break;
            }

        }
        public void UpdateConnectionState(bool Connected)
        {
            if (Connected)
            {

                m_eventAggregator.GetEvent<SecsGemCommState>().Publish("Communicating");
            }
            else
            {
                m_eventAggregator.GetEvent<SecsGemCommState>().Publish("Not Communicating");
            }
        }

        public void UpdateControlState(string ControlState)
        {
            m_eventAggregator.GetEvent<SecsGemCntrlState>().Publish(ControlState);
        }

        public void ReceivedEC_Changes(string AllEC)
        {
            // EAC:
            // 0 = Acknowledge
            // 1 = Denied. At least one constant does not exist (**Handled by M-Int)
            // 2 = Denied. Busy
            // 3 = Denied. At least one constant out of range
            // >3 = Other equipment specific error
            // 4-63 = Reserved

            string EAC;
            string[] ECList = AllEC.Split(',');
            string msg = "Host request to update the below EC values: " + Environment.NewLine;
            foreach (string newECVal in ECList)
            {
                string[] EC_Content = newECVal.Split(':');
                msg += EC_Content[0] + " --> " + EC_Content[1] + Environment.NewLine;
            }

            DialogResult dialogResult = MessageBox.Show(msg, "Equipment Constant Changes", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                foreach (string newECVal in ECList)
                {
                    string[] EC_Content = newECVal.Split(':');
                    m_DataBase.UpdateVIDtoTable(EC_Content[1], Convert.ToInt32(EC_Content[0]));
                }

                EAC = "0";
                m_MsgQue.SendMessage("S2F16:" + EAC, "");
                m_MsgQue.SendMessage("ECChange", "");

            }
            else if (dialogResult == DialogResult.No)
            {
                EAC = "2";
                m_MsgQue.SendMessage("S2F16:" + EAC, "");
            }
        }


        public void UpdateProcessState(string ProcID)
        {
            string ProcessName = m_DataBase.ReadTableValue("Process", "Name", "StateValue", ProcID.ToString());
        }

        public void UpdateReceivedRecipe(string StreamFunction, string RecipeName)
        {

        }

        public void UpdateReceivedTerminalTextBox(string Message)
        {
            string displayText = "";

            string[] splittedMsg = Message.Split('|'); // In case received message is in multi-lines (S10F5)
            foreach (string text in splittedMsg)
            {
                displayText += text;
                displayText += Environment.NewLine;
            }
            m_eventAggregator.GetEvent<ReceivedHostMsg>().Publish(displayText);


        }
        // REMOTE COMMAND -------------------------------------------------------------------------->
        public void ReceivedRemoteCommand(string RCMD)
        {
            //TB_RCMD.BeginInvoke(new Action(() => { TB_RCMD.Text = RCMD; }));
            //BlinkingTextBoxAcknowledge(TB_RCMD);
        }

        public void ReceivedHostCommand(string RCMD)
        {
            // HCACK:
            // 0 = Acknowledge, command has been performed
            // 1 = Command does not exist (**Handled by M-Int)
            // 2 = Cannot perform now
            // 3 = At least one parameter is invalid (**Handled by M-int only if wrong format given)
            // 4 = Acknowledge, command will be performed with completion signaled later by an event
            // 5 = Rejected, already in desired condition
            // 6 = No such object exists
            // 7-63 = Reserved

            // CPACK:
            // 1 = Parameter Name (CPNAME) does not exist
            // 2 = Illegal Value specified for CPVAL
            // 3 = Illegal Format specified for CPVAL
            // >3 = Other equipment specific error
            // 4-63 = Reserved

            //TB_RCMD.BeginInvoke(new Action(() => { TB_RCMD.Text = RCMD; }));
            //BlinkingTextBoxAcknowledge(TB_RCMD);

            //using (var frmRCMD = new frmRCMD(RCMD, "S2F41"))
            //{
            //    var result = frmRCMD.ShowDialog();
            //    if (result == DialogResult.OK)
            //    {
            //        string HCACK = frmRCMD.HCACK;
            //        string CPACKList = frmRCMD.CPResult;

            //        m_MsgQue.SendMessage("S2F42:" + HCACK, CPACKList);


            // Equipment can specific the CPACK that going to be send back in S2F42
            // For example, if your HCACK = "2", CPACKList = "USER:2,PASSWORD:2"
            // The reply format will looks like below:
            // < L2
            //    < B 2 >
            //    < L2
            //       < L2
            //          < A "USER" >
            //          < B 2 >
            //       >
            //       < L2
            //          < A "PASSWORD" >
            //          < B 2 >
            //       >
            //    >
            // >
        }

        public void ReceivedEnhancedCommand(string RCMD)
        {
            // HCACK:
            // 0 = Acknowledge, command has been performed
            // 1 = Command does not exist (**Handled by M-Int)
            // 2 = Cannot perform now
            // 3 = At least one parameter is invalid (**Handled by M-int only if wrong format given)
            // 4 = Acknowledge, command will be performed with completion signaled later by an event
            // 5 = Rejected, already in desired condition
            // 6 = No such object exists
            // 7-63 = Reserved

            // CEPACK:
            // 0 = No error
            // 1 = Parameter Name (CPNAME) does not exist
            // 2 = Illegal Value specified for CEPVAL
            // 3 = Illegal Format specified for CEPVAL
            // 4 = Parameter name (CPNAME) not valid as used
            // 5-63 = Reserved

            //TB_RCMD.BeginInvoke(new Action(() => { TB_RCMD.Text = RCMD; }));
            //BlinkingTextBoxAcknowledge(TB_RCMD);

            //using (var frmRCMD = new frmRCMD(RCMD, "S2F49"))
            //{
            //    var result = frmRCMD.ShowDialog();
            //    if (result == DialogResult.OK)
            //    {
            //        string HCACK = frmRCMD.HCACK;
            //        string CEPACKList = frmRCMD.CEPResult;

            //        m_MsgQue.SendMessage("S2F50:" + HCACK, CEPACKList);
            //        UpdateMSMQLog("Send", "S2F50:" + HCACK, CEPACKList);

            // Equipment can specific the CEPACK that going to be send back in S2F50
            // For example, if your HCACK = "2", CEPACKList = "USER:2,PASSWORD:2"
            // The reply format will looks like below:
            // < L2
            //    < B 2 >
            //    < L2
            //       < L2
            //          < A "USER" >
            //          < B 2 >
            //       >
            //       < L2
            //          < A "PASSWORD" >
            //          < B 2 >
            //       >
            //    >
            // >
        }
        public void ReceivedRecipeDeleteRequest(string AllFiles)
        {
            // ACKC7:
            // 0 = Accepted
            // 1 = Permission not granted
            // 2 = Length Error
            // 3 = Matrix overflow
            // 4 = PPID not found
            // 5 = Mode unsupportted
            // 6 = Command will be performed with completion signaled later
            // >6 = Other error
            // 7-63 = Reserved

            //TB_PPReceived.BeginInvoke(new Action(() => { TB_PPReceived.Text = "Request Delete: " + AllFiles; }));
            //BlinkingTextBoxAcknowledge(TB_PPReceived);

            //string ACKC7;
            //string path = DataBase.ReadVIDValue("PPDirectory");

            //if (AllFiles == "ALL") // Delete all recipes
            //{
            //    DialogResult dialogResult = MessageBox.Show("Do you want to delete all recipes?", "Process Program Delete", MessageBoxButtons.YesNo);
            //    if (dialogResult == DialogResult.Yes)
            //    {
            //        System.IO.DirectoryInfo dir = new DirectoryInfo(path);
            //        foreach (FileInfo file in dir.GetFiles())
            //            file.Delete();

            //        ACKC7 = "0";
            //        m_MsgQue.SendMessage("S7F18:" + ACKC7, "");
            //        UpdateMSMQLog("Send", "S7F18:" + ACKC7, "");
            //    }
            //    else
            //    {
            //        ACKC7 = "1";
            //        m_MsgQue.SendMessage("S7F18:" + ACKC7, "");
            //        UpdateMSMQLog("Send", "S7F18:" + ACKC7, "");
            //}

            //else // Delete specific recipes
            //{
            //    string[] RecipeList = AllFiles.Split(',');

            //    // Check whether all file exist in the folder
            //    // If your recipes locate at different folder, remember to check all of them
            //    foreach (string recipe in RecipeList)
            //    {
            //        string filepath = Path.Combine(path, recipe);
            //        if (!File.Exists(filepath))
            //        {
            //            ACKC7 = "4";
            //            m_MsgQue.SendMessage("S7F18:" + ACKC7, "");
            //            UpdateMSMQLog("Send", "S7F18:" + ACKC7, "");
            //            return;
            //        }
            //    }

            //    string text = "Do you want to delete the recipes below?" + Environment.NewLine;
            //    foreach (string recipe in RecipeList)
            //        text += recipe + Environment.NewLine;

            //    DialogResult dialogResult = MessageBox.Show(text, "Process Program Delete", MessageBoxButtons.YesNo);
            //    if (dialogResult == DialogResult.Yes)
            //    {
            //        foreach (string recipe in RecipeList)
            //        {
            //            string filepath = Path.Combine(path, recipe);
            //            File.Delete(filepath);
            //        }

            //        ACKC7 = "0";
            //        MsgQue.SendMessage("S7F18:" + ACKC7, "");
            //        UpdateMSMQLog("Send", "S7F18:" + ACKC7, "");
            //    }
            //    else
            //    {
            //        ACKC7 = "1";
            //        MsgQue.SendMessage("S7F18:" + ACKC7, "");
            //        UpdateMSMQLog("Send", "S7F18:" + ACKC7, "");
            //}
        }
    }
}



   




 
 

