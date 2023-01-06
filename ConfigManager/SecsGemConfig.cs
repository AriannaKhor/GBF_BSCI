using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigManager
{
    public class SecsGemConfig : BaseCfg
    {
        private static readonly string SectionName = "Secs_Gem_Config";

        public SecsGemConfig()
        {

        }

        public static SecsGemConfig Open(string path)
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
                return m_InstanceTbl[path] as SecsGemConfig;
            }
            Configuration configObj = null;
            SecsGemConfig SecsgemCounter = Open("SecsGemConfig", path, SectionName, out configObj) as SecsGemConfig;

            if (!m_CfgTbl.Keys.Contains(path))
            {
                m_CfgTbl.Add(path, configObj);
                m_InstanceTbl.Add(path, SecsgemCounter);
            }
            return SecsgemCounter;
        }

        public static void Save()
        {
            Save(m_CfgTbl, SectionName);
        }

        public static void Save(string path)
        {
            Save(path, m_CfgTbl, SectionName);
        }

        public static void Save(SecsGemConfig SecsgemCfg)
        {
            Save(SecsgemCfg, m_CfgTbl, SectionName);
        }

        [ConfigurationProperty("Setting", IsRequired = false)]
        public SecsGemCollection GPCollection
        {
            set { this["Setting"] = value; }
            get { return (SecsGemCollection)this["Setting"]; }
        }

        public class SecsGemCollection : ConfigurationElementCollection
        {
            #region Counter Collection
            public SecsGem this[int idx]
            {
                get { return base.BaseGet(idx) as SecsGem; }
            }

            protected override ConfigurationElement CreateNewElement()
            {
                return new SecsGem();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((SecsGem)element).ID;
            }
            #endregion
        }

        public class SecsGem : ConfigurationElement
        {
            public SecsGem()
            {
            }

            [ConfigurationProperty("ID")]
            public int ID
            {
                set { this["ID"] = value; }
                get { return (int)this["ID"]; }
            }

            [ConfigurationProperty("DatabaseDir")]
            public string DatabaseDir
            {
                set { this["DatabaseDir"] = value; }
                get { return (string)this["DatabaseDir"]; }
            }

            [ConfigurationProperty("EnableSecsGem", IsRequired = true)]
            public bool EnableComm
            {
                set { this["EnableSecsGem"] = value; }
                get { return (bool)this["EnableSecsGem"]; }
            }
        }
    }
}
