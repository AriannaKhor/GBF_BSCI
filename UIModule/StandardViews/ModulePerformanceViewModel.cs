using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Modal;
using Prism.Events;
using Prism.Mvvm;
using GreatechApp.Core.Resources;
using System.Windows.Forms;
using GreatechApp.Core.Helpers;
using ConfigManager;
using System.IO;
using Prism.Services.Dialogs;
using GreatechApp.Core.Enums;
using System.Diagnostics;
using Prism.Commands;
using GreatechApp.Services.UserServices;
using GreatechApp.Core.Events;
using Sequence;
using GreatechApp.Core.Cultures;
using UIModule.MainPanel;

namespace UIModule.StandardViews
{
    public class ModulePerformanceViewModel : BaseUIViewModel
    {
        #region Variable
        private readonly UserList m_UserInfo;
        private readonly LotInfo m_LotInfo;

        private DispatcherTimer tmrModulePerfScan;
        private Tuple<TimeSpan, TimeSpan, TimeSpan> m_ModulePerfRefreshRate;
        private string m_SelectedModulePerfRefreshRate;

        public ObservableCollection<SeqModulePerf> ModulePerfCollection { get; private set; }
        private DelegateCommand<string> m_UpdateUoMCommand;

        public DelegateCommand<string> UpdateUoMCommand
        {
            get { return m_UpdateUoMCommand; }
            set { SetProperty(ref m_UpdateUoMCommand, value); }
        }
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
        public bool IsUoMInSecond { get; set; }
        public bool IsUoMInMilliSecond { get; set; }

        private string m_SelectedUoM;
        public string SelectedUoM
        {
            get { return m_SelectedUoM; }
            private set { SetProperty(ref m_SelectedUoM, value); }
        }

        private ObservableCollection<PerfKeyValuePair> m_SelectedModulePerfCollection;
        public ObservableCollection<PerfKeyValuePair> SelectedModulePerfCollection
        {
            get { return m_SelectedModulePerfCollection; }
            private set { SetProperty(ref m_SelectedModulePerfCollection, value); }
        }

        private SeqModulePerf m_SelectedItem;
        public SeqModulePerf SelectedItem
        {
            get { return m_SelectedItem; }
            set
            {
                SetProperty(ref m_SelectedItem, value);
                SelectedModulePerfCollection = value.PerfCollection;
            }
        }

        public string SelectedModulePerfRefreshRate
        {
            get { return m_SelectedModulePerfRefreshRate; }
            set { SetProperty(ref m_SelectedModulePerfRefreshRate, value); }
        }
        #endregion

        #region Constructor
        public ModulePerformanceViewModel(LotInfo lotInfo, UserList userInfo)
        {
            m_LotInfo = lotInfo;
            m_UserInfo = userInfo;

            m_ModulePerfRefreshRate = Tuple.Create(new TimeSpan(0, 0, 0, 0, 100),
                                       new TimeSpan(0, 0, 0, 0, 300),
                                       new TimeSpan(0, 0, 0, 1, 0));
            tmrModulePerfScan = new DispatcherTimer();
            tmrModulePerfScan.Interval = m_ModulePerfRefreshRate.Item2; // Normal rate
            tmrModulePerfScan.Tick += tmrModulePerfScan_Tick;
            UpdateUoMCommand = new DelegateCommand<string>(UpdateUoM);
            UpdateModulePerfRefreshRateCommand = new DelegateCommand<string>(UpdateModulePerfRefreshRate);
            ExportCommand = new DelegateCommand(Export);

            SelectedModulePerfRefreshRate = GetStringTableValue("Normal");
            ModulePerfCollection = new ObservableCollection<SeqModulePerf>();

            for (int i = m_DelegateSeq.CoreSeqNum; i < m_DelegateSeq.TotalSeq; i++)
            {
                ModulePerfCollection.Add(new SeqModulePerf(i));
                SQID SeqID = (SQID)i;
                string[] perfNames = m_DelegateSeq.GetPerfNames(SeqID);
                Debug.Assert(perfNames != null, "perfNames cannot be null!");
                if (perfNames != null)
                {
                    // Automatically populate the views with Perf enum members from each machine sequence module.
                    foreach (string name in perfNames)
                    {
                        ModulePerfCollection[i - m_DelegateSeq.CoreSeqNum].PerfCollection.Add(new PerfKeyValuePair(name));
                    }
                }
            }
            SelectedItem = ModulePerfCollection[0];

            // Unit Of Measurement.
            IsUoMInSecond = false;
            IsUoMInMilliSecond = true;      // Default as ms
            SelectedUoM = GetStringTableValue("ms");   // Display as short hand.

            tmrModulePerfScan.Start();
        }
        #endregion

