namespace GreatechApp.Core.Enums
{
    public enum MachineStateType
    {
        None = 0,
        Ready = 1,
        Initializing = 2,
        Running = 3,
        Stop = 5,
        Warning = 6,
        Error = 7,
        Ending_Lot = 8,
        Idle = 9,
        Purging = 10,
        ScanBarcode,
    }
}
