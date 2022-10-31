using GreatechApp.Core.Enums;
using System.Configuration;
using System.Linq;

namespace ConfigManager
{
    public class SeqConfig : BaseCfg
    {
        private static readonly string SectionName = "Seq_Config";

        public SeqConfig()
        {

        }

        public static SeqConfig Open(string path)
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
                return m_InstanceTbl[path] as SeqConfig;
            }
            Configuration configObj = null;
            SeqConfig seqSetting = Open("SeqSetting", path, SectionName, out configObj) as SeqConfig;
            // We would like to build a lookup table of configuration object based on the path value.
            // Each module seq needs to have a separate instance of configObj.
            // Note: the keys are case-sensitive.
            if (!m_CfgTbl.Keys.Contains(path))
            {
                m_CfgTbl.Add(path, configObj);
                m_InstanceTbl.Add(path, seqSetting);
            }
            return seqSetting;
        }

        public static void Save()
        {
            Save(m_CfgTbl, SectionName);
        }

        public static void Save(string path)
        {
            Save(path, m_CfgTbl, SectionName);
        }

        public static void Save(SeqConfig seqCfg)
        {
            Save(seqCfg, m_CfgTbl, SectionName);
        }

        public static void SaveAs(string path, string newPath)
        {
            SaveAs(path, m_CfgTbl, newPath, SectionName);
        }

        [ConfigurationProperty("ErrorTimers", IsRequired = false)]
        public TimerCollection Err
        {
            set { this["ErrorTimers"] = value; }
            get { return (TimerCollection)this["ErrorTimers"]; }
        }

        [ConfigurationProperty("DelayTimers", IsRequired = false)]
        public TimerCollection Delay
        {
            set { this["DelayTimers"] = value; }
            get { return (TimerCollection)this["DelayTimers"]; }
        }

        [ConfigurationProperty("Counters", IsRequired = false)]
        public KVPCollection Counter
        {
            set { this["Counters"] = value; }
            get { return (KVPCollection)this["Counters"]; }
        }

        [ConfigurationProperty("Options", IsRequired = false)]
        public SeqOptionCollection Option
        {
            set { this["Options"] = value; }
            get { return (SeqOptionCollection)this["Options"]; }
        }

        [ConfigurationProperty("TestRun", IsRequired = false)]
        public TestRunCollection Test
        {
            set { this["TestRun"] = value; }
            get { return (TestRunCollection)this["TestRun"]; }
        }
    }

    public class TimerCollection : ConfigurationElementCollection
    {
        #region Timer Collection
        public TimerParam this[int idx]
        {
            get { return base.BaseGet(idx) as TimerParam; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TimerParam();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TimerParam)element).ID;
        }
        #endregion
    }

    public class TimerParam : ConfigurationElement
    {
        #region Timer
        public TimerParam()
        {
        }

        [ConfigurationProperty("ID", DefaultValue = 0, IsRequired = true)]
        public int ID
        {
            set { this["ID"] = value; }
            get { return (int)this["ID"]; }
        }

        [ConfigurationProperty("TimeOut", DefaultValue = 10f, IsRequired = true)]
        public float TimeOut
        {
            set { this["TimeOut"] = value; }
            get { return (float)this["TimeOut"]; }
        }

        [ConfigurationProperty("Description", DefaultValue = "Empty", IsRequired = false)]
        public string Description
        {
            set { this["Description"] = value; }
            get { return (string)this["Description"]; }
        }

        [ConfigurationProperty("Max", DefaultValue = 100f, IsRequired = false)]
        public float Max
        {
            set { this["Max"] = value; }
            get { return (float)this["Max"]; }
        }

        [ConfigurationProperty("Min", DefaultValue = 0f, IsRequired = false)]
        public float Min
        {
            set { this["Min"] = value; }
            get { return (float)this["Min"]; }
        }

        [ConfigurationProperty("IsVisible", DefaultValue = false, IsRequired = false)]
        public bool IsVisible
        {
            set { this["IsVisible"] = value; }
            get { return (bool)this["IsVisible"]; }
        }

        [ConfigurationProperty("Tooltip", DefaultValue = "", IsRequired = false)]
        public string Tooltip
        {
            set { this["Tooltip"] = value; }
            get { return (string)this["Tooltip"]; }
        }

        [ConfigurationProperty("UoM", DefaultValue = "s", IsRequired = false)]
        public string UoM
        {
            set { this["UoM"] = value; }
            get { return (string)this["UoM"]; }
        }
        #endregion
    }

    public class KVPCollection : ConfigurationElementCollection
    {
        #region Key-Value-Pair Collection
        public KeyValuePair this[int idx]
        {
            get { return base.BaseGet(idx) as KeyValuePair; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new KeyValuePair();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((KeyValuePair)element).ID;
        }
        #endregion
    }

    public class KeyValuePair : ConfigurationElement
    {
        #region Key-Value-Pair
        public KeyValuePair()
        {
        }

        [ConfigurationProperty("ID", DefaultValue = 0, IsRequired = true)]
        public int ID
        {
            set { this["ID"] = value; }
            get { return (int)this["ID"]; }
        }

        [ConfigurationProperty("Value", DefaultValue = 0, IsRequired = true)]
        public int Value
        {
            set { this["Value"] = value; }
            get { return (int)this["Value"]; }
        }

        [ConfigurationProperty("Description", DefaultValue = "Empty", IsRequired = true)]
        public string Description
        {
            set { this["Description"] = value; }
            get { return (string)this["Description"]; }
        }

        [ConfigurationProperty("IsVisible", DefaultValue = "false", IsRequired = false)]
        public bool IsVisible
        {
            set { this["IsVisible"] = value; }
            get { return (bool)this["IsVisible"]; }
        }

        [ConfigurationProperty("Tooltip", DefaultValue = "", IsRequired = false)]
        public string Tooltip
        {
            set { this["Tooltip"] = value; }
            get { return (string)this["Tooltip"]; }
        }

        [ConfigurationProperty("Max", DefaultValue = 100, IsRequired = false)]
        public int Max
        {
            set { this["Max"] = value; }
            get { return (int)this["Max"]; }
        }

        [ConfigurationProperty("Min", DefaultValue = 0, IsRequired = false)]
        public int Min
        {
            set { this["Min"] = value; }
            get { return (int)this["Min"]; }
        }

        #endregion
    }

    public class TestRunCollection : ConfigurationElementCollection
    {
        #region TestRun Collection
        public TestRun this[int idx]
        {
            get { return base.BaseGet(idx) as TestRun; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TestRun();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TestRun)element).ID;
        }
        #endregion
    }

    public class TestRun : ConfigurationElement
    {
        #region TestRun
        public TestRun()
        {
        }

        [ConfigurationProperty("ID", DefaultValue = 0, IsRequired = true)]
        public int ID
        {
            set { this["ID"] = value; }
            get { return (int)this["ID"]; }
        }

        [ConfigurationProperty("SeqNum", DefaultValue = TestRunEnum.SN.BeginSampleTest, IsRequired = true)]
        public TestRunEnum.SN SeqNum
        {
            set { this["SeqNum"] = value; }
            get { return (TestRunEnum.SN)this["SeqNum"]; }
        }

        [ConfigurationProperty("Desc", DefaultValue = "{Put Test Description Here}", IsRequired = false)]
        public string Desc
        {
            set { this["Desc"] = value; }
            get { return (string)this["Desc"]; }
        }

        [ConfigurationProperty("IsActive", DefaultValue = false, IsRequired = false)]
        public bool IsActive
        {
            set { this["IsActive"] = value; }
            get { return (bool)this["IsActive"]; }
        }

        [ConfigurationProperty("IsMultipleCycle", DefaultValue = false, IsRequired = false)]
        public bool IsMultipleCycle
        {
            set { this["IsMultipleCycle"] = value; }
            get { return (bool)this["IsMultipleCycle"]; }
        }
        #endregion
    }

    public class SeqOptionCollection : ConfigurationElementCollection
    {
        public SeqOption this[int idx]
        {
            get { return base.BaseGet(idx) as SeqOption; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SeqOption();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SeqOption)element).ID;
        }
    }

    public class SeqOption : ConfigurationElement
    {

        [ConfigurationProperty("ID", DefaultValue = 0, IsRequired = true)]
        public int ID
        {
            set { this["ID"] = value; }
            get { return (int)this["ID"]; }
        }

        [ConfigurationProperty("Value", DefaultValue = 0, IsRequired = true)]
        public int Value
        {
            set { this["Value"] = value; }
            get { return (int)this["Value"]; }
        }

        [ConfigurationProperty("ChoiceCollection", IsRequired = false)]
        public ChoiceCollection ChoiceCollection
        {
            set { this["ChoiceCollection"] = value; }
            get { return (ChoiceCollection)this["ChoiceCollection"]; }
        }

        [ConfigurationProperty("Description", DefaultValue = "Empty", IsRequired = true)]
        public string Description
        {
            set { this["Description"] = value; }
            get { return (string)this["Description"]; }
        }

        [ConfigurationProperty("IsVisible", DefaultValue = "false", IsRequired = false)]
        public bool IsVisible
        {
            set { this["IsVisible"] = value; }
            get { return (bool)this["IsVisible"]; }
        }

        [ConfigurationProperty("Tooltip", DefaultValue = "", IsRequired = false)]
        public string Tooltip
        {
            set { this["Tooltip"] = value; }
            get { return (string)this["Tooltip"]; }
        }
    }

    public class ChoiceCollection : ConfigurationElementCollection
    {
        public Choice this[int idx]
        {
            get { return base.BaseGet(idx) as Choice; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Choice();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Choice)element).ChoiceID;
        }

        public void Add(Choice choice)
        {
            base.BaseAdd(choice);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        public Choice Get(int choiceID)
        {
            Choice choice = null;

            foreach (Choice c in this)
            {
                if (c.ChoiceID == choiceID)
                {
                    choice = c;
                    break;
                }
            }
            return choice;
        }
    }

    public class Choice : ConfigurationElement
    {
        [ConfigurationProperty("ChoiceID", DefaultValue = 0, IsRequired = true)]
        public int ChoiceID
        {
            set { this["ChoiceID"] = value; }
            get { return (int)this["ChoiceID"]; }
        }

        [ConfigurationProperty("ChoiceDesc", DefaultValue = "Empty", IsRequired = true)]
        public string ChoiceDesc
        {
            set { this["ChoiceDesc"] = value; }
            get { return (string)this["ChoiceDesc"]; }
        }

        public Choice()
        {
        }
    }
}
