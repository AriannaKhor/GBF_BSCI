using ConfigManager;
using DataManager;
using GreatechApp.Core.Command;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Variable;
using IOManager;
using Prism.Commands;
using Prism.Events;
using Sequence.CoreSeq;
using Sequence.MachineSeq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using TCPIPManager;
using ThreadManager;

namespace Sequence
{
    public class DelegateSeq : IDelegateSeq
    {
        private object m_SyncInit = new object();
        private int InitCompletedCount;
        private int InitFailCount;
        private int EndLotCompletedCount;

        //private IBaseMotion m_BaseMotion;
        private IEventAggregator m_EventAggregator;
        private ITCPIP m_TCPIP;
        private IInsightVision   m_InsightVision;
        private ICodeReader m_CodeReader;
        private ISerialPort m_SerialPort;
        private IError m_Error;
        private SystemConfig m_SysConfig;
        private IBaseIO m_BaseIO;
        private IPollEngine IPollEngine;
        private IShowDialog ShowDialog;
        private CultureResources m_CultureResources;

        Dictionary<SQID, BaseClass> m_BaseSeq;

        public DelegateSeq(IEventAggregator eventAggregator, IError error, IBaseIO io, SystemConfig sysconfig, ITCPIP tcpip,IInsightVision insightvision,ICodeReader codereader, ISerialPort serialPort, IShowDialog showDialog, CultureResources cultureResources)
        {
            m_EventAggregator = eventAggregator;
            m_TCPIP = tcpip;
            m_InsightVision = insightvision;
            m_CodeReader = codereader;
            m_SerialPort = serialPort;
            //m_BaseMotion = motion;
            m_Error = error;
            m_SysConfig = sysconfig;
            m_BaseIO = io;
            m_CultureResources = cultureResources;
            ShowDialog = showDialog;

            var CoreSeqList = Assembly.LoadFrom(@"..\Execution\Sequence.dll").GetTypes()
                 .Where(t => (t.IsClass && t.BaseType.BaseType != null && t.Namespace == "Sequence.CoreSeq"))
                .ToList();


            // Bind SQID with the constructor class
            m_BaseSeq = new Dictionary<SQID, BaseClass>()
            {
                { SQID.CriticalScan, new CriticalScan() },
                { SQID.TopVisionSeq, new TopVisionSeq() },
                { SQID.CodeReaderSeq, new CodeReaderSeq() },
                { SQID.CountingScaleSeq, new CountingScaleSeq() },
            };

            CoreSeqNum = CoreSeqList.Count;
            MachineSeqNum = m_BaseSeq.Count - CoreSeqNum;

            IPollEngine = new PollEngine(m_BaseSeq.Count);
            CreateSeqInstance();
            IPollEngine.Start();

            /// Subscribe for TCP/IP
            m_BaseSeq[SQID.TopVisionSeq].SubscribeTCPMessage();
            /// Subscribe for SerialPort Event
            m_BaseSeq[SQID.CodeReaderSeq].SubscribeSerialMessage();

            m_EventAggregator.GetEvent<MachineOperation>().Subscribe(OnSeqInitCompleted, filter => filter.MachineOpr == MachineOperationType.InitDone);
            m_EventAggregator.GetEvent<InitOperation>().Subscribe(OnInit);
            m_EventAggregator.GetEvent<EndLotOperation>().Subscribe(OnEndLot);

            m_EventAggregator.GetEvent<MachineOperation>().Subscribe(OnSeqEndLotComplted, filter => filter.MachineOpr == MachineOperationType.EndLotComp);
            m_EventAggregator.GetEvent<MachineState>().Subscribe(OnMachineStateChange);
           // m_EventAggregator.GetEvent<CheckOperation>().Subscribe(OnCheckOperation);
        }

