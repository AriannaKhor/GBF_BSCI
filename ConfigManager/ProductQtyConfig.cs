using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigManager
{
   public class ProductQtyConfig : BaseCfg
    {
        private static readonly string SectionName = "ProductQty_Config";

        public ProductQtyConfig()
        {

        }

        public static ProductQtyConfig Open(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            if (m_InstanceTbl.Keys.Contains(path))
            {
                // return the same instance - so that UI & SeqManager can share.
                return m_InstanceTbl[path] as ProductQtyConfig;
            }
            Configuration configObj = null;
            ProductQtyConfig productQtySetting = Open("ProductQtyConfig", path, SectionName, out configObj) as ProductQtyConfig;

            if (!m_CfgTbl.Keys.Contains(path))
            {
                m_CfgTbl.Add(path, configObj);
                m_InstanceTbl.Add(path, productQtySetting);
            }
            return productQtySetting;
        }

        public static void Save(ProductQtyConfig productQtyCfg)
        {
            Save(productQtyCfg, m_CfgTbl, SectionName);
        }

        [ConfigurationProperty("Setting", IsRequired = false)]
        public PrdQtySettingCollection Setting
        {
            set { this["Setting"] = value; }
            get { return (PrdQtySettingCollection)this["Setting"]; }
        }

        public class PrdQtySettingCollection : ConfigurationElementCollection, IEnumerable<PrdQtySetting>
        {
            #region Setting Collection
            public PrdQtySetting this[int idx]
            {
                get { return base.BaseGet(idx) as PrdQtySetting; }
            }

            protected override ConfigurationElement CreateNewElement()
            {
                return new PrdQtySetting();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((PrdQtySetting)element).CurrentConfiguration;
            }

            IEnumerator<PrdQtySetting> IEnumerable<PrdQtySetting>.GetEnumerator()
            {
                foreach (var key in this.BaseGetAllKeys())
                {
                    yield return (PrdQtySetting)BaseGet(key);
                }
            }
            public void Add(PrdQtySetting newItem)
            {
                BaseAdd(newItem);
            }
            #endregion
        }

        public class PrdQtySetting : ConfigurationElement
        {
            public PrdQtySetting()
            {
            }

            [ConfigurationProperty("Id")]
            public int Id
            {
                set { this["Id"] = value; }
                get { return (int)this["Id"]; }
            }

            [ConfigurationProperty("Description")]
            public string Description
            {
                set { this["Description"] = value; }
                get { return (string)this["Description"]; }
            }

            [ConfigurationProperty("MinQuantity")]
            public string MinQuantity
            {
                set { this["MinQuantity"] = value; }
                get { return (string)this["MinQuantity"]; }
            }

            [ConfigurationProperty("MaxQuantity")]
            public string MaxQuantity
            {
                set { this["MaxQuantity"] = value; }
                get { return (string)this["MaxQuantity"]; }
            }
        }
    }
}
