using ConfigManager;
using DialogManager.WarningMsg;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using GreatechApp.Services.Utilities;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DialogManager
{
    public class ErrorOperation : BindableBase, IError
    {
        #region Variables
        public WarningMessageView warningMessageView;
        private static object m_SyncRaiseError = new object();

        private bool IsErrorPrompting = false;
        private bool IsWarningPrompting = false;
        private SQID m_SQID;
        public SQID SeqName
        {
            get { return m_SQID; }
            set { m_SQID = value; }
        }

        private List<AlarmParameter> ErrorList { get; set; }

        IEventAggregator m_EventAggregator;
        SystemConfig m_SysConfig;
        ShowDialog showDialog;
        CultureResources m_CultureResources;
        #endregion

        #region Constructor

        public ErrorOperation(IEventAggregator eventAggregator, SystemConfig sysConfig, IDialogService dialogService, CultureResources cultureResources)
        {
            m_SysConfig = sysConfig;
            m_EventAggregator = eventAggregator;
            m_CultureResources = cultureResources;

            showDialog = new ShowDialog(dialogService, cultureResources);
            ErrorList = new List<AlarmParameter>();
            m_EventAggregator.GetEvent<WarningMsgOperation>().Subscribe(OnWarningMsgOperation);
            m_EventAggregator.GetEvent<WarningStatusCheck>().Subscribe(OnWarningStatusCheck);
        }
        #endregion

        #region Method
        public void CloseWarning(int ErrorCode, SQID seqName)
        {
            lock (m_SyncRaiseError)
            {
                // Load Error Library
                ErrorConfig errorConfig = ErrorConfig.Open(m_SysConfig.SeqCfgRef[(int)seqName].ErrLibPath, m_SysConfig.SeqCfgRef[(int)seqName].ErrLib);
                if (ErrorList.Any(E => E.ErrorCode == errorConfig.ErrTable[ErrorCode].Code && E.Module == seqName))
                {
                    AlarmParameter Alarm = new AlarmParameter()
                    {
                        Module = seqName,
                        ErrorCode = errorConfig.ErrTable[ErrorCode].Code,
                        Station = errorConfig.ErrTable[ErrorCode].Station,
                        Causes = errorConfig.ErrTable[ErrorCode].Cause,
                        Recovery = errorConfig.ErrTable[ErrorCode].Recovery,
                        AlarmType = m_CultureResources.GetStringTable().GetKey(errorConfig.ErrTable[ErrorCode].AlarmType),
                    };

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ErrorList.Remove(ErrorList.Where(key => key.ErrorCode == Alarm.ErrorCode && key.Module == (SQID)Enum.Parse(typeof(SQID), Alarm.Station)).FirstOrDefault());
                        warningMessageView.RemoveWarning(Alarm);
                    });
                }

                if (!ErrorList.Any(key => key.AlarmType.Equals("Warning", StringComparison.CurrentCultureIgnoreCase)))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        warningMessageView.Close();
                        IsWarningPrompting = false;
                    });
                }
            }
        }

        public void RaiseError(int ErrorCode, SQID seqName)
        {
            lock (m_SyncRaiseError)
            {
                // Load Error Library
                ErrorConfig errorConfig = ErrorConfig.Open(m_SysConfig.SeqCfgRef[(int)seqName].ErrLibPath, m_SysConfig.SeqCfgRef[(int)seqName].ErrLib);

                // Check repeat error raised
                if (!ErrorList.Any(E => E.ErrorCode == errorConfig.ErrTable[ErrorCode].Code && E.Module == seqName))
                {
                    AlarmParameter Alarm = new AlarmParameter()
                    {
                        Module = seqName,
                        Date = DateTime.Now,
                        ErrorCode = errorConfig.ErrTable[ErrorCode].Code,
                        Station = errorConfig.ErrTable[ErrorCode].Station,
                        Causes = errorConfig.ErrTable[ErrorCode].Cause,
                        Recovery = errorConfig.ErrTable[ErrorCode].Recovery,
                        AlarmType = m_CultureResources.GetStringTable().GetKey(errorConfig.ErrTable[ErrorCode].AlarmType),
                        RetestDefault = errorConfig.ErrTable[ErrorCode].RetestDefault,
                        RetestOption = errorConfig.ErrTable[ErrorCode].RetestOption,
                        IsStopPage = errorConfig.ErrTable[ErrorCode].IsStoppage,
                    };
                    // Add Error Detail
                    ErrorList.Add(Alarm);

                    if (Alarm.AlarmType.Equals("Error"))
                    {
                        if (!IsErrorPrompting)
                        {
                            IsErrorPrompting = true;
                            PromptError();
                        }
                    }
                    else if (Alarm.AlarmType.Equals("Warning"))
                    {
                        if (Global.MachineStatus != MachineStateType.CriticalAlarm && Global.MachineStatus != MachineStateType.Initializing && Global.MachineStatus != MachineStateType.InitFail)
                        {
                            m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Warning);
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (!IsWarningPrompting)
                                {
                                    IsWarningPrompting = true;

                                    warningMessageView = new WarningMessageView();
                                    warningMessageView.Show();
                                }
                                warningMessageView.AddWarning(Alarm);
                            });
                        }
                    }
                }
            }
        }

        private void PromptError()
        {
            // If error raise from Critical Scan or Initializing, skip this
            if (Global.MachineStatus != MachineStateType.CriticalAlarm && Global.MachineStatus != MachineStateType.Initializing && Global.MachineStatus != MachineStateType.InitFail)
            {
                // If alarm type is error, stop the machine
                Global.MachineStatus = MachineStateType.Error;
                Global.SeqStop = true;
                m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Error);
            }

            AlarmParameter alarm = ErrorList.Where(key => key.AlarmType.Equals("Error", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

            ButtonResult buttonResult = showDialog.ErrorShow(alarm);
            if (buttonResult == ButtonResult.OK)
            {
                ErrorList.Remove(alarm);
            }

            if (ErrorList.Where(key => key.AlarmType.Equals("Error", StringComparison.CurrentCultureIgnoreCase)).Count() == 0)
            {
                IsErrorPrompting = false;
                if (Global.MachineStatus != MachineStateType.CriticalAlarm)
				{
                    m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Recovery);
                }
            }
            else
            {
                PromptError();
            }
        }
        #endregion

        #region Event
        private void OnWarningMsgOperation(WarningMsgOperation alarm)
        {
            ErrorList.Remove(ErrorList.Where(key => key.ErrorCode == alarm.ErrorCode && key.Module ==(SQID)Enum.Parse(typeof(SQID), alarm.Station)).FirstOrDefault());
            if(!ErrorList.Any(key=> key.AlarmType.Equals("Warning", StringComparison.CurrentCultureIgnoreCase)))
            {
                warningMessageView.Close();
                IsWarningPrompting = false;
            }
        }
        private void OnWarningStatusCheck()
        {
            if (ErrorList.Any(key => key.AlarmType.Equals("Warning", StringComparison.CurrentCultureIgnoreCase)))
            {
                if (Global.MachineStatus != MachineStateType.CriticalAlarm && Global.MachineStatus != MachineStateType.Initializing && Global.MachineStatus != MachineStateType.InitFail)
                {
                    m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Warning);
                }
            }
        }
        #endregion
    }
}
