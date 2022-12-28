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

            MissingResult,//2
            BatchNotMatch,//3
            ContainerNumberExist,//4
            BoxQtyNotMatch,//5
            ExceedTotalBatchQty,//6
            

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
            Begin,

            // Runnning Routine
            TriggerVis,
            TriggerCodeReader,
            WaitVisionResult,
            RetryVisionResult,
            WaitCodeReaderResult,
            RetryGetCodeReaderResult,

            // Intermediate Recovery
            IM_MoveMotorXHome,
            IM_WaitMtrXHome,

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
            UpdateLog,
            EndLot,
            TriggerDevices,
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
                            m_resultsDatalog.ClearAll();
                            if (m_SeqFlag.ProcCont)
                            {
                                Global.VisErrorCaused = "N/A";
                                m_SeqFlag.ProcCont = false;
                                m_SeqNum = SN.TriggerCodeReader;
                            }
                            else if (m_SeqFlag.ProcFail)
                            {
                                m_SeqFlag.ProcFail = false;

                                switch (m_FailType)
                                {
                                    case "WrongOrientation":
                                        Global.VisErrorCaused = RaiseError((int)ErrorCode.WrongOrientation);
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
                            if (m_SeqFlag.ProcCont)
                            {
                                m_SeqFlag.ProcCont = false;
                                m_SeqNum = SN.TriggerCodeReader;
                            }
                            else if (m_SeqFlag.ProcFail)
                            {
                                m_SeqFlag.ProcFail = false;

                                switch (m_FailType)
                                {
                                    case "MissingResult":
                                        Global.CodeReaderErrorCaused = RaiseError((int)ErrorCode.MissingResult);
                                        break;

                                    case "BatchNotMatch":
                                        Global.CodeReaderErrorCaused = RaiseVerificationError((int)ErrorCode.BatchNotMatch);
                                        break;

                                    case "BoxQtyNotMatch":
                                        Global.CodeReaderErrorCaused = RaiseError((int)ErrorCode.BoxQtyNotMatch);
                                        break;

                                    case "ExceedTotalBatchQty":
                                        Global.CodeReaderErrorCaused = RaiseVerificationError((int)ErrorCode.ExceedTotalBatchQty);
                                        break;
                                }
                                m_SeqRsm[(int)RSM.Err] = SN.TriggerCodeReader;
                                m_SeqNum = SN.ErrorRoutine;
                            }
                            DateTime currentTime = DateTime.Now;
                            DateTimeFormatInfo dateFormat = new DateTimeFormatInfo();
                            dateFormat.ShortDatePattern = "dd-MM-yyyy";
                            m_resultsDatalog.Date = currentTime.ToString("d", dateFormat);
                            m_resultsDatalog.Time = currentTime.ToString("HH:mm:ss.fff", DateTimeFormatInfo.InvariantInfo);
                            m_resultsDatalog.Timestamp = m_resultsDatalog.Date + " | " + m_resultsDatalog.Time;
                            WriteSoftwareResultLog(m_resultsDatalog);
                            m_resultsDatalog.ClearAll();
                            break;
                        #endregion

                        case SN.EndLot:
                            if (m_SeqFlag.EndLotComp)
                            {
                                m_SeqFlag.EndLotComp = false;
                                m_SeqNum = SN.EOS;
                            }
                            break;

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
                        case MachineOperationType.ProcStart:
                            m_SeqNum = SN.Begin;
                            break;
                        case MachineOperationType.ProcCont:
                            m_SeqFlag.ProcCont = true;
                            break;
                        case MachineOperationType.ProcFail:
                            m_SeqFlag.ProcFail = true;
                            break;
                    }
                }
                base.SequenceOperation(sequence);
            }
        }

        public override void StartProduction()
        {
            if (!checkOp)
            {
                m_SeqNum = SN.Begin;
            }
             
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
}
