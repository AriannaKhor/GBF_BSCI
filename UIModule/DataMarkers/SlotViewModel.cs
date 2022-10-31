using ConfigManager;
using DataManager;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Modal;
using Prism.Commands;
using Prism.Ioc;
using Sequence;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace UIModule.DataMarkers
{
    public class SlotViewModel : BaseMarkerSlotViewModel
    {
        #region Variables
        private ObservableCollection<BaseStation> m_BaseStationCollection;
        public ObservableCollection<BaseStation> BaseStationCollection
        {
            get { return m_BaseStationCollection; }
            set { SetProperty(ref m_BaseStationCollection, value); }
        }

        private SlotNamePos m_SlotNamePos;
        public SlotNamePos SlotNamePos
        {
            get { return m_SlotNamePos; }
            set { SetProperty(ref m_SlotNamePos, value); }
        }


        public DelegateCommand RotateSlotNamePositionCommand { get; set; }

        private IBaseData m_IMacData;
        private SystemConfig m_SysCfg;
        #endregion

        #region Constructor
        public SlotViewModel(int slotIndex, SQID seqName, int stationNum)
        {
            m_IMacData = ContainerLocator.Container.Resolve<IDelegateSeq>().GetBaseData(seqName);
            m_SysCfg = ContainerLocator.Container.Resolve<SystemConfig>();

            SlotIndex = slotIndex;

            SlotColor = new SolidColorBrush(Colors.LightGray);
            BaseStationCollection = new ObservableCollection<BaseStation>();
            for(int i = 0; i < stationNum; i++)
            {
                BaseStationCollection.Add(new BaseStation()
                {
                    StationId = i,
                    StationName = m_SysCfg.VisionTesterStations[i].Name,
                    StationType = (StationType)Enum.Parse(typeof(StationType), m_SysCfg.VisionTesterStations[i].Type),
                    StationResult = StationResult.NONE
                });
            }

            RotateSlotNamePositionCommand = new DelegateCommand(OnRotateSlotNamePosition);

            // Formula to calculate inner circle: https://math.stackexchange.com/questions/121272/calculate-radius-of-variable-circles-surrounding-big-circle
            // Radius of IconStatus UserControl
            double r = 17 / 2;
            double minInnerRadius = 12.5;
            double innerRadius = r * (1 - Math.Sin(Math.PI / stationNum)) / (Math.Sin(Math.PI / stationNum));

            InnerRadius = innerRadius > minInnerRadius ? innerRadius : minInnerRadius;
            OuterRadius = InnerRadius + 2*r;

            double minUnitStatusSize = 20;
            UnitStatusSize = innerRadius > minInnerRadius ? InnerRadius * 2 * 0.5 : minUnitStatusSize;

        }
        #endregion

        #region Methods
        public override void RefreshMarkerData()
        {
            UnitPsn = m_IMacData.GetUnitPresent(SlotIndex);
            EOLUnit = m_IMacData.GetEOLUnit(SlotIndex);
            UnitID = m_IMacData.GetUnitID(SlotIndex);
            SlotIndicator = m_IMacData.GetSlotID(SlotIndex);

            foreach (BaseStation station in BaseStationCollection)
            {
                station.StationResult = m_IMacData.GetStationResult(SlotIndex, station.StationId);
            }

            if (EOLUnit)
            {
                SlotColor = Brushes.Purple;
                UnitStatus = "EOL";
            }
            else if (UnitPsn)
            {
                if (BaseStationCollection.Any(x => x.StationResult == StationResult.Fail))
                {
                    SlotColor = Brushes.Red;
                    UnitStatus = "Present";
                }
                else
                {
                    SlotColor = Brushes.Green;
                    UnitStatus = "Present";
                }
            }
            else
            {
                SlotColor = Brushes.LightGray;
                UnitStatus = "Empty";
            }
        }

        private void OnRotateSlotNamePosition()
        {
            SlotNamePos = (int)SlotNamePos == 3 ? 0 : (SlotNamePos + 1);
        }
        #endregion
    }
}
