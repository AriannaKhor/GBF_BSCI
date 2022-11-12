using DBManager.Domains;
using GreatechApp.Core.Command;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Variable;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using TCPIPManager;
//using static UIModule.MainPanel.EquipmentViewModel;

namespace UIModule.MainPanel
{
    public class LotEntryViewModel : BaseUIViewModel
    {
        #region Variables

        public string LotCodeResult;

        private string m_LotID;
        public string LotID
        {
            get { return m_LotID; }
            set { SetProperty(ref m_LotID, value); }
        }

        private string m_OperatorID;
        public string OperatorID
        {
            get { return m_OperatorID; }
            set { SetProperty(ref m_OperatorID, value); }
        }

        private int m_TotalBatchQuantity;
        public int TotalBatchQuantity
        {
            get { return m_TotalBatchQuantity; }
            set { SetProperty(ref m_TotalBatchQuantity, value); }
        }

        private AppDBContext m_RecipeContext;

        private ObservableCollection<TblRecipe> m_Recipe;
        public ObservableCollection<TblRecipe> Recipe
        {
            get { return m_Recipe; }
            set
            {
                SetProperty(ref m_Recipe, value);
                if (SelectedRecipe == null || Recipe.FirstOrDefault(x => x.Product_Name == SelectedRecipe.Product_Name) == null)
                {
                    if (Recipe != null && Recipe.Count > 0)
                    {
                        SelectedRecipe = Recipe[0];
                    }
                }
            }
        }

        private TblRecipe m_SelectedRecipe;
        public TblRecipe SelectedRecipe
        {
            get { return m_SelectedRecipe; }
            set { SetProperty(ref m_SelectedRecipe, value); }
        }

        private bool m_IsAllowStartLot;
        public bool IsAllowStartLot
        {
            get { return m_IsAllowStartLot; }
            set { SetProperty(ref m_IsAllowStartLot, value); }
        }

        private bool m_IsAllowEndLot;
        public bool IsAllowEndLot
        {
            get { return m_IsAllowEndLot; }
            set { SetProperty(ref m_IsAllowEndLot, value); }
        }

        public DelegateCommand<string> LotCommand { get; private set; }
        #endregion

        #region Constructor
        public LotEntryViewModel()
        {
            m_EventAggregator.GetEvent<MachineState>().Subscribe(OnUpdateMachineState);
            m_EventAggregator.GetEvent<TCPIPMsg>().Subscribe(TCPMsgOperation);
            m_EventAggregator.GetEvent<UpdateDatabase>().Subscribe(RefreshRecipes);

            LotCommand = new DelegateCommand<string>(Lotoperation);

            RefreshRecipes();

            OnUpdateMachineState(Global.MachineStatus);
        }
        #endregion

        #region Methods
        public void RefreshRecipes()
        {
            m_RecipeContext = new AppDBContext();
            m_RecipeContext.TblRecipe.Load();
            Recipe = m_RecipeContext.TblRecipe.Local.ToObservableCollection();
        }

