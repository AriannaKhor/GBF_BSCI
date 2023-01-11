using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.ObjectModel;
using System.Windows;
using ConfigManager;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Resources;
using LiveCharts;
using Prism.Commands;
using Prism.Mvvm;
using DBManager.Domains;
using LiveCharts.Wpf;
using System.IO;
using System.Diagnostics;
using GreatechApp.Core.Helpers;
using GreatechApp.Core.Interface;
using GreatechApp.Services.UserServices;
using Prism.Events;
using GreatechApp.Core.Events;
using System.Globalization;
using System.Windows.Markup;
using System.Threading;
using Prism.Regions;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Variable;
using UIModule.MainPanel;

namespace UIModule.StandardViews
{
    public class ErrorAnalysisViewModel : BaseUIViewModel, INavigationAware
    {
        #region Variable
        private int m_SelectionCnt;
        private List<int> m_SelectionCntList = new List<int>();
        public Func<ChartPoint, string> PointLabel { get; set; }
        public DelegateCommand ReloadDBCommand { get; set; }
        public DelegateCommand ExportCommand { get; set; }

        private int m_MaxValue = 0;
        public int MaxValue
        {
            // Will be generated when the lot has ended. (User does not key in full LotNo in the beginning of the lot)
            get { return m_MaxValue; }
            set { SetProperty(ref m_MaxValue, value); }
        }

        private DateTime m_TodayFromDateTime = DateTime.Today;
        public DateTime TodayFromDateTime
        {
            get { return m_TodayFromDateTime; }
            set
            {
                if (m_TodayFromDateTime != value)
                {
                    SetProperty(ref m_TodayFromDateTime, value);
                }
            }
        }

        private DateTime m_TodayToDateTime = DateTime.Today.AddDays(1).AddTicks(-1);
        public DateTime TodayToDateTime
        {
            get { return m_TodayToDateTime; }
            set
            {
                if (m_TodayToDateTime != value)
                {
                    SetProperty(ref m_TodayToDateTime, value);
                }
            }
        }

        private DateTime m_SelectionStart = DateTime.Today;
        public DateTime SelectionStart
        {
            get { return m_SelectionStart; }
            set
            {
                if (m_SelectionStart != value)
                {
                    SetProperty(ref m_SelectionStart, value);
                    DisposeResource();
                }
            }
        }

        private DateTime m_SelectionEnd = DateTime.Today.AddDays(1).AddTicks(-1);
        public DateTime SelectionEnd
        {
            get { return m_SelectionEnd; }
            set
            {
                if (m_SelectionEnd != value)
                {
                    SetProperty(ref m_SelectionEnd, value);
                    DisposeResource();
                }
            }
        }

        private int m_TotalHistoryResult;
        public int TotalHistoryResult
        {
            get { return m_TotalHistoryResult; }
            set { SetProperty(ref m_TotalHistoryResult, value); }
        }

        private SeriesCollection m_SeriesCollection = new SeriesCollection();
        public SeriesCollection PieSeriesList
        {
            get { return m_SeriesCollection; }
            set { SetProperty(ref m_SeriesCollection, value); }
        }

        private ObservableCollection<TblError> m_ErrLogs;
        public ObservableCollection<TblError> ErrLogs
        {
            // ItemSource for grid view.
            get { return m_ErrLogs; }
            set
            {
                if (m_ErrLogs == value)
                {
                    return;
                }
                SetProperty(ref m_ErrLogs, value);
            }
        }

        private string m_ViewBy;
        public string ViewBy
        {
            // Will be generated when the lot has ended. (User does not key in full LotNo in the beginning of the lot)
            get { return m_ViewBy; }
            set { SetProperty(ref m_ViewBy, value); }
        }

        private Dictionary<string, int> m_StationErrList;
        public Dictionary<string, int> StationErrList
        {
            get { return m_StationErrList; }
            set { SetProperty(ref m_StationErrList, value); }
        }

        private bool m_IsAllowExportData;
        public bool IsAllowExportData
        {
            get { return m_IsAllowExportData; }
            set { SetProperty(ref m_IsAllowExportData, value); }
        }

        private XmlLanguage m_CurrentCulture;
        public XmlLanguage CurrentCulture
        {
            get { return m_CurrentCulture; }
            set { SetProperty(ref m_CurrentCulture, value); }
        }

