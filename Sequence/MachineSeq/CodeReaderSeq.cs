using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Variable;
using Sequence.SeqEventArg;
using System;
using System.Threading;
using System.Windows;

namespace Sequence.MachineSeq
{
    public class CodeReaderSeq : BaseClass
    {
        #region Enum
        public enum ErrorCode
        {

            MissingResult,//1
            BatchNotMatch,//2
            BoxQtyNotMatch,//3
            ExceedTotalBatchQty,//4
        }
        #endregion

        #region Variable
        private SN m_SeqNum;
        private SN m_PrevSeqNum;
        private SN[] m_SeqRsm = new SN[Total_RSM];
        private int m_CodeReaderLoopCount = 0;
        private string m_FailType;
        #endregion

        #region Enum
        public enum SN
        {
            NONE = -2,
            EOS = -1,
            BeginCodeReader,

            // Runnning Routine

            TriggerVis,
            TriggerCodeReader,
            WaitCodeReaderResult,
            RetryGetCodeReaderResult,

            // Intermediate Recovery
            IM_MoveMotorZHome,
            IM_WaitMtrZHome,

            //Initialization Routine
            IBegin,
            IMoveLifterRest,
            IWaitLifterRest,
            IMoveMotorXHome,
            IWaitMotorXHome,
            IMoveMotorYHome,
            IWaitMotorYHome,
            ISuccess,
            IEnd,

            //Error Routine
            ErrorRoutine,
            WaitResumeError,

            // Stop Routine
            StopRoutine,
            WaitResumeStop,


            ForceEOS,
            CodeReaderRepeat,
            EndLot,
            UpdateLog,
            DelayTrigger,
        }


        #endregion
        #region Constructor
        public CodeReaderSeq()
        {
            m_SeqNum = SN.EOS;
        }
        #endregion


        #region Thread
        private Sample2EventArg m_EventArg = new Sample2EventArg();

        public override void OnRunSeq(object sender, EventArgs args)
        {
            try
            {
                if (Monitor.TryEnter(m_SyncSN))
                {
                    switch (m_SeqNum)
                    {
                        #region Running Routine
                        case SN.BeginCodeReader:
                            m_SeqNum = SN.TriggerCodeReader;
                            break;

                        case SN.TriggerCodeReader:
                            m_TmrDelay.Time_Out = 0.01f;
                            m_SeqNum = SN.DelayTrigger;
                            break;

                        case SN.DelayTrigger:
                            if (m_TmrDelay.TimeOut())
                            {
                                CodeReader.TriggerCodeReader();
                                m_SeqNum = SN.WaitCodeReaderResult;
                            }
                            break;

                        case SN.WaitCodeReaderResult:
                            if (m_SeqFlag.ProcCont)
                            {
                                m_SeqFlag.ProcCont = false;
                                //Global.CodeReaderRetry = false;
                                m_SeqNum = SN.TriggerCodeReader;
                            }
                            else if (m_SeqFlag.ProcFail)
                            {
                                m_SeqFlag.ProcFail = false;
                    
                                switch (m_FailType)
                                {
                                    case "MissingResult":
                                        RaiseError((int)ErrorCode.MissingResult);
                                        break;

                                    case "BatchNotMatch":
                                        RaiseVerificationError((int)ErrorCode.BatchNotMatch);
                                        break;

                                    case "BoxQtyNotMatch":
                                        RaiseError((int)ErrorCode.BoxQtyNotMatch);
                                        break;

                                    case "ExceedTotalBatchQty":
                                        RaiseVerificationError((int)ErrorCode.ExceedTotalBatchQty);
                                        break;
                                }
                                m_SeqRsm[(int)RSM.Err] = SN.TriggerCodeReader;
                                m_SeqNum = SN.ErrorRoutine;
                            }
                            break;

                     

                        #region Error Routine
                        case SN.ErrorRoutine:
                            m_SeqNum = SN.WaitResumeError;
                            break;
                        #endregion

                        case SN.WaitResumeError:
                            if (Global.MachineStatus == MachineStateType.Running)
                            {
                                m_SeqNum = m_SeqRsm[(int)RSM.Err];
                                m_SeqRsm[(int)RSM.Err] = SN.NONE;
                                m_TmrDelay.Time_Out = 0.01f;

                            }
                            break;

                        #region EndLot
                        case SN.EndLot:
                            m_SeqNum = SN.EOS;
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

                        case MachineOperationType.ProcStart:
                            m_SeqFlag.ProcStart = true;
                            m_SeqNum = SN.BeginCodeReader;
                            break;

                        case MachineOperationType.ProcCont:
                            m_SeqFlag.ProcCont = true;
                            break;

                        case MachineOperationType.ProcFail:
                            m_SeqFlag.ProcFail = true;
                            m_FailType = sequence.FailType;
                            break;

                        case MachineOperationType.EndLotComp:
                            m_SeqNum = SN.EndLot;
                            break;

                        case MachineOperationType.ProcUpdate:
                            m_SeqFlag.ProcUpdate = true;
                            break;
                    }
                }

                base.SequenceOperation(sequence);
            }
        }

        #region IO
        internal override void IOMapping()
        {
            #region Input
            AssignIO(IN.DI0100_E_StopBtn);
            #endregion

            #region Output
            AssignIO(OUT.DO0100_RedTowerLight);
            AssignIO(OUT.DO0101_AmberTowerLight);
            AssignIO(OUT.DO0102_GreenTowerLight);
            AssignIO(OUT.DO0103_Buzzer);
            #endregion

        }
        #endregion
    }
    #endregion


}

#endregion