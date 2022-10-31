namespace GreatechApp.Core.Enums
{
    public enum SQID
    {
        None = -1,
        // Core Sequence
        CriticalScan = 0,

        // Machine Sequence
        SampleSeq = 1,
        SampleSeq2 = 2,
        SampleSeq3 = 3,
        SampleSeq4 = 4,
        SampleSeq5 = 5,
        BarcodeScanner = 6,
    }

    public enum MachineOperationType
    {
        None,
        StartLot,
        BeginSeq,
        BeginInit,
        InitDone,
        InitFail,
        ProcStart,
        ProcSkip,
        ProcAbort,
        ProcReady,
        ProcCont,
        ProcBusy,
        ProcComp,
        ItemGiven,
        ItemTaken,
        ReqNewItem,
        NewItemAvail,
        SeqIntLChk,
        SeqIntLComp,
        BeginEndLot,
        EndLotComp,
        KillSeq,
        ExtTestRunBegin,
        ExtTestRunAbort,
        ExtTestRunComp,
    }
}
