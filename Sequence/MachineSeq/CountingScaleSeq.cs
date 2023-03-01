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
        #region Other Enum
        public enum ErrorCode
        {
            WrongOrientation, //0
            BatchNotMatch,//1
            BoxQtyNotMatch,//2
            ExceedTotalBatchQty,//3
            ExceedUpperLimit, //4
        }
        #endregion

        #region Variable
        private SN m_SeqNum;
        private SN[] m_SeqRsm = new SN[Total_RSM];
        private string m_FailType;
        private string m_ContType;
        #endregion

        #region Seq Case Enum
        public enum SN
        {
            NONE = -2,
            EOS = -1,
            Begin,

            // Runnning Routine
            WaitForCurtainSensorSignalBreak,
            WaitForCurtainSensorSignalSafe,
            TriggerVis,
            DetectInitialSlipSheet,
            DetectForReversePouch,
            DetectColorPouch,
            DetectInvertColorPouch,
            DetectDFU,
            DetectFinalSlipSheet,
            GetWeightFromScale,
            CompareVisResultandScaleResult,
            EndLot,
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

                        #region Begin
                        case SN.Begin:
                            m_SeqNum = SN.WaitForCurtainSensorSignalBreak;
                            break;
                        #endregion

                        #region Curtain Sensor
                        case SN.WaitForCurtainSensorSignalBreak:
#if SIMULATION
                            inputsignal = 0;
#endif
                            if (inputsignal == (int)IN.DI0100_CurtainSensor)
                            {
                                m_SeqNum = SN.WaitForCurtainSensorSignalSafe;
                            }
                            break;

                        case SN.WaitForCurtainSensorSignalSafe:
#if SIMULATION
                            inputsignal = 1;
#endif
                            if (inputsignal == (int)IN.DI0100_CurtainSensor + 1)
                            {
                                m_TmrDelay.Time_Out = 0.1f;
                                m_SeqNum = SN.TriggerVis;
                            }
                            break;

                        #endregion

                        #region Vision

                        #region Trigger Vision
                        case SN.TriggerVis:
                            if (m_TmrDelay.TimeOut())
                            {
                                if (OverallQtyCount == 0)
                                {
                                    Publisher.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.TopVisionSeq, MachineOpr = MachineOperationType.ProcStart });
                                    m_SeqNum = SN.DetectInitialSlipSheet;
                                }
                                if (OverallPouchesCount != RcpQty && OverallQtyCount != 0)
                                {
                                    Publisher.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.TopVisionSeq, MachineOpr = MachineOperationType.ProcStart });
                                    if (!resumeError)
                                    {
                                        m_SeqNum = m_SeqRsm[(int)RSM.Src];
                                    }
                                    else
                                    {
                                        resumeError = false;
                                        m_SeqNum = m_SeqRsm[(int)RSM.Err];
                                    }
                                }
                                else
                                {
                                    m_SeqNum = SN.DetectDFU;
                                }
                            }
                            break;
                        #endregion

                        #region Process
                        case SN.DetectInitialSlipSheet:
#if SIMULATION
                            Global.VisSlipSheet = 1;
#endif
                            if (Global.VisSlipSheet == resultseqtyp.SlipSheet.ToString())
                            {
                                ResetGlobalResult();
                                OverallQtyCount++;
                                SlipSheetCount++;
                                m_SeqRsm[(int)RSM.Src] = SN.DetectForReversePouch;
                            }
                            else
                            {
                                
                            }
                            m_SeqNum = SN.WaitForCurtainSensorSignalBreak;
                            break;
                        case SN.DetectForReversePouch:
#if SIMULATION
                            Global.VisReversePouch = 2;
#endif
                            if (Global.VisReversePouch == resultseqtyp.ReversePouch.ToString())
                            {
                                ResetGlobalResult();
                                OverallQtyCount++;
                                m_SeqRsm[(int)RSM.Src] = SN.DetectColorPouch;
                            }
                            else
                            {
                                resumeError = true;
                                m_SeqRsm[(int)RSM.Err] = SN.DetectForReversePouch;
                            }
                            m_SeqNum = SN.WaitForCurtainSensorSignalBreak;
                            break;

                        case SN.DetectColorPouch:
#if SIMULATION
                            Global.VisColorPouch = 3;
