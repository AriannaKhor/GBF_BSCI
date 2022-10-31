using Prism.Mvvm;
using System;
using System.Windows.Media.Imaging;

namespace GreatechApp.Core.Modal
{
    public class IOList : BindableBase
    {
        public int Tag { get; set; }

        private string m_Assignment;
        public string Assignment
        {
            get { return m_Assignment; }
            set
            {
                string[] wiringLabel = value.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (wiringLabel.Length == 2)
                {
                    m_Assignment = wiringLabel[0].Trim();
                }
                else
                {
                    m_Assignment = value;
                }
                RaisePropertyChanged(nameof(Assignment));
            }
        }

        private string m_Description;
        public string Description 
        {
            get { return m_Description; }
            set 
            {
                string[] wiringLabel = value.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (wiringLabel.Length == 2)
                {
                    m_Description = wiringLabel[1].Trim();
                }
                else
                {
                    m_Description = value;
                }
                RaisePropertyChanged(nameof(Description));
            }
        }

        private bool m_Status = false;
        public bool Status
        {
            get { return m_Status; }
            set { SetProperty(ref m_Status, value); }
        }
    }
}
