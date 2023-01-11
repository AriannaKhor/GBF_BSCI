using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Sequence.MachineSeq
{
    public class CountingScaleSeq : BaseClass
    {
        #region Enum
        public enum ErrorCode
        {
            WrongOrientation, //0
            VisionOverallResultRejected,//1
            BatchNotMatch,//2
            ContainerNumberExist,//3
            BoxQtyNotMatch,//4
            ExceedTotalBatchQty,//5
            ExceedUpperLimit, //6
        }
        #endregion

        #region Variable
        private SN m_SeqNum;
        private SN[] m_SeqRsm = new SN[Total_RSM];
        private string m_FailType;
        private string m_ContType;
        #endregion

        #region Enum
        public enum SN
        {
            NONE = -2,
            EOS = -1,
            Begin,

            // Runnning Routine
            TriggerVis,
            TriggerCodeReader,
            WaitVisionResult,
            RetryVisionResult,
            WaitCodeReaderResult,
            EndLot,
            //Error Routine
            ErrorRoutine,
            WaitResumeError,
        }
        #endregion

        #region Constructor
        public CountingScaleSeq()
        {
            m_SeqNum = SN.EOS;
        }
        #endregion

        #region Thread
        public override void OnRunSeq(object sender, EventArgs args)
        {
            try
            {
                if (Monitor.TryEnter(m_SyncSN))
                {
                    switch (m_SeqNum)
                    {
                        #region Running Routine
                        case SN.Begin:
                            m_TmrDelay.Time_Out = 0.1f;
                            m_SeqNum = SN.TriggerVis;
                            break;

                        case SN.TriggerVis:
                            if (m_TmrDelay.TimeOut())
                            {
                                Publisher.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.TopVisionSeq, MachineOpr = MachineOperationType.ProcStart });
                                m_SeqNum = SN.WaitVisionResult;
                            }
                            break;

                        case SN.WaitVisionResult:
                            if (m_SeqFlag.ProcVisCont)
                            {
                                m_SeqFlag.ProcVisCont = false;
                                switch (m_ContType)
                                {
                                    case "ReTriggerVis":
                                        m_SeqNum = SN.TriggerVis;
                                        break;

                                    case "TriggerCodeReader":
                                        m_TmrDelay.Time_Out = 0.1f;
                                        m_SeqNum = SN.TriggerCodeReader;
                                        break;
                                }
                            }
                            else if (m_SeqFlag.ProcVisFail)
                            {
                                m_SeqFlag.ProcVisFail = false;

                                switch (m_FailType)
                                {
                                    case "WrongOrientation":
                                        Global.ErrorMsg = ErrorCode.WrongOrientation.ToString();
                                        RaiseError((int)ErrorCode.WrongOrientation);
                                        break;
                                    case "ExceedUpperLimit":
                                        Global.ErrorMsg = ErrorCode.ExceedUpperLimit.ToString();
                                        RaiseError((int)ErrorCode.ExceedUpperLimit);
                                        break;
                                }
                                m_SeqRsm[(int)RSM.Err] = SN.TriggerVis;
                                m_SeqNum = SN.ErrorRoutine;
                            }
                            break;

                        case SN.TriggerCodeReader:
                            if (m_TmrDelay.TimeOut())
                            {
                                Publisher.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CodeReaderSeq, MachineOpr = MachineOperationType.ProcStart });
                                m_SeqNum = SN.WaitCodeReaderResult;
                            }
                            break;

                        case SN.WaitCodeReaderResult:
                            if (m_SeqFlag.ProcCodeReaderFail)
                            {
                                m_SeqFlag.ProcCodeReaderFail = false;

                                switch (m_FailType)
                                {
                                    case "BatchNotMatch":
                                        Global.ErrorMsg = ErrorCode.BatchNotMatch.ToString();
                                        RaiseVerificationError((int)ErrorCode.BatchNotMatch);
                                        m_SeqRsm[(int)RSM.Err] = SN.TriggerCodeReader;
                                        break;

                                    case "BoxQtyNotMatch":
                                        Global.ErrorMsg = ErrorCode.BoxQtyNotMatch.ToString();
                                        RaiseError((int)ErrorCode.BoxQtyNotMatch);
                                        m_SeqRsm[(int)RSM.Err] = SN.TriggerVis;
                                        break;

                                    case "ExceedTotalBatchQty":
                                        Global.ErrorMsg = ErrorCode.ExceedTotalBatchQty.ToString();
                                        RaiseVerificationError((int)ErrorCode.ExceedTotalBatchQty);
                                        m_SeqRsm[(int)RSM.Err] = SN.TriggerCodeReader;
                                        break;
                                }

                                m_SeqNum = SN.ErrorRoutine;
                            }
                            else if (m_SeqFlag.ProcCodeReaderCont)
                            {
                                m_SeqFlag.ProcCodeReaderCont = false;
                                m_SeqNum = SN.Begin;
                            }
                            break;
                        #endregion

                        #region End Lot
                        case SN.EndLot:
                            if (m_SeqFlag.EndLotComp)
                            {
                                m_SeqFlag.EndLotComp = false;
                                m_SeqNum = SN.EOS;
                            }
                            break;
                        #endregion

                        #region Error Routine
                        case SN.ErrorRoutine:
                            m_SeqNum = SN.WaitResumeError;
                            break;

                        case SN.WaitResumeError:
                            if (Global.MachineStatus == MachineStateType.Running)
                            {
                                m_SeqNum = m_SeqRsm[(int)RSM.Err];
                                m_SeqRsm[(int)RSM.Err] = SN.NONE;
                                m_TmrDelay.Time_Out = 0.1f;
                            }
                            break;
                            #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                string excepMsg = GenerateExceptionMsg(ex).ToString();
                Publisher.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Error, MsgText = $"{this.SeqName} {GetStringTableValue("Error")} : {excepMsg}" });
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"{GetStringTableValue("Sequence")}: {this.SeqName} {GetStringTableValue("EncounterCriticalError")}...\n {GetStringTableValue("Error")}: \n {excepMsg} \n", GetStringTableValue("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                });
                m_SeqNum = SN.EOS;
            }
        }
        #endregion

        #region Events
        public override void SubscribeSeqEvent()
        {
            Publisher.GetEvent<MachineOperation>().Subscribe(SequenceOperation, filter => filter.TargetSeqName == SeqName);
        }

        internal override void SequenceOperation(SequenceEvent sequence)
        {
            lock (m_SyncEvent)
            {
                if (sequence.TargetSeqName == SeqName)
                {
                    switch (sequence.MachineOpr)
                    {
                        case MachineOperationType.EndLotComp:
                            m_SeqFlag.EndLotComp = true;
                            break;
                        case MachineOperationType.ProcVisCont:
                            m_SeqFlag.ProcVisCont = true;
                            m_ContType = sequence.ContType;
                            break;
                        case MachineOperationType.ProcCodeReaderCont:
                            m_SeqFlag.ProcCodeReaderCont = true;
                            break;
                        case MachineOperationType.ProcVisFail:
                            m_SeqFlag.ProcVisFail = true;
                            m_FailType = sequence.FailType;
                            break;
                        case MachineOperationType.ProcCodeReaderFail:
                            m_SeqFlag.ProcCodeReaderFail = true;
                            m_FailType = sequence.FailType;
                            break;
                        case MachineOperationType.ProcContErrRtn:
                            m_SeqNum = SN.ErrorRoutine;
                            break;
                    }
                }
                base.SequenceOperation(sequence);
            }
        }

        public override void StartProduction()
        {
            m_SeqNum = SN.Begin;
        }

        public override void OperationChecking(bool checkopr)
        {
            if (checkopr)
            {
                checkOp = true;
            }
        }
        #endregion

        #region IO
        internal override void IOMapping()
        {
            #region Output
            AssignIO(OUT.DO0100_Buzzer);
            #endregion
        }
        #endregion
    }
}
