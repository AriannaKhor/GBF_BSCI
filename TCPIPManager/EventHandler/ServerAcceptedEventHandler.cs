using TCPIPManager.EventArgs;

namespace TCPIPManager.EventHandler
{
    #region Description.
    /// <summary>
    /// Represent the methods that handles the ServerSocket.ServerAccepted event of a ServerSocket.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    #endregion
    public delegate void ServerAcceptedEventHandler(object sender, ServerAcceptedEventArgs args);
}