        #endregion

        #region Constructor
        public ErrorAnalysisViewModel()
        {
            DisposeResource();
            CurrentCulture = XmlLanguage.GetLanguage(Global.CurrentCulture.IetfLanguageTag);
            StationErrList = new Dictionary<string, int>();
            ReloadDBCommand = new DelegateCommand(ReloadDB);
            ExportCommand = new DelegateCommand(SaveAlarmAnalysis);
            m_ViewBy = GetStringTableValue("ViewChartByGroup");
            m_SelectionCnt = 10;

            for (int i = 1; i <= 5; i++)
            {
                m_SelectionCntList.Add(i * 10);
            }
        }
        #endregion

        #region Event
        public override void OnCultureChanged()
        {
            CurrentCulture = XmlLanguage.GetLanguage(Global.CurrentCulture.IetfLanguageTag);
        }
        #endregion

        #region Method        
        public void ReloadDB()
        {
            try
            {
                FetchAlarmRecord();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }
        private void FetchAlarmRecord()
        {
            try
            {
                Dictionary<string, float> timeTakenList = new Dictionary<string, float>();
                Dictionary<string, int> stationErrList = new Dictionary<string, int>();
                List<string> list = new List<string>();
                ErrLogs = new ObservableCollection<TblError>();
                string format = "yyyy-MM-dd HH:mm:ss.fff";
                string stopTime = SelectionEnd.ToString(format);
                string startTime = SelectionStart.ToString(format);

                if (SelectionStart > SelectionEnd)
                {
                    m_ShowDialog.Show(DialogIcon.Error, GetDialogTableValue("InvalidDate"));

                    return;
                }

                using (AppDBContext db = new AppDBContext())
                {
                    List<TblError> query = null;
                    try
                    {
                        query = (from x in db.TblError
                                 orderby x.Stop_Time descending
                                 where x.Stop_Time <= SelectionEnd && x.Start_Time >= SelectionStart && x.Error_Type == "Error"
                                 select x).ToList();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, ex.Source);
                    }
                    ErrLogs.AddRange(query);
                    IsAllowExportData = ErrLogs.Count > 0;
                }

                if(ErrLogs.Count > 0)
                {
                    for (int i = 0; i < ErrLogs.Count; i++)
                    {
                        if (ErrLogs[i].Error_Type == "Error")
                        {
                            list.Add(ErrLogs[i].Station);
                            string errMsg = ErrLogs[i].Station + " - " + ErrLogs[i].Error_Desc;
                        }
                    }

                    TotalHistoryResult = list.Count();
                    string[] totalList = list.ToArray();
                    // use Dictionary to separate keys and values.
                    // Select max 15 error record occur
                    Dictionary<string, int> counts = totalList.GroupBy(x => x)
                                              .OrderByDescending(g => g.Count())
                                              .ToDictionary(g => g.Key,
                                              g => g.Count());
                    StationErrList = counts;
                    PieSeriesList = new SeriesCollection();
                    foreach (KeyValuePair<string, int> pair in counts)
                    {
                        PointLabel = chartPoint =>
                        string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);
                        PieSeriesList.Add(new PieSeries()
                        {
                            Title = pair.Key,
                            Values = new ChartValues<int> { pair.Value },
                            DataLabels = true,
                        });
                    }

                    // assign max + 50 to display label of max record 
                    MaxValue = MaxValue + 100;
                    // update change for BarCollection
                }
                else
                {
                    m_ShowDialog.Show(DialogIcon.Information, GetDialogTableValue("NoRecordFound"));
                }

            }
            catch (Exception ex)
            {
            }
        }

        public void SaveAlarmAnalysis()
        {
            try
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                string filePath = $"{m_SystemConfig.FolderPath.AnalysisLog}Alarm\\{date}\\";
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);
                string datetime = DateTime.Now.ToString("yyyy-MM-dd HHmmss");
                string dateStart = SelectionStart.ToString();
                string dateEnd = SelectionEnd.ToString();
                int length = ErrLogs.Count;
                int totalErr = 0;
                int totalWarn = 0;
                List<string> errList = new List<string>();
                List<string> warnList = new List<string>();

