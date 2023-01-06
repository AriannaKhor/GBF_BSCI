using ConfigManager;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using GreatechApp.Services.UserServices;
using GreatechApp.Services.Utilities;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Windows;

namespace SerialPortManager.ViewModels
{
	public class SerialPortViewModel : BindableBase, IAccessService
    {
        #region Variable
        private AuthService m_authService;

        private string m_title;
        public string Title
        {
            get { return m_title; }
            set { SetProperty(ref m_title, value); }
        }

        private List<SerialPortParameter> m_SerialPortList;
        public List<SerialPortParameter> SerialPortList
        {
            get { return m_SerialPortList; }
            set { SetProperty(ref m_SerialPortList, value); }
        }

        public DelegateCommand<SerialPortParameter> OpenCommand { get; private set; }
        public DelegateCommand<SerialPortParameter> CloseCommand { get; private set; }
        public DelegateCommand<SerialPortParameter> SaveCommand { get; private set; }
        public DelegateCommand<SerialPortParameter> SendCommand { get; private set; }
        public DelegateCommand<SerialPortParameter> ClearTxCommand { get; private set; }
        public DelegateCommand<SerialPortParameter> ClearRxCommand { get; private set; }

        IEventAggregator m_EventAggregator;
        ISerialPort m_SerialPort;
        CultureResources m_CultureResources;
        SystemConfig m_SysCfg;
        #endregion

        #region Constructor
        public SerialPortViewModel(AuthService authService, IEventAggregator eventAggregator, ISerialPort SerialPort, SystemConfig sysConfig, CultureResources cultureResources)
        {
            m_SerialPort = SerialPort;
            m_EventAggregator = eventAggregator;
            m_SysCfg = sysConfig;
            m_authService = authService;
            m_CultureResources = cultureResources;

            SerialPortList = new List<SerialPortParameter>();

            for (int i = 0; i < Enum.GetNames(typeof(SerialDev)).Length; i++)
            {
                SerialPortConfig serialPortCfg = SerialPortConfig.Open(m_SysCfg.SerialPortRef[i].Reference);
                SerialPortList.Add(new SerialPortParameter()
                {
                    ID = i,
                    TabTitle = serialPortCfg.Device.Name,
                    SelectedPort = SerialPort.serialPortCollection[i].PortName,
                    SelectedBaudRate = SerialPort.serialPortCollection[i].BaudRate,
                    SelectedParityBit = SerialPort.serialPortCollection[i].Parity,
                    SelectedDataBit = SerialPort.serialPortCollection[i].DataBits,
                    SelectedStopBit = SerialPort.serialPortCollection[i].StopBits,
                    SelectedHandShake = SerialPort.serialPortCollection[i].Handshake,
                    IsPortOpen = SerialPort.serialPortCollection[i].IsOpen,
                    TxMsg = string.Empty,
                    RxMsg = string.Empty,
                });
            }

            Title = m_CultureResources.GetStringValue("SerialPort");


            OpenCommand = new DelegateCommand<SerialPortParameter>(Open);
            CloseCommand = new DelegateCommand<SerialPortParameter>(Close);
            SaveCommand = new DelegateCommand<SerialPortParameter>(Save);
            SendCommand = new DelegateCommand<SerialPortParameter>(Send);
            ClearTxCommand = new DelegateCommand<SerialPortParameter>(ClearTx);
            ClearRxCommand = new DelegateCommand<SerialPortParameter>(ClearRx);


            m_EventAggregator.GetEvent<SerialPortMsg>().Subscribe(SerialMessageReceived);
            m_EventAggregator.GetEvent<SerialPortStatus>().Subscribe(SerialStatusReceived);
            m_EventAggregator.GetEvent<ValidateLogin>().Subscribe(OnValidateLogin);
            m_EventAggregator.GetEvent<CultureChanged>().Subscribe(OnCultureChanged);

            RaisePropertyChanged(nameof(CanAccess));
        }
        #endregion

        #region Events
        private void OnCultureChanged()
        {
            Title = m_CultureResources.GetStringValue("SerialPort");
        }
        private void OnValidateLogin(bool IsAuthenticated)
        {
            RaisePropertyChanged(nameof(CanAccess));
        }

