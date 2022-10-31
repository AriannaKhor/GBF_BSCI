using ConfigManager;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Sequence;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace UIModule.MainPanel
{
    public class HomeViewModel : BaseUIViewModel
    {
        #region Variable
        #region InitStatus
        public const string GrayIcon = "/GreatechApp.Core;component/Icon/GrayIcon.png";
        public const string GreenIcon = "/GreatechApp.Core;component/Icon/GreenIcon.png";
        public const string RedIcon = "/GreatechApp.Core;component/Icon/RedIcon.png";

        private Visibility m_ShowInitState;

        public Visibility ShowInitState
        {
            get { return m_ShowInitState; }
            set { SetProperty(ref m_ShowInitState, value); }
        }

        private ObservableCollection<InitStatus> m_SeqCollecion;
        public ObservableCollection<InitStatus> SeqCollection
        {
            get { return m_SeqCollecion; }
            set { SetProperty(ref m_SeqCollecion, value); }
        }
        #endregion

        #region LotEntry
        private bool m_IsLotEntryExpand;
        public bool IsLotEntryExpand
        {
            get { return m_IsLotEntryExpand; }
            set { SetProperty(ref m_IsLotEntryExpand, value); }
        }
        #endregion

        #endregion

        #region Constructor

        public HomeViewModel()
        {
            ShowInitState = Visibility.Collapsed;
            m_EventAggregator.GetEvent<MachineOperation>().Subscribe(UpdateMachineOperation);
            m_EventAggregator.GetEvent<MachineState>().Subscribe(UpdateMachineState);
            m_EventAggregator.GetEvent<OpenLotEntryView>().Subscribe(OpenLotEntry);

            // Add machine seq into init status colection except core seq
            SeqCollection = new ObservableCollection<InitStatus>();
            for (int i = m_DelegateSeq.CoreSeqNum; i < m_DelegateSeq.TotalSeq; i++)
            {
                SeqCollection.Add(new InitStatus((SQID)i));
            }
        }

        private void OpenLotEntry(bool canOpenLotEntryView)
        {
            IsLotEntryExpand = canOpenLotEntryView;
        }

        #endregion

        #region Event
        private void UpdateMachineOperation(SequenceEvent evArg)
        {
            lock (evArg)
            {
                Debug.Assert(evArg != null);
                switch (evArg.MachineOpr)
                {
                    case MachineOperationType.InitDone:
                        string stateIcon = evArg.InitSuccess ? GreenIcon : RedIcon;
                        // Change state icon when init done or fail
                        int index = SeqCollection.IndexOf(SeqCollection.Where(x => x.SeqID == evArg.TargetSeqName).First());
                        SeqCollection[index].StateIcon = stateIcon;
                        break;
                }
            }
        }

        private void UpdateMachineState(MachineStateType stateType)
        {
            try
            {
                lock (this)
                {
                    switch (stateType)
                    {
                        case MachineStateType.Initializing:
                            SeqCollection.ToList().ForEach(key => key.StateIcon = GrayIcon);
                            ShowInitState = Visibility.Visible;
                            IsLotEntryExpand = false;
                            break;

                        case MachineStateType.Init_Done:
                            ShowInitState = Visibility.Visible;
                            IsLotEntryExpand = true;
                            break;

                        case MachineStateType.Running:
                            ShowInitState = Visibility.Collapsed;
                            IsLotEntryExpand = false;
                            break;

                        case MachineStateType.Ready:
                            IsLotEntryExpand = false;
                            break;

                        case MachineStateType.Stopped:
                            break;

                        case MachineStateType.Warning:
                            break;

                        case MachineStateType.Error:
                            break;

                        case MachineStateType.Ending_Lot:
                            IsLotEntryExpand = false;
                            break;

                        case MachineStateType.Lot_Ended:
                            // Update Lot Data
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                m_SQLOperation.UpdateLotData(Global.LotInitialBatchNo, DateTime.Now, Global.TotalInput, Global.TotalOutput);
                            });
                            IsLotEntryExpand = true;
                            break;

                        case MachineStateType.Idle:
                            break;

                        case MachineStateType.InitFail:
                            break;

                        case MachineStateType.ReInit:
                            break;

                        case MachineStateType.CriticalAlarm:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        public override void OnValidateLogin(bool IsAuthenticated)
        {
            RaisePropertyChanged(nameof(CanAccess));
        }
        #endregion

        #region Method

        #endregion

        #region Access Implementation
        public override bool CanAccess
        {
            get
            {
                if (m_AuthService.CurrentUser.IsAuthenticated)
                {
                    return true;
                }
                return false;
            }
        }
        #endregion
    }
}
