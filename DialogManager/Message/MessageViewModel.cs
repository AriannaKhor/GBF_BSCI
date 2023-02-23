using Prism.Mvvm;
using Prism.Services.Dialogs;
using Prism.Commands;
using System;
using System.Windows;
using GreatechApp.Core.Enums;
using System.Windows.Media.Imaging;
using GreatechApp.Core.Cultures;

namespace DialogManager.Message
{
    public class MessageViewModel : BindableBase, IDialogAware
    {
        #region Variable
        CultureResources m_CultureResources;
        private DelegateCommand<string> m_closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            m_closeDialogCommand ?? (m_closeDialogCommand = new DelegateCommand<string>(CloseDialog));

        private BitmapImage m_Image;
        public BitmapImage Image
        {
            get { return m_Image; }
            set { SetProperty(ref m_Image, value); }
        }
        private string m_message;
        public string Message
        {
            get { return m_message; }
            set { SetProperty(ref m_message, value); }
        }

        private string m_title = "";
        public string Title
        {
            get { return m_title; }
            set { SetProperty(ref m_title, value); }
        }

        private string m_Button1Text;
        public string Button1Text
        {
            get { return m_Button1Text; }
            set { SetProperty(ref m_Button1Text, value); }
        }

        private string m_Button2Text;
        public string Button2Text
        {
            get { return m_Button2Text; }
            set { SetProperty(ref m_Button2Text, value); }
        }

        private string m_Button3Text;
        public string Button3Text
        {
            get { return m_Button3Text; }
            set { SetProperty(ref m_Button3Text, value); }
        }

        private Visibility m_Button1Vis;
        public Visibility Button1Vis
        {
            get { return m_Button1Vis; }
            set { SetProperty(ref m_Button1Vis, value); }
        }

        private Visibility m_Button2Vis;
        public Visibility Button2Vis
        {
            get { return m_Button2Vis; }
            set { SetProperty(ref m_Button2Vis, value); }
        }

        private Visibility m_Button3Vis;
        public Visibility Button3Vis
        {
            get { return m_Button3Vis; }
            set { SetProperty(ref m_Button3Vis, value); }
        }

        public event Action<IDialogResult> RequestClose;
        #endregion

        #region Constructor
        public MessageViewModel(CultureResources cultureResources)
        {
            m_CultureResources = cultureResources;
        }

        #endregion

        #region Method
        protected virtual void CloseDialog(string parameter)
        {
            RaiseRequestClose(new DialogResult((ButtonResult)Enum.Parse(typeof(ButtonResult), m_CultureResources.GetDialogTable().GetKey(parameter), true)));
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

            Message = split[0];
            Title =  split[1];
            Button1Vis = split[2] == "true" ? Visibility.Visible : Visibility.Collapsed;
            Button1Text = split[3];
            Button2Vis = split[4] == "true" ? Visibility.Visible : Visibility.Collapsed;
            Button2Text = split[5];
            Button3Vis = split[6] == "true" ? Visibility.Visible : Visibility.Collapsed;
            Button3Text = split[7];

            DialogIcon icon = (DialogIcon)Enum.Parse(typeof(DialogIcon), split[8]);
            switch (icon)
            {
                case DialogIcon.Error:
                    Image = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/error.png", uriKind: UriKind.RelativeOrAbsolute));
                    break;

                case DialogIcon.Complete:
                    Image = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/complete.png", uriKind: UriKind.RelativeOrAbsolute));
                    break;

                case DialogIcon.Information:
                    Image = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/information.png", uriKind: UriKind.RelativeOrAbsolute));
                    break;

                case DialogIcon.Question:
                    Image = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/question.png", uriKind: UriKind.RelativeOrAbsolute));
                    break;

                case DialogIcon.Stop:
                    Image = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/stopsign.png", uriKind: UriKind.RelativeOrAbsolute));
                    break;
            }
        }
        #endregion
    }
}
