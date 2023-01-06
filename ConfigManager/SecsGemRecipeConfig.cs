using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigManager
{
    public class SecsGemRecipeConfig : BaseCfg
    {
        private static readonly string SectionName = "SecsGem_Recipe_Config";

        public SecsGemRecipeConfig()
        {

        }

        public static SecsGemRecipeConfig Open(string path)
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
                return m_InstanceTbl[path] as SecsGemRecipeConfig;
            }
            Configuration configObj = null;
            SecsGemRecipeConfig rcpCounter = Open("SecsGemRecipeConfig", path, SectionName, out configObj) as SecsGemRecipeConfig;

            if (!m_CfgTbl.Keys.Contains(path))
            {
                m_CfgTbl.Add(path, configObj);
                m_InstanceTbl.Add(path, rcpCounter);
            }
            return rcpCounter;
        }

        public static void Save()
        {
            Save(m_CfgTbl, SectionName);
        }

        public static void Save(string path)
        {
            Save(path, m_CfgTbl, SectionName);
        }

        public static void Save(SecsGemRecipeConfig secsgemrcpCfg)
        {
            Save(secsgemrcpCfg, m_CfgTbl, SectionName);
        }

        [ConfigurationProperty("Recipe")]
        public SecsGemRcpCollection SecsGemRcpCollection
        {
            set { this["Recipe"] = value; }
            get { return (SecsGemRcpCollection)this["Recipe"]; }
        }
    }

    public class SecsGemRcpCollection : ConfigurationElementCollection
    {
       
        public Recipe this[int idx]
        {
            get { return base.BaseGet(idx) as Recipe; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Recipe();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Recipe)element).ID;
        }
      
    }

    public class Recipe : ConfigurationElement
    {
     
        public Recipe()
        {
        }

        [ConfigurationProperty("ID")]
        public int ID
        {
            set { this["ID"] = value; }
            get { return (int)this["ID"]; }
        }

        [ConfigurationProperty("RecipeDir")]
        public string RecipeDir
        {
            set { this["RecipeDir"] = value; }
            get { return (string)this["RecipeDir"]; }
        }
    }
}

