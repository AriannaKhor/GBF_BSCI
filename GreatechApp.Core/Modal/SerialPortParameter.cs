using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Text;

namespace GreatechApp.Core.Modal
{
	public class SerialPortParameter : BindableBase
	{
        private StringBuilder m_ErrMsg = new StringBuilder();

        public SerialPortParameter()
		{
            #region Port Collection
            PortCollection = new ObservableCollection<string>();
            for (int i = 0; i < 10; i++)
            {
                PortCollection.Add(string.Format("COM{0}", i + 1));
            }

            #endregion

            #region Baud Rate
            BaudRateCollection = new ObservableCollection<int>();
            BaudRateCollection.Add(256000);
            BaudRateCollection.Add(128000);
            BaudRateCollection.Add(115200);
            BaudRateCollection.Add(57600);
            BaudRateCollection.Add(56000);
            BaudRateCollection.Add(43000);
            BaudRateCollection.Add(38400);
            BaudRateCollection.Add(28800);
            BaudRateCollection.Add(19200);
            BaudRateCollection.Add(9600);
            BaudRateCollection.Add(4800);
            #endregion

            #region Parity Bit
            ParityBitCollection = new ObservableCollection<Parity>();
            ParityBitCollection.Add(Parity.None);
            ParityBitCollection.Add(Parity.Odd);
            ParityBitCollection.Add(Parity.Even);
            ParityBitCollection.Add(Parity.Mark);
            ParityBitCollection.Add(Parity.Space);
            #endregion

            #region Data Bit
            DataBitCollection = new ObservableCollection<int>();
            for (int i = 8; i >= 6; i--)
            {
                DataBitCollection.Add(i);
            }
            #endregion

            #region Stop Bit
            StopBitCollection = new ObservableCollection<StopBits>();
            StopBitCollection.Add(StopBits.One);
            StopBitCollection.Add(StopBits.OnePointFive);
            StopBitCollection.Add(StopBits.Two);
            #endregion

            #region HandShake
            HandShakeCollection = new ObservableCollection<Handshake>();
            HandShakeCollection.Add(Handshake.None);
            HandShakeCollection.Add(Handshake.RequestToSend);
            HandShakeCollection.Add(Handshake.RequestToSendXOnXOff);
            HandShakeCollection.Add(Handshake.XOnXOff);
            #endregion
        }

        private int m_ID;
        public int ID
        {
            get { return m_ID; }
            set { SetProperty(ref m_ID, value); }
        }

        private string m_TabTitle;
        public string TabTitle
        {
            get { return m_TabTitle; }
            set { SetProperty(ref m_TabTitle, value); }
        }

        private string m_Title;
        public string Title
        {
            get { return m_Title; }
            set { SetProperty(ref m_Title, value); }
        }

        private ObservableCollection<string> m_PortCollection;
        public ObservableCollection<string> PortCollection
        {
            get { return m_PortCollection; }
            set { SetProperty(ref m_PortCollection, value); }
        }

        private ObservableCollection<int> m_BaudRateCollection;
        public ObservableCollection<int> BaudRateCollection
        {
            get { return m_BaudRateCollection; }
            set { SetProperty(ref m_BaudRateCollection, value); }
        }

        private ObservableCollection<Parity> m_ParityBitCollection;
        public ObservableCollection<Parity> ParityBitCollection
        {
            get { return m_ParityBitCollection; }
            set { SetProperty(ref m_ParityBitCollection, value); }
        }

        private ObservableCollection<int> m_DataBitCollection;
        public ObservableCollection<int> DataBitCollection
        {
            get { return m_DataBitCollection; }
            set { SetProperty(ref m_DataBitCollection, value); }
        }

        private ObservableCollection<StopBits> m_StopBitCollection;
        public ObservableCollection<StopBits> StopBitCollection
        {
            get { return m_StopBitCollection; }
            set { SetProperty(ref m_StopBitCollection, value); }
        }

        private ObservableCollection<Handshake> m_HandShakeCollection;
        public ObservableCollection<Handshake> HandShakeCollection
        {
            get { return m_HandShakeCollection; }
            set { SetProperty(ref m_HandShakeCollection, value); }
        }

        private string m_SelectedPort;
        public string SelectedPort
        {
            get { return m_SelectedPort; }
            set { SetProperty(ref m_SelectedPort, value); }
        }

        private int m_SelectedBaudRate;
        public int SelectedBaudRate
        {
            get { return m_SelectedBaudRate; }
            set { SetProperty(ref m_SelectedBaudRate, value); }
        }

        private Parity m_SelectedParityBit;
        public Parity SelectedParityBit
        {
            get { return m_SelectedParityBit; }
            set { SetProperty(ref m_SelectedParityBit, value); }
        }

        private StopBits m_SelectedStopBit;
        public StopBits SelectedStopBit
        {
            get { return m_SelectedStopBit; }
            set { SetProperty(ref m_SelectedStopBit, value); }
        }

        private int m_SelectedDataBit;
        public int SelectedDataBit
        {
            get { return m_SelectedDataBit; }
            set { SetProperty(ref m_SelectedDataBit, value); }
        }

        private Handshake m_SelectedHandShake;
        public Handshake SelectedHandShake
        {
            get { return m_SelectedHandShake; }
            set { SetProperty(ref m_SelectedHandShake, value); }
        }

        public bool IsOnline => IsPortOpen;

        public bool IsOffline => !IsPortOpen;

        private bool m_IsPortOpen;
        public bool IsPortOpen
        {
            get { return m_IsPortOpen; }
            set
            {
                SetProperty(ref m_IsPortOpen, value);
                RaisePropertyChanged(nameof(IsOnline));
                RaisePropertyChanged(nameof(IsOffline));
                RaisePropertyChanged(nameof(StatusIcon));
            }
        }

        public string StatusIcon
        {
            get
            {
                if (IsOnline)
                {
                    return "/GreatechApp.Core;component/Icon/GreenIcon.png";
                }
                return "/GreatechApp.Core;component/Icon/GrayIcon.png";
            }
        }

        private string m_RxMsg;
        public string RxMsg
        {
            get { return m_RxMsg; }
            set { SetProperty(ref m_RxMsg, value); }
        }

        private string m_TxMsg;
        public string TxMsg
        {
            get { return m_TxMsg; }
            set { SetProperty(ref m_TxMsg, value); }
        }
    }
}
