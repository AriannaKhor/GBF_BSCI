using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using TCPIPManager.Enums;
using TCPIPManager.EventArgs;
using TCPIPManager.EventHandler;
using TCPIPManager.Infrastructure;

namespace TCPIPManager
{
    public class ServerSocket : BaseSocket
    {
        #region Fields.
        private object m_Key;
        #endregion

        #region Properties.
        #region Description.
        /// <summary>
        /// Gets the instance of the System.Net.Sockets object that is currently used by ServerSocket. It can only be set internally.
        /// </summary>
        #endregion
        public Socket Listener
        {
            get;
            private set;
        }
        #region Description.
        /// <summary>
        /// Gets a value indicating whether the ServerSocket is already on listening state. It can only be set internally.
        /// </summary>
        #endregion
        public bool IsStarted
        {
            get;
            private set;
        }
        #region Description.
        /// <summary>
        /// Gets the collection of WorkerSockets that is corresponding to the connected clients. It can only be set internally.
        /// </summary>
        #endregion
        public List<WorkerSocket> WorkerSocketCollection
        {
            get;
            private set;
        }
        #endregion

        #region Delegates.
        private ServerAcceptedEventHandler m_ServerAccepted;
        #endregion

        #region Events.
        #region Description.
        /// <summary>
        /// Occurs when the ServerSocket accepted a connection from client.
        /// </summary>
        #endregion
        public event ServerAcceptedEventHandler ServerAccepted
        {
            add
            {
                m_ServerAccepted += value;
            }
            remove
            {
                m_ServerAccepted -= value;
            }
        }
        #endregion

        #region Public methods.
        #region Description.
        /// <summary>
        /// Start listening from client activity.
        /// </summary>
        /// <param name="ip">IP address that the ServerSocket must listen to.</param>
        /// <param name="port">The port number associated with the IP address, or 0 to specify any available port.</param>
        #endregion
        public void Start(IPAddress ip, int port)
        {
            try
            {
                // Instantiate an IPEndPoint using the given IP and given port.
                IPEndPoint endPoint = new IPEndPoint(ip, port);

                // Bind the Socket to the EndPoint.
                Listener.Bind(endPoint);

                // Start listening from the IPEndPoint. SocketOptionName.MaxConnections
                // specifies the number of incoming connections that can be queued for
                // acceptance.
                Listener.Listen((int)SocketOptionName.MaxConnections);
                IsStarted = true;

                // Start to listen for connection.
                Listener.BeginAccept(new AsyncCallback(OnServerAccepted), Listener);
            }
            catch (Exception ex)
            {
                ExceptionThrownEventArgs args = new ExceptionThrownEventArgs();
                args.Exception = ex;

                // The IF statement is used to ensure that listener exist for 
                // ExceptionThrown Event.
                if (m_ExceptionThrown != null)
                {
                    #region Fire the ExceptionThrown event (bound during instantiation of ServerSocket).
                    // Check the Target of each delegate in the event's invocation list, and marshal the call
                    // to the target thread if that target is ISynchronizeInvoke
                    // ref: http://stackoverflow.com/questions/1698889/raise-events-in-net-on-the-main-ui-thread

                    foreach (Delegate del in m_ExceptionThrown.GetInvocationList())
                    {
                        ISynchronizeInvoke syncer = del.Target as ISynchronizeInvoke;
                        if (syncer == null)
                        {
                            del.DynamicInvoke(this, args);
                        }
                        else
                        {
                            syncer.BeginInvoke(del, new object[] { this, args });
                        }
                    }
                    #endregion
                }

                SupportMethod.ShowExceptionMessage(ex, Output.EventLog);
            }
        }
        #region Description.
        /// <summary>
        /// Stop listening from client activity.
        /// </summary>
        #endregion
        public void Stop()
        {
            try
            {
                IsStarted = false;

                // Close the ServerSocket (Listener) IF AND ONLY IF the ServerSocket is currently bound
                // to a specific local port.
                if (Listener.IsBound)
                {
                    // Stop listening to more client and dispose the listening socket (m_Listener). Upon closing
                    // the listening socket using Socket.Close(), the listening socket will be disposed. Closing
                    // the listening socket will also trigger ServerSocket.OnServerAccepted() callback, which in
                    // turn throw ObjectDisposedException at Socket.EndAccept(). We have to catch this exception.
                    Listener.Close();
                }

                // Close all WorkerSocket.Worker in m_WorkerSocketCollection.
                int count = WorkerSocketCollection.Count;
                for (int i = 0; i < count; i++)
                {
                    // Close each WorkerSocket.Worker in WorkerSocketCollection gracefully.
                    WorkerSocketCollection[i].Worker.Close();
                }

                // Remove all WorkerSocket from m_WorkerSocketCollection.
                WorkerSocketCollection.Clear();
            }
            catch (Exception ex)
            {
                ExceptionThrownEventArgs args = new ExceptionThrownEventArgs();
                args.Exception = ex;

                // The IF statement is used to ensure that listener exist for 
                // ExceptionThrown Event.
                if (m_ExceptionThrown != null)
                {
                    #region Fire the ExceptionThrown event (bound during instantiation of ServerSocket).
                    // Check the Target of each delegate in the event's invocation list, and marshal the call
                    // to the target thread if that target is ISynchronizeInvoke
                    // ref: http://stackoverflow.com/questions/1698889/raise-events-in-net-on-the-main-ui-thread

                    foreach (Delegate del in m_ExceptionThrown.GetInvocationList())
                    {
                        ISynchronizeInvoke syncer = del.Target as ISynchronizeInvoke;
                        if (syncer == null)
                        {
                            del.DynamicInvoke(this, args);
                        }
                        else
                        {
                            syncer.BeginInvoke(del, new object[] { this, args });
                        }
                    }
                    #endregion
                }

                SupportMethod.ShowExceptionMessage(ex, Output.EventLog);
            }
        }
        #region Description.
        /// <summary>
        /// Send the message to all connected clients.
        /// </summary>
        /// <param name="message">Message that will be sent.</param>
        #endregion
        public void Broadcast(string message)
        {
            foreach (WorkerSocket workerSoc in WorkerSocketCollection)
            {
                workerSoc.Send(message);
            }
        }
        #region Description.
        /// <summary>
        /// Send the message to a specific client using its corresponding WorkerSocket.
        /// </summary>
        /// <param name="workerSoc">WorkerSocket corresponding to the intended client.</param>
        /// <param name="message">>Message that will be sent.</param>
        #endregion
        public void Send(WorkerSocket workerSoc, string message)
        {
            workerSoc.Send(message);
        }
        #endregion

