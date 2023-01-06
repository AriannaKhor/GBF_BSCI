using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using System;
using System.Threading;
using System.Windows;

namespace Sequence.CoreSeq
{
    public class CriticalScan : BaseClass
    {
        #region Variable

        private SN m_SeqNum;
        private SN m_PrevSeqNum;

        private enum SN
        {
            // Running Routine
            NONE = -2,
            EOS = -1,
            Begin = 0,

            CheckEStop,
            CheckAirPressure,
            CheckVacuum,
            CriticalAbort,
        }

        public enum ErrorCode
        {
            E_Stop,
            Air_Pressure,
            No_Vacuum,
            SafetyDoor,
        }
        private int m_ErrorDetected { get; set; }

        #endregion

        #region Constructor
        public CriticalScan()
        {
            m_SeqNum = SN.NONE;
            m_ErrorDetected = -1;
        }
        #endregion

        #region Thread
        public override void OnRunSeq(object sender, EventArgs args)
        {
            try
            {
                lock (m_SyncSN)
                {
                    //switch (m_SeqNum)
                    //{
       //                 case SN.Begin:
       //                     // when any critical error get triggered, stop all process and force user to reintialize the machine
       //                     //m_SeqNum = SN.CheckEStop;
       //                     m_SeqNum = SN.EOS;
       //                     break;

       //                 case SN.CheckEStop:
							//// NC Sensor
							//if (ReadBitF(IN.DI0100_E_StopBtn, INVERT, false))
							//{
       //                         // Raise Error 
       //                         m_ErrorDetected = (int)ErrorCode.E_Stop;
							//	// Goto Error Routine
							//	m_SeqNum = SN.CriticalAbort;
							//}
							//else
							//{
							//	m_SeqNum = SN.CheckAirPressure;
							//}
							//break;

       //                 case SN.CheckAirPressure:
       //                     ////if (SysCfgs.Machine.SafetyScan && !ReadBitT(IN.DI0101_AirPressure, false))
       //                     //{
       //                     //    // Raise Error 
       //                     //    m_ErrorDetected = (int)ErrorCode.Air_Pressure;
       //                     //    // Goto Error Routine
       //                     //    m_SeqNum = SN.CriticalAbort;
       //                     //}
       //                     else
       //                     {
       //                         m_SeqNum = SN.CheckVacuum;
       //                     }
       //                     break;

       //                 case SN.CheckVacuum:
       //                     ////if (SysCfgs.Machine.SafetyScan && !ReadBitT(IN.DI0102_Vacuum) && !Global.DryRun)
       //                     //{
       //                     //    // Raise Error 
       //                     //    m_ErrorDetected = (int)ErrorCode.No_Vacuum;
       //                     //    // Goto Error Routine
       //                     //    m_SeqNum = SN.CriticalAbort;
       //                     //}
       //                     else
       //                     {
       //                         //m_SeqNum = SN.CheckEStop;
       //                         m_SeqNum = SN.CheckAirPressure;
       //                     }
       //                     break;

       //                 case SN.CriticalAbort:
       //                     Global.MachineStatus = MachineStateType.CriticalAlarm;

       //                     // Kill all module's sequence
       //                     for (int i = 1; i < Enum.GetValues(typeof(SQID)).Length - 1; i++)
       //                     {
       //                         Publisher.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = (SQID)i, MachineOpr = MachineOperationType.KillSeq });
       //                     }
       //                     Publisher.GetEvent<MachineState>().Publish(MachineStateType.CriticalAlarm);

       //                     if (m_ErrorDetected != -1)
       //                     {
       //                         RaiseError(m_ErrorDetected);
       //                         m_ErrorDetected = -1;
       //                     }

       //                     m_SeqNum = SN.EOS;
       //                     break;
       //             }

                    SeqNum = m_SeqNum.ToString();
                    CheckLiveSeqStart();
                }
            }
            catch (Exception ex)
            {
                string excepMsg = GenerateExceptionMsg(ex).ToString();
                Publisher.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Error, MsgText = $"{this.SeqName} {GetStringTableValue("Error")} : {excepMsg}" });
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"{GetStringTableValue("Sequence")}: {this.SeqName} {GetStringTableValue("EncounterCriticalError")}...\n {GetStringTableValue("Error")}: \n {excepMsg} \n",  GetStringTableValue("Error") , MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

#endregion

        #region Event
        internal override void CheckLiveSeqStart()
        {
            if (Global.SeqStatusScanOn)
            {
                // if tracked seq num is changed, send latest seq num
                if (m_PrevSeqNum != m_SeqNum)
                {
                    Publisher.GetEvent<SeqStatusEvent>().Publish(new SeqStatus(SeqName) { SeqName = SeqName.ToString(), SeqNum = m_SeqNum.ToString() });
                    m_PrevSeqNum = m_SeqNum;
                }
            }
            else
            {
                // Reset m_PrevSeqNum
                // so that the seq num can be immediately record
                // during next record start
                m_PrevSeqNum = SN.NONE;
            }
        }

        public override void MachineOperation(string Operation)
        {
            if (Operation == "Init")
            {
                if(m_SeqNum == SN.EOS)
                {
                    m_SeqNum = SN.Begin;
                }
            }
        }

        public override void SubscribeSeqEvent()
        {
            Publisher.GetEvent<MachineOperation>().Subscribe(SequenceOperation, filter => filter.TargetSeqName == SQID.CriticalScan);
            Publisher.GetEvent<EStopOperation>().Subscribe(OnEStopOperation);
        }

        internal override void SequenceOperation(SequenceEvent sequence)
        {
            switch (sequence.MachineOpr)
            {
                case MachineOperationType.BeginSeq:
                    m_SeqNum = SN.Begin;
                    break;
            }
        }

        internal void OnEStopOperation()
        {
            lock (this)
            {
                // Raise Error 
                m_ErrorDetected = (int)ErrorCode.E_Stop;
                // Goto Error Routine
                m_SeqNum = SN.CriticalAbort;
            }
        }
        #endregion

        #region IO
        internal override void IOMapping()
        {
            AssignIO(IN.DI0100_E_StopBtn);
            //AssignIO(IN.DI0101_AirPressure);
            //AssignIO(IN.DI0102_Vacuum);

            AssignIO(OUT.DO0100_RedTowerLight);
            AssignIO(OUT.DO0101_AmberTowerLight);
            AssignIO(OUT.DO0102_GreenTowerLight);
            AssignIO(OUT.DO0103_Buzzer);
        }
        #endregion
    }
}
