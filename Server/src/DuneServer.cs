using System;
using System.Collections.Generic;
using System.Linq;
using DuneNet.Server.Controllers;
using DuneNet.Server.Modules;
using DuneNet.Shared;
using DuneNet.Shared.Enums;
using DuneNet.Shared.Events;

namespace DuneNet.Server
{
    /// <summary>
    /// A toolbox class to access DuneNet's server functionality.
    /// </summary>
    public class DuneServer
    {
#region internal
        
        private static DuneServer _instance;

        private readonly NetworkController _networkController;
        private readonly EntityController _entityController;
        private readonly EventController _eventController;
        
        private readonly List<DuneModule> _modules;
        
        internal static T InvokeOnModules<T>(Func<DuneModule, T> predicate) => GetInstance().InvokeOnModulesInternal(predicate);

        private DuneServer()
        {
            _networkController = new NetworkController();
            _entityController = new EntityController(NetworkContext.Server);
            _eventController = new EventController(NetworkContext.Server);

            DuneUpdater.Instance.OnStart += () => _eventController.InvokeEvent("DuneStart", new EventArguments(), false);
            
            DuneUpdater.Instance.OnUpdate += () => _eventController.InvokeEvent("DuneUpdate", new EventArguments(), false);
            DuneUpdater.Instance.OnUpdate += _networkController.Update;

            DuneUpdater.Instance.OnQuit += () => _eventController.InvokeEvent("DuneQuit", new EventArguments(), false);
            DuneUpdater.Instance.OnQuit += () =>
            {
                _eventController.Dispose();
                _entityController.Dispose();
                _networkController.Dispose();
            };
            
            _modules = new List<DuneModule>();
        }

        private void RegisterBaseModules()
        {
            UseInternal(new NewConnectionModule());
            UseInternal(new QuitModule());
        }

        private static DuneServer GetInstance()
        {
            if (_instance != null) return _instance;
            _instance = new DuneServer();
            _instance.RegisterBaseModules();
            return _instance;
        }
        
        private void UseInternal(DuneModule module)
        {
            if (_modules.Contains(module) || _modules.Any(x => x.GetType() == module.GetType())) return;
            
            DuneModule lastModule = _modules.LastOrDefault();
            if (lastModule != null)
            {
                lastModule.Next = module;
            }
            else
            {
                module.Next = null;
            }
            _modules.Add(module);
            module.OnUseInternal();
        }

        private void StopUsingInternal(DuneModule module)
        {
            if (!_modules.Contains(module)) return;
            
            int indexOfModule = _modules.IndexOf(module);
            if (indexOfModule - 1 >= 0)
            {
                DuneModule previousModule = _modules[indexOfModule - 1];
                if (indexOfModule + 1 < _modules.Count)
                {
                    DuneModule nextModule = _modules[indexOfModule + 1];

                    previousModule.Next = nextModule;
                }
                else
                {
                    previousModule.Next = null;
                }
            }
            _modules.Remove(module);
            module.OnStopUsingInternal();
        }

        private T GetModuleInternal<T>() where T:DuneModule
        {
            return (T) _modules.Find(x => x.GetType() == typeof(T));
        }

        private T InvokeOnModulesInternal<T>(Func<DuneModule, T> predicate)
        {
            DuneModule first = _modules.DefaultIfEmpty(null).FirstOrDefault();
            return first != null && predicate != null ? predicate.Invoke(first) : default(T);
        }

#endregion
#region public
        
        /// <summary>
        /// The server NetworkController.
        /// </summary>
        public static NetworkController NetworkController => GetInstance()._networkController;
        
        /// <summary>
        /// The server EntityController.
        /// </summary>
        public static EntityController EntityController => GetInstance()._entityController;
        
        /// <summary>
        /// The server EventController.
        /// </summary>
        public static EventController EventController => GetInstance()._eventController;
        
        /// <summary>
        /// Adds a DuneModule to the server module chain.
        /// </summary>
        /// <remarks>
        /// Only one instance of each module type is allowed to be registered at the same time.
        /// Modules are chained in the order they are added.
        /// </remarks>
        /// <param name="module">The module to add to the module chain.</param>
        public static void Use(DuneModule module) => GetInstance().UseInternal(module);
        
        /// <summary>
        /// Removes a DuneModule from the server module chain.
        /// </summary>
        /// <remarks>
        /// When a module is removed, the next module to the one being removed is chained to the previous one.
        /// </remarks>
        /// <param name="module">The module to remove from the module chain.</param>
        public static void StopUsing(DuneModule module) => GetInstance().StopUsingInternal(module);
        
        /// <summary>
        /// Returns the specified module type from the client module chain.
        /// </summary>
        /// <remarks>
        /// Since only one instance of each module type is allowed to be registered at the same time, this will only return a single result.
        /// </remarks>
        /// <typeparam name="T">The type of the module to obtain.</typeparam>
        /// <returns>The requested module if a match was found, null otherwise.</returns>
        public static T GetModule<T>() where T : DuneModule => GetInstance().GetModuleInternal<T>();

#endregion
    }
}