        #region Private methods.
        private void InitData()
        {
            // Fields
            m_EchoOnReceived = false;
            m_SendBufSize = 0;
            m_ReceiveBufSize = 0;

            // Properties
            Listener = null;
            IsStarted = false;
            WorkerSocketCollection = new List<WorkerSocket>();

            // Delegates
            m_ServerAccepted = null;
        }
        #endregion

        #region Callback for socket operation.
        private void OnServerAccepted(IAsyncResult result)
        {
            try
            {
                // Accepts the incoming connection and returns a new Socket that can be used 
                // to send data to and receive data from the particular client.
                Socket soc = Listener.EndAccept(result);

                // Instantiate a WorkerSocket object using soc and add it into m_WorkerSocketCollection.
                WorkerSocket workerSoc = new WorkerSocket(soc, m_SendBufSize, m_ReceiveBufSize, m_PollResTime, m_EchoOnReceived);
                workerSoc.MessageReceived += new MessageReceivedEventHandler(OnMessageReceived);
                workerSoc.MessageSent += new MessageSentEventHandler(OnMessageSent);
                workerSoc.ConnectionStateChanged += new ConnectionStateChangedEventHandler(OnConnectionStateChanged);
                workerSoc.ExceptionThrown += new ExceptionThrownEventHandler(OnExceptionThrown);
                WorkerSocketCollection.Add(workerSoc);

                // The IF statement is used to ensure that listener exist for 
                // ServerAccepted Event.
                ServerAcceptedEventArgs args = new ServerAcceptedEventArgs();
                args.Tag = workerSoc;
                if (m_ServerAccepted != null)
                {
                    #region Fire the ServerAccepted event (bound during instantiation of ServerSocket).
                    // Check the Target of each delegate in the event's invocation list, and marshal the call
                    // to the target thread if that target is ISynchronizeInvoke
                    // ref: http://stackoverflow.com/questions/1698889/raise-events-in-net-on-the-main-ui-thread

                    foreach (Delegate del in m_ServerAccepted.GetInvocationList())
                    {
                        ISynchronizeInvoke syncer = del.Target as ISynchronizeInvoke;
                        if (syncer == null)
                        {
                            del.DynamicInvoke(this, args);
                        }
                        else
                        {
                            syncer.BeginInvoke(del, new object[] { this, args });
                        }
                    }
                    #endregion
                }

                // Continue to listen for more connection.
                Listener.BeginAccept(new AsyncCallback(OnServerAccepted), Listener);
            }
            catch (ObjectDisposedException ex)
            {
                // Upon closing the Listener at ServerSocket.Stop(), Listener will be disposed. At the same
                // time, closing the listening socket (Listener) will also trigger ServerSocket.OnServerAccepted(),
                // which will in turn throw ObjectDisposedException at Socket.EndAccept(). Here we catch this exception.
                // Also, note that we WILL NOT fire ServerSocket.ExceptionThrown event since this is an expected exception.

                IsStarted = false;

                // Instead of let the exception go off silently, output some information to the 
                // IDE's output window.
                SupportMethod.ShowExceptionMessage(ex, Output.EventLog);

                // Re-instantiate a listening soket into m_Listener so that it can start listening to client when
                // ServerSocket.Start() is called.
                Listener = new Socket(Listener.AddressFamily, Listener.SocketType, Listener.ProtocolType);
            }
            catch (Exception ex)
            {
                ExceptionThrownEventArgs args = new ExceptionThrownEventArgs();
                args.Exception = ex;

                // The IF statement is used to ensure that listener exist for 
                // ExceptionThrown Event.
                if (m_ExceptionThrown != null)
                {
                    #region Fire the ExceptionThrown event (bound during instantiation of ServerSocket).
                    // Check the Target of each delegate in the event's invocation list, and marshal the call
                    // to the target thread if that target is ISynchronizeInvoke
                    // ref: http://stackoverflow.com/questions/1698889/raise-events-in-net-on-the-main-ui-thread

                    foreach (Delegate del in m_ExceptionThrown.GetInvocationList())
                    {
                        ISynchronizeInvoke syncer = del.Target as ISynchronizeInvoke;
                        if (syncer == null)
                        {
                            del.DynamicInvoke(this, args);
                        }
                        else
                        {
                            syncer.BeginInvoke(del, new object[] { this, args });
                        }
                    }
                    #endregion
                }

                SupportMethod.ShowExceptionMessage(ex, Output.EventLog);
            }
        }
        #endregion

