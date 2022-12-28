#define Servo_FE

#define GALIL    // ACS / ADVANTECH / GALIL
#define MOXA
//#define WAGO    // MOXA / ADVANTECH / ACS / WAGO
//#define ADLINK

using ConfigManager;
using CsvHelper;
using CsvHelper.Configuration;
using DataManager;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using GreatechApp.Services.Utilities;
using IOManager;
using MotionManager;
using MotionTest.IO_Manager;
using MotionTest.MotionManager;
using Prism.Commands;
using Prism.Events;
using Sequence.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using TCPIPManager;
using static ConfigManager.MotAxis;

namespace Sequence
{
    public abstract class BaseClass
    {
        #region Variable
        protected BaseClass[] m_BaseClass = null;

        protected AutoResetEvent m_PauseSeqEv = new AutoResetEvent(false);

        protected CTimer m_TmrErr = new CTimer();
        // Delay Timer
        protected CTimer m_TmrDelay = new CTimer();
        // Critical Scan Timer
        protected CTimer m_TmrScan = new CTimer();

        protected CTimer[] m_TmrDelayHM = null;

        protected CTimer[] m_TmrErrHM = null;

        protected TestEventArg m_TestEventArg = new TestEventArg();

        protected TestRunResult m_TestRunResult = new TestRunResult();

        protected bool[] m_TestEventComp = null;

        protected int[] m_HMStatus = null;

        protected int[] m_LmtOffsetCnt = null;

        protected const bool INVERT = true;

        protected int MAX_LMT_OFFSET = 2;

        protected object m_SyncSN = new object();

        protected object m_SyncEvent = new object();

        public SQID SeqName;

        public string SeqNum;

        float tempvisquantityholder = 0;

        public bool checkOp = false;

        public bool VisResume = false;

        public bool NotFrmResume = false;

        public int TotalInput = 0;

        public int TotalOutput = 0;

        public int TotalFail = 0;

        public MarkerType MarkerType = MarkerType.None;

        public int SlotNum = 0;

        public int StationNum = 0;

        protected bool InitialStart = true;

        private Dictionary<object, object> m_IOTbl = new Dictionary<object, object>();

        //Performance count
        internal List<PerfInfo> _PerfInfo = new List<PerfInfo>();

        protected CTimer _TmrPerfScan = new CTimer();

        protected AxisModel m_AxisModel = new AxisModel();

        internal Dictionary<int, string> MotCfgs = new Dictionary<int, string>();

        protected SeqConfig m_SeqCfg;

        public Type[] m_MCTypes;

        public SystemConfig SysCfgs;

        public CultureResources CultureResources;

        public ResultsDatalog m_resultsDatalog = new ResultsDatalog();

        public static object m_SyncLog = new object();

        public IEventAggregator Publisher;

        public IError Error;

        public ITCPIP TCPIP;

        public IInsightVision InsightVision;

        public ICodeReader CodeReader;

        public ISerialPort SerialPort;

        public IShowDialog ShowDialog;

        public IBaseData BaseData;

#if MOXA
        public IMoxaIO IO;
#elif ADVANTECH
        public IAdvantechIO IO;
#elif ACS
        public IACSIO IO;
#elif WAGO
        public IWagoIO IO;
#elif ADLINK
        public IAdlinkIO IO;
#endif

        public IBaseIO BaseIO
        {
            set
            {
#if MOXA
                IO = (IMoxaIO)value;
#elif ADVANTECH
                IO = (IAdvantechIO)value;
#elif ACS
                IO = (IACSIO)value;
#elif WAGO
                IO = (IWagoIO)value;
#elif ADLINK
                IO = (IAdlinkIO)value;
#endif
            }
        }

#if GALIL
        public IGalilMotion Motion;
#elif ADVANTECH
        public IAdvantechMotion Motion;
#elif ACS
        public IACSMotion Motion;
#elif ADLINK
        public IADLinkMotion Motion;
#endif

        public IBaseMotion BaseMotion
        {
            set
            {
#if GALIL
                Motion = (IGalilMotion)value;
#elif ADVANTECH
                Motion = (IAdvantechMotion)value;
#elif ACS
                Motion = (IACSMotion)value;
#elif ADLINK
                Motion = (IADLinkMotion)value;
#endif
            }
        }

        protected const int Total_RSM = 6;
        protected const int HM_SUCCESS = 1;
        protected const int HM_FAIL = -1;

        protected enum RSM
        {
            Err = 0,
            IntL,
            IntM,
            Src,
            Stop,
            Skip,
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct BaseFlag
        {
            internal bool Bypass;
            internal bool StartLot;
            internal bool BeginSeq;
            internal bool BeginInit;
            internal bool InitDone;
            internal bool InitSuccess;
            internal bool InitFail;
            internal bool ProcStart;
            internal bool ProcFail;
            internal bool ProcSkip;
            internal bool ProcAbort;
            internal bool ProcReady;
            internal bool ProcCont;
            internal bool ProcBusy;
            internal bool ProcComp;
            internal bool ItemGiven;
            internal bool ItemTaken;
            internal bool ReqNewItem;
            internal bool NewItemAvail;
            internal bool SeqIntLChk;
            internal bool SeqIntLComp;
            internal bool BeginEndLot;
            internal bool EndLotComp;
            internal bool KillSeq;
            internal bool ExtTestRunBegin;
            internal bool ExtTestRunAbort;
            internal bool ExtTestRunComp;
            internal bool ProcUpdate;
        }

        protected BaseFlag m_SeqFlag = new BaseFlag();
        #endregion

        public BaseClass()
        {
            m_TestEventArg.RunMode = TestEventArg.Run_Mode.None;
            m_TestEventComp = new bool[Enum.GetNames(typeof(TestEventArg.Run_Mode)).Length];
        }

