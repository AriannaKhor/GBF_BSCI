using ConfigManager;
using DBManager.Domains;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Resources;
using GreatechApp.Core.Variable;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Markup;
using UIModule.MainPanel;

namespace UIModule.StandardViews
{
    public class OEEAnalysisViewModel : BaseUIViewModel
    {
        #region Variable
        private string m_TabPageHeader;
        public string TabPageHeader
        {
            get { return m_TabPageHeader; }
            set { SetProperty(ref m_TabPageHeader, value); }
        }
     
        private DateTime m_StartDateSelection;
        public DateTime StartDateSelection
        {
            get { return m_StartDateSelection; }
            set { SetProperty(ref m_StartDateSelection, value); }
        }

        private DateTime m_EndDateSelection;
        public DateTime EndDateSelection
        {
            get { return m_EndDateSelection; }
            set { SetProperty(ref m_EndDateSelection, value); }
        }

        private string m_SelectedShiftNo;
        public string SelectedShiftNo
        {
            get { return m_SelectedShiftNo; }
            set { SetProperty(ref m_SelectedShiftNo, value); }
        }

        private ObservableCollection<string> m_ShiftNoCollection;
        public ObservableCollection<string> ShiftNoCollection 
        {
            get { return m_ShiftNoCollection; }
            set { SetProperty(ref m_ShiftNoCollection, value); }
        }

        private ObservableCollection<TblOEE> m_OEEDataCollection;
        public ObservableCollection<TblOEE> OEEDataCollection
        {
            get { return m_OEEDataCollection; }
            set { SetProperty(ref m_OEEDataCollection, value); }
        }

        private bool m_CanExportOEEData;
        public bool CanExportOEEData
        {
            get { return m_CanExportOEEData; }
            set { SetProperty(ref m_CanExportOEEData, value); }
        }

        private XmlLanguage m_CurrentCulture;
        public XmlLanguage CurrentCulture
        {
            get { return m_CurrentCulture; }
            set { SetProperty(ref m_CurrentCulture, value); }
        }

        public DelegateCommand RefreshOEEData { get; private set; }
        public DelegateCommand ExportOEEData { get; private set; }
        #endregion

        #region Constructor
        public OEEAnalysisViewModel()
        {
            RefreshOEEData = new DelegateCommand(RefreshOEE);
            ExportOEEData = new DelegateCommand(ExportOEE);
            ShiftNoCollection = new ObservableCollection<string>();
            OEEDataCollection = new ObservableCollection<TblOEE>();

            CurrentCulture = XmlLanguage.GetLanguage(Global.CurrentCulture.IetfLanguageTag);

            TabPageHeader = GetStringTableValue("OEEAnalysis");

            ShiftNoCollection.Add("All");
            StartDateSelection = DateTime.Now.AddYears(-1);
            EndDateSelection = DateTime.Now;

            SelectedShiftNo = ShiftNoCollection[0];

            CanExportOEEData = false;
            using (var db = new AppDBContext())
            {
                try
                {
                    var query = db.TblOEE.GroupBy(x => new { x.ShiftNo }).Select(x => new { sNo = x.Key.ShiftNo, count = x.Count() }).ToList();
                    foreach (var number in query)
                    {
                        ShiftNoCollection.Add(number.sNo.ToString());
                    }
                }
                catch (Exception ex)
                {
                    m_ShowDialog.Show(DialogIcon.Stop, ex.Message);
                }
            }
        }
        #endregion

        #region Method
        private void RefreshOEE()
        {
            string format = "yyyy-MM-dd HH:mm:ss.fff";
            string TimeStart = StartDateSelection.ToString(format);
            string TimeStop = EndDateSelection.ToString(format);

            if (StartDateSelection > EndDateSelection)
            {
                m_ShowDialog.Show(DialogIcon.Error, GetDialogTableValue("InvalidSelectionDate"));
            }

            OEEDataCollection.Clear();

            using (var db = new AppDBContext())
            {
                if (SelectedShiftNo == "All")
                {
                    var query = db.TblOEE.Where(x => x.OEEDateTime >= StartDateSelection && x.OEEDateTime <= EndDateSelection).Select(x => x);
                    query.ToList().ForEach(OEEDataCollection.Add); 
                }
                else
                {
                    var queryWShift = db.TblOEE.Where(x => x.OEEDateTime >= StartDateSelection && x.OEEDateTime <= EndDateSelection && x.ShiftNo == Convert.ToInt32(SelectedShiftNo)).Select(x => x);
                    OEEDataCollection.AddRange(queryWShift);
                }
            }

            if(OEEDataCollection.Count > 0)
            {
                CanExportOEEData = true;
            }
            else
            {
                CanExportOEEData = false;
            }
        }

        private void ExportOEE()
        {
            try
            {
                string OEE_FileName = $"[{DateTime.Now:yyyy-MM-dd}] {GetStringTableValue("OEEData")}.csv";

                if (!Directory.Exists(m_SystemConfig.FolderPath.OEELog))
                    Directory.CreateDirectory(m_SystemConfig.FolderPath.OEELog);

                if (File.Exists($"{m_SystemConfig.FolderPath.OEELog}{Path.DirectorySeparatorChar}{OEE_FileName}"))
                {
                    File.Delete($"{m_SystemConfig.FolderPath.OEELog}{Path.DirectorySeparatorChar}{OEE_FileName}");
                }

                using (FileStream stream = new FileStream(m_SystemConfig.FolderPath.OEELog + Path.DirectorySeparatorChar + OEE_FileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine($"{GetStringTableValue("DateTime")}, {GetStringTableValue("Availability")}, {GetStringTableValue("Performance")}, {GetStringTableValue("Quality")}, {GetStringTableValue("OEE")}, {GetStringTableValue("ShiftNo")}, {GetStringTableValue("PlannedProdTime")}, {GetStringTableValue("UnplannedDownTime")}, {GetStringTableValue("PlannedDownTime")}, {GetStringTableValue("RunTime")}, {GetStringTableValue("IdealCycleTime")}, {GetStringTableValue("TotalInput")}, {GetStringTableValue("TotalOutput")}");

                    for (int i = 0; i < OEEDataCollection.Count; i++)
                    {
                        writer.WriteLine($"{OEEDataCollection[i].OEEDateTime},{OEEDataCollection[i].ShiftNo}, {OEEDataCollection[i].OEE} , {OEEDataCollection[i].Availability}, {OEEDataCollection[i].Performance}, {OEEDataCollection[i].Quality}, " +
                                         $"{OEEDataCollection[i].PlannedProductionTime}, {OEEDataCollection[i].UnplannedDownTime}, {OEEDataCollection[i].PlannedDownTime}, {OEEDataCollection[i].RunTime}, {OEEDataCollection[i].IdealCycleTime}, " +
                                         $"{OEEDataCollection[i].TotalInput}, {OEEDataCollection[i].TotalOutput}");
                    }
                }

                m_ShowDialog.Show(DialogIcon.Complete, GetDialogTableValue("OEEDataSaved"));
            }
            catch (Exception ex)
            {
                m_ShowDialog.Show(DialogIcon.Stop, ex.Message);
            }
        }
        #endregion

        #region Event 
        public override void OnCultureChanged()
        {
            CurrentCulture = XmlLanguage.GetLanguage(Global.CurrentCulture.IetfLanguageTag);
            TabPageHeader = GetStringTableValue("OEEAnalysis");
        }
        #endregion
    }
}