        #region EventHandler.
        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                // NOTE: 
                // sender object is of type WorkerSocket which is passed to OnMessageReceived()
                // when WorkerReceived is fired in WorkerSocket.OnMessageReceived().

                // Assign MessageReceivedEventArgs.Tag property with the sender (which is a WorkerSocket).
                // Event handler at the client code can use this property to identify which WorkerSocket trigger
                // the event.
                e.Tag = sender;

                // The IF statement is used to ensure that listener exist for 
                // MessageReceived Event.
                if (m_MessageReceived != null)
                {
                    #region Fire the MessageReceived event (bound during instantiation of ServerSocket).
                    // Check the Target of each delegate in the event's invocation list, and marshal the call
                    // to the target thread if that target is ISynchronizeInvoke
                    // ref: http://stackoverflow.com/questions/1698889/raise-events-in-net-on-the-main-ui-thread

                    foreach (Delegate del in m_MessageReceived.GetInvocationList())
                    {
                        ISynchronizeInvoke syncer = del.Target as ISynchronizeInvoke;
                        if (syncer == null)
                        {
                            del.DynamicInvoke(this, e);
                        }
                        else
                        {
                            syncer.BeginInvoke(del, new object[] { this, e });
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                ExceptionThrownEventArgs args = new ExceptionThrownEventArgs();
                args.Exception = ex;

                // The IF statement is used to ensure that listener exist for 
                // ExceptionThrown Event.
                if (m_ExceptionThrown != null)
                {
                    #region Fire the ExceptionThrown event (bound during instantiation of ServerSocket).
                    // Check the Target of each delegate in the event's invocation list, and marshal the call
                    // to the target thread if that target is ISynchronizeInvoke
                    // ref: http://stackoverflow.com/questions/1698889/raise-events-in-net-on-the-main-ui-thread

                    foreach (Delegate del in m_ExceptionThrown.GetInvocationList())
                    {
                        ISynchronizeInvoke syncer = del.Target as ISynchronizeInvoke;
                        if (syncer == null)
                        {
                            del.DynamicInvoke(this, args);
                        }
                        else
                        {
                            syncer.BeginInvoke(del, new object[] { this, args });
                        }
                    }
                    #endregion
                }

                SupportMethod.ShowExceptionMessage(ex, Output.EventLog);
            }
        }
        private void OnMessageSent(object sender, MessageSentEventArgs e)
        {
            try
            {
                // NOTE: 
                // sender object is of type WorkerSocket which is passed to OnMessageSent()
                // when WorkerSent is fired in WorkerSocket.OnMessageSent().

                // Assign MessageSentEventArgs.Tag property with the sender (which is a WorkerSocket).
                // Event handler at the client code can use this property to identify which WorkerSocket trigger
                // the event.
                e.Tag = sender;

                // The IF statement is used to ensure that listener exist for 
                // MessageSent Event.
                if (m_MessageSent != null)
                {
                    #region Fire the MessageSent event (bound during instantiation of ServerSocket).
                    // Check the Target of each delegate in the event's invocation list, and marshal the call
                    // to the target thread if that target is ISynchronizeInvoke
                    // ref: http://stackoverflow.com/questions/1698889/raise-events-in-net-on-the-main-ui-thread

                    foreach (Delegate del in m_MessageSent.GetInvocationList())
                    {
                        ISynchronizeInvoke syncer = del.Target as ISynchronizeInvoke;
                        if (syncer == null)
                        {
                            del.DynamicInvoke(this, e);
                        }
                        else
                        {
                            syncer.BeginInvoke(del, new object[] { this, e });
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                ExceptionThrownEventArgs args = new ExceptionThrownEventArgs();
                args.Exception = ex;

                // The IF statement is used to ensure that listener exist for 
                // ExceptionThrown Event.
                if (m_ExceptionThrown != null)
                {
                    #region Fire the ExceptionThrown event (bound during instantiation of ServerSocket).
                    // Check the Target of each delegate in the event's invocation list, and marshal the call
                    // to the target thread if that target is ISynchronizeInvoke
                    // ref: http://stackoverflow.com/questions/1698889/raise-events-in-net-on-the-main-ui-thread

                    foreach (Delegate del in m_ExceptionThrown.GetInvocationList())
                    {
                        ISynchronizeInvoke syncer = del.Target as ISynchronizeInvoke;
                        if (syncer == null)
                        {
                            del.DynamicInvoke(this, args);
                        }
                        else
                        {
                            syncer.BeginInvoke(del, new object[] { this, args });
                        }
                    }
                    #endregion
                }

                SupportMethod.ShowExceptionMessage(ex, Output.EventLog);
            }
        }
        private void OnConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            // WorkerSocket has a mechanism to poll for its own state. The polling is done by a timer which
            // runs on a worker thread. This eventhandler will be triggered from the worker thread if the
            // worker socket is disconnected. There are cases where multiple worker sockets are disconnected
            // within a very short interval and cause multiple worker threads to enter to the following code
            // block at (almost) the same time. When this happens, removal of disconnected worker socket from
            // WorkerSocketCollection may fail as the list is not thread-safe. As such, we use 'lock' to ensure 
            // only one worker thread can enter the following code block.
            //
            // However, there is one loop hole with regards to this. Consider the following:
            // 
            // When a worker socket is removed from the WorkerSocketCollection, ServerSocket.ConnectionStateChanged 
            // will be triggered, which its eventhandler will be executed on the threading context of the target 
            // thread (instead of the threading context of timer's worker thread). Timer worker thread's control 
            // will return and release the lock while the eventhandler of ServerSocket.ConnectionStateChanged is
            // executing (on the threading context of the target thread). At this point, there could be a racing 
            // condition where the eventhandler of ServerSocket.ConnectionStateChanged attempt to access 
            // WorkerSocketCollection, at the same time another timer's worker thread acquire the lock and attempt 
            // to remove the disconnected worker socket from WorkerSocketCollection. When this happens, exception 
            // may thrown.
            lock (m_Key)
            {
                // NOTE: 
                // sender object is of type WorkerSocket which is passed to OnWorkerConnectionStatusChanged()
                // when ConnectionStateChanged is fired in WorkerSocket.IsAlive property.

                // Assign ConnectionStateChangedEventArgs.Tag property with the sender (which is a WorkerSocket).
                // Event handler at the client code can use this property to identify which WorkerSocket trigger
                // the event.
                e.Tag = sender;

                // In case of Worker disconnection (args.ConnectionStatus == false), it will be removed from 
                // m_WorkerSocketCollection.
                if (e.ConnectionStatus == false)
                {
                    // Remove disconnected worker socket. Removal of worker socket must be done in a thread-safe
                    // manner, hence the 'lock' statement.
                    WorkerSocketCollection.Remove((WorkerSocket)sender);
                }

                // The IF statement is used to ensure that listener exist for 
                // ConnectionStatusChanged Event.
                if (m_ConnectionStateChanged != null)
                {
                    #region Fire the ConnectionStateChanged event (bound during instantiation of ServerSocket).
                    // Check the Target of each delegate in the event's invocation list, and marshal the call
                    // to the target thread if that target is ISynchronizeInvoke
                    // ref: http://stackoverflow.com/questions/1698889/raise-events-in-net-on-the-main-ui-thread

                    foreach (Delegate del in m_ConnectionStateChanged.GetInvocationList())
                    {
                        ISynchronizeInvoke syncer = del.Target as ISynchronizeInvoke;
                        if (syncer == null)
                        {
                            del.DynamicInvoke(this, e);
                        }
                        else
                        {
                            syncer.BeginInvoke(del, new object[] { this, e });
                        }
                    }
                    #endregion
                }
            }
        }
        private void OnExceptionThrown(object sender, ExceptionThrownEventArgs e)
        {
            // NOTE: 
            // This event handler will be triggered when the WorkerSocket encounter exception. 
            // sender object is of type WorkerSocket which is passed to OnExceptionThrown()
            // when ExceptionThrown event is fired in WorkerSocket.

            // Assign ExceptionThrownEventArgs.Tag property with the sender (which is a WorkerSocket).
            // Event handler at the client code can use this property to identify which WorkerSocket trigger
            // the event.
            e.Tag = sender;

            // The IF statement is used to ensure that listener exist for 
            // ExceptionThrown Event.
            if (m_ExceptionThrown != null)
            {
                #region Fire the ExceptionThrown event (bound during instantiation of ServerSocket).
                // Check the Target of each delegate in the event's invocation list, and marshal the call
                // to the target thread if that target is ISynchronizeInvoke
                // ref: http://stackoverflow.com/questions/1698889/raise-events-in-net-on-the-main-ui-thread

                foreach (Delegate del in m_ExceptionThrown.GetInvocationList())
                {
                    ISynchronizeInvoke syncer = del.Target as ISynchronizeInvoke;
                    if (syncer == null)
                    {
                        del.DynamicInvoke(this, e);
                    }
                    else
                    {
                        syncer.BeginInvoke(del, new object[] { this, e });
                    }
                }
                #endregion
            }
        }
        #endregion

        #region Overload of constructors.
        private ServerSocket()
        {
            InitData();
        }
        private ServerSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
            : this()
        {
            Listener = new Socket(addressFamily, socketType, protocolType);
        }
        #region Description.
        /// <summary>
        /// Initializes a new instance of the ServerSocket class using the specified parameters.
        /// </summary>
        /// <param name="addressFamily">One of the System.Net.Sockets.AddressFamily values.</param>
        /// <param name="socketType">One of the System.Net.Sockets.SocketType values.</param>
        /// <param name="protocolType">One of the System.Net.Sockets.ProtocolType values.</param>
        /// <param name="sendBufSize">Size of the array of type System.Byte that contains the data to send.</param>
        /// <param name="receiveBufSize">Size of the array of type System.Byte that is the storage location of the received data.</param>
        /// <param name="echoOnReceived">TRUE if the WorkerSockets is required to send back last received message from the corresponding client, else FALSE.</param>
        #endregion
        public ServerSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, int sendBufSize, int receiveBufSize, bool echoOnReceived)
            : this(addressFamily, socketType, protocolType)
        {
            m_SendBufSize = sendBufSize;
            m_ReceiveBufSize = receiveBufSize;
            m_EchoOnReceived = echoOnReceived;
        }
        #region Description.
        /// <summary>
        /// Initializes a new instance of the ServerSocket class using the specified parameters.
        /// </summary>
        /// <param name="addressFamily">One of the System.Net.Sockets.AddressFamily values.</param>
        /// <param name="socketType">One of the System.Net.Sockets.SocketType values.</param>
        /// <param name="protocolType">One of the System.Net.Sockets.ProtocolType values.</param>
        /// <param name="sendBufSize">Size of the array of type System.Byte that contains the data to send.</param>
        /// <param name="receiveBufSize">Size of the array of type System.Byte that is the storage location of the received data.</param>
        /// <param name="pollResTime">Response time (us) when polling for the state of the worker socket.</param>
        /// <param name="echoOnReceived">TRUE if the WorkerSockets is required to send back last received message from the corresponding client, else FALSE.</param>
        #endregion
        public ServerSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, int sendBufSize, int receiveBufSize, int pollResTime, bool echoOnReceived)
            : this(addressFamily, socketType, protocolType, sendBufSize, receiveBufSize, echoOnReceived)
        {
            m_PollResTime = pollResTime;
        }
        #endregion
    }
}
