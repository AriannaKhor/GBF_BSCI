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

        // Lot Info
        public static string LotInitialBatchNo = string.Empty;
        public static int LotInitialTotalBatchQuantity;

        //InsightVision
        public static string VisionConnStatus;
        public static string VisClassification;
        public static bool VisConnection;
        public static float VisProductQuantity;
        public static float VisProductCrtOrientation;
        public static float VisProductWrgOrientation;
        public static string VisOverallResult;
        public static string VisInspectResult = resultstatus.PendingResult.ToString();
        public static bool EndTrigger = false;

        //Weighing Scale
        public static string WeighingResult;
        public static string OverallResult;
        public static string CurrentLotBatchNum;
       
        //Error 
        public static string ErrorMsg;
        public static string Remarks;
        public static string ForceEndLotRemarks;
        public static string CurrentApprovalLevel;
        public static string ForceEndLotCurrentApprovalLevel;

        // Machine Status
        public static MachineStateType MachineStatus = MachineStateType.Idle;
    }
}
