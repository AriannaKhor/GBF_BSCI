using DBManager.Domains;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using UIModule.MainPanel;
using UIModule.StandardViews.Services;

namespace UIModule.StandardViews
{
    public class OEELiveViewModel : BaseUIViewModel
    {
        #region Variable
        private string m_TabPageHeader;
        public string TabPageHeader
        {
            get { return m_TabPageHeader; }
            set { SetProperty(ref m_TabPageHeader, value); }
        }

        #region Performance Collection
        public SeriesCollection SeriesCollection { get; set; }
        public ChartValues<double> xLabels = new ChartValues<double>();
        public ChartValues<double> x1Labels = new ChartValues<double>();
        public List<string> yLabels = new List<string>();
        public string[] Labels { get; set; }

        public SeriesCollection PerformanceShiftCollection { get; set; }
        public ChartValues<double> pxLabels = new ChartValues<double>();
        public ChartValues<double> px1Labels = new ChartValues<double>();
        public List<string> pyLabels = new List<string>();
        public string[] pLabels { get; set; } 

        public int OldTotalOutput { get; set; }
        #endregion

        #region Machine Status Collection
        public SeriesCollection MachineStatusCollection { get; set; }
        private ColumnSeries m_MachineColumn = new ColumnSeries();
        public ColumnSeries MachineColumn 
        { 
            get { return m_MachineColumn; }
            set { SetProperty(ref m_MachineColumn, value); }
        }
        private ObservableCollection<string> m_Time = new ObservableCollection<string>();
        public ObservableCollection<string> Time 
        {
            get { return m_Time; }
            set { SetProperty(ref m_Time, value); } 
        }
        private List<SolidColorBrush> m_MachineStateColor;
        public List<SolidColorBrush> MachineStateColor
        {
            get { return m_MachineStateColor; }
            set { SetProperty(ref m_MachineStateColor, value); }
        }
        #endregion

        #region Top 5 Downtime Collection
        public SeriesCollection TopFiveDowntimeCollection { get; set; }
        public ChartValues<double> xAmount;
        public List<string> yType;
        public string[] Type { get; set; }
        #endregion

        private TimeSpan m_ShiftCountDownTimer;
        public TimeSpan ShiftCountDownTimer
        {
            get { return m_ShiftCountDownTimer; }
            set { SetProperty(ref m_ShiftCountDownTimer, value); }
        }

