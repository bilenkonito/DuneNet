using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using DuneNet.Client.Messages;
using DuneNet.Shared;
using DuneNet.Shared.Entities;
using DuneNet.Shared.Events;
using DuneNet.Shared.Messages;
using UnityEngine;

namespace DuneNet.Client.Entities
{
    /// <inheritdoc cref="BaseEntity" />
    /// <inheritdoc cref="IEventCompatible" />
    /// <summary>
    /// A client entity.
    /// </summary>
    [AddComponentMenu("")]
    public abstract class Entity : BaseEntity, IEventCompatible
    {
#region internal
        private int _stateCount;
        private EntityState[] _stateBuffer;
        
        private Dictionary<string, NetworkedVariable> _entUMessages;
        internal Dictionary<string, byte[]> EntNetworkData;
        
        public void RegisterEventInstance()
        {
            DuneClient.EventController.RegisterDynamicEvent(this);
        }
        
        protected sealed override void Awake()
        {
            _stateCount = 0;
            _stateBuffer = new EntityState[20];
            EntNetworkData = new Dictionary<string, byte[]>();
            _entUMessages = new Dictionary<string, NetworkedVariable>();
            base.Awake();
        }

        protected sealed override void Start()
        {
            base.Start();
        }

        protected sealed override void Update()
        {
            UpdatePosition();   
            base.Update();
        }

        protected sealed override void FixedUpdate()
        {
            NetUpdateTime += Time.deltaTime;
            if (NetUpdateTime > NetUpdateInterval)
            {
                OnNetUpdate();
                if (DirtyBit)
                {
                    UpdateUMessages();
                }
                NetUpdateTime = 0;
            }
            
            base.FixedUpdate();
        }

        protected sealed override void LateUpdate()
        {
            base.LateUpdate();
        }
        
        internal void AddState(EntityState state)
        {
            if (_stateCount < 1)
            {
                _stateBuffer[0] = state;
            }
            else
            {
                for (int i = 0; i < _stateCount; i++)
                {
                    if (_stateBuffer[i] == null || _stateBuffer[i].Timestamp >= state.Timestamp) continue;
                    
                    for (int k = _stateBuffer.Length - 1; k > i; k--)
                    {
                        _stateBuffer[k] = _stateBuffer[k - 1];
                    }

                    _stateBuffer[i] = state;
                        
                    break;
                }
            }

            _stateCount = Mathf.Min(_stateCount + 1, _stateBuffer.Length);
        }

        private void UpdatePosition()
        {
            if (_stateCount < 1) return;

            float renderDelay = Time.deltaTime * 10;
                
            byte error;
            int firstStateDelay = DuneClient.NetworkController.GetRemoteDelayTimeMs(_stateBuffer[0].Timestamp, out error);

            if (firstStateDelay < renderDelay)
            {
                for (int i = 0; i < _stateCount; i++)
                {
                    int lhsDelay = DuneClient.NetworkController.GetRemoteDelayTimeMs(_stateBuffer[i].Timestamp, out error);

                    if (!(lhsDelay >= renderDelay) && i != _stateCount - 1) continue;
                    
                    EntityState rhs = _stateBuffer[Mathf.Max(i - 1, 0)];

                    int rhsDelay = DuneClient.NetworkController.GetRemoteDelayTimeMs(rhs.Timestamp, out error);
                    
                    EntityState lhs = _stateBuffer[i];

                    float t = Mathf.InverseLerp(lhsDelay, rhsDelay, renderDelay);

                    transform.position = Vector3.Lerp(lhs.Position, rhs.Position, t);
                    transform.rotation = Quaternion.Slerp(lhs.Rotation, rhs.Rotation, t);

                    return;
                }
            }
            else
            {
                EntityState latest = _stateBuffer[0];
                
                transform.position = Vector3.Lerp(transform.position, latest.Position, renderDelay);
                transform.rotation = Quaternion.Slerp(transform.rotation, latest.Rotation, renderDelay);
            }
        }
        
        private void UpdateUMessages()
        {
            if (EntId.HasValue)
            {
                foreach (KeyValuePair<string, NetworkedVariable> kv in _entUMessages)
                {
                    if (!kv.Value.DirtyBit) continue;

                    ClientSetEntityUMessage msg = new ClientSetEntityUMessage(EntId.Value, kv.Key, kv.Value.Value);

                    DuneClient.NetworkController.SendByChannelToServer(MsgTypes.SetEntityUMessage, msg, MessageChannels.EntityDataReliableSequenced);

                    _entUMessages[kv.Key].DirtyBit = false;
                }
                DirtyBit = false;
            }
            else
            {
                DuneLog.LogError("Clientside entities do not support UMessages as they are not networked");
            }
        }

        internal void OnSetNetVarInternal(string varName)
        {
            OnSetNetVar(varName);
        }
        
        internal void SpawnEntity(string entName, Type entType)
        {
            if (IsEntitySpawned)
                return;
            
            EntName = entName;
            EntType = entType;
            
            IsEntitySpawned = true;
            
            OnEntitySpawned();
        }
        
#endregion
#region public
        
        /// <summary>
        /// True if the entity has local authority, False otherwise.
        /// </summary>
        public bool HasAuthority { get; internal set; }
        
