using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace GreatechApp.Core.Interface
{
    public interface IOEECalculation
    {
        double OEE { get; set; }
        double Availability { get; set; }
        double Performance { get; set; }
        double Quality { get; set; }
        double TotalRunTime { get; set; }
        double TotalHoursRunTime { get; set; }
        int CurrentShift { get; set; }
        int ShiftCount { get; set; }
        double RunTime { get; }

        TimeSpan ShiftCountDownTimer { get; set; }
        TimeSpan ShiftStartTime { get; set; }

        int TotalInput { get; set; }
        int TotalOutput { get; set; }
        double PlannedDowntime { get; set; }
        double UnplannedDowntime { get; set; }
        DateTime CurrentShiftBeginDateTime { get; set; }
        int CurrentShiftNo { get; set; }

        DateTime curShifBegin { get; set; }
        double PlannedProductionTime { get; }
        double IdealCycleTime { get; set; }
        int MaxProductPerShift { get; set; }
        int MaxProductMajorStep { get; set; }

        Dictionary<int, int> ProductOutputDict { get; set; }
        DateTime StartTime { get; set; }

        List<string> PassTime { get; set; }
        List<SolidColorBrush> MachineStateColor { get; set; }
    }
}
