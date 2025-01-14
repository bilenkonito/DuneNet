using DuneNet.Shared;
using DuneNet.Shared.Controllers;
using DuneNet.Shared.Enums;
using DuneNet.Shared.Events;
using DuneNet.Shared.Messages;

namespace DuneNet.Server.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// An Event Controller with extra functionality for the server.
    /// </summary>
    public sealed class EventController : BaseEventController
    {
#region internal
        
        internal EventController(NetworkContext networkContext) : base(networkContext)
        {
        }
        
        internal void Init()
        {
            DuneServer.NetworkController.RegisterHandler(MsgTypes.InvokeEvent, OnEventNetworkInvokeInternal);
        }
        
#endregion
#region public

        /// <summary>
        /// Invokes the target serverside event. 
        /// </summary>
        /// <param name="eventName">The registration name of the event to invoke.</param>
        /// <param name="eventArguments">The arguments the event will be invoked with. Must not be null but can contain no set variables.</param>
        /// <param name="invokeOnClient">Whether the event should be networked. True if the event should be called on clients, False otherwise.</param>
        public void InvokeEvent(string eventName, EventArguments eventArguments, bool invokeOnClient = false)
        {
            InvokeEventInternal(eventName, eventArguments);
            if (invokeOnClient)
            {
                DuneServer.NetworkController.SendByChannelToReady(MsgTypes.InvokeEvent, new SharedInvokeEvent(eventName, eventArguments), MessageChannels.GeneralReliableSequenced);
            }
        }

        /// <summary>
        /// Invokes the target clientside event on the provided client.
        /// </summary>
        /// <remarks>
        /// This method does NOT invoke the target event on the server. This method, therefore, works similar to a global scope RPC.
        /// </remarks>
        /// <param name="eventName">The registration name of the event to invoke.</param>
        /// <param name="eventArguments">The arguments the event will be invoked with. Must not be null but can contain no set variables.</param>
        /// <param name="targetClientConnection">The connection to invoke the event on.</param>
        public void InvokeEvent(string eventName, EventArguments eventArguments, DuneConnection targetClientConnection)
        {
            if (targetClientConnection.Authenticated)
            {
                targetClientConnection.SendByChannel(MsgTypes.InvokeEvent, new SharedInvokeEvent(eventName, eventArguments), MessageChannels.GeneralReliableSequenced);
            }
        }
        
#endregion
    }
}