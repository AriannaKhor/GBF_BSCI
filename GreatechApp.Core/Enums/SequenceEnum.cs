namespace GreatechApp.Core.Enums
{
    public enum SQID
    {
        None = -1,

        // Machine Sequence
        TopVisionSeq,
        //CodeReaderSeq,
        //OhausScaleSeq,
        CountingScaleSeq,
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
        ProcVisFail,
        ProcSkip,
        ProcAbort,
        ProcReady,
        ProcCont,
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
