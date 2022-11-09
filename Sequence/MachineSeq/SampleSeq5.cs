#define MOTION
#define Servo_FE

using ConfigManager;
using ConfigManager.Constant;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using GreatechApp.Services.Utilities;
using Sequence.Framework;
using Sequence.SeqEventArg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;

namespace Sequence.MachineSeq
{
    public class SampleSeq5 : BaseClass
    {
        #region Variable
        private SN m_SeqNum;
        private SN m_PrevSeqNum;
        private SN[] m_SeqRsm = new SN[Total_RSM];
        private int m_Count;

        private enum SN
        {
            NONE = -2,
            EOS = -1,
            Begin,

            // Runnning Routine
            PickandLoadData,
            RotateMotor,
            EvalTester2Result,
            TransferData,
            LoopBack,

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

            // Interlock Check
            IL_ShuttleSafeToMove,
            IL_WaitSeqIntLChk,
            IL_EvalIntLResult,

            ForceEOS,

        }

#if MOTION
        private enum SN_HM
        {
            EOS = -1,
            ResetMtrAlarm = 0,
            WaitAlarmReset,
            MoveMtrHome,
            WaitMtrHome,
            EndOfHomeProcess,
            MoveMtrFI,
            WaitMtrFI,
            WaitLimitStopSettle,
            EvalLmtOffsetCnt,
            MoveMtrLmtOffset,
            WaitMtrLmtOffset,
            MoveMtrHome2,
            WaitMtrHome2,
            MoveMtrGoLoad,
            WaitMtrGoLoad,
        }
        private SN_HM[] m_SeqHM = null;

#endif
        private struct LocalFlag
        {
            internal bool IsCodeReaderConnected;
        }
        private LocalFlag m_LocalFlag = new LocalFlag();

        private struct LocalVar
        {
            internal int EOLProcCompCnt;
            internal int TCPReconnectCnt;
            internal List<int> SeqIntLFailIDs;
            internal int SeqIntLCompCnt;
        }
        private LocalVar m_LocalVar = new LocalVar();

        #region Delay Timer, Error Timer, Counter, Option, Perf, Choice, and Position Profile
        public enum DLY_T
        {
#if MOTION
            SetZeroPos,
#endif
            Gripper,
        }

        public enum Counter
        {
            MaxTCPPReconnect,
            SeqIntLSync,
        }

        public enum Option
        {
            ReconnectCodeReader,
        }

        public enum Choice
        {
            Yes,
            No,
        }

        public enum ERR_T
        {
            // Error Timer array idx
#if MOTION
            LongDist = 0,	// used by motion
            ShortDist = 1,		// used by motion
#endif
            Gripper,
            LifterUp,
        }

        public enum Perf
        {
            Test,
            GripperWork,
            LifterUp,
        }

        public enum ErrorCode
        {
#if MOTION
            MtrAlarm = 0,       // 0
            MtrNotReady,        // 1
            MtrNotInPos,        // 2
            HitFwdLimit,        // 3
            HitRevLimit,        // 4
            UnknownMtrErr,      // 5
            AlarmNotClr,		// 6
#endif
            test,
            CodeReaderConnectionFail,
        }

#if MOTION
        public enum P_PRF
        {
            LmtOffset,
            Ready,
            Place,
            Pick,
            Load,
        }

#endif
        #endregion

        public enum MotAxis
        {
            AxisX,
            AxisY,
        }
        public enum Slot_Pos
        {
            In = 0,
            Tester2 = 1,
            Out = 2        }
        #endregion

        private ToolLifeConfig m_toolLifeCfg;

        #region Constructor
        public SampleSeq5(MarkerType markerType = MarkerType.None, int slotNum = 0, int stationNum = 0)
        {
            for (int i = 0; i < Total_RSM; i++)
            {
                m_SeqRsm[i] = SN.NONE;
            }

            for (int i = 0; i < Enum.GetNames(typeof(Perf)).Length; i++)
            {
                _PerfInfo.Add(new PerfInfo());
            }

            MarkerType = markerType;
            SlotNum = slotNum;
            StationNum = stationNum;

            m_SeqNum = SN.EOS;
        }
        #endregion


        protected override void InitData()
        {
            base.InitData();
            BaseData.SetDefault();
            m_LocalVar.TCPReconnectCnt = 0;
            m_LocalVar.SeqIntLFailIDs = new List<int>();
            m_LocalVar.SeqIntLCompCnt = 0;

            m_Count = 1;
        }

        #region Thread

