namespace TCPIPManager.EventArgs
{
    public class ConnectionStateChangedEventArgs : System.EventArgs
    {
        // Fields
        #region Description.
        /// <summary>
        /// Gets or sets the connection state of the socket.
        /// </summary>
        #endregion
        public bool ConnectionStatus
        {
            get;
            set;
        }
        #region Description.
        /// <summary>
        /// Gets or sets the object that contains additional data which may be used in the event handler.
        /// </summary>
        #endregion
        public object Tag
        {
            get;
            set;
        }

        // Constructors
        #region Description
        /// <summary>
        /// Provides data for the SocketManager.ClientSocket.ConnectionStateChanged, SocketServer.SocketManager.ConnectionStateChanged, and SocketManager.WorkerSocket.ConnectionStateChanged event.
        /// </summary>
        #endregion
        public ConnectionStateChangedEventArgs()
        {
            ConnectionStatus = false;
            Tag = null;
        }
    }
}
