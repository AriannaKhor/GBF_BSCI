using GreatechApp.Core.Enums;
using Prism.Commands;
using Prism.Mvvm;

namespace GreatechApp.Core.Modal
{
    public class CylinderIOParameters : BindableBase
    {
        public CylinderIOParameters()
        {

        }

        private string m_CylinderName;
        public string CylinderName
        {
            get { return m_CylinderName; }
            set { SetProperty(ref m_CylinderName, value); }
        }


        public DelegateCommand<CylinderIOParameters> WorkCommand { get; set; }
        public DelegateCommand<CylinderIOParameters> RestCommand { get; set; }
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

        private OUT m_Work;
        public OUT Work
        {
            get { return m_Work; }
            set { SetProperty(ref m_Work, value); }
        }

        public string WorkTooltip1
        {
            get { return m_WorkSns1 == null ? null: m_WorkSns1.ToString(); }
        }

        public string WorkTooltip2
        {
            get { return m_WorkSns2 == null? null: m_WorkSns2.ToString(); }
        }

        public string RestTooltip1
        {
            get { return m_RestSns1 == null? null: m_RestSns1.ToString(); }
        }

        public string RestTooltip2
        {
            get { return m_RestSns2 == null ? null : m_RestSns2.ToString(); }
        }

        public string WorkCylinderTooltip
        {
            get { return m_Work.ToString(); }
        }

        public string RestCylinderTooltip
        {
            get { return m_Rest == null ? null : m_Rest.ToString(); }
        }

        private bool m_IsCylinderWork;
        public bool IsCylinderWork
        {
            get { return m_IsCylinderWork; }
            set { SetProperty(ref m_IsCylinderWork, value); }
        }

        private bool? m_IsWorkSns1;
        public bool? IsWorkSns1
        {
            get { return m_IsWorkSns1; }
            set { SetProperty(ref m_IsWorkSns1, value); }
        }
        private bool? m_IsWorkSns2;
        public bool? IsWorkSns2
        {
            get { return m_IsWorkSns2; }
            set { SetProperty(ref m_IsWorkSns2, value); }
        }
        private bool? m_IsRestSns1;
        public bool? IsRestSns1
        {
            get { return m_IsRestSns1; }
            set { SetProperty(ref m_IsRestSns1, value); }
        }
        private bool? m_IsRestSns2;
        public bool? IsRestSns2
        {
            get { return m_IsRestSns2; }
            set { SetProperty(ref m_IsRestSns2, value); }
        }
    }
}
