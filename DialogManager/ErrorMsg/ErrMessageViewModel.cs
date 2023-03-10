using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using GreatechApp.Services.UserServices;
using IOManager;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DialogManager.ErrorMsg
{
    public class ErrMessageViewModel : BindableBase, IDialogAware
    {
        #region Variable
        private readonly IEventAggregator m_EventAggregator;
        private readonly ISQLOperation m_SQLOperation;
        private readonly IBaseIO m_IO;
        public IShowDialog m_ShowDialog;
        private CultureResources m_CultureResources;
        public IUser m_CurrentUser;

        private BitmapImage m_Image;
        public BitmapImage Image
        {
            get { return m_Image; }
            set { SetProperty(ref m_Image, value); }
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

        private Visibility m_SkipTray = Visibility.Collapsed;
        public Visibility SkipTray
        {
            get { return m_SkipTray; }
            set { SetProperty(ref m_SkipTray, value); }
        }

        private Visibility m_SkipRetestVis = Visibility.Collapsed;
        public Visibility SkipRetestVis
        {
            get { return m_SkipRetestVis; }
            set { SetProperty(ref m_SkipRetestVis, value); }
        }

        private Visibility m_ResetVis = Visibility.Collapsed;
        public Visibility ResetVis
        {
            get { return m_ResetVis; }
            set { SetProperty(ref m_ResetVis, value); }
        }

        private Visibility m_OkVis = Visibility.Collapsed;
        public Visibility OkVis
        {
            get { return m_OkVis; }
            set { SetProperty(ref m_OkVis, value); }
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


        private bool m_IsAllowStart;
        public bool IsAllowStart
        {
            get { return m_IsAllowStart; }
            set
            {
                SetProperty(ref m_IsAllowStart, value);
            }
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
        public DelegateCommand<string> OperationCommand { get; private set; }
        public DelegateCommand EndLotCommand { get; set; }

        private readonly IDialogService m_DialogService;

        #endregion

        #region Constructor
        public ErrMessageViewModel(IDialogService dialogService, IEventAggregator eventAggregator, ISQLOperation sqlOperation, IBaseIO baseIO, CultureResources cultureResources)
        {
            m_EventAggregator = eventAggregator;
            m_DialogService = dialogService;
            m_SQLOperation = sqlOperation;
            m_IO = baseIO;
            m_CurrentUser = (DefaultUser)ContainerLocator.Container.Resolve(typeof(DefaultUser));
            m_CultureResources = (CultureResources)ContainerLocator.Container.Resolve(typeof(CultureResources));
            m_ShowDialog = (IShowDialog)ContainerLocator.Container.Resolve(typeof(IShowDialog));

            OperationCommand = new DelegateCommand<string>(OperationMethod);
            EndLotCommand = new DelegateCommand(RaiseEndLotPopup);
            AlarmDetail = new AlarmParameter();
        }

        #endregion

        #region Access Implementation
        public IUser CurrentUser { get; }
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
            if (Command == "OK")
            {
                Reset();
            }
            else if (Command == "EndLot")
            {
                CloseDialog("");

                ButtonResult dialogResult = m_ShowDialog.Show(DialogIcon.Question, GetDialogTableValue("EndLot"), GetDialogTableValue("AskConfirmEndLot") + " " + Global.LotInitialBatchNo, ButtonResult.Yes, ButtonResult.No);

                if (dialogResult == ButtonResult.Yes) //lot ended
                {
                    if (Global.VisInspectResult == "NG") // for wrong and exceed
                    {
                        m_EventAggregator.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.EndLotComp });
                        m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = "Endlot" + Global.LotInitialBatchNo });
                        m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Idle);
                        ResetCounter();
                    }
                    //else if (Global.CodeReaderResult == "NG") //for unequal
                    //{
                    //    m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Error);
                    //    RaiseEndLotPopup();
                    //}
                    //CloseDialog("");
                }
                else if (dialogResult == ButtonResult.No) //reset and return same error
                {
                    Reset();
                    m_EventAggregator.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.ProcContErrRtn });
                }
            }
        }

        private void ResetCounter()
        {
            #region Code Reader
           
            Global.LotInitialBatchNo = String.Empty;
            Global.CurrentLotBatchNum = String.Empty;
            Global.LotInitialTotalBatchQuantity = 0;
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

        private string GetDialogTableValue(string key)
        {
            return m_CultureResources.GetDialogValue(key);
        }

        void RaiseEndLotPopup()
        {
            m_DialogService.ShowDialog(DialogList.ForcedEndLotView.ToString(),
                                      new DialogParameters($"message={""}"),
                                      null);
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
