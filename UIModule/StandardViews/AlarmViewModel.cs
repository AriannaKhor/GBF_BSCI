using Prism.Mvvm;
using Prism.Regions;
using System.Collections.ObjectModel;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Enums;
using System;
using GreatechApp.Core.Interface;
using ConfigManager;
using Prism.Services.Dialogs;
using System.Windows.Controls;
using GreatechApp.Services.UserServices;
using Prism.Events;
using GreatechApp.Core.Events;
using GreatechApp.Core.Cultures;
using UIModule.MainPanel;

namespace UIModule.StandardViews
{
    public class AlarmViewModel : BaseUIViewModel, INavigationAware
    {
        #region Variable
        private string m_Title;
        public string Title
        {
            get { return m_Title; }
            set { SetProperty(ref m_Title, value); }
        }

        private string m_SelectedAlarm;
        public string SelectedAlarm
        {
            get { return m_SelectedAlarm; }
            set
            {
                SetProperty(ref m_SelectedAlarm, value);
                ComboBoxSelectionChanged();
            }
        }

        private int m_SelectedAlarmIndex;
        public int SelectedAlarmIndex
        {
            get { return m_SelectedAlarmIndex; }
            set
            {
                SetProperty(ref m_SelectedAlarmIndex, value);
            }
        }

        private ObservableCollection<string> m_AlarmCollection;
        public ObservableCollection<string> AlarmCollection
        {
            get { return m_AlarmCollection; }
            set
            { SetProperty(ref m_AlarmCollection, value); }
        }

        private ObservableCollection<AlarmParameter> m_DataGridItems;
        public ObservableCollection<AlarmParameter> DataGridItems
        {
            get { return m_DataGridItems; }
            set
            {
                SetProperty(ref m_DataGridItems, value);
            }
        }

        IError m_Error;
        #endregion

        #region Constructor

        public AlarmViewModel(IError error)
        {
            m_Error = error;

            //Title = GetStringTableValue("AlarmConfig");
            AlarmCollection = new ObservableCollection<string>();

            for (int i = 0; i < Enum.GetNames(typeof(SQID)).Length - 1; i++)
            {
                AlarmCollection.Add(((SQID)i).ToString());
            }
        }

        #endregion

        #region Event
        #endregion

        #region Method
        public void ComboBoxSelectionChanged()
        {
            DataGridItems = new ObservableCollection<AlarmParameter>();

            if (AlarmCollection != null)
            {
                ErrorConfig errorConfig = ErrorConfig.Open(m_SystemConfig.SeqCfgRef[SelectedAlarmIndex].ErrLibPath, m_SystemConfig.SeqCfgRef[SelectedAlarmIndex].ErrLib);

                for (int i = 0; i < errorConfig.ErrTable.Count; i++)
                {
                    DataGridItems.Add(new AlarmParameter()
                    {
                        ErrorCode = errorConfig.ErrTable[i].Code,
                        Station = errorConfig.ErrTable[i].Station,
                        AlarmType = errorConfig.ErrTable[i].AlarmType,
                        Causes = errorConfig.ErrTable[i].Cause,
                        Recovery = errorConfig.ErrTable[i].Recovery,
                    });
                }
            }
        }

        public void Save(object sender, DataGridRowEditEndingEventArgs e)
        {
            int rowIndex = e.Row.GetIndex();
            ErrorConfig errorConfig = ErrorConfig.Open(m_SystemConfig.SeqCfgRef[SelectedAlarmIndex].ErrLibPath, m_SystemConfig.SeqCfgRef[SelectedAlarmIndex].ErrLib);

            ButtonResult dialogResult = m_ShowDialog.Show(DialogIcon.Question, GetDialogTableValue("AskConfirmSaveAlarmDetails"), ButtonResult.Cancel, ButtonResult.Yes);
            if (dialogResult == ButtonResult.Yes)
            {
                errorConfig.ErrTable[rowIndex].Cause = DataGridItems[rowIndex].Causes;
                errorConfig.ErrTable[rowIndex].Recovery = DataGridItems[rowIndex].Recovery;
                errorConfig.ErrTable[rowIndex].Station = DataGridItems[rowIndex].Station;
                errorConfig.ErrTable[rowIndex].AlarmType = DataGridItems[rowIndex].AlarmType;

                ErrorConfig.Save(errorConfig);

                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("User")} {m_CurrentUser.Username} {GetStringTableValue("Update")} {GetStringTableValue("AlarmConfig")} : [{DataGridItems[rowIndex].Causes}]." });
            }
            else
            {
                DataGridItems[rowIndex].Causes = errorConfig.ErrTable[rowIndex].Cause;
                DataGridItems[rowIndex].Recovery = errorConfig.ErrTable[rowIndex].Recovery;
                DataGridItems[rowIndex].Station = errorConfig.ErrTable[rowIndex].Station;
                DataGridItems[rowIndex].AlarmType = errorConfig.ErrTable[rowIndex].AlarmType;
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
            m_Error = null;
            DataGridItems = null;
            AlarmCollection = null;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        #endregion

    }

}
