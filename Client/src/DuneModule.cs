using System.Collections;
using DuneNet.Shared;
using DuneNet.Shared.Enums;
using DuneNet.Shared.Events;
using DuneNet.Shared.Modules;
using UnityEngine;
using UnityEngine.Networking;

namespace DuneNet.Client
{
    /// <inheritdoc />
    /// <summary>
    /// A client module to easily hook into DuneNet's client functionality.
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

        [EventReg("DuneQuit", NetworkContext.Client)]
        internal void OnQuitInternal(EventArguments args)
        {
            OnStopUsing();
        }

        [EventReg("DuneUpdate", NetworkContext.Client)]
        internal void OnUpdateInternal(EventArguments args)
        {
            OnUpdate();
        }
        
        internal HandshakeRequest SendHandshake()
        {
            return Next?.OnSendHandshakeInternal(OnSendHandshake(new HandshakeRequest())) ?? OnSendHandshake(new HandshakeRequest());
        }

        private HandshakeRequest OnSendHandshakeInternal(HandshakeRequest previous)
        {
            return Next?.OnSendHandshakeInternal(OnSendHandshake(previous)) ?? OnSendHandshake(previous);
        }
        
        [EventReg("OnClientAuthenticationSuccess", NetworkContext.Client)]
        protected void OnHandshakeSuccessfulInternal(EventArguments args)
        {
            OnHandshakeSuccessful(DuneClient.NetworkController.GetLocalConnection());
        }
        
        [EventReg("OnClientAuthenticationError", NetworkContext.Client)]
        protected void OnHandshakeErrorInternal(EventArguments args)
        {
            OnHandshakeError(DuneClient.NetworkController.GetLocalConnection(), args.GetVariable<string>("error"));
        }
        
        [EventReg("OnClientConnect", NetworkContext.Client)]
        protected void OnNetConnectedInternal(EventArguments args)
        {
            OnNetConnected(DuneClient.NetworkController.GetLocalConnection());
        }
        
        [EventReg("OnClientDisconnect", NetworkContext.Client)]
        protected void OnNetDisconnectedInternal(EventArguments args)
        {
            OnNetDisconnected(DuneClient.NetworkController.GetLocalConnection());
        }
        
        [EventReg("OnClientErrorTimeout", NetworkContext.Client)]
        protected void OnNetTimeoutInternal(EventArguments args)
        {
            OnNetTimeout(DuneClient.NetworkController.GetLocalConnection());
        }
        
        [EventReg("OnClientErrorBadMessage", NetworkContext.Client)]
        protected void OnNetBadMessageInternal(EventArguments args)
        {
            OnNetBadMessage(DuneClient.NetworkController.GetLocalConnection());
        }
        
        [EventReg("OnClientErrorDNSFailure", NetworkContext.Client)]
        protected void OnNetDnsFailureInternal(EventArguments args)
        {
            OnNetDnsFailure(DuneClient.NetworkController.GetLocalConnection());
        }
        
        [EventReg("OnClientErrorGeneric", NetworkContext.Client)]
        protected void OnNetErrorUnknownInternal(EventArguments args)
        {
            OnNetErrorUnknown(DuneClient.NetworkController.GetLocalConnection(), args.GetVariable<NetworkError>("networkError"));
        }
        
        [EventReg("OnClientReady", NetworkContext.Client)]
        protected void OnNetReadyInternal(EventArguments args)
        {
            OnNetReady(DuneClient.NetworkController.GetLocalConnection());
        }
        
        [EventReg("OnClientNotReady", NetworkContext.Client)]
        protected void OnNetNotReadyInternal(EventArguments args)
        {
            OnNetNotReady(DuneClient.NetworkController.GetLocalConnection());
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
        /// Overridable method called when the client is expected to send its authentication request to the server.
        /// </summary>
        /// <remarks>
        /// Since modules are chained together, multiple modules can be used to generate the final HandshakeRequest by modifying the previous HandshakeRequest and returning it. 
        /// </remarks>
        /// <param name="previous">The HandshakeRequest received from the previous module. This is never null but it may contain empty elements if this is the first module of the chain.</param>
        /// <returns>A HandshakeRequest instance to be passed onto the next module of the chain.</returns>
        protected virtual HandshakeRequest OnSendHandshake(HandshakeRequest previous)
        {
            return previous;
        }

        /// <summary>
        /// Overridable method called when the server accepted the authentication request.
        /// </summary>
        /// <param name="conn">The connection that performed the authentication.</param>
        protected virtual void OnHandshakeSuccessful(DuneConnection conn)
        {
        }

        /// <summary>
        /// Overridable method called when the server rejects the authentication request.
        /// </summary>
        /// <param name="conn">The connection that performed the authentication.</param>
        /// <param name="error">The descriptive error the server provided as a reason for rejecting the authentication.</param>
        protected virtual void OnHandshakeError(DuneConnection conn, string error)
        {
        }

        /// <summary>
        /// Overridable method called when the NetworkController connects to a server.
        /// </summary>
        /// <param name="conn">The connection that got connected.</param>
        protected virtual void OnNetConnected(DuneConnection conn)
        {
        }

        /// <summary>
        /// Overridable method called when the NetworkController disconnects from a server.
        /// </summary>
        /// <param name="conn">The connection that got disconnected.</param>
        protected virtual void OnNetDisconnected(DuneConnection conn)
        {
        }
        
        /// <summary>
        /// Overridable method called when the NetworkController's connection to a server times out.
        /// </summary>
        /// <param name="conn">The connection the error occurred in.</param>
        protected virtual void OnNetTimeout(DuneConnection conn)
        {
        }

        /// <summary>
        /// Overridable method called when the NetworkController gets disconnected from the server because a message was invalid.
        /// </summary>
        /// <param name="conn">The connection the error occurred in.</param>
        protected virtual void OnNetBadMessage(DuneConnection conn)
        {
        }
        
        /// <summary>
        /// Overridable method called when the NetworkController cannot resolve the FQDN provided for the connection.
        /// </summary>
        /// <param name="conn">The connection the error occurred in.</param>
        protected virtual void OnNetDnsFailure(DuneConnection conn)
        {
        }
        
        /// <summary>
        /// Overridable method called when the NetworkController experiences an unknown connection error.
        /// </summary>
        /// <param name="conn">The connection the error occurred in.</param>
        /// <param name="error">The connection error.</param>
        protected virtual void OnNetErrorUnknown(DuneConnection conn, NetworkError error)
        {
        }
        
        /// <summary>
        /// Overridable method called when the NetworkController is set ready by the server.
        /// </summary>
        /// <param name="conn">The connection that was set ready.</param>
        protected virtual void OnNetReady(DuneConnection conn)
        {
        }

        /// <summary>
        /// Overridable method called when the NetworkController is set not ready by the server.
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