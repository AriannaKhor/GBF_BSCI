using GreatechApp.Core.Enums;
using System;

namespace GreatechApp.Core.Events
{
    public class SequenceEvent
    {
        public MachineOperationType MachineOpr { get; set; }
        public SQID SenderSeqName { get; set; }
        public SQID TargetSeqName { get; set; }
        public bool InitSuccess { get; set; }
        public EventArgs EvArgs { get; set; }
        public bool SeqIntLSuccess { get; set; }
        public string FailType { get; set; }

    }
}
