namespace TCPIPManager.Enums
{
    public enum MessageType
    {
        #region Description
        /// <summary>
        /// Default message type.
        /// </summary>
        #endregion
        None = 0,
        #region Description
        /// <summary>
        /// Normal message.
        /// </summary>
        #endregion
        Message,
        #region Description
        /// <summary>
        /// Echo, typically used as the acknowledgement for communication-handshaking.
        /// </summary>
        #endregion
        Echo,
        #region Description
        /// <summary>
        /// A network ping.
        /// </summary>
        #endregion
        Ping,
    }
}
