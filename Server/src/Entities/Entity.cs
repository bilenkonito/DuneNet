using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using DuneNet.Server.Messages;
using DuneNet.Shared;
using DuneNet.Shared.Entities;
using DuneNet.Shared.Events;
using DuneNet.Shared.Messages;
using UnityEngine;
using UnityEngine.Networking;

namespace DuneNet.Server.Entities
{
    /// <inheritdoc cref="BaseEntity" />
    /// <inheritdoc cref="IEventCompatible" />
    /// <summary>
    /// A server entity.
    /// </summary>
    [AddComponentMenu("")]
    public abstract class Entity : BaseEntity, IEventCompatible
    {
#region internal
		
        private Dictionary<string, NetworkedVariable> _entNetworkData;
        internal Dictionary<string, byte[]> EntUMessages;
        
        public void RegisterEventInstance()
        {
            DuneServer.EventController.RegisterDynamicEvent(this);
        }
        
        protected sealed override void Awake()
        {
            _entNetworkData = new Dictionary<string, NetworkedVariable>();
            EntUMessages = new Dictionary<string, byte[]>();
            base.Awake();
        }

        protected sealed override void Start()
        {
            base.Start();
        }

        protected sealed override void Update()
        {
            base.Update();
        }

        protected sealed override void FixedUpdate()
        {
            NetUpdateTime += Time.deltaTime;
            if (NetUpdateTime > NetUpdateInterval)
            {
                OnNetUpdate();
                if (NetworkedPositionAndRotation && EntId.HasValue && DuneServer.NetworkController.IsListening())
                {
                    SharedUpdateEntityPositionAndRotation updateMsg = new SharedUpdateEntityPositionAndRotation(EntId.Value, transform.position, transform.rotation);
                    DuneServer.NetworkController.SendByChannelToReady(MsgTypes.UpdateEntityPositionAndRotation, updateMsg, MessageChannels.PositionAndRotationUnreliable);
                }
                if (DirtyBit)
                {
                    UpdateNetworkedVars();
                }
                NetUpdateTime = 0;
            }
            
            base.FixedUpdate();
        }

        protected sealed override void LateUpdate()
        {
            OnLateUpdate();
        }
        
        internal void SpawnEntity(string entName, Type entType, DuneConnection authorityConnection = null)
        {
            if (IsEntitySpawned)
                return;

            EntName = entName;
            EntType = entType;

            if (EntId.HasValue)
            {
                if (authorityConnection != null && DuneServer.NetworkController.IsListening())
                {
                    DuneServer.NetworkController.SendByChannelToReadyWithAuthority(MsgTypes.SpawnEntity, authorityConnection,
                        new SharedSpawnEntity(EntId.Value, entName, transform.position, transform.rotation), MessageChannels.GeneralReliableSequenced);
                    AuthorityConnection = authorityConnection;
                }
                else
                {
                    DuneServer.NetworkController.SendByChannelToReadyWithAuthority(MsgTypes.SpawnEntity, null,
                        new SharedSpawnEntity(EntId.Value, entName, transform.position, transform.rotation), MessageChannels.GeneralReliableSequenced);
                }
            }

            IsEntitySpawned = true;

            OnEntitySpawned();
        }

        protected sealed override void OnEntityDestroyedInternal()
        {
            base.OnEntityDestroyedInternal();
            
            if (!DuneUpdater.IsQuitting)
            {
                DuneServer.EntityController.GetEntities().Where(x => x != null && x.transform.parent != null && x.transform.parent == transform).ToList().ForEach(x =>
                {
                    if (x.EntId.HasValue)
                    {
                        SharedDestroyEntity childMsg = new SharedDestroyEntity(x.EntId.Value);
                        DuneServer.NetworkController.SendByChannelToReady(MsgTypes.DestroyEntity, childMsg, MessageChannels.GeneralReliableSequenced);
                    }
                
                    DuneServer.EntityController.DestroyEntity(x);
                });
                
                if (EntId.HasValue)
                {
                    SharedDestroyEntity parentMsg = new SharedDestroyEntity(EntId.Value);
                    DuneServer.NetworkController.SendByChannelToReady(MsgTypes.DestroyEntity, parentMsg, MessageChannels.GeneralReliableSequenced);
                }
            }
        }
        
        private void UpdateNetworkedVars()
        {
            if (!EntId.HasValue || !DuneServer.NetworkController.IsListening()) return;
            
            foreach (KeyValuePair<string, NetworkedVariable> kv in _entNetworkData)
            {
                if (!kv.Value.DirtyBit) continue;
                
                ServerSetEntityNetVar msg = new ServerSetEntityNetVar(EntId.Value, kv.Key, kv.Value.Value);
                DuneServer.NetworkController.SendByChannelToReady(MsgTypes.SetEntityData, msg, MessageChannels.EntityDataReliableSequenced);

                _entNetworkData[kv.Key].DirtyBit = false;
            }
            DirtyBit = false;
        }

        internal void OnSetUMessageInternal(string varName)
        {
            OnSetUMessage(varName);
        }
        
#endregion
#region public
        
        /// <summary>
        /// Whether the movement of the entity should be synchronized.
        /// </summary>
        public bool NetworkedPositionAndRotation { get; set; }
        
        /// <summary>
        /// The connection that has authority over the entity.
        /// </summary>
        public DuneConnection AuthorityConnection { get; private set; }
        
        /// <summary>
        /// A dictionary containing all the current Networked Variables and their values.
        /// </summary>
        /// <remarks>
        /// Used for display and debug purposes. For normal operation, use SetNetworkedVar and GetNetworkedVar.
        /// </remarks>
        public Dictionary<string, object> NetVars
        {
            get
            {
                Dictionary<string, object> res = new Dictionary<string, object>();
                foreach (var kv in _entNetworkData)
                {
                    res[kv.Key] = kv.Value.Value;
                }
                return res;
            }
        }
        
