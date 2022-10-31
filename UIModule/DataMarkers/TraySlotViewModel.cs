using DataManager;
using GreatechApp.Core.Enums;
using Prism.Ioc;
using Sequence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace UIModule.DataMarkers
{
    public class TraySlotViewModel : BaseMarkerSlotViewModel
    {
        #region Variables
        private int m_Size;
        public int Size
        {
            get { return m_Size; }
            set { SetProperty(ref m_Size, value); }
        }

        private int m_RowIndex;
        public int RowIndex
        {
            get { return m_RowIndex; }
            set { SetProperty(ref m_RowIndex, value); }
        }

        private int m_ColIndex;
        public int ColIndex
        {
            get { return m_ColIndex; }
            set { SetProperty(ref m_ColIndex, value); }
        }

        private IBaseData m_IMacData;
        #endregion

        #region Constructor
        public TraySlotViewModel(int rowIndex, int colIndex, SQID seqName)
        {
            m_IMacData = ContainerLocator.Container.Resolve<IDelegateSeq>().GetBaseData(seqName);

            RowIndex = rowIndex;
            ColIndex = colIndex;
            
            SlotColor = Brushes.LightGray;
        }
        #endregion

        #region Methods
        public override void RefreshMarkerData()
        {
            //UnitPsn = m_IMacData.GetUnitPresent(SlotIndex);
            //EOLUnit = m_IMacData.GetEOLUnit(SlotIndex);
            //UnitID = m_IMacData.GetUnitID(SlotIndex);
            //SlotIndicator = m_IMacData.GetSlotID(SlotIndex);

            //foreach (BaseStation station in BaseStationCollection)
            //{
            //    station.StationResult = m_IMacData.GetStationResult(SlotIndex, station.StationId);
            //}

            //if (EOLUnit)
            //{
            //    SlotColor = Brushes.Purple;
            //    UnitStatus = "EOL";
            //}
            //else if (UnitPsn)
            //{
            //    if (BaseStationCollection.Any(x => x.StationResult == StationResult.Fail))
            //    {
            //        SlotColor = Brushes.Red;
            //        UnitStatus = "Present";
            //    }
            //    else if (BaseStationCollection.Any(x => x.StationResult == StationResult.Pass || x.StationResult == StationResult.NONE))
            //    {
            //        SlotColor = Brushes.Green;
            //        UnitStatus = "Present";
            //    }
            //}
            //else
            //{
            //    SlotColor = Brushes.LightGray;
            //    UnitStatus = "Empty";
            //}
        }
        #endregion
    }
}
