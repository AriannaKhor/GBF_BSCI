using Prism.Mvvm;
using Prism.Services.Dialogs;
using Prism.Commands;
using System;
using System.Windows;
using GreatechApp.Core.Enums;
using System.Windows.Media.Imaging;
using Prism.Events;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using GreatechApp.Core.Events;
using System.Windows.Threading;
using IOManager;
using GreatechApp.Core.Cultures;
using GreatechApp.Services.UserServices;
using System.Windows.Controls;
using Prism.Ioc;
using System.Globalization;

namespace DialogManager.ErrorMsg
{
    public class ErrVerificationViewModel : BindableBase, IDialogAware
    {
        #region Variable

        private AuthService m_AuthService;
        private readonly IEventAggregator m_EventAggregator;
        private readonly ISQLOperation m_SQLOperation;
        private readonly IBaseIO m_IO;
        public IShowDialog m_ShowDialog;
        private CultureResources m_CultureResources;
        public IUser m_CurrentUser;
        private ResultsDatalog m_resultsDatalog = new ResultsDatalog();

        private string m_remarks;
        public string remarks
        {
            get { return m_remarks; }
            set
            {
                SetProperty(ref m_remarks, value);
                Global.Remarks = value;
            }
        }

        private BitmapImage m_Image;
        public BitmapImage Image
        {
            get { return m_Image; }
            set { SetProperty(ref m_Image, value); }
        }

        private string m_UserID;
        public string UserID
        {
            get { return m_UserID; }
            set { SetProperty(ref m_UserID, value); }
        }

        private bool m_IsUserIDFocused;
        public bool IsUserIDFocused
        {
            get { return m_IsUserIDFocused; }
            set { SetProperty(ref m_IsUserIDFocused, value); }
        }

        private string m_Password;
        public string Password
        {
            get { return m_Password; }
            set { SetProperty(ref m_Password, value); }
        }