#endif
                            if (Global.VisColorPouch == resultseqtyp.ColorPouch.ToString())
                            {
                                ResetGlobalResult();
                                OverallQtyCount++;
                                OverallPouchesCount++;
                                ClrPouchQty++;
                                m_SeqRsm[(int)RSM.Src] = SN.DetectInvertColorPouch;
                            }
                            else
                            {
                                resumeError = true;
                                m_SeqRsm[(int)RSM.Err] = SN.DetectColorPouch;
                            }
                            m_SeqNum = SN.WaitForCurtainSensorSignalBreak;
                            break;

                        case SN.DetectInvertColorPouch:
#if SIMULATION
                            Global.VisInvertColorPouch = 4;
#endif
                            if (Global.VisInvertColorPouch == resultseqtyp.InvertColorPouch.ToString())
                            {
                                ResetGlobalResult();
                                OverallQtyCount++;
                                OverallPouchesCount++;
                                InvertClrPouchQty++;
                                m_SeqRsm[(int)RSM.Src] = SN.DetectColorPouch;
                            }
                            else
                            {
                                resumeError = true;
                                m_SeqRsm[(int)RSM.Err] = SN.DetectInvertColorPouch;
                            }
                            m_SeqNum = SN.WaitForCurtainSensorSignalBreak;
                            break;

                        case SN.DetectDFU:
#if SIMULATION
                            Global.VisDFU = 5;
#endif
                            if (Global.VisDFU == resultseqtyp.DFU.ToString())
                            {
                                ResetGlobalResult();
                                OverallQtyCount++;
                                m_SeqRsm[(int)RSM.Src] = SN.DetectFinalSlipSheet;
                            }
                            else
                            {
                                resumeError = true;
                                m_SeqRsm[(int)RSM.Err] = SN.DetectDFU;
                            }
                            m_SeqNum = SN.WaitForCurtainSensorSignalBreak;
                            break;

                        case SN.DetectFinalSlipSheet:
#if SIMULATION
                            Global.VisSlipSheet = 1;
#endif
                            if (Global.VisSlipSheet == resultseqtyp.SlipSheet.ToString())
                            {
                                ResetGlobalResult();
                                OverallQtyCount++;
                                m_SeqNum = SN.GetWeightFromScale;
                            }
                            else
                            {
                                resumeError = true;
                                m_SeqRsm[(int)RSM.Err] = SN.DetectFinalSlipSheet;
                                m_SeqNum = SN.WaitForCurtainSensorSignalBreak;
                            }
                            break;
                        #endregion
                        #endregion

                        #region Weighing Scale
                        case SN.GetWeightFromScale:
                            m_TmrDelay.Time_Out = 0.1f;
                           // Publisher.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.OhausScaleSeq, MachineOpr = MachineOperationType.ProcStart });
                            m_SeqNum = SN.CompareVisResultandScaleResult;
                            break;

                        case SN.CompareVisResultandScaleResult:
                            //if ((Actual Scale Pouch Qty == OverallPouchesCount) && (Actual Scale Weightage >= min weight && Actual Scale Weightage <=min weight)
                            //{
                            //   normal end lot & log
                            //  m_SeqNum = SN.EndLot;
                            //}
                            //else
                            //{
                            //  retry++;
                            //  if(retry>3)
                            //  {
                            //      //End Lot Verification & Log 
                            //      m_SeqNum = SN.EndLot;
                            //  }
                            //  else
                            //  {
                            //       m_SeqNum = SN.GetWeightFromScale;
                            //  }
                            //}
                            break;
                        #endregion
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
                        //case MachineOperationType.ProcCodeReaderCont:
                        //    m_SeqFlag.ProcCodeReaderCont = true;
                        //    break;
                        case MachineOperationType.ProcVisFail:
                            m_SeqFlag.ProcVisFail = true;
                            m_FailType = sequence.FailType;
                            break;
                        //case MachineOperationType.ProcCodeReaderFail:
                        //    m_SeqFlag.ProcCodeReaderFail = true;
                        //    m_FailType = sequence.FailType;
                        //    break;
                        //case MachineOperationType.ProcContErrRtn:
                        //    m_SeqNum = SN.ErrorRoutine;
                        //    break;
                        case MachineOperationType.ProcCont:
                            m_SeqNum = SN.Begin;
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
