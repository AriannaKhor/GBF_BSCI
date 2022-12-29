using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Helpers;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace UIModule.MainPanel
{
	public class SeqStatusViewModel : BaseUIViewModel
    {
        #region Variable
        private string m_TabPageHeader;
        public string TabPageHeader
        {
            get { return m_TabPageHeader; }
            set { SetProperty(ref m_TabPageHeader, value); }
        }

        private DispatcherTimer tmrScanStatus;
        public DelegateCommand ClearLiveSeqNumCommand { get; set; }
        public DelegateCommand SaveLiveSeqNumCommand { get; set; }
        public DelegateCommand DumpInfoCommand { get; set; }
        public ObservableCollection<string> SeqList { get; set; }

        private ObservableCollection<SeqStatus> m_SeqStatusMonitor;
        public ObservableCollection<SeqStatus> SeqStatusMonitor
        {
            get { return m_SeqStatusMonitor; }
            set { SetProperty(ref m_SeqStatusMonitor, value); }
        }

        private FixedSizeObservableCollection<SeqNumTrackData> m_SeqNumTrackingCollection;
        public FixedSizeObservableCollection<SeqNumTrackData> SeqNumTrackingCollection
        {
            get { return m_SeqNumTrackingCollection; }
            set { SetProperty(ref m_SeqNumTrackingCollection, value); }
        }

        private bool m_IsRunScan = false;
        public bool IsRunScan
        {
            get { return m_IsRunScan; }
            set
            {
                SetProperty(ref m_IsRunScan, value);
                tmrScanStatus.IsEnabled = value;
                IsRunScanVisibility = m_IsRunScan ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private bool m_IsRunTracking = false;
        public bool IsRunTracking
        {
            get { return m_IsRunTracking; }
            set
            {
                SetProperty(ref m_IsRunTracking, value);
                Global.SeqStatusScanOn = m_IsRunTracking;
                IsRunLiveScanVisibility = m_IsRunTracking ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private Visibility m_IsRunScanVisibility = Visibility.Collapsed;
        public Visibility IsRunScanVisibility
        {
            get { return m_IsRunScanVisibility; }
            set { SetProperty(ref m_IsRunScanVisibility, value); }
        }

        private Visibility m_IsRunLiveScanVisibility = Visibility.Collapsed;
        public Visibility IsRunLiveScanVisibility
        {
            get { return m_IsRunLiveScanVisibility; }
            set { SetProperty(ref m_IsRunLiveScanVisibility, value); }
        }

        private bool m_CanSaveLiveSeqNum = false;
        public bool CanSaveLiveSeqNum
        {
            get { return m_CanSaveLiveSeqNum; }
            set { SetProperty(ref m_CanSaveLiveSeqNum, value); }
        }

        private bool m_IsSkipCoreSeq = false;
        public bool IsSkipCoreSeq
        {
            get { return m_IsSkipCoreSeq; }
            set { SetProperty(ref m_IsSkipCoreSeq, value); }
        }

        private string m_SelectedSeq = "All";
        public string SelectedSeq
        {
            get { return m_SelectedSeq; }
            set
            {
                m_SelectedSeq = value;
                if (string.IsNullOrEmpty(m_SelectedSeq))
                {
                    m_SelectedSeq = "All";
                }
                RaisePropertyChanged(nameof(SelectedSeq));
                TrackStatusView.Refresh();
            }
        }

        private ICollectionView TrackStatusView;

        #endregion

        #region Constructor

        public SeqStatusViewModel()
        {

            TabPageHeader = GetStringTableValue("SequenceStatus");

            // 200 ms timer set to get seq number from each machine seq
            tmrScanStatus = new DispatcherTimer();
            tmrScanStatus.Tick += new EventHandler(tmrScanStatus_Tick); ;
            tmrScanStatus.Interval = new TimeSpan(0, 0, 0, 0, 200);

            m_EventAggregator.GetEvent<SeqStatusEvent>().Subscribe(UpdateSeqStatus);

            SeqStatusMonitor = new ObservableCollection<SeqStatus>();
            SeqNumTrackingCollection = new FixedSizeObservableCollection<SeqNumTrackData>(10000);

            ClearLiveSeqNumCommand = new DelegateCommand(ClearLiveSeqNum);
            SaveLiveSeqNumCommand = new DelegateCommand(SaveLiveSeqNum);
            DumpInfoCommand = new DelegateCommand(DumpInfo);

            // Add all seq ( machine & core seq) into collection
            for (int i = 0; i < m_DelegateSeq.TotalSeq; i++)
            {
                SeqStatusMonitor.Add(new SeqStatus((SQID)i));
            }

            if (!Directory.Exists(m_SystemConfig.FolderPath.SeqInfoLog))
                Directory.CreateDirectory(m_SystemConfig.FolderPath.SeqInfoLog);
            else if (!Directory.Exists(m_SystemConfig.FolderPath.LiveSeqLog))
                Directory.CreateDirectory(m_SystemConfig.FolderPath.LiveSeqLog);

            SeqList = new ObservableCollection<string>();

            for (int i = 0; i < Enum.GetNames(typeof(SQID)).Length - 1; i++)
                SeqList.Add(((SQID)i).ToString());
            SeqList = new ObservableCollection<string>(SeqList.OrderBy(x => x).ToList());

            TrackStatusView = CollectionViewSource.GetDefaultView(SeqNumTrackingCollection);

            TrackStatusView.Filter = o => string.IsNullOrEmpty(SelectedSeq) || SelectedSeq == "All" || SelectedSeq.Split(',').ToList().Any(x => x == ((SeqNumTrackData)o).SeqName);
        }

        private void tmrScanStatus_Tick(object sender, EventArgs e)
        {
            try
            {
                // Get Seqnum and IsAlive from all machine seq
                for (int i = 0; i < Enum.GetNames(typeof(SQID)).Length - 1; i++)
                {
                    bool isAlive = m_DelegateSeq.GetIsAliveStatus((int)(SQID)i);
                    string seqNum = m_DelegateSeq.GetSeqNum((SQID)i);
                    m_SeqStatusMonitor[i].SeqNum = seqNum;
                    m_SeqStatusMonitor[i].IsSeqAlive = isAlive;
                }
            }
            catch (Exception ex)
            {
                tmrScanStatus.Stop();
                MessageBox.Show(ex.Message, ex.Source);
            }
        }
        #endregion

        #region Event
        public override void OnCultureChanged()
        {
            TabPageHeader = GetStringTableValue("SequenceStatus");
        }
        #endregion

        #region Method
        public void DumpInfo()
        {
            // Dump current seq num info for every seq
            try
            {
                DateTime dtNow = DateTime.Now;
                DateTimeFormatInfo dfi = new DateTimeFormatInfo();
                dfi.ShortDatePattern = "dd-MM-yy";
                string logDate = dtNow.ToString("d", dfi);
                string logTime = dtNow.ToString("HHmmss", DateTimeFormatInfo.InvariantInfo);

                // The saved file path is "GreatechApp.Net\Execution"
                string fileName = $"{m_SystemConfig.FolderPath.SeqInfoLog}{GetStringTableValue("SeqTrace")}[{logDate}_{logTime}].log";
                using (StreamWriter logWriter = new StreamWriter(fileName, true))
                {
                    //char space = Convert.ToChar(" ");	// Provide alingment for the text
                    logWriter.WriteLine(GetStringTableValue("LogTime") + " : " + logTime);
                    logWriter.WriteLine("------------------------------------------------------------------------");
                    logWriter.WriteLine(FileHelper.Pad(GetStringTableValue("Sequence"), 25, ' ') + FileHelper.Pad(GetStringTableValue("SequenceID"), 25, ' ') + GetStringTableValue("IsAlive"));
                    logWriter.WriteLine("------------------------------------------------------------------------");

                    foreach (SeqStatus item in SeqStatusMonitor)
                    {
                        logWriter.WriteLine(item.ToString());
                    }
                    logWriter.WriteLine();
                    logWriter.Close();
                    m_ShowDialog.Show(DialogIcon.Complete, GetDialogTableValue("SeqStatusSaved"));

                    string path = Directory.GetCurrentDirectory();
                    Process.Start(fileName, path);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        public void ClearLiveSeqNum()
        {
            try
            {
                ButtonResult buttonResult = m_ShowDialog.Show(DialogIcon.Question, GetDialogTableValue("AskConfirmDeleteLiveSeq"), ButtonResult.No, ButtonResult.Yes);
                if (buttonResult == ButtonResult.Yes)
                {
                    SeqNumTrackingCollection.Clear();
                    CanSaveLiveSeqNum = IsRunTracking && SeqNumTrackingCollection.Count != 0;
                    m_ShowDialog.Show(DialogIcon.Complete, GetDialogTableValue("LiveSeqCleared"));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }
        public void SaveLiveSeqNum()
        {
            try
            {
                DateTime dtNow = DateTime.Now;
                DateTimeFormatInfo dfi = new DateTimeFormatInfo();
                dfi.ShortDatePattern = "dd-MM-yy";
                string logDate = dtNow.ToString("d", dfi);
                string logTime = dtNow.ToString("HHmmss", DateTimeFormatInfo.InvariantInfo);

                string fileName = $"{m_SystemConfig.FolderPath.LiveSeqLog}{GetStringTableValue("LiveTracking")} [{logDate}_{logTime}].log";
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, true))
                {
                    file.WriteLine("----------------------------------------------------------------------------------");
                    file.WriteLine(FileHelper.Pad(GetStringTableValue("Time"), 20, ' ') + FileHelper.Pad(GetStringTableValue("Sequence"), 25, ' ') + FileHelper.Pad(GetStringTableValue("SequenceID"), 25, ' '));
                    file.WriteLine("----------------------------------------------------------------------------------");
                    foreach (SeqNumTrackData item in SeqNumTrackingCollection)
                    {
                        file.WriteLine(item.ToString());
                    }
                    file.WriteLine();
                    file.Close();
                }
                m_ShowDialog.Show(DialogIcon.Complete, GetDialogTableValue("LiveSeqSave"));

                string path = Directory.GetCurrentDirectory();
                Process.Start(fileName, path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        private void UpdateSeqStatus(SeqStatus updatedSeqStatus)
        {
            try
            {
                //if (IsSkipCoreSeq && updatedSeqStatus.SeqName == SQID.CriticalScan.ToString())
                //{
                //    return;
                //}

                if (IsRunTracking)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SeqNumTrackingCollection.Add(new SeqNumTrackData
                        {
                            DateTime = DateTime.Now.ToString("HH:mm:ss:fff"),
                            SeqName = updatedSeqStatus.SeqName,
                            SeqNum = updatedSeqStatus.SeqNum
                        });
                    });
                }

                CanSaveLiveSeqNum = IsRunTracking && SeqNumTrackingCollection.Count != 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }
        #endregion

        #region Access Implementation
        public override bool CanAccess
        {
            get
            {
                if (m_AuthService.CheckAccess(ACL.Setting) && m_AuthService.CurrentUser.IsAuthenticated)
                {
                    return true;
                }
                return false;
            }
        }
        #endregion
    }
}
