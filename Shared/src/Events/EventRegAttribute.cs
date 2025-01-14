using System;
using DuneNet.Shared.Enums;

namespace DuneNet.Shared.Events
{
    /// <inheritdoc />
    /// <summary>
    /// An Attribute used to describe events. This should be used to register new events by adding attaching it to an event's handler method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class EventRegAttribute : Attribute
    {
        /// <summary>
        /// The name of the event.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// The network context the event will be registered on.
        /// </summary>
        /// <remarks>
        /// Server events are not registered on the client and vice versa.
        /// </remarks>
        public NetworkContext Context { get; }
        
        /// <summary>
        /// Whether the event should be invoked over the network. 
        /// </summary>
        /// <remarks>
        /// If true, the event will be invoked on the clients when it is invoked on the client with invokeOnClient set to ture and on the server when it is invoked on a client with invokeOnServer set to true.
        /// Otherwise, the event is only invoked on the network context it was registered on.
        /// </remarks>
        public bool NetworkInvoked { get; }

        /// <inheritdoc />
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the event.</param>
        /// <param name="context">The network context the event will be registered on.</param>
        /// <param name="networkInvoked">Whether the event should be invoked over the network.</param>
        public EventRegAttribute(string name, NetworkContext context, bool networkInvoked = false)
        {
            Name = name;
            Context = context;
            NetworkInvoked = networkInvoked;
        }
    }
}