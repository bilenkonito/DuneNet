using System;
using System.Collections.Generic;
using System.Linq;
using DuneNet.Shared.Entities;
using DuneNet.Shared.Enums;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DuneNet.Shared.Controllers
{
    /// <summary>
    /// A Base Entity Controller.
    /// </summary>
    public abstract class BaseEntityController
    {
#region internal
        
        private Dictionary<string, EntityInfo> _registeredEnts;
        private List<BaseEntity> _entities;
        
        protected BaseEntityController(NetworkContext networkContext)
        {
            _registeredEnts = new Dictionary<string, EntityInfo>();
            _entities = new List<BaseEntity>();

            RegisterEntitiesInternal(networkContext);
        }

        private void RegisterEntitiesInternal(NetworkContext networkContext)
        {
            var entityTypeAttributes =
                from a in AppDomain.CurrentDomain.GetAssemblies().Where(x => x != null)
                from t in a.GetTypes().AsParallel()
                let attributes = t.GetCustomAttributes(typeof(EntityRegAttribute), false)
                where attributes != null && attributes.Length > 0
                select new { Type = t, Attributes = attributes.Cast<EntityRegAttribute>() };

            foreach (var kv in entityTypeAttributes)
            {
                EntityRegAttribute regAttribute = kv.Attributes.DefaultIfEmpty(null).FirstOrDefault();
                if (string.IsNullOrEmpty(regAttribute?.Name) || string.IsNullOrEmpty(regAttribute.PrefabPath) || regAttribute.Context != networkContext) continue;

                GameObject go = null;
                if (!string.IsNullOrEmpty(regAttribute.AssetBundle))
                {
                    foreach (AssetBundle loadedBundle in AssetBundle.GetAllLoadedAssetBundles())
                    {
                        if (loadedBundle.name != regAttribute.AssetBundle) continue;
                        
                        go = loadedBundle.LoadAsset<GameObject>(regAttribute.PrefabPath);
                        break;
                    }

                    if (go == null)
                    {
                        string bundlePath = FindAssetBundle(regAttribute.AssetBundle);
                        if (!string.IsNullOrEmpty(bundlePath))
                        {
                            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
                            go = bundle?.LoadAsset<GameObject>(regAttribute.PrefabPath);
                        }
                    }
                }
                else
                {
                    go = Resources.Load<GameObject>(regAttribute.PrefabPath);
                }
                
                if (go == null) continue;
                
                _registeredEnts[regAttribute.Name] = new EntityInfo
                {
                    EntType = kv.Type,
                    EntObject = go,
                    EntAssetBundleName = regAttribute.AssetBundle
                };
            }
        }

        private static string FindAssetBundle(string bundleName)
        {
            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "AssetBundles", bundleName);

            if (!filePath.Contains("://")) return filePath;
            
            Debug.LogError("Packaged asset bundles not supported yet.");
            return null;

        }
        
        protected EntityInfo? GetEntityInfoInternal(string name)
        {
            if (_registeredEnts.ContainsKey(name))
            {
                return _registeredEnts[name];
            }
            return null;
        }
        
        protected void DestroyEntityInternal(BaseEntity ent)
        {
            if (ent == null) return;
            
            ent.DestroyEntity();
            
            GameObject go = ent.gameObject;
            _entities.Remove(ent);
            if (go != null)
            {
                Object.Destroy(go);
            }                
            _entities.RemoveAll(x => x == null);
        }
        
        protected void DestroyAllEntitiesInternal()
        {
            foreach (BaseEntity ent in _entities)
            {
                GameObject go = ent.gameObject;
                Object.Destroy(ent);
                Object.Destroy(go);
            }
            _entities.RemoveAll(x => true);
        }
        
        protected BaseEntity GetEntityFromIdInternal(uint entId)
        {
            return (from ent in _entities
                where ent.EntId == entId
                select ent).DefaultIfEmpty(null).FirstOrDefault();
        }
        
        protected IEnumerable<T> GetEntitiesInternal<T>()
        {
            return GetEntitiesInternal().Where(x => typeof(T).IsAssignableFrom(x.EntType)).Cast<T>();
        }

        protected IEnumerable<BaseEntity> GetEntitiesInternal()
        {
            return _entities.ToList();
        }

        protected virtual void AddEntity(BaseEntity ent)
        {
            _entities.Add(ent);
        }
        
#endregion
#region public

        /// <summary>
        /// Disposes the Entity Controller and frees any resources internally used by it.
        /// </summary>
        /// <remarks>
        /// There should be no need to manually call this in most cases as it is already automatically called on application quit.
        /// </remarks>
        public void Dispose()
        {
            DestroyAllEntitiesInternal();
            _registeredEnts = null;
            _entities = null;
        }
        
#endregion
    }
}