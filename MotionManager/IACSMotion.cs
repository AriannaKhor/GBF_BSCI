namespace MotionManager
{
    public interface IACSMotion : IBaseMotion
    {
        bool JogM(int cardNo, short directio);

        bool GetVariableStatus(int cardNo, string cmd);

        bool GetSyncStatus(int cardNo, int axis);

        bool DisableMasterSlave(int cardNo, int axis);

        bool RunBufferProgram(int cardNo, int bufProg, string label);

    }
}
