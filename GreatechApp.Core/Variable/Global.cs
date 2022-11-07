using GreatechApp.Core.Enums;
using GreatechApp.Core.Modal;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace GreatechApp.Core.Variable
{
    public class Global
    {        
        public static string MachName;
        public static int MachNo;
        public static string SoftwareVersion;
        public static CultureInfo CurrentCulture; 
       
        // Machine Operation
        public static bool AutoMode;
        public static bool DryRun;
        public static bool SeqStop;

        // Custom Bypass
        public static bool BypassDoor;

        // Sequence Initialization
        public static bool InitDone = false;

        //Error Recovery
        public static List<ErrRecovery> SkipRetest = new List<ErrRecovery>();

        // Machine Performance
        public static double CycleTime;
        public static int TotalInput;
        public static int TotalOutput;

        // Lot Info
        public static string LotInitialBatchNo;
        public static int LotInitialTotalBatchQuantity;
        public static string LotOperatorID;
        public static string LotRecipe;

        //InsightVision
        public static string VisionConnStatus;
        public static bool VisConnection;
        public static float VisProductQuantity;
        public static string VisProductCrtOrientation;
        public static string VisProductWrgOrientation;
        public static string VisInspectResult = resultstatus.PendingResult.ToString();

        //Code Reader Result
        public static string CurrentContainerNum;
        public static int CurrentBatchQuantity;
        public static int CurrentMatl;
        public static string CurrentBatchNum;
        public static int CurrentBoxQuantity;
        public static int AccumulateCurrentBatchQuantity;
        public static string CodeReaderConnStatus;
        public static string CodeReaderResult = resultstatus.PendingResult.ToString();

        //Recipe Parameter
        public static int MotionLoopCount;
        public static int PointIntervalDistance;
        public static double PointIntervalDelay;

        // Sequence Tracking 
        public static bool SeqStatusScanOn = false;

        // Machine Status
        public static MachineStateType MachineStatus = MachineStateType.Ready;

        //Secs Gem
        public static bool OnSecsGem;
    }
}
