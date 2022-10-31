using TCPIPManager.EventArgs;

namespace TCPIPManager.EventHandler
{
    #region Description.
    /// <summary>
    /// Represent the methods that handles the BaseSocket.ConnectionStateChanged event of a BaseSocket.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    #endregion
    public delegate void ConnectionStateChangedEventHandler(object sender, ConnectionStateChangedEventArgs args);
}
