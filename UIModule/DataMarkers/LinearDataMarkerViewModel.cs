using GreatechApp.Core.Enums;
using ProtoBuf;
using System;
using System.Windows;

namespace UIModule.DataMarkers
{
    [ProtoContract(Name = "LinearDataMarker")]
    public class LinearDataMarkerViewModel : BaseDataMarkerViewModel
    {
        private Point m_StartPoint;
        private Point m_EndPoint;

        public LinearDataMarkerViewModel()
        {
            DataMarkerType = MarkerType.LinearDataMarker;
            StartPoint = new Point(0.0, 0.0);
            EndPoint = new Point(0.0, 0.0);
        }

        public void BuildDataMarker(string markerTitle, SQID seqName, bool isVisible = true)
        {
            base.ConfigureMarker(markerTitle, seqName);

            for (int i = 0; i < NumOfMarker; ++i)
            {
                SlotViewModel marker = new SlotViewModel(i, seqName, NumOfStation);
                marker.IsVisible = isVisible;
                DataMarkers.Add(marker);
            }
        }
        public void SetSlotName(int slotIndex, string slotName)
        {
            DataMarkers[slotIndex].SlotName = slotName;
            DataMarkers[slotIndex].IsVisible = true;
        }

        private Point m_CenterPoint;
        public Point CenterPoint
        {
            get { return m_CenterPoint; }

            set { SetProperty(ref m_CenterPoint, value); }
        }

        public Point StartPoint
        {
            get { return m_StartPoint; }
            set { SetProperty(ref m_StartPoint, value); }
        }

        public Point EndPoint
        {
            get { return m_EndPoint; }
            set { SetProperty(ref m_EndPoint, value); }
        }

        private double Theta(Point p1, Point p2)
        {
            double distX = Math.Abs(p1.X - p2.X);
            double distY = Math.Abs(p1.Y - p2.Y);
            double radians = Math.Atan(distY / distX);
            return radians;
        }

        private double DistanceBetweenPoints(Point p1, Point p2)
        {
            double distX = Math.Abs(p1.X - p2.X);
            double distY = Math.Abs(p1.Y - p2.Y);

            double distance = Math.Sqrt(distX * distX + distY * distY);
            return distance;
        }

        private void RepositionDataMarker(Point startPoint, Point endPoint)
        {
            double distance = DistanceBetweenPoints(startPoint, endPoint);

            double markerGap = NumOfMarker > 1 ? distance / (NumOfMarker - 1) : 0;

            double angle = Theta(StartPoint, EndPoint);

            int i = 0;

            foreach (BaseMarkerSlotViewModel marker in DataMarkers)
            {
                // a = opposite; b = adjacent; c = hypotenuse
                // sinA = a/c; cosA = b/c; tanA = a/b
                if (endPoint.X >= startPoint.X)
                {
                    marker.Left = startPoint.X + (Math.Cos(angle) * markerGap * i);
                }
                else
                {
                    marker.Left = startPoint.X - (Math.Cos(angle) * markerGap * i);
                }

                if (endPoint.Y >= startPoint.Y)
                {
                    marker.Top = startPoint.Y + (Math.Sin(angle) * markerGap * i);
                }
                else
                {
                    marker.Top = startPoint.Y - (Math.Sin(angle) * markerGap * i);
                }

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

                double centerX = Width / 2.0;
                double radiusX = centerX;
                m_StartPoint.X = centerX + (radiusX * Math.Sin(Offset));
                m_EndPoint.X = centerX + (radiusX * Math.Sin(Math.PI + Offset));
                m_CenterPoint.Y = centerX;

                RaisePropertyChanged(nameof(StartPoint));
                RaisePropertyChanged(nameof(EndPoint));
                RaisePropertyChanged(nameof(CenterPoint));
                RepositionDataMarker(StartPoint, EndPoint);
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

                double centerY = Height / 2.0;
                double radiusY = centerY;

                m_StartPoint.Y = centerY - (radiusY * Math.Cos(Offset));
                m_EndPoint.Y = centerY - (radiusY * Math.Cos(Math.PI + Offset));
                m_CenterPoint.Y = Height;

                RaisePropertyChanged(nameof(StartPoint));
                RaisePropertyChanged(nameof(EndPoint));
                RaisePropertyChanged(nameof(CenterPoint));
                RepositionDataMarker(StartPoint, EndPoint);
            }
        }

        [ProtoMember(3)]
        public override double Offset
        {
            get { return m_Offset; }
            set
            {
                m_Offset = value;
                Point center = new Point(Width / 2.0, Height / 2.0);
                Point radius = new Point(Width / 2.0, Height / 2.0);

                m_StartPoint.X = center.X + (radius.X * Math.Sin(0 + value));
                m_StartPoint.Y = center.Y - (radius.Y * Math.Cos(0 + value));

                m_EndPoint.X = center.X + (radius.X * Math.Sin(Math.PI + value));
                m_EndPoint.Y = center.Y - (radius.Y * Math.Cos(Math.PI + value));

                RaisePropertyChanged(nameof(StartPoint));
                RaisePropertyChanged(nameof(EndPoint));
                RepositionDataMarker(StartPoint, EndPoint);
            }
        }
        #endregion
    }
}
