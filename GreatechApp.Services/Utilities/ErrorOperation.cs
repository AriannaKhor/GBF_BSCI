using ConfigManager;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GreatechApp.Services.Utilities
{
    public class ErrorOperation : BindableBase, IError
    {
        #region Variables
        private static object m_SyncRaiseError = new object();

        private bool IsErrorPrompting = false;
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
        }

        #endregion

        #region Method
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
                        Global.SeqStop = true;
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
                        }
                        ButtonResult buttonResult = showDialog.Show(DialogIcon.Warning, Alarm.Causes);
                        if (buttonResult == ButtonResult.OK)
                        {
                            ErrorList.Remove(ErrorList.Where(key => key.ErrorCode == Alarm.ErrorCode && key.Module == Alarm.Module).FirstOrDefault());
                        }
                    }
                }
            }
        }

        private void PromptError()
        {
            // If alarm type is error, stop the machine
            Global.SeqStop = true;

            // If error raise from Critical Scan or Initializing, skip this
            if (Global.MachineStatus != MachineStateType.CriticalAlarm && Global.MachineStatus != MachineStateType.Initializing && Global.MachineStatus != MachineStateType.InitFail)
            {
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
                m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Recovery);
            }
            else
            {
                PromptError();
            }
        }

        #endregion
    }
}
