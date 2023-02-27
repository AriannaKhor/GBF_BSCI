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
        string[] GetPerfNames(SQID seqName);
        int GetProcCycleTime(SQID seqName, int perfID);    // Use individual module's Perf enum. UoM - ms (Last cycle time)
        double GetAvgCycleTime(SQID seqName, int perfID);  // Use individual module's Perf enum. UoM - ms (Average cyle time)
        int GetMinCycleTime(SQID seqName, int perfID);     // Use individual module's Perf enum. UoM - ms (Min cyle time)
        int GetMaxCycleTime(SQID seqName, int perfID);     // Use individual module's Perf enum. UoM - ms (Max cyle time)
        Dictionary<int, string> GetMotCfg(SQID seqName);
    }
}