        public override void OnRunSeq(object sender, EventArgs args)
        {
            try
            {
//                if (Monitor.TryEnter(m_SyncSN))
//                {
//                    switch (m_SeqNum)
//                    {
//                        #region Running Routine
//                        case SN.Begin:
//                            if (!Global.SeqStop)
//                            {
//                                if (BaseData.ModuleSlots[(int) Slot_Pos.In].IsPresent)
//                                {
//                                    m_SeqNum = SN.EvalTester2Result;
//                                }
//                            }
//                            break;

//                        case SN.EvalTester2Result:
//                            // Inspection
//                            // set the inspection result to station
//#if SIMULATION
//                            if (BaseData.ModuleSlots[(int)Slot_Pos.Tester2].IsPresent)
//                            {
//                                StationResult stationResult = m_Count % 2 == 0 ? StationResult.Pass : StationResult.Fail;
//                                BaseData.SetStationResult((int) Slot_Pos.Tester2, TestStation.Tester2, stationResult);
//                            }
//#endif
//                            Wait(0.5);
//                            m_SeqNum = SN.TransferData;
//                            break;

//                        case SN.TransferData:
//                            if (BaseData.ModuleSlots[(int) Slot_Pos.Out].IsPresent)
//                            {
//                                BaseData.ModuleSlots[(int)Slot_Pos.Out].ResetUnit(); // last part of machine flow, temporary hardcode to remove the unit data
//                                TotalOutput++;
//                                BaseData.SwapMarkerPosition(SQID.SampleSeq4, SQID.SampleSeq5);
//                                //BaseData.TransferData(2, SQID.SampleSeq5, 0);
//                            }
//                            m_SeqNum = SN.RotateMotor;
//                            break;

//                        case SN.RotateMotor:
//                            // Rotate the module
//                            // transfer unit data in memory for every module motion
//                            BaseData.Shift();
//                            Wait(0.5);
//                            m_SeqNum = SN.LoopBack;
//                            break;

//                        case SN.LoopBack:
//                            m_SeqNum = SN.Begin;
//                            break;
//                        #endregion

//                        #region Intermediate Recovery
//                        case SN.IM_MoveMotorXHome:
//                            m_HMStatus[(int)MotAxis.AxisX] = 0;
//                            m_SeqHM[(int)MotAxis.AxisX] = SN_HM.ResetMtrAlarm;
//                            m_SeqNum = SN.IM_WaitMtrXHome;
//                            break;

//                        case SN.IM_WaitMtrXHome:
//                            if (m_HMStatus[(int)MotAxis.AxisX] == HM_SUCCESS)
//                            {
//                                m_SeqNum = m_SeqRsm[(int)RSM.IntM];
//                            }
//                            else if (m_HMStatus[(int)MotAxis.AxisX] == HM_FAIL)
//                            {
//                                Motion.StopServo(m_AxisModel.MotCfgs[(int)MotAxis.AxisX].Axis.CardID, m_AxisModel.MotCfgs[(int)MotAxis.AxisX].Axis.AxisID);
//                                m_SeqRsm[(int)RSM.Err] = SN.IM_MoveMotorXHome;
//                                RaiseError((int)MtrErr((int)MotAxis.AxisX));
//                                m_SeqNum = SN.ErrorRoutine;
//                            }
//                            break;
//                        #endregion

//                        #region Initilization Routine
//                        case SN.IBegin:
//                            InitData();
//                            m_SeqFlag.InitSuccess = false;
//                            //m_SeqNum = SN.IMoveLifterRest;
//                            m_SeqNum = SN.ISuccess;
//                            break;

//                        case SN.IMoveLifterRest:
//                            WriteBit(OUT.DO0109_Test_GripperWork, OUT.DO0110_Test_GripperRest, true);
//                            m_TmrErr.Time_Out = GetErrTimeOut((int)ERR_T.Gripper);
//                            m_SeqNum = SN.IWaitLifterRest;
//                            break;

//                        case SN.IWaitLifterRest:
//                            if (!ReadBitF(IN.DI0109_Test_IsGripperWork) && ReadBitT(IN.DI0110_Test_IsGripperRest))
//                            {
//                                m_SeqNum = SN.ISuccess;
//                            }
//                            else if (m_TmrErr.TimeOut())
//                            {
//                                // Raise Error 
//                                RaiseError((int)ErrorCode.test);
//                                m_SeqNum = SN.IEnd;
//                            }
//                            break;

//                        case SN.IMoveMotorXHome:
//                            m_HMStatus[(int)MotAxis.AxisX] = 0;
//                            m_SeqHM[(int)MotAxis.AxisX] = SN_HM.ResetMtrAlarm;
//                            m_SeqNum = SN.IWaitMotorXHome;
//                            break;

//                        case SN.IWaitMotorXHome:
//                            if (m_HMStatus[(int)MotAxis.AxisX] == HM_SUCCESS)
//                            {
//                                m_SeqNum = SN.IMoveMotorYHome;
//                            }
//                            else if (m_HMStatus[(int)MotAxis.AxisX] == HM_FAIL)
//                            {
//                                Motion.StopServo(m_AxisModel.MotCfgs[(int)MotAxis.AxisX].Axis.CardID, m_AxisModel.MotCfgs[(int)MotAxis.AxisX].Axis.AxisID);
//                                m_SeqHM[(int)MotAxis.AxisX] = SN_HM.EOS;
//                                m_SeqNum = SN.IEnd;
//                            }
//                            break;

//                        case SN.IMoveMotorYHome:
//                            m_HMStatus[(int)MotAxis.AxisX] = 0;
//                            m_SeqHM[(int)MotAxis.AxisX] = SN_HM.ResetMtrAlarm;
//                            m_SeqNum = SN.IWaitMotorYHome;
//                            break;

//                        case SN.IWaitMotorYHome:
//                            if (m_HMStatus[(int)MotAxis.AxisX] == HM_SUCCESS)
//                            {
//                                m_SeqNum = SN.ISuccess;
//                            }
//                            else if (m_HMStatus[(int)MotAxis.AxisX] == HM_FAIL)
//                            {
//                                Motion.StopServo(m_AxisModel.MotCfgs[(int)MotAxis.AxisY].Axis.CardID, m_AxisModel.MotCfgs[(int)MotAxis.AxisY].Axis.AxisID);
//                                m_SeqHM[(int)MotAxis.AxisY] = SN_HM.EOS;
//                                m_SeqNum = SN.IEnd;
//                            }
//                            break;

//                        case SN.ISuccess:
//                            {
//                                m_SeqRsm[(int)RSM.Src] = SN.NONE;
//                                m_SeqRsm[(int)RSM.Err] = SN.NONE;

//                                m_SeqFlag.InitSuccess = true;
//                                m_SeqNum = SN.IEnd;
//                            }
//                            break;

//                        case SN.IEnd:
//                            Publisher.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = this.SeqName, MachineOpr = MachineOperationType.InitDone, InitSuccess = m_SeqFlag.InitSuccess });
//                            m_SeqNum = SN.EOS;
//                            break;

//                        #endregion

//                        #region Stop Routine
//                        case SN.StopRoutine:
//                            if (Global.MachineStatus != MachineStateType.Stopped && Global.MachineStatus != MachineStateType.Error)
//                            {
//                                Global.MachineStatus = MachineStateType.Stopped;
//                                Publisher.GetEvent<MachineState>().Publish(MachineStateType.Stopped);
//                                Global.SeqStop = true;
//                            }
//                            m_SeqNum = SN.WaitResumeStop;
//                            break;

//                        case SN.WaitResumeStop:
//                            if (!Global.SeqStop && Global.MachineStatus == MachineStateType.Running || Global.MachineStatus == MachineStateType.Ending_Lot)
//                            {
//                                m_SeqNum = m_SeqRsm[(int)RSM.Stop];
//                                m_SeqRsm[(int)RSM.Stop] = SN.NONE;
//                            }
//                            break;
//                        #endregion

//                        #region Error Routine
//                        case SN.ErrorRoutine:
//                            m_SeqNum = SN.WaitResumeError;
//                            break;

//                        case SN.WaitResumeError:
//                            if (Global.MachineStatus == MachineStateType.Running)
//                            {
//                                m_SeqNum = Global.SkipRetest.Find(x => x.AlarmModule == SeqName).IsSkipRetest ? m_SeqRsm[(int)RSM.Skip] : m_SeqRsm[(int)RSM.Err];
//                                m_SeqRsm[(int)RSM.Err] = SN.NONE;
//                                m_SeqRsm[(int)RSM.Skip] = SN.NONE;
//                                Global.SkipRetest.RemoveAll(x => x.AlarmModule == SeqName);
//                            }
//                            break;

//                        #endregion

//                        #region Sequence Interlock Check
//                        case SN.IL_ShuttleSafeToMove:
//                            m_LocalVar.SeqIntLFailIDs.Clear();
//                            Publisher.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.SampleSeq2, MachineOpr = MachineOperationType.SeqIntLChk });
//                            m_SeqNum = SN.IL_WaitSeqIntLChk;
//                            break;

//                        case SN.IL_WaitSeqIntLChk:
//                            if (m_LocalVar.SeqIntLCompCnt == GetCounterValue((int)Counter.SeqIntLSync))
//                            {
//                                m_LocalVar.SeqIntLCompCnt = 0;
//                                // 1. SampleSeq2
//                                // All Module complete interlock checking
//                                m_SeqNum = SN.IL_EvalIntLResult;
//                            }
//                            break;

//                        case SN.IL_EvalIntLResult:
//                            if (IsPassSeqIntL())
//                            {
//                                m_SeqNum = m_SeqRsm[(int)RSM.IntL];
//                                m_SeqRsm[(int)RSM.IntL] = SN.NONE;
//                            }
//                            else
//                            {
//                                m_SeqRsm[(int)RSM.Err] = SN.IL_ShuttleSafeToMove;
//                                m_SeqNum = SN.ErrorRoutine;
//                            }
//                            break;
//                        #endregion

//                        case SN.ForceEOS:
//#if MOTION
//                            for (int i = 0; i < m_AxisModel.NumOfAxis; i++)
//                            {
//                                m_SeqHM[i] = SN_HM.EOS;
//                                Motion.StopServo(m_AxisModel.MotCfgs[i].Axis.CardID, m_AxisModel.MotCfgs[i].Axis.AxisID);
//                            }
//#endif
//                            m_SeqNum = SN.EOS;
//                            break;

//                    }

//                    CheckTestRunSN();
//                    SeqNum = m_SeqNum.ToString();
//                    CheckLiveSeqStart();
//#if MOTION
//                    for (int i = 0; i < m_AxisModel.NumOfAxis; i++)
//                    {
//                        PerformHomingSeq(m_AxisModel.MotCfgs[i], i);
//                    }
//#endif
//                    Monitor.Exit(m_SyncSN);
//                }
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

        public override void SubscribeTestRunEvent()
        {
            Publisher.GetEvent<TestRunEvent>().Subscribe(TestRunOperation, filter => filter.SeqName == SeqName);
        }

        public override void SubscribeTCPMessage()
        {
            Publisher.GetEvent<TCPIPMsg>().Subscribe(TCPMsgOperation, filter => filter.TCPDevice == NetworkDev.CodeReader);
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
                            Sample2EventArg arg = sequence.EvArgs as Sample2EventArg;
                            Debug.Assert(arg != null, "Invalid argument type.");
                            int test = arg.pickcnt;
                            int test2 = arg.placecnt;

                            m_SeqFlag.ProcStart = true;
                            // Do extra operation here
                            break;

                        case MachineOperationType.SeqIntLComp:
                            if (!sequence.SeqIntLSuccess)
                            {
                                m_LocalVar.SeqIntLFailIDs.Add((int)sequence.SenderSeqName);
                            }
                            m_LocalVar.SeqIntLCompCnt++;
                            break;

                        case MachineOperationType.KillSeq:
                            m_SeqNum = SN.ForceEOS;
                            SeqNum = m_SeqNum.ToString();
                            CheckLiveSeqStart();
                            break;
                    }
                }

                base.SequenceOperation(sequence);
            }

        }

