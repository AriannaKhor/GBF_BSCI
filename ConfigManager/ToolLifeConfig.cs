using System.Configuration;
using System.Linq;

namespace ConfigManager
{
    public class ToolLifeConfig : BaseCfg
    {
        private static readonly string SectionName = "ToolLife_Config";

        public ToolLifeConfig()
        {

        }

        public static ToolLifeConfig Open(string path)
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
                return m_InstanceTbl[path] as ToolLifeConfig;
            }
            Configuration configObj = null;
            ToolLifeConfig toolLifeCounter = Open("ToolLifeConfig", path, SectionName, out configObj) as ToolLifeConfig;

            if (!m_CfgTbl.Keys.Contains(path))
            {
                m_CfgTbl.Add(path, configObj);
                m_InstanceTbl.Add(path, toolLifeCounter);
            }
            return toolLifeCounter;
        }

        public static void Save()
        {
            Save(m_CfgTbl, SectionName);
        }

        public static void Save(string path)
        {
            Save(path, m_CfgTbl, SectionName);
        }

        public static void Save(ToolLifeConfig toolLifeCfg)
        {
            Save(toolLifeCfg, m_CfgTbl, SectionName);
        }

        [ConfigurationProperty("ToolLife", IsRequired = false)]
        public ToolLifeCollection TLCollection
        {
            set { this["ToolLife"] = value; }
            get { return (ToolLifeCollection)this["ToolLife"]; }
        }
    }

    public class ToolLifeCollection : ConfigurationElementCollection
    {
        #region Counter Collection
        public ToolLife this[int idx]
        {
            get { return base.BaseGet(idx) as ToolLife; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ToolLife();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ToolLife)element).ID;
        }
        #endregion
    }

    public class ToolLife : ConfigurationElement
    {
        #region Tool Life
        public ToolLife()
        {
        }

        [ConfigurationProperty("ID", DefaultValue = 0, IsRequired = true)]
        public int ID
        {
            set { this["ID"] = value; }
            get { return (int)this["ID"]; }
        }

        [ConfigurationProperty("ToolName", DefaultValue = "Empty", IsRequired = true)]
        public string ToolName
        {
            set { this["ToolName"] = value; }
            get { return (string)this["ToolName"]; }
        }

        [ConfigurationProperty("Tooltip", DefaultValue = "", IsRequired = true)]
        public string Tooltip
        {
            set { this["Tooltip"] = value; }
            get { return (string)this["Tooltip"]; }
        }

        // Cleaning Counter
        [ConfigurationProperty("CleaningValue", DefaultValue = 0, IsRequired = true)]
        public int CleaningValue
        {
            set { this["CleaningValue"] = value; }
            get { return (int)this["CleaningValue"]; }
        }

        [ConfigurationProperty("MinCleaning", DefaultValue = 0, IsRequired = true)]
        public int MinCleaning
        {
            set { this["MinCleaning"] = value; }
            get { return (int)this["MinCleaning"]; }
        }

        [ConfigurationProperty("MaxCleaning", DefaultValue = 1000, IsRequired = true)]
        public int MaxCleaning
        {
            set { this["MaxCleaning"] = value; }
            get { return (int)this["MaxCleaning"]; }
        }

        // Tool Life Counter
        [ConfigurationProperty("ToolLifeValue", DefaultValue = 0, IsRequired = true)]
        public int ToolLifeValue
        {
            set { this["ToolLifeValue"] = value; }
            get { return (int)this["ToolLifeValue"]; }
        }

        [ConfigurationProperty("MinToolLife", DefaultValue = 0, IsRequired = true)]
        public int MinToolLife
        {
            set { this["MinToolLife"] = value; }
            get { return (int)this["MinToolLife"]; }
        }

        [ConfigurationProperty("MaxToolLife", DefaultValue = 1000, IsRequired = true)]
        public int MaxToolLife
        {
            set { this["MaxToolLife"] = value; }
            get { return (int)this["MaxToolLife"]; }
        }

        [ConfigurationProperty("IsCleaningEnable", DefaultValue = false, IsRequired = true)]
        public bool IsCleaningEnable
        {
            set { this["IsCleaningEnable"] = value; }
            get { return (bool)this["IsCleaningEnable"]; }
        }

        [ConfigurationProperty("IsToolLifeEnable", DefaultValue = false, IsRequired = true)]
        public bool IsToolLifeEnable
        {
            set { this["IsToolLifeEnable"] = value; }
            get { return (bool)this["IsToolLifeEnable"]; }
        }
        #endregion
    }
}
