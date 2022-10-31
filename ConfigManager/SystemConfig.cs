using GreatechApp.Core.Interface;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;

namespace ConfigManager
{
    public class SystemConfig : BaseCfg
    {
        private static SystemConfig m_Instance = null;
        private static Configuration m_Config = null;
        private static readonly string SectionName = "System_Config";

        public SystemConfig()
        {
        }

        public static SystemConfig Open(string path)
        {
            m_Instance = null;
            m_Instance = Open("SystemConfig", path, SectionName, out m_Config) as SystemConfig;
            Debug.Assert(m_Instance != null);
            return m_Instance;
        }

        public static SystemConfig Open()
        {
            // Once we have gotten the instance, other class can obtain the same instance without passing in the path.
            // Don't have provision to open default config since we do not know which one to refer.
            Debug.Assert(m_Instance != null);
            return m_Instance;
        }

        public void Save()
        {
            Debug.Assert(m_Config != null);
            if (m_Config == null)
            {
                return;
            }
            m_Config.Save(ConfigurationSaveMode.Full);
            ConfigurationManager.RefreshSection(SectionName);
        }

        [ConfigurationProperty("FolderPath")]
        public FolderPath FolderPath
        {
            set { this["FolderPath"] = value; }
            get { return (FolderPath)this["FolderPath"]; }
        }

        [ConfigurationProperty("Machine")]
        public Machine Machine
        {
            set { this["Machine"] = value; }
            get { return (Machine)this["Machine"]; }
        }

        [ConfigurationProperty("General")]
        public SysGen General
        {
            set { this["General"] = value; }
            get { return (SysGen)this["General"]; }
        }

        [ConfigurationProperty("Regional")]
        public Regional Regional
        {
            set { this["Regional"] = value; }
            get { return (Regional)this["Regional"]; }
        }

        [ConfigurationProperty("DigitalIO")]
        public DigitalIO DigitalIO
        {
            set { this["DigitalIO"] = value; }
            get { return (DigitalIO)this["DigitalIO"]; }
        }

        [ConfigurationProperty("TCOSIODevice")]
        public TCOSIODeviceCollection TCOSIODevice
        {
            set { this["TCOSIODevice"] = value; }
            get { return (TCOSIODeviceCollection)this["TCOSIODevice"]; }
        }

        [ConfigurationProperty("IOCards")]
        public IOCardCollection IOCards
        {
            set { this["IOCards"] = value; }
            get { return (IOCardCollection)this["IOCards"]; }
        }

        [ConfigurationProperty("IOInDevices")]
        public IODeviceCollection IOInDevices
        {
            set { this["IOInDevices"] = value; }
            get { return (IODeviceCollection)this["IOInDevices"]; }
        }

        [ConfigurationProperty("IOOutDevices")]
        public IODeviceCollection IOOutDevices
        {
            set { this["IOOutDevices"] = value; }
            get { return (IODeviceCollection)this["IOOutDevices"]; }
        }

        [ConfigurationProperty("IOAdvantechDevices")]
        public IODeviceCollection IOAdvantechDevices
        {
            set { this["IOAdvantechDevices"] = value; }
            get { return (IODeviceCollection)this["IOAdvantechDevices"]; }
        }

        [ConfigurationProperty("IOWagoDevices")]
        public IODeviceCollection IOWagoDevices
        {
            set { this["IOWagoDevices"] = value; }
            get { return (IODeviceCollection)this["IOWagoDevices"]; }
        }

        [ConfigurationProperty("Motion")]
        public Motion Motion
        {
            set { this["Motion"] = value; }
            get { return (Motion)this["Motion"]; }
        }

        [ConfigurationProperty("MotionCards")]
        public MotionCardCollection MotionCards
        {
            set { this["MotionCards"] = value; }
            get { return (MotionCardCollection)this["MotionCards"]; }
        }

        [ConfigurationProperty("VisionTesterStations")]
        public StationCollection VisionTesterStations
        {
            set { this["VisionTesterStations"] = value; }
            get { return (StationCollection)this["VisionTesterStations"]; }
        }

        [ConfigurationProperty("NetworkDevices")]
        public NetworkDeviceCollection NetworkDevices
        {
            set { this["NetworkDevices"] = value; }
            get { return (NetworkDeviceCollection)this["NetworkDevices"]; }
        }