        #region Logging 
        public void WriteSoftwareResultLog(ResultsDatalog log)
        {
            lock (m_SyncLog)
            {
                {
                    if (m_resultsDatalog.UserId == string.Empty)
                    {
                        m_resultsDatalog.UserId = Global.UserId;
                        m_resultsDatalog.UserLvl = Global.UserLvl;
                        DateTime currentTime = DateTime.Now;
                        DateTimeFormatInfo dateFormat = new DateTimeFormatInfo();
                        dateFormat.ShortDatePattern = "dd-MM-yyyy";
                        m_resultsDatalog.Date = currentTime.ToString("d", dateFormat);
                        m_resultsDatalog.Time = currentTime.ToString("HH:mm:ss.fff", DateTimeFormatInfo.InvariantInfo);
                        m_resultsDatalog.Timestamp = m_resultsDatalog.Date + " | " + m_resultsDatalog.Time;
                        m_resultsDatalog.CodeReader = inspectiontype.CodeReader.ToString();
                        m_resultsDatalog.DecodeBatchQuantity = Global.CurrentBatchQuantity;
                        m_resultsDatalog.DecodeBoxQuantity = Global.CurrentBoxQuantity;
                        m_resultsDatalog.DecodeAccuQuantity = Global.AccumulateCurrentBatchQuantity;
                        m_resultsDatalog.DecodeResult = Global.CodeReaderResult;
                        m_resultsDatalog.TopVision = inspectiontype.TopVision.ToString();
                        m_resultsDatalog.VisTotalPrdQty = Global.VisProductQuantity;
                        m_resultsDatalog.VisCorrectOrient = Global.VisProductCrtOrientation;
                        m_resultsDatalog.VisWrongOrient = Global.VisProductWrgOrientation;
                        m_resultsDatalog.ErrorMessage = null;
                        m_resultsDatalog.Remarks = null;
                        m_resultsDatalog.ApprovedBy = null;
                    }
                    //create log directory V2
                    string date = DateTime.Now.ToString("dd-MM-yyyy");
                    string filePath = $"{SysCfgs.FolderPath.SoftwareResultLog}Log[{date}]\\";
                    if (!Directory.Exists(filePath))
                        Directory.CreateDirectory(filePath);
                    
                    if(m_resultsDatalog.DecodeResult != null && m_resultsDatalog.VisTotalPrdQty != 0)
                    {
                        string filename = $"Batch {Global.CurrentBatchNum}.csv";
                        filename = filePath + filename;

                        var records = new List<ResultsDatalog>();
                        records.Add(m_resultsDatalog);

                        if(tempvisquantityholder != m_resultsDatalog.VisTotalPrdQty)
                        {
                            if (m_resultsDatalog.DecodeResult != "PendingResult" && Global.CurrentBatchNum != string.Empty)
                            {
                                if (!File.Exists(filename.ToString()))
                                {
                                    //create new .csv file and initialize the headers
                                    using (var writer = new StreamWriter(filename))
                                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                                    {
                                        csv.Context.RegisterClassMap<ResultsDatalog.ResultsDatalogMap>();
                                        csv.WriteRecords(records);
                                    }
                                }
                                else
                                {
                                    //append to .csv file
                                    var configList = new CsvConfiguration(CultureInfo.InvariantCulture)
                                    {
                                        HasHeaderRecord = false
                                    };

                                    using (var stream = File.Open(filename, FileMode.Append))
                                    using (var writer = new StreamWriter(stream))
                                    using (var csv = new CsvWriter(writer, configList))
                                    {
                                        csv.Context.RegisterClassMap<ResultsDatalog.ResultsDatalogMap>();
                                        csv.WriteRecords(records);
                                    }
                                }
                                tempvisquantityholder = m_resultsDatalog.VisTotalPrdQty;
                                m_resultsDatalog.ClearAll();
                            }
                        }
                    }
                    else
                    {
                        date = DateTime.Now.ToString("dd-MM-yyyy");
                    }
                }
            }
        }
        #endregion

        #region Method
        public string GetStringTableValue(string key)
        {
            return CultureResources.GetStringValue(key);
        }
        public string GetDialogTableValue(string key)
        {
            return CultureResources.GetDialogValue(key);
        }

        private void ResetPerfInfo()
        {
            foreach (PerfInfo info in _PerfInfo)
            {
                info.Reset();
            }
        }

        protected void StartCycleTime(int perfInfo)
        {
            if (perfInfo < 0 || perfInfo >= _PerfInfo.Count)
            {
                return;
            }
            _PerfInfo[perfInfo].ArmTime = _TmrPerfScan.Mili_Time;
        }

        protected void EndCycleTime(int perfInfo)
        {
            if (perfInfo < 0 || perfInfo >= _PerfInfo.Count)
            {
                return;
            }

            if (_PerfInfo[perfInfo].ArmTime == 0)
            {
                // since did not call a StartCycleTime().
                return;
            }

            // Record last cycle time.
            _PerfInfo[perfInfo].LastCycleTime = (int)(_TmrPerfScan.Mili_Time - _PerfInfo[perfInfo].ArmTime);
            // Total all cycle time.
            _PerfInfo[perfInfo].TotalTime += _PerfInfo[perfInfo].LastCycleTime;

            // Record min cycle time.
            if (0 == _PerfInfo[perfInfo].Min || _PerfInfo[perfInfo].Min > _PerfInfo[perfInfo].LastCycleTime)
            {
                _PerfInfo[perfInfo].Min = _PerfInfo[perfInfo].LastCycleTime;
            }

            // Record max cycle time.
            if (0 == _PerfInfo[perfInfo].Max || _PerfInfo[perfInfo].Max < _PerfInfo[perfInfo].LastCycleTime)
            {
                _PerfInfo[perfInfo].Max = _PerfInfo[perfInfo].LastCycleTime;
            }

            // Increase total cycle count.
            ++_PerfInfo[perfInfo].TotalCycle;

            // Average cycle time.
            if (_PerfInfo[perfInfo].TotalCycle > 0)
            {
                _PerfInfo[perfInfo].Avg = (int)(_PerfInfo[perfInfo].TotalTime / _PerfInfo[perfInfo].TotalCycle);
            }
        }

        internal int GetLastCycleTime(int perfInfo)
        {
            if (_PerfInfo == null)
            {
                return 0;
            }
            if (perfInfo < 0 || perfInfo >= _PerfInfo.Count)
            {
                return 0;
            }
            return _PerfInfo[perfInfo].LastCycleTime;
        }

