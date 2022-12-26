using ConfigManager;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Variable;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace Sequence.MachineSeq
{
    public class TopVisionSeq : BaseClass
    {

        #region Enum
        public enum ErrorCode
        {
            WrongOrientation, //0
        }
        #endregion

        #region Variable
        private SN m_SeqNum;
        private SN m_PrevSeqNum;
        private SN[] m_SeqRsm = new SN[Total_RSM];
        private string m_FailType;
       

        // private int m_InsightVisionLoopCount = 0;
        #endregion

        #region Enum
        public enum SN
        {
            NONE = -2,
            EOS = -1,
            BeginVision,


            // Runnning Routine
            TriggerVis,
            DelayTrigger,
            TriggerCodeReader,
            WaitVisionResult,
            RetryGetVisionResult,

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
            BeginTopVision,
            TopVisionRepeat,
            EndLot,
            UpdateLog,
        }

        #endregion

        #region Constructor
        public TopVisionSeq()
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
                        case SN.BeginTopVision:
                            m_SeqNum = SN.TriggerVis;
                            break;

                        case SN.TriggerVis:
                            m_TmrDelay.Time_Out = 0.01f;
                            m_SeqNum = SN.DelayTrigger;
                            break;

                        case SN.DelayTrigger:
                            if (m_TmrDelay.TimeOut())
                            {
                                InsightVision.TriggerVisCapture();
                                m_SeqNum = SN.WaitVisionResult;
                            }
                            break;

                        case SN.WaitVisionResult:
                            m_resultsDatalog.ClearAll();

                            if (m_SeqFlag.ProcCont)
                            {
                                Global.VisErrorCaused = "N/A";
                                m_SeqFlag.ProcCont = false;
                                m_SeqNum = SN.TriggerVis;
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
                            
                            m_resultsDatalog.UserId = Global.UserId;
                            m_resultsDatalog.UserLvl = Global.UserLvl;
                            DateTime currentTime = DateTime.Now;
                            DateTimeFormatInfo dateFormat = new DateTimeFormatInfo();
                            dateFormat.ShortDatePattern = "dd-MM-yyyy";
                            m_resultsDatalog.Date = currentTime.ToString("d", dateFormat);
                            m_resultsDatalog.Time = currentTime.ToString("HH:mm:ss.fff", DateTimeFormatInfo.InvariantInfo);
                            m_resultsDatalog.Timestamp = m_resultsDatalog.Date + " | " + m_resultsDatalog.Time;
                            m_resultsDatalog.TopVision = inspectiontype.TopVision.ToString();
                            m_resultsDatalog.VisTotalPrdQty = Global.VisProductQuantity;
                            m_resultsDatalog.VisCorrectOrient = Global.VisProductCrtOrientation;
                            m_resultsDatalog.VisWrongOrient = Global.VisProductWrgOrientation;
                            m_resultsDatalog.VisErrorMessage = Global.VisErrorCaused;
                            Publisher.GetEvent<Resultlog>().Publish(m_resultsDatalog);
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
                                m_TmrDelay.Time_Out = 0.01f;
                                VisResume = true;

                            }

                            break;
                        #endregion

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
                            m_SeqNum = SN.BeginTopVision;
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
                    }
                }



                base.SequenceOperation(sequence);
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