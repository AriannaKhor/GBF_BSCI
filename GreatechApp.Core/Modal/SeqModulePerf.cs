using GreatechApp.Core.Enums;
using System.Collections.ObjectModel;

namespace GreatechApp.Core.Modal
{
    public class SeqModulePerf
    {
        public SeqModulePerf(int id)
        {
            PerfCollection = new ObservableCollection<PerfKeyValuePair>();
            SeqID = (SQID)id;
        }

        public ObservableCollection<PerfKeyValuePair> PerfCollection { get; internal set; }
        public SQID SeqID { get; private set; }
    }
}
