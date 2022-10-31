namespace TCPIPManager.EventArgs
{
    public class MessageReceivedEventArgs : System.EventArgs
    {
        // Fields
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
        /// Provides data for the SocketManager.ClientSocket.MessageReceived, SocketManager.ServerSocket.MessageReceived, and SocketManager.WorkerSocket.MessageReceived event.
        /// </summary>
        #endregion
        public MessageReceivedEventArgs()
        {
            Tag = null;
        }
    }
}