        private void Lotoperation(string Command)
        {
            if (Command == "StartLot")
            {
                if (!string.IsNullOrEmpty(LotID) && !string.IsNullOrEmpty(TotalBatchQuantity.ToString()) && !string.IsNullOrEmpty(OperatorID) && SelectedRecipe != null)
                {
                    ButtonResult dialogResult = m_ShowDialog.Show(DialogIcon.Question, GetDialogTableValue("StartLot"), GetDialogTableValue("AskConfirmStartLot") + " " + LotID, ButtonResult.No, ButtonResult.Yes);

                    if (dialogResult == ButtonResult.Yes)
                    {
                        Global.LotInitialBatchNo = LotID;
                        Global.LotOperatorID = OperatorID;
                        Global.LotInitialTotalBatchQuantity = TotalBatchQuantity;
                        Global.LotRecipe = SelectedRecipe.Product_Name.ToString();
                        //m_EventAggregator.GetEvent<OpenLotEntryView>().Publish(false);


                        if (!m_SQLOperation.InsertNewLot(LotID, OperatorID, TotalBatchQuantity.ToString(), SelectedRecipe.Product_Name.ToString(), DateTime.Now))
                        {
                            m_ShowDialog.Show(DialogIcon.Error, GetDialogTableValue("FailInsertLot"));
                            return;
                        }
                        m_EventAggregator.GetEvent<RefreshTotalInputOutput>().Publish();
                        // Trigger all modules to do house keeping
                        ApplicationCommands.OperationCommand.Execute("Submit_Lot");
                        m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Ready);
                        m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetDialogTableValue("User")} {m_CurrentUser.Username} {GetDialogTableValue("SubmitLot")} : {LotID}" });
                    }
                }
                else
                {
                    string ErrorMsg = "";

                    if (string.IsNullOrEmpty(LotID))
                    {
                        ErrorMsg = GetDialogTableValue("LotIDEmpty");
                    }
                    else if (string.IsNullOrEmpty(TotalBatchQuantity.ToString()))
                    {
                        ErrorMsg = GetDialogTableValue("TotalBatchQuantityEmpty"); // See whether it's perform manually or not (If using code reader then this have to be taken out 
                    }
                    else if (string.IsNullOrEmpty(OperatorID))
                    {
                        ErrorMsg = GetDialogTableValue("OperatorIDEmpty");
                    }
                    else if (SelectedRecipe == null)
                    {
                        ErrorMsg = GetDialogTableValue("RecipeEmpty");
                    }

                    m_ShowDialog.Show(DialogIcon.Error, ErrorMsg);
                }
            }
            else if (Command == "EndLot")
            {
                if (Global.AccumulateCurrentBatchQuantity == Global.LotInitialTotalBatchQuantity)
                {
                    ButtonResult dialogResult = m_ShowDialog.Show(DialogIcon.Question, GetDialogTableValue("EndLot"), GetDialogTableValue("AskConfirmEndLot") + " " + Global.LotInitialBatchNo, ButtonResult.No, ButtonResult.Yes);

                    if (dialogResult == ButtonResult.Yes)
                    {
                        m_EventAggregator.GetEvent<EndLotOperation>().Publish();
                        m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Ending_Lot);
                        Global.MachineStatus = MachineStateType.Ending_Lot;

                        // Send EndLot event to the sequence that required
                        m_EventAggregator.GetEvent<MachineOperation>().Publish(new SequenceEvent() { TargetSeqName = SQID.CountingScaleSeq, MachineOpr = MachineOperationType.EndLotComp});

                        m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("User")} {m_CurrentUser.Username} {GetStringTableValue("Init")} {GetStringTableValue("EndLot")} {GetStringTableValue("Sequence")} : {Global.LotInitialBatchNo}" });
                        m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Lot_Ended);
                        LotCodeResult = resultstatus.Pass.ToString();
                        m_EventAggregator.GetEvent<ResultlogEntity>().Publish(new ResultlogEntity() { MsgType = LogMsgType.Info, MsgText = LotCodeResult });
                        EmptyLotEntry();
                    }
                }
                else
                {
                    m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Error);
                    
                    ButtonResult dialogResult = m_ShowDialog.Show(DialogIcon.Error, "Current Batch Quantity less than Lot Batch Quantity", ButtonResult.OK);

                    if (dialogResult == ButtonResult.OK)
                    {
                        m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.Ready);
                        LotCodeResult = resultstatus.Fail.ToString();
                        m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = LotCodeResult });
                        m_EventAggregator.GetEvent<ResultlogEntity>().Publish(new ResultlogEntity() { MsgType = LogMsgType.Info, MsgText = LotCodeResult });
                    }
                }
            }
        }


        void TCPMsgOperation(TCPIPMsg tcpMsg)
        {
            switch (tcpMsg.TCPDevice)
            {
                case NetworkDev.CodeReader:
                    if (tcpMsg.Message.Contains("LotDone"))
                    {

                    }
                    break;

                case NetworkDev.TopVision:

                    break;
            }
        }

        public void EmptyLotEntry()
        {
            LotID = string.Empty;
            OperatorID = string.Empty;
            TotalBatchQuantity = 0;
        }
        #endregion

        #region Events

   
        private void OnUpdateMachineState(MachineStateType stateType)
        {
            try
            {
                lock (this)
                {
                    switch (stateType)
                    {
                        case MachineStateType.Running:
                            IsAllowStartLot = false;
                            IsAllowEndLot = false;
                            break;

                        case MachineStateType.Stopped:
                            IsAllowStartLot = false;
                            IsAllowEndLot = true;
                            break;

                        case MachineStateType.Warning:
                            IsAllowStartLot = false;
                            IsAllowEndLot = false;
                            break;

                        case MachineStateType.Error:
                            IsAllowStartLot = false;
                            IsAllowEndLot = false;
                            break;

                        case MachineStateType.Lot_Ended:
                            IsAllowStartLot = true;
                            IsAllowEndLot = false;
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                RefreshRecipes();
                            });
                            break;

                        case MachineStateType.Idle:
                            IsAllowStartLot = true;
                            IsAllowEndLot = false;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }
        #endregion
    }
}
