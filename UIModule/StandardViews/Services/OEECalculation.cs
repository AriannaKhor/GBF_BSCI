using ConfigManager;
using DBManager.Domains;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Media;

namespace UIModule.StandardViews.Services
{
    public class OEECalculation : BindableBase, IOEECalculation
    {
        #region Variable
        private double m_OEE;
        public double OEE
        {
            get { return m_OEE; }
            set { SetProperty(ref m_OEE, value); }
        }

        private double m_Availabiltiy;
        public double Availability
        {
            get { return m_Availabiltiy; }
            set { SetProperty(ref m_Availabiltiy, value); }
        }

        private double m_Performance;
        public double Performance
        {
            get { return m_Performance; }
            set { SetProperty(ref m_Performance, value); }
        }

        private double m_Quality;
        public double Quality
        {
            get { return m_Quality; }
            set { SetProperty(ref m_Quality, value); }
        }

        public double PlannedProductionTime
        {
            // Total equipment running time
            // In minutes
            get { return (double)Math.Round((24.0 / ShiftCount) * 60, 0); }
        }

        private int m_ShiftCount;
        public int ShiftCount
        {
            get { return m_ShiftCount; }
            set { SetProperty(ref m_ShiftCount, value); }
        }

        public double PlannedDowntime
        {
            // e.g. Changeovers
            // In minutes
            // Manufacturing process is scheduled for production and is not running because of a planned Setup, Make Ready, or Adjustment Event.
            get { return m_OEEConfig.OEERuntime.PlannedStops; }
            set
            {
                if (m_OEEConfig.OEERuntime.PlannedStops != value)
                {
                    m_OEEConfig.OEERuntime.PlannedStops = value;
                    RaisePropertyChanged(nameof(PlannedDowntime));
                    RaisePropertyChanged(nameof(RunTime));
                    SaveConfig();
                }
            }
        }

        public double UnplannedDowntime
        {
            // e.g. Breakdowns, 
            // In minutes
            get { return m_OEEConfig.OEERuntime.UnplannedStops; }
            set
            {
                if (m_OEEConfig.OEERuntime.UnplannedStops != value)
                {
                    m_OEEConfig.OEERuntime.UnplannedStops = value;
                    RaisePropertyChanged(nameof(UnplannedDowntime));
                    RaisePropertyChanged(nameof(RunTime));
                    SaveConfig();
                }
            }
        }

        public DateTime CurrentShiftBeginDateTime
        {
            get { return m_OEEConfig.OEERuntime.BeginDateTime; }
            set
            {
                m_OEEConfig.OEERuntime.BeginDateTime = value;
                RaisePropertyChanged(nameof(CurrentShiftBeginDateTime));
            }
        }

        public int CurrentShiftNo
        {
            get { return m_OEEConfig.OEERuntime.ShiftNo; }
            set
            {
                m_OEEConfig.OEERuntime.ShiftNo = value;
                RaisePropertyChanged(nameof(CurrentShiftNo));
            }
        }

        private TimeSpan m_ShiftStartTime;
        public TimeSpan ShiftStartTime
        {
            get { return m_ShiftStartTime; }
            set { SetProperty(ref m_ShiftStartTime, value); }
        }

        public double RunTime
        {
            get
            {
                double runtime = PlannedProductionTime - (UnplannedDowntime + PlannedDowntime);
                if (runtime < 0)
                {
                    return 0.0;
                }
                return runtime;
            }
        }

        private TimeSpan m_ShiftCountDownTimer;

        public TimeSpan ShiftCountDownTimer
        {
            get { return m_ShiftCountDownTimer; }
            set { SetProperty(ref m_ShiftCountDownTimer, value); }
        }



        private double m_IdealCycleTime;
        public double IdealCycleTime
        {
            get { return m_IdealCycleTime; }
            set { SetProperty(ref m_IdealCycleTime, value); }
        }

        public int TotalInput
        {
            get { return m_OEEConfig.OEERuntime.TotalInput; }
            set
            {
                if (m_OEEConfig.OEERuntime.TotalInput != value)
                {
                    m_OEEConfig.OEERuntime.TotalInput = value;
                    RaisePropertyChanged(nameof(TotalInput));
                    SaveConfig();
                }
            }
        }

