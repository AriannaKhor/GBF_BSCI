using Prism.Mvvm;

namespace GreatechApp.Core.Modal
{
    public class LotInfo : BindableBase
    {
        private string m_LotID;
        public string LotID 
        {
            get { return m_LotID; }
            set { SetProperty(ref m_LotID, value); }
        }

        private string m_UnitID;
        public string UnitID
        {
            get { return m_UnitID; }
            set { SetProperty(ref m_UnitID, value); }
        }

        private string m_CarrierID;

        public string CarrierID
        {
            get { return m_CarrierID; }
            set { SetProperty(ref m_CarrierID, value); }
        }


        public LotInfo()
        {
            InitData();
        }

        public void InitData()
        {
            LotID = string.Empty;
            UnitID = string.Empty;
            CarrierID = string.Empty;
            // Project: Add project information here.
        }

        
    }
}
