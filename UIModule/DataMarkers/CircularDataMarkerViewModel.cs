using GreatechApp.Core.Enums;
using ProtoBuf;
using System;
using System.Windows;

namespace UIModule.DataMarkers
{
    [ProtoContract(Name = "CircularDataMarker")]
    public class CircularDataMarkerViewModel : BaseDataMarkerViewModel
    {
        private Point m_CenterPoint;
        private double m_RadiusX;
        private double m_RadiusY;
        private double m_Angle;

        public CircularDataMarkerViewModel()
        {
            DataMarkerType =  MarkerType.CircularDataMarker;
            CenterPoint = new Point();
        }

        public void BuildDataMarker(string markerTitle, SQID seqName , UnitFlowDir unitDir, bool isVisible = true)
        {
            base.ConfigureMarker(markerTitle, seqName);
            int markID = 0;
            m_UnitFlowDir = unitDir;

            for (int i = 0; i < NumOfMarker; ++i)
            {
                if(m_UnitFlowDir == UnitFlowDir.CCW)
                {
                    markID = i == 0 ? i : NumOfMarker - i;
                }
                else
                {
                    markID = i;
                }

                SlotViewModel marker = new SlotViewModel(markID, seqName, NumOfStation);
                marker.IsVisible = isVisible;
                DataMarkers.Add(marker);
            }

            m_Angle = (360.0 / NumOfMarker) * (Math.PI / 180.0);
            Offset = 0.0;
        }

        public void SetSlotName(int slotIndex, string slotName)
        {
            if (m_UnitFlowDir == UnitFlowDir.CCW)
            {
                if (slotIndex != 0)
                {
                    DataMarkers[NumOfMarker - slotIndex].SlotName = slotName;
                    DataMarkers[NumOfMarker - slotIndex].IsVisible = true;
                }
                else
                {
                    DataMarkers[0].SlotName = slotName;
                    DataMarkers[0].IsVisible = true;
                }
            }
            else
            {
                DataMarkers[slotIndex].SlotName = slotName;
                DataMarkers[slotIndex].IsVisible = true;
            }
        }

        public Point CenterPoint
        {
            get { return m_CenterPoint; }

            set { SetProperty(ref m_CenterPoint, value); }
        }

        public double RadiusX
        {
            get { return m_RadiusX; }

            private set
            {
                m_RadiusX = value;
                RaisePropertyChanged(nameof(RadiusX));
                m_CenterPoint.X = value;
                RaisePropertyChanged(nameof(CenterPoint));
                RepositionDataMarker(new Point(value, RadiusY), CenterPoint, Offset);
            }
        }

        public double RadiusY
        {
            get { return m_RadiusY; }

            private set
            {
                m_RadiusY = value;
                RaisePropertyChanged(nameof(RadiusY));
                m_CenterPoint.Y = value;
                RaisePropertyChanged(nameof(CenterPoint));
                RepositionDataMarker(new Point(RadiusX, value), CenterPoint, Offset);
            }
        }

        private void RepositionDataMarker(Point radius, Point center, double offset)
        {
            int i = 0;
            foreach (SlotViewModel marker in DataMarkers)
            {
                marker.Left = center.X + (radius.X * Math.Sin((m_Angle * i) + offset));
                // For Y-Axis (screen coordinate), use "-" minus because the value increase 
                // from Top -> Bottom.
                marker.Top = center.Y - (radius.Y * Math.Cos((m_Angle * i) + offset));
                ++i;
            }
        }

        #region IDesignerItem Interface Implementation
        [ProtoMember(1)]
        public override double Width
        {
            get { return m_Width; }
            set
            {
                m_Width = value;
                RaisePropertyChanged(nameof(Width));
                RadiusX = value / 2.0;
                RaisePropertyChanged(nameof(CenterPoint));
            }
        }

        [ProtoMember(2)]
        public override double Height
        {
            get { return m_Height; }
            set
            {
                m_Height = value;
                RaisePropertyChanged(nameof(Height));
                RadiusY = value / 2.0;
                RaisePropertyChanged(nameof(CenterPoint));
            }
        }

        [ProtoMember(3)]
        public override double Offset
        {
            get { return m_Offset; }
            set
            {
                m_Offset = value;
                RaisePropertyChanged(nameof(Offset));
                RepositionDataMarker(new Point(RadiusX, RadiusY), CenterPoint, value);
            }
        }
        #endregion
    }
}
