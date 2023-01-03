using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using GreatechApp.Services.UserServices;
using GreatechApp.Services.Utilities;
using Prism.Events;
using Prism.Mvvm;
using Sequence;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace UIModule.MainPanel
{
    public class BypassViewModel : BaseUIViewModel
    {
        #region Variable
        private bool m_IsAllowBypass;
        public bool IsAllowBypass
        {
            get { return m_IsAllowBypass; }
            set { SetProperty(ref m_IsAllowBypass, value); }
        }

        public enum BYP_IDX
        {
            [Description("Bypass All")]
            BypassAll = 0,
            [Description("Door")]
            Door,
            [Description("Code Reader")]
            CodeReader,
            [Description("Top Vision")]
            TopVision,
        }
        private ObservableCollection<Bypass> m_BypassCollection;
        public ObservableCollection<Bypass> BypassCollection
        {
            get { return m_BypassCollection; }
            set { SetProperty(ref m_BypassCollection, value); }
        }
        #endregion

        #region Constructor
        public BypassViewModel()
        {
            BypassCollection = new ObservableCollection<Bypass>();

            m_EventAggregator.GetEvent<MachineState>().Subscribe(MachineStateHandle);

            // Add all machine seq except core seq into collection
            for (int i = 0; i < Enum.GetNames(typeof(BYP_IDX)).Length; i++)
            {
                BypassCollection.Add(new Bypass(i));
                BypassCollection[i].Description = EnumHelper.GetDescription((BYP_IDX)i);
            }

        }
        #endregion

        #region Event
        private void MachineStateHandle(MachineStateType machineState)
        {
            switch (machineState)
            {
                case MachineStateType.Init_Done:
                    IsAllowBypass = true;
                    RefreshAllBypassEnabledState();
                    break;

                case MachineStateType.Lot_Ended:
                    IsAllowBypass = true;
                    RefreshAllBypassEnabledState();
                    break;

                case MachineStateType.Running:
                    IsAllowBypass = false;
                    RefreshAllBypassEnabledState();
                    break;

                case MachineStateType.Stopped:
                    IsAllowBypass = true;
                    RefreshAllBypassEnabledState();
                    break;

                case MachineStateType.Idle:
                    IsAllowBypass = true;
                    RefreshAllBypassEnabledState();
                    break;
            }
        }
        public void BypassStation(Bypass b)
        {
            switch (b.ID)
            {
                case (int)BYP_IDX.BypassAll:
                    // If Bypass ALL is set to TRUE, set all other station's bypass as true
                    // If Bypass ALL is set to FALSE it should only affect Bypass All Flag
                    if (BypassCollection[(int)BYP_IDX.BypassAll].IsBypass)
                    {
                        // Set Bypass in every Bypass
                        foreach (Bypass bp in BypassCollection)
                        {
                            if (bp.ID != (int)BYP_IDX.BypassAll)
                            {
                                bp.IsBypass = true;
                            }
                        }
                    }
                    else
                    {
                        foreach (Bypass bp in BypassCollection)
                        {
                            if (bp.ID != (int)BYP_IDX.BypassAll)
                            {
                                bp.IsBypass = false;
                            }
                        }
                    }
                    // Set flag from BypassCollection in UI to respective BypassFlag in sequence/ global data
                    UpdateBypassToAllSeq();
                    break;

                case (int)BYP_IDX.Door:
                    // TODO : No action or seq done when Bypass Door is opened or closed
                    Global.BypassDoor = BypassCollection[(int)BYP_IDX.Door].IsBypass;
                    break;

                case (int)BYP_IDX.CodeReader:
                    // Set the bypass flag to particular machine seq when triggerd
                    m_DelegateSeq.BypassStation(SQID.CodeReaderSeq, BypassCollection[(int)BYP_IDX.CodeReader].IsBypass);
                    break;

                case (int)BYP_IDX.TopVision:
                    // Set the bypass flag to particular machine seq when triggerd
                    m_DelegateSeq.BypassStation(SQID.CodeReaderSeq, BypassCollection[(int)BYP_IDX.TopVision].IsBypass);
                    break;
            }
        }
        #endregion

        #region Method
        private void UpdateBypassToAllSeq()
        {
            // Update bypass flag to all machine seq
            Global.BypassDoor = BypassCollection[(int)BYP_IDX.Door].IsBypass;
            m_DelegateSeq.BypassStation(SQID.CodeReaderSeq, BypassCollection[(int)BYP_IDX.CodeReader].IsBypass);
            m_DelegateSeq.BypassStation(SQID.CodeReaderSeq, BypassCollection[(int)BYP_IDX.TopVision].IsBypass);
        }

        private void RefreshAllBypassEnabledState()
        {
            // Only disable /enable some bypass option when there is a change of equipstate
            BypassCollection[(int)BYP_IDX.BypassAll].IsEnabled = IsAllowBypass;
            BypassCollection[(int)BYP_IDX.Door].IsEnabled = IsAllowBypass;
            BypassCollection[(int)BYP_IDX.CodeReader].IsEnabled = IsAllowBypass;
            BypassCollection[(int)BYP_IDX.TopVision].IsEnabled = IsAllowBypass;
            RaisePropertyChanged(nameof(BypassCollection));
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
