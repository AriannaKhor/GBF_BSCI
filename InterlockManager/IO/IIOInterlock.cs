using GreatechApp.Core.Enums;

namespace InterlockManager.IO
{
    public interface IIOInterlock
    {
        bool CheckIOInterlock(int ioNum, bool oState, bool isChildExist);

        SQID Provider { get; }

        string GetSeqName(OUT ioNum);
    }
}
