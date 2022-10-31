using GreatechApp.Core.Cultures;
using GreatechApp.Core.Events;
using GreatechApp.Core.Resources;
using GreatechApp.Services.UserServices;
using GreatechApp.Services.Utilities;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Linq;
using System.Windows.Controls;

namespace DialogManager.Login
{
    public class LoginViewModel : BindableBase, IDialogAware
    {
        #region Variable
        private AuthService m_authService;
        CultureResources m_CultureResources;
        private IEventAggregator m_eventAggregator;

        private DelegateCommand<string> m_closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            m_closeDialogCommand ?? (m_closeDialogCommand = new DelegateCommand<string>(CloseDialog));

        private string m_ErrMessage;
        public string ErrMessage
        {
            get { return m_ErrMessage; }
            set { SetProperty(ref m_ErrMessage, value); }
        }

        private string m_title = "User Login";
        public string Title
        {
            get { return m_title; }
            set { SetProperty(ref m_title, value); }
        }

        public event Action<IDialogResult> RequestClose;

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

        private bool m_IsMaskPass;
        public bool IsMaskPass
        {
            get { return m_IsMaskPass; }
            set { SetProperty(ref m_IsMaskPass, value); }
        }

        public DelegateCommand<object> LoginCommand { get; private set; }
        #endregion

        #region Constructor

        public LoginViewModel(AuthService authService, IEventAggregator eventAggregator,CultureResources cultureResources )
        {
            LoginCommand = new DelegateCommand<object>(LoginMethod);
            m_authService = authService;
            m_eventAggregator = eventAggregator;
            m_CultureResources = cultureResources;
        }

        #endregion

        #region Method

        private void LoginMethod(object value)
        {
            var passwordBox = value as PasswordBox;
            var password = passwordBox.Password;

            if (m_authService.Authenticate(UserID, password))
            {
                m_authService.CurrentUser.Username = UserID;
                m_authService.CurrentUser.IsAuthenticated = true;
                m_eventAggregator.GetEvent<ValidateLogin>().Publish(true);
                CloseDialog("");
            }
            else
            {
                ErrMessage = m_CultureResources.GetStringValue("InvalidLoginInfo");
            }
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
            IsUserIDFocused = true;
        }
        #endregion
    }
}
