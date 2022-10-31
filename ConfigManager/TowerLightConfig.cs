using GreatechApp.Core.Enums;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigManager
{
    public class TowerLightConfig : BaseCfg
    {
        private static readonly string SectionName = "TowerLight_Config";

        // Constructor
        public TowerLightConfig()
        {
        }

        public static TowerLightConfig Open(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            if (m_InstanceTbl.Keys.Contains(path))
            {
                // return the same instance - so that UI & SeqManager can share.
                return m_InstanceTbl[path] as TowerLightConfig;
            }
            Configuration configObj = null;
            TowerLightConfig towerLightSetting = Open("TowerLightConfig", path, SectionName, out configObj) as TowerLightConfig;

            if (!m_CfgTbl.Keys.Contains(path))
            {
                m_CfgTbl.Add(path, configObj);
                m_InstanceTbl.Add(path, towerLightSetting);
            }
            return towerLightSetting;
        }

        // Save to single file
        public static void Save(string path)
        {
            Save(path, m_CfgTbl, SectionName);
        }

        public static void Save(TowerLightConfig towerLightCfg)
        {
            Save(towerLightCfg, m_CfgTbl, SectionName);
        }

        [ConfigurationProperty("TimerInterval")]
        public TimerIntervalCfg TimerInterval
        {
            set { this["TimerInterval"] = value; }
            get { return (TimerIntervalCfg)this["TimerInterval"]; }
        }

        [ConfigurationProperty("Setting", IsRequired = false)]
        public TLSettingCollection Setting
        {
            set { this["Setting"] = value; }
            get { return (TLSettingCollection)this["Setting"]; }
        }

        public class TimerIntervalCfg : ConfigurationElement
        {
            #region Timer Interval
            [ConfigurationProperty("LightTimerInterval", DefaultValue = 1000, IsRequired = true)]
            public int LightTimerInterval
            {
                set { this["LightTimerInterval"] = value; }
                get { return (int)this["LightTimerInterval"]; }
            }

            [ConfigurationProperty("BuzzerTimerInterval", DefaultValue = 1000, IsRequired = true)]
            public int BuzzerTimerInterval
            {
                set { this["BuzzerTimerInterval"] = value; }
                get { return (int)this["BuzzerTimerInterval"]; }
            }
            #endregion
        }

        public class TLSettingCollection : ConfigurationElementCollection, IEnumerable<TLSetting>
        {
            #region Setting Collection
            public TLSetting this[int idx]
            {
                get { return base.BaseGet(idx) as TLSetting; }
            }

            protected override ConfigurationElement CreateNewElement()
            {
                return new TLSetting();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((TLSetting)element).ID;
            }

            IEnumerator<TLSetting> IEnumerable<TLSetting>.GetEnumerator()
            {
                foreach (var key in this.BaseGetAllKeys())
                {
                    yield return (TLSetting)BaseGet(key);
                }
            }
            public void Add(TLSetting newItem)
            {
                BaseAdd(newItem);
            }
            #endregion
        }

        public class TLSetting : ConfigurationElement
        {
            public TLSetting()
            {
                Name = null;
            }

            [ConfigurationProperty("ID", DefaultValue = 0, IsRequired = true)]
            public int ID
            {
                set { this["ID"] = value; }
                get { return (int)this["ID"]; }
            }

            [ConfigurationProperty("Name", IsRequired = true)]
            public string Name
            {
                set { this["Name"] = value; }
                get { return (string)this["Name"]; }
            }

            [ConfigurationProperty("RedState", IsRequired = true, DefaultValue = TowerLightState.OFF)]
            public TowerLightState RedState
            {
                set { this["RedState"] = value; }
                get { return (TowerLightState)this["RedState"]; }
            }

            [ConfigurationProperty("YellowState", IsRequired = true, DefaultValue = TowerLightState.OFF)]
            public TowerLightState YellowState
            {
                set { this["YellowState"] = value; }
                get { return (TowerLightState)this["YellowState"]; }
            }

            [ConfigurationProperty("GreenState", IsRequired = true, DefaultValue = TowerLightState.OFF)]
            public TowerLightState GreenState
            {
                set { this["GreenState"] = value; }
                get { return (TowerLightState)this["GreenState"]; }
            }

            [ConfigurationProperty("BuzzerState", IsRequired = true, DefaultValue = TowerLightState.OFF)]
            public TowerLightState BuzzerState
            {
                set { this["BuzzerState"] = value; }
                get { return (TowerLightState)this["BuzzerState"]; }
            }
        }
    }
}
