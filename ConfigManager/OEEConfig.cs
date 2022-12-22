using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

namespace ConfigManager
{
    public class OEEConfig : BaseCfg
    {
        private static OEEConfig m_Instance = null;
        private static Configuration m_Config = null;
        private static readonly string SectionName = "OEE_Config";

        public static OEEConfig Open(string path)
        {
            m_Instance = null;
            m_Instance = Open("OEEConfig", path, SectionName, out m_Config) as OEEConfig;
            Debug.Assert(m_Instance != null);
            return m_Instance;
        }

        public static OEEConfig Open()
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
           // m_Config.Save(ConfigurationSaveMode.Full);
           // ConfigurationManager.RefreshSection(SectionName);

            string oEEXmlFile = @"..\Config Section\OEE\OEE.Config";
            string tempOEEXmlfile = @"..\Config Section\OEE\OEE_Temp.Config";
            CopyXmlDocument(oEEXmlFile, tempOEEXmlfile);
        }

        private static void CopyXmlDocument(string oldfile, string newfile)
        {
            XmlDocument document = new XmlDocument();
            document.Load(oldfile);

            if (File.Exists(newfile))
                File.Delete(newfile);
            document.Save(newfile);
        }

        public static void Save(string path)
        {
            Save(path, m_CfgTbl, SectionName);
        }

        public static void Save(OEEConfig seqCfg)
        {
            Save(seqCfg, m_CfgTbl, SectionName);
        }

        [ConfigurationProperty("OEERuntime", IsRequired = true)]
        public OEERuntime OEERuntime
        {
            set { this["OEERuntime"] = value; }
            get { return (OEERuntime)this["OEERuntime"]; }
        }
    }

    public class OEERuntime : ConfigurationElement
    {
        #region Counters
        [ConfigurationProperty("TotalInput", DefaultValue = 0, IsRequired = true)]
        public int TotalInput
        {
            set { this["TotalInput"] = value; }
            get { return (int)this["TotalInput"]; }
        }

        [ConfigurationProperty("TotalOutput", DefaultValue = 0, IsRequired = true)]
        public int TotalOutput
        {
            set { this["TotalOutput"] = value; }
            get { return (int)this["TotalOutput"]; }
        }
        #endregion

        #region Downtime
        [ConfigurationProperty("PlannedStops", DefaultValue = 0.0, IsRequired = true)]
        public double PlannedStops
        {
            set { this["PlannedStops"] = value; }
            get { return (double)this["PlannedStops"]; }
        }

        [ConfigurationProperty("UnplannedStops", DefaultValue = 0.0, IsRequired = true)]
        public double UnplannedStops
        {
            set { this["UnplannedStops"] = value; }
            get { return (double)this["UnplannedStops"]; }
        }

        [ConfigurationProperty("BeginDateTime", DefaultValue = "1/1/2021 07:00:00", IsRequired = true)]
        public DateTime BeginDateTime
        {
            set { this["BeginDateTime"] = value; }
            get { return (DateTime)this["BeginDateTime"]; }
        }

        [ConfigurationProperty("ShiftNo", DefaultValue = "1", IsRequired = true)]
        public int ShiftNo
        {
            set { this["ShiftNo"] = value; }
            get { return (int)this["ShiftNo"]; }
        }
        #endregion
    }
}
