using System;
using DuneNet.Shared.Events;
using UnityEngine;

namespace DuneNet.Shared.Entities
{
    /// <inheritdoc />
    /// <summary>
    /// A base entity.
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public abstract class BaseEntity : MonoBehaviour
    {
#region internal
        
        protected bool DirtyBit;
        protected float NetUpdateTime;
        
        protected virtual void Awake()
        {
            OnAwake();
        }

        protected virtual void Start()
        {
            OnStart();
        }

        protected virtual void Update()
        {
            OnUpdate();
        }

        protected virtual void FixedUpdate()
        {
            OnFixedUpdate();
        }

        protected virtual void LateUpdate()
        {
            OnLateUpdate();
        }

        protected virtual void OnEntityDestroyedInternal()
        {
            OnEntityDestroyed();
        }

        internal void DestroyEntity()
        {
            OnEntityDestroyedInternal();
        }

#endregion
#region public
        
        /// <summary>
        /// The ID of the entity. Null if the entity is not networked.
        /// </summary>
        public uint? EntId;
        
        /// <summary>
        /// The registration name of the entity.
        /// </summary>
        public string EntName;
        
        /// <summary>
        /// The reflection type of the entity.
        /// </summary>
        public Type EntType;
        
        /// <summary>
        /// Whether the entity has been spawned. True if it has been spawned, False otherwise.
        /// </summary>
        public bool IsEntitySpawned;
        
        /// <summary>
        /// The network tickrate of the entity.
        /// This is how often (in seconds) the entity is synchronized.
        /// By default this is set to 0.030s, which means 33 times per second 
        /// </summary>
        public float NetUpdateInterval = 0.030f;
        
        /// <summary>
        /// Sets the position of the entity and sends it to all ready clients.
        /// </summary>
        /// <param name="position">The position to set the entity to.</param>
        public virtual void SetPos(Vector3 position)
        {
            
        }

        /// <summary>
        /// Sets the rotation of the entity and sends it to all ready clients.
        /// </summary>
        /// <param name="rotation">The rotation to set the entity to.</param>
        public virtual void SetRot(Quaternion rotation)
        {
            
        }

        /// <summary>
        /// Sets the parent of the entity and sends it to all ready clients.
        /// </summary>
        /// <param name="newParent">The entity the parent will be se to.</param>
        public virtual void SetParent(BaseEntity newParent)
        {
            
        }
        
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

        /// <summary>
        /// Overridable method called when the entity is spawned.
        /// </summary>
        protected virtual void OnEntitySpawned()
        {
        }

        /// <summary>
        /// Overridable method called when the entity is destroyed.
        /// </summary>
        protected virtual void OnEntityDestroyed()
        {
        }
        
        /// <summary>
        /// Overridable method called when the entity's position is changed.
        /// </summary>
        /// <param name="oldPos">The position before the entity was moved.</param>
        /// <param name="newPos">The position after the entity has been moved.</param>
        protected virtual void OnSetPos(Vector3 oldPos, Vector3 newPos)
        {
        }

        /// <summary>
        /// Overridable method called when the entity's rotation is changed.
        /// </summary>
        /// <param name="oldRot">The rotation before the entity was moved.</param>
        /// <param name="newRot">The rotation after the entity has been moved.</param>
        protected virtual void OnSetRot(Quaternion oldRot, Quaternion newRot)
        {
        }

        /// <summary>
        /// Overridable method called when the entity's parent is changed.
        /// </summary>
        /// <param name="newParent">The new parent of the entity.</param>
        protected virtual void OnSetParent(BaseEntity newParent)
        {
        }
        
#endregion
    }
}