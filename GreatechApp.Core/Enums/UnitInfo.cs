using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreatechApp.Core.Enums
{
    public enum LocID
    {
        None = 0,

        TrayIDIN,
        TrayIDOUT1,
        TrayIDOUT2,
        TrayIDOUT3,

        PnP_I1,
        PnP_I2,
        PnP_I3,
        PnP_I4,
        PnP_I5,
        PnP_I6,
        PnP_I7,

        NestShuttle_In1_1,
        NestShuttle_In1_2,
        NestShuttle_In1_3,
        NestShuttle_In1_4,
        NestShuttle_In1_5,
        NestShuttle_In1_6,
        NestShuttle_In1_7,

        NestShuttle_In2_1,
        NestShuttle_In2_2,
        NestShuttle_In2_3,
        NestShuttle_In2_4,
        NestShuttle_In2_5,
        NestShuttle_In2_6,
        NestShuttle_In2_7,

        MT_NestShuttleIN,
        MT_Spare1,
        MT_Precisor1,
        MT_Rotator,
        MT_Spare2,
        MT_RotaryTable12,
        MT_Spare3,
        MT_RotaryTable10,
        MT_Spare4,
        MT_Precisor2,
        MT_Vision1,
        MT_Vision2,
        MT_Vision3,
        MT_Spare5,
        MT_RotaryTable12B,
        MT_Spare6,
        MT_Precisor3,
        MT_Spare7,
        MT_TapeReel1,
        MT_Spare8,
        MT_TapeReel2,
        MT_Spare9,
        MT_FinalPurgeBin,
        MT_Spare10,

        VisionPlace_1,
        VisionPlace_2,
        VisionPlace_3,

        RotaryTable12_InputFromMainTurret,
        RotaryTable12_2,
        RotaryTable12_3,
        RotaryTable12_4,
        RotaryTable12_5,
        RotaryTable12_6,
        RotaryTable12_7,
        RotaryTable12_OutputToPNPA,
        RotaryTable12_9,
        RotaryTable12_10,
        RotaryTable12_11,
        RotaryTable12_12,

        PNP_A_Input,
        PNP_A_Output,

        TesterSubTurret_1,
        TesterSubTurret_2,
        TesterSubTurret_3,
        TesterSubTurret_4,
        TesterSubTurret_5,
        TesterSubTurret_6,
        TesterSubTurret_7,
        TesterSubTurret_8,
        TesterSubTurret_9,
        TesterSubTurret_10,

        RotaryTable10_InputFromPNPB,
        RotaryTable10_2,
        RotaryTable10_3,
        RotaryTable10_4,
        RotaryTable10_5,
        RotaryTable10_6,
        RotaryTable10_OutputToMainTurret,
        RotaryTable10_8,
        RotaryTable10_9,
        RotaryTable10_10,

        PNP_B_Input,
        PNP_B_Output,

        VisionPlace_4,
        VisionPlace_5,

        RotaryTable12B_InputFromMainTurret,
        RotaryTable12B_2,
        RotaryTable12B_3,
        RotaryTable12B_4,
        RotaryTable12B_5,
        RotaryTable12B_OutputToPNPC,
        RotaryTable12B_7,
        RotaryTable12B_8,
        RotaryTable12B_9,
        RotaryTable12B_10,
        RotaryTable12B_11,
        RotaryTable12B_12,

        PNP_C_Input,
        PNP_C_Output,

        NestShuttle_Out1_1,
        NestShuttle_Out1_2,
        NestShuttle_Out1_3,
        NestShuttle_Out1_4,
        NestShuttle_Out1_5,
        NestShuttle_Out1_6,
        NestShuttle_Out1_7,

        NestShuttle_Out2_1,
        NestShuttle_Out2_2,
        NestShuttle_Out2_3,
        NestShuttle_Out2_4,
        NestShuttle_Out2_5,
        NestShuttle_Out2_6,
        NestShuttle_Out2_7,

        PnP_O1,
        PnP_O2,
        PnP_O3,
        PnP_O4,
        PnP_O5,
        PnP_O6,
        PnP_O7,

        TapeReel_1,
        TapeReel_2,
        TapeReel_3,
        TapeReel_4,
        TapeReel_5,
        TapeReel_6,
        TapeReel_7,
        TapeReel_8,
        TapeReel_9,
        TapeReel_10,
        TapeReel_11,
        TapeReel_12,
        TapeReel_13,
        TapeReel_14,
        TapeReel_15,
        TapeReel_16,
        TapeReel_17,
        TapeReel_18,
        TapeReel_19,
        TapeReel_20,
        TapeReel_21,
        TapeReel_22,
        TapeReel_23,
        TapeReel_24,
        TapeReel_25,
        TapeReel_26,
        TapeReel_27,
        TapeReel_28,
        TapeReel_29,
        TapeReel_30,
        TapeReel_31,
        TapeReel_32,
        TapeReel_33,
        TapeReel_34,
        TapeReel_35,
        TapeReel_36,
        TapeReel_37,
        TapeReel_38,
    }

    public enum In_NestShuttle1Loc
    {
        None = 0,
        TrayShuttle_Loc0,
        TrayShuttle_Loc1,
        TrayShuttle_Loc2,
        TrayShuttle_RetractLoc,
        TrayShuttle_ExtendLoc
    }

    public enum In_NestShuttle2Loc
    {
        None = 0,
        TrayShuttle_Loc0,
        TrayShuttle_Loc1,
        TrayShuttle_Loc2,
        TrayShuttle_RetractLoc,
        TrayShuttle_ExtendLoc
    }

    public enum Out_NestShuttle1Loc
    {
        None = 0,
        TrayShuttle_Loc0,
        TrayShuttle_Loc1,
        TrayShuttle_Loc2,
        TrayShuttle_RetractLoc,
        TrayShuttle_ExtendLoc
    }

    public enum Out_NestShuttle2Loc
    {
        None = 0,
        TrayShuttle_Loc0,
        TrayShuttle_Loc1,
        TrayShuttle_Loc2,
        TrayShuttle_RetractLoc,
        TrayShuttle_ExtendLoc
    }
}
