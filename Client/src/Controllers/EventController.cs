using DuneNet.Shared.Controllers;
using DuneNet.Shared.Enums;
using DuneNet.Shared.Events;
using DuneNet.Shared.Messages;
using JetBrains.Annotations;
using UnityEngine.Networking;

namespace DuneNet.Client.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// An Event Controller with extra functionality for the client.
    /// </summary>
    public sealed class EventController : BaseEventController
    {
#region internal

        internal EventController(NetworkContext networkContext) : base(networkContext)
        {
        }

        internal void Init()
        {
            DuneClient.NetworkController.RegisterHandler(MsgTypes.InvokeEvent, OnEventNetworkInvokeInternal);
        }
        
#endregion
#region public

        /// <summary>
        /// Invokes the target clientside event. 
        /// </summary>
        /// <param name="eventName">The registration name of the event to invoke.</param>
        /// <param name="eventArguments">The arguments the event will be invoked with. Must not be null but can contain no set variables.</param>
        /// <param name="invokeOnServer">Whether the event should be networked. True if the event should be called on the server, False otherwise.</param>
        public void InvokeEvent(string eventName, [NotNull] EventArguments eventArguments, bool invokeOnServer = false)
        {
            InvokeEventInternal(eventName, eventArguments);
            if (invokeOnServer)
            {
                DuneClient.NetworkController.SendByChannelToServer(MsgTypes.InvokeEvent, new SharedInvokeEvent(eventName, eventArguments), MessageChannels.GeneralReliableSequenced);
            }
        }
        
#endregion
    }
}