        private void CreateSeqInstance()
        {
            foreach (KeyValuePair<SQID, BaseClass> baseSeq in m_BaseSeq)
            {
                SQID seqName = baseSeq.Key;
                BaseClass seq = baseSeq.Value;

                seq.SeqName = seqName;
                seq.Publisher = m_EventAggregator;
                seq.Error = m_Error;
                seq.Error.SeqName = seqName;
                //seq.BaseMotion = m_BaseMotion;
                seq.BaseIO = m_BaseIO;
                seq.BaseData = new BaseData(seq.MarkerType, seqName, seq.SlotNum, m_EventAggregator);
                seq.SysCfgs = m_SysConfig;
                seq.TCPIP = m_TCPIP;
                seq.InsightVision = m_InsightVision;
                seq.CodeReader = m_CodeReader;
                seq.SerialPort = m_SerialPort;
                seq.ShowDialog = ShowDialog;
                seq.CultureResources = m_CultureResources;
                seq.SubscribeSeqEvent();
                seq.SubscribeTestRunEvent();
                seq.LoadMotionCfg();
                seq.LoadSeqCfg(m_SysConfig.SeqCfgRef[(int)seqName].Reference);
                seq.IOMapping();
                seq.LoadToolLifeCfg();

                seq.MachOperation = new DelegateCommand<string>(seq.MachineOperation);

                seq.TLOperation = new DelegateCommand(seq.ToolLifeOperation);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ApplicationCommands.OperationCommand.RegisterCommand(seq.MachOperation);
                });

