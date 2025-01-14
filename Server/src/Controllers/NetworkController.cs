using System;
using System.Linq;
using DuneNet.Shared;
using DuneNet.Shared.Events;
using DuneNet.Shared.Messages;
using DuneNet.Shared.Modules;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace DuneNet.Server.Controllers
{
    /// <summary>
    /// A Network Controller with extra functionality for the server.
    /// </summary>
    public sealed class NetworkController
    {
#region internal
        
        private NetworkServerSimple _netServer;
        private float _maxDelay;
        private bool _netActive;
        
        internal NetworkController()
        {
            Application.runInBackground = true;
            _netServer = new NetworkServerSimple();
            _netServer.SetNetworkConnectionClass<DuneConnection>();
        }

        internal void Update()
        {
            if (IsListening())
            {
                _netServer.Update();
            }
            else
            {
                _netServer.UpdateConnections();
            }
        }
        
        private void RegisterNetworkHandlers()
        {
            _netServer.RegisterHandler(MsgType.Connect, OnConnectInternal);
            _netServer.RegisterHandler(MsgType.Disconnect, OnDisconnectInternal);
            _netServer.RegisterHandler(MsgType.NotReady, OnNotReadyMessageInternal);
            _netServer.RegisterHandler(MsgType.Ready, OnReadyMessageInternal);
            _netServer.RegisterHandler(MsgType.Error, OnErrorInternal);
            _netServer.RegisterHandler(MsgTypes.RequestHandshake, OnReceiveHandshakeInternal);
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
            msg.conn.SetMaxDelay(_maxDelay);
            EventArguments args = new EventArguments();
            args.SetVariable("connection", msg.conn.connectionId);
            DuneServer.EventController.InvokeEvent("OnServerConnect", args);
        }
        
        private void OnDisconnectInternal(NetworkMessage msg)
        {
            EventArguments args = new EventArguments();
            args.SetVariable("connection", msg.conn.connectionId);
            DuneServer.EventController.InvokeEvent("OnServerDisconnect", args);
        }
        
        private void OnReadyMessageInternal(NetworkMessage msg)
        {
            if (msg.conn.isReady || !((DuneConnection)msg.conn).Authenticated || !((DuneConnection)msg.conn).LocalReadiness) return;
            
            msg.conn.isReady = true;
            EventArguments args = new EventArguments();
            args.SetVariable("connection", msg.conn.connectionId);
            DuneServer.EventController.InvokeEvent("OnServerReady", args);
        }
        
        private void OnNotReadyMessageInternal(NetworkMessage msg)
        {
            if (!msg.conn.isReady || !((DuneConnection)msg.conn).Authenticated || !((DuneConnection)msg.conn).LocalReadiness) return;
            
            msg.conn.isReady = false;
            EventArguments args = new EventArguments();
            args.SetVariable("connection", msg.conn.connectionId);
            DuneServer.EventController.InvokeEvent("OnServerNotReady", args);
        }
        
        private void OnErrorInternal(NetworkMessage msg)
        {
            ErrorMessage errorMsg = msg.ReadMessage<ErrorMessage>();
            EventArguments args = new EventArguments();
            args.SetVariable("connection", msg.conn.connectionId);
            args.SetVariable("networkError", (NetworkError) errorMsg.errorCode);
            DuneServer.EventController.InvokeEvent("OnServerError", args, false);
        }

        private void OnReceiveHandshakeInternal(NetworkMessage msg)
        {
            SharedHandshakeRequest req = msg.ReadMessage<SharedHandshakeRequest>();
            DuneConnection conn = GetConnectionFromId(msg.conn.connectionId);
            conn.IDToken = req.IDToken;
            
            HandshakeResponse handshakeResponse = null;
            DuneServer.InvokeOnModules(x => handshakeResponse = x.RespondHandshake(req.IDToken, req.Secret));
            
            EventArguments args = new EventArguments();
            args.SetVariable("connection", conn.connectionId);

            if (handshakeResponse != null)
            {
                SharedHandshakeResponse resMsg = new SharedHandshakeResponse(req.IDToken, handshakeResponse);
                conn.SendByChannel(MsgTypes.RespondHandshake, resMsg, MessageChannels.GeneralReliableSequenced);

                if (resMsg.Allowed)
                {
                    conn.Authenticated = true;
                    conn.AuthenticationToken = handshakeResponse.AuthenticationToken;
                    DuneServer.EventController.InvokeEvent("OnServerAuthenticationSuccess", args);
                }
                else
                {
                    KickConnection(conn);
                    DuneServer.EventController.InvokeEvent("OnServerAuthenticationError", args);
                }
            }
            else
            {
                SharedHandshakeResponse resMsg = new SharedHandshakeResponse(req.IDToken, true, null);
                conn.SendByChannel(MsgTypes.RespondHandshake, resMsg, MessageChannels.GeneralReliableSequenced);
                conn.Authenticated = true;
                conn.AuthenticationToken = "";
                DuneServer.EventController.InvokeEvent("OnServerAuthenticationSuccess", args);
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
        /// <param name="maxConnections">The maximum amount of connections the Network Controller will handle at the same time.
        /// One connection is required per client so this normally equals the maximum amount of clients the Network Controller will be able to handle at the same time.
        /// This value should be higher than the amount of expected clients and the maximum amount of clients manually restricted with a DuneModule or by listening for the OnServerConnect event.</param>
        /// <param name="maxDelay">The maximum delay before sending packets on connections. Setting this to zero will send messages as soon as they are ready instead of aggregating them.</param>
        public void Init(GlobalConfig globalConfig, ConnectionConfig connectionConfig, int maxConnections = 1, float maxDelay = 0.1f)
        {
            _maxDelay = maxDelay;
            NetworkCRC.scriptCRCCheck = false;
            NetworkTransport.Init(globalConfig);
            _netServer.Configure(RegisterBaseChannels(connectionConfig), maxConnections);
            RegisterNetworkHandlers();
            DuneServer.EntityController.Init();
            DuneServer.EventController.Init();
        }

        /// <summary>
        /// Listens for connections on the supplied endpoint.
        /// </summary>
        /// <remarks>
        /// After listening, the network interactions can be hooked into by using a DuneModule or by listening for the following events:
        /// <list type="bullet">
        ///     <item>OnServerConnect. Event Variables:
        ///         <list type="bullet"> 
        ///             <item>(int) connection = The ID of the connection that invoked the event.</item> 
        ///         </list>
        ///     </item>
        ///     <item>OnServerDisconnect. Event Variables:
        ///         <list type="bullet"> 
        ///             <item>(int) connection = The ID of the connection that invoked the event.</item> 
        ///         </list>
        ///     </item>
        ///     <item>OnServerError. Event Variables:
        ///         <list type="bullet">
        ///             <item>(int) connection = The ID of the connection that invoked the event.</item> 
        ///             <item>(NetworkError) networkError = The error code.</item> 
        ///         </list>
        ///     </item>
        ///     <item>OnServerAuthenticationSuccess. Event Variables:
        ///         <list type="bullet"> 
        ///             <item>(int) connection = The ID of the connection that invoked the event.</item> 
        ///         </list>
        ///     </item>
        ///     <item>OnServerAuthenticationError. Event Variables:
        ///         <list type="bullet"> 
        ///             <item>(int) connection = The ID of the connection that invoked the event.</item> 
        ///         </list>
        ///     </item>
        /// </list>
        /// </remarks>
        /// <param name="ip">The IP Network Controller should listen on.</param>
        /// <param name="port">The port the Network Controller should listen on.</param>
        /// <returns>
        /// True if the Network Controller was able to start listening at the provided endpoint, 
        /// False otherwise (for example, if the IP is not valid or the endpoint is already in use by another process).
        /// </returns>
        public bool Listen(string ip, int port)
        {
            if (IsListening()) return false;
            return _netActive = _netServer.Listen(ip, port);
        }

        /// <summary>
        /// Stops listening.
        /// </summary>
        /// <returns>
        /// True if the Network Controller was able to stop listening, 
        /// False otherwise (for example, if the server was not listening).
        /// </returns>
        public bool Disconnect()
        {
            if (!IsListening()) return _netActive = false;
            _netServer.DisconnectAllConnections();
            _netServer.Stop();
            return _netActive = false;
        }
        
        /// <summary>
        /// Shuts down the Network Controller. Init must be called again to perform further operations with the Network Controller after calling this.
        /// </summary>
        /// <remarks>
        /// There should be no need to manually call this in most cases as it is already automatically called on application quit.
        /// </remarks>
        public void Shutdown()
        {
            if (!_netActive) return;
            Disconnect();
        }
        
        /// <summary>
        /// Returns whether the Network Controller is currently listening.
        /// </summary>
        /// <returns>True if the Network Controller is listening, False otherwise.</returns>
        public bool IsListening()
        {
            return _netServer.serverHostId != -1 && _netActive;
        }

        /// <summary>
        /// Returns the port the Network Controller is currently listening on.
        /// </summary>
        /// <returns>The port the server is listening on if the server is listening, -1 otherwise</returns>
        public int GetListenPort()
        {
            return _netActive ? _netServer.listenPort : -1;
        }
        
        /// <summary>
        /// Sets the provided connection ready.
        /// </summary>
        /// <remarks>
        /// This instructs the receiving clientside Network Controller that it should be able to receive scene information.
        /// This method has no effect until the client has been authenticated.
        /// </remarks>
        /// <param name="conn">The connection to set ready.</param>
        public void SetReady(DuneConnection conn)
        {
            if (!conn.Authenticated || conn.isReady) return;
            
            ReadyMessage readyMessage = new ReadyMessage();
            conn.Send(MsgType.Ready, readyMessage);
            conn.isReady = true;
            
            EventArguments args = new EventArguments();
            args.SetVariable("connection", conn.connectionId);
            DuneServer.EventController.InvokeEvent("OnServerReady", args);
        }

        /// <summary>
        /// Sets the provided connection not ready.
        /// </summary>
        /// <remarks>
        /// This instructs the receiving clientside Network Controller that it should not be able to receive scene information (useful for changing scenes).
        /// This method has no effect until the client has been authenticated.
        /// </remarks>
        /// <param name="conn">The connection to set not ready.</param>
        public void SetNotReady(DuneConnection conn)
        {
            if (!conn.Authenticated || !conn.isReady) return;
            
            NotReadyMessage notReadyMessage = new NotReadyMessage();
            conn.Send(MsgType.NotReady, notReadyMessage);
            conn.isReady = false;
            
            EventArguments args = new EventArguments();
            args.SetVariable("connection", conn.connectionId);
            DuneServer.EventController.InvokeEvent("OnServerNotReady", args);
        }

        /// <summary>
        /// Returns whether the provided connection is ready.
        /// </summary>
        /// <returns>True if the provided connection is ready, False otherwise.</returns>
        public bool IsReady(DuneConnection conn)
        {
            return conn.isReady;
        }

        /// <summary>
        /// Prevents the provided connection from controlling its own readiness.
        /// </summary>
        /// <remarks>
        /// This is useful for preventing clients from setting theirselves ready and unready when they are not supposed to.
        /// </remarks>
        /// <param name="conn">The connection to force readiness on.</param>
        public void ForceServerReadiness(DuneConnection conn)
        {
            if (conn.LocalReadiness)
            {
                conn.LocalReadiness = false;
            }
        }

        /// <summary>
        /// Allows the provided connection to control its own readiness.
        /// </summary>
        /// <remarks>
        /// This is useful to allow clients to set theirselves ready and unready, for example, to indicate the loading state of a scene change.
        /// </remarks>
        /// <param name="conn">The connection to allow local readiness control on.</param>
        public void AllowLocalReadiness(DuneConnection conn)
        {
            if (!conn.LocalReadiness)
            {
                conn.LocalReadiness = true;
            }
        }

        public int GetConnectionAmount()
        {
            return IsListening() ? _netServer.connections.Count(x => x != null) : 0;
        }

        /// <summary>
        /// Returns a connection identified by the provided ID.
        /// </summary>
        /// <param name="connectionId">The ID of the connection to retrieve</param>
        /// <returns>A DuneConnection instance representing the requested connection.</returns>
        public DuneConnection GetConnectionFromId(int connectionId)
        {
            return _netServer.connections.Where(conn => conn != null && conn.connectionId == connectionId).Cast<DuneConnection>().FirstOrDefault();
        }

        /// <summary>
        /// Kicks a connection.
        /// </summary>
        /// <remarks>
        /// This forcefuly disconnects the provided connection after one second. This delay allows for graceful disconnection messages to be sent before the connetion is closed.
        /// </remarks>
        /// <param name="conn">The connection to kick.</param>
        public void KickConnection(NetworkConnection conn)
        {
            DuneUpdater.Instance.WaitForSecondsAction(1f, conn.Disconnect);
        }

        /// <summary>
        /// Registers a network message handler.
        /// </summary>
        /// <param name="msgType">The type of the message to register. This must be unique for each message type.</param>
        /// <param name="handler">The handler that the Network Controller will invoke when a message of this type is received.</param>
        public void RegisterHandler(short msgType, NetworkMessageDelegate handler)
        {
            _netServer.RegisterHandler(msgType, handler);
        }

        /// <summary>
        /// Sends a message to all ready clients using the provided channel.
        /// </summary>
        /// <remarks>
        /// The channel must have been previously been manually registered or be one of the default channels from MessageChannels.
        /// </remarks>
        /// <param name="msgType">The type of the message to send.</param>
        /// <param name="msg">The message to send.</param>
        /// <param name="channel">The channel to send the message through.</param>
        public void SendByChannelToReady(short msgType, MessageBase msg, short channel)
        {
            foreach (NetworkConnection connection in _netServer.connections)
            {
                if (connection == null || !connection.isConnected || !connection.isReady) continue;
                
                connection.SendByChannel(msgType, msg, channel);
            }
        }

        /// <summary>
        /// Sends a message to all ready clients using the provided channel. Additionally, it sets the authority of the message to the provided connection before sending it.
        /// </summary>
        /// <remarks>
        /// The channel must have been previously been manually registered or be one of the default channels from MessageChannels.
        /// This method is specially useful for custom messages that deal with authority entities.
        /// </remarks>
        /// <param name="msgType">The type of the message to send.</param>
        /// <param name="authoritativeConnection">The connection that will have authority over the message.</param>
        /// <param name="msg">The message to send.</param>
        /// <param name="channel">The channel to send the message through.</param>
        public void SendByChannelToReadyWithAuthority(short msgType, NetworkConnection authoritativeConnection, SharedAuthorityMessage msg, short channel)
        {
            foreach (NetworkConnection connection in _netServer.connections)
            {
                if (connection == null || !connection.isConnected || !connection.isReady) continue;
                
                msg.HasAuthority = authoritativeConnection != null && connection.connectionId == authoritativeConnection.connectionId;
                connection.SendByChannel(msgType, msg, channel);
            }
        }

        /// <summary>
        /// Performs the provided action for each valid connection.
        /// </summary>
        /// <remarks>
        /// This method skips the default server connection, which is always equal to null.
        /// </remarks>
        /// <param name="action">The action to perform for each connection.</param>
        public void ForEachConnection(Action<DuneConnection> action)
        {
            if (!IsListening()) return;

            foreach (NetworkConnection conn in _netServer.connections.Where(x => x != null))
            {
                action((DuneConnection)conn);
            }
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
            _netServer = null;
        }
        
#endregion
    }
}