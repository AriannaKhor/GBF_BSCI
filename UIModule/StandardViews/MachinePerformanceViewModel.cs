using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Helpers;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using UIModule.MainPanel;

namespace UIModule.StandardViews
{
    public class MachinePerformanceViewModel : BaseUIViewModel
    {
        #region Variable
        private readonly LotInfo m_LotInfo;
        private readonly SysInfo m_SysInfo;
        private DispatcherTimer tmrSysPerfScan;

        private Tuple<TimeSpan, TimeSpan, TimeSpan> m_SysPerfRefreshRate;

        private string m_SelectedSysPerfRefreshRate;

        // Performance related variables
        private DateTime m_dtEquipStart;
        private DateTime m_dtLotStart;
        private TimeSpan m_tsLotElapsed;
        private DateTime m_dtLastEquipExe;
        private TimeSpan m_tsSumEquipExe;
        private TimeSpan m_tsEquipExe;
        private DateTime m_dtEquipNeedAssist;
        private int m_NumOfAssist;
        private TimeSpan m_tsTime2Assist;
        private long m_TotalTime2Assist;
        private long m_TotalTime2Repair;
        private DateTime m_dtEquipNeedRepair;
        private int m_NumOfRepair;
        private int m_NumOfFailures;
        private TimeSpan m_tsTime2Repair;
        private int m_NumOfStopPages;
        private List<double> m_MachineCycleTimeList;

        // Seq Module Peformance
        private string m_SelectedUoM;

        private bool isEquipExe;
        private bool isFirstLoad;
        private bool isFreshLot;
        private bool isLotEnded;

        private enum SysPerfItem : ushort
        {
            TotalExecutionTime,    // 0
            TotalExecutionDay,     // 1
            ProdStartTime,   // 2
            ProdElapsedTime, // 3
            ExeTime,         // 4
            DownTime,        // 5
            LotStartTime,    // 6
            LotElapsedTime,  // 7
            LotFinishTime,   // 8
            CycleTime,       // 9
            AvgCycleTime,    // 10
            MaxCycleTime,    // 11
            MinCycleTime,    // 12
            UPH,             // 13
            MTBA,            // 14
            MTTA,            // 15
            MTTR,            // 16
            MTBF,            // 17
            StopPages,       // 18
            OverallYield,    // 19 Yield calculation = Total good output / (standard output per hour X actual running hour)
            Throughput,      // 20
        }

        // System Performance
        public ObservableCollection<PerfKeyValuePair> SysPerfCollection { get; private set; }
        public bool IsSysPerfHighRate { get; set; }
        public bool IsSysPerfNormalRate { get; set; }
        public bool IsSysPerfSlowRate { get; set; }
        public string SelectedSysPerfRefreshRate
        {
            get { return m_SelectedSysPerfRefreshRate; }
            private set { SetProperty(ref m_SelectedSysPerfRefreshRate, value); }
        }

        public bool IsUoMInSecond { get; set; }
        public bool IsUoMInMilliSecond { get; set; }

        public string SelectedUoM
        {
            get { return m_SelectedUoM; }
            private set { SetProperty(ref m_SelectedUoM, value); }
        }

        private double m_UPH;
        public double UPH
        {
            get { return m_UPH; }
            private set { SetProperty(ref m_UPH, value); }
        }

        private double m_CycleTime;
        public double CycleTime
        {
            get { return m_CycleTime; }
            private set { SetProperty(ref m_CycleTime, value); }
        }

        public double MaxUPH { get; set; }
        public double MaxCycleTime { get; set; }

        private DelegateCommand<string> m_UpdateModulePerfRefreshRateCommand;

        public DelegateCommand<string> UpdateModulePerfRefreshRateCommand
        {
            get { return m_UpdateModulePerfRefreshRateCommand; }
            set { SetProperty(ref m_UpdateModulePerfRefreshRateCommand, value); }
        }
        private DelegateCommand m_ExportCommand;

        public DelegateCommand ExportCommand
        {
            get { return m_ExportCommand; }
            set { SetProperty(ref m_ExportCommand, value); }
        }
        private DelegateCommand m_ResetMeanTimeCommand;

