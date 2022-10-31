
using TCPIPManager.Enums;

namespace TCPIPManager.EventArgs
{
    public class MessageSentEventArgs : System.EventArgs
    {
        // Fields
        #region Description.
        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        #endregion
        public MessageType MessageType
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
        /// Provides data for the SocketManager.ClientSocket.MessageSent, SocketManager.ServerSocket.MessageSent, and SocketManager.WorkerSocket.MessageSent event.
        /// </summary>
        #endregion
        public MessageSentEventArgs()
        {
            MessageType = MessageType.None;
            Tag = null;
        }
    }
}
