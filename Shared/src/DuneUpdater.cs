using System;
using System.Collections;
using UnityEngine;

namespace DuneNet.Shared
{
    /// <inheritdoc />
    /// <summary>
    /// Unitliy MonoBehaviour to provide hooks for other DuneNet elements
    /// </summary>
    [AddComponentMenu("")] 
    public class DuneUpdater : MonoBehaviour
    {
#region internal
        
        private static DuneUpdater _instance;

        private void Start()
        {
            OnStart?.Invoke();
        }
        
        private void Update()
        {
            OnUpdate?.Invoke();
        }

        private void OnApplicationQuit()
        {
            IsQuitting = true;
            OnQuit?.Invoke();
        }
        
        private static IEnumerator WaitForSecondsActionInternal(float seconds, Action callback)
        {
            yield return new WaitForSeconds(seconds);
            callback?.Invoke();
        }
        
#endregion
#region public
        
        /// <summary>
        /// The Singleton instance.
        /// </summary>
        public static DuneUpdater Instance
        {
            get
            {
                if (_instance != null) return _instance;
                
                GameObject go = new GameObject();
                _instance = go.AddComponent<DuneUpdater>();
                go.name = "DuneUpdater";
                DontDestroyOnLoad(go);

                return _instance;
            }
        }

        /// <summary>
        /// Whether the application is quitting.
        /// </summary>
        public static bool IsQuitting;

        /// <summary>
        /// Event called when DuneNet initializes.
        /// </summary>
        public event Action OnStart;
        
        /// <summary>
        /// Event called when DuneNet updates.
        /// </summary>
        public event Action OnUpdate;
        
        /// <summary>
        /// Event called when the application quits.
        /// </summary>
        public event Action OnQuit;

        /// <summary>
        /// Invokes an Action after a provided amount of seconds.
        /// </summary>
        /// <param name="seconds">The amount of seconds to execute the action after.</param>
        /// <param name="callback">The action to execute after the provided time.</param>
        public void WaitForSecondsAction(float seconds, Action callback)
        {
            StartCoroutine(WaitForSecondsActionInternal(seconds, callback));
        }
        
#endregion
    }
}