        public DelegateCommand ResetMeanTimeCommand
        {
            get { return m_ResetMeanTimeCommand; }
            set { SetProperty(ref m_ResetMeanTimeCommand, value); }
        }
        private bool m_IsAllowExportData;
        public bool IsAllowExportData
        {
            get { return m_IsAllowExportData; }
            set { SetProperty(ref m_IsAllowExportData, value); }
        }
        private bool m_IsAllowReset;
        public bool IsAllowReset
        {
            get { return m_IsAllowReset; }
            set { SetProperty(ref m_IsAllowReset, value); }
        }
        private bool m_IsRecovery;

        public bool IsRecovery
        {
            get { return m_IsRecovery; }
            set { SetProperty(ref m_IsRecovery, value); }
        }

        private DispatcherTimer tmrMcPerfUpdate;
        #endregion

        #region Constructor
        public MachinePerformanceViewModel(LotInfo lotInfo, SysInfo sysInfo)
        {
            m_LotInfo = lotInfo;
            m_SysInfo = sysInfo;

            m_NumOfStopPages = 0;
            m_NumOfAssist = 0;
            m_TotalTime2Assist = 0;
            m_TotalTime2Repair = 0;

            m_EventAggregator.GetEvent<MachineState>().Subscribe(OnMachineStateChange);

            // Awake Critical Scan
            m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Idle);
            m_EventAggregator.GetEvent<MachineOperation>().Publish(new SequenceEvent()
            {
                TargetSeqName = SQID.CriticalScan,
                MachineOpr = MachineOperationType.BeginSeq,
            });

            IsAllowExportData = false;
            IsAllowReset = false;
            UpdateModulePerfRefreshRateCommand = new DelegateCommand<string>(UpdateModulePerfRefreshRate);
            ExportCommand = new DelegateCommand(Export);
            ResetMeanTimeCommand = new DelegateCommand(ResetMeanTime);
            m_SysPerfRefreshRate = Tuple.Create(new TimeSpan(0, 0, 0, 0, 500),
                new TimeSpan(0, 0, 0, 1, 0),
                new TimeSpan(0, 0, 0, 3, 0));

            // System Performance
            tmrSysPerfScan = new DispatcherTimer();
            tmrSysPerfScan.Interval = m_SysPerfRefreshRate.Item2;   // Normal rate
            tmrSysPerfScan.Tick += tmrSysPerfScan_Tick;
            IsSysPerfHighRate = false;
            IsSysPerfNormalRate = true;
            IsSysPerfSlowRate = false;
            SelectedSysPerfRefreshRate = GetStringTableValue("Normal");

            // Unit Of Measurement.
            IsUoMInSecond = false;
            IsUoMInMilliSecond = true;      // Default as ms
            SelectedUoM = GetStringTableValue("ms");   // Display as short hand.

            SysPerfCollection = new ObservableCollection<PerfKeyValuePair>();
            m_MachineCycleTimeList = new List<double>();

            #region Create elements in SysPerfCollection, ProdCntCollection and ProdInfoCollection
            for (int i = 0; i < Enum.GetNames(typeof(SysPerfItem)).Length; i++)
            {
                SysPerfCollection.Add(new PerfKeyValuePair());
            }

            #endregion

            #region Assign description to perf items
            SysPerfCollection[(int)SysPerfItem.ProdStartTime].Title = GetStringTableValue("StartTime") + " :";
            SysPerfCollection[(int)SysPerfItem.ProdElapsedTime].Title = GetStringTableValue("ElapsedTime") + " :";
            SysPerfCollection[(int)SysPerfItem.ExeTime].Title = GetStringTableValue("ExeTime") + " :";
            SysPerfCollection[(int)SysPerfItem.DownTime].Title = GetStringTableValue("DownTime") + " :";
            SysPerfCollection[(int)SysPerfItem.LotStartTime].Title = GetStringTableValue("LotStartTime") + " :";
            SysPerfCollection[(int)SysPerfItem.LotElapsedTime].Title = GetStringTableValue("LotElapsedTime") + " :";
            SysPerfCollection[(int)SysPerfItem.LotFinishTime].Title = GetStringTableValue("FinishTime") + " :";
            SysPerfCollection[(int)SysPerfItem.CycleTime].Title = GetStringTableValue("CycleTime") + " :";
            SysPerfCollection[(int)SysPerfItem.AvgCycleTime].Title = GetStringTableValue("AvgCycleTime") + " :";
            SysPerfCollection[(int)SysPerfItem.MaxCycleTime].Title = GetStringTableValue("MaxCycleTime") + " :";
            SysPerfCollection[(int)SysPerfItem.MinCycleTime].Title = GetStringTableValue("MinCycleTime") + " :";
            SysPerfCollection[(int)SysPerfItem.UPH].Title = GetStringTableValue("UPH") + " :";
            SysPerfCollection[(int)SysPerfItem.MTBA].Title = GetStringTableValue("MTBA") + " :";
            SysPerfCollection[(int)SysPerfItem.MTTA].Title = GetStringTableValue("MTTA") + " :";
            SysPerfCollection[(int)SysPerfItem.MTTR].Title = GetStringTableValue("MTTR") + " :";
            SysPerfCollection[(int)SysPerfItem.MTBF].Title = GetStringTableValue("MTBF") + " :";
            SysPerfCollection[(int)SysPerfItem.StopPages].Title = GetStringTableValue("Stoppages") + " :";
            SysPerfCollection[(int)SysPerfItem.StopPages].Value = m_NumOfStopPages.ToString();
            SysPerfCollection[(int)SysPerfItem.OverallYield].Title = GetStringTableValue("OverallYield") + " :";
            SysPerfCollection[(int)SysPerfItem.Throughput].Title = GetStringTableValue("Throughput") + " :";
            // Project: Add project specific information here.
            #endregion

