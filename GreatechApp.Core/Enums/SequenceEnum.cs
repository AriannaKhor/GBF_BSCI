namespace GreatechApp.Core.Enums
{
    public enum SQID
    {
        None = -1,

        // Machine Sequence
        TopVisionSeq = 0,
        CodeReaderSeq = 1,
        CountingScaleSeq = 2,
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
        ProcContErrRtn,
        ProcStop,
    }

    public enum ErrorCode
    {
        BatchNotMatch,
        BoxQtyNotMatch,
        ExceedTotalBatchQty,
    }
   
}