        private void SerialMessageReceived(SerialPortMsg message)
        {
            if (Global.MachineStatus != MachineStateType.Running)
            {
                for (int i = 0; i < SerialPortList.Count; i++)
                {
                    if (message.DevName == EnumHelper.GetValueFromDescription<SerialDev>(SerialPortList[i].TabTitle))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            string space = String.IsNullOrEmpty(SerialPortList[i].RxMsg) ? "" : "\n";
                            SerialPortList[i].RxMsg = SerialPortList[i].RxMsg + space + $"[{DateTime.Now:HH:mm:ss.fff}]: {message.Message.Replace("\r", string.Empty).Replace("\n", string.Empty)}";  
                        });
                    }
                }
            }
        }

        private void SerialStatusReceived(SerialPortStatus message)
        {
            if (Global.MachineStatus != MachineStateType.Running)
            {
                for (int i = 0; i < SerialPortList.Count; i++)
                {
                    if (message.DevName == EnumHelper.GetValueFromDescription<SerialDev>(SerialPortList[i].TabTitle))
                    {
                        SerialPortList[i].IsPortOpen = message.IsPortOpen;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            SerialPortList[i].RxMsg = $"{message.DevName} Connection Status :  {message.IsPortOpen}";
                        });
                    }
                }
            }
        }
        #endregion

        #region Method
        public void Open(SerialPortParameter serialPortTabList)
        {
            try
            {
                if (m_SerialPort.serialPortCollection[serialPortTabList.ID].IsOpen) 
                { 
                    m_SerialPort.serialPortCollection[serialPortTabList.ID].Close(); 
                }

                m_SerialPort.serialPortCollection[serialPortTabList.ID].PortName = serialPortTabList.SelectedPort;
                m_SerialPort.serialPortCollection[serialPortTabList.ID].BaudRate = serialPortTabList.SelectedBaudRate;
                m_SerialPort.serialPortCollection[serialPortTabList.ID].Parity = serialPortTabList.SelectedParityBit;
                m_SerialPort.serialPortCollection[serialPortTabList.ID].StopBits = serialPortTabList.SelectedStopBit;
                m_SerialPort.serialPortCollection[serialPortTabList.ID].DataBits = serialPortTabList.SelectedDataBit;
                m_SerialPort.serialPortCollection[serialPortTabList.ID].Handshake = serialPortTabList.SelectedHandShake;

                m_SerialPort.serialPortCollection[serialPortTabList.ID].Open();

                serialPortTabList.IsPortOpen = m_SerialPort.serialPortCollection[serialPortTabList.ID].IsOpen;

                RaisePropertyChanged(nameof(serialPortTabList.IsPortOpen));
            }
            catch (Exception ex)
            {
                serialPortTabList.RxMsg = ex.Message;
            }
        }

        public void Close(SerialPortParameter serialPortTabList)
        {
            try
            {
                m_SerialPort.serialPortCollection[serialPortTabList.ID].Close();

                serialPortTabList.IsPortOpen = m_SerialPort.serialPortCollection[serialPortTabList.ID].IsOpen;

                RaisePropertyChanged(nameof(serialPortTabList.IsPortOpen));
            }
            catch (Exception ex)
            {
                serialPortTabList.RxMsg = ex.Message;
            }
        }

        public void Send(SerialPortParameter serialPortTabList)
        {
            try
            {
                m_SerialPort.serialPortCollection[serialPortTabList.ID].WriteLine(serialPortTabList.TxMsg);
            }
            catch (Exception ex)
            {
                serialPortTabList.RxMsg = ex.Message;
            }
        }

        public void Save(SerialPortParameter serialPortTabList)
        {
            try
			{
                SerialPortConfig serialCfg = SerialPortConfig.Open(m_SysCfg.SerialPortRef[serialPortTabList.ID].Reference);

                serialCfg.Device.PortName = serialPortTabList.SelectedPort;
                serialCfg.Device.BaudRate = serialPortTabList.SelectedBaudRate;
                serialCfg.Device.Parity = serialPortTabList.SelectedParityBit;
                serialCfg.Device.DataBits = serialPortTabList.SelectedDataBit;
                serialCfg.Device.StopBits = serialPortTabList.SelectedStopBit;
                serialCfg.Device.Handshake = serialPortTabList.SelectedHandShake;

                SerialPortConfig.Save(serialCfg);
            }
            catch(Exception ex)
			{
                serialPortTabList.RxMsg = ex.Message;
            }
        }

        public void ClearTx(SerialPortParameter serialPortTabList)
        {
            serialPortTabList.TxMsg = string.Empty;
        }

        public void ClearRx(SerialPortParameter serialPortTabList)
        {
            serialPortTabList.RxMsg = string.Empty;
        }
        #endregion

        #region Access Implementation
        public IUser CurrentUser { get; }

        public bool CanAccess
        {
            get
            {
                if (m_authService.CheckAccess(ACL.Communication) && m_authService.CurrentUser.IsAuthenticated)
                {
                    return true;
                }
                return false;
            }
        }
        #endregion
    }
}
