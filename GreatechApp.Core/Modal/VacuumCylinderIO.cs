using GreatechApp.Core.Enums;
using Prism.Mvvm;

namespace GreatechApp.Core.Modal
{
    public class VacuumCylinderIO : BindableBase
    {
        private SQID m_SeqName;
        public SQID SeqName
        {
            get { return m_SeqName; }
            set { SetProperty(ref m_SeqName, value); }
        }

        #region Cylinder
        private string m_CylinderName;
        public string CylinderName
        {
            get { return m_CylinderName; }
            set { SetProperty(ref m_CylinderName, value); }
        }

        private IN? m_WorkSns1;
        public IN? WorkSns1
        {
            get { return m_WorkSns1; }
            set { SetProperty(ref m_WorkSns1, value); }
        }

        private IN? m_WorkSns2;
        public IN? WorkSns2
        {
            get { return m_WorkSns2; }
            set { SetProperty(ref m_WorkSns2, value); }
        }

        private IN? m_RestSns1;
        public IN? RestSns1
        {
            get { return m_RestSns1; }
            set { SetProperty(ref m_RestSns1, value); }
        }

        private IN? m_RestSns2;
        public IN? RestSns2
        {
            get { return m_RestSns2; }
            set { SetProperty(ref m_RestSns2, value); }
        }

        private OUT? m_Rest;
        public OUT? Rest
        {
            get { return m_Rest; }
            set { SetProperty(ref m_Rest, value); }
        }

        private OUT? m_Work;
        public OUT? Work
        {
            get { return m_Work; }
            set { SetProperty(ref m_Work, value); }
        }
        #endregion

        #region VacuumPurge
        private string m_VacuumName;
        public string VacuumName
        {
            get { return m_VacuumName; }
            set { SetProperty(ref m_VacuumName, value); }
        }

        private IN? m_VacuumPressureSns1;
        public IN? VacuumPressureSns1
        {
            get { return m_VacuumPressureSns1; }
            set { SetProperty(ref m_VacuumPressureSns1, value); }
        }

        private IN? m_VacuumPickedUpSns1;
        public IN? VacuumPickedUpSns1
        {
            get { return m_VacuumPickedUpSns1; }
            set { SetProperty(ref m_VacuumPickedUpSns1, value); }
        }

        private OUT? m_Vacuum;
        public OUT? Vacuum
        {
            get { return m_Vacuum; }
            set { SetProperty(ref m_Vacuum, value); }
        }

        private OUT? m_Purge;
        public OUT? Purge
        {
            get { return m_Purge; }
            set { SetProperty(ref m_Purge, value); }
        }
        #endregion

        public VacuumCylinderIO()
        {
            
        }
    }
}
