using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace DuneNet.Shared.Events
{
    /// <summary>
    /// The arguments of an event.
    /// </summary>
    public class EventArguments
    {
#region internal
        
        private readonly Dictionary<string, byte[]> _eventData;

#endregion
#region public
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public EventArguments()
        {
            _eventData = new Dictionary<string, byte[]>();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="rawArguments">A dictionary containing the raw variables to include in the arguments.</param>
        public EventArguments(Dictionary<string, byte[]> rawArguments)
        {
            _eventData = rawArguments;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="arguments">
        /// A range of arguments to be added.
        /// This is equivalent to constructing the object with the parameterless constructor and then using SetVariable(string varName, object varValue) for each argument.
        /// </param>
        public EventArguments(params KeyValuePair<string, object>[] arguments)
        {
            foreach (KeyValuePair<string, object> arg in arguments)
            {
                SetVariable(arg.Key, arg.Value);
            }
        }

        /// <summary>
        /// Returns the specified argument variable's value.
        /// </summary>
        /// <param name="varName">The name of the variable to retrieve.</param>
        /// <param name="binder">An optional serialization binder to handle the variable deserialization.</param>
        /// <typeparam name="T">The type of the variable to retrieve. It must flagged as Serializable.</typeparam>
        /// <returns>An instance of type T representing the value of the variable. If the variable does not exist, the default value of T is returned instead.</returns>
        public T GetVariable<T>(string varName, SerializationBinder binder = null)
        {
            if (!_eventData.ContainsKey(varName) || !typeof(T).IsSerializable)
            {
                DuneLog.LogError($"{varName} does not exist or {typeof(T)} is not serializable.");
                return default(T);
            }
            
            BinaryFormatter bf = new BinaryFormatter();
            if (binder != null) bf.Binder = binder;
            
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(_eventData[varName], 0, _eventData[varName].Length);
                ms.Seek(0, SeekOrigin.Begin);
                return (T) bf.Deserialize(ms);
            }
        }

        /// <summary>
        /// Sets specified variable's value.
        /// </summary>
        /// <param name="varName">The name of the variable to set.</param>
        /// <param name="varValue">The value to set the variable to.</param>
        public void SetVariable(string varName, object varValue)
        {
            if (!varValue.GetType().IsSerializable)
            {
                DuneLog.LogError($"{varValue.GetType()} is not serializable.");
            }
            
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, varValue);
                byte[] serializedObject = ms.ToArray();
                _eventData[varName] = serializedObject;
            }
        }

        /// <summary>
        /// returns the raw variables of the arguments
        /// </summary>
        /// <remarks>
        /// Used for display and debug purposes. For normal operation, use SetVeriable and GetVariable.
        /// </remarks>
        /// <returns>A Dictionary containing the raw variables of the arguments.</returns>
        public Dictionary<string, byte[]> GetRawData()
        {
            return _eventData;
        }
        
#endregion
    }
}