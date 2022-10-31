using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreatechApp.Core.Modal
{
    public class SysInfo
    {
        public SysInfo()
        {
            InitData();
        }

        public void InitData()
        {
            // Project: Add project information here.
            UPH = "-";
            CycleTime = "-";
            TotalPass = "-";
            TotalFail = "-";
            // Allow child view like EquipmentViewModel and AlarmViewModel to access it.
            ErrList = new List<tagErrLookUp>();
            StopList = new List<object>();
        }

        public string UPH { get; set; }
        public string CycleTime { get; set; }

        public string TotalPass { get; set; }
        public string TotalFail { get; set; }

        public List<tagErrLookUp> ErrList { set; get; }
        public List<object> StopList { set; get; }
    }
    public struct tagErrLookUp
    {
        internal int seqID;
        internal int errCode;
        internal string errEnum;
    }
}
