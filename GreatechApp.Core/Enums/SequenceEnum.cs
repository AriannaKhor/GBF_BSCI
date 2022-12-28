namespace GreatechApp.Core.Enums
{
    public enum SQID
    {
        None = -1,
        // Core Sequence
        CriticalScan = 0,

        // Machine Sequence
        TopVisionSeq = 1,
        CodeReaderSeq = 2,
        SampleSeq3 = 3,
        SampleSeq4 = 4,
        SampleSeq5 = 5,
        BarcodeScanner = 6,
        CountingScaleSeq = 7,
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
        ProcCodeReaderFail,
        ProcVisFail,
        ProcSkip,
        ProcAbort,
        ProcReady,
        ProcCont,
        ProcCodeReaderCont,
        ProcVisCont,
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
        ProcUpdate,
        ProcStop,
    }

    public enum ErrorCode
    {
        MissingResult,
        BatchNotMatch,
        BoxQtyNotMatch,
        ExceedTotalBatchQty,
    }
   
}
