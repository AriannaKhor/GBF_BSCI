using GreatechApp.Core.Enums;
using GreatechApp.Core.Modal;
using System.Collections.ObjectModel;

namespace DataManager
{
    public interface IBaseData
    {
        ObservableCollection<Slot> ModuleSlots { get; set; }
        void SetDefault();
        Slot GetUnitSlot(int slotIndex);
        StationResult GetStationResult(int slotIndex, int stationID);
        bool GetUnitPresent(int slotIndex);
        bool GetEOLUnit(int slotIndex);
        int GetSlotID(int slotIndex);
        string GetUnitID(int slotIndex);
        string GetLotID(int slotIndex);
        string GetCarrierID(int slotIndex);
        int GetUnitRow(int slotIndex);
        int GetUnitCol(int slotIndex);
        void SetStationResult(int slotIndex, int stationID, StationResult result);
        void SetStationResult(int slotIndex, TestStation stationID, StationResult result);
        void SetUnitPresent(int slotIndex);
        void SetUnitNotPresent(int slotIndex);
        void SetEOLUnitPresent(int slotIndex);
        void SetEOLUnitNotPresent(int slotIndex);
        void SetUnitID(int slotIndex, string unitID);
        void SetLotID(int slotIndex, string lotID);
        void SetCarrierID(int slotIndex, string carrierID);
        void SetUnitRow(int slotIndex, int row);
        void SetUnitCol(int slotIndex, int col);

        void ResetUnit(int slotIndex);
        void Shift();
        void TransferData(SQID targetSQID, int fromSlotIndex = -1, int toSlotIndex = 0);
        void SwapMarkerPosition(SQID Seq1, SQID Seq2);

    }
}
