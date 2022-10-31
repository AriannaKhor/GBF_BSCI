using ConfigManager;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Services.Utilities;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using TCPIPManager.Enums;
using TCPIPManager.EventArgs;
using TCPIPManager.EventHandler;
using TCPIPManager.Infrastructure;

namespace TCPIPManager
{

	public class TCPIPBase : ITCPIP
    {
        #region Variables
        IEventAggregator m_EventAggregator;
        SystemConfig m_SysCfg;

        TCPIPStatus m_TCPIPStatus;
        TCPIPMsg m_TCPIPMsg;

        private object m_SyncTCPMsg = new object();
        private object m_SyncTCPStatus = new object();
        #endregion

        #region Constructor
        public TCPIPBase(IEventAggregator eventAggregator, SystemConfig systemCfg)
        {
            m_EventAggregator = eventAggregator;
            m_SysCfg = systemCfg;

            m_TCPIPStatus = new TCPIPStatus();
            m_TCPIPMsg = new TCPIPMsg();

            clientSockets = new List<ClientSocket>();

            serverSockets = new List<ServerSocket>();

            foreach (NetworkDevice device in m_SysCfg.NetworkDevices)
            {
                try
                {
                    ClientSocket socket = new ClientSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, 1024, 2048, false);
                    TCPIPDataFormat senderDataFormat = (TCPIPDataFormat)Enum.Parse(typeof(TCPIPDataFormat), device.ReceivedDataFormat);
                    TCPIPDataFormat receiverDataFormat = (TCPIPDataFormat)Enum.Parse(typeof(TCPIPDataFormat), device.ReceivedDataFormat);
                    socket.Connect(device.IPAddress, device.Port, senderDataFormat, receiverDataFormat);
                    clientSockets.Add(socket);

                    clientSockets[device.ID].Name = EnumHelper.GetDescription(((NetworkDev)(int)device.ID));
                    clientSockets[device.ID].ConnectionStateChanged += new ConnectionStateChangedEventHandler(OnClientSocket_StatusChanged);
                    clientSockets[device.ID].MessageReceived += new MessageReceivedEventHandler(OnClientSocket_MessageReceived);
                }
                catch(Exception ex)
                {
                    SupportMethod.ShowExceptionMessage(ex, Output.MessageBox);
                }
            }

            foreach(NetworkDevice device in m_SysCfg.ServerSockets)
            {
                try
                {
                    ServerSocket socket = new ServerSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, 1024, 2048, false);
                    socket.Start(IPAddress.Any, device.Port);
                    serverSockets.Add(socket);
                }
                catch (Exception ex)
                {
                    SupportMethod.ShowExceptionMessage(ex, Output.MessageBox);
                }
            }
        }
        #endregion

        #region Properties
        public List<ClientSocket> clientSockets { get; set; }

        public List<ServerSocket> serverSockets { get; set; }
        #endregion

        #region Events
        private void OnClientSocket_StatusChanged(object sender, ConnectionStateChangedEventArgs c)
        {
            lock (m_SyncTCPStatus)
            {
                ClientSocket socket = sender as ClientSocket;

                m_TCPIPStatus = new TCPIPStatus
                {
                    TCPDevice = EnumHelper.GetValueFromDescription<NetworkDev>(socket.Name),
                    IpAddress = socket.IPAdddress,
                    Port = socket.Port,
                    IsAlive = c.ConnectionStatus
                };

                m_EventAggregator.GetEvent<TCPIPStatus>().Publish(m_TCPIPStatus);

                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.TCP, MsgText = $"{socket.Name} [{socket.IPAdddress}] {(socket.IsAlive ? "CONNECTED" : "DISCONNECTED")}" });
            }
        }

        private void OnClientSocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            lock (m_SyncTCPMsg)
            {
                ClientSocket socket = sender as ClientSocket;

                m_TCPIPMsg = new TCPIPMsg
                {
                    TCPDevice = EnumHelper.GetValueFromDescription<NetworkDev>(socket.Name),
                    IpAddress = socket.IPAdddress,
                    Port = socket.Port,
                    MessageByte = socket.LastMessageByteReceived,
                    Message = socket.ReceivedDataFormat == TCPIPDataFormat.ASCII ? socket.LastMessageReceived.Replace("\r", string.Empty).Replace("\n", string.Empty) : BitConverter.ToString(socket.LastMessageByteReceived.ToArray()).Replace("-", "").Trim('0'),
                };

                m_EventAggregator.GetEvent<TCPIPMsg>().Publish(m_TCPIPMsg);

                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.TCP, MsgText = $"[ RECEIVED ] {socket.Name} : {m_TCPIPMsg.Message}" });
            }
        }
        #endregion
    }
}
