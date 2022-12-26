using GreatechApp.Core.Enums;

namespace GreatechApp.Core.Interface
{
    public interface IError
    {
        SQID SeqName { get; set; }
        string RaiseError(int ErrorCode, SQID seqName);
        void CloseWarning(int ErrorCode, SQID SeqName);
        string RaiseVerificationError(int ErrorCode, SQID SeqName);
    }
}
