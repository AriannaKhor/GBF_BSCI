using System.Windows.Media;

namespace UIModule.DataMarkers.Interfaces
{
	public interface IMarkerSlot
    {
        int SlotIndex { get; set; }
        int SlotIndicator { get; set; }
        string SlotName { get; set; }
        Brush SlotColor { get; set; }

        bool UnitPsn { get; set; }
        bool EOLUnit { get; set; }
        string UnitID { get; set; }

        double Left { get; set; }
        double Top { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        bool IsVisible { get; set; }
        void RefreshMarkerData();

    }
}
