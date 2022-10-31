namespace TCPIPManager.EventArgs
{
    public class ServerAcceptedEventArgs : System.EventArgs
    {
        // Fields

        // Properties
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
        /// Provides data for the SocketManager.ServerSocket.ServerAccepted event.
        /// </summary>
        #endregion
        public ServerAcceptedEventArgs()
        {
            Tag = null;
        }
    }
}