        internal double GetAvgCycleTime(int perfInfo)
        {
            if (perfInfo < 0 || perfInfo >= _PerfInfo.Count)
            {
                return 0;
            }
            return _PerfInfo[perfInfo].Avg;
        }

        internal int GetMaxCycleTime(int perfInfo)
        {
            if (perfInfo < 0 || perfInfo >= _PerfInfo.Count)
            {
                return 0;
            }
            return _PerfInfo[perfInfo].Max;
        }

        internal int GetMinCycleTime(int perfInfo)
        {
            if (perfInfo < 0 || perfInfo >= _PerfInfo.Count)
            {
                return 0;
            }
            return _PerfInfo[perfInfo].Min;
        }

        internal long GetTotalCycleTime(int perfInfo)
        {
            if (_PerfInfo == null)
            {
                return 0;
            }
            if (perfInfo < 0 || perfInfo >= _PerfInfo.Count)
            {
                return 0;
            }
            return _PerfInfo[perfInfo].TotalTime;
        }

        protected int GetCycleCount(int perfInfo)
        {
            if (perfInfo < 0 || perfInfo >= _PerfInfo.Count)
            {
                return 0;
            }
            return _PerfInfo[perfInfo].TotalCycle;
        }
        internal virtual void CheckLiveSeqStart()
        {

        }

        internal virtual void CheckTestRunSN()
        {

        }

        #endregion

        #region Properties
        public virtual void OnBypassStation(bool state)
        {
            m_SeqFlag.Bypass = state;
        }

        public virtual void OperationChecking(bool checkopr)
        {

        }

        public virtual void OnRunSeq(object sender, EventArgs args)
        {

        }

        public virtual void SubscribeSeqEvent()
        {

        }

        public virtual void SubscribeTestRunEvent()
        {

        }


        public virtual void SubscribeTCPMessage()
        {

        }

        public virtual void SubscribeSerialMessage()
        {

        }

        internal virtual void LoadMotionCfg()
        {
        }

        internal virtual void LoadSeqCfg(string seqCfgRef)
        {
            m_SeqCfg = SeqConfig.Open(seqCfgRef);
            Debug.Assert(m_SeqCfg != null);
        }

        internal virtual void LoadToolLifeCfg()
        {

        }

        internal virtual void SequenceOperation(SequenceEvent sequence)
        {
            switch (sequence.MachineOpr)
            {
                case MachineOperationType.StartLot:
                    m_SeqFlag.StartLot = true;
                    break;

                case MachineOperationType.BeginSeq:
                    m_SeqFlag.BeginSeq = true;
                    break;

                case MachineOperationType.BeginInit:
                    m_SeqFlag.BeginInit = true;
                    break;

                case MachineOperationType.InitDone:
                    m_SeqFlag.InitDone = true;
                    break;

                case MachineOperationType.InitFail:
                    m_SeqFlag.InitFail = true;
                    break;

                case MachineOperationType.ProcStart:
                    m_SeqFlag.ProcStart = true;
                    break;

                case MachineOperationType.ProcSkip:
                    m_SeqFlag.ProcSkip = true;
                    break;

                case MachineOperationType.ProcAbort:
                    m_SeqFlag.ProcAbort = true;
                    break;

                case MachineOperationType.ProcReady:
                    m_SeqFlag.ProcReady = true;
                    break;

                case MachineOperationType.ProcCont:
                    m_SeqFlag.ProcCont = true;
                    break;

                case MachineOperationType.ProcUpdate:
                    m_SeqFlag.ProcUpdate = true;
                    break;

             

                case MachineOperationType.ProcBusy:
                    m_SeqFlag.ProcBusy = true;
                    break;

                case MachineOperationType.ProcComp:
                    m_SeqFlag.ProcComp = true;
                    break;

                case MachineOperationType.ItemGiven:
                    m_SeqFlag.ItemGiven = true;
                    break;

                case MachineOperationType.ItemTaken:
                    m_SeqFlag.ItemTaken = true;
                    break;

                case MachineOperationType.ReqNewItem:
                    m_SeqFlag.ReqNewItem = true;
                    break;

                case MachineOperationType.NewItemAvail:
                    m_SeqFlag.NewItemAvail = true;
                    break;

                case MachineOperationType.SeqIntLChk:
                    m_SeqFlag.SeqIntLChk = true;
                    break;

                case MachineOperationType.SeqIntLComp:
                    m_SeqFlag.SeqIntLComp = true;
                    break;

                case MachineOperationType.BeginEndLot:
                    m_SeqFlag.BeginEndLot = true;
                    break;

                case MachineOperationType.EndLotComp:
                    m_SeqFlag.EndLotComp = true;
                    break;

                case MachineOperationType.KillSeq:
                    m_SeqFlag.KillSeq = true;
                    break;

                case MachineOperationType.ExtTestRunBegin:
                    m_SeqFlag.ExtTestRunBegin = true;
                    break;

                case MachineOperationType.ExtTestRunAbort:
                    m_SeqFlag.ExtTestRunAbort = true;
                    break;

                case MachineOperationType.ExtTestRunComp:
                    m_SeqFlag.ExtTestRunComp = true;
                    break;
            }
        }

        internal virtual void TestRunOperation(TestRunEvent testRunEvent)
        {

        }

        internal virtual void TCPIPMsg(TCPIPMsg tcpMsg)
        {

        }

