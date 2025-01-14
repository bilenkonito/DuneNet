using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DuneNet.Shared.Enums;
using DuneNet.Shared.Events;
using DuneNet.Shared.Messages;
using UnityEngine.Networking;
using Event = DuneNet.Shared.Events.Event;

namespace DuneNet.Shared.Controllers
{
    /// <summary>
    /// A Base Event Controller.
    /// </summary>
    public abstract class BaseEventController
    {   
#region internal
        
        private Dictionary<string, Event> _registeredEvents;
        private readonly NetworkContext _networkContext;
        
        protected BaseEventController(NetworkContext networkContext)
        {
            _registeredEvents = new Dictionary<string, Event>();
            _networkContext = networkContext;
            RegisterStaticEvents();
        }

        private void RegisterStaticEvents()
        {
            var eventTypeAttributes =
                from a in AppDomain.CurrentDomain.GetAssemblies().Where(x => x != null)
                from t in a.GetTypes().AsParallel()
                from m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                let attributes = m.GetCustomAttributes(true)
                where attributes.OfType<EventRegAttribute>().Any()
                select new {Method = m, Attributes = attributes.Cast<EventRegAttribute>()};
            
            foreach (var kv in eventTypeAttributes)
            {
                EventRegAttribute regAttribute = kv.Attributes.DefaultIfEmpty(null).FirstOrDefault();
                if (regAttribute == null) continue;
                
                NetworkContext context = regAttribute.Context;
                MethodInfo method = kv.Method;
                
                if (!string.IsNullOrEmpty(regAttribute.Name) && context == _networkContext && method.ReturnType == typeof(void) 
                                 && !method.ContainsGenericParameters 
                                 && method.GetParameters().Any(x => x.ParameterType == typeof(EventArguments)))
                {
                    if (!_registeredEvents.ContainsKey(regAttribute.Name))
                    {
                        RegisterEventInternal(regAttribute.Name);
                    }
                    
                    SubscribeEventInternal(regAttribute.Name, (EventDelegate) method.CreateDelegate(typeof(EventDelegate)), regAttribute.NetworkInvoked);
                }
            }
        }

        private void UnregisterAllEventsInternal()
        {
            foreach (var kv in _registeredEvents)
            {
                _registeredEvents[kv.Key].UnSubscribeAll();
            }
        }

        private Event GetEventInternal(string eventName)
        {
            return _registeredEvents.ContainsKey(eventName) ? _registeredEvents[eventName] : null;
        }

        private void RegisterEventInternal(string eventName)
        {
            if (!_registeredEvents.ContainsKey(eventName))
            {
                _registeredEvents[eventName] = new Event();
            }
            else
            {
                DuneLog.LogError("Event(" + eventName + ") already registered.");
            }
        }

        private void SubscribeEventInternal(string eventName, EventDelegate eventAction, bool networkInvoked)
        {
            if (_registeredEvents.ContainsKey(eventName))
            {
                _registeredEvents[eventName].Subscribe(eventAction, networkInvoked);
            }
        }
        
        protected void OnEventNetworkInvokeInternal(NetworkMessage msg)
        {
            SharedInvokeEvent parsedMsg = msg.ReadMessage<SharedInvokeEvent>();
            Event targetEvent = GetEventInternal(parsedMsg.EventName);
            parsedMsg.EventArguments.SetVariable("connectionId", msg.conn.connectionId);
            targetEvent?.Invoke(parsedMsg.EventArguments);
        }

        protected void InvokeEventInternal(string eventName, EventArguments eventArguments)
        {
            GetEventInternal(eventName)?.Invoke(eventArguments);
        }
        
#endregion
#region public

        /// <summary>
        /// Registers an instance of IEventCompatible with the Event Controller.
        /// </summary>
        /// <remarks>
        /// This should never be called manually.
        /// </remarks>
        public void RegisterDynamicEvent(IEventCompatible target)
        {
            Type targetType = target.GetType();

            var eventTypeAttributes =
                from m in targetType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                let attributes = m.GetCustomAttributes(true)
                where attributes.OfType<EventRegAttribute>().Any()
                select new {Method = m, Attributes = attributes.Cast<EventRegAttribute>()};

            foreach (var kv in eventTypeAttributes)
            {
                EventRegAttribute regAttribute = kv.Attributes.DefaultIfEmpty(null).FirstOrDefault();
                if (regAttribute == null) continue;

                NetworkContext context = regAttribute.Context;
                MethodInfo method = kv.Method;

                if (!string.IsNullOrEmpty(regAttribute.Name) && context == _networkContext && method.ReturnType == typeof(void)
                    && !method.ContainsGenericParameters
                    && method.GetParameters().Any(x => x.ParameterType == typeof(EventArguments)))
                {
                    if (!_registeredEvents.ContainsKey(regAttribute.Name))
                    {
                        RegisterEventInternal(regAttribute.Name);
                    }
                    
                    SubscribeEventInternal(regAttribute.Name, (EventDelegate) method.CreateDelegate(typeof(EventDelegate), target), regAttribute.NetworkInvoked);
                }
            }
        }
        
        /// <summary>
        /// Disposes the Event Controller and frees any resources internally used by it.
        /// </summary>
        /// <remarks>
        /// There should be no need to manually call this in most cases as it is already automatically called on application quit.
        /// </remarks>
        public void Dispose()
        {
            UnregisterAllEventsInternal();
            _registeredEvents = null;
        }
        
#endregion
    }
}