using System;
using DuneNet.Shared.Enums;

namespace DuneNet.Shared.Entities
{
    /// <inheritdoc />
    /// <summary>
    /// An Attribute used to describe entities. This should be used to register new entities by adding attaching it to an entity's class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class EntityRegAttribute : Attribute
    {
        /// <summary>
        /// The registration name of the entity.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// The relative path to the prefab of the entity.
        /// </summary>
        /// <remarks>
        /// The path starts inside the Resources folder once it has been mounted by Unity and should not contain the extension.
        /// If using Asset Bundles, this should be the path to the prefab inside the Asset Bundle instead.
        /// <example>entities/server/test_entity</example>
        /// </remarks>
        public string PrefabPath { get; }
        
        /// <summary>
        /// The network context the entity will be used on.
        /// </summary>
        /// <remarks>
        /// Servers cannot spawn client entities and vice versa.
        /// </remarks>
        public NetworkContext Context { get; }
        
        /// <summary>
        /// The name of the Asset Bundle where the entity prefab is stored
        /// </summary>
        /// <remarks>
        /// If this is null or empty, the entity prefab is loaded from the Resources folder.
        /// </remarks>
        public string AssetBundle { get; }

        /// <inheritdoc />
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The registration name of the entity.</param>
        /// <param name="prefabPath">
        /// The relative path to the prefab of the entity.
        /// The path starts inside the Resources folder once it has been mounted by Unity and should not contain the extension.
        /// <example>entities/server/test_entity</example>
        /// </param>
        /// <param name="context">The network context the entity will be used on.
        /// Servers cannot spawn client entities and vice versa.</param>
        public EntityRegAttribute(string name, string prefabPath, NetworkContext context, string assetBundle = null)
        {
            Name = name;
            PrefabPath = prefabPath;
            Context = context;
            AssetBundle = assetBundle;
        }
    }
}