        /// <summary>
        /// The amount of movement states that have not been processed yet.
        /// </summary>
        /// <remarks>
        /// This will always be Zero if the entity is clientside or its movement is not being synchronized.
        /// </remarks>
        public int StateCount => _stateCount;
        
        /// <summary>
        /// A dictionary containing all the current User Messages and their values.
        /// </summary>
        /// <remarks>
        /// Used for display and debug purposes. For normal operation, use SetUMessage and GetUMessage.
        /// </remarks>
        public Dictionary<string, object> UMessages
        {
            get
            {
                Dictionary<string, object> res = new Dictionary<string, object>();
                foreach (var kv in _entUMessages)
                {
                    res[kv.Key] = kv.Value.Value;
                }
                return res;
            }
        }

        /// <summary>
        /// Returns the specified Networked Variable's value.
        /// </summary>
        /// <remarks>
        /// Networked Variables are unidirectional: they are set by the server and synchronized to all clients that can observe the entity.
        /// </remarks>
        /// <param name="varName">The name of the Networked Variable to obtain.</param>
        /// <typeparam name="T">The type of the Networked Variable to obtain. It must flagged as Serializable.</typeparam>
        /// <returns>An instance of type T representing the value of the Networked Variable. If the Networked Variable does not exist, the default value of T is returned instead.</returns>
        public T GetNetworkedVar<T>(string varName)
        {
            if (!EntNetworkData.ContainsKey(varName) || !typeof(T).IsSerializable) return default(T);
            
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(EntNetworkData[varName], 0, EntNetworkData[varName].Length);
                ms.Seek(0, SeekOrigin.Begin);
                return (T) bf.Deserialize(ms);
            }
        }
        
        /// <summary>
        /// Sets the specified User Message's value.
        /// </summary>
        /// <remarks>
        /// User Messages are unidirectional: they are set by a client and synchronized to the server. 
        /// Only clients with authority over the entity may set its User Messages. 
        /// Clientside entities do not support User Messages since they are not networked. 
        /// </remarks>
        /// <param name="varName">The name of the User Message to set.</param>
        /// <param name="value">The value to set the User Message to.</param>
        public void SetUMessage(string varName, object value)
        {
            if (_entUMessages.ContainsKey(varName) && _entUMessages[varName].Value.Equals(value) || string.IsNullOrEmpty(varName) || value == null) return;

            if (!value.GetType().IsSerializable)
            {
                DuneLog.LogError($"{value.GetType()} is not serializable. User Message ({varName}, {value}) not set.");
                return;
            }
            
            _entUMessages[varName] = new NetworkedVariable
            {
                Value = value,
                DirtyBit = true
            };
        
            DirtyBit = true;
        }
        
        /// <summary>
        /// Returns the specified User Message's value.
        /// </summary>
        /// <remarks>
        /// User Messages are unidirectional: they are set by a client and synchronized to to the server. 
        /// Only clients with authority over the entity may set its User Messages. 
        /// Clientside entities do not support User Messages since they are not networked. 
        /// </remarks>
        /// <param name="varName">The name of the User Message to obtain.</param>
        /// <typeparam name="T">The type of the User Message to obtain. It must flagged as Serializable.</typeparam>
        /// <returns>An instance of type T representing the value of the User Message. If the User Message does not exist, the default value of T is returned instead.</returns>
        public T GetUMessage<T>(string varName)
        {
            return _entUMessages.ContainsKey(varName) && typeof(T).IsSerializable ? (T) _entUMessages[varName].Value : default(T);
        }
        
        /// <summary>
        /// Sets the position of the entity.
        /// </summary>
        /// <param name="position">The position to set the entity to.</param>
        public override void SetPos(Vector3 position)
        {
            Vector3 oldPos = transform.position;
            transform.position = position;
            OnSetPos(oldPos, position);
        }
        
        /// <summary>
        /// Sets the rotation of the entity.
        /// </summary>
        /// <param name="rotation">The rotation to set the entity to.</param>
        public override void SetRot(Quaternion rotation)
        {   
            Quaternion oldRot = transform.rotation;
            transform.rotation = rotation;    
            OnSetRot(oldRot, rotation);
        }

        /// <summary>
        /// Sets the parent of the entity.
        /// </summary>
        /// <param name="newParent">The entity the parent will be se to.</param>
        public override void SetParent(BaseEntity newParent)
        {
            transform.parent = newParent.transform;
            OnSetParent(newParent);
        }

        /// <summary>
        /// Overridable method called once every network update. 
        /// </summary>
        /// <remarks>
        /// How often this method gets called depends on the network tickrate of the entity (by default, 33 times per second).
        /// This method is called before any synchronization takes place.
        /// The entity should update its User Messages here since this is the fastest they will be synchronized.
        /// </remarks>
        protected virtual void OnNetUpdate()
        {
        }

        /// <summary>
        /// Overridable method called when the server updates any of the Networked Variables of the entity.
        /// </summary>
        /// <remarks>
        /// For entities with a large amount of Networked Variables, it is advisable to switch over varName.
        /// </remarks>
        /// <param name="varName">The name of the Networked Variable that was received from the server.</param>
        protected virtual void OnSetNetVar(string varName)
        {
        }
#endregion
    }
}