        protected virtual void InitData()
        {
            InitialStart = true;
            TotalInput = 0;
            TotalOutput = 0;
            TotalFail = 0;
            m_SeqFlag.StartLot = false;
            m_SeqFlag.BeginInit = false;
            m_SeqFlag.BeginSeq = false;
            m_SeqFlag.InitDone = false;
            m_SeqFlag.InitSuccess = false;
            m_SeqFlag.InitFail = false;
            m_SeqFlag.ProcStart = false;
            m_SeqFlag.ProcSkip = false;
            m_SeqFlag.ProcAbort = false;
            m_SeqFlag.ProcReady = false;
            m_SeqFlag.ProcCont = false;
            m_SeqFlag.ProcBusy = false;
            m_SeqFlag.ProcComp = false;
            m_SeqFlag.ItemGiven = false;
            m_SeqFlag.ItemTaken = false;
            m_SeqFlag.ReqNewItem = false;
            m_SeqFlag.NewItemAvail = false;
            m_SeqFlag.SeqIntLChk = false;
            m_SeqFlag.SeqIntLComp = false;
            m_SeqFlag.BeginEndLot = false;
            m_SeqFlag.EndLotComp = false;
            m_SeqFlag.KillSeq = false;

            m_SeqFlag.ExtTestRunBegin = false;
            m_SeqFlag.ExtTestRunAbort = false;
            m_SeqFlag.ExtTestRunComp = false;

            ResetPerfInfo();

            // Reset BaseData
            foreach (Slot slot in BaseData.ModuleSlots)
                slot.ResetUnit();
        }

        public DelegateCommand<string> MachOperation { get; set; }

        public virtual void MachineOperation(string Func)
        {

        }

        public virtual void StartProduction()
        {

        }

        public DelegateCommand TLOperation { get; set; }

        public virtual void ToolLifeOperation()
        {

        }
        #endregion

        #region Cylinder
        internal float GetErrTimeOut(int idx)
        {
            Debug.Assert(idx < m_SeqCfg.Err.Count, "idx must be < than the array length.");
            if (idx >= m_SeqCfg.Err.Count)
            {
                return 0.0f;
            }
            return m_SeqCfg.Err[idx].TimeOut;
        }

        internal float GetDelayTimeOut(int idx)
        {
            Debug.Assert(idx < m_SeqCfg.Delay.Count, "idx must be < than the array length.");
            if (idx >= m_SeqCfg.Delay.Count)
            {
                return 0.0f;
            }
            return m_SeqCfg.Delay[idx].TimeOut;
        }
        #endregion

        #region Counter & Option
        internal int GetCounterValue(int idx)
        {
            Debug.Assert(idx < m_SeqCfg.Counter.Count, "idx must be < than the array length.");
            if (idx >= m_SeqCfg.Counter.Count)
            {
                return 0;
            }
            return m_SeqCfg.Counter[idx].Value;
        }

        internal int GetOptionValue(int idx)
        {
            Debug.Assert(idx < m_SeqCfg.Option.Count, "idx must be < than the array length.");
            if (idx >= m_SeqCfg.Option.Count)
            {
                return 0;
            }
            return m_SeqCfg.Option[idx].Value;
        }
        #endregion

        #region IO
        internal virtual void IOMapping()
        {

        }

        protected virtual void AssignVacuumIO(string vacuumName, IN? pickedUpSns, OUT vacuum, OUT? purge)
        {
            // Vacuum input and output pairing 
            IO.VacuumCylinderList.Add(new VacuumCylinderIO
            {
                SeqName = SeqName,
                VacuumName = vacuumName,
                Vacuum = vacuum,
                VacuumPickedUpSns1 = pickedUpSns,
                Purge = purge,

            });

            // Assign Vacuum Output into Seq OutputList
            IO.AssignOutput(SeqName, vacuum);
            if (purge != null)
                IO.AssignOutput(SeqName, purge.Value);

            // Assign Vacuum Input into Seq InputList
            if (pickedUpSns != null)
                IO.AssignInput(SeqName, pickedUpSns.Value);
        }

        protected virtual void AssignVacuumIO(string vacuumName, IN? vacuumOnSns, IN? pickedUpSns, OUT vacuum, OUT? purge)
        {
            // vacuumOnSns is sensor of checking is vacuum turned on after writebit
            // pickedUpsns is sensor of checking vacuum picked up successfully

            // Vacuum input and output pairing 
            IO.VacuumCylinderList.Add(new VacuumCylinderIO
            {
                SeqName = SeqName,
                VacuumName = vacuumName,
                Vacuum = vacuum,
                VacuumPressureSns1 = vacuumOnSns,
                Purge = purge,
                VacuumPickedUpSns1 = pickedUpSns,
            });

            // Assign Vacuum Output into Seq OutputList
            IO.AssignOutput(SeqName, vacuum);
            if (purge != null)
                IO.AssignOutput(SeqName, purge.Value);

            // Assign Vacuum Input into Seq InputList
            if (vacuumOnSns != null)
                IO.AssignInput(SeqName, vacuumOnSns.Value);
            if (pickedUpSns != null)
                IO.AssignInput(SeqName, pickedUpSns.Value);
        }

        protected virtual void AssignCylinderIO(string doubleSolenoidCylinderName, IN? workSns1, IN? workSns2, IN? restSns1, IN? restSns2, OUT work, OUT? rest)
        {
            // Cylinder input and output pairing 
            IO.VacuumCylinderList.Add(new VacuumCylinderIO
            {
                SeqName = SeqName,
                CylinderName = doubleSolenoidCylinderName,
                Work = work,
                WorkSns1 = workSns1,
                WorkSns2 = workSns2,
                Rest = rest,
                RestSns1 = restSns1,
                RestSns2 = restSns2
            });

            // Assign Cylinder Output into Seq OutputList
            IO.AssignOutput(SeqName, work);
            if (rest != null)
                IO.AssignOutput(SeqName, rest.Value);

            // Assign Cylinder Input into Seq InputList
            if(workSns1 !=null)
                IO.AssignInput(SeqName, workSns1.Value);
            if(restSns1 !=null)
                IO.AssignInput(SeqName, restSns1.Value);
            if (workSns2 != null)
                IO.AssignInput(SeqName, workSns2.Value);
            if (restSns2 != null)
                IO.AssignInput(SeqName, restSns2.Value);
        }

