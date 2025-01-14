using DuneNet.Shared;
using DuneNet.Shared.Events;
using DuneNet.Shared.Messages;
using DuneNet.Shared.Modules;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace DuneNet.Client.Controllers
{
    /// <summary>
    /// A Network Controller with extra functionality for the client.
    /// </summary>
    public sealed class NetworkController
    {
#region internal
        
        private NetworkClient _netClient;
        private float _maxDelay;

        private bool _connecting;
        private bool _ready;
        
        internal NetworkController()
        {
            Application.runInBackground = true;
            _netClient = new NetworkClient();
            _netClient.SetNetworkConnectionClass<DuneConnection>();
        }

        private void RegisterNetworkHandlers()
        {
            _netClient.RegisterHandler(MsgType.Connect, OnConnectInternal);
            _netClient.RegisterHandler(MsgType.Disconnect, OnDisconnectInternal);
            _netClient.RegisterHandler(MsgType.Ready, OnReadyMessageInternal);
            _netClient.RegisterHandler(MsgType.NotReady, OnNotReadyMessageInternal);
            _netClient.RegisterHandler(MsgType.Error, OnErrorInternal);
            _netClient.RegisterHandler(MsgTypes.RespondHandshake, OnHandshakeResponse);
        }
        
        private static ConnectionConfig RegisterBaseChannels(ConnectionConfig config)
        {
            ConnectionConfig newConfig = new ConnectionConfig
            {
                AckDelay = config.AckDelay,
                AcksType = config.AcksType,
                AllCostTimeout = config.AllCostTimeout,
                BandwidthPeakFactor = config.BandwidthPeakFactor,
                ConnectTimeout = config.ConnectTimeout,
                DisconnectTimeout = config.DisconnectTimeout,
                FragmentSize = config.FragmentSize,
                InitialBandwidth = config.InitialBandwidth,
                MaxCombinedReliableMessageCount = config.MaxCombinedReliableMessageCount,
                MaxCombinedReliableMessageSize = config.MaxCombinedReliableMessageSize,
                MaxConnectionAttempt = config.MaxConnectionAttempt,
                MaxSentMessageQueueSize = config.MaxSentMessageQueueSize,
                MinUpdateTimeout = config.MinUpdateTimeout,
                NetworkDropThreshold = config.NetworkDropThreshold,
                OverflowDropThreshold = config.OverflowDropThreshold,
                PacketSize = config.PacketSize,
                PingTimeout = config.PingTimeout,
                ReducedPingTimeout = config.ReducedPingTimeout,
                ResendTimeout = config.ResendTimeout,
                SendDelay = config.SendDelay,
                SSLCAFilePath = config.SSLCAFilePath,
                SSLCertFilePath = config.SSLCertFilePath,
                SSLPrivateKeyFilePath = config.SSLPrivateKeyFilePath,
                UdpSocketReceiveBufferMaxSize = config.UdpSocketReceiveBufferMaxSize,
                UsePlatformSpecificProtocols = config.UsePlatformSpecificProtocols,
                WebSocketReceiveBufferMaxSize = config.WebSocketReceiveBufferMaxSize
            };

            newConfig.AddChannel(QosType.ReliableSequenced);
            newConfig.AddChannel(QosType.Unreliable);
            newConfig.AddChannel(QosType.Unreliable);
            newConfig.AddChannel(QosType.ReliableSequenced);
            
            foreach (ChannelQOS channel in config.Channels)
            {
                newConfig.AddChannel(channel.QOS);
            }

            return newConfig;
        }

        private void OnConnectInternal(NetworkMessage msg)
        {
            _connecting = false;
            GetLocalConnection().SetMaxDelay(_maxDelay);
            EventArguments args = new EventArguments();
            DuneClient.EventController.InvokeEvent("OnClientConnect", args);

            HandshakeRequest handshakeRequest = null;
            DuneClient.InvokeOnModules(x => handshakeRequest = x.SendHandshake());

            if (handshakeRequest != null)
            {
                GetLocalConnection().IDToken = handshakeRequest.IDToken;
                SharedHandshakeRequest reqMsg = new SharedHandshakeRequest(handshakeRequest);
                SendByChannelToServer(MsgTypes.RequestHandshake, reqMsg, MessageChannels.GeneralReliableSequenced);
            }
            else
            {
                DuneLog.LogError("No authentication providers found. Are you missing a DuneModule?");
                Disconnect();
            }
        }
        
        private void OnDisconnectInternal(NetworkMessage msg)
        {
            _connecting = false;
            EventArguments args = new EventArguments();
            DuneClient.EventController.InvokeEvent("OnClientDisconnect", args);
        }
        
        private void OnReadyMessageInternal(NetworkMessage msg)
        {
            _ready = true;
            msg.conn.isReady = true;
            EventArguments args = new EventArguments();
            DuneClient.EventController.InvokeEvent("OnClientReady", args);
        }
        
        private void OnNotReadyMessageInternal(NetworkMessage msg)
        {
            _ready = false;
            msg.conn.isReady = false;
            EventArguments args = new EventArguments();
            DuneClient.EventController.InvokeEvent("OnClientNotReady", args);
        }
        
        private void OnErrorInternal(NetworkMessage msg)
        {
            _connecting = false;
            ErrorMessage errorMsg = msg.ReadMessage<ErrorMessage>();
            HandleErrorsInternal((NetworkError) errorMsg.errorCode);
        }

        private void HandleErrorsInternal(NetworkError error)
        {
            EventArguments args = new EventArguments();
            
            switch (error)
            {
                case NetworkError.Timeout:
                    DuneClient.EventController.InvokeEvent("OnClientErrorTimeout", args);
                    break;
                case NetworkError.BadMessage:
                    DuneClient.EventController.InvokeEvent("OnClientErrorBadMessage", args);
                    break;
                case NetworkError.DNSFailure:
                    DuneClient.EventController.InvokeEvent("OnClientErrorDNSFailure", args);
                    break;
            }
            
            args.SetVariable("networkError", error);
            DuneClient.EventController.InvokeEvent("OnClientErrorGeneric", args);
        }

        private void OnHandshakeResponse(NetworkMessage msg)
        {
            SharedHandshakeResponse res = msg.ReadMessage<SharedHandshakeResponse>();

            if (res.Allowed)
            {
                GetLocalConnection().Authenticated = true;
                EventArguments args = new EventArguments();
                DuneClient.EventController.InvokeEvent("OnClientAuthenticationSuccess", args);
            }
            else
            {
                EventArguments args = new EventArguments();
                args.SetVariable("error", res.Error);
                DuneClient.EventController.InvokeEvent("OnClientAuthenticationError", args);
                
                Disconnect();
            }
        }
        
#endregion
#region public

        /// <summary>
        /// Initializes the Network Controller.
        /// </summary>
        /// <remarks>
        /// Must be called before using any of the Network Controller's functionality.
        /// </remarks>
        /// <param name="globalConfig">The global configuration to use for the Network Controller.</param>
        /// <param name="connectionConfig">The connection configuration to use for the Network Controller.</param>
        /// <param name="maxConnections">The maximum amount of connections the Network Controller will handle at the same time. 1 connection is usually enough for most cases.</param>
        /// <param name="maxDelay">The maximum delay before sending packets on connections. Setting this to zero will send messages as soon as they are ready instead of aggregating them.</param>
        public void Init(GlobalConfig globalConfig, ConnectionConfig connectionConfig, int maxConnections = 1, float maxDelay = 0.1f)
        {
            _maxDelay = maxDelay;
            NetworkCRC.scriptCRCCheck = false;
            NetworkTransport.Init(globalConfig);
            _netClient.Configure(RegisterBaseChannels(connectionConfig), maxConnections);
            RegisterNetworkHandlers();
            DuneClient.EntityController.Init();
            DuneClient.EventController.Init();
        }

        /// <summary>
        /// Connects to a serverside Network Controller.
        /// </summary>
        /// <remarks>
        /// This is an asynchronous operation whose results can be hooked into by using a DuneModule or by listening for the following events:
        /// <list type="bullet">
        ///     <item>OnClientConnect. Event Variables: None
        ///     </item>
        ///     <item>OnClientDisconnect. Event Variables: None
        ///     </item>
        ///     <item>OnClientErrorTimeout. Event Variables: None
        ///     </item>
        ///     <item>OnClientErrorBadMessage. Event Variables: None
        ///     </item>
        ///     <item>OnClientErrorDNSFailure. Event Variables: None
        ///     </item>
        ///     <item>OnClientErrorGeneric. Event Variables:
        ///         <list type="bullet"> 
        ///             <item>(NetworkError) networkError = The error code.</item> 
        ///         </list>
        ///     </item>
        ///     <item>OnClientAuthenticationSuccess. Event Variables: None
        ///     </item>
        ///     <item>OnClientAuthenticationError. Event Variables: None
        ///     </item>
        /// </list>
        /// </remarks>
        /// <param name="ip">The IP of the serverside Network Controller to connect to.</param>
        /// <param name="port">The port of the serverside Network Controller to connect to.</param>
        /// <returns>
        /// True if the Network Controller was able to start the connection process, 
        /// False otherwise (for example, if the client was already trying to establish a connection or was already connected).
        /// </returns>
        public bool Connect(string ip, int port)
        {
            if (_connecting || IsConnected()) return false;
            
            _netClient.Connect(ip, port);
            return _connecting = true;
        }

        /// <summary>
        /// Disconnects the Network Controller.
        /// </summary>
        /// <remarks>
        /// If no connetion was established when called but the Network Controller was trying to do so, it will stop attempting the connection (this case will not invoke the OnClientDisconnect event).
        /// </remarks>
        /// <returns>
        /// True if the Network Controller was able to disconnect, 
        /// False otherwise (for example, if the client was not connected).
        /// </returns>
        public bool Disconnect()
        {
            if (!IsConnected()) return _connecting = false;
            
            _netClient.Disconnect();
            return true;
        }

        /// <summary>
        /// Shuts down the Network Controller. Init must be called again to perform further operations with the Network Controller after calling this.
        /// </summary>
        /// <remarks>
        /// There should be no need to manually call this in most cases as it is already automatically called on application quit.
        /// </remarks>
        public void Shutdown()
        {
            Disconnect();
            _netClient.Shutdown();
            _connecting = false;
        }
        
        /// <summary>
        /// Returns whether the Network Controller is currently connected to a serverside Network Controller.
        /// </summary>
        /// <returns>True if the Network Controller is connected, False otherwise.</returns>
        public bool IsConnected()
        {
            return _netClient != null && _netClient.isConnected;
        }

        /// <summary>
        /// Returns whether the Network Controller is currently attempting to establish a connection.
        /// </summary>
        /// <returns>True if the Network Controller is attempting a connection, False otherwise.</returns>
        public bool IsConnecting()
        {
            return _connecting;
        }

        /// <summary>
        /// Returns whether the Network Controller has successfully authenticated with a serverside Network Controller.
        /// </summary>
        /// <remarks>
        /// It is preferable to use a DuneModule or the events OnClientAuthenticationSuccess and OnClientAuthenticationError to handle authentication rather than using this method. 
        /// </remarks>
        /// <returns>True if the Network Controller is authenticated, False otherwise.</returns>
        public bool IsAuthenticated()
        {
            return GetLocalConnection().Authenticated;
        }

        /// <summary>
        /// Sets the Network Controller ready.
        /// </summary>
        /// <remarks>
        /// This instructs the receiving serverside Network Controller that this Network Controller is ready to receive scene information.
        /// This method has no effect until the Network Controller has been authenticated with the serverside Network Controller or if the Network Controller is not connected.
        /// </remarks>
        public void SetReady()
        {
            if (!GetLocalConnection().Authenticated) return;
            
            ReadyMessage readyMessage = new ReadyMessage();
            _netClient.connection.Send(MsgType.Ready, readyMessage);
            _ready = true;
            _netClient.connection.isReady = true;
            
            EventArguments args = new EventArguments();
            args.SetVariable("connection", _netClient.connection.connectionId);
            DuneClient.EventController.InvokeEvent("OnClientReady", args);
        }

        /// <summary>
        /// Sets the Network Controller not ready.
        /// </summary>
        /// <remarks>
        /// This instructs the receiving serverside Network Controller that this Network Controller is not ready to receive scene information (useful for changing scenes).
        /// This method has no effect until the Network Controller has been authenticated with the serverside Network Controller or if the Network Controller is not connected.
        /// </remarks>
        public void SetNotReady()
        {
            if (!GetLocalConnection().Authenticated) return;
            
            NotReadyMessage notReadyMessage = new NotReadyMessage();
            _netClient.connection.Send(MsgType.NotReady, notReadyMessage);
            _ready = false;
            _netClient.connection.isReady = false;
            
            EventArguments args = new EventArguments();
            args.SetVariable("connection", _netClient.connection.connectionId);
            DuneClient.EventController.InvokeEvent("OnClientNotReady", args);
        }

        /// <summary>
        /// Returns whether the Network Controller is ready.
        /// </summary>
        /// <returns>True if the Network Controller is ready, False otherwise.</returns>
        public bool IsReady()
        {
            return _ready;
        }
        
        /// <summary>
        /// Returns the local connection of the Network Controller.
        /// </summary>
        /// <returns>A DuneConnection instance representing the connection used by the Network Controller to communicate with a server.</returns>
        public DuneConnection GetLocalConnection()
        {
            return (DuneConnection) _netClient.connection;
        }

        /// <summary>
        /// Calculates the delay of the provided timestamp for the current connection.
        /// </summary>
        /// <remarks>
        /// This is useful for time sensitive operations between the client and the server such as movement synchronization and some types of authentication.
        /// </remarks>
        /// <param name="remoteTime">Timestamp received from the server.</param>
        /// <param name="error">A byte representing any error during the delay calculation (can be casted to NetworkError for more information). Null if no errors occurred.</param>
        /// <returns>The delay between the client and the server.</returns>
        public int GetRemoteDelayTimeMs(int remoteTime, out byte error)
        {
            return NetworkTransport.GetRemoteDelayTimeMS(GetLocalConnection().hostId, GetLocalConnection().connectionId, remoteTime, out error);
        }

        /// <summary>
        /// Registers a network message handler.
        /// </summary>
        /// <param name="msgType">The type of the message to register. This must be unique for each message type.</param>
        /// <param name="handler">The handler that the Network Controller will invoke when a message of this type is received.</param>
        public void RegisterHandler(short msgType, NetworkMessageDelegate handler)
        {
            _netClient.RegisterHandler(msgType, handler);
        }

        /// <summary>
        /// Sends a message to the server using the provided channel.
        /// </summary>
        /// <remarks>
        /// The channel must have been previously been manually registered or be one of the default channels from MessageChannels.
        /// </remarks>
        /// <param name="msgType">The type of the message to send.</param>
        /// <param name="msg">The message to send.</param>
        /// <param name="channel">The channel to send the message through.</param>
        public void SendByChannelToServer(short msgType, MessageBase msg, short channel)
        {
            _netClient.connection.SendByChannel(msgType, msg, channel);
        }

        /// <summary>
        /// Disposes the Network Controller and frees any resources internally used by it.
        /// </summary>
        /// <remarks>
        /// There should be no need to manually call this in most cases as it is already automatically called on application quit.
        /// </remarks>
        public void Dispose()
        {
            Shutdown();
            _netClient = null;
        }
        
#endregion
    }
}