using System;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using TCPIPManager.Enums;
using TCPIPManager.EventArgs;
using TCPIPManager.Infrastructure;

namespace TCPIPManager
{
    public class WorkerSocket : BaseSocket
    {
        #region Fields.
        private bool m_IsAlive;
        private ASCIIEncoding m_Encoder;
        private Timer m_Timer;
        #endregion

        #region Properties.
        #region Description.
        /// <summary>
        /// Gets the instance of the System.Net.Sockets object that is currently used by WorkerSocket. It can only be set internally.
        /// </summary>
        #endregion
        public Socket Worker
        {
            get;
            private set;
        }
        #region Description.
        /// <summary>
        /// Gets or sets the name of the WorkerSocket.
        /// </summary>
        #endregion
        public string Name
        {
            get;
            set;
        }
        #region Description.
        /// <summary>
        /// Gets the lastest string which has been sent by WorkerSocket. It can only be set internally.
        /// </summary>
        #endregion
        public string LastMessageSent
        {
            get;
            private set;
        }
        #region Description.
        /// <summary>
        /// Gets the lastest string which has been received by WorkerSocket. It can only be set internally.
        /// </summary>
        #endregion
        public string LastMessageReceived
        {
            get;
            private set;
        }
        #region Description.
        /// <summary>
        /// Gets a value indicating whether the WorkerSocket is still connected. It can only be set internally.
        /// </summary>
        #endregion
        internal bool IsAlive
        {
            get
            {
                return m_IsAlive;
            }
            private set
            {
                if (m_IsAlive != value && m_ConnectionStateChanged != null)
                {
                    ConnectionStateChangedEventArgs args = new ConnectionStateChangedEventArgs();
                    args.ConnectionStatus = value;

                    // Fire the event (bound during instantiation of WorkerSocket).
                    m_ConnectionStateChanged.Invoke(this, args);
                }

                m_IsAlive = value;
            }
        }
        #endregion

        #region Delegates.
        #endregion

        #region Events.
        #endregion

