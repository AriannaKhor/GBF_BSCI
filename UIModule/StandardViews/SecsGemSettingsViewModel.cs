using ConfigManager;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using Prism.Commands;
using Prism.Ioc;
using SecsGemManager;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using UIModule.MainPanel;
using System.Xml;
using GreatechApp.Core.Events;

namespace UIModule.StandardViews
{
   public class SecsGemSettingsViewModel : BaseUIViewModel
    {
        #region Variable

        OpenFileDialog m_dialog;
        public SecsGemConfig m_GemCfg;
        public SecsGemRecipeConfig m_SecsGemrcpCfg;
        public SecsGemBase m_SesGemControl;
        string rcpDir;
        string dbdir;

        GemMsgQue m_MsgQue = new GemMsgQue();
        GemDatabase m_DataBase = new GemDatabase();

        public DelegateCommand<string> BtnCommand { get; private set; }

        private bool Initialization = true;  // For initializing data from database, prevent send any messages
        private string ConnPath;
        #endregion

        #region Properties
        private string m_TabPageHeader;
        public string TabPageHeader
        {
            get { return m_TabPageHeader; }
            set { SetProperty(ref m_TabPageHeader, value); }
        }

        private string m_DbDir;
        public string DbDir
        {
            get { return m_DbDir; }
            set { SetProperty(ref m_DbDir, value); }
        }

        private string m_RcpDir ;
        public string RcpDir
        {
            get { return m_RcpDir; }
            set { SetProperty(ref m_RcpDir, value); }
        }

        private string m_DeviceID = "Device ID :";
        public string DeviceID
        {
            get { return m_DeviceID; }
            set { SetProperty(ref m_DeviceID, value); }
        }

        private int m_deviceId ;
        public int deviceId
        {
            get { return m_deviceId; }
            set { SetProperty(ref m_deviceId, value); }
        }

        private string m_IPAddress = "IP Address :";
        public string IPAddress
        {
            get { return m_IPAddress; }
            set { SetProperty(ref m_IPAddress, value); }
        }

        private string m_ipAddress;
        public string ipAddress
        {
            get { return m_ipAddress; }
            set { SetProperty(ref m_ipAddress, value); }
        }

        private string m_PortID = "Port ID :";
        public string PortID
        {
            get { return m_PortID; }
            set { SetProperty(ref m_PortID, value); }
        }

        private int m_portId;
        public int portId
        {
            get { return m_portId; }
            set { SetProperty(ref m_portId, value); }
        }

        private string m_ReplyT3 = "Reply T3 :";
        public string ReplyT3
        {
            get { return m_ReplyT3; }
            set { SetProperty(ref m_ReplyT3, value); }
        }

        private int m_t3;
        public int t3
        {
            get { return m_t3; }
            set { SetProperty(ref m_t3, value); }
        }

        private string m_T5 = "Connect Seperation (T5) :";
        public string T5
        {
            get { return m_T5; }
            set { SetProperty(ref m_T5, value); }
        }

        private int m_t5;
        public int t5
        {
            get { return m_t5; }
            set { SetProperty(ref m_t5, value); }
        }

        private string m_T6 = "Control Transaction (T6) :";
        public string T6
        {
            get { return m_T6; }
            set { SetProperty(ref m_T6, value); }
        }

        private int m_t6;
        public int t6
        {
            get { return m_t6; }
            set { SetProperty(ref m_t6, value); }
        }

        private string m_T7 = "Not Selected (T7) :";
        public string T7
        {
            get { return m_T7; }
            set { SetProperty(ref m_T7, value); }
        }

        private int m_t7;
        public int t7
        {
            get { return m_t7; }
            set { SetProperty(ref m_t7, value); }
        }

        private string m_T8 = "Network Inter - Character (T8) :";
        public string T8
        {
            get { return m_T8; }
            set { SetProperty(ref m_T8, value); }
        }

        private int m_t8;
        public int t8
        {
            get { return m_t8; }
            set { SetProperty(ref m_t8, value); }
        }

        private string m_msgtoHost;
        public string msgtoHost
        {
            get { return m_msgtoHost; }
            set { SetProperty(ref m_msgtoHost, value); }
        }