        protected virtual void AssignCylinderIO(string singleSolenoidCylinderName, IN workSns, IN restSns, OUT work)
        {
            // Cylinder input and output pairing 
            IO.VacuumCylinderList.Add(new VacuumCylinderIO
            {
                SeqName = SeqName,
                CylinderName = singleSolenoidCylinderName,
                Work = work,
                WorkSns1 = workSns,
                RestSns1 = restSns,
            });

            // Assign Cylinder Output into Seq OutputList
            IO.AssignOutput(SeqName, work);

            // Assign Cylinder Input into Seq InputList
            IO.AssignInput(SeqName, workSns);
            IO.AssignInput(SeqName, restSns);
        }

        protected virtual void AssignIO(IN masterInput)
        {
            // Assign selected input from masterlist into Seq InputList
            IO.AssignInput(SeqName, masterInput);
        }
        protected virtual void AssignIO(OUT masterOutput)
        {
            // Assign selected output from masterlist into Seq OutputList
            IO.AssignOutput(SeqName, masterOutput);
        }

        protected void AssignIO(object key, IN ioNum)
        {
            m_IOTbl.Add(key, ioNum);
            IO.AssignInput(SeqName, ioNum);
        }

        protected void AssignIO(object key, OUT ioNum)
        {
            m_IOTbl.Add(key, ioNum);
            IO.AssignOutput(SeqName, ioNum);
        }

        protected virtual bool ReadBit(object bit, bool isUsingLocalEnum = false)
        {
            if (isUsingLocalEnum)
            {
                return IO.ReadBit((int)m_IOTbl[bit]);
            }
            else
            {
                return IO.ReadBit((int)bit);
            }
        }

        protected virtual bool ReadBit(object bit, bool invert, bool isUsingLocalEnum = false)
        {
            if (isUsingLocalEnum)
            {
                return IO.ReadBit((int)m_IOTbl[bit], invert);
            }
            else
            {
                return IO.ReadBit((int)bit, invert);
            }
        }

        protected virtual bool ReadOutBit(object bit, bool isUsingLocalEnum = false)
        {
            if (isUsingLocalEnum)
            {
                return IO.ReadOutBit((int)m_IOTbl[bit]);
            }
            else
            {
                return IO.ReadOutBit((int)bit);
            }
        }

        protected virtual bool ReadBitT(object bit, bool isUsingLocalEnum = false)
        {
#if SIMULATION
            return true;
#else
            return ReadBit(bit, isUsingLocalEnum);
#endif
        }

        protected virtual bool ReadBitT(object bit, bool invert, bool isUsingLocalEnum = false)
        {
#if SIMULATION
            return true;
#else
            return ReadBit(bit, invert, isUsingLocalEnum);
#endif
        }

        protected virtual bool ReadBitF(object bit, bool isUsingLocalEnum = false)
        {
#if SIMULATION
            return false;
#else
            return ReadBit(bit, isUsingLocalEnum);
#endif
        }

        protected virtual bool ReadBitF(object bit, bool invert, bool isUsingLocalEnum = false)
        {
#if SIMULATION
            return false;
#else
            return ReadBit(bit, invert, isUsingLocalEnum);
#endif
        }

        public bool ReadOutBitF(int bit, bool isUsingLocalEnum = false)
        {
#if SIMULATION
            return false;
#else
            return ReadOutBit(bit, isUsingLocalEnum);
#endif
        }

        public bool ReadOutBitT(int bit, bool isUsingLocalEnum = false)
        {
#if SIMULATION
            return true;
#else
            return ReadOutBit(bit, isUsingLocalEnum);
#endif
        }


        protected virtual void WriteBit(object bit, bool oState, bool isUsingLocalEnum = false)
        {
#if !SIMULATION
            if(isUsingLocalEnum)
            {
                IO.WriteBit((int)m_IOTbl[bit], oState);
            }
            else
            {
                IO.WriteBit((int)bit, oState);
            }
#endif
        }

        /// Single Solenoid Valve
        /// State true - Extend Cylinder
        /// State false - Retract Cylinder
        protected virtual void WriteBit(object work, object rest, bool workState, bool isUsingLocalEnum = false)
        {
            if (isUsingLocalEnum)
            {
                if (workState)
                    IO.WorkCylinder((int)m_IOTbl[work], (int)m_IOTbl[rest]);
                else
                    IO.RestCylinder((int)m_IOTbl[rest], (int)m_IOTbl[work]);
            }
            else
            {
                if (workState)
                    IO.WorkCylinder((int)work, (int)rest);
                else
                    IO.RestCylinder((int)rest, (int)work);
            }
        }

        public bool CheckCylinderWork(int workSensor, int restSensor, bool isUsingLocalEnum = false)
        {
            return ReadBitT(workSensor, isUsingLocalEnum) && !ReadBitF(restSensor, isUsingLocalEnum);
        }

        public bool CheckCylinderRest(int restSensor, int workSensor, bool isUsingLocalEnum = false)
        {
            return ReadBitT(restSensor, isUsingLocalEnum) && !ReadBitF(workSensor, isUsingLocalEnum);
        }
        #endregion

        #region Motion
        protected virtual bool MtrReady(int axis = 0)
        {
#if SIMULATION
            return true;
#else

            m_AxisModel.Status[axis].Busy = Motion.AxisInMotion(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID);
            // Retrieve Motor Alarm signal status
            m_AxisModel.Status[axis].Alarm = GetAlarmStatus(m_AxisModel.MotCfgs[axis]);
            // Retrieve Motor Ready signal status
            m_AxisModel.Status[axis].Ready = GetReadyStatus(m_AxisModel.MotCfgs[axis]);

            if (m_AxisModel.MotCfgs[axis].Option.ChkInPos)
            {
                m_AxisModel.Status[axis].InPos = Motion.GetMotionDoneStatus(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID);
                //m_AxisModel.Status[axis].InPos = !Motion.AxisInMotion(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID);
            }
            if (m_AxisModel.MotCfgs[axis].Option.ChkFwdLimit)
            {
                m_AxisModel.Status[axis].FwdLmt = Motion.GetPositiveLimitStatus(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID);
            }
            if (m_AxisModel.MotCfgs[axis].Option.ChkRevLimit)
            {
                m_AxisModel.Status[axis].RevLmt = Motion.GetNegativeLimitStatus(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID);
            }

            if (m_AxisModel.Status[axis].AllPass())
            {
                return true;
            }
            return false;
#endif
        }
        protected virtual bool GetAlarmStatus(MotionConfig motCfg, Enum iAlarm = null)
        {
#if SIMULATION
            return false;
#endif
            if (motCfg.Option.ChkAlarm)
            {
                if (motCfg.Option.AlarmContact == Contact.NO)
                {
                    // No inversion
                    return Motion.GetAlarmStatus(motCfg.Axis.CardID, motCfg.Axis.AxisID);
                }
                return !Motion.GetAlarmStatus(motCfg.Axis.CardID, motCfg.Axis.AxisID);
            }
            // Always return false if no need to check for alarm.
            return false;
        }

