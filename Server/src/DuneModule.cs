using System.Collections;
using DuneNet.Shared;
using DuneNet.Shared.Enums;
using DuneNet.Shared.Events;
using DuneNet.Shared.Modules;
using UnityEngine;
using UnityEngine.Networking;

namespace DuneNet.Server
{
    /// <inheritdoc />
    /// <summary>
    /// A server module to easily hook into DuneNet's server functionality.
    /// </summary>
    /// <remarks>
    /// This is the preferable way to extend DuneNet's functionality.
    /// </remarks>
    public abstract class DuneModule : DuneBehaviour
    {
#region internal
        
        internal DuneModule Next;

        internal void OnUseInternal()
        {
            OnUse();
        }

        internal void OnStopUsingInternal()
        {
            OnStopUsing();
        }

        [EventReg("DuneQuit", NetworkContext.Server)]
        internal void OnQuitInternal(EventArguments args)
        {
            OnStopUsing();
        }

        [EventReg("DuneUpdate", NetworkContext.Server)]
        internal void OnUpdateInternal(EventArguments args)
        {
            OnUpdate();
        }
        
        internal HandshakeResponse RespondHandshake(string idToken, byte[] secret)
        {
            return Next?.OnRespondHandshakeInternal(OnRespondHandshake(new HandshakeResponse(), idToken, secret), idToken, secret) ?? OnRespondHandshake(new HandshakeResponse(), idToken, secret);
        }

        private HandshakeResponse OnRespondHandshakeInternal(HandshakeResponse previous, string idToken, byte[] secret)
        {
            return Next?.OnRespondHandshakeInternal(OnRespondHandshake(previous, idToken, secret), idToken, secret) ?? OnRespondHandshake(previous, idToken, secret);
        }
        
        [EventReg("OnServerAuthenticationSuccess", NetworkContext.Server)]
        protected void OnHandshakeSuccessfulInternal(EventArguments args)
        {
            OnHandshakeSuccessful(DuneServer.NetworkController.GetConnectionFromId(args.GetVariable<int>("connection")));
        }
        
        [EventReg("OnServerAuthenticationError", NetworkContext.Server)]
        protected void OnHandshakeErrorInternal(EventArguments args)
        {
            OnHandshakeError(DuneServer.NetworkController.GetConnectionFromId(args.GetVariable<int>("connection")));
        }
        
        [EventReg("OnServerConnect", NetworkContext.Server)]
        protected void OnNetConnectedInternal(EventArguments args)
        {
            OnNetConnected(DuneServer.NetworkController.GetConnectionFromId(args.GetVariable<int>("connection")));
        }
        
        [EventReg("OnServerDisconnect", NetworkContext.Server)]
        protected void OnNetDisconnectedInternal(EventArguments args)
        {
            int connectionId = args.GetVariable<int>("connection");
            DuneConnection conn = DuneServer.NetworkController.GetConnectionFromId(connectionId) ?? new DuneConnection {connectionId = connectionId, isReady = false};
            OnNetDisconnected(conn);
        }
        
        [EventReg("OnServerError", NetworkContext.Server)]
        protected void OnNetErrorInternal(EventArguments args)
        {
            int connectionId = args.GetVariable<int>("connection");
            DuneConnection conn = DuneServer.NetworkController.GetConnectionFromId(connectionId) ?? new DuneConnection {connectionId = connectionId, isReady = false};
            OnNetError(conn, args.GetVariable<NetworkError>("networkError"));
        }
        
        [EventReg("OnServerReady", NetworkContext.Server)]
        protected void OnNetReadyInternal(EventArguments args)
        {
            OnNetReady(DuneServer.NetworkController.GetConnectionFromId(args.GetVariable<int>("connection")));
        }
        
        [EventReg("OnServerNotReady", NetworkContext.Server)]
        protected void OnNetNotReadyInternal(EventArguments args)
        {
            OnNetNotReady(DuneServer.NetworkController.GetConnectionFromId(args.GetVariable<int>("connection")));
        }
        
#endregion
#region public

        /// <summary>
        /// Overridable method called when the module is added to the module chain.
        /// </summary>
        protected virtual void OnUse()
        {
        }
        
        /// <summary>
        /// Overridable method called when the module is removed from the module chain.
        /// </summary>
        protected virtual void OnStopUsing()
        {
        }

        /// <summary>
        /// Overridable method called every frame.
        /// </summary>
        protected virtual void OnUpdate()
        {
        }
        
        /// <summary>
        /// Overridable method called when the server is expected to respond to a client's authentication request.
        /// </summary>
        /// <remarks>
        /// Since modules are chained together, multiple modules can be used to generate the final HandshakeResponse by modifying the previous HandshakeResponse and returning it. 
        /// </remarks>
        /// <param name="previous">The HandshakeResponse received from the previous module. This is never null but it may contain empty elements if this is the first module of the chain.</param>
        /// <param name="idToken">The authentication token received from the client.</param>
        /// <param name="secret">The authentication secret received from the client.</param>
        /// <returns>A HandshakeResponse instance to be passed onto the next module of the chain.</returns>
        protected virtual HandshakeResponse OnRespondHandshake(HandshakeResponse previous, string idToken, byte[] secret)
        {
            return previous;
        }
        
        /// <summary>
        /// Overridable method called when the server accepted the authentication request of a client.
        /// </summary>
        /// <param name="conn">The connection of the authenticated client.</param>
        protected virtual void OnHandshakeSuccessful(DuneConnection conn)
        {
        }

        /// <summary>
        /// Overridable method called when the server rejected the authentication request of a client.
        /// </summary>
        /// <param name="conn">The connection of the rejected client.</param>
        protected virtual void OnHandshakeError(DuneConnection conn)
        {
        }
        
        /// <summary>
        /// Overridable method called when a client connects to the NetworkController.
        /// </summary>
        /// <param name="conn">The connection that got connected.</param>
        protected virtual void OnNetConnected(DuneConnection conn)
        {
        }

        /// <summary>
        /// Overridable method called when a client disconnects from the NetworkController.
        /// </summary>
        /// <param name="conn">The connection that got disconnected.</param>
        protected virtual void OnNetDisconnected(DuneConnection conn)
        {
        }

        /// <summary>
        /// Overridable method called when the NetworkController experiences a connection error.
        /// </summary>
        /// <param name="conn">The connection the error occurred in.</param>
        /// <param name="error">The connection error.</param>
        protected virtual void OnNetError(DuneConnection conn, NetworkError error)
        {
        }

        /// <summary>
        /// Overridable method called when a client sets itself ready.
        /// </summary>
        /// <param name="conn">The connection that was set ready.</param>
        protected virtual void OnNetReady(DuneConnection conn)
        {
        }

        /// <summary>
        /// Overridable method called when a client sets itself not ready.
        /// </summary>
        /// <param name="conn">The connection that was set not ready.</param>
        protected virtual void OnNetNotReady(DuneConnection conn)
        {
        }
        
        /// <summary>
        /// Starts a coroutine
        /// </summary>
        /// <param name="routine"></param>
        protected Coroutine StartCoroutine(IEnumerator routine)
        {
            return DuneUpdater.Instance.StartCoroutine(routine);
        }
        
#endregion
    }
}