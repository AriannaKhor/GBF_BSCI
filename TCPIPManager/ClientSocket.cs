using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using TCPIPManager.Enums;
using TCPIPManager.EventArgs;
using TCPIPManager.Infrastructure;

namespace TCPIPManager
{
	public class ClientSocket : BaseSocket
    {
        #region Fields.
        private bool m_IsAlive;
        private ASCIIEncoding m_Encoder;
        private Timer m_Timer;
        internal IEventAggregator m_EventAggregator;
        #endregion

        #region Properties.
        #region Description.
        /// <summary>
        /// Gets the instance of the System.Net.Sockets object that is currently used by ClientSocket. It can only be set internally.
        /// </summary>
        #endregion
        public Socket Client
        {
            get;
            private set;
        }
        public string Name
        {
            get;
            set;
        }
        #region Description.
        /// <summary>
        /// Gets the lastest string which has been sent by ClientSocket. It can only be set internally.
        /// </summary>
        #endregion
        public string LastMessageSent
        {
            get;
            private set;
        }
        #region Description.
        /// <summary>
        /// Gets the lastest byte values which has been received by ClientSocket. It can only be set internally.
        /// </summary>
        #endregion
        public byte[] LastMessageByteReceived
        {
            get;
            private set;
        }
        #region Description.
        /// <summary>
        /// Gets the lastest string which has been received by ClientSocket. It can only be set internally.
        /// </summary>
        #endregion
        public string LastMessageReceived
        {
            get;
            private set;
        }
        #region Description.
        /// <summary>
        /// Gets a value indicating whether the ClientSocket is still connected. It can only be set internally.
        /// </summary>
        #endregion
        public bool IsAlive
        {
            get
            {
                return m_IsAlive;
            }
            private set
            {
                if (m_IsAlive != value && m_ConnectionStateChanged != null)
                {
                    m_IsAlive = value;
                    ConnectionStateChangedEventArgs args = new ConnectionStateChangedEventArgs();
                    args.ConnectionStatus = value;

                    #region Fire the ConnectionStateChanged event (bound during instantiation of ClientSocket).
                    // Check the Target of each delegate in the event's invocation list, and marshal the call
                    // to the target thread if that target is ISynchronizeInvoke
                    // ref: http://stackoverflow.com/questions/1698889/raise-events-in-net-on-the-main-ui-thread

                    foreach (Delegate del in m_ConnectionStateChanged.GetInvocationList())
                    {
                        ISynchronizeInvoke syncer = del.Target as ISynchronizeInvoke;
                        if (syncer == null)
                        {
                            del.DynamicInvoke(this, args);
                        }
                        else
                        {
                            syncer.BeginInvoke(del, new object[] { this, args });
                        }
                    }
                    #endregion
                }
            }
        }

        public string IPAdddress
        {
            get;
            private set;
        }

        public int Port
        {
            get;
            private set;
        }

        public TCPIPDataFormat SendDataFormat
        {
            get;
            set;
        }

        public TCPIPDataFormat ReceivedDataFormat
        {
            get;
            set;
        }

        #endregion

        #region Delegates.
        #endregion

        #region Events.
        #endregion

        #region Public methods.
        #region Description.
        /// <summary>
        /// Connect the ClientSocket to the server at specified IP address and port number.
        /// </summary>
        /// <param name="ipAddress">IP Address to connect to, e.g. "127.0.0.1".</param>
        /// <param name="port">Port number to connect to.</param>
        #endregion
        public void Connect(string ipAddress, int port, TCPIPDataFormat senderDataFormat, TCPIPDataFormat receiverDataFormat)
        {
            lock(this)
            {
                try
                {
                    // Cache
                    IPAdddress = ipAddress;
                    Port = port;
                    SendDataFormat = senderDataFormat;
                    ReceivedDataFormat = receiverDataFormat;
                    // Parse the given IP into an IPAdress object.
                    IPAddress ip = IPAddress.Parse(ipAddress);
                    // Instantiate an IPEndPoint using the parsed IP and given port.
                    IPEndPoint endPoint = new IPEndPoint(ip, port);

                    Client.BeginConnect(endPoint, new AsyncCallback(OnClientConnected), null);
                }
                catch (Exception ex)
                {
                    ExceptionThrownEventArgs args = new ExceptionThrownEventArgs();
                    args.Exception = ex;

                    // The IF statement is used to ensure that listener exist for 
                    // ExceptionThrown Event.
                    if (m_ExceptionThrown != null)
                    {
                        #region Fire the ExceptionThrown event (bound during instantiation of ClientSocket).
                        // Check the Target of each delegate in the event's invocation list, and marshal the call
                        // to the target thread if that target is ISynchronizeInvoke
                        // ref: http://stackoverflow.com/questions/1698889/raise-events-in-net-on-the-main-ui-thread

                        foreach (Delegate del in m_ExceptionThrown.GetInvocationList())
                        {
                            ISynchronizeInvoke syncer = del.Target as ISynchronizeInvoke;
                            if (syncer == null)
                            {
                                del.DynamicInvoke(this, args);
                            }
                            else
                            {
                                syncer.BeginInvoke(del, new object[] { this, args });
                            }
                        }
                        #endregion
                    }

                    SupportMethod.ShowExceptionMessage(ex, Output.EventLog);
                }
            }
        }