        internal override void TestRunOperation(TestRunEvent testRunEvent)
        {
            if (testRunEvent.SeqName == SeqName)
            {
                m_TestEventArg = new TestEventArg();
                m_TestEventArg.CycleCnt = testRunEvent.TestRunCycle;
                switch (testRunEvent.TestRunSeq)
                {
                    case TestRunEnum.SN.EndSampleTest:
                        lock (m_SyncSN)
                        {
                            m_SeqNum = SN.EOS;
                            m_TestEventArg.RunMode = TestEventArg.Run_Mode.None;

#if MOTION
                            for (int i = 0; i < m_AxisModel.NumOfAxis; i++)
                            {
                                m_SeqHM[i] = SN_HM.EOS;
                                Motion.StopServo(m_AxisModel.MotCfgs[i].Axis.CardID, m_AxisModel.MotCfgs[i].Axis.AxisID);
                            }
#endif
                        }
                        break;

                    case TestRunEnum.SN.HomeMotor:
                        lock (m_SyncSN)
                        {
                            m_TestEventComp[(int)TestEventArg.Run_Mode.HomeMotor] = false;
                            m_TestEventArg.RunMode = TestEventArg.Run_Mode.HomeMotor;
                            m_TestEventArg.MotorIndex = Array.IndexOf(MotCfgs.Keys.ToArray(), testRunEvent.MtrIdx);
                            m_HMStatus[Array.IndexOf(MotCfgs.Keys.ToArray(), testRunEvent.MtrIdx)] = 0;
#if MOTION
                            m_SeqHM[Array.IndexOf(MotCfgs.Keys.ToArray(), testRunEvent.MtrIdx)] = SN_HM.ResetMtrAlarm;
#endif
                        }
                        break;

                    case TestRunEnum.SN.BeginSampleTest:
                        lock (m_SyncSN)
                        {
                            m_TestEventArg.RunMode = TestEventArg.Run_Mode.TestRun;
                            // Assign The fist SN for this Test Run Mode
                            m_TestEventArg.FirstSN = SN.Begin.ToString();
                            // Assign The last SN for this Test Run Mode
                            //m_TestEventArg.LastSN = SN.WaitLifterUp.ToString();

                            // Start the first SN
                            m_SeqNum = SN.Begin;
                        }
                        break;

                    case TestRunEnum.SN.CustomTest1:
                        lock (m_SyncSN)
                        {
                            m_TestEventArg.RunMode = TestEventArg.Run_Mode.Custom1;
                            // Assign The fist SN for this Test Run Mode
                            //m_TestEventArg.FirstSN = SN.StopGripperWork.ToString();
                            //// Assign The last SN for this Test Run Mode
                            //m_TestEventArg.LastSN = SN.WaitGripperWork.ToString();

                            //// Start the first SN
                            //m_SeqNum = SN.StopGripperWork;
                        }
                        break;

                    default:
                        m_TestRunResult.result = false;
                        m_TestRunResult.ErrMsg = GetDialogTableValue("TestRunNotFound");
                        Publisher.GetEvent<TestRunResult>().Publish(m_TestRunResult);
                        break;

                }
            }
        }