        private string m_RecmsgfrmHost;
        public string RecmsgfrmHost
        {
            get { return m_RecmsgfrmHost; }
            set { SetProperty(ref m_RecmsgfrmHost, value); }
        }

        private bool m_canProceed = false;
        public bool canProceed
        {
            get { return m_canProceed; }
            set { SetProperty(ref m_canProceed, value); }
        }

        private Visibility m_EnableConnState = Visibility.Visible;
        public Visibility EnableConnState
        {
            get { return m_EnableConnState; }
            set { SetProperty(ref m_EnableConnState, value); }
        }

        private Visibility m_DisableConnState = Visibility.Collapsed;
        public Visibility DisableConnState
        {
            get { return m_DisableConnState; }
            set { SetProperty(ref m_DisableConnState, value); }
        }

        private BitmapImage m_GemOnOIcon = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/Switch On.png", uriKind: System.UriKind.RelativeOrAbsolute));
        public BitmapImage GemOnOIcon
        {
            get { return m_GemOnOIcon; }
            set { SetProperty(ref m_GemOnOIcon, value); }
        }

        private BitmapImage m_GemOffIcon = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/Switch Off.png", uriKind: System.UriKind.RelativeOrAbsolute));
        public BitmapImage GemOffIcon
        {
            get { return m_GemOffIcon; }
            set { SetProperty(ref m_GemOffIcon, value); }
        }

        private BitmapImage m_LoadDataIcon = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/CollectData.png", uriKind: System.UriKind.RelativeOrAbsolute));
        public BitmapImage LoadDataIcon
        {
            get { return m_LoadDataIcon; }
            set { SetProperty(ref m_LoadDataIcon, value); }
        }
        #endregion

        #region Constructor
        public SecsGemSettingsViewModel(SecsGemBase SesGemControl)
        {
            m_GemCfg = SecsGemConfig.Open(m_SystemConfig.GemRef[0].Reference);
            m_SecsGemrcpCfg = SecsGemRecipeConfig.Open(m_SystemConfig.SecsGemRcpRef[0].Reference);
            ConnPath = m_GemCfg.GPCollection[0].DatabaseDir;
            TabPageHeader = GetStringTableValue("Secs Gem Settings");
            m_dialog = new OpenFileDialog();

            m_SesGemControl = SesGemControl;

            //Events
            m_EventAggregator.GetEvent<ReceivedHostMsg>().Subscribe(OnMessageReceived);
            m_EventAggregator.GetEvent<LoadSecsGem>().Subscribe(LoadData);
            m_EventAggregator.GetEvent<SecsGemCommState>().Subscribe(SGConnectionState);

            //ButtonCommand
            BtnCommand = new DelegateCommand<string>(Btnfunc);


          
        }

        private void SGConnectionState(string connect)
        {
        switch (connect)
            {
                case "Communicating":
                    EnableConnState = Visibility.Collapsed;
                    DisableConnState = Visibility.Visible;
                    canProceed = true;
                    break;
                case "Not Communicating":
                    DisableConnState = Visibility.Collapsed;
                    EnableConnState = Visibility.Visible;
                    canProceed = false;
                    ClearAllString();
                    break;
            }
        }


        #endregion

        #region Event
        public override void OnCultureChanged()
        {
            TabPageHeader = GetStringTableValue("Secs Gem Settings");
        }

        public void SecsGemStation(SecsGem s)
        {

        }

        private void OnMessageReceived(string obj)
        {
            RecmsgfrmHost = obj;
        }

        #endregion

        #region Method

