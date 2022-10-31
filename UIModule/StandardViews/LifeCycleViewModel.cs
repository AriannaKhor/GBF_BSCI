namespace UIModule.StandardViews
{
	using ConfigManager;
	using ConfigManager.Constant;
    using GreatechApp.Core.Cultures;
    using GreatechApp.Core.Enums;
	using GreatechApp.Core.Events;
	using GreatechApp.Core.Interface;
	using GreatechApp.Core.Modal;
	using GreatechApp.Core.Variable;
	using GreatechApp.Services.UserServices;
	using Prism.Commands;
	using Prism.Events;
	using Prism.Mvvm;
	using Prism.Regions;
	using Prism.Services.Dialogs;
	using System;
	using System.Collections.ObjectModel;
	using System.Timers;
	using System.Windows.Controls;
    using UIModule.MainPanel;

    public class LifeCycleViewModel : BaseUIViewModel, INavigationAware, IRegionMemberLifetime
    {
        #region Variable
        private Timer tmr_UpdateToolLife;

        private const int CleaningCol = 2;
        private const int ToolLifeCol = 5;

        private string m_title;
        public string Title
        {
            get { return m_title; }
            set { SetProperty(ref m_title, value); }
        }

        private ObservableCollection<ToolLifeParameter> m_ToolLifeCollection;

        public ObservableCollection<ToolLifeParameter> ToolLifeCollection
        {
            get { return m_ToolLifeCollection; }
            set { SetProperty(ref m_ToolLifeCollection, value); }
        }

        public DelegateCommand<ToolLifeParameter> ResetCleaningCommand { get; set; }
        public DelegateCommand<ToolLifeParameter> ResetTLCommand { get; set; }
        public DelegateCommand ResetAllCmd { get; set; }

        private ToolLifeConfig m_toolLifeCfg;

        #endregion

        #region Constructor

        public LifeCycleViewModel()
        {
            Title = GetStringTableValue("ToolLifeCycle");

            ToolLifeCollection = new ObservableCollection<ToolLifeParameter>();

            m_toolLifeCfg = ToolLifeConfig.Open(m_SystemConfig.CounterCfgRef[CounterCFG.ToolLife].Reference);

            for (int i = 0; i < m_toolLifeCfg.TLCollection.Count; i++)
            {
                ToolLifeCollection.Add(new ToolLifeParameter
                {
                    ID = m_toolLifeCfg.TLCollection[i].ID,
                    ToolName = m_toolLifeCfg.TLCollection[i].ToolName,
                    ToolTip = m_toolLifeCfg.TLCollection[i].Tooltip,
                    CleaningValue = m_toolLifeCfg.TLCollection[i].CleaningValue,
                    MinCleaning = m_toolLifeCfg.TLCollection[i].MinCleaning,
                    MaxCleaning = m_toolLifeCfg.TLCollection[i].MaxCleaning,
                    PrevMaxCleaning = m_toolLifeCfg.TLCollection[i].MaxCleaning,
                    ToolLifeValue = m_toolLifeCfg.TLCollection[i].ToolLifeValue,
                    MinToolLife = m_toolLifeCfg.TLCollection[i].MinToolLife,
                    MaxToolLife = m_toolLifeCfg.TLCollection[i].MaxToolLife,
                    PrevMaxToolLife = m_toolLifeCfg.TLCollection[i].MaxToolLife,
                    IsCleaningEnable = m_toolLifeCfg.TLCollection[i].IsCleaningEnable,
                    IsToolLifeEnable = m_toolLifeCfg.TLCollection[i].IsToolLifeEnable,
                });
            }
            tmr_UpdateToolLife = new Timer();
            tmr_UpdateToolLife.Interval = 100;
            tmr_UpdateToolLife.Elapsed += UpdateToolLifeCnt;

            ResetCleaningCommand = new DelegateCommand<ToolLifeParameter>(ResetCleaningCnt);
            ResetTLCommand = new DelegateCommand<ToolLifeParameter>(ResetToolLifeCnt);
            ResetAllCmd = new DelegateCommand(ResetAllCounter);
        }
        #endregion

        #region Method
        private void ResetCleaningCnt(ToolLifeParameter tLParam)
        {
            ButtonResult dialogResult = m_ShowDialog.Show(DialogIcon.Question, GetDialogTableValue("AskReset") + " " + tLParam.ToolName + " " + GetDialogTableValue("CleaningCounter"));
            if (dialogResult == ButtonResult.Yes)
            {
                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("User")} {m_CurrentUser.Username} {GetStringTableValue("Reset")} {GetStringTableValue("CleaningCounter")} : {tLParam.ToolName}, {GetStringTableValue("PrevValue")} : {m_toolLifeCfg.TLCollection[tLParam.ID].CleaningValue}" });

                m_toolLifeCfg.TLCollection[tLParam.ID].CleaningValue = 0;
                ToolLifeCollection[tLParam.ID].CleaningValue = m_toolLifeCfg.TLCollection[tLParam.ID].CleaningValue;
                ToolLifeConfig.Save(m_toolLifeCfg);

                m_EventAggregator.GetEvent<RefreshToolLife>().Publish();
            }
        }

        private void ResetToolLifeCnt(ToolLifeParameter tLParam)
        {
            ButtonResult dialogResult = m_ShowDialog.Show(DialogIcon.Question, GetDialogTableValue("AskReset") + " " + tLParam.ToolName + " " + GetDialogTableValue("ToolLifeCounter"));
            if (dialogResult == ButtonResult.Yes)
            {
                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("User")} {m_CurrentUser.Username} {GetStringTableValue("Reset")} {GetDialogTableValue("ToolLifeCounter")} : {tLParam.ToolName}, {GetStringTableValue("PrevValue")} : {m_toolLifeCfg.TLCollection[tLParam.ID].ToolLifeValue}" });

                m_toolLifeCfg.TLCollection[tLParam.ID].ToolLifeValue = 0;
                ToolLifeCollection[tLParam.ID].ToolLifeValue = m_toolLifeCfg.TLCollection[tLParam.ID].ToolLifeValue;
                ToolLifeConfig.Save(m_toolLifeCfg);

                m_EventAggregator.GetEvent<RefreshToolLife>().Publish();
            }
        }

        private void ResetAllCounter()
        {
            ButtonResult dialogResult = m_ShowDialog.Show(DialogIcon.Question, GetDialogTableValue("AskResetAllCounter"));
            if (dialogResult == ButtonResult.Yes)
            {
                for (int i = 0; i < m_toolLifeCfg.TLCollection.Count; i++)
                {
                    m_toolLifeCfg.TLCollection[i].CleaningValue = 0;
                    m_toolLifeCfg.TLCollection[i].ToolLifeValue = 0;
                    ToolLifeCollection[i].CleaningValue = m_toolLifeCfg.TLCollection[i].CleaningValue;
                    ToolLifeCollection[i].ToolLifeValue = m_toolLifeCfg.TLCollection[i].ToolLifeValue;
                }
                ToolLifeConfig.Save(m_toolLifeCfg);

                m_EventAggregator.GetEvent<RefreshToolLife>().Publish();

                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("User")} {m_CurrentUser.Username} {GetStringTableValue("Reset")} {GetStringTableValue("AllToolLifeCounter")}" });
            }
        }

        public void DataGridCellChangedMethod(object sender, DataGridRowEditEndingEventArgs e)
        {
            DataGrid datagrid = sender as DataGrid;
            int rowIndex = e.Row.GetIndex();

            if (datagrid.CurrentColumn != null)
            {
                if (datagrid.CurrentColumn.DisplayIndex == CleaningCol)
                {
                    ButtonResult dialogResult = m_ShowDialog.Show(DialogIcon.Question, GetDialogTableValue("AskUpdate") + " [" + ToolLifeCollection[rowIndex].ToolName + "] " + GetDialogTableValue("MaxCleaningCounter"));
                    if (dialogResult == ButtonResult.Yes)
                    {
                        ToolLifeCollection[rowIndex].PrevMaxCleaning = ToolLifeCollection[rowIndex].MaxCleaning;

                        m_toolLifeCfg.TLCollection[rowIndex].MaxCleaning = ToolLifeCollection[rowIndex].MaxCleaning;

                        ToolLifeConfig.Save(m_toolLifeCfg);
                    }
                    else
                    {
                        ToolLifeCollection[rowIndex].MaxCleaning = ToolLifeCollection[rowIndex].PrevMaxCleaning;
                    }
                }
                else if (datagrid.CurrentColumn.DisplayIndex == ToolLifeCol)
                {
                    ButtonResult dialogResult = m_ShowDialog.Show(DialogIcon.Question, GetDialogTableValue("AskUpdate") + " [" + ToolLifeCollection[rowIndex].ToolName + "] " + GetDialogTableValue("ToolMaxCounter"));
                    if (dialogResult == ButtonResult.Yes)
                    {
                        ToolLifeCollection[rowIndex].PrevMaxToolLife = ToolLifeCollection[rowIndex].MaxToolLife;

                        m_toolLifeCfg.TLCollection[rowIndex].MaxToolLife = ToolLifeCollection[rowIndex].MaxToolLife;

                        ToolLifeConfig.Save(m_toolLifeCfg);
                    }
                    else
                    {
                        ToolLifeCollection[rowIndex].MaxToolLife = ToolLifeCollection[rowIndex].PrevMaxToolLife;
                    }
                }
            }
        }
        #endregion

        #region Event
        public override void OnCultureChanged()
        {
            Title = GetStringTableValue("ToolLifeCycle");
        }

        private void UpdateToolLifeCnt(object sender, EventArgs e)
        {
            ToolLifeConfig toolLifeCfg = ToolLifeConfig.Open(m_SystemConfig.CounterCfgRef[CounterCFG.ToolLife].Reference);

            for (int i = 0; i < toolLifeCfg.TLCollection.Count; i++)
            {
                ToolLifeCollection[i].CleaningValue = toolLifeCfg.TLCollection[i].CleaningValue;
                ToolLifeCollection[i].MaxCleaning = toolLifeCfg.TLCollection[i].MaxCleaning;
                ToolLifeCollection[i].ToolLifeValue = toolLifeCfg.TLCollection[i].ToolLifeValue;
                ToolLifeCollection[i].MaxToolLife = toolLifeCfg.TLCollection[i].MaxToolLife;
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
            tmr_UpdateToolLife.Stop();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (Global.MachineStatus == MachineStateType.Running)
            {
                tmr_UpdateToolLife.Start();
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
