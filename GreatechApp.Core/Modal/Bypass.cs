using Prism.Mvvm;

namespace GreatechApp.Core.Modal
{
    public class Bypass :BindableBase
    {
        private bool m_IsBypass;
        private bool m_IsEnabled;

        public Bypass(int id)
        {
            ID = id;
            InitData();
        }

        public Bypass(int id, string desc, bool bypass)
            : this(id)
        {
            Description = desc;
            IsBypass = bypass;
        }

        public void InitData()
        {
            Description = string.Empty;
            IsBypass = false;
            // Visiblilty is true by default
            IsVisible = true;
            IsEnabled = true;
        }

        public int ID { get; set; }

        public string Description { get; set; }

        public bool IsBypass
        {
            get { return m_IsBypass; }
            set { SetProperty(ref m_IsBypass, value); }
        }

        public bool IsVisible { get; set; }

        public bool IsEnabled
        {
            get { return m_IsEnabled; }
            set { SetProperty(ref m_IsEnabled, value); }
        }
    }
}
