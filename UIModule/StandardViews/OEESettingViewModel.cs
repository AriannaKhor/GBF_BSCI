using DBManager.Domains;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Resources;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using UIModule.MainPanel;

namespace UIModule.StandardViews
{
    public class OEESettingViewModel : BaseUIViewModel
    {
        private string m_TabPageHeader;
        public string TabPageHeader
        {
            get { return m_TabPageHeader; }
            set { SetProperty(ref m_TabPageHeader, value); }
        }

        private double m_IdealCycleTime;
        public double IdealCycleTime
        {
            get { return m_IdealCycleTime; }
            set { SetProperty(ref m_IdealCycleTime, value); }
        }

        private ObservableCollection<int> m_TimePickerHourList;
        public ObservableCollection<int> TimePickerHourList
        {
            get { return m_TimePickerHourList; }
            set { SetProperty(ref m_TimePickerHourList, value); }
        }

        private ObservableCollection<string> m_TimePickerMinList;

        public ObservableCollection<string> TimePickerMinList
        {
            get { return m_TimePickerMinList; }
            set { SetProperty(ref m_TimePickerMinList, value); }
        }

        private ObservableCollection<string> m_TimePickerSecList;

        public ObservableCollection<string> TimePickerSecList
        {
            get { return m_TimePickerSecList; }
            set { SetProperty(ref m_TimePickerSecList, value); }
        }

        private ObservableCollection<string> m_TimePickerPeriodList;

        public ObservableCollection<string> TimePickerPeriodList
        {
            get { return m_TimePickerPeriodList; }
            set { SetProperty(ref m_TimePickerPeriodList, value); }
        }

        private int m_TimePickerHour;
        public int TimePickerHour
        {
            get { return m_TimePickerHour; }
            set { SetProperty(ref m_TimePickerHour, value); }
        }

        private string m_TimePickerMin;
        public string TimePickerMin
        {
            get { return m_TimePickerMin; }
            set { SetProperty(ref m_TimePickerMin, value); }
        }

        private string m_TimePickerSec;
        public string TimePickerSec
        {
            get { return m_TimePickerSec; }
            set { SetProperty(ref m_TimePickerSec, value); }
        }

        public string m_TimePickerPeriod;
        public string TimePickerPeriod
        {
            get { return m_TimePickerPeriod; }
            set { SetProperty(ref m_TimePickerPeriod, value); }
        }

        private DateTime m_ShiftStartTime;
        public DateTime ShiftStartTime
        {
            get { return m_ShiftStartTime; }
            set { SetProperty(ref m_ShiftStartTime, value); }
        }

        private int m_ShiftCount;
        public int ShiftCount
        {
            get { return m_ShiftCount; }
            set { SetProperty(ref m_ShiftCount, value); }
        }

        private string m_Status;
        public string Status
        {
            get { return m_Status; }
            set { SetProperty(ref m_Status, value); }
        }

        public DelegateCommand SaveCommand { get; set; }

        public OEESettingViewModel()
        {
            TabPageHeader = GetStringTableValue("OEESetting");

            TimePickerHourList = new ObservableCollection<int>();
            TimePickerMinList = new ObservableCollection<string>();
            TimePickerSecList = new ObservableCollection<string>();
            TimePickerPeriodList = new ObservableCollection<string>();

            for (int i = 1; i <= 12; i++)
                TimePickerHourList.Add(i);

            for (int i = 0; i <= 59; i++)
            {
                TimePickerMinList.Add(i.ToString("00"));
                TimePickerSecList.Add(i.ToString("00"));
            }

            TimePickerPeriodList.Add("AM");
            TimePickerPeriodList.Add("PM");

            SaveCommand = new DelegateCommand(SaveMethod);

            LoadDataFromDB();
        }


        #region Method
        private void LoadDataFromDB()
        {
            TblOEEConfig config = new TblOEEConfig();

            using (var db = new AppDBContext())
            {
                try
                {
                    config = (from x in db.TblOEEConfig
                             select x).First();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.Source);
                }
            }

            IdealCycleTime = (double)config.IdealCycleTime;
            ShiftCount = (int)config.ShiftCount;

            TimeSpan time = (TimeSpan)config.ShiftStartTime;
            TimePickerHour = time.Hours > 12 ? time.Hours - 12 : time.Hours;
            TimePickerMin = time.Minutes.ToString("00");
            TimePickerSec = time.Seconds.ToString("00");
            TimePickerPeriod = time.Hours > 12 ? "PM" : "AM";
        }

        private void SaveMethod()
        {
            try
            {
                using (var db = new AppDBContext())
                {
                    TblOEEConfig config = db.TblOEEConfig.Single();
                    config.IdealCycleTime = IdealCycleTime;
                    config.ShiftCount = ShiftCount;
                    config.ShiftStartTime = new TimeSpan(TimePickerPeriod == "PM" ? TimePickerHour + 12 : TimePickerHour, Convert.ToInt32(TimePickerMin), Convert.ToInt32(TimePickerSec));
                    db.TblOEEConfig.Update(config);
                    db.SaveChanges();
                }
                Status = GetDialogTableValue("OEESaved");
                m_ShowDialog.Show(DialogIcon.Information, GetDialogTableValue("RestartApp"));
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
        }
        #endregion

        #region Event 
        public override void OnCultureChanged()
        {
            TabPageHeader = GetStringTableValue("OEESetting");
        }
        #endregion
    }
}
