using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreatechApp.Core.Modal
{
   public class ProductQuantityParameter : BindableBase
    {
        public int Id { get; set; }

        private string m_Description;
        public string Description
        {
            get { return m_Description; }
            set { SetProperty(ref m_Description, value); }
        }

        private string m_Min;
        public string Min
        {
            get { return m_Min; }
            set { SetProperty(ref m_Min, value); }
        }

        private string m_Max;
        public string Max
        {
            get { return m_Max; }
            set { SetProperty(ref m_Max, value); }
        }
        public int PrevId { get; set; }
        public string PrevDescription { get; set; }
        public string PrevMin { get; set; }
        public string PrevMax { get; set; }
    }
}
