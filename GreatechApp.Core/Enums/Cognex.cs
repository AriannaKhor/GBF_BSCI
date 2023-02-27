using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreatechApp.Core.Enums
{
    public enum resultstatus
    {
        PendingResult,
        OK,
        NG,
        NoBoxDetected,
    }

    public enum inspectiontype
    {
        TopVision,
    }

    public enum resultseqtyp
    {
        [Description("Slip Sheet")]
        SlipSheet = 1,
        [Description("Reverse Pouch")]
        ReversePouch,
        [Description("Color Pouch")]
        ColorPouch,
        [Description("Invert Color Pouch")]
        InvertColorPouch,
        [Description("DFU")]
        DFU,
        [Description("Check Empty")]
        CheckEmpty,
    }
}
