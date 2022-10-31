using GreatechApp.Core.Enums;
using Prism.Services.Dialogs;

namespace GreatechApp.Core.Interface
{
    public interface IShowDialog
    {
        void LoginShow(DialogList View);
        ButtonResult Show(DialogIcon icon, string title, string Message, ButtonResult Btn1Result = ButtonResult.OK, ButtonResult Btn2Result = ButtonResult.None, ButtonResult Btn3Result = ButtonResult.None);
        ButtonResult Show(DialogIcon icon, string Message, ButtonResult Btn1Result = ButtonResult.OK, ButtonResult Btn2Result = ButtonResult.None, ButtonResult Btn3Result = ButtonResult.None);
        ButtonResult Show(DialogIcon icon, string Message);
        ButtonResult Show(DialogIcon icon, string title, string Message, SQID seqName, ButtonResult Btn1Result = ButtonResult.OK, ButtonResult Btn2Result = ButtonResult.None, ButtonResult Btn3Result = ButtonResult.None);
        ButtonResult Show(DialogIcon icon, string Message, SQID seqName, ButtonResult Btn1Result = ButtonResult.OK, ButtonResult Btn2Result = ButtonResult.None, ButtonResult Btn3Result = ButtonResult.None);
        ButtonResult Show(DialogIcon icon, string Message, SQID seqName);
    }
}