        private double m_Availability;
        public double Availability
        {
            get { return m_Availability; }
            set { SetProperty(ref m_Availability, value); }
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

        #region Run Time
        private int m_TotalInput;
        public int TotalInput
        {
            get { return m_TotalInput; }
            set { SetProperty(ref m_TotalInput, value); }
        }

        private int m_TotalOutput;
        public int TotalOutput
        {
            get { return m_TotalOutput; }
            set { SetProperty(ref m_TotalOutput, value); }
        }

        private double m_PlannedDowntime;
        public double PlannedDowntime
        {
            get { return m_PlannedDowntime; }
            set { SetProperty(ref m_PlannedDowntime, value); }
        }

        private double m_UnplannedDowntime;
        public double UnplannedDowntime
        {
            get { return m_UnplannedDowntime; }
            set { SetProperty(ref m_UnplannedDowntime, value); }
        }

        private DateTime m_CurrentShiftBeginDateTime;
        public DateTime CurrentShiftBeginDateTime
        {
            get { return m_CurrentShiftBeginDateTime; }
            set { SetProperty(ref m_CurrentShiftBeginDateTime,value); }
        }

        private int m_CurrentShift;
        public int CurrentShift
        {
            get { return m_CurrentShift; }
            set
            {
                SetProperty(ref m_CurrentShift, value);
                RaisePropertyChanged(nameof(CurrentShift));
            }
        }
        #endregion

        private double m_OEE;
        public double OEE
        {
            get { return m_OEE; }
            set { SetProperty(ref m_OEE, value); }
        }

        #region OEE Overview
        private double m_TotalRunTIme;
        public double TotalRunTime
        {
            get { return m_TotalRunTIme; }
            set { SetProperty(ref m_TotalRunTIme, value); }
        }

        private double m_TotalHoursRunTime;
        public double TotalHoursRunTime
        {
            get { return m_TotalHoursRunTime; }
            set { SetProperty(ref m_TotalHoursRunTime, value); }
        }

        #endregion

        private TimeSpan m_ShiftStartTime;
        public TimeSpan ShiftStartTime
        {
            get { return m_ShiftStartTime; }
            private set
            {
                SetProperty(ref m_ShiftStartTime, value);
                RaisePropertyChanged(nameof(ShiftStartTime));
            }
        }

        private int m_ShiftCount;
        public int ShiftCount
        {
            get { return m_ShiftCount; }
            set
            {
                SetProperty(ref m_ShiftCount, value);
                RaisePropertyChanged(nameof(ShiftCount));
                RaisePropertyChanged(nameof(PlannedProductionTime));
                RaisePropertyChanged(nameof(RunTime));
            }
        }

        private double  m_PlannedProductionTime;
        public double PlannedProductionTime
        { 
            get { return m_PlannedProductionTime; }
            set { m_PlannedProductionTime = value; }
        }

        private double m_RunTime;
        public double RunTime
        {
            get { return m_RunTime; }
            set { m_RunTime = value; }
        }

        private DateTime m_ActualTimeShiftStart;

        public DateTime ActualTimeShiftStart
        {
            get { return m_ActualTimeShiftStart; }
            set { SetProperty(ref m_ActualTimeShiftStart, value); }
        }

        private OEEDataUpdate m_oEEUpdata;
        public OEEDataUpdate OEEUpdate
        {
            get { return m_oEEUpdata; }
            set { SetProperty(ref m_oEEUpdata, value); }
        }

        private int m_MaxProductPerShift;
        public int MaxProductPerShift
        {
            get { return m_MaxProductPerShift; }
            set { SetProperty(ref m_MaxProductPerShift, value); }
        }

        private int m_MaxProductMajorStep;
        public int MaxProductMajorStep
        {
            get { return m_MaxProductMajorStep; }
            set { SetProperty(ref m_MaxProductMajorStep, value); }
        }

        private static object m_SyncOEE = new object();
        public DelegateCommand RefreshOEEData { get; private set; }
        public DelegateCommand ExportOEEData { get; private set; }
        private DispatcherTimer tmrOEEUpdate;
        OEECalculation m_OEECalculation;
        public DateTime StartTime;
        #endregion

        #region Constructor
        public OEELiveViewModel(OEECalculation oEECalculation)
        {
            m_OEECalculation = oEECalculation;

            TabPageHeader = GetStringTableValue("OEEView");

            m_EventAggregator.GetEvent<RefreshOEE>().Subscribe(UpdatePerformanceChart);
            m_EventAggregator.GetEvent<CultureChanged>().Subscribe(OnCultureChanged);

            tmrOEEUpdate = new DispatcherTimer();
            tmrOEEUpdate.Interval = new TimeSpan(0, 0, 1); // 1 second timer
            tmrOEEUpdate.Tick += tmrOEEUpdate_Tick;
            // This timer will run as long as the application is alive.
            tmrOEEUpdate.Start();

            #region Performance Chart
            StartTime = DateTime.Now;
            SeriesCollection = new SeriesCollection();
            xLabels = new ChartValues<double>();
            yLabels = new List<string>();
            x1Labels = new ChartValues<double>();

            PerformanceShiftCollection = new SeriesCollection();
            pxLabels = new ChartValues<double>();
            px1Labels = new ChartValues<double>();
            pyLabels = new List<string>();

            OldTotalOutput = 0;
            #endregion

            #region Machine Status Chart
            MachineStatusCollection = new SeriesCollection();
            MachineStateColor = new List<SolidColorBrush>();
            Time = new ObservableCollection<string>();
            MachineColumn.DataLabels = false;
            MachineColumn.MaxColumnWidth = double.PositiveInfinity;
            MachineColumn.Values = new ChartValues<double>() { 1 };

            for (int i = 0; i < m_OEECalculation.PassTime.Count; i++)
            {
                Time.Add(m_OEECalculation.PassTime[i]);
                MachineStateColor.Add(m_OEECalculation.MachineStateColor[i]);
            }
            RaisePropertyChanged(nameof(Time));
            RaisePropertyChanged(nameof(MachineStateColor));

            CartesianMapper<double> mapper = Mappers.Xy<double>()
                                                    .X((xvalue, yvalue) => yvalue)
                                                    .Y(xvalue => xvalue)
                                                    .Fill((value, index) => MachineStateColor[index]);
            MachineColumn.Configuration = mapper;
            MachineStatusCollection.Add(MachineColumn);
            RaisePropertyChanged(nameof(MachineStatusCollection));
            m_EventAggregator.GetEvent<MachineStatusColor>().Subscribe(UpdateMachineStatusChart);
            #endregion

            LoadDownTimeReason();

            ActualTimeShiftStart = m_OEECalculation.StartTime;
        }
        #endregion

        public void Restart()
        {
            if (!tmrOEEUpdate.IsEnabled)
            {
                tmrOEEUpdate.Start();
            }

            m_EventAggregator.GetEvent<RefreshOEE>().Unsubscribe(UpdatePerformanceChart);
            m_EventAggregator.GetEvent<CultureChanged>().Unsubscribe(OnCultureChanged);
            m_EventAggregator.GetEvent<MachineStatusColor>().Unsubscribe(UpdateMachineStatusChart);
            m_EventAggregator.GetEvent<RefreshOEE>().Subscribe(UpdatePerformanceChart);
            m_EventAggregator.GetEvent<CultureChanged>().Subscribe(OnCultureChanged);
            m_EventAggregator.GetEvent<MachineStatusColor>().Subscribe(UpdateMachineStatusChart);
        }

        public void Reset()
        {
            if (tmrOEEUpdate.IsEnabled)
            {
                tmrOEEUpdate.Stop();
            }
            m_EventAggregator.GetEvent<RefreshOEE>().Unsubscribe(UpdatePerformanceChart);
            m_EventAggregator.GetEvent<CultureChanged>().Unsubscribe(OnCultureChanged);
            m_EventAggregator.GetEvent<MachineStatusColor>().Unsubscribe(UpdateMachineStatusChart);
        }

        #region Method
        private void tmrOEEUpdate_Tick(object sender, EventArgs arg)
        {
            if (Monitor.TryEnter(m_SyncOEE))
            {
                try
                {
                    PlannedProductionTime = m_OEECalculation.PlannedProductionTime;
                    RunTime = m_OEECalculation.RunTime;
                    ShiftCount = m_OEECalculation.ShiftCount;
                    ShiftStartTime = m_OEECalculation.ShiftStartTime;
                    // Shift period CountDown
                    ShiftCountDownTimer = m_OEECalculation.ShiftCountDownTimer;
                    PlannedDowntime = m_OEECalculation.PlannedDowntime;
                    UnplannedDowntime = m_OEECalculation.UnplannedDowntime;

                    // OEE Chart
                    Availability = m_OEECalculation.Availability;
                    Performance = m_OEECalculation.Performance;
                    Quality = m_OEECalculation.Quality;
                    OEE = m_OEECalculation.OEE;

                    TotalRunTime = m_OEECalculation.TotalRunTime;
                    TotalHoursRunTime = m_OEECalculation.TotalHoursRunTime;

                    CurrentShift = m_OEECalculation.CurrentShiftNo;
                }
                catch (Exception ex)
                {
                    tmrOEEUpdate.Stop();
                    m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Error, MsgText = $"{GetStringTableValue("OEEUpdateError")} : {ex.Message}" });
                }
                finally
                {
                    // Ensure the lock is released.
                    Monitor.Exit(m_SyncOEE);
                }
            }
        }

