using GreatechApp.Core.Enums;
using Prism.Mvvm;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using GreatechApp.Core.Interface;

namespace GreatechApp.Core.Modal
{
    public class UnitData : BindableBase
    {

        private int m_UnitNo;
        public int  UnitNo
        {
            get { return m_UnitNo; }
            set { SetProperty(ref m_UnitNo, value); }
        }

        private string m_UnitID;
        public string UnitID
        {
            get { return m_UnitID; }
            set { SetProperty(ref m_UnitID, value); }
        }

        private string m_LotID;
        public string LotID
        {
            get { return m_LotID; }
            set { SetProperty(ref m_LotID, value); }
        }

        private string m_CarrierID;
        public string CarrierID
        {
            get { return m_CarrierID; }
            set { SetProperty(ref m_CarrierID, value); }
        }

        private int m_Row;
        public int Row
        {
            get { return m_Row; }
            set { SetProperty(ref m_Row, value); }
        }

        private int m_Col;
        public int Col
        {
            get { return m_Col; }
            set { SetProperty(ref m_Col, value); }
        }

        private bool m_IsEOL;
        public bool IsEOL
        {
            get { return m_IsEOL; }
            set { SetProperty(ref m_IsEOL, value); }
        }

    }

    public class BaseStation : BindableBase
    {
        private int m_StationId;
        public int StationId
        {
            get { return m_StationId; }
            set { SetProperty(ref m_StationId, value); }
        }


        private string m_StationName;
        public string StationName
        {
            get { return m_StationName; }
            set { SetProperty(ref m_StationName, value); }
        }

        private StationType m_StationType;
        public StationType StationType
        {
            get { return m_StationType; }
            set { SetProperty(ref m_StationType, value); }
        }

        private StationResult m_StationResult;
        public StationResult StationResult
        {
            get { return m_StationResult; }
            set { SetProperty(ref m_StationResult, value); }
        }
    }

    public enum StationType
    {
        NONE,
        Vision,
        Tester,
        Scanner,
        SecsGem
    }

    public enum StationResult
    {
        NONE = 0,
        Pass,
        Fail,
    }

}