        #region Event
        private void tmrModulePerfScan_Tick(object sender, EventArgs e)
        {
            try
            {
                RefreshModulePerf();
            }
            catch (Exception ex)
            {
                tmrModulePerfScan.Stop();
                MessageBox.Show(ex.Message, ex.Source);
            }
        }
        #endregion

        #region Method
        private void RefreshModulePerf()
        {
            int index = 0;
            foreach (PerfKeyValuePair item in SelectedModulePerfCollection)
            {
                // items in PerfCollection are arranged in the same order as in Perf enum for each module sequence.
                // Default UoM = ms
                int curCycleTime = m_DelegateSeq.GetProcCycleTime(SelectedItem.SeqID, index);
                int minCycleTime = m_DelegateSeq.GetMinCycleTime(SelectedItem.SeqID, index);
                double avgCycleTime = m_DelegateSeq.GetAvgCycleTime(SelectedItem.SeqID, index);
                int maxCycleTime = m_DelegateSeq.GetMaxCycleTime(SelectedItem.SeqID, index);

                if (IsUoMInSecond)
                {
                    // Convert from ms to second.
                    item.Value = Math.Round(curCycleTime / 1000.0, 3).ToString();
                    item.MinCycleTime = Math.Round(minCycleTime / 1000.0, 3).ToString();
                    item.AvgCycleTime = Math.Round(avgCycleTime / 1000.0, 3).ToString();
                    item.MaxCycleTime = Math.Round(maxCycleTime / 1000.0, 3).ToString();
                }
                else
                {
                    // Remains in ms
                    item.Value = curCycleTime.ToString();
                    item.MinCycleTime = minCycleTime.ToString();
                    item.AvgCycleTime = avgCycleTime.ToString();
                    item.MaxCycleTime = maxCycleTime.ToString();
                }
                index++;
            }
        }

        public void UpdateModulePerfRefreshRate(string rate)
        {
            // Set new Refresh Rate selected by user.
            if (rate == "High")
            {
                SelectedModulePerfRefreshRate = GetStringTableValue("High");
                tmrModulePerfScan.Interval = m_ModulePerfRefreshRate.Item1;
            }
            else if (rate == "Normal")
            {
                SelectedModulePerfRefreshRate = GetStringTableValue("Normal");
                tmrModulePerfScan.Interval = m_ModulePerfRefreshRate.Item2;
            }
            else if (rate == "Slow")
            {
                SelectedModulePerfRefreshRate = GetStringTableValue("Slow");
                tmrModulePerfScan.Interval = m_ModulePerfRefreshRate.Item3;
            }
        }