        protected virtual bool GetReadyStatus(MotionConfig motCfg)
        {
#if SIMULATION
            return true;
#endif
            if (motCfg.Option.ChkReady)
            {
                if (motCfg.Option.ReadyContact == Contact.NO)
                {
                    return Motion.GetServoStatus(motCfg.Axis.CardID, motCfg.Axis.AxisID);
                }
                else
                {
                    return !Motion.GetServoStatus(motCfg.Axis.CardID, motCfg.Axis.AxisID);
                }
            }
            // Always return true if no Ready Signal is available.
            return true;
        }

        protected void Wait(double period)
        {
            double delay = period * 1000; // convert from s to ms.
            if (delay > 0)
            {
                // Causes current thread to go into a Wait state
                m_PauseSeqEv.WaitOne((int)delay);
            }
        }

        protected virtual bool MotionTimeout(CTimer tmr, int axis = 0)
        {
            // When a system does not have limit sensors or alarm signal, the flag is always false.
            // Note: can't use this for limit offset homing phase.
            if (tmr.TimeOut() || m_AxisModel.Status[axis].FwdLmt || m_AxisModel.Status[axis].RevLmt || m_AxisModel.Status[axis].Alarm)
            {
                return true;
            }
            return false;
        }

        protected virtual bool MoveAbs(int axis, double position, int speed)
        {
#if SIMULATION
            return true;
#else
            Motion.SetAxisParam(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID,
                                m_AxisModel.MotCfgs[axis].Velocity[speed].DriveVel, m_AxisModel.MotCfgs[axis].Velocity[speed].Acc, m_AxisModel.MotCfgs[axis].Velocity[speed].Dcc,
                                m_AxisModel.MotCfgs[axis].Velocity[speed].JerkTime, m_AxisModel.MotCfgs[axis].Velocity[speed].KillDcc);

            int pulse = 0;

#if ACS
            pulse = position * 1000;
#else
            if (m_AxisModel.MotCfgs[axis].Axis.System == DriveMethod.Linear)
            {
#if ADVANTECH
                pulse = Motion.mm2Pulse(m_AxisModel.MotCfgs[axis].Axis.Revolution, m_AxisModel.MotCfgs[axis].Axis.Pitch, (float)position, true);
#else
                pulse = Motion.mm2Pulse(m_AxisModel.MotCfgs[axis].Axis.Revolution, m_AxisModel.MotCfgs[axis].Axis.Pitch, (float)position);
#endif
            }
            else if (m_AxisModel.MotCfgs[axis].Axis.System == DriveMethod.Rotary)
            {
                pulse = Motion.degree2pulse(m_AxisModel.MotCfgs[axis].Axis.Revolution, (float)position);
            }
#endif
            return Motion.MoveAbs(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID, pulse, m_AxisModel.MotCfgs[axis].Dir.Opr);
#endif
        }

        protected virtual bool MoveAbs(int axis, int distIdx, int speed)
        {
#if SIMULATION
            return true;
#else

            Motion.SetAxisParam(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID,
                                m_AxisModel.MotCfgs[axis].Velocity[speed].DriveVel, m_AxisModel.MotCfgs[axis].Velocity[speed].Acc, m_AxisModel.MotCfgs[axis].Velocity[speed].Dcc,
                                m_AxisModel.MotCfgs[axis].Velocity[speed].JerkTime, m_AxisModel.MotCfgs[axis].Velocity[speed].KillDcc);

            int pulse = 0;

#if ACS
            pulse = (int)(m_AxisModel.MotCfgs[axis].Position[distIdx].Point * 1000);
#else
            if (m_AxisModel.MotCfgs[axis].Axis.System == DriveMethod.Linear)
            {
#if ADVANTECH
                pulse = Motion.mm2Pulse(m_AxisModel.MotCfgs[axis].Axis.Revolution, m_AxisModel.MotCfgs[axis].Axis.Pitch, (float)m_AxisModel.MotCfgs[axis].Position[distIdx].Point, true);
#else
                pulse = Motion.mm2Pulse(m_AxisModel.MotCfgs[axis].Axis.Revolution, m_AxisModel.MotCfgs[axis].Axis.Pitch, (float)m_AxisModel.MotCfgs[axis].Position[distIdx].Point);
#endif
            }
            else if (m_AxisModel.MotCfgs[axis].Axis.System == DriveMethod.Rotary)
            {
                pulse = Motion.degree2pulse(m_AxisModel.MotCfgs[axis].Axis.Revolution, (float)m_AxisModel.MotCfgs[axis].Position[distIdx].Point);
            }
#endif
            return Motion.MoveAbs(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID, pulse, m_AxisModel.MotCfgs[axis].Dir.Opr);
#endif
        }

