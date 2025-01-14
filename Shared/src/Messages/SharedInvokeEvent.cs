using System.Collections.Generic;
using DuneNet.Shared.Events;
using UnityEngine.Networking;

namespace DuneNet.Shared.Messages
{
    public class SharedInvokeEvent : MessageBase
    {
        public string EventName;
        public EventArguments EventArguments;

        public SharedInvokeEvent()
        {
        }
        
        public SharedInvokeEvent(string eventName, EventArguments eventArguments)
        {
            EventName = eventName;
            EventArguments = eventArguments;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(EventName);
            Dictionary<string, byte[]> data = EventArguments.GetRawData();
            writer.WritePackedUInt64((uint) data.Count);
            
            foreach (KeyValuePair<string,byte[]> kv in data)
            {
                writer.Write(kv.Key);
                writer.WriteBytesAndSize(kv.Value, kv.Value.Length);
            }
        }

        public override void Deserialize(NetworkReader reader)
        {
            EventName = reader.ReadString();
            
            Dictionary<string, byte[]> data = new Dictionary<string, byte[]>();
            uint dataCount = reader.ReadPackedUInt32();

            for (uint i = 0; i < dataCount; i++)
            {
                string argName = reader.ReadString();
                byte[] argVal = reader.ReadBytesAndSize();
                data[argName] = argVal;
            }
            
            EventArguments = new EventArguments(data);
        }
    }
}