        internal void TCPMsgOperation(TCPIPMsg tcpMsg)
        {

        }

        public override void MachineOperation(string Operation)
        {
            if (Operation == "Init")
            {
                for (int i = 0; i < Total_RSM; i++)
                {
                    m_SeqRsm[i] = SN.NONE;
                }

                m_SeqNum = SN.IBegin;
            }
            else if (Operation == "Submit_Lot")
            {
                InitData();
            }
        }

        public override void StartProduction()
        {
            if (InitialStart)
            {
                InitialStart = false;
                //m_SeqNum = SN.Begin;

                m_SeqNum = SN.EOS;
            }
        }
        #endregion

        #region IO
        internal override void IOMapping()
        {
            //switch (SeqName)
            //{
            //    // In case ONE class with MULTIPLE module
            //    // assign IO into switch case individually
            //    case SQID.SampleSeq:
            //        // IO Mapping will assign all inputs and outputs of Sample Seq into IO view & Setup View
            //        // FIRST, Assign Vacuum IO to list, vaccum pairing and assign into Seq input and output list
            //        // Example:
            //        AssignVacuumIO("Vacuum 1", IN.DI0104_Input5, IN.DI0107_Input8, OUT.DO0107_Output8, OUT.DO0108_Output9); // Vacuum with both Vacuum, Purge, Vacuum on successful sensor, and vacuum picked up succeesful sensor
            //        AssignVacuumIO("Vacuum 2", null, IN.DI0107_Input8, OUT.DO0107_Output8, OUT.DO0108_Output9); // Only vacuum, purge and picked up sensor installed
            //        AssignVacuumIO("Vacuum 3", null, null, OUT.DO0107_Output8, OUT.DO0108_Output9); // No vacuum sensor installed 

            //        // NEXT, assign cylinder IO to list
            //        // Cylinder pairing and assign into Seq input and output list
            //        // Example:
            //        AssignCylinderIO("Test Gripper 1", null, null, null, null, OUT.DO0109_Test_GripperWork, null); // Single solenoid cylinder (set null to IO for rest)
            //        AssignCylinderIO("Test Gripper 2", IN.DI0109_Test_IsGripperWork, IN.DI0106_Input7, IN.DI0110_Test_IsGripperRest, IN.DI0105_Input6, OUT.DO0109_Test_GripperWork, OUT.DO0110_Test_GripperRest); // Double solenoid cylinder
            //        AssignCylinderIO("Test Gripper 3", IN.DI0109_Test_IsGripperWork, null, IN.DI0110_Test_IsGripperRest, null, OUT.DO0109_Test_GripperWork, OUT.DO0110_Test_GripperRest);
            //        AssignCylinderIO("Test Gripper 4", IN.DI0109_Test_IsGripperWork, IN.DI0106_Input7, IN.DI0110_Test_IsGripperRest, IN.DI0105_Input6, OUT.DO0109_Test_GripperWork, OUT.DO0110_Test_GripperRest);
            //        AssignCylinderIO("Test Gripper 5", IN.DI0109_Test_IsGripperWork, IN.DI0106_Input7, IN.DI0110_Test_IsGripperRest, IN.DI0105_Input6, OUT.DO0109_Test_GripperWork, OUT.DO0110_Test_GripperRest);
            //        AssignCylinderIO("Test Gripper 6", IN.DI0109_Test_IsGripperWork, IN.DI0106_Input7, IN.DI0110_Test_IsGripperRest, IN.DI0105_Input6, OUT.DO0109_Test_GripperWork, OUT.DO0110_Test_GripperRest);
            //        AssignCylinderIO("Test Gripper 7", IN.DI0109_Test_IsGripperWork, IN.DI0106_Input7, IN.DI0110_Test_IsGripperRest, IN.DI0105_Input6, OUT.DO0109_Test_GripperWork, OUT.DO0110_Test_GripperRest);

            //        // LASTLY, assign OTHER general IO to list
            //        // Input
            //        AssignIO(IN.DI0103_Input4);
            //        AssignIO(IN.DI0108_Input9);

            //        //Output
            //        AssignIO(OUT.DO0104_Output5);
            //        AssignIO(OUT.DO0105_Output6);
            //        AssignIO(OUT.DO0106_Output7);
            //        AssignIO(OUT.DO0107_Output8);
            //        break;


            //}
        }
        #endregion