        public void UpdatePerformanceChart(RefreshOEE data)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var top10Data = data.OutputDict.OrderByDescending(x => x.Key).Take(10).OrderBy(x => x.Key);
                var top10ShiftData = data.OutputShiftDict.OrderByDescending(x => x.Key).Take(10).OrderBy(x => x.Key);

                if (xLabels.Count >= 10)
                {
                    xLabels.RemoveAt(0);
                    yLabels.RemoveAt(0);
                    x1Labels.RemoveAt(0);
                }

                if (m_OEECalculation.CurrentShiftNo > ShiftCount)
                {
                    pxLabels.Clear();
                    px1Labels.Clear();
                    pyLabels.Clear();
                }

                foreach (KeyValuePair<int, int> OEEKeyPair in top10Data)
                {
                    string CheckCurrentTime = m_OEECalculation.StartTime.AddHours(OEEKeyPair.Key).ToString("yyyy-MM-dd HH:00:00");

                    #region Performance (Hours)
                    if (yLabels.Contains(CheckCurrentTime))
                    {
                        int index = yLabels.IndexOf(CheckCurrentTime);
                        xLabels[index] = OEEKeyPair.Value;
                        x1Labels[index] = (m_OEECalculation.MaxProductPerShift / (24.0 / m_OEECalculation.ShiftCount)) - xLabels[index];
                    }
                    else
                    {
                        string strDateTime = data.OEEStartTime.AddHours(OEEKeyPair.Key).ToString("yyyy-MM-dd HH:00:00");
                        OldTotalOutput = OEEKeyPair.Value;

                        xLabels.Add(OEEKeyPair.Value);
                        yLabels.Add(strDateTime);
                        x1Labels.Add(Math.Round(m_OEECalculation.MaxProductPerShift / (24.0 / (double)m_OEECalculation.ShiftCount), 2));
                        SeriesCollection = new SeriesCollection
                        {
                            new StackedRowSeries
                            {
                                Title = "Output",
                                Values = xLabels, //Total Output which need to put inside
                                StackMode = StackMode.Values,
                                DataLabels = true,
                                Foreground = Brushes.Black,
                                Fill = Brushes.GreenYellow,
                            },

                            new StackedRowSeries
                            {
                                Title = "Target",
                                Values = x1Labels,
                                StackMode = StackMode.Values,
                                DataLabels = true,
                                Foreground = Brushes.Black,
                                Fill = Brushes.OrangeRed,
                            }
                        };

                        Labels = yLabels.ToArray();
                        SeriesCollection[0].Values = xLabels;
                        RaisePropertyChanged(nameof(Labels));
                        RaisePropertyChanged(nameof(SeriesCollection));
                    }
                    #endregion
                }