            #region Assign value to perf items
            SysPerfCollection[(int)SysPerfItem.Throughput].Value = "-";
            SysPerfCollection[(int)SysPerfItem.OverallYield].Value = "-";
            #endregion

            isFirstLoad = true;
            isFreshLot = false;
            isLotEnded = false;
            MaxUPH = m_SystemConfig.General.IdealUPH;
            MaxCycleTime = m_SystemConfig.General.IdealCycleTime;

            tmrMcPerfUpdate = new DispatcherTimer();
            tmrMcPerfUpdate.Interval = new TimeSpan(0, 0, 1); // 1 second timer
            tmrMcPerfUpdate.Tick += tmrMcPerfUpdate_Tick;
            // This timer will run as long as the application is alive.
            tmrMcPerfUpdate.Start();

        }

        #endregion

        #region Event
        private void tmrSysPerfScan_Tick(object sender, EventArgs e)
        {
            try
            {
                RefreshSysPerf();
            }
            catch (Exception ex)
            {
                tmrSysPerfScan.Stop();
                m_ShowDialog.Show(DialogIcon.Stop, ex.ToString());
                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Error, MsgText = $"{GetStringTableValue("PerfScanTimerError")} : {ex.Message}" });
            }
        }

        private void tmrMcPerfUpdate_Tick(object sender, EventArgs arg)
        {
            m_EventAggregator.GetEvent<PerformanceCompact>().Publish(
                new PerformanceCompact
                {
                    OverallYield = SysPerfCollection[(int)SysPerfItem.OverallYield].Value,
                    Throughput = SysPerfCollection[(int)SysPerfItem.Throughput].Value,
                    StopPages = SysPerfCollection[(int)SysPerfItem.StopPages].Value,
                    StartTime = SysPerfCollection[(int)SysPerfItem.ProdStartTime].Value,
                    ElapsedTime = SysPerfCollection[(int)SysPerfItem.ProdElapsedTime].Value,
                    DownTime = SysPerfCollection[(int)SysPerfItem.DownTime].Value,
                    LotStartTime = SysPerfCollection[(int)SysPerfItem.LotStartTime].Value,
                    LotElapsedTime = SysPerfCollection[(int)SysPerfItem.LotElapsedTime].Value,
                    LotFinishTime = SysPerfCollection[(int)SysPerfItem.LotFinishTime].Value,
                    MTBA = SysPerfCollection[(int)SysPerfItem.MTBA].Value,
                    MTTA = SysPerfCollection[(int)SysPerfItem.MTTA].Value,
                    MTTR = SysPerfCollection[(int)SysPerfItem.MTTR].Value,
                    MTBF = SysPerfCollection[(int)SysPerfItem.MTBF].Value
                });
        }

        private void OnMachineStateChange(MachineStateType machineState)
        {
            lock (this)
            {
                switch (machineState)
                {
                    case MachineStateType.Initializing:
                        // Clear cycltime list on every init.
                        m_MachineCycleTimeList.Clear();
                        IsAllowReset = true;
                        if (!isFirstLoad)
                        {
                            tmrSysPerfScan.Start();
                        }
                        break;

                    case MachineStateType.Running:
                        // User click start button in toolbar view
                        // Start EquipExe tmr
                        IsAllowExportData = true;
                        IsAllowReset = false;
                        StartEquipExe();
                        if (isFirstLoad)
                        {
                            isFirstLoad = false;
                            // Record the start time
                            m_dtEquipStart = DateTime.Now;
                            SysPerfCollection[(int)SysPerfItem.ProdStartTime].Value =
                               m_dtEquipStart.ToString("d", DateTimeFormatInfo.InvariantInfo) + " | " +
                               m_dtEquipStart.ToString("T", DateTimeFormatInfo.InvariantInfo);
                        }
                        if (isFreshLot)
                        {
                            // At least one lot submitted
                            tmrSysPerfScan.Start();
                            isFreshLot = false;
                            // Beginning of new lot...
                            isLotEnded = false;
                            // Record Lot start time
                            m_dtLotStart = DateTime.Now;
                            SysPerfCollection[(int)SysPerfItem.LotStartTime].Value = m_dtLotStart.ToString("T",
                                DateTimeFormatInfo.InvariantInfo);
                            SysPerfCollection[(int)SysPerfItem.LotFinishTime].Value = "--:--:--";
                        }
                        if (IsRecovery)
                        {
                            // Record the repairing detail (MTTR & MTBR) after "Start" button pressed 
                            IsRecovery = false;
                            RecordRepair();
                        }
                        break;

                    case MachineStateType.Repairing:
                        if (Global.InitDone)
                        {
                            // Increase the NumOfAssist counter value.
                            m_NumOfAssist++;
                            // Increase the NumOfRepair counter value.
                            m_NumOfRepair++;
                            // Get the total time span from the time assistance was initially needed, 
                            // i.e when the equipment first enter into Down state
                            m_tsTime2Assist = DateTime.Now.Subtract(m_dtEquipNeedAssist);
                            // Accumulate the total time to assist.
                            m_TotalTime2Assist += m_tsTime2Assist.Ticks;
                            // Calculate & display the MTTA.
                            SysPerfCollection[(int)SysPerfItem.MTTA].Value = CalMeanTime(m_TotalTime2Assist, m_NumOfAssist);
                            // After the equipment has been assisted, it needed to be repaired.
                            // Record the time when it first needed to be repaired.
                            // This value will be used to calculate the MTTR value.
                            m_dtEquipNeedRepair = DateTime.Now;
                        }
                        IsRecovery = false;
                        break;

                    case MachineStateType.Recovery:
                        if (Global.InitDone)
                        {
                            IsRecovery = true;
                        }
                        break;

                    case MachineStateType.Ready:
                        // When user click the Start Lot button in Lot Entry form.
                        // Clear cycle time list - to restart the min/max/avg cycle computation.
                        m_MachineCycleTimeList.Clear();
                        // isFreshLot will be evaluated when StartButton is pressed to tell whether it is the start of a new lot
                        isFreshLot = true;
                        SysPerfCollection[(int)SysPerfItem.LotStartTime].Value = "--:--:--";
                        SysPerfCollection[(int)SysPerfItem.LotElapsedTime].Value = "--:--:--";
                        SysPerfCollection[(int)SysPerfItem.LotFinishTime].Value = "--:--:--";
                        break;

                    case MachineStateType.Error:
                    case MachineStateType.CriticalAlarm:
                        StopEquipExe();
                        // Record the time when equipment need assistance
                        // Will be used in MTTA calculation
                        m_dtEquipNeedAssist = DateTime.Now;
                        RecordStopPage();
                        break;

                    case MachineStateType.Lot_Ended:
                        StopEquipExe();
                        IsAllowReset = true;
                        isLotEnded = true;
                        tmrSysPerfScan.Stop();
                        RefreshSysPerf();
                        // Do this after all in-memory objects already refresh.
                        SysPerfCollection[(int)SysPerfItem.LotFinishTime].Value = DateTime.Now.ToString("T", DateTimeFormatInfo.InvariantInfo);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // Write information on screen to file.
                            WriteScreenInfoToFile();
                            // Write Lot Summary - this format may varies, depending on what customer want.
                            WriteLotSummary();
                        });
                        m_LotInfo.InitData();
                        break;

                    case MachineStateType.Stopped:
                        StopEquipExe();
                        IsAllowReset = true;
                        break;
                }
            }
        }

        #endregion

        #region Method
        private void RefreshSysPerf()
        {
            DateTime dtElapsed = DateTime.Now;
            // Calculate the elapsed time from the Start Time
            TimeSpan tsElapsed = dtElapsed.Subtract(m_dtEquipStart);
            // Update the Performance list view control
            SysPerfCollection[(int)SysPerfItem.ProdElapsedTime].Value =
                tsElapsed.Hours.ToString("00") + ":" +
                tsElapsed.Minutes.ToString("00") + ":" +
                tsElapsed.Seconds.ToString("00");

            if (!isLotEnded)
            {
                // Calculate the lot elapsed time from Lot Start Time
                m_tsLotElapsed = dtElapsed.Subtract(m_dtLotStart);
                SysPerfCollection[(int)SysPerfItem.LotElapsedTime].Value =
                    m_tsLotElapsed.Hours.ToString("00") + ":" +
                    m_tsLotElapsed.Minutes.ToString("00") + ":" +
                    m_tsLotElapsed.Seconds.ToString("00");
            }

            m_tsEquipExe = isEquipExe ? dtElapsed.Subtract(m_dtLastEquipExe).Add(m_tsSumEquipExe) : m_tsSumEquipExe;
            var sb = new StringBuilder();
            if (m_tsEquipExe.Days > 0)
            {
                SysPerfCollection[(int)SysPerfItem.TotalExecutionDay].Value = m_tsEquipExe.Days.ToString();
            }
            sb.Append(m_tsEquipExe.Hours.ToString("00") + ":" +
                      m_tsEquipExe.Minutes.ToString("00") + ":" +
                      m_tsEquipExe.Seconds.ToString("00"));
            SysPerfCollection[(int)SysPerfItem.TotalExecutionTime].Value = sb.ToString();
            SysPerfCollection[(int)SysPerfItem.ExeTime].Value = sb.ToString();

            // Update the MTBA value.
            SysPerfCollection[(int)SysPerfItem.MTBA].Value = CalMeanTime(m_tsEquipExe.Ticks, m_NumOfStopPages);

            // Calc down time
            TimeSpan tsDownTime = tsElapsed.Subtract(m_tsEquipExe);
            SysPerfCollection[(int)SysPerfItem.DownTime].Value = tsDownTime.Hours.ToString("00") + ":" +
                                                                tsDownTime.Minutes.ToString("00") + ":" +
                                                                tsDownTime.Seconds.ToString("00");

            //Convert the Cycle Time in Second.In display, it will be shown as ms.
            //One of the Machine Sequence will write the calculated cycle time
            //into this global variable.
            double cycleInSec = Math.Round(Global.CycleTime / 1000.0, 2);
            //0.2s - 1 unit
            //3600s(1 hour) - 1 * 3600 / 0.2 = 18000
            if (cycleInSec > 0 && !Global.SeqStop)
            {
                // Calculate UPH
                double uph = Math.Round(3600.0 / cycleInSec, 0);
                // Update to the UPH Item in Peformance list view control
                SysPerfCollection[(int)SysPerfItem.UPH].Value = uph.ToString();
                m_SysInfo.UPH = uph.ToString();
                UPH = uph;
                // Update to the Cycle Time Item in Peformance list view control
                SysPerfCollection[(int)SysPerfItem.CycleTime].Value = cycleInSec + " " + GetStringTableValue("s");
                m_SysInfo.CycleTime = SysPerfCollection[(int)SysPerfItem.CycleTime].Value;
                CycleTime = cycleInSec;
                // Add cycle time to a list so that we can compute Max, Min, Average Machine Cycle Time
                // The list will be cleared on machine init & New Lot.
                m_MachineCycleTimeList.Add(cycleInSec);
                SysPerfCollection[(int)SysPerfItem.AvgCycleTime].Value = Math.Round(m_MachineCycleTimeList.Average(), 2) + " " + GetStringTableValue("s");
                SysPerfCollection[(int)SysPerfItem.MaxCycleTime].Value = Math.Round(m_MachineCycleTimeList.Max(), 2) + " " + GetStringTableValue("s");
                SysPerfCollection[(int)SysPerfItem.MinCycleTime].Value = Math.Round(m_MachineCycleTimeList.Min(), 2) + " " + GetStringTableValue("s");
                m_EventAggregator.GetEvent<PerformanceEntity>().Publish(new PerformanceEntity { UPH = UPH, CycleTime = CycleTime });
            }

            // Update Pass / Fail Count
            m_SysInfo.TotalPass = m_DelegateSeq.TotalOutput.ToString();
            m_SysInfo.TotalFail = m_DelegateSeq.TotalFail.ToString();

            Global.TotalInput = m_DelegateSeq.TotalInput;
            Global.TotalOutput = m_DelegateSeq.TotalOutput;

            m_EventAggregator.GetEvent<RefreshTotalInputOutput>().Publish();

            // Compute Overall Yield
            if (m_DelegateSeq.TotalInput > 0)
            {
                int totalIncomingMaterial = m_DelegateSeq.TotalInput;
                SysPerfCollection[(int)SysPerfItem.OverallYield].Value = Math.Round(((double)m_DelegateSeq.TotalOutput / (double)totalIncomingMaterial) * 100.0, 2) + " %";
            }
            // Compute Throughput
            if (m_DelegateSeq.TotalOutput > 0 && m_tsLotElapsed.TotalHours > 0)
            {
                // TODO: If TotalGoodUnitProduced is reset for every new lot, throughput = good unit produced / Lot Elapsed Time
                // TODO: else, throughput = good unit produced / machine elapsed time
                double throughPut = (double)m_DelegateSeq.TotalOutput / m_tsLotElapsed.TotalHours;
                // NOTE: Throughput is projected unit output per hour basis
                SysPerfCollection[(int)SysPerfItem.Throughput].Value = Math.Round(throughPut, 2) + GetStringTableValue("UnitsHour");
            }
        }
        private string CalMeanTime(long totalTime, int cnt)
        {
            long avgTime = 0;
            if (cnt > 0)
            {
                // Get the average - totalTime / cnt
                avgTime = (long)Math.Round(totalTime / (double)cnt, 0);
            }
            else
            {
                // Mainly for MTBA - when equipment runs initially without any stoppages
                avgTime = totalTime;
            }
            TimeSpan tsMeanTime = new TimeSpan(avgTime);
            return (tsMeanTime.Hours.ToString("00") + ":" +
                tsMeanTime.Minutes.ToString("00") + ":" +
                tsMeanTime.Seconds.ToString("00"));
        }
        public void UpdateModulePerfRefreshRate(string rate)
        {
            // Set new Refresh Rate selected by user.
            if (rate == "High")
            {
                SelectedSysPerfRefreshRate = GetStringTableValue("High");
                tmrSysPerfScan.Interval = m_SysPerfRefreshRate.Item1;
            }
            else if (rate == "Normal")
            {
                SelectedSysPerfRefreshRate = GetStringTableValue("Normal");
                tmrSysPerfScan.Interval = m_SysPerfRefreshRate.Item2;
            }
            else if (rate == "Slow")
            {
                SelectedSysPerfRefreshRate = GetStringTableValue("Slow");
                tmrSysPerfScan.Interval = m_SysPerfRefreshRate.Item3;
            }
        }
        public void UpdateUoM()
        {
            // For Seq Module Performance (eithier s or ms only).
            SelectedUoM = IsUoMInSecond ? GetStringTableValue("s") : GetStringTableValue("ms");
        }

        public void Export()
        {
            WriteLotSummary();
        }
        private void WriteScreenInfoToFile()
        {
            try
            {
                // As a standard, we will write all on screen info to a file
                // Build the directory on a daily basis.
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                string filePath = $"{m_SystemConfig.FolderPath.AnalysisLog}Performance\\{date}\\";
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);
                string datetime = DateTime.Now.ToString("yyyy-MM-dd HHmmss");

                string fileName = $"{filePath}{Global.LotInitialBatchNo}.log";
                using (StreamWriter logWriter = new StreamWriter(fileName, true))
                {
                    StringBuilder screenData = new StringBuilder();

                    #region Header Formats
                    // Production Info
                    const string prodInfoColFormat = "{0, -25}{1, -15}";
                    object[] prodInfoColHeaderTitle = {
                                                   GetStringTableValue("Item"),
                                                   GetStringTableValue("Data"),
                                              };
                    string prodInfoColHeader = string.Format(prodInfoColFormat, prodInfoColHeaderTitle);

                    // Production Counter
                    const string prodCntColFormat = "{0, -25}{1, -15}";
                    object[] prodCntColHeaderTitle = {
                                                   GetStringTableValue("Item"),
                                                   GetStringTableValue("Data"),
                                             };
                    string prodCntColHeader = string.Format(prodCntColFormat, prodCntColHeaderTitle);

                    // System Performance
                    const string sysPerfColFormat = "{0, -25}{1, -15}";
                    object[] sysPerfColHeaderTitle = {
                                                   GetStringTableValue("Title"),
                                                   GetStringTableValue("Data"),
                                             };
                    string sysPerfColHeader = string.Format(sysPerfColFormat, sysPerfColHeaderTitle);

                    // Module Performance
                    const string modPerfColFormat = "{0, -30}{1, -15}";
                    object[] modPerfColHeaderTitle = {
                                                   GetStringTableValue("Title"),
                                                   GetStringTableValue("Data"),
                                             };
                    string modPerfColHeader = string.Format(modPerfColFormat, modPerfColHeaderTitle);

                    #endregion

                    screenData.Append(FileHelper.Pad(GetStringTableValue("PerfSummary"), 100, '*')).AppendLine();
                    screenData.AppendLine();

                    #region System Performance
                    screenData.Append(FileHelper.Pad(GetStringTableValue("SystemPerf"), 40, '-')).AppendLine();
                    screenData.Append(sysPerfColHeader).AppendLine();
                    screenData.Append(FileHelper.Pad(string.Empty, 40, '-')).AppendLine();
                    for (int i = 0; i < Enum.GetNames(typeof(SysPerfItem)).Length; i++)
                    {
                        SysPerfItem item = (SysPerfItem)i;

                        string sysPerf = string.Format(sysPerfColFormat, new string[] { GetStringTableValue(item.ToString()), SysPerfCollection[i].Value });
                        screenData.Append(sysPerf).AppendLine();
                    }
                    screenData.AppendLine();

                    #endregion

                    screenData.Append(FileHelper.Pad(GetStringTableValue("EndOfSummary"), 40, '-')).AppendLine();
                    logWriter.Write(screenData);
                    logWriter.Close();

                    string path = Directory.GetCurrentDirectory();
                    Process.Start(fileName, path);
                }

                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("SaveLotPerf")} : {Global.LotInitialBatchNo}, {GetStringTableValue("Path")} : {fileName}" });
            }
            catch (Exception ex)
            {
                m_ShowDialog.Show(DialogIcon.Stop, ex.ToString());
                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Error, MsgText = $"{GetStringTableValue("SaveLotPerf")} {GetStringTableValue("Error")} : {ex.Message}" });
            }
        }

        private void WriteLotSummary()
        {
            try
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                string filePath = $"{m_SystemConfig.FolderPath.AnalysisLog}Lot Summary\\{date}\\";
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);
                string datetime = DateTime.Now.ToString("yyyy-MM-dd HHmmss");

                string fileName = $"{filePath}{Global.LotInitialBatchNo}.log";
                using (StreamWriter logWriter = new StreamWriter(fileName, true))
                {
                    StringBuilder lotData = new StringBuilder();
                    lotData.Append(FileHelper.Pad(GetStringTableValue("LotSummary"), 100, '*')).AppendLine();
                    lotData.AppendLine();
                    lotData.Append(FileHelper.Pad(GetStringTableValue("LotNo"), 16, ' ') + ": " + Global.LotInitialBatchNo).AppendLine();
                    lotData.Append(FileHelper.Pad(GetStringTableValue("LotStartTime"), 16, ' ') + ": " + SysPerfCollection[(int)SysPerfItem.LotStartTime].Value).AppendLine();
                    lotData.Append(FileHelper.Pad(GetStringTableValue("LotElapsedTime"), 16, ' ') + ": " + SysPerfCollection[(int)SysPerfItem.LotElapsedTime].Value).AppendLine();
                    lotData.Append(FileHelper.Pad(GetStringTableValue("LotFinishTime"), 16, ' ') + ": " + SysPerfCollection[(int)SysPerfItem.LotFinishTime].Value).AppendLine();
                    lotData.Append(FileHelper.Pad(GetStringTableValue("AvgCycleTime"), 16, ' ') + ": " + SysPerfCollection[(int)SysPerfItem.AvgCycleTime].Value).AppendLine();
                    lotData.Append(FileHelper.Pad(GetStringTableValue("UPH"), 16, ' ') + ": " + SysPerfCollection[(int)SysPerfItem.UPH].Value).AppendLine();
                    lotData.Append(FileHelper.Pad(GetStringTableValue("Throughput"), 16, ' ') + ": " + SysPerfCollection[(int)SysPerfItem.Throughput].Value).AppendLine();
                    lotData.Append(FileHelper.Pad(GetStringTableValue("Yield"), 16, ' ') + ": " + SysPerfCollection[(int)SysPerfItem.OverallYield].Value).AppendLine();

                    #region Add project specific code here

                    #endregion

                    lotData.Append(FileHelper.Pad(GetStringTableValue("EndOfSummary"), 40, '-')).AppendLine();
                    logWriter.Write(lotData);
                    logWriter.Close();

                    string path = Directory.GetCurrentDirectory();
                    Process.Start(fileName, path);
                }

                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("WriteLotSummary")}: {Global.LotInitialBatchNo}, {GetStringTableValue("Path")} : {fileName}" });
            }
            catch (Exception ex)
            {
                m_ShowDialog.Show(DialogIcon.Stop, ex.ToString());
                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Error, MsgText = $"{GetStringTableValue("WriteLotSummary")} {GetStringTableValue("Error")}: {ex.Message}" });
            }
        }

        private void StartEquipExe()
        {
            if (!isEquipExe)
            {
                m_dtLastEquipExe = DateTime.Now;
                isEquipExe = true;
            }
        }

        private void StopEquipExe()
        {
            // Since this function can be called in from several case, we
            // only add execution time when the machine state is change from executing to stopped
            if (!isFirstLoad && isEquipExe)
            {
                // Reset flag to indicate execution stopped
                isEquipExe = false;
                m_tsSumEquipExe = m_tsSumEquipExe.Add(DateTime.Now.Subtract(m_dtLastEquipExe));
            }
        }

        private void RecordRepair()
        {
            // Called by "start" button after repairing the error 
            // Get the total time span from the time repair was initially needed
            m_tsTime2Repair = DateTime.Now.Subtract(m_dtEquipNeedRepair);
            // Accumulate the total time to repair.
            m_TotalTime2Repair += m_tsTime2Repair.Ticks;
            // Calculate & display the MTTR.
            SysPerfCollection[(int)SysPerfItem.MTTR].Value = CalMeanTime(m_TotalTime2Repair, m_NumOfRepair);
            if (TimeSpan.FromTicks(m_tsTime2Repair.Ticks) > TimeSpan.FromMinutes(m_SystemConfig.General.DurationToFailure))
            {
                m_NumOfFailures++;
                SysPerfCollection[(int)SysPerfItem.MTBF].Value = CalMeanTime(m_tsEquipExe.Ticks, m_NumOfFailures);
            }
        }

        private void RecordStopPage()
        {
            // Source: Error machine state - already filter IsStoppage.
            m_NumOfStopPages++;
            SysPerfCollection[(int)SysPerfItem.StopPages].Value = m_NumOfStopPages.ToString();
        }

        private void ResetMeanTime()
        {
            try
            {
                m_NumOfStopPages = 0;
                m_NumOfAssist = 0;
                m_TotalTime2Assist = 0;
                m_NumOfRepair = 0;
                m_NumOfFailures = 0;
                m_TotalTime2Repair = 0;
                SysPerfCollection[(int)SysPerfItem.MTBA].Value = "--:--:--";
                SysPerfCollection[(int)SysPerfItem.MTTA].Value = "--:--:--";
                SysPerfCollection[(int)SysPerfItem.MTTR].Value = "--:--:--";
                SysPerfCollection[(int)SysPerfItem.MTBF].Value = "--:--:--";
                SysPerfCollection[(int)SysPerfItem.StopPages].Value = m_NumOfStopPages.ToString();

                // Set the new MTBA start time
                m_tsSumEquipExe = new TimeSpan(0, 0, 0, 0, 0);
                // Set the EquipStartTime as EquipExe time has been reseted as well
                // NOTE: DownTime = Elapsed Time - EquipExe Time
                m_dtEquipStart = DateTime.Now;
                SysPerfCollection[(int)SysPerfItem.ProdStartTime].Value = m_dtEquipStart.ToString("T", DateTimeFormatInfo.InvariantInfo);
                SysPerfCollection[(int)SysPerfItem.ExeTime].Value = "--:--:--";
            }
            catch (Exception ex)
            {
                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Error, MsgText = $"{GetStringTableValue("FailResetMeanTime")} : {ex.Message}" });
                m_ShowDialog.Show(DialogIcon.Stop, ex.ToString());
            }
        }
        #endregion
    }
}

