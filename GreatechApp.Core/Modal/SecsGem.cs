using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreatechApp.Core.Modal
{
   public class SecsGem : BindableBase
    {

        #region Properties
        private int m_DeviceID;
        public int DeviceID
        {
            get { return m_DeviceID; }
            set { SetProperty(ref m_DeviceID, value); }
        }

        private string m_IpAddress;
        public string IpAddress
        {
            get { return m_IpAddress; }
            set { SetProperty(ref m_IpAddress, value); }
        }

        private int m_PortID;
        public int PortID
        {
            get { return m_PortID; }
            set { SetProperty(ref m_PortID, value); }
        }
        private int m_t3;
        public int t3
        {
            get { return m_t3; }
            set { SetProperty(ref m_t3, value); }
        }
        private int m_t5;
        public int t5
        {
            get { return m_t5; }
            set { SetProperty(ref m_t5, value); }
        }
        private int m_t6;
        public int t6
        {
            get { return m_t6; }
            set { SetProperty(ref m_t6, value); }
        }

        private int m_t7;
        public int t7
        {
            get { return m_t7; }
            set { SetProperty(ref m_t7, value); }
        }
        private int m_t8;
        public int t8
        {
            get { return m_t8; }
            set { SetProperty(ref m_t8, value); }
        }
        private string m_DbDir;
        public string DbDir
        {
            get { return m_DbDir; }
            set { SetProperty(ref m_DbDir, value); }
        }

        private string m_RcpDir;
        public string RcpDir
        {
            get { return m_RcpDir; }
            set { SetProperty(ref m_RcpDir, value); }
        }

    



        #endregion


    }
}
