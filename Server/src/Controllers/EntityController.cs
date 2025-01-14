using System.Collections.Generic;
using System.Linq;
using DuneNet.Shared.Controllers;
using DuneNet.Shared.Entities;
using DuneNet.Shared.Messages;
using UnityEngine;
using UnityEngine.Networking;
using DuneNet.Server.Entities;
using DuneNet.Server.Messages;
using DuneNet.Shared;
using DuneNet.Shared.Enums;

namespace DuneNet.Server.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// An Entity Controller with extra functionality for the server.
    /// </summary>
    public sealed class EntityController : BaseEntityController
    {
#region internal
        
        private uint _lastEntityId;
        
        internal EntityController(NetworkContext networkContext) : base(networkContext)
        {
        }

        internal void Init()
        {
            DuneServer.NetworkController.RegisterHandler(MsgTypes.SetEntityUMessage, OnSetEntityUMessage);
        }

        private Entity SpawnEntityInternal(string entName, DuneConnection authorityConnection = null, Vector3 entPos = new Vector3(), Quaternion entRot = new Quaternion())
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
            
            ent.EntId = _lastEntityId;
            
            entGo.name = "Entity(" + entName + ") #" + _lastEntityId;
            AddEntity(ent);
            
            _lastEntityId++;
            ent.transform.SetPositionAndRotation(entPos, entRot);
            ent.SpawnEntity(entName, entityInfo.Value.EntType, authorityConnection);
            return ent;
        }
        
        private void OnSetEntityUMessage(NetworkMessage msg)
        {
            ServerSetEntityUMessage parsedMsg = msg.ReadMessage<ServerSetEntityUMessage>();
            Entity targetEnt = (Entity) GetEntityFromIdInternal(parsedMsg.EntId);
            if (targetEnt != null && targetEnt.AuthorityConnection != null && targetEnt.AuthorityConnection == msg.conn)
            {
                targetEnt.EntUMessages[parsedMsg.Name] = parsedMsg.Value;
                targetEnt.OnSetUMessageInternal(parsedMsg.Name);
            }
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
        /// <param name="entName">The registration name of the entity to spawn.</param>
        /// <param name="authorityConnection">The connection that will have authority over the entity to spawn. If Null, the server will keep authority over it.</param>
        /// <param name="entPos">The initial position of the entity to spawn.</param>
        /// <param name="entRot">The initial rotation of the entity to spawn.</param>
        /// <returns>An instance of Entity representing the spawned entity.</returns>
        public Entity SpawnEntity(string entName, DuneConnection authorityConnection = null, Vector3 entPos = new Vector3(), Quaternion entRot = new Quaternion())
        {
            return SpawnEntityInternal(entName, authorityConnection, entPos, entRot);
        }

        /// <summary>
        /// Destroys a spawned entity.
        /// </summary>
        /// <param name="ent">The enttiy to destroy.</param>
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
            _lastEntityId = 0;
        }

        /// <summary>
        /// Returns a spawned entity identified by its id.
        /// </summary>
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
        
        /// <summary>
        /// Returns all the spawned entities the provided connection has authority over.
        /// </summary>
        /// <param name="conn">The authoritative connection for the requested entities.</param>
        /// <returns>An IEnumerable instance of type Entity containing all the spawned entities.</returns>
        public IEnumerable<Entity> GetEntitiesForAuthority(NetworkConnection conn)
        {
            return GetEntitiesInternal().Cast<Entity>().Where(x => x?.AuthorityConnection != null && x.AuthorityConnection.connectionId == conn.connectionId);
        }
        
#endregion
    }
}