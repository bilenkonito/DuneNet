using UnityEngine;
using UnityEngine.Networking;

namespace DuneNet.Shared
{
    /// <summary>
    /// A static wrapper around Unity's Debug class to support log filtering.
    /// </summary>
    public static class DuneLog
    {
        /// <summary>
        ///   <para>Logs message to the Unity Console.</para>
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        public static void Log(string message)
        {
            if (LogFilter.logInfo)
            {
                Debug.Log(message);
            }
        }

        /// <summary>
        ///   <para>Logs a warning message to the console.</para>
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        public static void LogWarning(string message)
        {
            if (LogFilter.logWarn)
            {
                Debug.LogWarning(message);
            }
        }

        /// <summary>
        ///   <para>Logs an error message to the console.</para>
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        public static void LogError(string message)
        {
            if (LogFilter.logError)
            {
                Debug.LogError(message);
            }
        }
    }
}