        protected bool MoveAbsZeroPos(int axis, int speed)
        {
#if SIMULATION
            return true;
#else

            Motion.SetAxisParam(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID,
                                m_AxisModel.MotCfgs[axis].Velocity[speed].DriveVel, m_AxisModel.MotCfgs[axis].Velocity[speed].Acc, m_AxisModel.MotCfgs[axis].Velocity[speed].Dcc,
                                m_AxisModel.MotCfgs[axis].Velocity[speed].JerkTime, m_AxisModel.MotCfgs[axis].Velocity[speed].KillDcc);
            int ZeroPos = 0;
            int pulse = 0;

#if ACS
            pulse = (int)(m_AxisModel.MotCfgs[axis].Position[distIdx].Point * 1000);
#else
            if (m_AxisModel.MotCfgs[axis].Axis.System == DriveMethod.Linear)
            {
#if ADVANTECH
                pulse = Motion.mm2Pulse(m_AxisModel.MotCfgs[axis].Axis.Revolution, m_AxisModel.MotCfgs[axis].Axis.Pitch, (float)m_AxisModel.MotCfgs[axis].Position[distIdx].Point, true);
#else
                pulse = Motion.mm2Pulse(m_AxisModel.MotCfgs[axis].Axis.Revolution, m_AxisModel.MotCfgs[axis].Axis.Pitch, ZeroPos);
#endif
            }
            else if (m_AxisModel.MotCfgs[axis].Axis.System == DriveMethod.Rotary)
            {
                pulse = Motion.degree2pulse(m_AxisModel.MotCfgs[axis].Axis.Revolution, ZeroPos);
            }
#endif
            return Motion.MoveAbs(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID, pulse, m_AxisModel.MotCfgs[axis].Dir.Opr);
#endif
        }

        protected virtual bool MoveRel(int axis, int distIdx, int speed)
        {
#if SIMULATION
            return true;
#else
            Motion.SetAxisParam(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID,
                                m_AxisModel.MotCfgs[axis].Velocity[speed].DriveVel, m_AxisModel.MotCfgs[axis].Velocity[speed].Acc, m_AxisModel.MotCfgs[axis].Velocity[speed].Dcc,
                                m_AxisModel.MotCfgs[axis].Velocity[speed].JerkTime, m_AxisModel.MotCfgs[axis].Velocity[speed].KillDcc);

            int pulse = 0;

            pulse = (int)(m_AxisModel.MotCfgs[axis].Position[distIdx].Point);
#if ACS
            if (m_AxisModel.MotCfgs[axis].Position[distIdx].Point > 0)
            {
                pulse = Math.Abs((int)pulse * 1000);
            }
            else
            {
                pulse = -(Math.Abs((int)pulse * 1000));
            }
#else
            if (m_AxisModel.MotCfgs[axis].Axis.System == DriveMethod.Linear)
            {
#if ADVANTECH
                pulse = Motion.mm2Pulse(m_AxisModel.MotCfgs[axis].Axis.Revolution, m_AxisModel.MotCfgs[axis].Axis.Pitch, (float)pulse, true);
#else
                pulse = Motion.mm2Pulse(m_AxisModel.MotCfgs[axis].Axis.Revolution, m_AxisModel.MotCfgs[axis].Axis.Pitch, (float)pulse);
#endif
            }
            else if (m_AxisModel.MotCfgs[axis].Axis.System == DriveMethod.Rotary)
            {
                pulse = Motion.degree2pulse(m_AxisModel.MotCfgs[axis].Axis.Revolution, (float)pulse);
            }
#endif
            return Motion.MoveRel(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID, pulse);
#endif
        }


        protected bool MoveRelRecipe(int axis, int dist, int speed)
        {
#if SIMULATION
            return true;
#else
            Motion.SetAxisParam(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID,
                                m_AxisModel.MotCfgs[axis].Velocity[speed].DriveVel, m_AxisModel.MotCfgs[axis].Velocity[speed].Acc, m_AxisModel.MotCfgs[axis].Velocity[speed].Dcc,
                                m_AxisModel.MotCfgs[axis].Velocity[speed].JerkTime, m_AxisModel.MotCfgs[axis].Velocity[speed].KillDcc);

            int pulse = 0;

            pulse = dist;
#if ACS
            if (m_AxisModel.MotCfgs[axis].Position[distIdx].Point > 0)
            {
                pulse = Math.Abs((int)pulse * 1000);
            }
            else
            {
                pulse = -(Math.Abs((int)pulse * 1000));
            }
#else
            if (m_AxisModel.MotCfgs[axis].Axis.System == DriveMethod.Linear)
            {
#if ADVANTECH
                pulse = Motion.mm2Pulse(m_AxisModel.MotCfgs[axis].Axis.Revolution, m_AxisModel.MotCfgs[axis].Axis.Pitch, (float)pulse, true);
#else
                pulse = Motion.mm2Pulse(m_AxisModel.MotCfgs[axis].Axis.Revolution, m_AxisModel.MotCfgs[axis].Axis.Pitch, (float)pulse);
#endif
            }
            else if (m_AxisModel.MotCfgs[axis].Axis.System == DriveMethod.Rotary)
            {
                pulse = Motion.degree2pulse(m_AxisModel.MotCfgs[axis].Axis.Revolution, (float)pulse);
            }
#endif
            return Motion.MoveRel(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID, pulse);
#endif
        }


        protected virtual bool Jog(int axis, int speed, short direction)
        {
#if Advantech
            Motion.SetAxisJogParam(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID,
                                   m_AxisModel.MotCfgs[axis].Velocity[speed].DriveVel, m_AxisModel.MotCfgs[axis].Velocity[speed].Acc, m_AxisModel.MotCfgs[axis].Velocity[speed].Dcc);
#else
            Motion.SetAxisParam(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID,
                                m_AxisModel.MotCfgs[axis].Velocity[speed].DriveVel, m_AxisModel.MotCfgs[axis].Velocity[speed].Acc, m_AxisModel.MotCfgs[axis].Velocity[speed].Dcc,
                                m_AxisModel.MotCfgs[axis].Velocity[speed].JerkTime, m_AxisModel.MotCfgs[axis].Velocity[speed].KillDcc);
#endif

            return Motion.Jog(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID,
                                direction, m_AxisModel.MotCfgs[axis].Velocity[speed].DriveVel);
        }

        protected virtual bool ChkServoStatus(int axis)
        {
#if SIMULATION
            return true;
#else
            return Motion.GetServoStatus(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID);
#endif
        }

        protected virtual bool ResetMtrAlarm(int axis, bool isResetAlarm)
        {
#if SIMULATION
            return true;
#else
            return Motion.ResetMotorAlarm(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID, isResetAlarm);
#endif
        }

