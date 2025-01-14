using System.Collections;
using DuneNet.Shared;
using DuneNet.Shared.Events;
using UnityEngine;

namespace DuneNet.Server
{
    /// <inheritdoc cref="IEventCompatible" />
    /// <summary>
    /// A Behaviour not bound to a GameObject that can receive Events.
    /// </summary>
    public abstract class DuneBehaviour : IEventCompatible
    {
#region internal
        
        public void RegisterEventInstance()
        {
            DuneServer.EventController.RegisterDynamicEvent(this);
        }
        
        protected DuneBehaviour()
        {
            RegisterEventInstance();
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