        [ConfigurationProperty("ServerSockets")]
        public NetworkDeviceCollection ServerSockets
        {
            set { this["ServerSockets"] = value; }
            get { return (NetworkDeviceCollection)this["ServerSockets"]; }
        }

        [ConfigurationProperty("SeqCfg")]
        public CfgCollection SeqCfgRef
        {
            set { this["SeqCfg"] = value; }
            get { return (CfgCollection)this["SeqCfg"]; }
        }

        [ConfigurationProperty("MotCfg", IsRequired = false)]
        public CfgCollection MotCfgRef
        {
            set { this["MotCfg"] = value; }
            get { return (CfgCollection)this["MotCfg"]; }
        }

        [ConfigurationProperty("CounterCfg", IsRequired = false)]
        public CfgCollection CounterCfgRef
        {
            set { this["CounterCfg"] = value; }
            get { return (CfgCollection)this["CounterCfg"]; }
        }

        [ConfigurationProperty("SerialPortCfg", IsRequired = false)]
        public CfgCollection SerialPortRef
        {
            set { this["SerialPortCfg"] = value; }
            get { return (CfgCollection)this["SerialPortCfg"]; }
        }

        [ConfigurationProperty("TowerLightCfg", IsRequired = false)]
        public CfgCollection TowerLightRef
        {
            set { this["TowerLightCfg"] = value; }
            get { return (CfgCollection)this["TowerLightCfg"]; }
        }


        [ConfigurationProperty("GemCfg", IsRequired = false)]
        public CfgCollection GemRef
        {
            set { this["GemCfg"] = value; }
            get { return (CfgCollection)this["GemCfg"]; }
        }

        [ConfigurationProperty("SecsGemRcpCfg", IsRequired = false)]
        public CfgCollection SecsGemRcpRef
        {
            set { this["SecsGemRcpCfg"] = value; }
            get { return (CfgCollection)this["SecsGemRcpCfg"]; }
        }
    }

