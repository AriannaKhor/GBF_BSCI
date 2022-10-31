using Prism.Mvvm;
using Prism.Commands;
using GreatechApp.Core.Enums;
using Prism.Events;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Variable;
using GreatechApp.Core.Events;
using IOManager;
using GreatechApp.Core.Cultures;
using System.Linq;
using System.Collections.Generic;
using GreatechApp.Core.Modal;
using System;

namespace DialogManager.WarningMsg
{
    public class WarningMessageViewModel : BindableBase
    {
        #region Variable
        private readonly IEventAggregator m_EventAggregator;
        private CultureResources m_CultureResources;
        public DelegateCommand OKCommand { get; set; }
        public DelegateCommand<string> NavigateCommand { get; set; }
        private int ListIndex { get; set; }

        private string m_WarningMsg;
        public string WarningMsg
        {
            get { return m_WarningMsg; }
            set { SetProperty(ref m_WarningMsg, value); }
        }

        private string m_Station;
        public string Station
        {
            get { return m_Station; }
            set { SetProperty(ref m_Station, value); }
        }

        private string m_Action;
        public string Action
        {
            get { return m_Action; }
            set { SetProperty(ref m_Action, value); }
        }

        private int m_ErrorCode;

        public int ErrorCode
        {
            get { return m_ErrorCode; }
            set { m_ErrorCode = value; }
        }

        private List<AlarmParameter> m_WarningList;

        public List<AlarmParameter> WarningList
        {
            get { return m_WarningList; }
            set { SetProperty(ref m_WarningList, value); }
        }

        private int m_SelectedWarningIndex;
        public int SelectedWarningIndex
        {
            get { return m_SelectedWarningIndex; }
            set 
            { 
                SetProperty(ref m_SelectedWarningIndex, value);
                ListIndex = value - 1;
                IsPreviousEnable = value != 1;
                IsNextEnable = value != WarningList.Count();
            }
        }

        private int m_TotalWarning;
        public int TotalWarning
        {
            get { return m_TotalWarning; }
            set { SetProperty(ref m_TotalWarning, value); }
        }

        private bool m_IsNextEnable = false;

        public bool IsNextEnable
        {
            get { return m_IsNextEnable; }
            set { SetProperty(ref m_IsNextEnable, value); }
        }

        private bool m_IsPreviousEnable = false;

        public bool IsPreviousEnable
        {
            get { return m_IsPreviousEnable; }
            set { SetProperty(ref m_IsPreviousEnable, value); }
        }
        #endregion

        #region Constructor
        public WarningMessageViewModel(IEventAggregator eventAggregator, CultureResources cultureResources)
        {
            m_EventAggregator = eventAggregator;
            m_CultureResources = cultureResources;

            OKCommand = new DelegateCommand(OKOperation);
            NavigateCommand = new DelegateCommand<string>(OnNavigateOperation);
            WarningList = new List<AlarmParameter>();
            SelectedWarningIndex = 1;
        }
        #endregion

        #region Method
        public void RemoveWarning(AlarmParameter warningParameter)
        {
            int currentIndex = WarningList.IndexOf(WarningList.Where(key=> key.ErrorCode == warningParameter.ErrorCode && key.Module == warningParameter.Module).FirstOrDefault());
            WarningList.RemoveAt(currentIndex);
            if (WarningList.Count == 0)
            {
                if (Global.MachineStatus == MachineStateType.Warning)
                {
                    m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Running);
                }
            }
            else
            {
                TotalWarning = WarningList.Count();
                if (currentIndex <= ListIndex)
                {
                    if (SelectedWarningIndex != 1)
                        SelectedWarningIndex--;
                }
                ShowWarning();
            }
        }

        public void AddWarning(AlarmParameter warningParameter)
        {
            WarningList.Add(warningParameter);
            TotalWarning = WarningList.Count();
            ShowWarning();

            if (TotalWarning > 1)
                IsNextEnable = true;

            m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Warning, MsgText = $"{m_CultureResources.GetStringValue("Warning")}, " +
                $"{m_CultureResources.GetStringValue("Station")} : { warningParameter.Module}, {m_CultureResources.GetStringValue("Warning")} : { warningParameter.Causes}" });
        }
        public void ShowWarning()
        {
            Station = WarningList[ListIndex].Module.ToString();
            Action = WarningList[ListIndex].Recovery;
            WarningMsg = WarningList[ListIndex].Causes.Split(':').LastOrDefault();
            ErrorCode = WarningList[ListIndex].ErrorCode;
            IsPreviousEnable = SelectedWarningIndex != 1;
            IsNextEnable = SelectedWarningIndex != WarningList.Count();
        }

        #region Event
        private void OnNavigateOperation(string instruction)
        {
            if(instruction == "Previous")
                SelectedWarningIndex--;
            else if (instruction == "Next")
                SelectedWarningIndex++;

            ShowWarning();
        }

        private void OKOperation()
        {
            m_EventAggregator.GetEvent<WarningMsgOperation>().Publish(new WarningMsgOperation { ErrorCode = WarningList[ListIndex].ErrorCode, Station = WarningList[ListIndex].Module.ToString() });
            WarningList.RemoveAt(ListIndex);

            if(WarningList.Count == 0)
            {
                if (Global.MachineStatus == MachineStateType.Warning)
                {
                    m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Running);
                }
            }
            else
            {
                TotalWarning = WarningList.Count();
                if (SelectedWarningIndex != 1)
                    SelectedWarningIndex--;
                ShowWarning();
            }
        }
        #endregion

        #endregion
    }
}