        #region Method
        internal override void LoadToolLifeCfg()
        {
            m_toolLifeCfg = ToolLifeConfig.Open(SysCfgs.CounterCfgRef[CounterCFG.ToolLife].Reference);
        }

#if MOTION
        internal override void LoadMotionCfg()
        {
            m_MCTypes = Motion.GetType().GetInterfaces();

            MotCfgs.Clear();
            MotCfgs.Add(MotCFG.XAxis, SysCfgs.MotCfgRef[(int)MotCFG.XAxis].Reference);
            MotCfgs.Add(MotCFG.YAxis, SysCfgs.MotCfgRef[(int)MotCFG.YAxis].Reference);

            m_TmrErrHM = new CTimer[MotCfgs.Count];
            m_TmrDelayHM = new CTimer[MotCfgs.Count];
            m_HMStatus = new int[MotCfgs.Count];
            m_SeqHM = new SN_HM[MotCfgs.Count];
            m_LmtOffsetCnt = new int[MotCfgs.Count];

            for (int i = 0; i < MotCfgs.Count; i++)
            {
                m_TmrErrHM[i] = new CTimer();
                m_TmrDelayHM[i] = new CTimer();
                m_HMStatus[i] = 0;
                m_SeqHM[i] = SN_HM.EOS;
                m_LmtOffsetCnt[i] = 0;
            }

            m_AxisModel.ClearAll();

            for (int i = 0; i < MotCfgs.Count; i++)
            {
                m_AxisModel.MotCfgs.Add(MotionConfig.Open(MotCfgs.ElementAt(i).Value));
                m_AxisModel.Status.Add(new AxisStatus());
            }
        }

