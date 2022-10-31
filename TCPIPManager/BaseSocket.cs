using TCPIPManager.EventHandler;

namespace TCPIPManager
{
    public abstract class BaseSocket
    {
        #region Fields.
        #region Description.
        /// <summary>
        /// Size of the array of type System.Byte that contains the data to send.
        /// </summary>
        #endregion
        protected int m_SendBufSize;
        #region Description.
        /// <summary>
        /// Size of the array of type System.Byte that is the storage location of the received data.
        /// </summary>
        #endregion
        protected int m_ReceiveBufSize;
        #region Description.
        /// <summary>
        /// TRUE if the socket is required to send back last received message from sender, else FALSE.
        /// </summary>
        #endregion
        protected bool m_EchoOnReceived;
        #region Description.
        /// <summary>
        /// TRUE if the socket is required to wait echo message from sender after sending data, else FALSE.
        /// </summary>
        #endregion
        protected bool m_WaitEchoOnSent;
        #region Description.
        /// <summary>
        /// Response time when polling for the state of the socket.
        /// </summary>
        #endregion
        protected int m_PollResTime;
        #endregion

        #region Delegates.
        #region Description.
        /// <summary>
        /// Occurs when the socket received a message.
        /// </summary>
        #endregion
        protected MessageReceivedEventHandler m_MessageReceived;
        #region Description.
        /// <summary>
        /// Occurs when the socket sent a message.
        /// </summary>
        #endregion
        protected MessageSentEventHandler m_MessageSent;
        #region Description.
        /// <summary>
        /// Occurs when connection state of the socket has changed.
        /// </summary>
        #endregion
        protected ConnectionStateChangedEventHandler m_ConnectionStateChanged;
        #region Description.
        /// <summary>
        /// Occurs when SocketManager throw exception.
        /// </summary>
        #endregion
        protected ExceptionThrownEventHandler m_ExceptionThrown;
        #endregion

        #region Events.
        #region Description.
        /// <summary>
        /// Occurs when the socket received a message.
        /// </summary>
        #endregion
        public event MessageReceivedEventHandler MessageReceived
        {
            add
            {
                m_MessageReceived += value;
            }
            remove
            {
                m_MessageReceived -= value;
            }
        }
        #region Description.
        /// <summary>
        /// Occurs when the socket sent a message.
        /// </summary>
        #endregion
        public event MessageSentEventHandler MessageSent
        {
            add
            {
                m_MessageSent += value;
            }
            remove
            {
                m_MessageSent -= value;
            }
        }
        #region Description.
        /// <summary>
        /// Occurs when connection state of the socket has changed.
        /// </summary>
        #endregion
        public event ConnectionStateChangedEventHandler ConnectionStateChanged
        {
            add
            {
                m_ConnectionStateChanged += value;
            }
            remove
            {
                m_ConnectionStateChanged -= value;
            }
        }
        #region Description.
        /// <summary>
        /// Occurs when SocketManager throw exception.
        /// </summary>
        #endregion
        public event ExceptionThrownEventHandler ExceptionThrown
        {
            add
            {
                m_ExceptionThrown += value;
            }
            remove
            {
                m_ExceptionThrown -= value;
            }
        }
        #endregion

        #region Private methods.
        private void InitData()
        {
            // Fields
            m_SendBufSize = 0;
            m_ReceiveBufSize = 0;
            m_EchoOnReceived = false;
            m_WaitEchoOnSent = false;

            // Delegates
            m_MessageReceived = null;
            m_MessageSent = null;
            m_ConnectionStateChanged = null;
            m_ExceptionThrown = null;
        }
        #endregion

        #region Overload of constructors.
        #region Description.
        /// <summary>
        /// Define a base class for SocketManager.ServerSocket, SocketManager.WorkerSocket, and SocketManager.ClientSocket.
        /// </summary>
        #endregion
        protected BaseSocket()
        {
            InitData();
        }
        #endregion
    }
}
