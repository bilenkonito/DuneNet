using System;
using UnityEngine;

namespace DuneNet.Shared.Entities
{
    /// <summary>
    /// A struct representing the registration information of an entity.
    /// </summary>
    public struct EntityInfo
    {
        /// <summary>
        /// The reflection type of the entity
        /// </summary>
        public Type EntType;
        
        /// <summary>
        /// The GameObject of the entity.
        /// </summary>
        /// <remarks>
        /// This is an out of scene GameObject reference, similar to prefab references inside the Editor.
        /// </remarks>
        public GameObject EntObject;

        /// <summary>
        /// The name of the asset bundle the entity is contained in.
        /// </summary>
        /// <remarks>
        /// This is null if the entity is a Resources based entity.
        /// </remarks>
        public string EntAssetBundleName;
    }
}