        private void PerformHomingSeq(MotionConfig motcfg, int i = 0)
        {
            switch (m_SeqHM[i])
            {
                case SN_HM.ResetMtrAlarm:
                    m_LmtOffsetCnt[i] = 0;
                    motcfg.Axis.IsHome = false;
                    ResetMtrAlarm(i, true);
                    m_TmrErrHM[i].Time_Out = 3.0f;
                    m_SeqHM[i] = SN_HM.WaitAlarmReset;
                    break;

                case SN_HM.WaitAlarmReset:
                    if (!GetAlarmStatus(motcfg))
                    {
                        ResetMtrAlarm(i, false);
                        ServoON(i);
                        m_SeqHM[i] = SN_HM.MoveMtrHome;
                    }
                    else if (m_TmrErrHM[i].TimeOut())
                    {
                        ResetMtrAlarm(i, false);
                        RaiseError((int)ErrorCode.AlarmNotClr);
                        m_HMStatus[i] = HM_FAIL;
                        m_SeqHM[i] = SN_HM.EOS;
                    }
                    break;

                case SN_HM.MoveMtrHome:
                    if (m_MCTypes[1].Name == "IACSMotion")
                    {
                        //Motion.RunBufferProgram(motcfg.Axis.CardID, i == 0 ? 2 : 3, null);
                        m_TmrErrHM[i].Time_Out = m_SeqCfg.Err[(int)ERR_T.LongDist].TimeOut;
                        m_SeqHM[i] = SN_HM.WaitMtrHome;
                    }
                    else
                    {
                        m_LmtOffsetCnt[i] = 0;
                        FindEdge(motcfg.Axis.CardID, motcfg.Axis.AxisID, (int)motcfg.Velocity[(int)MotCFG.Home].DriveVel);
                        m_TmrErrHM[i].Time_Out = m_SeqCfg.Err[(int)ERR_T.LongDist].TimeOut;
                        m_SeqHM[i] = SN_HM.WaitMtrHome;
                    }
                    break;

                case SN_HM.WaitMtrHome:
                    if (MtrReady(i))
                    {
                        m_SeqHM[i] = m_MCTypes[1].Name == "IACSMotion" ? SN_HM.MoveMtrGoLoad : SN_HM.MoveMtrFI;
                    }
                    else if (!ChkNegativeLimitStatus(motcfg.Axis.CardID, motcfg.Axis.AxisID) && m_MCTypes[1].Name != "IACSMotion")
                    {
                        Motion.StopServo(motcfg.Axis.CardID, motcfg.Axis.AxisID);
                        m_TmrDelayHM[i].Time_Out = 1;
                        m_SeqHM[i] = SN_HM.WaitLimitStopSettle;
                    }
                    else if (m_TmrErrHM[i].TimeOut())
                    {
                        StopServo(i);
                        RaiseError((int)MtrErr(i));
                        m_HMStatus[i] = HM_FAIL;
                        m_SeqHM[i] = SN_HM.EOS;
                    }
                    break;

                case SN_HM.MoveMtrFI:
#if Servo_FE_FI
                    FindIndex(motcfg.Axis.CardID, motcfg.Axis.AxisID, motcfg.Velocity[(int)MotCFG.Home].DriveVel);
#elif Servo_FE
                    FindEdge(motcfg.Axis.CardID, motcfg.Axis.AxisID, (int)motcfg.Velocity[(int)MotCFG.Home].DriveVel);
#endif
                    m_TmrErrHM[i].Time_Out = 10;
                    m_SeqHM[i] = SN_HM.WaitMtrFI;
                    break;

                case SN_HM.WaitMtrFI:
                    if (MtrReady(i))
                    {
                        m_SeqHM[i] = SN_HM.EndOfHomeProcess;
                    }
                    else if (m_TmrErrHM[i].TimeOut())
                    {
                        RaiseError((int)MtrErr(i));
                        m_HMStatus[i] = HM_FAIL;
                        m_SeqHM[i] = SN_HM.EOS;
                    }
                    break;

                case SN_HM.EndOfHomeProcess:
                    Wait(GetDelayTimeOut((int)DLY_T.SetZeroPos));
                    SetZeroPosition(motcfg.Axis.CardID, motcfg.Axis.AxisID);
                    m_SeqHM[i] = SN_HM.MoveMtrGoLoad;
                    break;

                // --- Initiate Negative Limit Switch Offset Process -------------------------------------------------

                case SN_HM.WaitLimitStopSettle:
                    if (m_TmrDelayHM[i].TimeOut())
                    {
                        m_LmtOffsetCnt[i]++;
                        m_SeqHM[i] = SN_HM.EvalLmtOffsetCnt;
                    }
                    break;

                case SN_HM.EvalLmtOffsetCnt:
                    if (m_LmtOffsetCnt[i] >= MAX_LMT_OFFSET)
                    {
                        RaiseError((int)MtrErr(i));
                        m_HMStatus[i] = HM_FAIL;
                        m_SeqHM[i] = SN_HM.EOS;
                    }
                    else
                    {
                        m_SeqHM[i] = SN_HM.MoveMtrLmtOffset;
                    }
                    break;

                case SN_HM.MoveMtrLmtOffset:
                    MoveRel(i, (int)P_PRF.LmtOffset, MotCFG.Medium);
                    m_TmrErrHM[i].Time_Out = GetErrTimeOut((int)ERR_T.ShortDist);
                    m_SeqHM[i] = SN_HM.WaitMtrLmtOffset;
                    break;

                case SN_HM.WaitMtrLmtOffset:
                    if (MtrReady(i))
                    {
                        m_SeqHM[i] = SN_HM.MoveMtrHome;
                    }
                    else if (m_TmrErrHM[i].TimeOut())
                    {
                        Motion.StopServo(motcfg.Axis.CardID, motcfg.Axis.AxisID);
                        RaiseError((int)MtrErr(i));
                        m_HMStatus[i] = HM_FAIL;
                        m_SeqHM[i] = SN_HM.EOS;
                    }
                    break;

                case SN_HM.MoveMtrGoLoad:
                    MoveAbs(motcfg.Axis.AxisID, (int)P_PRF.Load, MotCFG.Medium);
                    m_TmrErrHM[i].Time_Out = GetErrTimeOut((int)ERR_T.LongDist);
                    m_SeqHM[i] = SN_HM.WaitMtrGoLoad;
                    break;

                case SN_HM.WaitMtrGoLoad:
                    if (MtrReady(i))
                    {
                        if (motcfg.Position[(int)P_PRF.Load].Point == 0)
                        {
                            Wait(0.1f);
                            StopServo(i);
                        }

                        if (m_TestEventArg.RunMode == TestEventArg.Run_Mode.HomeMotor)
                        {
                            m_TestEventArg.RunMode = TestEventArg.Run_Mode.None;
                            m_TestRunResult.result = true;
                            m_TestRunResult.ErrMsg = string.Empty;
                            Publisher.GetEvent<TestRunResult>().Publish(m_TestRunResult);
                        }

                        motcfg.Axis.IsHome = true;
                        m_HMStatus[i] = HM_SUCCESS;
                        m_SeqHM[i] = SN_HM.EOS;
                    }
                    else if (m_TmrErrHM[i].TimeOut())
                    {
                        RaiseError((int)MtrErr(i));
                        m_HMStatus[i] = HM_FAIL;
                        m_SeqHM[i] = SN_HM.EOS;
                    }
                    break;
            }
        }
#endif
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

