using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreatechApp.Core.Modal
{
    public class Slot : BindableBase
    {
        private int m_SlotID;
        public int SlotID
        {
            get { return m_SlotID; }
            set { SetProperty(ref m_SlotID, value); }
        }

        private int m_TargetSlotID;
        public int TargetSlotID
        {
            get { return m_TargetSlotID; }
            set { SetProperty(ref m_TargetSlotID, value); }
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


        private UnitData m_UnitData = new UnitData();
        public UnitData UnitData
        {
            get { return m_UnitData; }
            set { SetProperty(ref m_UnitData, value); }
        }

        private bool m_IsPresent;
        public bool IsPresent
        {
            get { return m_IsPresent; }
            set { SetProperty(ref m_IsPresent, value); }
        }

        private bool m_IsBypass;
        public bool IsBypass
        {
            get { return m_IsBypass; }
            set { SetProperty(ref m_IsBypass, value); }
        }

        private ObservableCollection<BaseStation> m_StationCollection = new ObservableCollection<BaseStation>();
        public ObservableCollection<BaseStation> StationCollection
        {
            get { return m_StationCollection; }
            set { SetProperty(ref m_StationCollection, value); }
        }

        public void ResetUnit()
        {
            UnitData = new UnitData();
            IsPresent = false;
            IsBypass = false;
            StationCollection = new ObservableCollection<BaseStation>(StationCollection.Select(x => { x.StationResult = StationResult.NONE; return x; }));
        }
    }
}
