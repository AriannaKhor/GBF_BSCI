using System.ComponentModel;

namespace GreatechApp.Core.Enums
{
    public enum MachineStateType
    {
        None = 0,
        Ready = 1,
        Initializing = 2,
        Init_Done = 3,
        Running = 4,
        Stopped = 5,
        Warning = 6,
        Error = 7,
        Ending_Lot = 8,
        Lot_Ended = 9,
        Idle = 10,
        Recovery = 11,
        Repairing = 12,
        CriticalAlarm = 13,
        InitFail = 14,
        ReInit = 15,
    }

    public enum NetworkDev
    {
        [Description("Top Vision")]
        TopVision,

        [Description("Code Reader")]
        CodeReader,
    }

    public enum ToolLifeID
    {
        Test1,
        Test2,
        Test3,
        Test4,
        Test5,
        Test6,
    }

    public enum ToolLifeType
    {
        Cleaning,
        ToolLife,
    }
}
