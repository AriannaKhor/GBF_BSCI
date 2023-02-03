using GreatechApp.Core.Enums;
using GreatechApp.Core.Modal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace GreatechApp.Core.Variable
{
    public class Global
    {
        public static bool Temp;
        public static string MachName;
        public static int MachNo;
        public static string SoftwareVersion;
        public static CultureInfo CurrentCulture;

        //UserInfo
        public static string UserId;
        public static string UserLvl;

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
        public static string LotInitialBatchNo = string.Empty;
        public static int LotInitialTotalBatchQuantity;

        //InsightVision
        public static string VisionConnStatus;
        public static bool VisConnection;
        public static float VisProductQuantity;
        public static float VisProductCrtOrientation;
        public static float VisProductWrgOrientation;
        public static string VisOverallResult;
        public static string VisInspectResult = resultstatus.PendingResult.ToString();
        public static bool EndTrigger = false; 

        //Code Reader Result
        public static string CurrentContainerNum;
        public static int CurrentBatchQuantity;
        public static string CurrentMatl;
        public static string CurrentBatchNum;
        public static int CurrentBoxCount=0;
        public static int CurrentBoxQuantity;
        public static int AccumulateCurrentBatchQuantity;
        public static bool ContAccumBatchQty = true;
        public static string CodeReaderConnStatus = "Not Connected";
        public static string CodeReaderResult = resultstatus.PendingResult.ToString();
        public static string OverallResult;
        public static string CurrentLotBatchNum;
       
        //Error 
        public static string ErrorMsg;
        public static string Remarks;
        public static string ForceEndLotRemarks;
        public static string CurrentApprovalLevel;
        public static string ForceEndLotCurrentApprovalLevel;

        //Recipe Parameter
        public static int MotionLoopCount;
        public static int PointIntervalDistance;
        public static double PointIntervalDelay;

        // Sequence Tracking 
        public static bool SeqStatusScanOn = false;

        // Machine Status
        public static MachineStateType MachineStatus = MachineStateType.Idle;

        //Secs Gem
        public static bool OnSecsGem;

        //End Lot Flag
        public static bool TopVisionEndLot = false;
        public static bool CodeReaderEndLot = false;

        //Proceed New Box Flag
        public static bool TopVisionProceedNewBox = false;
        public static bool CodeReaderProceedNewBox = false;
    }
}