        protected virtual bool ServoON(int axis)
        {
#if SIMULATION
            return true;
#else
            return Motion.ServoON(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID);
#endif
        }

        protected virtual bool StopServo(int axis)
        {
#if SIMULATION
            return true;
#else
            return Motion.StopServo(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID);
#endif
        }

        protected virtual bool ChkAxisInMotion(int axis)
        {
#if SIMULATION
            return false;
#else
            return Motion.AxisInMotion(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID);
#endif
        }

        protected virtual bool ChkHomeLimitStatus(int cardID, int axisID)
        {
#if SIMULATION
            return true;
#else
            return Motion.GetHomeLimitStatus(cardID, axisID);
#endif
        }

        protected virtual bool ChkPositiveLimitStatus(int cardID, int axisID)
        {
#if SIMULATION
            return true;
#else
            return Motion.GetPositiveLimitStatus(cardID, axisID);
#endif
        }

        protected virtual bool ChkNegativeLimitStatus(int cardID, int axisID)
        {
#if SIMULATION
            return true;
#else
            return Motion.GetNegativeLimitStatus(cardID, axisID);
#endif
        }

        protected virtual bool ChkMotionDoneStatus(int axis)
        {
#if SIMULATION
            return true;
#else
            return Motion.GetMotionDoneStatus(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID);
#endif
        }

        protected virtual double GetEncoderPosition(int axis)
        {
#if SIMULATION
            return 0;
#else
            return Motion.GetEncoderPosition(m_AxisModel.MotCfgs[axis].Axis.CardID, m_AxisModel.MotCfgs[axis].Axis.AxisID);
#endif
        }

        protected virtual bool SetZeroPosition(int cardID, int axisID)
        {
#if SIMULATION
            return true;
#else
            return Motion.SetZeroPosition(cardID, axisID);
#endif
        }

        protected virtual bool FindEdge(int cardID, int axisID, double speed)
        {
#if SIMULATION
            return true;
#else
            return Motion.FindEdge(cardID, axisID, speed);
#endif
        }

        protected virtual bool FindIndex(int cardID, int axisID, double speed)
        {
#if SIMULATION
            return true;
#elif GALIL
            return Motion.FindIndex(cardID, axisID, speed);
#else
            return true;
#endif
        }

        public void StopAllMotorPulse()
        {
            for (int i = 0; i < MotCfgs.Count; i++)
            {
                Motion.StopServo(m_AxisModel.MotCfgs[i].Axis.CardID, m_AxisModel.MotCfgs[i].Axis.AxisID);
            }
        }
        #endregion
        /// <summary>
        /// This methos shows a UI dialog box at center of screen, the calling thread/ Seq Module will stop and wait for button result from user.
        /// </summary>
        /// <param name="ErrorCode"></param>
        #region Error Management
        internal string RaiseError(int ErrorCode)
        {
            if (m_TestEventArg.RunMode == TestEventArg.Run_Mode.None)
            {
                return Error.RaiseError(ErrorCode, SeqName);
            }
            else
            {
                ErrorConfig errorConfig = ErrorConfig.Open(SysCfgs.SeqCfgRef[(int)SeqName].ErrLibPath, SysCfgs.SeqCfgRef[(int)SeqName].ErrLib);
                m_TestEventArg.RunMode = TestEventArg.Run_Mode.Stop;
                m_TestRunResult.result = false;
                m_TestRunResult.ErrMsg = errorConfig.ErrTable[ErrorCode].Cause;
                Publisher.GetEvent<TestRunResult>().Publish(m_TestRunResult);
                return string.Empty;
            }
        }

        internal string RaiseVerificationError(int ErrorCode)
        {
            if (m_TestEventArg.RunMode == TestEventArg.Run_Mode.None)
            {
               return Error.RaiseVerificationError(ErrorCode, SeqName);
            }
            else
            {
                ErrorConfig errorConfig = ErrorConfig.Open(SysCfgs.SeqCfgRef[(int)SeqName].ErrLibPath, SysCfgs.SeqCfgRef[(int)SeqName].ErrLib);
                m_TestEventArg.RunMode = TestEventArg.Run_Mode.Stop;
                m_TestRunResult.result = false;
                m_TestRunResult.ErrMsg = errorConfig.ErrTable[ErrorCode].Cause;
                Publisher.GetEvent<TestRunResult>().Publish(m_TestRunResult);
                return string.Empty;
            }
        }
        /// <summary>
        /// This method shows a threading UI dialog box at center of screen, while the calling thread/ Seq Module will continue executing.
        /// </summary>
        /// <param name="ErrorCode"></param>
        internal void RaiseWarning(int ErrorCode)
        {
            Error.RaiseError(ErrorCode, SeqName);
        }

        internal void RaiseVerificationWarning(int ErrorCode)
        {
            Error.RaiseVerificationError(ErrorCode, SeqName);
        }


        internal void CloseWarning(int ErrorCode)
        {
            Error.CloseWarning(ErrorCode, SeqName);
        }
        #endregion

        #region Exception Handling
        internal virtual StringBuilder GenerateExceptionMsg(Exception ex)
        {
            StringBuilder errMsg = new StringBuilder();

            string type = ex.GetType() == null ? "N/A" : ex.GetType().Name + "\n";
            string message = ex.Message == null ? "N/A" : ex.Message + "\n";
            string declaringType = "N/A";
            string targetSite = "N/A";

            if (ex.TargetSite != null)
            {
                targetSite = ex.TargetSite.Name + "\n";
                declaringType = ex.TargetSite.DeclaringType == null ? "N/A" + "\n" : ex.TargetSite.DeclaringType.Name + "\n";
            }
            string stackTrace = ex.StackTrace == null ? "N/A" + "\n" : ex.StackTrace + "\n";

            return errMsg.Append("Type : " + type +
                                 "Message : " + message +
                                 "Declaring Type : " + declaringType +
                                 "Target Site : " + targetSite +
                                 "Stack Trace : \n" + stackTrace);
        }
        #endregion
    }
}