                foreach (KeyValuePair<int, int> OEEShiftKeyPair in top10ShiftData)
                {
                    int CheckCurrentShift = m_OEECalculation.CurrentShiftNo;

                    #region Performance (Shift)
                    if (pyLabels.Contains(CheckCurrentShift.ToString()))
                    {
                        int indexCurrentShift = pyLabels.IndexOf(CheckCurrentShift.ToString());
                        pxLabels[indexCurrentShift] = OEEShiftKeyPair.Value;
                        px1Labels[indexCurrentShift] = m_OEECalculation.MaxProductPerShift - pxLabels[indexCurrentShift];
                    }
                    else
                    {
                        int ShiftNow = m_OEECalculation.CurrentShiftNo;
                        pxLabels.Add(OEEShiftKeyPair.Value);
                        px1Labels.Add(m_OEECalculation.MaxProductPerShift);
                        pyLabels.Add(m_OEECalculation.CurrentShiftNo.ToString());

                        PerformanceShiftCollection = new SeriesCollection
                        {
                            new StackedRowSeries
                            {
                                Title = "Output Current Shift",
                                Values = pxLabels,
                                StackMode = StackMode.Values,
                                DataLabels = true,
                                Foreground =  Brushes.Black,
                                Fill = (SolidColorBrush) new BrushConverter().ConvertFromString("#E69A8DFF"),
                            },

                            new StackedRowSeries
                            {
                                Title = "Target Current Shift",
                                Values = px1Labels,
                                StackMode = StackMode.Values,
                                DataLabels = true,
                                Foreground = Brushes.Black,
                                Fill = (SolidColorBrush) new BrushConverter().ConvertFromString("#5F4B8BFF")
                            }
                        };

                        pLabels = pyLabels.ToArray();
                        PerformanceShiftCollection[0].Values = pxLabels;
                        RaisePropertyChanged(nameof(pLabels));
                        RaisePropertyChanged(nameof(PerformanceShiftCollection));
                    }
                    #endregion
                }
            });
        }

        public override void OnCultureChanged()
        {
            TabPageHeader = GetStringTableValue("OEEView");
        }

        public void UpdateMachineStatusChart(MachineStatusColor OEEColor)
        {
            lock(this)
            {
                if (MachineColumn.Values.Count >= 60)
                {
                    MachineStateColor.RemoveAt(0);
                    MachineColumn.Values.RemoveAt(0);
                    Time.RemoveAt(0);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var color = OEEColor.StatusColor;
                    MachineStateColor.Add(color);
                    MachineColumn.Title = "Status";
                    MachineColumn.Values.Add((double)1);
                    MachineColumn.MaxColumnWidth = double.PositiveInfinity;
                    MachineColumn.ColumnPadding = 0;
                    Time.Add(OEEColor.Time);
                    RaisePropertyChanged(nameof(Time));
                    RaisePropertyChanged(nameof(MachineStatusCollection));
                });
            }
        }
       
        public class ErrorSummary
        {
            public string[] ErrorType { get; set; }
            public ChartValues<double> ErrorCount { get; set; }
        }

        private void LoadDownTimeReason()
        {
            using (var db = new AppDBContext())
            {
                var ErrTypeSummary = db.TblError.GroupBy(e => new { e.Error_Desc }).OrderByDescending(e => e.Count()).Select(x => new { name = x.Key.Error_Desc, count = Convert.ToDouble(x.Count()) }).Take(5);
                ErrorSummary summary = new ErrorSummary();
                summary.ErrorType = ErrTypeSummary.Select(x => x.name).ToArray();
                summary.ErrorCount = new ChartValues<double>();
                ErrTypeSummary.Select(x => x.count).ToList().ForEach(summary.ErrorCount.Add);

                TopFiveDowntimeCollection = new SeriesCollection()
                {
                    new RowSeries
                    {
                        Title = "Total",
                        Values = summary.ErrorCount,
                        DataLabels = true,
                        Fill =(SolidColorBrush) new BrushConverter().ConvertFromString("#FFD662FF"),
                    }
                };
                Type = summary.ErrorType;
            }
        }
        #endregion
    }
}
