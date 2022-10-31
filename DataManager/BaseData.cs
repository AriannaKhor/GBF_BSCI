using ConfigManager;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Modal;
using Prism.Events;
using System;
using System.Collections.ObjectModel;

namespace DataManager
{
    public class BaseData : IBaseData
    {
        public IEventAggregator Publisher;
        public ObservableCollection<Slot> ModuleSlots { get; set; }
        public ObservableCollection<Slot> TraySlots { get; set; }
        private MarkerType MarkerType { get; set; }
        private SQID seqName { get; set; }
        private struct LocalVar
        {
            internal Slot UnitDataTransfered;
        }
        private LocalVar m_LocalVar = new LocalVar();

        public BaseData(MarkerType markerType, SQID sqid, int numOfSlots, IEventAggregator eventAggregator)
        {
            MarkerType = markerType;
            seqName = sqid;
            Publisher = eventAggregator;

            m_LocalVar.UnitDataTransfered = new Slot();
            Publisher.GetEvent<UnitDataTranfer>().Subscribe(UnitTransferOperation, filter => filter.TargetSeq == seqName);

            SystemConfig systemConfig = SystemConfig.Open(@"..\Config Section\General\System.Config");

            TraySlots = new ObservableCollection<Slot>();
            ModuleSlots = new ObservableCollection<Slot>();
            for (int i = 0; i < numOfSlots; i++)
            {

                Slot slot = new Slot();

                if (MarkerType == MarkerType.CircularDataMarker)
                {
                    slot.SlotID = i == 0 ? i : numOfSlots - i;
                }
                else if (MarkerType == MarkerType.LinearDataMarker)
                {
                    slot.SlotID = i;
                }

                for (int j = 0; j < systemConfig.VisionTesterStations.Count; j++)
                {
                    slot.StationCollection.Add(new BaseStation()
                    {
                        StationId = j,
                        StationName = systemConfig.VisionTesterStations[j].Name,
                        StationType = (StationType)Enum.Parse(typeof(StationType), systemConfig.VisionTesterStations[j].Type),
                        StationResult = StationResult.NONE
                    });
                }

                ModuleSlots.Add(slot);
            }
        }

        private void UnitTransferOperation(UnitDataTranfer newUnitData)
        {
            if(newUnitData.TargetSeq == seqName)
            {
                for(int index = 0;index < newUnitData.UnitInfo.StationCollection.Count;index++)
                {
                    if (newUnitData.UnitInfo.StationCollection[index].StationResult == StationResult.NONE)
                        SetStationResult(newUnitData.TargetSlotIndex, index, StationResult.NONE);
                    else if(newUnitData.UnitInfo.StationCollection[index].StationResult == StationResult.Pass)
                        SetStationResult(newUnitData.TargetSlotIndex, index, StationResult.Pass);
                    else if (newUnitData.UnitInfo.StationCollection[index].StationResult == StationResult.Fail)
                        SetStationResult(newUnitData.TargetSlotIndex, index, StationResult.Fail);
                }
                if (newUnitData.UnitInfo.UnitData.IsEOL)
                    SetEOLUnitPresent(newUnitData.TargetSlotIndex);
                else
                    SetEOLUnitNotPresent(newUnitData.TargetSlotIndex);
                SetUnitCol(newUnitData.TargetSlotIndex, newUnitData.UnitInfo.UnitData.Col);
                SetUnitRow(newUnitData.TargetSlotIndex, newUnitData.UnitInfo.UnitData.Row);
                SetCarrierID(newUnitData.TargetSlotIndex, newUnitData.UnitInfo.UnitData.CarrierID);
                SetLotID(newUnitData.TargetSlotIndex, newUnitData.UnitInfo.UnitData.LotID);

                SetUnitID(newUnitData.TargetSlotIndex, newUnitData.UnitInfo.UnitData.UnitID);
            }
        }

        public void SetDefault()
        {
            foreach (var item in ModuleSlots)
            {
                item.ResetUnit();
            }
        }

        public Slot GetUnitSlot(int slotIndex)
        {
            return ModuleSlots[slotIndex];
        }

        public StationResult GetStationResult(int slotIndex, int stationID)
        {
            return ModuleSlots[slotIndex].StationCollection[stationID].StationResult;
        }

