using ConfigManager;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Modal;
using GreatechApp.Services.UserServices;
using GreatechApp.Services.Utilities;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using UIModule.MainPanel;

namespace UIModule.StandardViews
{
    public class SeqParameterViewModel : BaseUIViewModel
    {
        #region Variables
        private ObservableCollection<SQID> m_SequenceList;
        public ObservableCollection<SQID> SequenceList
        {
            get { return m_SequenceList; }
            set { SetProperty(ref m_SequenceList, value); }
        }

        private SQID m_SelectedSequence;
        public SQID SelectedSequence
        {
            get { return m_SelectedSequence; }
            set 
            { 
                SetProperty(ref m_SelectedSequence, value);
                GetConfig();
            }
        }

        private string m_TabPageHeader;
        public string TabPageHeader 
        {
            get { return m_TabPageHeader; }
            set { SetProperty(ref m_TabPageHeader, value); }
        }

        private ObservableCollection<SeqConfigParameter> m_ErrorTimers;
        public ObservableCollection<SeqConfigParameter> ErrorTimers
        {
            get { return m_ErrorTimers; }
            set { SetProperty(ref m_ErrorTimers, value); }
        }

        private ObservableCollection<SeqConfigParameter> m_DelayTimers;
        public ObservableCollection<SeqConfigParameter> DelayTimers
        {
            get { return m_DelayTimers; }
            set { SetProperty(ref m_DelayTimers, value); }
        }

        private ObservableCollection<SeqConfigParameter> m_Counters;
        public ObservableCollection<SeqConfigParameter> Counters
        {
            get { return m_Counters; }
            set { SetProperty(ref m_Counters, value); }
        }

        private ObservableCollection<SeqConfigParameter> m_Options;
        public ObservableCollection<SeqConfigParameter> Options
        {
            get { return m_Options; }
            set { SetProperty(ref m_Options, value); }
        }
        #endregion

        public SeqParameterViewModel()
        {
            TabPageHeader = GetStringTableValue("ModuleParameter");

            SequenceList = new ObservableCollection<SQID>();
            for (int i = 1; i < Enum.GetNames(typeof(SQID)).Length - 1; i++)
            {
                SequenceList.Add((SQID)i);
            }

            SelectedSequence = SequenceList[0];
        }

        private void GetConfig()
        {
            SeqConfig seqcfg = SeqConfig.Open(m_SystemConfig.SeqCfgRef[(int)SelectedSequence].Reference);

            ErrorTimers = new ObservableCollection<SeqConfigParameter>();
            DelayTimers = new ObservableCollection<SeqConfigParameter>();
            Counters = new ObservableCollection<SeqConfigParameter>();
            Options = new ObservableCollection<SeqConfigParameter>();

            for (int i = 0; i < seqcfg.Err.Count; i++)
            {
                if(seqcfg.Err[i].IsVisible)
                {
                    ErrorTimers.Add(new SeqConfigParameter()
                    {
                        Id = seqcfg.Err[i].ID,
                        Description = seqcfg.Err[i].Description,
                        Value = seqcfg.Err[i].TimeOut,
                        Min = seqcfg.Err[i].Min,
                        Max = seqcfg.Err[i].Max,
                        ToolTip = seqcfg.Err[i].Tooltip,

                        PrevDescription = seqcfg.Err[i].Description,
                        PrevValue = seqcfg.Err[i].TimeOut,
                        PrevMin = seqcfg.Err[i].Min,
                        PrevMax = seqcfg.Err[i].Max,
                    });
                }
              
            }

            for (int i = 0; i < seqcfg.Delay.Count; i++)
            {
                if (seqcfg.Delay[i].IsVisible)
                {
                    DelayTimers.Add(new SeqConfigParameter()
                    {
                        Id = seqcfg.Delay[i].ID,
                        Description = seqcfg.Delay[i].Description,
                        Value = seqcfg.Delay[i].TimeOut,
                        Min = seqcfg.Delay[i].Min,
                        Max = seqcfg.Delay[i].Max,
                        ToolTip = seqcfg.Delay[i].Tooltip,

                        PrevDescription = seqcfg.Delay[i].Description,
                        PrevValue = seqcfg.Delay[i].TimeOut,
                        PrevMin = seqcfg.Delay[i].Min,
                        PrevMax = seqcfg.Delay[i].Max,
                    });
                }
            }

            for (int i = 0; i < seqcfg.Counter.Count; i++)
            {
                if (seqcfg.Counter[i].IsVisible)
                {
                    Counters.Add(new SeqConfigParameter()
                    {
                        Id = seqcfg.Counter[i].ID,
                        Description = seqcfg.Counter[i].Description,
                        Value = seqcfg.Counter[i].Value,
                        Min = seqcfg.Counter[i].Min,
                        Max = seqcfg.Counter[i].Max,
                        ToolTip = seqcfg.Counter[i].Tooltip,

                        PrevDescription = seqcfg.Counter[i].Description,
                        PrevValue = seqcfg.Counter[i].Value,
                        PrevMin = seqcfg.Counter[i].Min,
                        PrevMax = seqcfg.Counter[i].Max,
                    });
                }
            }

            for(int i = 0; i < seqcfg.Option.Count; i++)
            {
                if (seqcfg.Option[i].IsVisible)
                {
                    ObservableCollection<OptionMember> member = new ObservableCollection<OptionMember>();
                    for (int j = 0; j < seqcfg.Option[i].ChoiceCollection.Count; j++)
                    {
                        member.Add(new OptionMember()
                        {
                            ChoiceId = seqcfg.Option[i].ChoiceCollection[j].ChoiceID,
                            ChoiceDesc = seqcfg.Option[i].ChoiceCollection[j].ChoiceDesc
                        });
                    }
                
                    Options.Add(new SeqConfigParameter()
                    {
                        Id = seqcfg.Option[i].ID,
                        Description = seqcfg.Option[i].Description,
                        OptionMemberCollection = member,
                        SelectedOption = member[seqcfg.Option[i].Value],
                        ToolTip = seqcfg.Option[i].Tooltip,

                        PrevDescription = seqcfg.Option[i].Description,
                        PrevValue = seqcfg.Option[i].Value,
                        PrevSelectedOption = member[seqcfg.Option[i].Value]
                    });
                }
            }
        }