                IPollEngine.Loop_Entry = new EventHandler(seq.OnRunSeq);
                IPollEngine.Module_Name = seq.SeqName.ToString();
                IPollEngine.Interval = 1;
                IPollEngine.Priority_Level = ThreadPriority.AboveNormal;
            }
        }


        public int TotalSeq { get { return m_BaseSeq.Count; } }
        public int CoreSeqNum { get; set; }
        public int MachineSeqNum { get; set; }

        private void OnInit()
        {
            InitFailCount = 0;
            InitCompletedCount = 0;
        }

        private void OnEndLot()
        {
            EndLotCompletedCount = 0;
        }

        private void OnSeqInitCompleted(SequenceEvent sequence)
        {
            lock (m_SyncInit)
            {
                if (!sequence.InitSuccess)
                {
                    InitFailCount++;
                }
                InitCompletedCount++;

                if (InitCompletedCount == MachineSeqNum)  // Exclude CriticalScan Module
                {
                    if (InitFailCount > 0)
                    {
                        m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.InitFail);
                    }
                    else
                    {
                        m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Init_Done);
                        Global.InitDone = true;
                    }
                    InitFailCount = 0;
                    InitCompletedCount = 0;
                }
            }
        }

        private void OnSeqEndLotComplted(SequenceEvent sequence)
        {
           m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Idle);
        }

        private void OnMachineStateChange(MachineStateType state)
        {
            switch (state)
            {
                case MachineStateType.CriticalAlarm:
                    foreach (KeyValuePair<SQID, BaseClass> baseSeq in m_BaseSeq)
                    {
                        //baseSeq.Value.StopAllMotorPulse();
                    }
                    break;

                case MachineStateType.Running:
                    foreach (KeyValuePair<SQID, BaseClass> baseSeq in m_BaseSeq)
                    {
                        baseSeq.Value.StartProduction();
                    }
                    break;
            }
        }

        private void OnCheckOperation(bool CheckOp)
        {
            foreach (KeyValuePair<SQID, BaseClass> baseSeq in m_BaseSeq)
            {
                baseSeq.Value.OperationChecking(CheckOp);
            }
            
        }

        public string GetSeqNum(SQID seqName)
        {
            return m_BaseSeq.Where(x => x.Key == seqName).FirstOrDefault().Value.SeqNum;
        }

        public int GetSlotNum(SQID seqName)
        {
            return m_BaseSeq.Where(x => x.Key == seqName).FirstOrDefault().Value.SlotNum;
        }

        public int GetStationNum(SQID seqName)
        {
            return m_BaseSeq.Where(x => x.Key == seqName).FirstOrDefault().Value.StationNum;
        }

        public bool GetIsAliveStatus(int sqid)
        {
            return IPollEngine.IsAlive(sqid);
        }

        public void BypassStation(SQID id, bool state)
        {
            m_BaseSeq[id].OnBypassStation(state);
        }

        IBaseData IDelegateSeq.GetBaseData(SQID seqName)
        {
            return m_BaseSeq.Where(x => x.Key == seqName).FirstOrDefault().Value.BaseData;
        }

        public Dictionary<int, string> GetMotCfg(SQID seqName)
        {
            return m_BaseSeq.Where(x => x.Key == seqName).FirstOrDefault().Value.MotCfgs;
        }

        string[] IDelegateSeq.GetPerfNames(SQID seqName)
        {
            string[] perfList = null;
            // Populate with Perf Enums
            var dynamicType = m_BaseSeq.Where(x => x.Key == seqName).FirstOrDefault().Value.GetType();
            perfList = FindPerfEnum(dynamicType);
            if (perfList != null)
            {
                return perfList;
            }
            // If you reach here, then this could be a child class of a sequence module.
            // Let's search from its base class instead.
            var baseType = m_BaseSeq.Where(x => x.Key == seqName).FirstOrDefault().Value.GetType().BaseType;
            return FindPerfEnum(baseType);
        }
        private string[] FindPerfEnum(Type t)
        {
            MemberInfo[] memberInfos = t.FindMembers(MemberTypes.NestedType, BindingFlags.Public,
                (m, ignore) => (m as Type).IsEnum, null);
            foreach (MemberInfo info in memberInfos)
            {
                var ty = info as Type;
                if (ty != null && ty.Name == "Perf")
                {
                    string[] names = Enum.GetNames(ty);
                    return names;
                }
            }
            return null;
        }
        int IDelegateSeq.GetProcCycleTime(SQID seqName, int perfID)
        {
            return m_BaseSeq.Where(x => x.Key == seqName).FirstOrDefault().Value.GetLastCycleTime(perfID);
        }
        double IDelegateSeq.GetAvgCycleTime(SQID seqName, int perfID)
        {
            return m_BaseSeq.Where(x => x.Key == seqName).FirstOrDefault().Value.GetAvgCycleTime(perfID);
        }

        int IDelegateSeq.GetMinCycleTime(SQID seqName, int perfID)
        {
            return m_BaseSeq.Where(x => x.Key == seqName).FirstOrDefault().Value.GetMinCycleTime(perfID);
        }

        int IDelegateSeq.GetMaxCycleTime(SQID seqName, int perfID)
        {
            return m_BaseSeq.Where(x => x.Key == seqName).FirstOrDefault().Value.GetMaxCycleTime(perfID);
        }
        int IDelegateSeq.TotalInput
        {
            get
            {
                return m_BaseSeq.Where(x => x.Key == SQID.TopVisionSeq).FirstOrDefault().Value.TotalInput;
            }
        }
        int IDelegateSeq.TotalOutput
        {
            // You must implement project specific code here.
            // Return output count value from one or more seq module.
            // Example:
            get
            {
                // the name of machine seq which provide total good unit
                return m_BaseSeq.Where(x => x.Key == SQID.SampleSeq5).FirstOrDefault().Value.TotalOutput;
            }
        }

        int IDelegateSeq.TotalFail
        {
            get
            {
                // the name of machine seq which provide total good unit
                return 0;
            }
        }

        #region Project Specific Properties
        //bool IDelegateSeq.WriteTrayDataSuccess
        //{
        //    set
        //    {
        //        OutputLinearPnP module = m_BaseSeq.Where(x => x.Key == SQID.OutputLinearPnP).FirstOrDefault().Value as OutputLinearPnP;
        //        Debug.Assert(module != null);
        //        module.WriteTrayDataSuccess = value;
        //    }
        //}
        #endregion

    }
}
