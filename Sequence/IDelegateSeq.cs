using DataManager;
using GreatechApp.Core.Enums;
using System.Collections.Generic;

namespace Sequence
{
    public interface IDelegateSeq
    {
        int CoreSeqNum { get; }
        int MachineSeqNum { get; }
        int TotalSeq { get; }
        bool GetIsAliveStatus(int sqid);
        string GetSeqNum(SQID seqName);
        int GetSlotNum(SQID seqName);
        int GetStationNum(SQID seqName);
        void BypassStation(SQID id, bool state);
        int TotalInput { get; }               // Total units feed into the machine.
        int TotalOutput { get; }              // Total units produced by the machine.
        int TotalFail { get; }                // Total Fail units
        string[] GetPerfNames(SQID seqName);
        int GetProcCycleTime(SQID seqName, int perfID);    // Use individual module's Perf enum. UoM - ms (Last cycle time)
        double GetAvgCycleTime(SQID seqName, int perfID);  // Use individual module's Perf enum. UoM - ms (Average cyle time)
        int GetMinCycleTime(SQID seqName, int perfID);     // Use individual module's Perf enum. UoM - ms (Min cyle time)
        int GetMaxCycleTime(SQID seqName, int perfID);     // Use individual module's Perf enum. UoM - ms (Max cyle time)
        Dictionary<int, string> GetMotCfg(SQID seqName);
        IBaseData GetBaseData(SQID seqName);
    }
}