        internal override void CheckTestRunSN()
        {
            // Check is test running
            if (m_TestEventArg.RunMode != TestEventArg.Run_Mode.None)
            {
                // If error occurred, reset and stop seq
                if (m_TestEventArg.RunMode == TestEventArg.Run_Mode.Stop)
                {
                    m_TestEventArg.RunMode = TestEventArg.Run_Mode.None;
                    m_SeqRsm[(int)RSM.Err] = SN.NONE;
                    m_SeqNum = SN.EOS;
                }
                else
                {
                    // check m_SeqNum - 1 to make sure the LastSN is completely executed
                    if (m_TestEventArg.LastSN == (m_SeqNum - 1).ToString() || m_TestEventComp[(int)m_TestEventArg.RunMode])
                    {
                        m_TestEventComp[(int)m_TestEventArg.RunMode] = false;

                        m_TestEventArg.CycleCnt--;

                        // if cycle count more than one, set FirstSN to m_SeqNum again
                        if (m_TestEventArg.CycleCnt > 0)
                        {
                            m_SeqNum = (SN)Enum.Parse(typeof(SN), m_TestEventArg.FirstSN);
                        }
                        else
                        {
                            // reset and stop seq when cycle count is 0
                            m_TestEventArg.RunMode = TestEventArg.Run_Mode.None;
                            m_TestRunResult.result = true;
                            m_TestRunResult.ErrMsg = string.Empty;
                            Publisher.GetEvent<TestRunResult>().Publish(m_TestRunResult);
                            m_SeqNum = SN.EOS;
                        }
                    }
                }
            }
        }