        public int TotalOutput
        {
            get { return m_OEEConfig.OEERuntime.TotalOutput; }
            set
            {
                if (m_OEEConfig.OEERuntime.TotalOutput != value)
                {
                    m_OEEConfig.OEERuntime.TotalOutput = value;
                    RaisePropertyChanged(nameof(TotalOutput));
                    SaveConfig();
                }
            }
        }

        private double m_TotalRunTime;
        public double TotalRunTime
        {
            get { return m_TotalRunTime; }
            set { SetProperty(ref m_TotalRunTime, value); }
        }

        private double m_TotalHoursRunTime;
        public double TotalHoursRunTime
        {
            get { return m_TotalHoursRunTime; }
            set { SetProperty(ref m_TotalHoursRunTime, value); }
        }

        private int m_CurrentShift;
        public int CurrentShift
        {
            get { return m_CurrentShift; }
            set { SetProperty(ref m_CurrentShift, value); }
        }

        private DateTime m_curShifBegin;
        public DateTime curShifBegin
        {
            get { return m_curShifBegin; }
            set { SetProperty(ref m_curShifBegin, value); }
        }

        private Dictionary<int, int> m_ProductOutputDict;
        public Dictionary<int, int> ProductOutputDict
        {
            get { return m_ProductOutputDict; }
            set { SetProperty(ref m_ProductOutputDict, value); }
        }

        private Dictionary<int, int> m_ProductOutputShiftDict;

        public Dictionary<int, int> ProductOutputShiftDict
        {
            get { return m_ProductOutputShiftDict; }
            set { SetProperty(ref m_ProductOutputShiftDict, value); }
        }


        private DateTime m_StartTime;
        public DateTime StartTime
        {
            get { return m_StartTime; }
            set { SetProperty(ref m_StartTime, value); }
        }

        private string m_newStartTime;

        public string NewStartTime
        {
            get { return m_newStartTime; }
            set { SetProperty(ref m_newStartTime, value); }
        }


        private string m_Time;
        public string Time
        {
            get { return m_Time; }
            set { SetProperty(ref m_Time, value); }
        }

        private Stopwatch m_PlannedDownTimeStopWatch;

        private Stopwatch m_UnplannedDownTimeStopWatch;

        private List<string> m_PassTime = new List<string>();
        public List<string> PassTime
        {
            get { return m_PassTime; }
            set { SetProperty(ref m_PassTime, value); }
        }

        private List<SolidColorBrush> m_MachineStateColor = new List<SolidColorBrush>();
        public List<SolidColorBrush> MachineStateColor
        {
            get { return m_MachineStateColor; }
            set { SetProperty(ref m_MachineStateColor, value); }
        }

        private static object m_SyncOEE = new object();
        private CultureResources m_CultureResources;
        private OEEConfig m_OEEConfig;
        private SysInfo m_SysInfo;
        public Timer tmrOEEUpdate;
        public Timer tmrMachineStatus;
        private Stopwatch m_ShiftIntervalStopWatch;
        private readonly IEventAggregator m_EventAggregator;
        private TimeSpan m_OEERecordInterval;
        private double m_LastPlannedDownTime;
        private double m_LastUnplannedDownTime;
        public int MaxProductPerShift { get; set; }
        public int MaxProductMajorStep { get; set; }
        private bool machineRunning, machineError;
        int OldTotalOutput = 0;
        int OldShiftTotalOutput = 0;
        #endregion

