using DuneNet.Shared.Events;
using UnityEngine;

namespace DuneNet.Client
{
    /// <inheritdoc cref="MonoBehaviour" />
    /// <inheritdoc cref="IEventCompatible" />
    /// <summary>
    /// A MonoBehaviour wrapper that can receive Events.
    /// </summary>
    public abstract class DuneMonoBehaviour : MonoBehaviour, IEventCompatible
    {
#region internal
        
        public void RegisterEventInstance()
        {
            DuneClient.EventController.RegisterDynamicEvent(this);
        }
        
        private void Awake()
        {
            RegisterEventInstance();
            OnAwake();
        }

        private void Start()
        {
            OnStart();
        }

        private void Update()
        {
            OnUpdate();
        }

        private void FixedUpdate()
        {
            OnFixedUpdate();
        }

        private void LateUpdate()
        {
            OnLateUpdate();
        }
        
#endregion
#region public
        
        /// <summary>
        /// Overridable method called when the instance is being loaded.
        /// </summary>
        protected virtual void OnAwake()
        {
        }

        /// <summary>
        /// Overridable method called on the frame when the instance is enabled just before any of the Update methods is called the first time.
        /// </summary>
        protected virtual void OnStart()
        {
        }

        /// <summary>
        /// Overridable method called called every frame, if the DuneMonobehaviour is enabled.
        /// </summary>
        protected virtual void OnUpdate()
        {
        }

        /// <summary>
        /// Overridable method called every fixed framerate frame, if the DuneMonobehaviour is enabled.
        /// </summary>
        protected virtual void OnFixedUpdate()
        {
        }

        /// <summary>
        /// Overridable method called every frame, if the DuneMonobehaviour is enabled.
        /// </summary>
        protected virtual void OnLateUpdate()
        {
        }
        
#endregion
    }
}