        #region Internal methods.
        internal void Disconnect()
        {
            try
            {
                // Close the WorkerSocket (Worker). Upon closing the WorkerSocket using Socket.Close(),
                // the WorkerSocket will be disposed and property Socket.Connected will set to FALSE.
                //
                // Closing either one of two CONNECTED socket (using Socket.Close()) will cause both socket
                // to receive null value (0) in their own receive-buffer. Note that this will not happen when
                // we close an UNCONNECTED socket.
                Worker.Close();
            }
            catch (Exception ex)
            {
                ExceptionThrownEventArgs args = new ExceptionThrownEventArgs();
                args.Exception = ex;

                // Fire the ExceptionThrown event (bound during instantiation of WorkerSocket).
                // The IF statement is used to ensure that listener exist for ExceptionThrown Event.
                if (m_ExceptionThrown != null)
                {
                    m_ExceptionThrown.Invoke(this, args);
                }

                SupportMethod.ShowExceptionMessage(ex, Output.EventLog);
            }
        }
        internal bool Send(string message)
        {
            try
            {
                // Provide additional data for the socket operation.
                Metadata metadata = new Metadata();
                metadata.MessageType = MessageType.Message;
                metadata.SendBuf = m_Encoder.GetBytes(message);

                Worker.BeginSend(
                    metadata.SendBuf,
                    0,
                    metadata.SendBuf.Length,
                    SocketFlags.None,
                    new AsyncCallback(OnMessageSent),
                    metadata);

                return true;
            }
            catch (Exception ex)
            {
                ExceptionThrownEventArgs args = new ExceptionThrownEventArgs();
                args.Exception = ex;

                // Fire the ExceptionThrown event (bound during instantiation of WorkerSocket).
                // The IF statement is used to ensure that listener exist for ExceptionThrown Event.
                if (m_ExceptionThrown != null)
                {
                    m_ExceptionThrown.Invoke(this, args);
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
            m_SendBufSize = 0;
            m_ReceiveBufSize = 0;
            m_IsAlive = false;
            m_Encoder = new ASCIIEncoding();

            // Properties
            Worker = null;
            LastMessageSent = string.Empty;
            LastMessageReceived = string.Empty;
            Name = string.Empty;

            m_Timer = new Timer();
            m_Timer.Elapsed += new ElapsedEventHandler(Poll);
            m_Timer.Interval = 100;

            // Start polling the connection status of WorkerSocket.
            m_Timer.Start();
        }
        private void InitWorker()
        {
            try
            {
                // Provide additional data for the socket operation.
                Metadata metadata = new Metadata();
                metadata.ReceiveBuf = new byte[m_ReceiveBufSize];

                // Prepare to receive data from the connected client.
                Worker.BeginReceive(
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

                // Fire the ExceptionThrown event (bound during instantiation of WorkerSocket).
                // The IF statement is used to ensure that listener exist for ExceptionThrown Event.
                if (m_ExceptionThrown != null)
                {
                    m_ExceptionThrown.Invoke(this, args);
                }

                SupportMethod.ShowExceptionMessage(ex, Output.EventLog);
            }
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
                if (Worker.Connected && Worker.Poll(m_PollResTime, SelectMode.SelectWrite))
                {
                    // Set the m_IsAlive value thru properties IsAlive. If the m_IsAlive has changed 
                    // to different value, ConnectionStateChanged event will be fired.
                    IsAlive = true;

                    // Re-enable the timer IF AND ONLY IF the socket is still connected.
                    m_Timer.Start();
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
            catch (ObjectDisposedException ex)
            {
                // Upon closing the Worker at WorkerSocket.Disconnect(), Worker will be disposed. 
                // When Socket.Poll() is called by WorkerSocket.Poll() (timer-driven method), this 
                // can cause ObjectDisposedException to be thrown. Here we catch this exception.
                // Also, note that we WILL NOT fire ServerSocket.ExceptionThrown event since this 
                // is an expected exception.

                // Set the m_IsAlive value thru properties IsAlive. If the m_IsAlive has changed 
                // to different value, ConnectionStateChanged event will be fired.
                IsAlive = false;

                // Instead of let the exception go off silently, output some information to the 
                // IDE's output window.
                SupportMethod.ShowExceptionMessage(ex, Output.EventLog);
            }
            catch (Exception ex)
            {
                ExceptionThrownEventArgs args = new ExceptionThrownEventArgs();
                args.Exception = ex;

                // Fire the ExceptionThrown event (bound during instantiation of WorkerSocket).
                // The IF statement is used to ensure that listener exist for ExceptionThrown Event.
                if (m_ExceptionThrown != null)
                {
                    m_ExceptionThrown.Invoke(this, args);
                }

                SupportMethod.ShowExceptionMessage(ex, Output.EventLog);
            }
        }
        private void Echo(string echo)
        {
            try
            {
                // Provide additional data for the socket operation.
                Metadata metadata = new Metadata();
                metadata.MessageType = MessageType.Echo;
                metadata.SendBuf = m_Encoder.GetBytes(echo);

                Worker.BeginSend(
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

                // Fire the ExceptionThrown event (bound during instantiation of WorkerSocket).
                // The IF statement is used to ensure that listener exist for ExceptionThrown Event.
                if (m_ExceptionThrown != null)
                {
                    m_ExceptionThrown.Invoke(this, args);
                }

                SupportMethod.ShowExceptionMessage(ex, Output.EventLog);
            }
        }
        private void Ping()
        {
            try
            {
                // Send an empty string to connected client to verify the connection.
                // If the ping failed, Socket.Connected will be set to FALSE, which
                // will be detected by the timer-driven method Poll().

                // Provide additional data for the socket operation.
                Metadata metadata = new Metadata();
                metadata.MessageType = MessageType.Ping;
                metadata.SendBuf = m_Encoder.GetBytes(string.Empty);

                Worker.BeginSend(
                    metadata.SendBuf,
                    0,
                    metadata.SendBuf.Length,
                    SocketFlags.None,
                    new AsyncCallback(OnMessageSent),
                    metadata);
            }
            catch (Exception ex)
            {
                SupportMethod.ShowExceptionMessage(ex, Output.EventLog);
            }
        }
        #endregion

        #region Callback for socket operation.
        private void OnMessageReceived(IAsyncResult result)
        {
            try
            {
                // IAsyncResult.AsyncState contain the Object which passed as last parameter in 
                // Socket.BeginReceive().
                Metadata metadata = (Metadata)result.AsyncState;

                if (metadata.ReceiveBuf[0] == 0)
                {
                    // Close the WorkerSocket (Worker). Upon closing the WorkerSocket using Socket.Close(),
                    // the WorkerSocket will be disposed and property Socket.Connected will set to FALSE.
                    //
                    // Closing either one of two CONNECTED socket (using Socket.Close()) will cause both socket
                    // to receive null value (0) in their own receive-buffer. Note that this will not happen when
                    // we close an UNCONNECTED socket.
                    //
                    // When the ServerSocket close this WorkerSocket (using ServerSocket.Stop()), both
                    // WorkerSocket and its corresponding ClientSocket at client side will received null value (0)
                    // in its receive-buffer (i.e. ReceiveBuf in this case).
                    //
                    // The same happens when ClientSocket close the socket, both ClientSocket and its coressponding
                    // WorkerSocket will receive null value (0) in its receive-buffer as well.
                    //
                    // We use this as an indicator that the ClientSocket has stopped/closed at client side. 
                    // At server side, we need to close the corresponding WorkerSocket as well.
                    Disconnect();
                }
                else
                {
                    Worker.EndReceive(result);

                    LastMessageReceived = m_Encoder.GetString(metadata.ReceiveBuf).Trim('\0');

                    // Fire the event (bound during instantiation of WorkerSocket).
                    // The IF statement is used to ensure that listener exist for 
                    // MessageReceived Event.
                    MessageReceivedEventArgs args = new MessageReceivedEventArgs();
                    if (m_MessageReceived != null)
                    {
                        m_MessageReceived.Invoke(this, args);
                    }

                    if (m_EchoOnReceived)
                    {
                        // Echo to the server upon receiving message from client.
                        Echo(LastMessageReceived);
                    }

                    // Provide additional data for the socket operation. Re-instantiate a new Metadata object.
                    metadata = new Metadata();
                    metadata.ReceiveBuf = new byte[m_ReceiveBufSize];

                    // Continue to receive data from the connected Client.
                    Worker.BeginReceive(
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

                // Fire the ExceptionThrown event (bound during instantiation of WorkerSocket).
                // The IF statement is used to ensure that listener exist for ExceptionThrown Event.
                if (m_ExceptionThrown != null)
                {
                    m_ExceptionThrown.Invoke(this, args);
                }

                SupportMethod.ShowExceptionMessage(ex, Output.EventLog);
            }
        }
        private void OnMessageSent(IAsyncResult result)
        {
            try
            {
                Worker.EndSend(result);
                // IAsyncResult.AsyncState contain the Object which passed as last parameter in 
                // Socket.BeginSend().
                Metadata metadata = (Metadata)result.AsyncState;

                LastMessageSent = m_Encoder.GetString(metadata.SendBuf);

                // Fire the event (bound during instantiation of WorkerSocket). 
                // The IF statement is used to ensure that listener exist for MessageSent Event.
                MessageSentEventArgs args = new MessageSentEventArgs();
                args.MessageType = metadata.MessageType;
                if (m_MessageSent != null)
                {
                    m_MessageSent.Invoke(this, args);
                }
            }
            catch (Exception ex)
            {
                ExceptionThrownEventArgs args = new ExceptionThrownEventArgs();
                args.Exception = ex;

                // Fire the ExceptionThrown event (bound during instantiation of WorkerSocket).
                // The IF statement is used to ensure that listener exist for ExceptionThrown Event.
                if (m_ExceptionThrown != null)
                {
                    m_ExceptionThrown.Invoke(this, args);
                }

                SupportMethod.ShowExceptionMessage(ex, Output.EventLog);
            }
        }
        #endregion

        #region Overload of constructors.
        private WorkerSocket()
        {
            InitData();
        }
        #region Description.
        /// <summary>
        /// Initializes a new instance of the SocketManager.WorkerSocket class using the specified parameters.
        /// </summary>
        /// <param name="soc">An instance of type System.Net.Sockets.Socket class.</param>
        /// <param name="sendBufSize">Size of the array of type System.Byte that contains the data to send.</param>
        /// <param name="receiveBufSize">Size of the array of type System.Byte that is the storage location of the received data.</param>
        /// <param name="echoOnReceived">TRUE if the WorkerSockets is required to send back last received message from the corresponding client, else FALSE.</param>
        #endregion
        internal WorkerSocket(Socket soc, int sendBufSize, int receiveBufSize, bool echoOnReceived)
            : this()
        {
            Worker = soc;
            m_SendBufSize = sendBufSize;
            m_ReceiveBufSize = receiveBufSize;
            m_EchoOnReceived = echoOnReceived;

            InitWorker();
        }
        #region Description.
        /// <summary>
        /// Initializes a new instance of the SocketManager.WorkerSocket class using the specified parameters.
        /// </summary>
        /// <param name="soc">An instance of type System.Net.Sockets.Socket class.</param>
        /// <param name="sendBufSize">Size of the array of type System.Byte that contains the data to send.</param>
        /// <param name="receiveBufSize">Size of the array of type System.Byte that is the storage location of the received data.</param>
        /// <param name="pollResTime">Response time (us) when polling for the state of the socket.</param>
        /// <param name="echoOnReceived">TRUE if the WorkerSockets is required to send back last received message from the corresponding client, else FALSE.</param>
        #endregion
        internal WorkerSocket(Socket soc, int sendBufSize, int receiveBufSize, int pollResTime, bool echoOnReceived)
            : this(soc, sendBufSize, receiveBufSize, echoOnReceived)
        {
            m_PollResTime = pollResTime;
        }
        #endregion
    }
}
