using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Sequence.MachineSeq
{
    public class CountingScaleSeq : BaseClass
    {
        #region Variable
        private SN m_SeqNum;
        private SN m_PrevSeqNum;
        private SN[] m_SeqRsm = new SN[Total_RSM];
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
            RetryGetVisionResult,
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
                            m_SeqNum = SN.TriggerVis;
                            break;
                        case SN.TriggerVis:
                            //Call Vision Class Method 
                           
                            break;
                        case SN.WaitVisionResult:
                            if (m_SeqFlag.ProcComp) //gotten from VisInspectResultPass published event
                            {
                                m_SeqFlag.ProcComp = false;
                                m_SeqNum = SN.TriggerCodeReader;
                            }
                            else if (m_SeqFlag.ProcFail) //Gotten from CodeReaderSeqFail published event
                            {
                                m_TmrDelay.Time_Out = 5f;
                                m_SeqNum = SN.RetryGetVisionResult;
                            }
                            break;
                        case SN.RetryGetVisionResult:
                            if (m_TmrDelay.TimeOut())
                            {
                                m_SeqNum = SN.TriggerVis;
                            }
                            break;
                        case SN.TriggerCodeReader:
                           
                            break;
                        case SN.WaitCodeReaderResult:
                            if (m_SeqFlag.ProcComp) //gotten from VisInspectResultPass published event
                            {
                                m_SeqFlag.ProcComp = false;
                                m_SeqNum = SN.TriggerVis;
                            }
                            else if (m_SeqFlag.ProcFail || m_TmrDelay.TimeOut())
                            {
                                m_TmrDelay.Time_Out = 5f;
                                m_SeqNum = SN.RetryGetCodeReaderResult;
                            }
                            break;
                        case SN.RetryGetCodeReaderResult:
                            if (m_TmrDelay.TimeOut())
                            {
                                m_SeqNum = SN.TriggerCodeReader;
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
                m_SeqRsm[(int)RSM.Stop] = m_SeqNum;
                m_SeqNum = SN.StopRoutine;
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

                            break;

                        case MachineOperationType.ProcComp:
                            m_SeqFlag.ProcComp = true;
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
           m_SeqNum = SN.Begin;
        }
        #endregion
    }
}