        public override void ToolLifeOperation()
        {
            m_toolLifeCfg = ToolLifeConfig.Open(SysCfgs.CounterCfgRef[CounterCFG.ToolLife].Reference);
        }

        public bool IsPassSeqIntL()
        {
            bool isPassIntL = true;

            #region Project Specific 
            if (m_LocalVar.SeqIntLFailIDs.Count > 0)
            {
                if (m_LocalVar.SeqIntLFailIDs.Contains((int)SQID.SampleSeq2))
                {
                    RaiseError((int)ErrorCode.CodeReaderConnectionFail);
                    isPassIntL = false;
                }
            }
            #endregion

            return isPassIntL;
        }
        #endregion

        #region Properties
        public int Test1CleaningCnt
        {
            get
            {
                return m_toolLifeCfg.TLCollection[(int)ToolLifeID.Test1].CleaningValue;
            }
            set
            {
                m_toolLifeCfg.TLCollection[(int)ToolLifeID.Test1].CleaningValue = value;
                try
                {
                    ToolLifeConfig.Save(SysCfgs.CounterCfgRef[CounterCFG.ToolLife].Reference);
                }
                catch (Exception ex)
                {
                    Publisher.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Error, MsgText = $"{GetStringTableValue("Test1CleaningCntSaveError")} : {ex.Message}" });
                }
            }
        }
        #endregion

        #region Motion Error
#if MOTION
        private ErrorCode MtrErr(int axis = 0)
        {
            ErrorCode errCode;

            if (m_AxisModel.Status[axis].Alarm)
            {
                errCode = ErrorCode.MtrAlarm;
            }
            else if (m_AxisModel.Status[axis].Ready)
            {
                errCode = ErrorCode.MtrNotReady;
            }
            else if (m_AxisModel.Status[axis].InPos)
            {
                errCode = ErrorCode.MtrNotInPos;
            }
            else if (m_AxisModel.Status[axis].FwdLmt)
            {
                errCode = ErrorCode.HitFwdLimit;
            }
            else if (m_AxisModel.Status[axis].RevLmt)
            {
                errCode = ErrorCode.HitRevLimit;
            }
            else
            {
                errCode = ErrorCode.UnknownMtrErr;
            }
            return errCode;
        }
#endif
        #endregion
    }
}
