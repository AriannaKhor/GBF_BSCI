using System.ComponentModel;

namespace GreatechApp.Core.Enums
{
    public enum MachineStateType
    {
        None = 0,
        Running = 4,
        Error = 7,
        Idle = 10,
    }

    public enum NetworkDev
    {
        [Description("Top Vision")]
        TopVision,
    }
}
