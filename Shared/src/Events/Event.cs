using System.Collections.Generic;

namespace DuneNet.Shared.Events
{
    internal class Event
    {
        private readonly List<EventMetadata> _eventData;

        internal Event()
        {
            _eventData = new List<EventMetadata>();
        }

        internal void Invoke(EventArguments arguments, bool networkInvoked = false)
        {
            _eventData.ForEach(e =>
            {
                if (networkInvoked)
                {
                    if (e.NetworkInvoked)
                    {
                        e.Callback?.Invoke(arguments);
                    }
                }
                else
                {
                    e.Callback?.Invoke(arguments);
                }
            });
        }

        internal void Subscribe(EventDelegate subscriberAction, bool networkInvoked)
        {
            _eventData.Add(new EventMetadata
            {
                Callback = subscriberAction,
                NetworkInvoked = networkInvoked
            });
        }

        internal void UnSubscribe(EventDelegate subscriberAction)
        {
            _eventData.RemoveAll(e => e.Callback != null && e.Callback == subscriberAction);
        }

        internal void UnSubscribeAll()
        {
            _eventData.Clear();
        }
    }
}