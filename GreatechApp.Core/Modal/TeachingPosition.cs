using System.Collections.Generic;
using System.Windows;
using GreatechApp.Core.Enums;
using Prism.Mvvm;

namespace GreatechApp.Core.Modal
{
    public class MotorTabList : BindableBase
    {
        public string MotReference { get; set; }
        public string AxisName { get; set; }
        public int MotorIndex { get; set; }
        public int AxisID { get; set; }
        public int CardID { get; set; }
        public List<TeachingPosition> Positions { get; set; }
        public List<Velocity> Velocities { get; set; }

        private bool m_IsAllowAccess;
        public bool IsAllowAccess
        {
            get { return m_IsAllowAccess; }
            set { SetProperty(ref m_IsAllowAccess, value); }
        }

        #region Axis
        public int Revolution { get; set; }
        public float Pitch { get; set; }
        public string UoM { get; set; }
        public string Type { get; set; }
        public string System { get; set; }
        public bool SetZeroPosAfterGoLoad { get; set; }
        #endregion

        #region Option
        public bool IsChkAlarm { get; set; }
        public bool IsChkReady { get; set; }
        public bool IsChkInPos{ get; set; }
        public bool IsChkFwdLmt { get; set; }
        public bool IsChkRevLmt { get; set; }
        public bool IsChkAxisHome { get; set; }
        public Contact AlarmContact { get; set; }
        public Contact ReadyContact { get; set; }
        public bool IsUseIORstAlarm { get; set; }
        #endregion

        #region View Config
        public string Icon1 { get; set; }
        public string Icon2 { get; set; }
        public string Dir1 { get; set; }
        public string Dir2 { get; set; }
        public int Sign1 { get; set; }
        public int Sign2 { get; set; }
        #endregion
    }

    public class TeachingPosition : BindableBase
    {
        public int ID { get; set; }
        public string TeachingPointName { get; set; }
        public string TeachingPointUOM { get; set; }

        private double m_TeachingPointValue;
        public double TeachingPointValue
        {
            get { return m_TeachingPointValue; }
            set { SetProperty(ref m_TeachingPointValue, value); }
        }

        private double m_SoftLimit;
        public double SoftLimit
        {
            get { return m_SoftLimit; }
            set { SetProperty(ref m_SoftLimit, value); }
        }

        private string m_SoftLimitToolTip;
        public string SoftLimitToolTip
        {
            get { return m_SoftLimitToolTip; }
            set { SetProperty(ref m_SoftLimitToolTip, value); }
        }

        private bool m_IsAllowMoveMtr;
        public bool IsAllowMoveMtr
        {
            get { return m_IsAllowMoveMtr; }
            set { SetProperty(ref m_IsAllowMoveMtr, value); }
        }
        public Visibility btnGoVisible { get; set; }
        public bool IsReadOnly { get; set; }
    }

    public class Velocity : BindableBase
    {
        public int ID { get; set; }
        public string ProfileName { get; set; }

        public int RowNum { get; set; }
        public int ColNum { get; set; }

        private string m_ProfileNameWithCulture;
        public string ProfileNameWithCulture
        {
            get { return m_ProfileNameWithCulture; }
            set { SetProperty(ref m_ProfileNameWithCulture, value); }
        }

        private string m_VelUoM;
        public string VelUoM
        {
            get { return m_VelUoM; }
            set { SetProperty(ref m_VelUoM, value); }
        }

        private string m_AccUoM;
        public string AccUoM
        {
            get { return m_AccUoM; }
            set { SetProperty(ref m_AccUoM, value); }
        }

        private string m_MaxDriveVel;
        public string MaxDriveVel
        {
            get { return m_MaxDriveVel; }
            set { SetProperty(ref m_MaxDriveVel, value); }
        }

        private string m_MaxAccVel;
        public string MaxAccVel
        {
            get { return m_MaxAccVel; }
            set { SetProperty(ref m_MaxAccVel, value); }
        }

        private string m_MaxDccVel;
        public string MaxDccVel
        {
            get { return m_MaxDccVel; }
            set { SetProperty(ref m_MaxDccVel, value); }
        }

        private long m_DriveVel;
        public long DriveVel 
        { 
            get { return m_DriveVel; }
            set { SetProperty(ref m_DriveVel, value); }
        }

        private double m_Acc;
        public double Acc 
        {
            get { return m_Acc; }
            set { SetProperty(ref m_Acc, value); }
        }

        private double m_Dcc;
        public double Dcc 
        {
            get { return m_Dcc; }
            set { SetProperty(ref m_Dcc, value); }
        }

        private long m_MaxVel;
        public long MaxVel 
        {
            get { return m_MaxVel; }
            set { SetProperty(ref m_MaxVel, value); }
        }

        private double m_MaxAcc;
        public double MaxAcc 
        {
            get { return m_MaxAcc; }
            set { SetProperty(ref m_MaxAcc, value); }
        }

        private double m_JerkTime;
        public double JerkTime
        {
            get { return m_JerkTime; }
            set { SetProperty(ref m_JerkTime, value); }
        }

        private double m_KillDcc;
        public double KillDcc
        {
            get { return m_KillDcc; }
            set { SetProperty(ref m_KillDcc, value); }
        }

        private Visibility m_IsACSMotion;
        public Visibility IsACSMotion
        {
            get { return m_IsACSMotion; }
            set { SetProperty(ref m_IsACSMotion, value); }
        }
    }
}
