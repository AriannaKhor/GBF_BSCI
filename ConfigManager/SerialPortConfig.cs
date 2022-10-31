using System.Configuration;
using System.IO.Ports;
using System.Linq;

namespace ConfigManager
{
    public class SerialPortConfig : BaseCfg
    {
        private static readonly string SectionName = "SerialPort_Config";
        
        // Constructor
        public SerialPortConfig()
        {
        }

        public static SerialPortConfig Open(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                // When system config does not indicate any reference, just return null.
                // Client code is responsible to make further evaluation before usage.
                return null;
            }
            if (m_InstanceTbl.Keys.Contains(path))
            {
                // return the same instance - so that UI & SeqManager can share.
                return m_InstanceTbl[path] as SerialPortConfig;
            }
            Configuration configObj = null;
            SerialPortConfig serialPortSetting = Open("SerialPortConfig", path, SectionName, out configObj) as SerialPortConfig;

            if (!m_CfgTbl.Keys.Contains(path))
            {
                m_CfgTbl.Add(path, configObj);
                m_InstanceTbl.Add(path, serialPortSetting);
            }
            return serialPortSetting;
        }

        // Save to single file
        public static void Save(string path)
        {
            Save(path, m_CfgTbl, SectionName);
        }

        public static void Save(SerialPortConfig serialPortCfg)
        {
            Save(serialPortCfg, m_CfgTbl, SectionName);
        }

        [ConfigurationProperty("Device")]
        public DeviceCfg Device
        {
            set { this["Device"] = value; }
            get { return (DeviceCfg)this["Device"]; }
        }

        [ConfigurationProperty("User")]
        public UserCfg User
        {
            set { this["User"] = value; }
            get { return (UserCfg)this["User"]; }
        }

        public class DeviceCfg : ConfigurationElement
        {
            [ConfigurationProperty("Name", IsRequired = true, DefaultValue = "Dev1")]
            public string Name
            {
                set { this["Name"] = value; }
                get { return (string)this["Name"]; }
            }

            [ConfigurationProperty("BaudRate", IsRequired = true, DefaultValue = "9600")]
            public int BaudRate
            {
                set { this["BaudRate"] = value; }
                get { return (int)this["BaudRate"]; }
            }

            [ConfigurationProperty("Parity", IsRequired = true, DefaultValue = "None")]
            public Parity Parity
            {
                set { this["Parity"] = value; }
                get { return (Parity)this["Parity"]; }
            }

            [ConfigurationProperty("PortName", IsRequired = true, DefaultValue = "COM1")]
            public string PortName
            {
                set { this["PortName"] = value; }
                get { return (string)this["PortName"]; }
            }

            [ConfigurationProperty("StopBits", IsRequired = true, DefaultValue = "One")]
            public StopBits StopBits
            {
                set { this["StopBits"] = value; }
                get { return (StopBits)this["StopBits"]; }
            }

            [ConfigurationProperty("Handshake", IsRequired = true, DefaultValue = "None")]
            public Handshake Handshake
            {
                set { this["Handshake"] = value; }
                get { return (Handshake)this["Handshake"]; }
            }

            [ConfigurationProperty("DataBits", IsRequired = true, DefaultValue = "8")]
            public int DataBits
            {
                set { this["DataBits"] = value; }
                get { return (int)this["DataBits"]; }
            }
        }

        public class UserCfg : ConfigurationElement
        {
            [ConfigurationProperty("RXTerminator", IsRequired = true, DefaultValue = "0xD")]
            public byte RXTerminator
            {
                set { this["RXTerminator"] = value; }
                get { return (byte)this["RXTerminator"]; }
            }

            [ConfigurationProperty("LogPath", IsRequired = true, DefaultValue = @"..\SerialPortLogPath\Dev1\")]
            public string LogPath
            {
                set { this["LogPath"] = value; }
                get { return (string)this["LogPath"]; }
            }
        }
    }
}