        public void Reconnect()
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(IPAdddress), "IPAdddress cannot be empty!");
            System.Diagnostics.Debug.Assert(Port > 0, "Part cannot be 0!");
            // Connect using cache info
            Connect(IPAdddress, Port, SendDataFormat, ReceivedDataFormat);
        }

        #region Description.
        /// <summary>
        /// Disconnect the ClientSocket from the connected server.
        /// </summary>
        #endregion
        public void Disconnect()
        {
            try
            {
                // Close the ClientSocket (Client). Upon closing the ClientSocket using Socket.Close(),
                // the ClientSocket will be disposed and property Socket.Connected will set to FALSE.
                //
                // Closing either one of two CONNECTED socket (using Socket.Close()) will cause both socket
                // to receive null value (0) in their own receive-buffer. Note that this will not happen when
                // we close an UNCONNECTED socket.
                Client.Close();

                // Recreate a new instance of the Socket object.
                Client = new Socket(Client.AddressFamily, Client.SocketType, Client.ProtocolType);
            }
            catch (Exception ex)
            {
                ExceptionThrownEventArgs args = new ExceptionThrownEventArgs();
                args.Exception = ex;

                // The IF statement is used to ensure that listener exist for 
                // ExceptionThrown Event.
                if (m_ExceptionThrown != null)
                {
                    #region Fire the ExceptionThrown event (bound during instantiation of ClientSocket).
                    // Check the Target of each delegate in the event's invocation list, and marshal the call
                    // to the target thread if that target is ISynchronizeInvoke
                    // ref: http://stackoverflow.com/questions/1698889/raise-events-in-net-on-the-main-ui-thread

                    foreach (Delegate del in m_ExceptionThrown.GetInvocationList())
                    {
                        ISynchronizeInvoke syncer = del.Target as ISynchronizeInvoke;
                        if (syncer == null)
                        {
                            del.DynamicInvoke(this, args);
                        }
                        else
                        {
                            syncer.BeginInvoke(del, new object[] { this, args });
                        }
                    }
                    #endregion
                }

                SupportMethod.ShowExceptionMessage(ex, Output.EventLog);
            }
        }
        #region Description.
        /// <summary>
        /// Send message to the connected server.
        /// </summary>
        /// <param name="message">Message that will be sent.</param>
        #endregion
        public bool Send(string message)
        {
            try
            {
                // Provide additional data for the socket operation.
                Metadata metadata = new Metadata();
                metadata.MessageType = MessageType.Message;
                if(SendDataFormat == TCPIPDataFormat.ASCII)
                {
                    metadata.SendBuf = m_Encoder.GetBytes(message);
                }
                else if(SendDataFormat == TCPIPDataFormat.HEX)
                {
                    List<byte> byteList = new List<byte>();
                    message = message.Replace(" ", string.Empty);
                    for (int i = 0; i < message.Length; i += 2)
                    {
                        var hexString = message.Substring(i, 2);
                        var decimalValue = (decimal)Int64.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
                        byteList.Add((byte)decimalValue);
                    }
                    metadata.SendBuf = byteList.ToArray();
                }

                Client.BeginSend(
                    metadata.SendBuf,
                    0,
                    metadata.SendBuf.Length,
                    SocketFlags.None,
                    new AsyncCallback(OnMessageSent),
                    metadata);

                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.TCP, MsgText = $"[ SEND ] {Name} : {message}" });
                return true;
            }
            catch (Exception ex)
            {
                ExceptionThrownEventArgs args = new ExceptionThrownEventArgs();
                args.Exception = ex;

                // The IF statement is used to ensure that listener exist for 
                // ExceptionThrown Event.
                if (m_ExceptionThrown != null)
                {
                    #region Fire the ExceptionThrown event (bound during instantiation of ClientSocket).
                    // Check the Target of each delegate in the event's invocation list, and marshal the call
                    // to the target thread if that target is ISynchronizeInvoke
                    // ref: http://stackoverflow.com/questions/1698889/raise-events-in-net-on-the-main-ui-thread

                    foreach (Delegate del in m_ExceptionThrown.GetInvocationList())
                    {
                        ISynchronizeInvoke syncer = del.Target as ISynchronizeInvoke;
                        if (syncer == null)
                        {
                            del.DynamicInvoke(this, args);
                        }
                        else
                        {
                            syncer.BeginInvoke(del, new object[] { this, args });
                        }
                    }
                    #endregion
                }

                SupportMethod.ShowExceptionMessage(ex, Output.EventLog);
                return false;
            }
        }
        #endregion

        #region Private methods.
        private void InitData()
        {
            // Fields
            m_EchoOnReceived = false;
            m_WaitEchoOnSent = false;
            m_SendBufSize = 0;
            m_ReceiveBufSize = 0;
            m_IsAlive = false;
            m_Encoder = new ASCIIEncoding();

            // Properties
            Client = null;
            LastMessageSent = string.Empty;
            LastMessageReceived = string.Empty;

            m_Timer = new Timer();
            m_Timer.Elapsed += new ElapsedEventHandler(Poll);
            m_Timer.Interval = 100;

            // Start polling the connection status of ClientSocket.
            m_Timer.Start();
        }
        private void Poll(object sender, ElapsedEventArgs e)
        {
            // ref: http://stackoverflow.com/questions/11737056/timer-interval-is-smaller-than-function-in-tick-event
            //
            // Temporally disable the timer to ensure the following block of codes 
            // executed completely before the next Elapsed event is triggered.
            m_Timer.Stop();

            try
            {
                // ref: http://msdn.microsoft.com/en-us/library/system.net.sockets.socket.poll%28v=vs.100%29.aspx
                //
                // Poll the status of the socket using the SelectWrite mode, where it will return TRUE if
                // 01.  processing a Connect, and the connection has succeeded, OR
                // 02.  data can be sent
                //
                // Otherwise, returns FALSE.
                if (Client.Connected && Client.Poll(m_PollResTime, SelectMode.SelectWrite))
                {
                    // Set the m_IsAlive value thru properties IsAlive. If the m_IsAlive has changed 
                    // to different value, ConnectionStateChanged event will be fired.
                    IsAlive = true;
                }
                else
                {
                    #region NOTES.
                    // If the socket has been closed by your own actions (disposing the socket,
                    // calling methods to disconnect), Socket.Connected will return FALSE.
                    #endregion

                    // Set the m_IsAlive value thru properties IsAlive. If the m_IsAlive has changed 
                    // to different value, ConnectionStateChanged event will be fired.
                    IsAlive = false;
                }
            }
            catch (ObjectDisposedException)
            {
                // Upon closing the Client at ClientSocket.Disconnect(), Client will be disposed.
                // There exist a very small time gap where the Client is NULL before a new socket
                // instance is instantiated. On the same time, when Socket.Poll() is called by
                // ClientSocket.Poll() (timer-driven method), this can cause ObjectDisposedException 
                // to be thrown. Here we catch this exception.

                // Set the m_IsAlive value thru properties IsAlive. If the m_IsAlive has changed 
                // to different value, ConnectionStateChanged event will be fired.
                IsAlive = false;
            }
            catch (Exception ex)
            {
                ExceptionThrownEventArgs args = new ExceptionThrownEventArgs();
                args.Exception = ex;

                // The IF statement is used to ensure that listener exist for 
                // ExceptionThrown Event.
                if (m_ExceptionThrown != null)
                {
                    #region Fire the ExceptionThrown event (bound during instantiation of ClientSocket).
                    // Check the Target of each delegate in the event's invocation list, and marshal the call
                    // to the target thread if that target is ISynchronizeInvoke
                    // ref: http://stackoverflow.com/questions/1698889/raise-events-in-net-on-the-main-ui-thread

                    foreach (Delegate del in m_ExceptionThrown.GetInvocationList())
                    {
                        ISynchronizeInvoke syncer = del.Target as ISynchronizeInvoke;
                        if (syncer == null)
                        {
                            del.DynamicInvoke(this, args);
                        }
                        else
                        {
                            syncer.BeginInvoke(del, new object[] { this, args });
                        }
                    }
                    #endregion
                }

                SupportMethod.ShowExceptionMessage(ex, Output.EventLog);
            }

            // Re-enable the timer.
            m_Timer.Start();
        }
        private void Echo(string echo)
        {
            try
            {
                // Provide additional data for the socket operation.
                Metadata metadata = new Metadata();
                metadata.MessageType = MessageType.Echo;
                metadata.SendBuf = m_Encoder.GetBytes(echo);

                Client.BeginSend(
                    metadata.SendBuf,
                    0,
                    metadata.SendBuf.Length,
                    SocketFlags.None,
                    new AsyncCallback(OnMessageSent),
                    metadata);
            }
            catch (Exception ex)
            {
                ExceptionThrownEventArgs args = new ExceptionThrownEventArgs();
                args.Exception = ex;

                // The IF statement is used to ensure that listener exist for 
                // ExceptionThrown Event.
                if (m_ExceptionThrown != null)
                {
                    #region Fire the ExceptionThrown event (bound during instantiation of ClientSocket).
                    // Check the Target of each delegate in the event's invocation list, and marshal the call
                    // to the target thread if that target is ISynchronizeInvoke
                    // ref: http://stackoverflow.com/questions/1698889/raise-events-in-net-on-the-main-ui-thread

                    foreach (Delegate del in m_ExceptionThrown.GetInvocationList())
                    {
                        ISynchronizeInvoke syncer = del.Target as ISynchronizeInvoke;
                        if (syncer == null)
                        {
                            del.DynamicInvoke(this, args);
                        }
                        else
                        {
                            syncer.BeginInvoke(del, new object[] { this, args });
                        }
                    }
                    #endregion
                }

                SupportMethod.ShowExceptionMessage(ex, Output.EventLog);
            }
        }
        #endregion

        #region Callback for socket operation.
        private void OnClientConnected(IAsyncResult result)
        {
            try
            {
                Client.EndConnect(result);

                // Provide additional data for the socket operation.
                Metadata metadata = new Metadata();
                metadata.ReceiveBuf = new byte[m_ReceiveBufSize];
                // Prepare to receive data from the connected Server.
                Client.BeginReceive(
                    metadata.ReceiveBuf,
                    0,
                    metadata.ReceiveBuf.Length,
                    SocketFlags.None,
                    new AsyncCallback(OnMessageReceived),
                    metadata);
            }
            catch (Exception ex)
            {
                ExceptionThrownEventArgs args = new ExceptionThrownEventArgs();
                args.Exception = ex;

                // The IF statement is used to ensure that listener exist for 
                // ExceptionThrown Event.
                if (m_ExceptionThrown != null)
                {
                    #region Fire the ExceptionThrown event (bound during instantiation of ClientSocket).
                    // Check the Target of each delegate in the event's invocation list, and marshal the call
                    // to the target thread if that target is ISynchronizeInvoke
                    // ref: http://stackoverflow.com/questions/1698889/raise-events-in-net-on-the-main-ui-thread

                    foreach (Delegate del in m_ExceptionThrown.GetInvocationList())
                    {
                        ISynchronizeInvoke syncer = del.Target as ISynchronizeInvoke;
                        if (syncer == null)
                        {
                            del.DynamicInvoke(this, args);
                        }
                        else
                        {
                            syncer.BeginInvoke(del, new object[] { this, args });
                        }
                    }
                    #endregion
                }

                SupportMethod.ShowExceptionMessage(ex, Output.EventLog);
            }
        }
        private void OnMessageReceived(IAsyncResult result)
        {
            try
            {
                // IAsyncResult.AsyncState contain the Object which passed as last parameter in 
                // Socket.BeginReceive().
                Metadata metadata = (Metadata)result.AsyncState;

                if (metadata.ReceiveBuf[0] == 0)
                {
                    // Closing either one of two CONNECTED socket (using Socket.Close()) will cause both socket
                    // to receive null value (0) in their own receive-buffer. Note that this will not happen when
                    // we close an UNCONNECTED socket.
                    //
                    // When the ServerSocket close its WorkerSocket corresponding to this ClientSocket (Client),
                    // both WorkerSocket and ClientSocket will received null value (0) in its receive-buffer 
                    // (i.e. m_ReceiveBuf in this case).
                    //
                    // The same happens when ClientSocket close the socket, both ClientSocket and its coressponding
                    // WorkerSocket will receive null value (0) in its receive-buffer as well.
                    //
                    // We use this as an indicator that the WorkerSocket has stopped/closed at server side. 
                    // At client side, we need to close the corresponding ClientSocket as well.
                    Disconnect();
                }
                else
                {
                    Client.EndReceive(result);

                    LastMessageByteReceived = metadata.ReceiveBuf;
                    LastMessageReceived = m_Encoder.GetString(metadata.ReceiveBuf).Trim('\0');

                    if (m_EchoOnReceived)
                    {
                        // Echo to the server upon receiving message from server.
                        Echo(LastMessageReceived);
                    }

                    // The IF statement is used to ensure that listener exist for 
                    // MessageReceived Event.
                    MessageReceivedEventArgs args = new MessageReceivedEventArgs();
                    if (m_MessageReceived != null)
                    {
                        #region Fire the MessageReceived event (bound during instantiation of ClientSocket).
                        // Check the Target of each delegate in the event's invocation list, and marshal the call
                        // to the target thread if that target is ISynchronizeInvoke
                        // ref: http://stackoverflow.com/questions/1698889/raise-events-in-net-on-the-main-ui-thread

                        foreach (Delegate del in m_MessageReceived.GetInvocationList())
                        {
                            ISynchronizeInvoke syncer = del.Target as ISynchronizeInvoke;
                            if (syncer == null)
                            {
                                del.DynamicInvoke(this, args);
                            }
                            else
                            {
                                syncer.BeginInvoke(del, new object[] { this, args });
                            }
                        }
                        #endregion
                    }

                    // Provide additional data for the socket operation. Re-instantiate a new Metadata object.
                    metadata = new Metadata();
                    metadata.ReceiveBuf = new byte[m_ReceiveBufSize];

                    // Continue to prepare receive data from the connected Server.
                    Client.BeginReceive(
                        metadata.ReceiveBuf,
                        0,
                        metadata.ReceiveBuf.Length,
                        SocketFlags.None,
                        new AsyncCallback(OnMessageReceived),
                        metadata);
                }
            }
            catch (Exception ex)
            {
                ExceptionThrownEventArgs args = new ExceptionThrownEventArgs();
                args.Exception = ex;

                // The IF statement is used to ensure that listener exist for 
                // ExceptionThrown Event.
                if (m_ExceptionThrown != null)
                {
                    #region Fire the ExceptionThrown event (bound during instantiation of ClientSocket).
                    // Check the Target of each delegate in the event's invocation list, and marshal the call
                    // to the target thread if that target is ISynchronizeInvoke
                    // ref: http://stackoverflow.com/questions/1698889/raise-events-in-net-on-the-main-ui-thread

                    foreach (Delegate del in m_ExceptionThrown.GetInvocationList())
                    {
                        ISynchronizeInvoke syncer = del.Target as ISynchronizeInvoke;
                        if (syncer == null)
                        {
                            del.DynamicInvoke(this, args);
                        }
                        else
                        {
                            syncer.BeginInvoke(del, new object[] { this, args });
                        }
                    }
                    #endregion
                }

                SupportMethod.ShowExceptionMessage(ex, Output.EventLog);
            }
        }
        private void OnMessageSent(IAsyncResult result)
        {
            try
            {
                Client.EndSend(result);
                // IAsyncResult.AsyncState contain the Object which passed as last parameter in 
                // Socket.BeginSend().
                Metadata metadata = (Metadata)result.AsyncState;

                LastMessageSent = m_Encoder.GetString(metadata.SendBuf);

                // Fire the event (bound during instantiation of ClientSocket). 
                // The IF statement is used to ensure that listener exist for MessageSent Event.
                MessageSentEventArgs args = new MessageSentEventArgs();
                args.MessageType = metadata.MessageType;
                if (m_MessageSent != null)
                {
                    #region Fire the MessageSent event (bound during instantiation of ClientSocket).
                    // Check the Target of each delegate in the event's invocation list, and marshal the call
                    // to the target thread if that target is ISynchronizeInvoke
                    // ref: http://stackoverflow.com/questions/1698889/raise-events-in-net-on-the-main-ui-thread

                    foreach (Delegate del in m_MessageSent.GetInvocationList())
                    {
                        ISynchronizeInvoke syncer = del.Target as ISynchronizeInvoke;
                        if (syncer == null)
                        {
                            del.DynamicInvoke(this, args);
                        }
                        else
                        {
                            syncer.BeginInvoke(del, new object[] { this, args });
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                ExceptionThrownEventArgs args = new ExceptionThrownEventArgs();
                args.Exception = ex;

                // The IF statement is used to ensure that listener exist for 
                // ExceptionThrown Event.
                if (m_ExceptionThrown != null)
                {
                    #region Fire the ExceptionThrown event (bound during instantiation of ClientSocket).
                    // Check the Target of each delegate in the event's invocation list, and marshal the call
                    // to the target thread if that target is ISynchronizeInvoke
                    // ref: http://stackoverflow.com/questions/1698889/raise-events-in-net-on-the-main-ui-thread

                    foreach (Delegate del in m_ExceptionThrown.GetInvocationList())
                    {
                        ISynchronizeInvoke syncer = del.Target as ISynchronizeInvoke;
                        if (syncer == null)
                        {
                            del.DynamicInvoke(this, args);
                        }
                        else
                        {
                            syncer.BeginInvoke(del, new object[] { this, args });
                        }
                    }
                    #endregion
                }
                SupportMethod.ShowExceptionMessage(ex, Output.EventLog);
            }
        }
        #endregion

        #region Overload of constructors.
        private ClientSocket()
        {
            m_EventAggregator = ContainerLocator.Container.Resolve<IEventAggregator>();
            InitData();
        }
        private ClientSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
            : this()
        {
            Client = new Socket(addressFamily, socketType, protocolType);
        }
        #region Description.
        /// <summary>
        /// Initializes a new instance of the SocketManager.ClientSocket class using the specified parameters.
        /// </summary>
        /// <param name="addressFamily">One of the System.Net.Sockets.AddressFamily values.</param>
        /// <param name="socketType">One of the System.Net.Sockets.SocketType values.</param>
        /// <param name="protocolType">One of the System.Net.Sockets.ProtocolType values.</param>
        /// <param name="sendBufSize">Size of the array of type System.Byte that contains the data to send.</param>
        /// <param name="receiveBufSize">Size of the array of type System.Byte that is the storage location of the received data.</param>
        /// <param name="echoOnReceived">TRUE if the ClientSocket is required to send back last received message from server, else FALSE.</param>
        /// <param name="waitEchoOnSent">TRUE if the ClientSocket is required to wait echo message from server, else FALSE.</param>
        #endregion
        public ClientSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, int sendBufSize, int receiveBufSize, bool echoOnReceived, bool waitEchoOnSent = false)
            : this(addressFamily, socketType, protocolType)
        {
            m_SendBufSize = sendBufSize;
            m_ReceiveBufSize = receiveBufSize;
            m_EchoOnReceived = echoOnReceived;
            m_WaitEchoOnSent = waitEchoOnSent;
        }
        #region Description.
        /// <summary>
        /// Initializes a new instance of the SocketManager.ClientSocket class using the specified parameters.
        /// </summary>
        /// <param name="addressFamily">One of the System.Net.Sockets.AddressFamily values.</param>
        /// <param name="socketType">One of the System.Net.Sockets.SocketType values.</param>
        /// <param name="protocolType">One of the System.Net.Sockets.ProtocolType values.</param>
        /// <param name="sendBufSize">Size of the array of type System.Byte that contains the data to send.</param>
        /// <param name="receiveBufSize">Size of the array of type System.Byte that is the storage location of the received data.</param>
        /// <param name="pollResTime">Response time (us) when polling for the state of the socket.</param>
        /// <param name="echoOnReceived">TRUE if the ClientSocket is required to send back last received message from server, else FALSE.</param>
        /// <param name="waitEchoOnSent">TRUE if the ClientSocket is required to wait echo message from server, else FALSE.</param>
        #endregion
        public ClientSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, int sendBufSize, int receiveBufSize, int pollResTime, bool echoOnReceived, bool waitEchoOnSent = false)
            : this(addressFamily, socketType, protocolType, sendBufSize, receiveBufSize, echoOnReceived, waitEchoOnSent)
        {
            m_PollResTime = pollResTime;
        }
        #endregion
    }
}
