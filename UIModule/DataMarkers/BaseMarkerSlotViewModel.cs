using Prism.Mvvm;
using System.Windows;
using System.Windows.Media;
using UIModule.DataMarkers.Interfaces;

namespace UIModule.DataMarkers
{
    public class BaseMarkerSlotViewModel : BindableBase, IMarkerSlot
    {
        #region Variables
        private Point m_DropPoint = new Point();
        private double m_ActualWidth = 0.0;
        private double m_ActualHeight = 0.0;
        private bool m_IsVisible;
        private Visibility m_SimpleVisibility;

        // This is the index number for slot. The number will remain when rotary object is rotate.
        private int m_SlotIndex;
        public int SlotIndex
        {
            get { return m_SlotIndex; }
            set { SetProperty(ref m_SlotIndex, value); }
        }

        // This number is the label number for slot. The number will changed when rotary object is rotate.
        private int m_SlotIndicator;
        public int SlotIndicator
        {
            get { return m_SlotIndicator; }
            set { SetProperty(ref m_SlotIndicator, value); }
        }

        private double m_InnerRadius = 12.5;
        public double InnerRadius
        {
            get { return m_InnerRadius; }
            set { SetProperty(ref m_InnerRadius, value); }
        }

        private double m_UnitStatusSize = 20;
        public double UnitStatusSize
        {
            get { return m_UnitStatusSize; }
            set { SetProperty(ref m_UnitStatusSize, value); }
        }

        private double m_OuterRadius = 27;
        public double OuterRadius
        {
            get { return m_OuterRadius; }
            set { SetProperty(ref m_OuterRadius, value); }
        }

        private string m_SlotName =" ";
        public string SlotName
        {
            get { return m_SlotName; }
            set { SetProperty(ref m_SlotName, value); }
        }

        private Brush m_SlotColor;
        public Brush SlotColor
        {
            get { return m_SlotColor; }
            set { SetProperty(ref m_SlotColor, value); }
        }

        private bool m_UnitPsn;
        public bool UnitPsn
        {
            get { return m_UnitPsn; }
            set { SetProperty(ref m_UnitPsn, value); }
        }

        private bool m_EOLUnit;
        public bool EOLUnit
        {
            get { return m_EOLUnit; }
            set { SetProperty(ref m_EOLUnit, value); }
        }

        private string m_UnitID;
        public string UnitID
        {
            get { return m_UnitID; }
            set { SetProperty(ref m_UnitID, value); }
        }

        private string m_UnitStatus;
        public string UnitStatus
        {
            get { return m_UnitStatus; }
            set { SetProperty(ref m_UnitStatus, value); }
        }
        #endregion

        #region Marker Properties
        public double Left
        {
            get { return m_DropPoint.X; }

            set
            {
                // When user resize interactively.
                m_DropPoint.X = value - (Width / 2);
                RaisePropertyChanged(nameof(Left));
            }
        }

        public double Top
        {
            get { return m_DropPoint.Y; }

            set
            {
                // When user resize interactively.
                m_DropPoint.Y = value - (Height / 2);
                RaisePropertyChanged(nameof(Top));
            }
        }

        // Track the actual width and height of this user control.
        // Will use these information to place the control right in the center.
        public double Width
        {
            get { return m_ActualWidth; }

            set
            {
                m_ActualWidth = value;
                RaisePropertyChanged(nameof(Width));
                RaisePropertyChanged(nameof(Left));
            }
        }

        public double Height
        {
            get { return m_ActualHeight; }

            set
            {
                m_ActualHeight = value;
                RaisePropertyChanged(nameof(Height));
                RaisePropertyChanged(nameof(Top));
            }
        }

        public bool IsVisible
        {
            get { return m_IsVisible; }
            set { SetProperty(ref m_IsVisible, value); }
        }

        public Visibility SimpleVisibility
        {
            get { return m_SimpleVisibility; }
            set { SetProperty(ref m_SimpleVisibility, value); }
        }
        #endregion

        #region Methods
        public virtual void RefreshMarkerData()
        {
            
        }
        #endregion
    }
}
