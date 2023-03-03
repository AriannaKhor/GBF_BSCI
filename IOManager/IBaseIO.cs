using GreatechApp.Core.Enums;
using GreatechApp.Core.Modal;
using System.Collections.Generic;

namespace IOManager
{
    public interface IBaseIO
    {
        string ErrorMsg { get; set; }
        int MaxBitPerDevice { get; set; }

        //List<VacuumCylinderIO> VacuumCylinderList { get; set; }
        Dictionary<SQID, List<object>> InputMapList { get; set; }
        Dictionary<SQID, List<object>> OutputMapList { get; set; }
        Dictionary<OUT, SQID> IOKeyList { get; set; }

        void AssignInput(SQID seqname, IN masterIO);
        void AssignOutput(SQID seqname, OUT masterIO);
        void StartScanIO();
        bool OpenDevice();
        bool CloseDevice();
        bool ReadBit(int bit);
        bool ReadBit(int bit, bool invert);
        bool ReadOutBit(int bit);
        bool WriteBit(int? bit, bool state);
        void WorkCylinder(int work, int rest);
        void RestCylinder(int rest, int work);
    }
}