        private bool m_IsMaskPass;
        public bool IsMaskPass
        {
            get { return m_IsMaskPass; }
            set { SetProperty(ref m_IsMaskPass, value); }
        }
        private string m_ErrorMsg;
        public string ErrorMsg
        {
            get { return m_ErrorMsg; }
            set { SetProperty(ref m_ErrorMsg, value); }
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

        private string m_title = "";
        public string Title
        {
            get { return m_title; }
            set { SetProperty(ref m_title, value); }
        }

        private bool m_IsAllowStop;
        public bool IsAllowStop
        {
            get { return m_IsAllowStop; }
            set
            {
                SetProperty(ref m_IsAllowStop, value);
            }
        }
        private Visibility m_SkipTray = Visibility.Collapsed;
        public Visibility SkipTray
        {
            get { return m_SkipTray; }
            set { SetProperty(ref m_SkipTray, value); }
        }


        private Visibility m_Error = Visibility.Collapsed;
        public Visibility Error
        {
            get { return m_Error; }
            set { SetProperty(ref m_Error, value); }
        }

        private Visibility m_SkipRetestVis = Visibility.Collapsed;
        public Visibility SkipRetestVis
        {
            get { return m_SkipRetestVis; }
            set { SetProperty(ref m_SkipRetestVis, value); }
        }

        private Visibility m_YesSituation = Visibility.Collapsed;
        public Visibility YesSituation
        {
            get { return m_YesSituation; }
            set { SetProperty(ref m_YesSituation, value); }
        }

        private bool m_btnYesEnable = false;
        public bool btnYesEnable
        {
            get { return m_btnYesEnable; }
            set { SetProperty(ref m_btnYesEnable, value); }
        }

        private Visibility m_NoSituation = Visibility.Collapsed;
        public Visibility NoSituation
        {
            get { return m_NoSituation; }
            set { SetProperty(ref m_NoSituation, value); }
        }

        private string m_ErrMessage;
        public string ErrMessage
        {
            get { return m_ErrMessage; }
            set { SetProperty(ref m_ErrMessage, value); }
        }

        private bool m_IsSkipRetest;
        public bool IsSkipRetest
        {
            get { return m_IsSkipRetest; }
            set { SetProperty(ref m_IsSkipRetest, value); }
        }

        public event Action<IDialogResult> RequestClose;

        private AlarmParameter m_AlarmDetail;
        public AlarmParameter AlarmDetail
        {
            get { return m_AlarmDetail; }
            set
            {
                SetProperty(ref m_AlarmDetail, value);
            }
        }

        private string m_RemarksMessage;
        public string RemarksMessage
        {
            get { return m_RemarksMessage; }
            set
            {
                SetProperty(ref m_RemarksMessage, value);
                Global.Remarks = value;
            }
        }

        public DelegateCommand<string> OperationCommand { get; private set; }
        public DelegateCommand EndLotCommand { get; set; }

        private readonly IDialogService m_DialogService;
        public DelegateCommand<object> VerificationCommand { get; private set; }

        #endregion

        #region Constructor
        public ErrVerificationViewModel(IDialogService dialogService, IEventAggregator eventAggregator, AuthService authService, ISQLOperation sqlOperation, IBaseIO baseIO, CultureResources cultureResources)
        {
            m_EventAggregator = eventAggregator;
            m_DialogService = dialogService;
            m_SQLOperation = sqlOperation;
            m_IO = baseIO;
            m_CultureResources = cultureResources;
            m_AuthService = authService;

            VerificationCommand = new DelegateCommand<object>(VerificationMethod);
            m_CurrentUser = (DefaultUser)ContainerLocator.Container.Resolve(typeof(DefaultUser));
            m_CultureResources = (CultureResources)ContainerLocator.Container.Resolve(typeof(CultureResources));
            m_ShowDialog = (IShowDialog)ContainerLocator.Container.Resolve(typeof(IShowDialog));
            OperationCommand = new DelegateCommand<string>(OperationMethod);
            AlarmDetail = new AlarmParameter();
        }
        #endregion

        #region Event
        private void VerificationMethod(object value)
        {
            var passwordBox = value as PasswordBox;
            var password = passwordBox.Password;

            if (m_AuthService.Authenticate(UserID, password))
            {
                var currentUserLevel = m_AuthService.CurrentUser.UserLevel;
                if ((currentUserLevel == ACL.UserLevel.Admin || currentUserLevel == ACL.UserLevel.Engineer || currentUserLevel == ACL.UserLevel.Technician) && (remarks != null || remarks != string.Empty))
                {
                    if (remarks != null && remarks != string.Empty)
                    {
                        btnYesEnable = true;
                        ErrMessage = "Valid Login";
                        Global.CurrentApprovalLevel = currentUserLevel.ToString();
                        SaveGlobalResult();
                        m_EventAggregator.GetEvent<ResultLoggingEvent>().Publish(m_resultsDatalog);
                        m_resultsDatalog.ClearAll();
                    }
                    else
                    {
                        btnYesEnable = false;
                        ErrMessage = "Enter Remarks";
                    }
                }
                else
                {
                    btnYesEnable = false;
                    if (remarks == null || remarks == string.Empty)
                    {
                        RemarksMessage = "Enter remarks";
                    }
                    ErrMessage = m_CultureResources.GetStringValue("InvalidLoginInfo");
                }
            }
            else
            {
                ErrMessage = m_CultureResources.GetStringValue("InvalidLoginInfo");
            }
        }
        #endregion

        #region Method
        private void Reset()
        {
            m_EventAggregator.GetEvent<CheckOperation>().Publish(true);
            m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Running);
            CloseDialog("");
        }

        private void OperationMethod(string Command)
        {
            if (Command == "Continue")
            {
                Reset();
                m_EventAggregator.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcContErrRtn });
            }

