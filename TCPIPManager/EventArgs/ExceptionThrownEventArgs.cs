using System;

namespace TCPIPManager.EventArgs
{
    public class ExceptionThrownEventArgs : System.EventArgs
    {
        // Fields
        #region Description.
        /// <summary>
        /// Gets the instance of the System.Exception object. It can only be set internally.
        /// </summary>
        #endregion
        public Exception Exception
        {
            get;
            internal set;
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
        /// Provides data for the SocketManager.ClientSocket.ExceptionThrown, SocketManager.ServerSocket.ExceptionThrown, and SocketManager.WorkerSocket.ExceptionThrown event.
        /// </summary>
        #endregion
        public ExceptionThrownEventArgs()
        {
            Exception = null;
            Tag = null;
        }
    }
}
