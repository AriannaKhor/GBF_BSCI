using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Variable;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace Sequence.MachineSeq
{
    public class OhausScaleSeq : BaseClass
    {
        #region Enum
        public enum ErrorCode
        {
            BatchNotMatch,//1
            BoxQtyNotMatch,//2
            ExceedTotalBatchQty,//3
        }
        #endregion

        #region Variable
        private SN m_SeqNum;
        private SN[] m_SeqRsm = new SN[Total_RSM];
        private string m_FailType;
        #endregion

        #region Enum
        public enum SN
        {
            NONE = -2,
            EOS = -1,
            BeginCodeReader,

            // Runnning Routine
            TriggerWeighingScale,
            TriggerWeighingScaleSendCurrentWeight,
            WaitTriggerWeighingScaleSendCurrentWeight,
            TriggerWeighingScaleSendCurrentQty,
            WaitTriggerWeighingScaleSendCurrentQty,
        }
        #endregion

        #region Constructor
        public OhausScaleSeq()
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
                        case SN.TriggerWeighingScaleSendCurrentWeight:
                            SerialPort.Send("S");
                            m_TmrDelay.Time_Out = 0.1f;
                            break;
                        case SN.TriggerWeighingScaleSendCurrentQty:
                            if (m_TmrDelay.TimeOut())
                            {
                                SerialPort.Send("IP");
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

                        case MachineOperationType.ProcStart:
                            m_SeqNum = SN.TriggerWeighingScaleSendCurrentWeight;
                            break;
                    }
                }

                base.SequenceOperation(sequence);
            }
        }
        #region IO
        internal override void IOMapping()
        {
            #region Output
            AssignIO(OUT.DO0100_Buzzer);
            #endregion
        }
        #endregion
    }
    #endregion
}