        public void UpdateUoM(string selectedUnit)
        {
            if (selectedUnit == "s")
            {
                IsUoMInMilliSecond = false;
                IsUoMInSecond = true;
            }
            else if (selectedUnit == "ms")
            {
                IsUoMInSecond = false;
                IsUoMInMilliSecond = true;
            }

            // For Seq Module Performance (eithier s or ms only).
            SelectedUoM = IsUoMInSecond ? GetStringTableValue("s") : GetStringTableValue("ms");
            RefreshModulePerf();
        }
        public void Export()
        {
            try
            {
                RefreshModulePerf();
                WriteModulePerfToFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        private void WriteModulePerfToFile()
        {
            try
            {
                // When user click the Export button at Module Performance section
                // As a standard, we will write all on screen info to a file
                // Build the directory on a daily basis.
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                string filePath = $"{m_SystemConfig.FolderPath.AnalysisLog}Performance\\{date}\\";
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);
                string datetime = DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss");

                string fileName = $"{filePath}{GetStringTableValue("ModPerf")}[{datetime}].log";
                using (StreamWriter logWriter = new StreamWriter(fileName, true))
                {
                    StringBuilder screenData = new StringBuilder();
                    screenData.Append(FileHelper.Pad(GetStringTableValue("ModPerfSummary"), 80, '*')).AppendLine();
                    screenData.AppendLine();
                    screenData.AppendLine(ModulePerformanceData);
                    screenData.Append(FileHelper.Pad(GetStringTableValue("EndOfSummary"), 80, '-')).AppendLine();
                    logWriter.Write(screenData);
                    logWriter.Close();

                    m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("WriteModPerf")}, {GetStringTableValue("Path")} : {fileName}" });
                    m_ShowDialog.Show(DialogIcon.Complete, GetDialogTableValue("ModulePerfSaved"));

                    string path = Directory.GetCurrentDirectory();
                    Process.Start(fileName, path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        private string ModulePerformanceData
        {
            //-----------------MagIn-----------------------------------------
            //Title                         Last      Min       Max       Avg          
            //---------------------------------------------------------------
            //Overall                       0         0         0         0
            get
            {
                StringBuilder moduleData = new StringBuilder();
                // Module Performance
                const string modPerfColFormat = "{0, -40}{1, -10}{2, -10}{3, -10}{4, -10}";
                string[] modPerfColHeaderTitle = { GetStringTableValue("Title"), GetStringTableValue("Last"), GetStringTableValue("Min"), GetStringTableValue("Max"), GetStringTableValue("Average") };
                string modPerfColHeader = string.Format(modPerfColFormat, modPerfColHeaderTitle);

                for (int i = 0; i < ModulePerfCollection.Count(); i++)
                {
                    moduleData.Append(FileHelper.Pad(ModulePerfCollection[i].SeqID.ToString(), 80, '-')).AppendLine();
                    moduleData.Append(modPerfColHeader).AppendLine();
                    moduleData.Append(FileHelper.Pad(string.Empty, 80, '-')).AppendLine();
                    int index = 0;
                    foreach (PerfKeyValuePair item in ModulePerfCollection[i].PerfCollection)
                    {
                        // items in PerfCollection are arranged in the same order as in Perf enum for each module sequence.
                        if (IsUoMInSecond)
                        {
                            // Convert from ms to second.
                            double cycleTime = Math.Round(m_DelegateSeq.GetProcCycleTime(ModulePerfCollection[i].SeqID, index) / 1000.0, 3);
                            item.Value = cycleTime.ToString();
                        }
                        else
                        {
                            // Remains in ms
                            item.Value = m_DelegateSeq.GetProcCycleTime(ModulePerfCollection[i].SeqID, index).ToString();
                        }
                        item.MinCycleTime = m_DelegateSeq.GetMinCycleTime(ModulePerfCollection[i].SeqID, index).ToString();
                        item.MaxCycleTime = m_DelegateSeq.GetMaxCycleTime(ModulePerfCollection[i].SeqID, index).ToString();
                        item.AvgCycleTime = Math.Round(m_DelegateSeq.GetAvgCycleTime(ModulePerfCollection[i].SeqID, index), 3).ToString();
                        index++;
                        string modPerf = string.Format(modPerfColFormat,
                            new string[] { item.Title, item.Value, item.MinCycleTime.ToString(), item.MaxCycleTime.ToString(), item.AvgCycleTime.ToString() });
                        moduleData.Append(modPerf).AppendLine();
                    }
                    moduleData.AppendLine();
                }
                moduleData.AppendLine();
                return moduleData.ToString();
            }
        }
        #endregion
    }
}
