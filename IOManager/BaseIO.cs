using GreatechApp.Core.Enums;
using GreatechApp.Core.Modal;
using System.Collections.Generic;
using System.Text;

namespace IOManager
{
    public class BaseIO : IBaseIO
    {
        private StringBuilder m_ErrMsg = new StringBuilder();
        public string ErrorMsg
        {
            set
            {
                m_ErrMsg.Remove(0, m_ErrMsg.Length);
                m_ErrMsg.Append(value);
            }
            get { return m_ErrMsg.ToString(); }
        }

        private int m_MaxPortNum;
        public int MaxPortNum
        {
            get { return m_MaxPortNum; }
            set { m_MaxPortNum = value; }
        }

        private int m_MaxBitPerDevice;
        public int MaxBitPerDevice
        {
            get { return m_MaxBitPerDevice; }
            set { m_MaxBitPerDevice = value; }
        }

        private Dictionary<SQID, List<object>> m_InputMapList = new Dictionary<SQID, List<object>>();
        public Dictionary<SQID, List<object>> InputMapList
        {
            get { return m_InputMapList; }
            set { m_InputMapList = value; }
        }

        private Dictionary<SQID, List<object>> m_OutputMapList = new Dictionary<SQID, List<object>>();
        public Dictionary<SQID, List<object>> OutputMapList
        {
            get { return m_OutputMapList; }
            set { m_OutputMapList = value; }
        }

        private Dictionary<OUT, SQID> m_IOKeyList = new Dictionary<OUT, SQID>();
        public Dictionary<OUT, SQID> IOKeyList
        {
            get { return m_IOKeyList; }
            set { m_IOKeyList = value; }
        }
        public void AssignOutput(SQID seqName, OUT masterOutput)
        {
            if (!OutputMapList.ContainsKey(seqName))
                OutputMapList.Add(seqName, new List<object>() { { masterOutput } });
            else
                OutputMapList[seqName].Add(masterOutput);

            if (!IOKeyList.ContainsKey(masterOutput))
                IOKeyList.Add(masterOutput, seqName);
        }

        public virtual bool CloseDevice()
        {
            return true;
        }

        public void WorkCylinder(int work, int rest)
        {
            WriteBit(work, true);
            WriteBit(rest, false);
        }

        public virtual bool OpenDevice()
        {
            return false;
        }

        public virtual bool ReadBit(int bit)
        {
            return false;
        }

        public virtual bool ReadBit(int bit, bool invert)
        {
            return invert && !ReadBit(bit);
        }

        public virtual bool ReadOutBit(int bit)
        {
            return false;
        }

        public virtual void RestCylinder(int rest, int work)
        {
            WriteBit((ushort)work, false);
            WriteBit((ushort)rest, true);
        }

        public virtual void StartScanIO()
        {
            
        }

        public virtual bool WriteBit(int? bit, bool state)
        {
            return false;
        }
    }
}
