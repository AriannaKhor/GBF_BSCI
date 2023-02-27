using ConfigManager;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Modal;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GreatechApp.Services.Utilities
{
    public class ShowDialog : IShowDialog
    {
        #region Variable
        IDialogService m_dialogService;
        CultureResources m_CultureResources;
        #endregion

        #region Constructor
        public ShowDialog(IDialogService dialogService, CultureResources cultureResources)
        {
            m_dialogService = dialogService;
            m_CultureResources = cultureResources;
        }
        #endregion

        #region Method
        public ButtonResult Show(DialogIcon icon, string title, string Message, ButtonResult Btn1Result = ButtonResult.OK, ButtonResult Btn2Result = ButtonResult.None, ButtonResult Btn3Result = ButtonResult.None)
        {
            ButtonResult buttonResult = ButtonResult.None;

            Message += ";" + title + ";" + "true" + ";" + m_CultureResources.GetDialogValue(Btn1Result.ToString());
            string Btn2Condition = Btn2Result != ButtonResult.None ? "true" + ";" + m_CultureResources.GetDialogValue(Btn2Result.ToString()) : "false" + ";" + m_CultureResources.GetDialogValue(Btn2Result.ToString());
            string Btn3Condition = Btn3Result != ButtonResult.None ? "true" + ";" + m_CultureResources.GetDialogValue(Btn3Result.ToString()) : "false" + ";" + m_CultureResources.GetDialogValue(Btn3Result.ToString());
            string IconString = icon.ToString();

            Message += ";" + Btn2Condition + ";" + Btn3Condition + ";" + IconString;
            Application.Current.Dispatcher.Invoke(() =>
            {
                m_dialogService.ShowDialog(DialogList.MessageView.ToString(), new DialogParameters($"message={Message}"), r =>
                {
                    buttonResult = r.Result;
                });
            });
            return buttonResult;
        }

        public ButtonResult Show(DialogIcon icon, string Message, ButtonResult Btn1Result = ButtonResult.OK, ButtonResult Btn2Result = ButtonResult.None, ButtonResult Btn3Result = ButtonResult.None)
        {
            return Show(icon, m_CultureResources.GetDialogValue(icon.ToString()), Message, Btn1Result, Btn2Result, Btn3Result);
        }

        public ButtonResult Show(DialogIcon icon, string Message)
        {
            ButtonResult Btn1Result = ButtonResult.None;
            ButtonResult Btn2Result = ButtonResult.None;
            ButtonResult Btn3Result = ButtonResult.None;

            switch (icon)
            {
                case DialogIcon.Complete:
                    Btn1Result = ButtonResult.OK;
                    break;
                case DialogIcon.Error:
                    Btn1Result = ButtonResult.OK;
                    break;
                case DialogIcon.Information:
                    Btn1Result = ButtonResult.OK;
                    break;
                case DialogIcon.Question:
                    Btn1Result = ButtonResult.Cancel;
                    Btn2Result = ButtonResult.Yes;
                    break;
                case DialogIcon.Stop:
                    Btn1Result = ButtonResult.OK;
                    break;
            }
            return Show(icon, m_CultureResources.GetDialogValue(icon.ToString()), Message, Btn1Result, Btn2Result, Btn3Result);
        }

        public ButtonResult Show(DialogIcon icon, string title, string Message, SQID seqName, ButtonResult Btn1Result = ButtonResult.OK, ButtonResult Btn2Result = ButtonResult.None, ButtonResult Btn3Result = ButtonResult.None)
        {
            ButtonResult buttonResult = ButtonResult.None;

            Message += ";" + title + ";" + "true" + ";" + m_CultureResources.GetDialogValue(Btn1Result.ToString());
            string Btn2Condition = Btn2Result != ButtonResult.None ? "true" + ";" + m_CultureResources.GetDialogValue(Btn2Result.ToString()) : "false" + ";" + m_CultureResources.GetDialogValue(Btn2Result.ToString());
            string Btn3Condition = Btn3Result != ButtonResult.None ? "true" + ";" + m_CultureResources.GetDialogValue(Btn3Result.ToString()) : "false" + ";" + m_CultureResources.GetDialogValue(Btn3Result.ToString());
            string IconString = icon.ToString();

            Message += ";" + Btn2Condition + ";" + Btn3Condition + ";" + IconString;
            Application.Current.Dispatcher.Invoke(() =>
            {
                m_dialogService.ShowDialog(DialogList.MessageView.ToString(), new DialogParameters($"message={seqName}: {Message}"), r =>
                {
                    buttonResult = r.Result;
                });
            });
            return buttonResult;
        }
        public ButtonResult Show(DialogIcon icon, string Message, SQID seqName, ButtonResult Btn1Result = ButtonResult.OK, ButtonResult Btn2Result = ButtonResult.None, ButtonResult Btn3Result = ButtonResult.None)
        {
            return Show(icon, m_CultureResources.GetDialogValue(icon.ToString()), Message, seqName, Btn1Result, Btn2Result, Btn3Result);
        }

        public ButtonResult Show(DialogIcon icon, string Message, SQID seqName)
        {
            ButtonResult Btn1Result = ButtonResult.None;
            ButtonResult Btn2Result = ButtonResult.None;
            ButtonResult Btn3Result = ButtonResult.None;

            switch (icon)
            {
                case DialogIcon.Complete:
                    Btn1Result = ButtonResult.OK;
                    break;
                case DialogIcon.Error:
                    Btn1Result = ButtonResult.OK;
                    break;
                case DialogIcon.Information:
                    Btn1Result = ButtonResult.OK;
                    break;
                case DialogIcon.Question:
                    Btn1Result = ButtonResult.Cancel;
                    Btn2Result = ButtonResult.Yes;
                    break;
                case DialogIcon.Stop:
                    Btn1Result = ButtonResult.OK;
                    break;
            }
            return Show(icon, m_CultureResources.GetDialogValue(icon.ToString()), Message, seqName, Btn1Result, Btn2Result, Btn3Result);
        }

        // Show ErrMeassageView for Error Detail
        public ButtonResult ErrorShow(AlarmParameter Message)
        {
            ButtonResult buttonResult = ButtonResult.None;

            string message = "";

            // ErrorCode, Station, Causes, Recovery, AlarmType, RetestDefault, RetestOption
            message += $"{Message.ErrorCode};";
            message += $"{Message.Station};";
            message += $"{Message.Causes.Split(':').LastOrDefault()};";
            message += $"{Message.Recovery};";
            message += $"{Message.AlarmType};";
            message += $"{Message.RetestDefault};";
            message += $"{Message.RetestOption};";
            message += $"{Message.IsStopPage};";

            message += Message.Module.ToString();

            Application.Current.Dispatcher.Invoke(() =>
            {
                m_dialogService.ShowDialog(DialogList.ErrMessageView.ToString(), new DialogParameters($"message={message}"), r =>
                {
                    buttonResult = r.Result;
                });
            });
            return buttonResult;
        }



        // Show ErrVerificationView for Error Detail
        public ButtonResult ErrorVerificationShow(AlarmParameter Message)
        {
            ButtonResult buttonResult = ButtonResult.None;

            string message = "";

            // ErrorCode, Station, Causes, Recovery, AlarmType, RetestDefault, RetestOption
            message += $"{Message.ErrorCode};";
            message += $"{Message.Station};";
            message += $"{Message.Causes.Split(':').LastOrDefault()};";
            message += $"{Message.Recovery};";
            message += $"{Message.AlarmType};";
            message += $"{Message.RetestDefault};";
            message += $"{Message.RetestOption};";
            message += $"{Message.IsStopPage};";

            message += Message.Module.ToString();

            Application.Current.Dispatcher.Invoke(() =>
            {
                m_dialogService.ShowDialog(DialogList.ErrVerificationView.ToString(), new DialogParameters($"message={message}"), r =>
                {
                    buttonResult = r.Result;
                });
            });
            return buttonResult;
        }

        public void LoginShow(DialogList View)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                m_dialogService.ShowDialog(View.ToString(), new DialogParameters($"message={""}"), r =>
                {

                });
            });
        }
        #endregion
    }
}
