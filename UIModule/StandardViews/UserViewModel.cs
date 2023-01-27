namespace UIModule.StandardViews
{
    using DBManager.Domains;
    using GreatechApp.Core.Cultures;
    using GreatechApp.Core.Enums;
    using GreatechApp.Core.Events;
    using GreatechApp.Core.Interface;
    using GreatechApp.Core.Resources;
    using GreatechApp.Services.UserServices;
    using GreatechApp.Services.Utilities;
    using Microsoft.EntityFrameworkCore;
    using Prism.Commands;
    using Prism.Events;
    using Prism.Mvvm;
    using Prism.Regions;
    using Prism.Services.Dialogs;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using UIModule.MainPanel;
    using static GreatechApp.Core.Enums.ACL;

    public class UserViewModel : BaseUIViewModel, INavigationAware, IRegionMemberLifetime
    {
        #region Variable
        private AppDBContext m_DBContext;
        private string m_title;
        public string Title
        {
            get { return m_title; }
            set { SetProperty(ref m_title, value); }
        }

        private string m_UserID;
        public string UserID
        {
            get { return m_UserID; }
            set { SetProperty(ref m_UserID, value); }
        }

        private int m_SelectedIndex;
        public int SelectedIndex
        {
            get { return m_SelectedIndex; }
            set { SetProperty(ref m_SelectedIndex, value); LoadData(); }
        }

        private string m_UserName;
        public string UserName
        {
            get { return m_UserName; }
            set { SetProperty(ref m_UserName, value); }
        }

        private string m_UserPass;
        public string UserPass
        {
            get { return m_UserPass; }
            set { SetProperty(ref m_UserPass, value); }
        }

        private string m_UserCPass;
        public string UserCPass
        {
            get { return m_UserCPass; }
            set { SetProperty(ref m_UserCPass, value); }
        }

        private int m_UserLevel = 1;
        public int UserLevel
        {
            get { return m_UserLevel; }
            set { SetProperty(ref m_UserLevel, value); }
        }

        private string m_EUserID;
        public string EUserID
        {
            get { return m_EUserID; }
            set { SetProperty(ref m_EUserID, value); }
        }

        private string m_EUserName;
        public string EUserName
        {
            get { return m_EUserName; }
            set { SetProperty(ref m_EUserName, value); }
        }

        private int m_EUserLevel;
        public int EUserLevel
        {
            get { return m_EUserLevel; }
            set { SetProperty(ref m_EUserLevel, value); }
        }

        private string m_EUserPass;
        public string EUserPass
        {
            get { return m_EUserPass; }
            set { SetProperty(ref m_EUserPass, value); }
        }

        private string m_EUserCPass;
        public string EUserCPass
        {
            get { return m_EUserCPass; }
            set { SetProperty(ref m_EUserCPass, value); }
        }

        private string m_UserStatus;
        public string UserStatus
        {
            get { return m_UserStatus; }
            set { SetProperty(ref m_UserStatus, value); }
        }

        private string m_AccessControlStatus;
        public string AccessControlStatus
        {
            get { return m_AccessControlStatus; }
            set { SetProperty(ref m_AccessControlStatus, value); }
        }

        private TblAccessControl m_SelectedAccessCtrl;
        public TblAccessControl SelectedAccessCtrl
        {
            get { return m_SelectedAccessCtrl; }
            set { SetProperty(ref m_SelectedAccessCtrl, value); }
        }

        private List<string> m_UserLevelCollection;
        public List<string> UserLevelCollection
        {
            get { return m_UserLevelCollection; }
            set { SetProperty(ref m_UserLevelCollection, value); }
        }

        private ObservableCollection<TblUser> m_UserCollection;
        public ObservableCollection<TblUser> UserCollection
        {
            get { return m_UserCollection; }
            set
            {
                SetProperty(ref m_UserCollection, value);
            }
        }

        private ObservableCollection<TblAccessControl> m_AccessCtrlCollection;
        public ObservableCollection<TblAccessControl> AccessCtrlCollection
        {
            get { return m_AccessCtrlCollection; }
            set
            {
                SetProperty(ref m_AccessCtrlCollection, value);
            }
        }
        public DelegateCommand<string> UserCommand { get; private set; }
        public DelegateCommand<string> KeyBoardCommand { get; set; }
        #endregion

        #region Constructor
        public UserViewModel()
        {
            Title = GetStringTableValue("UserAccount");

            UserCommand = new DelegateCommand<string>(UserOperation);

            m_UserLevelCollection = new List<string>();
            for (int i = 0; i < Enum.GetNames(typeof(UserLevel)).Length - 1; i++)
            {
                UserLevelCollection.Add(EnumHelper.GetDescription((UserLevel)i));
            }
            RefreshDatabase();
        }
        #endregion

        #region Events
        public override void OnCultureChanged()
        {
            Title = GetStringTableValue("UserAccount");
        }
        #endregion

        #region Method
        private void UserOperation(string Command)
        {
            if (Command == "Add")
            {
                bool isValidEntry = m_SQLOperation.IsValidEntry(UserCollection, UserID);
                bool isEmptyEntry = !string.IsNullOrEmpty(UserID) && !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(UserPass) && UserLevel > 0;
                if (isValidEntry && isEmptyEntry)
                {
                    UserStatus = m_SQLOperation.AddUser(UserID, UserName, UserLevel, UserPass) ? $"{DialogTable.SuccessAddNewUser} : {UserID}" : $"{DialogTable.FailAddNewUser} : {UserID}";
                    m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("User")} {m_CurrentUser.Username} {GetStringTableValue("AddUser")} : {UserID}" });
                    RefreshDatabase();
                }
                else
                {
                    UserStatus = !isValidEntry ? $"{DialogTable.User} : {UserID} {DialogTable.alreadyexistindatabase}. " : $"{DialogTable.InputEmpty}";
                }
            }
            else if (Command == "Delete")
            {
                ButtonResult buttonResult = m_ShowDialog.Show(DialogIcon.Question, GetDialogTableValue("AskConfirmDeleteUser"));


                    if(buttonResult == ButtonResult.Yes)
                    {
                        UserStatus = m_SQLOperation.DeleteUser(UserCollection[SelectedIndex].User_ID) ? $"{DialogTable.SuccessdeleteUser} : {UserCollection[SelectedIndex].User_ID}" : $"{DialogTable.FaildeleteUser} : {UserCollection[SelectedIndex].User_ID}";
                        m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("User")} {m_CurrentUser.Username} {GetStringTableValue("Delete")} : {UserCollection[SelectedIndex].User_ID}" });
                        RefreshDatabase();
                    }
            }
            else if (Command == "Update")
            {
                if (EUserPass == EUserCPass)
                {
                    UserStatus = m_SQLOperation.EditUser(EUserID, EUserName, EUserLevel, EUserPass) ? $"{DialogTable.SuccessupdateUser} : {EUserID}" : $"{DialogTable.FailupdateUser} : {EUserID}";
                    m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("User")} {m_CurrentUser.Username} {GetStringTableValue("UpdateUser")}: {EUserID}" });
                    RefreshDatabase();
                }
                else
                {
                    UserStatus = DialogTable.BothPassNotMatch;
                }
            }
            else if (Command == "OnRowEditEnded")
            {
                AccessControlStatus = m_SQLOperation.UpdateAcessControl(SelectedAccessCtrl) ? $"{DialogTable.SuccessUpdateAccessCtrl} : {SelectedAccessCtrl.User_Desc}" : $"{DialogTable.FailUpdateAccessCtrl} : {SelectedAccessCtrl.User_Desc}";
                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("User")} {m_CurrentUser.Username} {GetStringTableValue("UpdateAccessControl")} : {SelectedAccessCtrl.User_Desc}" });
            }
        }

        private void LoadData()
        {
            if (SelectedIndex <= UserCollection.Count - 1 && SelectedIndex >= 0)
            {
                EUserID = UserCollection[SelectedIndex].User_ID;
                EUserName = UserCollection[SelectedIndex].UserName;
                EUserLevel = AccessCtrlCollection.Single(x => x.User_Level == UserCollection[SelectedIndex].User_Level).User_Level;
            }
        }
        private void RefreshDatabase()
        {
            m_DBContext = new AppDBContext();
            m_DBContext.TblUser.Load();
            UserCollection = m_DBContext.TblUser.Local.ToObservableCollection();

            m_DBContext.TblAccessControl.Load();
            AccessCtrlCollection = m_DBContext.TblAccessControl.Local.ToObservableCollection();
        }
        #endregion

        #region Access Implementation
        public override bool CanAccess
        {
            get
            {
                if (m_AuthService.CheckAccess(ACL.User_Management) && m_AuthService.CurrentUser.IsAuthenticated)
                {
                    return true;
                }
                return false;
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

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {

        }
        #endregion

    }
}