        #region Method
        public void DataGridCellChangedMethod(object sender, DataGridRowEditEndingEventArgs e)
        {
            DataGrid datagrid = sender as DataGrid;
            int rowIndex = e.Row.GetIndex();

            ObservableCollection<SeqConfigParameter> configParam = new ObservableCollection<SeqConfigParameter>();
            SeqConfig seqcfg = SeqConfig.Open(m_SystemConfig.SeqCfgRef[(int)SelectedSequence].Reference);

            switch (datagrid.Name)
            {
                case "ErrorTimers":
                    configParam = ErrorTimers;
                    break;

                case "DelayTimers":
                    configParam = DelayTimers;
                    break;

                case "Counters":
                    configParam = Counters;
                    break;

                case "Options":
                    configParam = Options;
                    break;
            }

            ButtonResult dialogResult = m_ShowDialog.Show(DialogIcon.Question, $"{GetDialogTableValue("AskSaveChange")} {datagrid.Name} ?");
            if (dialogResult == ButtonResult.Yes)
            {
                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("User")} {m_CurrentUser.Username} {GetStringTableValue("Update")} [{SelectedSequence}] {GetStringTableValue("SeqParameter")} : {configParam[rowIndex].Description}, {GetStringTableValue("PrevValue")} : {configParam[rowIndex].PrevValue} ; {GetStringTableValue("NewValue")} : {configParam[rowIndex].Value}" });

                configParam[rowIndex].PrevDescription = configParam[rowIndex].Description;
                configParam[rowIndex].PrevValue = configParam[rowIndex].Value;
                configParam[rowIndex].PrevMin = configParam[rowIndex].Min;
                configParam[rowIndex].PrevMax = configParam[rowIndex].Max;
                configParam[rowIndex].PrevSelectedOption = configParam[rowIndex].SelectedOption;

                int rowIndexId = configParam[rowIndex].Id;

                switch (datagrid.Name)
                {
                    case "ErrorTimers":
                        seqcfg.Err[rowIndexId].Description = configParam[rowIndex].Description;
                        seqcfg.Err[rowIndexId].TimeOut = configParam[rowIndex].Value;
                        seqcfg.Err[rowIndexId].Min = configParam[rowIndex].Min;
                        seqcfg.Err[rowIndexId].Max = configParam[rowIndex].Max;
                        break;

                    case "DelayTimers":
                        seqcfg.Delay[rowIndexId].Description = configParam[rowIndex].Description;
                        seqcfg.Delay[rowIndexId].TimeOut = configParam[rowIndex].Value;
                        seqcfg.Delay[rowIndexId].Min = configParam[rowIndex].Min;
                        seqcfg.Delay[rowIndexId].Max = configParam[rowIndex].Max;
                        break;

                    case "Counters":
                        seqcfg.Counter[rowIndexId].Description = configParam[rowIndex].Description;
                        seqcfg.Counter[rowIndexId].Value = (int)configParam[rowIndex].Value;
                        seqcfg.Counter[rowIndexId].Min = (int)configParam[rowIndex].Min;
                        seqcfg.Counter[rowIndexId].Max = (int)configParam[rowIndex].Max;
                        break;

                    case "Options":
                        seqcfg.Option[rowIndexId].Description = configParam[rowIndex].Description;
                        seqcfg.Option[rowIndexId].Value = configParam[rowIndex].SelectedOption.ChoiceId;
                        break;
                }

                SeqConfig.Save(seqcfg);
            }
            else if(dialogResult == ButtonResult.Cancel)
            {
                configParam[rowIndex].Description = configParam[rowIndex].PrevDescription;
                configParam[rowIndex].Value = configParam[rowIndex].PrevValue;
                configParam[rowIndex].Min = configParam[rowIndex].PrevMin;
                configParam[rowIndex].Max = configParam[rowIndex].PrevMax;
                configParam[rowIndex].SelectedOption = configParam[rowIndex].PrevSelectedOption;
            }
        }
        #endregion

        #region Event
        public override void OnCultureChanged()
        {
            TabPageHeader = GetStringTableValue("ModuleParameter");
        }
        #endregion
    }
}
