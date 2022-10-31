using ConfigManager;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Interface;
using GreatechApp.Services.Utilities;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace SerialPortManager
{
	public class SerialPortBase : BindableBase, ISerialPort
	{
		IEventAggregator m_EventAggregator;
		SystemConfig m_SysCfg;
		SerialPortMsg SerialPortMsg;
		SerialPortStatus SerialPortStatus;
		CultureResources m_CultureResources;

		private object m_SyncSerialMsg = new object();
		private object m_SyncSerialStatus = new object();

		public SerialPortBase(IEventAggregator eventAggregator, SystemConfig sysConfig, CultureResources cultureResources)
		{
			m_EventAggregator = eventAggregator;
			m_SysCfg = sysConfig;
			m_CultureResources = cultureResources;

			SerialPortMsg = new SerialPortMsg();
			SerialPortStatus = new SerialPortStatus();

			serialPortCollection = new List<SerialPort>();

			for (int i = 0; i < m_SysCfg.SerialPortRef.Count; i++)
			{
				SerialPortConfig serialPortCfg = SerialPortConfig.Open(m_SysCfg.SerialPortRef[i].Reference);

				try
				{
					SerialPort serialPort = new SerialPort(serialPortCfg.Device.PortName, serialPortCfg.Device.BaudRate, serialPortCfg.Device.Parity, serialPortCfg.Device.DataBits, serialPortCfg.Device.StopBits);
					serialPortCollection.Add(serialPort);
					serialPortCollection[i].DataReceived += new SerialDataReceivedEventHandler(OnSerialPort_MessageReceived);
					serialPortCollection[i].PinChanged += new SerialPinChangedEventHandler(OnSerialPort_PinChanged);

					serialPort.Open();
				}
				catch (Exception ex)
				{
					m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity { MsgType = LogMsgType.SerialPort, MsgText = $"{m_CultureResources.GetStringValue("SerialPort")} {serialPortCfg.Device.PortName} {m_CultureResources.GetStringValue("Error")} : {ex.Message}"});
				}
			}
		}

		#region Event
		private void OnSerialPort_MessageReceived(object sender, SerialDataReceivedEventArgs e)
		{
			lock (m_SyncSerialMsg)
			{
				SerialPort serial = sender as SerialPort;

				for (int i = 0; i < m_SysCfg.SerialPortRef.Count; i++)
				{
					SerialPortConfig serialPortCfg = SerialPortConfig.Open(m_SysCfg.SerialPortRef[i].Reference);

					if (serialPortCfg.Device.PortName == serial.PortName)
					{
						SerialPortMsg.DevName = EnumHelper.GetValueFromDescription<SerialDev>(serialPortCfg.Device.Name);
						break;
					}
				}

				SerialPortMsg.Message = serial.ReadLine().Replace("\r", string.Empty).Replace("\n", string.Empty);

				m_EventAggregator.GetEvent<SerialPortMsg>().Publish(SerialPortMsg);

				m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.SerialPort, MsgText = $"[ RECEIVED ] {SerialPortMsg.DevName} - {serial.PortName} : {SerialPortMsg.Message}" });
			}
		}

		private void OnSerialPort_PinChanged(object sender, SerialPinChangedEventArgs e)
		{
			lock (m_SyncSerialStatus)
			{
				SerialPort serial = sender as SerialPort;

				SerialPinChange pinStatus = 0;
				pinStatus = e.EventType;

				switch (pinStatus)
				{
					case SerialPinChange.CDChanged:
						for (int i = 0; i < m_SysCfg.SerialPortRef.Count; i++)
						{
							SerialPortConfig serialPortCfg = SerialPortConfig.Open(m_SysCfg.SerialPortRef[i].Reference);

							if (serialPortCfg.Device.PortName == serial.PortName)
							{
								SerialPortStatus.DevName = EnumHelper.GetValueFromDescription<SerialDev>(serialPortCfg.Device.Name);
								break;
							}
						}

#if SIMULATION
						SerialPortStatus.IsPortOpen = serial.IsOpen;
#else
						SerialPortStatus.IsPortOpen = serial.IsOpen && serial.CDHolding;
#endif
						m_EventAggregator.GetEvent<SerialPortStatus>().Publish(SerialPortStatus);

						m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.SerialPort, MsgText = $"{serial.PortName} {(SerialPortStatus.IsPortOpen ? "CONNECTED" : "DISCONNECTED")}" });
						break;
				}
			}
		}
		#endregion

		#region Properties
		public List<SerialPort> serialPortCollection { get; set; }
		#endregion
	}
}