    public class FolderPath : ConfigurationElement
    {
        [ConfigurationProperty("AppLog", DefaultValue = @"..\Log\", IsRequired = true)]
        public string AppLog
        {
            set { this["AppLog"] = value; }
            get { return (string)this["AppLog"]; }
        }

        [ConfigurationProperty("SoftwareResultLog", DefaultValue = @"..\SoftwareResultLog\", IsRequired = true)]
        public string SoftwareResultLog
        {
            set { this["SoftwareResultLog"] = value; }
            get { return (string)this["SoftwareResultLog"]; }
        }

        [ConfigurationProperty("SeqInfoLog", DefaultValue = @"..\SeqInfoLog\", IsRequired = true)]
        public string SeqInfoLog
        {
            set { this["SeqInfoLog"] = value; }
            get { return (string)this["SeqInfoLog"]; }
        }

        [ConfigurationProperty("CommCfgPath", DefaultValue = @"..\Config Section\CommSetting\", IsRequired = false)]
        public string CommCfgPath
        {
            set { this["CommCfgPath"] = value; }
            get { return (string)this["CommCfgPath"]; }
        }

        [ConfigurationProperty("LiveSeqLog", DefaultValue = @"..\LiveSeqLog\", IsRequired = true)]
        public string LiveSeqLog
        {
            set { this["LiveSeqLog"] = value; }
            get { return (string)this["LiveSeqLog"]; }
        }

        [ConfigurationProperty("OEELog", DefaultValue = @"..\OEELog\", IsRequired = true)]
        public string OEELog
        {
            set { this["OEELog"] = value; }
            get { return (string)this["OEELog"]; }
        }

        [ConfigurationProperty("AnalysisLog", DefaultValue = @"..\AnalysisLog\", IsRequired = true)]
        public string AnalysisLog
        {
            set { this["AnalysisLog"] = value; }
            get { return (string)this["AnalysisLog"]; }
        }
    }

    public class Machine : ConfigurationElement
    {
        #region Machine
        [ConfigurationProperty("SafetyScan", DefaultValue = true, IsRequired = true)]
        public bool SafetyScan
        {
            set { this["SafetyScan"] = value; }
            get { return (bool)this["SafetyScan"]; }
        }

        [ConfigurationProperty("BypInterlock", DefaultValue = false, IsRequired = true)]
        public bool BypInterlock
        {
            set { this["BypInterlock"] = value; }
            get { return (bool)this["BypInterlock"]; }
        }

        [ConfigurationProperty("AdminEStop", DefaultValue = true, IsRequired = true)]
        public bool AdminEStop
        {
            set { this["AdminEStop"] = value; }
            get { return (bool)this["AdminEStop"]; }
        }

        [ConfigurationProperty("EquipName", DefaultValue = "Put Machine Name Here", IsRequired = true)]
        public string EquipName
        {
            set { this["EquipName"] = value; }
            get { return (string)this["EquipName"]; }
        }

        [ConfigurationProperty("HardVer", DefaultValue = "1.0.0", IsRequired = true)]
        public string HardVer
        {
            set { this["HardVer"] = value; }
            get { return (string)this["HardVer"]; }
        }

        [ConfigurationProperty("MachineID", DefaultValue = "1", IsRequired = true)]
        public string MachineID
        {
            set { this["MachineID"] = value; }
            get { return (string)this["MachineID"]; }
        }
        #endregion
    }

    public class SysGen : ConfigurationElement
    {
        #region SysGen
        [ConfigurationProperty("MaxLogItem", DefaultValue = 5000, IsRequired = true)]
        public int MaxLogItem
        {
            set { this["MaxLogItem"] = value; }
            get { return (int)this["MaxLogItem"]; }
        }

        [ConfigurationProperty("IdealUPH", DefaultValue = 10000, IsRequired = true)]
        public int IdealUPH
        {
            set { this["IdealUPH"] = value; }
            get { return (int)this["IdealUPH"]; }
        }

        [ConfigurationProperty("IdealCycleTime", DefaultValue = 5.0, IsRequired = true)]
        public double IdealCycleTime
        {
            set { this["IdealCycleTime"] = value; }
            get { return (double)this["IdealCycleTime"]; }
        }

        [ConfigurationProperty("DurationToFailure", DefaultValue = 2, IsRequired = true)]
        public int DurationToFailure
        {
            set { this["DurationToFailure"] = value; }
            get { return (int)this["DurationToFailure"]; }
        }

        [ConfigurationProperty("FontSize", DefaultValue = 15, IsRequired = true)]
        public int FontSize
        {
            set { this["FontSize"] = value; }
            get { return (int)this["FontSize"]; }
        }

        [ConfigurationProperty("IOScanRate", DefaultValue = 100, IsRequired = true)]
        public int IOScanRate
        {
            set { this["IOScanRate"] = value; }
            get { return (int)this["IOScanRate"]; }
        }

        [ConfigurationProperty("IsCompactView", DefaultValue = false, IsRequired = true)]
        public bool IsCompactView
        {
            set { this["IsCompactView"] = value; }
            get { return (bool)this["IsCompactView"]; }
        }

        [ConfigurationProperty("SQLService", DefaultValue = "MSSQL$SQLEXPRESS", IsRequired = true)]
        public string SQLService
        {
            set { this["SQLService"] = value; }
            get { return (string)this["SQLService"]; }
        }

        [ConfigurationProperty("IsLogTCPMsg", DefaultValue = "false", IsRequired = true)]
        public bool IsLogTCPMsg
        {
            set { this["IsLogTCPMsg"] = value; }
            get { return (bool)this["IsLogTCPMsg"]; }
        }

        [ConfigurationProperty("IsLogSerialMsg", DefaultValue = "false", IsRequired = true)]
        public bool IsLogSerialMsg
        {
            set { this["IsLogSerialMsg"] = value; }
            get { return (bool)this["IsLogSerialMsg"]; }
        }
        #endregion
    }

    public class Regional : ConfigurationElement
    {
        [ConfigurationProperty("Culture", IsRequired = true)]
        public string Culture
        {
            set { this["Culture"] = value; }
            get { return (string)this["Culture"]; }
        }
    }

    public class Database : ConfigurationElement
    {
        [ConfigurationProperty("Connection", IsRequired = true)]
        public string Connection
        {
            set { this["Connection"] = value; }
            get { return (string)this["Connection"]; }
        }
    }

    public class DigitalIO : ConfigurationElement
    {
        #region Digital IO
        [ConfigurationProperty("ClassName", IsRequired = true)]
        public string ClassName
        {
            set { this["ClassName"] = value; }
            get { return (string)this["ClassName"]; }
        }

        [ConfigurationProperty("MaxPortNum", DefaultValue = 2, IsRequired = true)]
        public int MaxPortNum
        {
            set { this["MaxPortNum"] = value; }
            get { return (int)this["MaxPortNum"]; }
        }

        [ConfigurationProperty("MaxBitPerPort", DefaultValue = 8, IsRequired = true)]
        public int MaxBitPerPort
        {
            set { this["MaxBitPerPort"] = value; }
            get { return (int)this["MaxBitPerPort"]; }
        }

        [ConfigurationProperty("MaxSlaveNo", DefaultValue = 1, IsRequired = true)]
        public int MaxSlaveNo
        {
            set { this["MaxSlaveNo"] = value; }
            get { return (int)this["MaxSlaveNo"]; }
        }
        #endregion
    }

    public class IOCardCollection : ConfigurationElementCollection
    {
        #region IO Cards Collection
        public IOCards this[int idx]
        {
            get { return base.BaseGet(idx) as IOCards; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new IOCards();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((IOCards)element).ID;
        }
        #endregion
    }

    public class IODeviceCollection : ConfigurationElementCollection
    {
        #region IO Devices Collection
        public IODevices this[int idx]
        {
            get { return base.BaseGet(idx) as IODevices; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new IODevices();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((IODevices)element).ID;
        }
        #endregion
    }

    public class TCOSIODeviceCollection : ConfigurationElementCollection
    {
        #region TCOSIODevice Devices Collection
        public TCOSIODevice this[int idx]
        {
            get { return base.BaseGet(idx) as TCOSIODevice; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TCOSIODevice();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TCOSIODevice)element).ID;
        }
        #endregion
    }


    public class TCOSIODevice : ConfigurationElement
    {
        #region TCOS IO Devices
        public TCOSIODevice()
        {
        }

        [ConfigurationProperty("ID", DefaultValue = 0, IsRequired = true)]
        public int ID
        {
            set { this["ID"] = value; }
            get { return (int)this["ID"]; }
        }

        [ConfigurationProperty("SerialNumber", DefaultValue = "N0200100006", IsRequired = true)]
        public string SerialNumber
        {
            set { this["SerialNumber"] = value; }
            get { return (string)this["SerialNumber"]; }
        }
        #endregion
    }

    public class IOCards : ConfigurationElement
    {
        #region IO Cards
        public IOCards()
        {
        }

        [ConfigurationProperty("ID", DefaultValue = 0, IsRequired = true)]
        public int ID
        {
            set { this["ID"] = value; }
            get { return (int)this["ID"]; }
        }

        [ConfigurationProperty("NumOfSetID", DefaultValue = 1, IsRequired = true)]
        public int NumOfSetID
        {
            set { this["NumOfSetID"] = value; }
            get { return (int)this["NumOfSetID"]; }
        }
        #endregion
    }

    public class IODevices : ConfigurationElement
    {
        #region IO Devices
        public IODevices()
        {
        }

        [ConfigurationProperty("ID", DefaultValue = 0, IsRequired = false)]
        public int ID
        {
            set { this["ID"] = value; }
            get { return (int)this["ID"]; }
        }

        [ConfigurationProperty("IOName", DefaultValue = "MXIO Input Card", IsRequired = false)]
        public string IOName
        {
            set { this["Name"] = value; }
            get { return (string)this["IOName"]; }
        }

        [ConfigurationProperty("DeviceAddress", DefaultValue = "192.0.0.10", IsRequired = false)]
        public string DeviceAddress
        {
            set { this["DeviceAddress"] = value; }
            get { return (string)this["DeviceAddress"]; }
        }

        [ConfigurationProperty("DevicePort", DefaultValue = "503", IsRequired = false)]
        public string DevicePort
        {
            set { this["DevicePort"] = value; }
            get { return (string)this["DevicePort"]; }
        }

        [ConfigurationProperty("InputStartAddress", DefaultValue = "0", IsRequired = false)]
        public string InputStartAddress
        {
            set { this["InputStartAddress"] = value; }
            get { return (string)this["InputStartAddress"]; }
        }

        [ConfigurationProperty("OutputStartAddress", DefaultValue = "512", IsRequired = false)]
        public string OutputStartAddress
        {
            set { this["OutputStartAddress"] = value; }
            get { return (string)this["OutputStartAddress"]; }
        }

        [ConfigurationProperty("NumOfInputModules", DefaultValue = 0, IsRequired = false)]
        public int NumOfInputModules
        {
            set { this["NumOfInputModules"] = value; }
            get { return (int)this["NumOfInputModules"]; }
        }

        [ConfigurationProperty("NumOfOutputModules", DefaultValue = 0, IsRequired = false)]
        public int NumOfOutputModules
        {
            set { this["NumOfOutputModules"] = value; }
            get { return (int)this["NumOfOutputModules"]; }
        }

        [ConfigurationProperty("IOFile", IsRequired = false)]
        public string IOFile
        {
            set { this["IOFile"] = value; }
            get { return (string)this["IOFile"]; }
        }
        #endregion
    }

    public class Motion : ConfigurationElement
    {
        #region Motion
        [ConfigurationProperty("ClassName", IsRequired = true)]
        public string ClassName
        {
            set { this["ClassName"] = value; }
            get { return (string)this["ClassName"]; }
        }

        [ConfigurationProperty("NumOfController", DefaultValue = 1, IsRequired = true)]
        public int NumOfController
        {
            set { this["NumOfController"] = value; }
            get { return (int)this["NumOfController"]; }
        }

        [ConfigurationProperty("NumOfAxis", DefaultValue = 1, IsRequired = true)]
        public int NumOfAxis
        {
            set { this["NumOfAxis"] = value; }
            get { return (int)this["NumOfAxis"]; }
        }
        #endregion
    }

    public class MotionCardCollection : ConfigurationElementCollection
    {
        #region Motion Cards Collection
        public MotionCard this[int idx]
        {
            get { return base.BaseGet(idx) as MotionCard; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new MotionCard();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MotionCard)element).ID;
        }
        #endregion
    }

    public class MotionCard : ConfigurationElement
    {
        #region Motion Card
        public MotionCard()
        {
        }

        [ConfigurationProperty("ID", DefaultValue = 0, IsRequired = true)]
        public int ID
        {
            set { this["ID"] = value; }
            get { return (int)this["ID"]; }
        }

        [ConfigurationProperty("CardID", DefaultValue = 0, IsRequired = true)]
        public int CardID
        {
            set { this["CardID"] = value; }
            get { return (int)this["CardID"]; }
        }

        [ConfigurationProperty("DeviceAddress", DefaultValue = "192.0.0.10", IsRequired = false)]
        public string DeviceAddress
        {
            set { this["DeviceAddress"] = value; }
            get { return (string)this["DeviceAddress"]; }
        }

        [ConfigurationProperty("DevicePort", DefaultValue = "8500", IsRequired = false)]
        public string DevicePort
        {
            set { this["DevicePort"] = value; }
            get { return (string)this["DevicePort"]; }
        }

        [ConfigurationProperty("ConfigFile", DefaultValue = "", IsRequired = false)]
        public string ConfigFile
        {
            set { this["ConfigFile"] = value; }
            get { return (string)this["ConfigFile"]; }
        }
        #endregion
    }

    public class StationCollection : ConfigurationElementCollection
    {
        #region Station Collection
        public Station this[int idx]
        {
            get { return base.BaseGet(idx) as Station; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Station();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Station)element).ID;
        }
        #endregion
    }

    public class Station : ConfigurationElement
    {
        #region Station
        public Station()
        {
        }

        [ConfigurationProperty("ID", DefaultValue = 0, IsRequired = true)]
        public int ID
        {
            set { this["ID"] = value; }
            get { return (int)this["ID"]; }
        }

        [ConfigurationProperty("Name", DefaultValue = "", IsRequired = true)]
        public string Name
        {
            set { this["Name"] = value; }
            get { return (string)this["Name"]; }
        }

        [ConfigurationProperty("Type", DefaultValue = "", IsRequired = true)]
        public string Type
        {
            set { this["Type"] = value; }
            get { return (string)this["Type"]; }
        }
        #endregion
    }

    public class NetworkDeviceCollection : ConfigurationElementCollection
    {
        #region Network Devices Collection
        public NetworkDevice this[int idx]
        {
            get { return base.BaseGet(idx) as NetworkDevice; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new NetworkDevice();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NetworkDevice)element).ID;
        }
        #endregion
    }

    public class NetworkDevice : ConfigurationElement
    {
        #region Network Device
        public NetworkDevice()
        {
        }

        [ConfigurationProperty("ID", DefaultValue = 0, IsRequired = true)]
        public int ID
        {
            set { this["ID"] = value; }
            get { return (int)this["ID"]; }
        }

        [ConfigurationProperty("Name", DefaultValue = "Default", IsRequired = true)]
        public string Name
        {
            set { this["Name"] = value; }
            get { return (string)this["Name"]; }
        }

        [ConfigurationProperty("IPAddress", DefaultValue = "192.0.0.10", IsRequired = false)]
        public string IPAddress
        {
            set { this["IPAddress"] = value; }
            get { return (string)this["IPAddress"]; }
        }

        [ConfigurationProperty("Port", DefaultValue = 0, IsRequired = true)]
        public int Port
        {
            set { this["Port"] = value; }
            get { return (int)this["Port"]; }
        }

        [ConfigurationProperty("SendDataFormat", DefaultValue = "ASCII", IsRequired = true)]
        public string SendDataFormat
        {
            set { this["SendDataFormat"] = value; }
            get { return (string)this["SendDataFormat"]; }
        }

        [ConfigurationProperty("ReceivedDataFormat", DefaultValue = "ASCII", IsRequired = true)]
        public string ReceivedDataFormat
        {
            set { this["ReceivedDataFormat"] = value; }
            get { return (string)this["ReceivedDataFormat"]; }
        }

        [ConfigurationProperty("Type", DefaultValue = "Scanner", IsRequired = true)]
        public string Type
        {
            set { this["Type"] = value; }
            get { return (string)this["Type"]; }
        }
        #endregion
    }

    public class CfgCollection : ConfigurationElementCollection, IEnumerable<Cfg>
    {
        #region Cfg Collection
        public Cfg this[int idx]
        {
            get { return base.BaseGet(idx) as Cfg; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Cfg();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Cfg)element).ID;
        }

        IEnumerator<Cfg> IEnumerable<Cfg>.GetEnumerator()
        {
            foreach (var key in this.BaseGetAllKeys())
            {
                yield return (Cfg)BaseGet(key);
            }
        }
        #endregion
    }

    public class Cfg : ConfigurationElement
    {
        #region Cfg
        public Cfg()
        {
        }

        [ConfigurationProperty("ID", DefaultValue = 0, IsRequired = true)]
        public int ID
        {
            set { this["ID"] = value; }
            get { return (int)this["ID"]; }
        }

        [ConfigurationProperty("SeqID", DefaultValue = 0, IsRequired = false)]
        public int SeqID
        {
            set { this["SeqID"] = value; }
            get { return (int)this["SeqID"]; }
        }

        [ConfigurationProperty("Reference", DefaultValue = "", IsRequired = true)]
        public string Reference
        {
            set { this["Reference"] = value; }
            get { return (string)this["Reference"]; }
        }

        [ConfigurationProperty("BakRef", DefaultValue = "", IsRequired = false)]
        public string BakRef
        {
            set { this["BakRef"] = value; }
            get { return (string)this["BakRef"]; }
        }

        [ConfigurationProperty("ErrLib", DefaultValue = "", IsRequired = false)]
        public string ErrLib
        {
            set { this["ErrLib"] = value; }
            get { return (string)this["ErrLib"]; }
        }

        [ConfigurationProperty("ErrLibPath", DefaultValue = "", IsRequired = false)]
        public string ErrLibPath
        {
            set { this["ErrLibPath"] = value; }
            get { return (string)this["ErrLibPath"]; }
        }


        //[ConfigurationProperty("ErrLibZhHant", DefaultValue = "", IsRequired = false)]
        //public string ErrLibZhHant
        //{
        //    set { this["ErrLibZhHant"] = value; }
        //    get { return (string)this["ErrLibZhHant"]; }
        //}

        //[ConfigurationProperty("ErrLibZhHans", DefaultValue = "", IsRequired = false)]
        //public string ErrLibZhHans
        //{
        //    set { this["ErrLibZhHans"] = value; }
        //    get { return (string)this["ErrLibZhHans"]; }
        //}
        #endregion
    }
}
