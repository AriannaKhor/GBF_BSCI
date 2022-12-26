using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
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
                            m_SeqNum = SN.TriggerDevices;
                            break;
                        case SN.TriggerDevices:
                            Publisher.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.TopVisionSeq, MachineOpr = MachineOperationType.ProcStart });
                            Publisher.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CodeReaderSeq, MachineOpr = MachineOperationType.ProcStart });
                            m_SeqNum = SN.EndLot;
                         break;
                        #endregion
                        case SN.EndLot:
                            if (m_SeqFlag.EndLotComp)
                            {
                                m_SeqFlag.EndLotComp = false;
                                Publisher.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.TopVisionSeq, MachineOpr = MachineOperationType.EndLotComp });
                                Publisher.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CodeReaderSeq, MachineOpr = MachineOperationType.EndLotComp });
                                m_SeqNum = SN.EOS;
                            }
                            break;
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