            else if (Command == "EndLot")
            {
                m_EventAggregator.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.EndLotComp });
               // m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = "Endlot" + Global.CurrentBatchNum });
                m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Idle);
                ResetCounter();
            }
            CloseDialog("");
        }

        private void SaveGlobalResult()
        {
            if (Global.CurrentLotBatchNum == null || Global.CurrentLotBatchNum == String.Empty)
            {
               // Global.CurrentLotBatchNum = Global.CurrentBatchNum;
            }

            if (Global.OverallResult == "OK")
            {
                Global.ErrorMsg = "N/A";
                Global.Remarks = "N/A";
                Global.CurrentApprovalLevel = "N/A";
            }

            m_resultsDatalog.UserId = Global.UserId;
            m_resultsDatalog.UserLvl = Global.UserLvl;
            DateTime currentTime = DateTime.Now;
            DateTimeFormatInfo dateFormat = new DateTimeFormatInfo();
            dateFormat.ShortDatePattern = "dd-MM-yyyy";
            m_resultsDatalog.Date = currentTime.ToString("d", dateFormat);
            m_resultsDatalog.Time = currentTime.ToString("HH:mm:ss.fff", DateTimeFormatInfo.InvariantInfo);
            m_resultsDatalog.Timestamp = m_resultsDatalog.Date + " | " + m_resultsDatalog.Time;
            m_resultsDatalog.OverallResult = Global.OverallResult;
            m_resultsDatalog.TopVision = inspectiontype.TopVision.ToString();
            m_resultsDatalog.VisTotalPrdQty = Global.VisProductQuantity;
            m_resultsDatalog.VisCorrectOrient = Global.VisProductCrtOrientation;
            m_resultsDatalog.VisWrongOrient = Global.VisProductWrgOrientation;
            m_resultsDatalog.ErrorMessage = Global.ErrorMsg;
            m_resultsDatalog.Remarks = Global.Remarks;
            m_resultsDatalog.ApprovedBy = Global.CurrentApprovalLevel;
        }

        private void ResetCounter()
        {
            #region Code Reader
            //Global.CurrentContainerNum = String.Empty;
            //Global.CurrentBatchQuantity = 0;
            //Global.AccumulateCurrentBatchQuantity = 0;
            //Global.CurrentBoxQuantity = 0;
            //Global.CurrentBatchNum = String.Empty;
            Global.CurrentLotBatchNum = String.Empty;
            Global.LotInitialBatchNo = String.Empty;
            Global.LotInitialTotalBatchQuantity = 0;
            //Global.CurrentBoxCount = 0;
            //Global.CodeReaderResult = resultstatus.PendingResult.ToString();
            #endregion

            #region Top Vision
            Global.VisProductQuantity = 0f;
            Global.VisProductCrtOrientation = 0f;
            Global.VisProductWrgOrientation = 0f;
            Global.VisInspectResult = resultstatus.PendingResult.ToString();
            m_EventAggregator.GetEvent<TopVisionResultEvent>().Publish();
            #endregion

            #region Error
            Global.ErrorMsg = string.Empty;
            #endregion
        }

        protected virtual void CloseDialog(string parameter)
        {
            RaiseRequestClose(new DialogResult(ButtonResult.OK));
        }
        #endregion

        #region Properties
        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        public virtual bool CanCloseDialog()
        {
            return true;
        }

        public virtual void OnDialogClosed()
        {

        }

        public virtual void OnDialogOpened(IDialogParameters parameters)
        {
            string[] split = parameters.GetValue<string>("message").Split(';');

            AlarmDetail.Module = (SQID)Enum.Parse(typeof(SQID), split[8]);
            AlarmDetail.Date = DateTime.Now;
            AlarmDetail.ErrorCode = Convert.ToInt32(split[0]);
            AlarmDetail.Station = split[1];
            AlarmDetail.Causes = split[2];
            AlarmDetail.Recovery = split[3];
            AlarmDetail.AlarmType = split[4];
            AlarmDetail.RetestDefault = Convert.ToBoolean(split[5]);
            AlarmDetail.RetestOption = Convert.ToBoolean(split[6]);
            AlarmDetail.IsStopPage = Convert.ToBoolean(split[7]);

            Station = split[8];
            ErrorMsg = split[2].Trim();
            Action = split[3];

            if (Convert.ToBoolean(split[5]))
            {
                IsSkipRetest = false;
            }
            else
            {
                IsSkipRetest = true;
            }

            if (Convert.ToBoolean(split[6]))
            {
                SkipRetestVis = Visibility.Visible;
            }
            else
            {
                SkipRetestVis = Visibility.Collapsed;
            }
        }
        #endregion
    }
}
