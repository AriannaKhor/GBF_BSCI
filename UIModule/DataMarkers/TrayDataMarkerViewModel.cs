using GreatechApp.Core.Enums;
using GreatechApp.Core.Modal;
using ProtoBuf;
using System;
using System.Windows;
using System.Windows.Media;

namespace UIModule.DataMarkers
{
    [ProtoContract(Name = "TrayDataMarker")]
    public class TrayDataMarkerViewModel : BaseDataMarkerViewModel
    {
        private Point m_StartPoint;
        private Point m_EndPoint;
        private int m_RowNum;
        private int m_ColNum;
        private int m_MakerSize;

        public TrayDataMarkerViewModel()
        {
            DataMarkerType = MarkerType.TrayDataMarker;
            StartPoint = new Point(0.0, 0.0);
            EndPoint = new Point(0.0, 0.0);
        }

        public void BuildDataMarker(string markerTitle, SQID seqName, int colNum, int rowNum)
        {
            base.ConfigureMarker(markerTitle, seqName);

            m_RowNum = rowNum;
            m_ColNum = colNum;
            m_MakerSize = 8;
            for (int i = 0; i < NumOfMarker; ++i)
            {
                TraySlotViewModel marker = new TraySlotViewModel(i / colNum, i % colNum, seqName);
                marker.RowIndex = i / colNum;
                marker.ColIndex = i % colNum;
                marker.Size = m_MakerSize;
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
  

        private void RepositionDataMarker(Point startPoint, Point endPoint)
        {
            double margin = 5;
            double horizontalGap = ((endPoint.X - startPoint.X - 2*margin) - (m_ColNum * m_MakerSize)) / (m_ColNum - 1);
            double verticalGap = ((endPoint.Y - startPoint.Y - 2*margin) - (m_RowNum * m_MakerSize)) / (m_RowNum - 1);

            foreach (TraySlotViewModel marker in DataMarkers)
            {
                marker.Left = startPoint.X + margin + marker.ColIndex * m_MakerSize + horizontalGap * marker.ColIndex;
                marker.Top = startPoint.Y + margin + (marker.RowIndex * m_MakerSize + verticalGap * marker.RowIndex);
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

                m_StartPoint.X = 0;
                m_EndPoint.X = value;

                RaisePropertyChanged(nameof(StartPoint));
                RaisePropertyChanged(nameof(EndPoint));
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

                m_StartPoint.Y = 0;
                m_EndPoint.Y = value;

                RaisePropertyChanged(nameof(StartPoint));
                RaisePropertyChanged(nameof(EndPoint));
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
                //Point center = new Point(Width / 2.0, Height / 2.0);
                //Point radius = new Point(Width / 2.0, Height / 2.0);

                //m_StartPoint.X = center.X + (radius.X * Math.Sin(0 + value));
                //m_StartPoint.Y = center.Y - (radius.Y * Math.Cos(0 + value));

                //m_EndPoint.X = center.X + (radius.X * Math.Sin(Math.PI + value));
                //m_EndPoint.Y = center.Y - (radius.Y * Math.Cos(Math.PI + value));

                //RaisePropertyChanged(nameof(StartPoint));
                //RaisePropertyChanged(nameof(EndPoint));
                RepositionDataMarker(StartPoint, EndPoint);
            }
        }
        #endregion
    }
}
