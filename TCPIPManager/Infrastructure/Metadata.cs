
using TCPIPManager.Enums;

namespace TCPIPManager.Infrastructure
{
    internal class Metadata
    {
        // Properties
        #region Description.
        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        #endregion
        internal MessageType MessageType
        {
            get;
            set;
        }
        #region Description.
        /// <summary>
        /// Gets or sets the stream buffer that is currently used by the socket to send data.
        /// </summary>
        #endregion
        internal byte[] SendBuf
        {
            get;
            set;
        }
        #region Description.
        /// <summary>
        /// Gets or sets the stream buffer that is currently used by the socket to receive data.
        /// </summary>
        #endregion
        internal byte[] ReceiveBuf
        {
            get;
            set;
        }

        // Constructors
        #region Description
        /// <summary>
        /// Provides additional data for the socket operation.
        /// </summary>
        #endregion
        public Metadata()
        {
            MessageType = MessageType.None;
            SendBuf = null;
            ReceiveBuf = null;
        }
    }
}
