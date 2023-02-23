using GreatechApp.Core.Variable;
using System;
using System.Collections;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ConfigManager
{
    public class ErrorConfig : BaseCfg
    {
        private static readonly string SectionName = "Err_Lib";

        public ErrorConfig()
        {
        }

        public static ErrorConfig Open(string path, string libName)
        {
            string targetPath = Path.Combine(path, Global.CurrentCulture.Name, libName);
            if (string.IsNullOrEmpty(targetPath))
            {
                // When system config does not indicate any reference, just return null.
                // Client code is responsible to make further evaluation before usage.
                return null;
            }
            if (m_InstanceTbl.Keys.Contains(targetPath))
            {
                // return the same instance - so that UI & SeqManager can share.
                return m_InstanceTbl[targetPath] as ErrorConfig;
            }
            Configuration configObj = null;
            ErrorConfig errLib = Open("ErrLib", targetPath, SectionName, out configObj) as ErrorConfig;
            // We would like to build a lookup table of configuration object based on the path value.
            // Each module seq needs to have a separate instance of configObj.
            if (!m_CfgTbl.Keys.Contains(targetPath))
            {
                m_CfgTbl.Add(targetPath, configObj);
                m_InstanceTbl.Add(targetPath, errLib);
            }
            return errLib;
        }

        public static void Save()
        {
            Save(m_CfgTbl, SectionName);
        }

        public static void Save(ErrorConfig errLib)
        {
            Save(errLib, m_CfgTbl, SectionName);
        }

        #region Commmented
        //public void AddIOList(int ioNum)
        //{
        //    lock (this)
        //    {
        //        m_IOAdd.Enqueue(ioNum);
        //    }
        //}

        //public void AddMtrList(string axisInfo)
        //{
        //    lock (this)
        //    {
        //        m_MtrAxis.Enqueue(axisInfo);
        //    }
        //}

        //public object[] GetFaultyIO()
        //{
        //    return m_IOAdd.ToArray();
        //}

        //public object[] GetFaultyAxis()
        //{
        //    return m_MtrAxis.ToArray();
        //}

        //public int IOAdd_Size
        //{
        //    get { return m_IOAdd.Count; }
        //}

        //public int MtrAxis_Size
        //{
        //    get { return m_MtrAxis.Count; }
        //}

        ///// <summary>
        ///// Clear IO faulty devices list.
        ///// </summary>
        //public void ClrIOList()
        //{
        //    m_IOAdd.Clear();
        //}

        ///// <summary>
        ///// Clear Mtr Axis faulty devices list.
        ///// </summary>
        //public void ClrAxisList()
        //{
        //    m_MtrAxis.Clear();
        //}

        ///// <summary>
        ///// Clear IO & Mtr Axis faulty devices list.
        ///// </summary>
        //public void ClrAllList()
        //{
        //    m_IOAdd.Clear();
        //    m_MtrAxis.Clear();
        //}
        #endregion

        [ConfigurationProperty("ErrTable", IsRequired = true)]
        public ErrTblCollection ErrTable
        {
            set { this["ErrTable"] = value; }
            get { return (ErrTblCollection)this["ErrTable"]; }
        }
    }

    [ConfigurationCollection(typeof(Alarm), AddItemName = "Alarm")]
    public class ErrTblCollection : ConfigurationElementCollection
    {
        #region ErrTbl Collection
        protected override ConfigurationElement CreateNewElement()
        {
            return new Alarm();
        }

        public Alarm this[int idx]
        {
            get { return base.BaseGet(idx) as Alarm; }
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Alarm)element).Code;
        }
        #endregion
    }

    public class Alarm : ConfigurationElement
    {
        public bool IsRetest { get; set; }

        #region Alarm
        [ConfigurationProperty("Code", DefaultValue = 0, IsRequired = true)]
        public int Code
        {
            set { this["Code"] = value; }
            get { return (int)this["Code"]; }
        }

        [ConfigurationProperty("Station", DefaultValue = "", IsRequired = true)]
        public string Station
        {
            set { this["Station"] = value; }
            get { return (string)this["Station"]; }
        }

        [ConfigurationProperty("Cause", DefaultValue = "", IsRequired = true)]
        public string Cause
        {
            set { this["Cause"] = value; }
            get { return (string)this["Cause"]; }
        }

        [ConfigurationProperty("AlarmType", DefaultValue = "", IsRequired = true)]
        public string AlarmType
        {
            set { this["AlarmType"] = value; }
            get { return (string)this["AlarmType"]; }
        }

        [ConfigurationProperty("Recovery", DefaultValue = "", IsRequired = true)]
        public string Recovery
        {
            set { this["Recovery"] = value; }
            get { return (string)this["Recovery"]; }
        }

        [ConfigurationProperty("RetestOption", DefaultValue = false, IsRequired = true)]
        public bool RetestOption
        {
            set { this["RetestOption"] = value; }
            get { return (bool)this["RetestOption"]; }
        }

        [ConfigurationProperty("RetestDefault", DefaultValue = false, IsRequired = true)]
        public bool RetestDefault
        {
            set { this["RetestDefault"] = value; }
            get { return (bool)this["RetestDefault"]; }
        }

        [ConfigurationProperty("IsStoppage", DefaultValue = true, IsRequired = true)]
        public bool IsStoppage
        {
            set { this["IsStoppage"] = value; }
            get { return (bool)this["IsStoppage"]; }
        }

        [ConfigurationProperty("PicPath", DefaultValue = "no-image.jpg", IsRequired = true)]
        public string Pic_Path
        {
            set { this["PicPath"] = value; }
            get { return (string)this["PicPath"]; }
        }
        #endregion
    }
}