        /// <summary>
        /// Sets the specified Networked Variable's value.
        /// </summary>
        /// <remarks>
        /// Networked Variables are unidirectional: they are set by the server and synchronized to all clients that can observe the entity.  
        /// </remarks>
        /// <param name="varName">The name of the Networked Variable to set.</param>
        /// <param name="value">The value to set the Networked Variable to.</param>
        public void SetNetworkedVar(string varName, object value)
        {
            if (_entNetworkData.ContainsKey(varName) && _entNetworkData[varName].Value.Equals(value) || string.IsNullOrEmpty(varName) || value == null) return;
            
            if (!value.GetType().IsSerializable)
            {
                DuneLog.LogError($"{value.GetType()} is not serializable. Networked Variable ({varName}, {value}) not set.");
                return;
            }
            
            _entNetworkData[varName] = new NetworkedVariable
            {
                Value = value,
                DirtyBit = true
            };
        
            DirtyBit = true;
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
            return _entNetworkData.ContainsKey(varName) ? (T) _entNetworkData[varName].Value : default(T);
        }
        
        /// <summary>
        /// Returns the specified User Message's value.
        /// </summary>
        /// <remarks>
        /// User Messages are unidirectional: they are set by a client and synchronized to to the server. 
        /// Only clients with authority over the entity may set its User Messages. 
        /// </remarks>
        /// <param name="varName">The name of the User Message to obtain.</param>
        /// <typeparam name="T">The type of the User Message to obtain. It must flagged as Serializable.</typeparam>
        /// <returns>An instance of type T representing the value of the User Message. If the User Message does not exist, the default value of T is returned instead.</returns>
        public T GetUMessage<T>(string varName)
        {
            if (!EntUMessages.ContainsKey(varName)) return default(T);
            
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(EntUMessages[varName], 0, EntUMessages[varName].Length);
                ms.Seek(0, SeekOrigin.Begin);
                return (T) bf.Deserialize(ms);
            }
        }

        /// <summary>
        /// Sends the entity to the provided connection without respawning it on the server or the unaffected clients.
        /// </summary>
        /// <remarks>
        /// This is useful for sending entities to newly connecing clients.
        /// </remarks>
        /// <param name="conn">The connection to send the entity to.</param>
        public void SendEntityToConnection(DuneConnection conn)
        {
            if (EntId.HasValue)
            {
                conn.SendByChannel(MsgTypes.SpawnEntity, new SharedSpawnEntity(EntId.Value, EntName, transform.position, transform.rotation), MessageChannels.GeneralReliableSequenced);
            }
        }

        /// <summary>
        /// Sets the position of the entity and sends it to all ready clients.
        /// </summary>
        /// <param name="position">The position to set the entity to.</param>
        public override void SetPos(Vector3 position)
        {
            Vector3 oldPos = transform.position;
            transform.position = position;

            if (EntId.HasValue && DuneServer.NetworkController.IsListening())
            {
                SharedSetEntityPosition msg = new SharedSetEntityPosition(EntId.Value, position);
                DuneServer.NetworkController.SendByChannelToReady(MsgTypes.SetEntityPosition, msg, MessageChannels.GeneralUnreliable);
            }

            OnSetPos(oldPos, position);
        }

        /// <summary>
        /// Sets the rotation of the entity and sends it to all ready clients.
        /// </summary>
        /// <param name="rotation">The rotation to set the entity to.</param>
        public override void SetRot(Quaternion rotation)
        {
            Quaternion oldRot = transform.rotation;
            transform.rotation = rotation;

            if (EntId.HasValue && DuneServer.NetworkController.IsListening())
            {
                SharedSetEntityRotation msg = new SharedSetEntityRotation(EntId.Value, rotation);
                DuneServer.NetworkController.SendByChannelToReady(MsgTypes.SetEntityRotation, msg, MessageChannels.GeneralUnreliable);
            }

            OnSetRot(oldRot, rotation);
        }

        /// <summary>
        /// Sets the parent of the entity and sends it to all ready clients.
        /// </summary>
        /// <param name="newParent">The entity the parent will be se to.</param>
        public override void SetParent(BaseEntity newParent)
        {
            transform.parent = newParent.transform;

            if (EntId.HasValue && newParent.EntId.HasValue && DuneServer.NetworkController.IsListening())
            {
                SharedSetEntityParent msg = new SharedSetEntityParent(EntId.Value, newParent.EntId.Value);
                DuneServer.NetworkController.SendByChannelToReady(MsgTypes.SetEntityParent, msg, MessageChannels.GeneralReliableSequenced);
            }
            
            OnSetParent(newParent);
        }
        
        /// <summary>
        /// Overridable method called once every network update. 
        /// </summary>
        /// <remarks>
        /// How often this method gets called depends on the network tickrate of the entity (by default, 33 times per second).
        /// This method is called before any synchronization takes place.
        /// The entity should update its Networked Variables here since this is the fastest they will be synchronized.
        /// </remarks>
        protected virtual void OnNetUpdate()
        {
        }

        /// <summary>
        /// Overridable method called when the authoritative client updates any of the User Messages of the entity.
        /// </summary>
        /// <remarks>
        /// For entities with a large amount of User Messages, it is advisable to switch over varName.
        /// </remarks>
        /// <param name="varName">The name of the User Message that was received from the server.</param>
        protected virtual void OnSetUMessage(string varName)
        {
        }
        
#endregion
    }
}