        #region Constructor
        public OEECalculation(IEventAggregator eventAggregator, SysInfo sysInfo, OEEConfig oeeCfg, CultureResources cultureResources)
        {
            m_SysInfo = sysInfo;
            m_EventAggregator = eventAggregator;
            m_CultureResources = cultureResources;

            m_EventAggregator.GetEvent<MachineState>().Subscribe(OnMachineStateChange);
            m_EventAggregator.GetEvent<RefreshTotalInputOutput>().Subscribe(OEEInputOutput);
            ProductOutputDict = new Dictionary<int, int>();
            ProductOutputShiftDict = new Dictionary<int, int>();
            m_OEEConfig = oeeCfg;

            using (var db = new AppDBContext())
            {
                try
                {
                    var query = db.TblOEEConfig.Select(x => new { x.IdealCycleTime, x.ShiftCount, x.ShiftStartTime }).First();
                    IdealCycleTime = query.IdealCycleTime;
                    ShiftCount = query.ShiftCount;
                    ShiftStartTime = query.ShiftStartTime;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.Source);
                }
            }

            UpdateMachineStatus();

            tmrOEEUpdate = new Timer();
            tmrOEEUpdate.Interval = 1000;       // ms, 1 second
            tmrOEEUpdate.Elapsed += tmrOEEUpdate_Tick;
            tmrOEEUpdate.Start();

            tmrMachineStatus = new Timer();
            tmrMachineStatus.Interval = 30000;  // ms, 30 seconds
            tmrMachineStatus.Elapsed += tmrMachineStatus_Tick;
            tmrMachineStatus.Start();

            StartTime = DateTime.Now;
            NewStartTime = StartTime.ToString("yyyy-MM-dd HH:00:00");
            m_ShiftIntervalStopWatch = new Stopwatch();
            m_PlannedDownTimeStopWatch = new Stopwatch();
            m_UnplannedDownTimeStopWatch = new Stopwatch();

            DateTime curShifBegin = CalculateShiftInterval();
            DateTime curShifEnd = CurrentShiftBeginDateTime.Add(new TimeSpan(0, (int)PlannedProductionTime, 0));

            if (curShifBegin.CompareTo(CurrentShiftBeginDateTime) >= 0 && curShifBegin.CompareTo(curShifEnd) <= 0)
            {
                TotalInput = m_OEEConfig.OEERuntime.TotalInput;
                TotalOutput = m_OEEConfig.OEERuntime.TotalOutput;
                m_LastPlannedDownTime = m_OEEConfig.OEERuntime.PlannedStops;
                m_LastUnplannedDownTime = m_OEEConfig.OEERuntime.UnplannedStops;
            }
            else
            {
                TotalInput = 0;
                TotalOutput = 0;
                m_LastPlannedDownTime = 0.0;
                m_LastUnplannedDownTime = 0.0;
            }

            CurrentShiftBeginDateTime = curShifBegin;
            SaveConfig();

            // Stop Watch starts when application started - for Planned Stop / DownTime
            m_PlannedDownTimeStopWatch.Start();

            MaxProductPerShift = Convert.ToInt32(60 / IdealCycleTime * PlannedProductionTime);

            MaxProductMajorStep = MaxProductPerShift / 10;

            OnMachineStateChange(Global.MachineStatus);
        }
        #endregion

        #region Method
        private void StartPlannedDownTimeTimer()
        {
            if (!m_PlannedDownTimeStopWatch.IsRunning)
            {
                m_PlannedDownTimeStopWatch.Start();
            }
            m_UnplannedDownTimeStopWatch.Stop();
        }

        private void StartUnplannedDowntimeTimer()
        {
            if (!m_UnplannedDownTimeStopWatch.IsRunning)
            {
                m_UnplannedDownTimeStopWatch.Start();
            }
            m_UnplannedDownTimeStopWatch.Stop();
        }

        private void StopAllDowntimeTimers()
        {
            m_PlannedDownTimeStopWatch.Stop();
            m_UnplannedDownTimeStopWatch.Stop();
        }

