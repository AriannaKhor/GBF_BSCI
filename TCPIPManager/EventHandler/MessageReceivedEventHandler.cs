using TCPIPManager.EventArgs;

namespace TCPIPManager.EventHandler
{
    #region Description.
    /// <summary>
    /// Represent the methods that handles the BaseSocket.MessageReceived event of a BaseSocket.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    #endregion
    public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs args);
}