        public bool GetUnitPresent(int slotIndex)
        {
            return ModuleSlots[slotIndex].IsPresent;
        }
        public int GetSlotID(int slotIndex)
        {
            return ModuleSlots[slotIndex].SlotID;
        }

        public bool GetEOLUnit(int slotIndex)
        {
            return ModuleSlots[slotIndex].UnitData.IsEOL;
        }

        public string GetUnitID(int slotIndex)
        {
            return ModuleSlots[slotIndex].UnitData.UnitID ?? "";
        }

        public string GetLotID(int slotIndex)
        {
            return ModuleSlots[slotIndex].UnitData.LotID ?? "";
        }

        public string GetCarrierID(int slotIndex)
        {
            return ModuleSlots[slotIndex].UnitData.CarrierID ?? "";
        }

        public int GetUnitRow(int slotIndex)
        {
            return ModuleSlots[slotIndex].UnitData.Row;
        }

        public int GetUnitCol(int slotIndex)
        {
            return ModuleSlots[slotIndex].UnitData.Col;
        }

        public void SetStationResult(int slotIndex, int stationID, StationResult result)
        {
            ModuleSlots[slotIndex].StationCollection[stationID].StationResult = result;
        }

        public void SetStationResult(int slotIndex, TestStation stationID, StationResult result)
        {
            SetStationResult(slotIndex, (int)stationID, result);
        }

        public void SetUnitPresent(int slotIndex)
        {
            ModuleSlots[slotIndex].IsPresent = true;
        }

        public void SetUnitNotPresent(int slotIndex)
        {
            ModuleSlots[slotIndex].IsPresent = false;
        }

        public void SetEOLUnitPresent(int slotIndex)
        {
            ModuleSlots[slotIndex].UnitData.IsEOL = true;
        }

        public void SetEOLUnitNotPresent(int slotIndex)
        {
            ModuleSlots[slotIndex].UnitData.IsEOL = false;
        }

        public void SetUnitID(int slotIndex, string unitID)
        {
            ModuleSlots[slotIndex].UnitData.UnitID = unitID;
            SetUnitPresent(slotIndex);
        }

        public void SetLotID(int slotIndex, string lotID)
        {
            ModuleSlots[slotIndex].UnitData.LotID = lotID;
        }

        public void SetCarrierID(int slotIndex, string carrierID)
        {
            ModuleSlots[slotIndex].UnitData.CarrierID = carrierID;
        }

        public void SetUnitRow(int slotIndex, int row)
        {
            ModuleSlots[slotIndex].UnitData.Row = row;
        }

        public void ResetUnit(int slotIndex)
        {
            ModuleSlots[slotIndex].ResetUnit();
        }

        public void SetUnitCol(int slotIndex, int col)
        {
            ModuleSlots[slotIndex].UnitData.Col = col;
        }

        public void Shift()
        {
            for (int index = ModuleSlots.Count - 1; index > 0; index--)
            {
                ModuleSlots.Move(index - 1, index);
            }
        }

        public void TransferData(SQID targetSQID, int fromSlotIndex, int toSlotIndex)
        {
            if (fromSlotIndex < 0)
                fromSlotIndex = ModuleSlots.Count - 1;
            
            m_LocalVar.UnitDataTransfered = ModuleSlots[fromSlotIndex];
            Publisher.GetEvent<UnitDataTranfer>().Publish(new UnitDataTranfer { TargetSeq = targetSQID, UnitInfo = m_LocalVar.UnitDataTransfered, TargetSlotIndex = toSlotIndex });
            ModuleSlots[fromSlotIndex].ResetUnit();
        }


        #region Tray Level
        public void TransferFromTray(int rowIndex, int colIndex, SQID targetSQID, int toSloIndex)
        {
            
        }

        public void TrasferToTray(int rowIndex, int colIndex, int fromSlotIndex = -1)
        {

        }
        #endregion

        // Swap marker position in EquipmentView
        // Application: Nest Shuttle 1 & 2
        public void SwapMarkerPosition(SQID Seq1, SQID Seq2)
        {
            Publisher.GetEvent<SwapPosition>().Publish(new SwapPosition { Seq1 = Seq1, Seq2 = Seq2 });
        }
    }
}