        private void tmrOEEUpdate_Tick(object sender, EventArgs args)
        {
            try
            {
                StartOEECalculation();
            }
            catch (Exception ex)
            {
                tmrOEEUpdate.Stop();
                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Error, MsgText = $"{m_CultureResources.GetStringValue("OEE")} {m_CultureResources.GetStringValue("Update")} {m_CultureResources.GetStringValue("Error")} : {ex.Message}" });
            }
        }

        private void StartOEECalculation()
        {
            if (System.Threading.Monitor.TryEnter(m_SyncOEE))
            {
                try
                {
                    ShiftCountDownTimer = m_OEERecordInterval - m_ShiftIntervalStopWatch.Elapsed;
                    PlannedDowntime = m_LastPlannedDownTime + Math.Round(m_PlannedDownTimeStopWatch.Elapsed.TotalMinutes, 2);
                    UnplannedDowntime = m_LastUnplannedDownTime + Math.Round(m_UnplannedDownTimeStopWatch.Elapsed.TotalMinutes, 2);

                    Availability = Math.Round(CalculateAvailability() * 100.0, 2);
                    Performance = Math.Round(CalculatePerformance() * 100.0, 2);
                    Quality = Math.Round(CalculateQuality() * 100.0, 2);
                    OEE = Math.Round(Availability / 100 * (Performance / 100) * (Quality / 100) * 100.0, 2);

                    TotalRunTime = Math.Round(RunTime / 60, 2);
                    TotalHoursRunTime = Math.Round(PlannedProductionTime / 60, 2);

                    CurrentShift = CurrentShiftNo;

                    if (ShiftCountDownTimer.TotalSeconds <= 0)
                    {
                        // Store to Database
                        DateTime now = DateTime.Now;
                        using (var db = new AppDBContext())
                        {
                            db.TblOEE.Add(new TblOEE()
                            {
                                OEEDateTime = now,
                                ShiftNo = CurrentShiftNo,
                                Availability = Availability,
                                Performance = Performance,
                                Quality = Quality,
                                OEE = OEE,
                                PlannedProductionTime = (int)PlannedProductionTime,
                                PlannedDownTime = PlannedDowntime,
                                RunTime = RunTime,
                                UnplannedDownTime = UnplannedDowntime,
                                IdealCycleTime = IdealCycleTime,
                                TotalInput = TotalInput,
                                TotalOutput = TotalOutput,
                            });
                            db.SaveChanges();
                        }

                        // Calculate the next shift interval
                        CurrentShiftBeginDateTime = CalculateShiftInterval();
                        SaveConfig();

                        // Reset Planned DownTime stopwatch
                        if (m_PlannedDownTimeStopWatch.IsRunning)
                        {
                            // Machine is still idle, Reset the elapsed and keep stopwatch running
                            m_PlannedDownTimeStopWatch.Restart();
                        }
                        else
                        {
                            // Stopwatch is not running, reset the stopwatch
                            m_PlannedDownTimeStopWatch.Reset();
                        }

                        // Reset Unplanned DownTime stopwatch
                        if (m_UnplannedDownTimeStopWatch.IsRunning)
                        {
                            // Machine is still idle, Reset the elapsed and keep stopwatch running
                            m_UnplannedDownTimeStopWatch.Restart();
                        }
                        else
                        {
                            // Stopwatch is not running, reset the stopwatch
                            m_UnplannedDownTimeStopWatch.Reset();
                        }

                        m_LastPlannedDownTime = 0.0;
                        m_LastUnplannedDownTime = 0.0;

                        TotalInput = 0;
                        TotalOutput = 0;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    // Ensure the lock is released.
                    System.Threading.Monitor.Exit(m_SyncOEE);
                }
            }
        }

        public void SaveConfig()
        {
            lock (this)
            {
                m_OEEConfig.Save();
            }
        }

        private void tmrMachineStatus_Tick(object sender, EventArgs args)
        {
            try
            {
                UpdateMachineStatus();
            }
            catch (Exception ex)
            {
                tmrMachineStatus.Stop();
                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Error, MsgText = $"{m_CultureResources.GetStringValue("OEE")} {m_CultureResources.GetStringValue("MachineState")} {m_CultureResources.GetStringValue("Error")} : {ex.Message}" });
            }
        }

        private void UpdateMachineStatus()
        {
            try
            {
                if (PassTime.Count >= 60)
                {
                    PassTime.RemoveAt(0);
                    MachineStateColor.RemoveAt(0);
                }
                SolidColorBrush color = !machineRunning && !machineError ? Brushes.Yellow : machineRunning && !machineError ? Brushes.GreenYellow : Brushes.Red;
                Time = DateTime.Now.ToString("HH:mm:ss");
                PassTime.Add(Time);
                MachineStateColor.Add(color);
                m_EventAggregator.GetEvent<MachineStatusColor>().Publish(new MachineStatusColor { StatusColor = color, Time = Time });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void OEEInputOutput()
        {

            int TimeDiff = Convert.ToInt32(Math.Floor((DateTime.Now - Convert.ToDateTime(NewStartTime)).TotalHours));

            if (!ProductOutputDict.ContainsKey(TimeDiff))
            {
                OldTotalOutput += TimeDiff > 0 ? ProductOutputDict.Count > 0 ? ProductOutputDict.ContainsKey(TimeDiff - 1) ? ProductOutputDict[TimeDiff - 1] : 0 : 0 : 0;
                ProductOutputDict.Add(TimeDiff, TotalOutput - OldTotalOutput);
            }
            else
            {
                ProductOutputDict[TimeDiff] = TotalOutput - OldTotalOutput;
            }

            if (!ProductOutputShiftDict.ContainsKey(CurrentShiftNo))
            {
                OldShiftTotalOutput += ProductOutputShiftDict.ContainsKey(CurrentShiftNo - 1) ? ProductOutputShiftDict[CurrentShiftNo - 1] : 0;
                ProductOutputShiftDict.Add(CurrentShiftNo, TotalOutput - OldShiftTotalOutput);
            }
            else
            {
                ProductOutputShiftDict[CurrentShiftNo] = TotalOutput - OldShiftTotalOutput;
            }

            m_EventAggregator.GetEvent<RefreshOEE>().Publish(new RefreshOEE() { OEEStartTime = Convert.ToDateTime(NewStartTime), OutputDict = ProductOutputDict, OutputShiftDict = ProductOutputShiftDict });
        }

        private double CalculateAvailability()
        {
            return PlannedProductionTime > 0 ? RunTime / PlannedProductionTime : 0.0;
        }

        private double CalculatePerformance()
        {
            return RunTime > 0 ? (IdealCycleTime * TotalInput) / (RunTime * 60) : 0.0;
        }

        private double CalculateQuality()
        {
            return TotalInput > 0 ? (double)TotalOutput / (double)TotalInput : 0;
        }

        private DateTime CalculateShiftInterval()
        {
            DateTime now = DateTime.Now;
            int duration = (int)Math.Round((new TimeSpan(now.Hour, now.Minute, now.Second) - ShiftStartTime).TotalSeconds, 0);

            int remainder = 0;

            if (duration < 0)
            {
                // Use 24 hours to offset. (24 x 60 x 60)
                duration = 86400 - Math.Abs(duration);
            }

            // Find out remaining time before a shift period ends
            int quotient = Math.DivRem(duration, (int)PlannedProductionTime * 60, out remainder);
            CurrentShiftNo = quotient + 1;

            // Find out the shift begin date/time. PlannedStops/UnplannedStops will depend on it to decide whether to use them as last downtimes.
            m_OEERecordInterval = new TimeSpan(0, 0, ((int)PlannedProductionTime * 60) - remainder);
            // Also restart this stop watch.
            m_ShiftIntervalStopWatch.Restart();
            return now.Subtract(new TimeSpan(0, 0, remainder));
        }
        #endregion

        #region Event
        public void OnMachineStateChange(MachineStateType state)
        {
            switch (state)
            {
                case MachineStateType.Running:
                    machineRunning = true;
                    machineError = false;
                    // Activated by Start Button.
                    // Equipment is processing the unit
                    // Stop all Downtime timers.
                    StopAllDowntimeTimers();
                    break;

                case MachineStateType.Error:
                    machineRunning = false;
                    machineError = true;
                    // Unplanned DownTime - Breakdown.
                    StartUnplannedDowntimeTimer();
                    break;

                case MachineStateType.Idle:
                    machineRunning = false;
                    machineError = false;
                    break;
            }
        }
        #endregion
    }
}