        private void Btnfunc(string cmd)
        {
            try
            {
                switch (cmd)
                {
                    case "SendToHost":
                        // Send msg to host
                        m_MsgQue.SendMessage("S10F1", msgtoHost);
                        break;

                    case "AckCmd":
                        //Acknowledge received message to host 
                        string CEID = m_DataBase.ReadCEID_FromName("MessageAcknowledge");
                        m_MsgQue.SendMessage("MessageAcknowledge", "");
                        break;

                    case "LoadDbCmd":
                        m_EventAggregator.GetEvent<LoadSecsGem>().Subscribe(LoadData);
                        break;

                    case "BrowseRcp":
                        OpenFileDialog("Rcp");
                        break;

                    case "BrowseDB":
                        OpenFileDialog("Db");
                        break;

                    case "SaveMchSetup":
                        //Save directory settings
                        SaveRcpDir();
                        break;

                    case "SaveDatabaseDir":
                        //Save directory settings
                        SaveDbDir();
                        break;

                    case "Upload":
                        //Upload recipe to host
                        m_MsgQue.SendMessage("S7F3:" + RcpDir, "");
                        break;

                    case "Download":
                        //Download recipe from host
                        m_MsgQue.SendMessage("S7F5:" + RcpDir, "");
                        break;

                    case "EnableSecsGemConn":
                        EnableConnState = Visibility.Collapsed;
                        DisableConnState = Visibility.Visible;
                        canProceed = true;
                        break;

                    case "DisableSecsGemConn":
                        DisableConnState = Visibility.Collapsed;
                        EnableConnState = Visibility.Visible;
                        canProceed = false;
                        ClearAllString();
                        break;
                }

            }
            catch
            {

            }
        }

        private void LoadData(SecsGem obj)
        {
            dbdir = obj.DbDir;
            rcpDir = obj.RcpDir;
            deviceId = obj.DeviceID;
            ipAddress = obj.IpAddress;
            portId = obj.PortID;
            t3 = obj.t3;
            t5 = obj.t5;
            t6 = obj.t6;
            t7 = obj.t7;
            t8 = obj.t8;
        }

        private void ClearAllString()
        {
            //clear all data in textboxes
            RcpDir = null;
            DbDir = null;
            deviceId = Convert.ToInt32(null);
            ipAddress = null;
            portId = Convert.ToInt32(null);
            t3 = Convert.ToInt32(null);
            t5 = Convert.ToInt32(null);
            t6 = Convert.ToInt32(null);
            t7 = Convert.ToInt32(null);
            t8 = Convert.ToInt32(null);
        }

        private void OpenFileDialog(string Browsetype)
        {
            try
            {
                if (Browsetype == "Rcp")
                {
                    m_dialog.InitialDirectory = @"..\Config Section\SecsGemRecipe";
                    m_dialog.AutoUpgradeEnabled = false;
                    m_dialog.RestoreDirectory = true;
                    m_dialog.FileName = String.Empty;
                    
                    if (m_dialog.ShowDialog() == DialogResult.OK)
                    {
                        
                        string path = Path.GetFullPath(m_dialog.FileName);
                        RcpDir = Directory.GetParent(path).ToString();
                        rcpDir = m_dialog.FileName;
                    }
                }

                if (Browsetype == "Db")
                {
                    m_dialog.InitialDirectory = @"..\Config Section\GemDb";
                    m_dialog.AutoUpgradeEnabled = false;
                    m_dialog.RestoreDirectory = true;
                    m_dialog.FileName = String.Empty;

                    if (m_dialog.ShowDialog() == DialogResult.OK)
                    {
                        string path = Path.GetFullPath(m_dialog.FileName);
                        DbDir = Directory.GetParent(path).ToString();
                        dbdir = m_dialog.FileName;
                    }
                }
            }
            catch
            {

            }
        }

        private void SaveDbDir()
        {
            try
            {
                m_GemCfg.GPCollection[0].DatabaseDir = dbdir;
                SecsGemConfig gemcfg = SecsGemConfig.Open(m_GemCfg.GPCollection[0].DatabaseDir);
                SecsGemConfig.Save();
            }
            catch
            {

            }
        }

        private void SaveRcpDir()
        {
            try
            {
                m_SecsGemrcpCfg.SecsGemRcpCollection[0].RecipeDir = rcpDir;
                SecsGemRecipeConfig rcpcfg = SecsGemRecipeConfig.Open(m_SecsGemrcpCfg.SecsGemRcpCollection[0].RecipeDir);
                SecsGemRecipeConfig.Save();
            }
            catch
            {

            }
        }
        #endregion
    }
}
