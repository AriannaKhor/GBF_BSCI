using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Diagnostics;
using GreatechApp.Core.Enums;

namespace ConfigManager
{
    public class MotionConfig : BaseCfg
    {
        private static readonly string SectionName = "Mot_Config";
        private static Dictionary<string, Configuration> m_MotTbl = new Dictionary<string, Configuration>();

        public MotionConfig()
        {
        }

        public static MotionConfig Open(string path)
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
                return m_InstanceTbl[path] as MotionConfig;
            }
            Configuration configObj = null;
            MotionConfig motionSetting = Open("MotionSetting", path, SectionName, out configObj) as MotionConfig;
            if (!m_MotTbl.Keys.Contains(path))
            {
                m_MotTbl.Add(path, configObj);
                m_InstanceTbl.Add(path, motionSetting);
            }
            return motionSetting;
        }

        public static void Save()
        {
            Save(m_MotTbl, SectionName);
        }

        public static void Save(string path)
        {
            Save(path, m_MotTbl, SectionName);
        }

        public static void Save(MotionConfig motCfg)
        {
            Save(motCfg, m_MotTbl, SectionName);
        }

        public static void SaveAs(string path, string newPath)
        {
            SaveAs(path, m_MotTbl, newPath, SectionName);
        }

        [ConfigurationProperty("Velocity", IsRequired = true)]
        public MotProfileCollection Velocity
        {
            set { this["Velocity"] = value; }
            get { return (MotProfileCollection)this["Velocity"]; }
        }

        [ConfigurationProperty("Position", IsRequired = true)]
        public MotPosCollection Position
        {
            set { this["Position"] = value; }
            get { return (MotPosCollection)this["Position"]; }
        }

        [ConfigurationProperty("Option", IsRequired = true)]
        public MotOption Option
        {
            set { this["Option"] = value; }
            get { return (MotOption)this["Option"]; }
        }

        [ConfigurationProperty("Axis", IsRequired = true)]
        public MotAxis Axis
        {
            set { this["Axis"] = value; }
            get { return (MotAxis)this["Axis"]; }
        }

        [ConfigurationProperty("ViewCfg", IsRequired = true)]
        public ViewCfg ViewCfg
        {
            set { this["ViewCfg"] = value; }
            get { return (ViewCfg)this["ViewCfg"]; }
        }

        [ConfigurationProperty("Dir", IsRequired = true)]
        public MotDir Dir
        {
            set { this["Dir"] = value; }
            get { return (MotDir)this["Dir"]; }
        }
    }

    public class MotProfileCollection : ConfigurationElementCollection
    {
        #region Motion Profile Collection
        public MotProfile this[int idx]
        {
            get { return base.BaseGet(idx) as MotProfile; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new MotProfile();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MotProfile)element).ID;
        }
        #endregion
    }

    public class MotProfile : ConfigurationElement
    {
        #region Motion Profile
        public MotProfile()
        {
        }

        [ConfigurationProperty("ID", DefaultValue = 0, IsRequired = true)]
        public int ID
        {
            set { this["ID"] = value; }
            get { return (int)this["ID"]; }
        }

        [ConfigurationProperty("DriveVel", DefaultValue = (long)1000, IsRequired = true)]
        public long DriveVel
        {
            set { this["DriveVel"] = value; }
            get { return (long)this["DriveVel"]; }
        }

        [ConfigurationProperty("Acc", DefaultValue = 10000.0, IsRequired = true)]
        public double Acc
        {
            set { this["Acc"] = value; }
            get { return (double)this["Acc"]; }
        }

        [ConfigurationProperty("Dcc", DefaultValue = 10000.0, IsRequired = false)]
        public double Dcc
        {
            set { this["Dcc"] = value; }
            get { return (double)this["Dcc"]; }
        }

        [ConfigurationProperty("JerkTime", DefaultValue = 0.1, IsRequired = false)]
        public double JerkTime
        {
            set { this["JerkTime"] = value; }
            get { return (double)this["JerkTime"]; }
        }

        [ConfigurationProperty("KillDcc", DefaultValue = 0.1, IsRequired = false)]
        public double KillDcc
        {
            set { this["KillDcc"] = value; }
            get { return (double)this["KillDcc"]; }
        }

        [ConfigurationProperty("MaxVel", DefaultValue = (long)0, IsRequired = true)]
        public long MaxVel
        {
            set { this["MaxVel"] = value; }
            get { return (long)this["MaxVel"]; }
        }

        [ConfigurationProperty("MaxAcc", DefaultValue = 0.0, IsRequired = true)]
        public double MaxAcc
        {
            set { this["MaxAcc"] = value; }
            get { return (double)this["MaxAcc"]; }
        }

        [ConfigurationProperty("MaxDcc", DefaultValue = 0.0, IsRequired = false)]
        public double MaxDcc
        {
            set { this["MaxDcc"] = value; }
            get { return (double)this["MaxDcc"]; }
        }

        [ConfigurationProperty("ProfileName", DefaultValue = "Empty", IsRequired = false)]
        public string ProfileName
        {
            set { this["ProfileName"] = value; }
            get { return (string)this["ProfileName"]; }
        }

        [ConfigurationProperty("IsVisible", DefaultValue = false, IsRequired = false)]
        public bool IsVisible
        {
            set { this["IsVisible"] = value; }
            get { return (bool)this["IsVisible"]; }
        }
        #endregion
    }

    public class MotPosCollection : ConfigurationElementCollection, IEnumerable<PosProfile>
    {
        #region Position Profile Collection
        public PosProfile this[int idx]
        {
            get { return base.BaseGet(idx) as PosProfile; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PosProfile();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PosProfile)element).ID;

        }

        IEnumerator<PosProfile> IEnumerable<PosProfile>.GetEnumerator()
        {
            foreach (var key in this.BaseGetAllKeys())
            {
                yield return (PosProfile)BaseGet(key);
            }
        }
        #endregion
    }

    public class PosProfile : ConfigurationElement
    {
        #region Position Profile
        public PosProfile()
        {
        }

        [ConfigurationProperty("ID", DefaultValue = 0, IsRequired = true)]
        public int ID
        {
            set { this["ID"] = value; }
            get { return (int)this["ID"]; }
        }

        [ConfigurationProperty("SetPoint", DefaultValue = 0.0, IsRequired = true)]
        public double Point
        {
            set { this["SetPoint"] = value; }
            get { return (double)this["SetPoint"]; }
        }

        [ConfigurationProperty("UoM", DefaultValue = "mm", IsRequired = false)]
        public string UoM
        {
            set { this["UoM"] = value; }
            get { return (string)this["UoM"]; }
        }

        [ConfigurationProperty("SoftLimit", DefaultValue = 0, IsRequired = false)]
        public int SoftLimit
        {
            set { this["SoftLimit"] = value; }
            get { return (int)this["SoftLimit"]; }
        }

        [ConfigurationProperty("Editable", DefaultValue = true, IsRequired = false)]
        public bool Editable
        {
            // UI can query this state to decide whether to allow user to change the position value.
            set { this["Editable"] = value; }
            get { return (bool)this["Editable"]; }
        }

        [ConfigurationProperty("Description", DefaultValue = "", IsRequired = false)]
        public string Description
        {
            set { this["Description"] = value; }
            get { return (string)this["Description"]; }
        }

        [ConfigurationProperty("IsVisible", DefaultValue = false, IsRequired = false)]
        public bool IsVisible
        {
            set { this["IsVisible"] = value; }
            get { return (bool)this["IsVisible"]; }
        }
        #endregion
    }

    public class MotOption : ConfigurationElement
    {
        #region MotOption
        public MotOption()
        {
        }

        [ConfigurationProperty("ChkAlarm", DefaultValue = true, IsRequired = true)]
        public bool ChkAlarm
        {
            set { this["ChkAlarm"] = value; }
            get { return (bool)this["ChkAlarm"]; }
        }

        [ConfigurationProperty("ChkReady", DefaultValue = true, IsRequired = true)]
        public bool ChkReady
        {
            set { this["ChkReady"] = value; }
            get { return (bool)this["ChkReady"]; }
        }

        [ConfigurationProperty("ChkInPos", DefaultValue = true, IsRequired = true)]
        public bool ChkInPos
        {
            set { this["ChkInPos"] = value; }
            get { return (bool)this["ChkInPos"]; }
        }

        [ConfigurationProperty("ChkFwdLimit", DefaultValue = true, IsRequired = true)]
        public bool ChkFwdLimit
        {
            set { this["ChkFwdLimit"] = value; }
            get { return (bool)this["ChkFwdLimit"]; }
        }

        [ConfigurationProperty("ChkRevLimit", DefaultValue = true, IsRequired = true)]
        public bool ChkRevLimit
        {
            set { this["ChkRevLimit"] = value; }
            get { return (bool)this["ChkRevLimit"]; }
        }

        [ConfigurationProperty("ChkAxisHome", DefaultValue = true, IsRequired = true)]
        public bool ChkAxisHome
        {
            set { this["ChkAxisHome"] = value; }
            get { return (bool)this["ChkAxisHome"]; }
        }

        [ConfigurationProperty("AlarmContact", DefaultValue = Contact.NC, IsRequired = true)]
        public Contact AlarmContact
        {
            set { this["AlarmContact"] = value; }
            get { return (Contact)this["AlarmContact"]; }
        }

        [ConfigurationProperty("ReadyContact", DefaultValue = Contact.NC, IsRequired = true)]
        public Contact ReadyContact
        {
            set { this["ReadyContact"] = value; }
            get { return (Contact)this["ReadyContact"]; }
        }

        [ConfigurationProperty("UseIORstAlarm", DefaultValue = false, IsRequired = true)]
        public bool UseIORstAlarm
        {
            set { this["UseIORstAlarm"] = value; }
            get { return (bool)this["UseIORstAlarm"]; }
        }

        #endregion
    }

    public class MotAxis : ConfigurationElement
    {
        public enum DriveMethod
        {
            Linear,
            Rotary
        }

        public enum MotorType
        {
            Servo,
            Stepper,
        }

        #region MotAxis
        public MotAxis()
        {
        }

        [ConfigurationProperty("Name", DefaultValue = "", IsRequired = true)]
        public string Name
        {
            set { this["Name"] = value; }
            get { return (string)this["Name"]; }
        }

        [ConfigurationProperty("CardID", DefaultValue = "0", IsRequired = true)]
        public int CardID
        {
            set { this["CardID"] = value; }
            get { return (int)this["CardID"]; }
        }

        [ConfigurationProperty("AxisID", DefaultValue = "0", IsRequired = true)]
        public byte AxisID
        {
            set { this["AxisID"] = value; }
            get { return (byte)this["AxisID"]; }
        }

        [ConfigurationProperty("Revolution", DefaultValue = "10000", IsRequired = true)]
        public int Revolution
        {
            set { this["Revolution"] = value; }
            get { return (int)this["Revolution"]; }
        }

        [ConfigurationProperty("Pitch", DefaultValue = 10.0f, IsRequired = true)]
        public float Pitch
        {
            set { this["Pitch"] = value; }
            get { return (float)this["Pitch"]; }
        }

        [ConfigurationProperty("UoM", DefaultValue = "mm", IsRequired = true)]
        public string UoM
        {
            // Meant for Pitch
            set { this["UoM"] = value; }
            get { return (string)this["UoM"]; }
        }

        [ConfigurationProperty("Type", DefaultValue = MotorType.Servo, IsRequired = true)]
        public MotorType Type
        {
            set { this["Type"] = value; }
            get { return (MotorType)this["Type"]; }
        }

        [ConfigurationProperty("System", DefaultValue = DriveMethod.Rotary, IsRequired = true)]
        public DriveMethod System
        {
            set { this["System"] = value; }
            get { return (DriveMethod)this["System"]; }
        }

        [ConfigurationProperty("SetZeroPosAfterGoLoad", DefaultValue = false, IsRequired = false)]
        public bool SetZeroPosAfterGoLoad
        {
            set { this["SetZeroPosAfterGoLoad"] = value; }
            get { return (bool)this["SetZeroPosAfterGoLoad"]; }
        }

        [ConfigurationProperty("HomeMode", DefaultValue = (short)7, IsRequired = false)]
        public short HomeMode
        {
            set { this["HomeMode"] = value; }
            get { return (short)this["HomeMode"]; }
        }

        public bool IsHome { get; set; }
        #endregion
    }

    public class ViewCfg : ConfigurationElement
    {
        #region ViewCfg
        // Mainly for MotorPanel's Left/Right or Up/Down direction pairing button.
        public ViewCfg()
        {
        }

        // Actions-go-left-icon.png, Actions-go-right-icon.png
        // Actions-go-up-icon.png, Actions-go-down-icon.png
        // CCW-arrow-icon.png, CW-arrow-icon.png
        [ConfigurationProperty("Dir1", DefaultValue = "Left", IsRequired = true)]
        public string Dir1
        {
            set { this["Dir1"] = value; }
            get { return (string)this["Dir1"]; }
        }

        [ConfigurationProperty("Icon1", DefaultValue = "", IsRequired = true)]
        public string Icon1
        {
            set { this["Icon1"] = value; }
            get { return (string)this["Icon1"]; }
        }

        [ConfigurationProperty("Sign1", DefaultValue = "-1", IsRequired = true)]
        public int Sign1
        {
            set { this["Sign1"] = value; }
            get { return (int)this["Sign1"]; }
        }

        [ConfigurationProperty("Dir2", DefaultValue = "Right", IsRequired = true)]
        public string Dir2
        {
            set { this["Dir2"] = value; }
            get { return (string)this["Dir2"]; }
        }

        [ConfigurationProperty("Icon2", DefaultValue = "", IsRequired = true)]
        public string Icon2
        {
            set { this["Icon2"] = value; }
            get { return (string)this["Icon2"]; }
        }

        [ConfigurationProperty("Sign2", DefaultValue = "1", IsRequired = true)]
        public int Sign2
        {
            set { this["Sign2"] = value; }
            get { return (int)this["Sign2"]; }
        }

        [ConfigurationProperty("VelUoM", DefaultValue = "pps", IsRequired = true)]
        public string VelUoM
        {
            set { this["VelUoM"] = value; }
            get { return (string)this["VelUoM"]; }
        }

        [ConfigurationProperty("AccUoM", DefaultValue = "pps2", IsRequired = true)]
        public string AccUoM
        {
            set { this["AccUoM"] = value; }
            get { return (string)this["AccUoM"]; }
        }
        #endregion
    }

    public class MotDir : ConfigurationElement
    {
        #region MotDir
        public MotDir()
        {
        }

        [ConfigurationProperty("Opr", DefaultValue = (short)1, IsRequired = true)]
        public short Opr
        {
            // For Absoluate move only.
            set { this["Opr"] = value; }
            get { return (short)this["Opr"]; }
        }

        [ConfigurationProperty("LimitOffset", DefaultValue = (short)1, IsRequired = true)]
        public short LimitOffset
        {
            set { this["LimitOffset"] = value; }
            get { return (short)this["LimitOffset"]; }
        }

        [ConfigurationProperty("Right", DefaultValue = (short)1, IsRequired = false)]
        public short Right
        {
            set { this["Right"] = value; }
            get { return (short)this["Right"]; }
        }

        [ConfigurationProperty("Left", DefaultValue = (short)-1, IsRequired = false)]
        public short Left
        {
            set { this["Left"] = value; }
            get { return (short)this["Left"]; }
        }

        [ConfigurationProperty("Up", DefaultValue = (short)1, IsRequired = false)]
        public short Up
        {
            set { this["Up"] = value; }
            get { return (short)this["Up"]; }
        }

        [ConfigurationProperty("Down", DefaultValue = (short)-1, IsRequired = false)]
        public short Down
        {
            set { this["Down"] = value; }
            get { return (short)this["Down"]; }
        }

        [ConfigurationProperty("CW", DefaultValue = (short)1, IsRequired = false)]
        public short CW
        {
            set { this["CW"] = value; }
            get { return (short)this["CW"]; }
        }

        [ConfigurationProperty("CCW", DefaultValue = (short)-1, IsRequired = false)]
        public short CCW
        {
            set { this["CCW"] = value; }
            get { return (short)this["CCW"]; }
        }

        [ConfigurationProperty("Front", DefaultValue = (short)1, IsRequired = false)]
        public short Front
        {
            set { this["Front"] = value; }
            get { return (short)this["Front"]; }
        }

        [ConfigurationProperty("Rear", DefaultValue = (short)-1, IsRequired = false)]
        public short Rear
        {
            set { this["Rear"] = value; }
            get { return (short)this["Rear"]; }
        }

        [ConfigurationProperty("Fwd", DefaultValue = (short)1, IsRequired = false)]
        public short Fwd
        {
            set { this["Fwd"] = value; }
            get { return (short)this["Fwd"]; }
        }

        [ConfigurationProperty("Rev", DefaultValue = (short)-1, IsRequired = false)]
        public short Rev
        {
            set { this["Rev"] = value; }
            get { return (short)this["Rev"]; }
        }

        [ConfigurationProperty("Index", DefaultValue = (short)1, IsRequired = false)]
        public short Index
        {
            set { this["Index"] = value; }
            get { return (short)this["Index"]; }
        }
        #endregion
    }
}