                for (int i = 0; i < length; i++)
                {
                    totalErr = ErrLogs[i].Error_Type == "Error" ? totalErr + 1 : totalErr;
                    totalWarn = ErrLogs[i].Error_Type == "Warning" ? totalWarn + 1 : totalWarn;
                }

                for (int i = 0; i < length; i++)
                {
                    if (ErrLogs[i].Error_Type == "Error")
                    {
                        errList.Add(ErrLogs[i].Station + " : " + ErrLogs[i].Error_Desc);
                    }
                    else if (ErrLogs[i].Error_Type == "Warning")
                    {
                        warnList.Add(ErrLogs[i].Station + " : " + ErrLogs[i].Error_Desc);
                    }
                }

                string[] totalErrList = errList.ToArray();
                string[] totalWarnList = warnList.ToArray();
                // use Dictionary to separate keys and values.
                Dictionary<string, int> errCounts = totalErrList.GroupBy(x => x)
                                          .OrderByDescending(g => g.Count())
                                          .ToDictionary(g => g.Key,
                                          g => g.Count());
                Dictionary<string, int> warnCounts = totalWarnList.GroupBy(x => x)
                                          .OrderByDescending(g => g.Count())
                                          .ToDictionary(g => g.Key,
                                          g => g.Count());

                string fileName = $"{filePath}{GetStringTableValue("AlarmSummary")} [{datetime}].log";
                using (StreamWriter logWriter = new StreamWriter(fileName, true))
                {
                    StringBuilder lotSummary = new StringBuilder();
                    lotSummary.Append(FileHelper.Pad(string.Empty, 100, '-')).AppendLine();
                    lotSummary.Append($"{GetStringTableValue("StartDate")} : " + dateStart).AppendLine();
                    lotSummary.Append($"{GetStringTableValue("EndDate")}   : " + dateEnd).AppendLine();
                    lotSummary.Append($"{GetStringTableValue("TotalErrorData")} : " + length).AppendLine();
                    lotSummary.Append(FileHelper.Pad(string.Empty, 100, '-')).AppendLine();
                    lotSummary.Append(FileHelper.Pad(string.Empty, 100, '-')).AppendLine();
                    //  Error 
                    lotSummary.Append($"# {GetStringTableValue("ErrorOccurence")} #").AppendLine();
                    // Loop over pairs with foreach
                    foreach (KeyValuePair<string, int> pair in errCounts)
                    {
                        lotSummary.Append("(" + pair.Value + ")   - " + pair.Key).AppendLine();
                    }
                    // Warning
                    lotSummary.Append($"# {GetStringTableValue("WarningOccurence")} #").AppendLine();
                    // Loop over pairs with foreach
                    foreach (KeyValuePair<string, int> pair in warnCounts)
                    {
                        lotSummary.Append("(" + pair.Value + ")   - " + pair.Key).AppendLine();
                    }
                    lotSummary.Append(FileHelper.Pad(string.Empty, 100, '-')).AppendLine();
                    lotSummary.Append($"{GetStringTableValue("TotalErrorCount")} : " + totalErr).AppendLine();
                    lotSummary.Append($"{GetStringTableValue("TotalWarningCount")} : " + totalWarn).AppendLine();

                    lotSummary.Append(FileHelper.Pad(string.Empty, 100, '-')).AppendLine();
                    lotSummary.Append(FileHelper.Pad(string.Empty, 100, '-')).AppendLine();
                    lotSummary.Append(GetStringTableValue("LogCompilationCompleted")).AppendLine();

                    logWriter.Write(lotSummary);
                    logWriter.Close();
                    m_ShowDialog.Show(DialogIcon.Complete, GetDialogTableValue("AlarmSummarySave"));

                    string path = Directory.GetCurrentDirectory();
                    Process.Start(fileName, path);

                    m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("Save")} {GetStringTableValue("ErrorAnalysis")} , {GetStringTableValue("Path")} : {fileName}" });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }
        private void DisposeResource()
        {
            // When user selects a new filter or screen is inactive
            ErrLogs = null;
            StationErrList = null;
            TotalHistoryResult = 0;
            PieSeriesList = new SeriesCollection();
            IsAllowExportData = false;
        }
        #endregion

        #region Properties
        public bool KeepAlive
        {
            get
            {
                return false;
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {

        }
        #endregion

    }
}
