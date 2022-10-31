using GreatechApp.Core.Enums;
using System.Collections.ObjectModel;
using UIModule.DataMarkers;

namespace UIModule.DataMarkers.Interfaces
{
    public interface IMachineData
    {
        SQID SeqName { get; }
        MarkerType DataMarkerType { get; }
        string Title { get; }
        int NumOfMarker { get; }
        bool IsScanStarted { set; }
        ObservableCollection<BaseMarkerSlotViewModel> DataMarkers { get; set; }
        bool CanSelect { get; set; }
        string NavigateView { get; set; }
        
    }
}
