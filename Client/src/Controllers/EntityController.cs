using System.Collections.Generic;
using System.Linq;
using DuneNet.Client.Entities;
using DuneNet.Client.Messages;
using DuneNet.Shared;
using DuneNet.Shared.Controllers;
using DuneNet.Shared.Entities;
using DuneNet.Shared.Enums;
using DuneNet.Shared.Messages;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace DuneNet.Client.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// An Entity Controller with extra functionality for the client.
    /// </summary>
    public sealed class EntityController : BaseEntityController
    {
#region internal

        internal EntityController(NetworkContext networkContext) : base(networkContext)
        {
        }

        internal void Init()
        {
            RegisterEntityNetworkHandlers();
        }
        
        private Entity SpawnEntityInternal(uint? entId, string entName, bool hasAuthority, Vector3 entPos = new Vector3(), Quaternion entRot = new Quaternion())
        {
            EntityInfo? entityInfo = GetEntityInfoInternal(entName);
            if (entityInfo == null || entityInfo.Value.EntType == null || entityInfo.Value.EntObject == null)
            {
                DuneLog.LogError("Error creating entity " + entName);
                return null;
            }
            
            GameObject entGo = Object.Instantiate(entityInfo.Value.EntObject);
            
            Entity ent = entGo.AddComponent(entityInfo.Value.EntType) as Entity;
            if (ent == null)
            {
                Object.Destroy(entGo);
                DuneLog.LogError("Error creating entity " + entName);
                return null;
            }
            
            ent.EntId = entId;
            ent.transform.SetPositionAndRotation(entPos, entRot);
            ent.HasAuthority = hasAuthority;

            entGo.name = "Entity(" + entName + ") " + (entId.HasValue ? "#" + entId.Value : "Local Entity");
            
            AddEntity(ent);
            ent.SpawnEntity(entName, entityInfo.Value.EntType);
            return ent;
        }
        
        private void OnSpawnEntityInternal(NetworkMessage msg)
        {
            SharedSpawnEntity parsedMsg = msg.ReadMessage<SharedSpawnEntity>();

            DuneLog.Log("Received entity(" + parsedMsg.EntId + ") Name: " + parsedMsg.EntName);
            
            if (GetEntityFromIdInternal(parsedMsg.EntId) != null)
            {
                DuneLog.LogError("Entity with id " + parsedMsg.EntId + " already exists. Can't Spawn it again.");
            }
            else
            {
                SpawnEntityInternal(parsedMsg.EntId, parsedMsg.EntName, parsedMsg.HasAuthority, parsedMsg.EntPos, parsedMsg.EntRot);
            }
        }
        
        private void OnDestroyEntityInternal(NetworkMessage msg)
        {
            SharedDestroyEntity parsedMsg = msg.ReadMessage<SharedDestroyEntity>();
            Entity targetEnt = (Entity) GetEntityFromIdInternal(parsedMsg.EntId);
            if (targetEnt != null)
            {
                DestroyEntityInternal(targetEnt);
            }
        }

        private void OnUpdateEntityPositionAndRotationInternal(NetworkMessage msg)
        {
            SharedUpdateEntityPositionAndRotation parsedMsg = msg.ReadMessage<SharedUpdateEntityPositionAndRotation>();
            Entity targetEnt = (Entity) GetEntityFromIdInternal(parsedMsg.EntId);
            
            if (targetEnt == null) return;
            
            EntityState state = new EntityState
            {
                Timestamp = parsedMsg.Timestamp,
                Position = parsedMsg.Position,
                Rotation = parsedMsg.Rotation
            };
            targetEnt.AddState(state);
        }

        private void OnSetEntityPositionInternal(NetworkMessage msg)
        {
            SharedSetEntityPosition parsedMsg = msg.ReadMessage<SharedSetEntityPosition>();
            Entity targetEnt = (Entity) GetEntityFromIdInternal(parsedMsg.EntId);
            if (targetEnt != null)
            {
                targetEnt.SetPos(parsedMsg.Position);
            }
        }
        
        private void OnSetEntityRotationInternal(NetworkMessage msg)
        {
            SharedSetEntityRotation parsedMsg = msg.ReadMessage<SharedSetEntityRotation>();
            Entity targetEnt = (Entity) GetEntityFromIdInternal(parsedMsg.EntId);
            if (targetEnt != null)
            {
                targetEnt.SetRot(parsedMsg.Rotation);
            }            
        }

        private void OnSetEntityParentInternal(NetworkMessage msg)
        {
            SharedSetEntityParent parsedMsg = msg.ReadMessage<SharedSetEntityParent>();
            Entity baseEnt = (Entity) GetEntityFromIdInternal(parsedMsg.EntId);
            Entity targetEnt = (Entity) GetEntityFromIdInternal(parsedMsg.ParentEntId);
            if (baseEnt != null && targetEnt != null)
            {
                baseEnt.SetParent(targetEnt);
            }
        }

        private void OnSetEntityData(NetworkMessage msg)
        {
            ClientSetEntityNetVar parsedMsg = msg.ReadMessage<ClientSetEntityNetVar>();
            Entity targetEnt = (Entity) GetEntityFromIdInternal(parsedMsg.EntId);
            if (targetEnt != null)
            {
                targetEnt.EntNetworkData[parsedMsg.EntVarName] = parsedMsg.EntVarValue;
                targetEnt.OnSetNetVarInternal(parsedMsg.EntVarName);
            }
        }
        
        private void RegisterEntityNetworkHandlers()
        {
            DuneClient.NetworkController.RegisterHandler(MsgTypes.SpawnEntity, OnSpawnEntityInternal);
            DuneClient.NetworkController.RegisterHandler(MsgTypes.DestroyEntity, OnDestroyEntityInternal);
            DuneClient.NetworkController.RegisterHandler(MsgTypes.UpdateEntityPositionAndRotation, OnUpdateEntityPositionAndRotationInternal);
            DuneClient.NetworkController.RegisterHandler(MsgTypes.SetEntityPosition, OnSetEntityPositionInternal);
            DuneClient.NetworkController.RegisterHandler(MsgTypes.SetEntityRotation, OnSetEntityRotationInternal);
            DuneClient.NetworkController.RegisterHandler(MsgTypes.SetEntityParent, OnSetEntityParentInternal);
            DuneClient.NetworkController.RegisterHandler(MsgTypes.SetEntityData, OnSetEntityData);
        }
        
#endregion
#region public

        /// <summary>
        /// Returns the registration information of the entity identified by its registration name.
        /// </summary>
        /// <param name="name">The registration name of the entity.</param>
        /// <returns>
        /// If the requested entity was found, an EntityInfo instance containing the information of the registered entity.
        /// Null if the requested entity was not found.
        /// </returns>
        public EntityInfo? GetEntityInfo(string name)
        {
            return GetEntityInfoInternal(name);
        }

        /// <summary>
        /// Spawns an entity in the current scene.
        /// </summary>
        /// <param name="entId">The id of the entity to spawn. If null, the entity will be clientside.</param>
        /// <param name="entName">The registration name of the entity to spawn.</param>
        /// <param name="hasAuthority">The authority of the entity to spawn. True if the entity will have local authority, False otherwise.</param>
        /// <param name="entPos">The initial position of the entity to spawn.</param>
        /// <param name="entRot">The initial rotation of the entity to spawn.</param>
        /// <returns>An instance of Entity representing the spawned entity.</returns>
        public Entity SpawnEntity(uint? entId, string entName, bool hasAuthority, Vector3 entPos = new Vector3(), Quaternion entRot = new Quaternion())
        {
            return SpawnEntityInternal(entId, entName, hasAuthority, entPos, entRot);
        }

        /// <summary>
        /// Destroys a spawned entity.
        /// </summary>
        /// <param name="ent">The entity to destroy.</param>
        public void DestroyEntity(Entity ent)
        {
            DestroyEntityInternal(ent);
        }
        
        /// <summary>
        /// Destroys all spawned entities.
        /// </summary>
        public void DestroyAllEntities()
        {
            DestroyAllEntitiesInternal();
        }

        /// <summary>
        /// Returns a spawned entity identified by its ID.
        /// </summary>
        /// <remarks>
        /// Only returns networked entities.
        /// </remarks>
        /// <param name="entId">The id of the entity to find.</param>
        /// <returns>An instance of Entity representing the requested entity. Null if no such entity was found.</returns>
        public Entity GetEntityFromId(uint entId)
        {
            return (Entity) GetEntityFromIdInternal(entId);
        }

        /// <summary>
        /// Returns all the spawned entities of the type specified.
        /// </summary>
        /// <typeparam name="T">The type of the entities to find. Must inherit from Entity</typeparam>
        /// <returns>An IEnumerable instance of type T containing all the spawned entities of that type.</returns>
        public IEnumerable<T> GetEntities<T>()
        {
            return GetEntitiesInternal<T>();
        }

        /// <summary>
        /// Returns all the spawned entities.
        /// </summary>
        /// <returns>An IEnumerable instance of type Entity containing all the spawned entities.</returns>
        public IEnumerable<Entity> GetEntities()
        {
            return GetEntitiesInternal().Cast<Entity>();
        }

#endregion
    }
}