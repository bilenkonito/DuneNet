using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Networking;

namespace DuneNet.Client.Messages
{
    internal class ClientSetEntityUMessage : MessageBase
    {
        private readonly uint _entId;
        private readonly string _name;
        private readonly object _value;
        
        internal ClientSetEntityUMessage(uint entId, string name, object value)
        {
            _entId = entId;
            _name = name;
            _value = value;
        }
        
        public override void Serialize(NetworkWriter writer)
        {
            writer.WritePackedUInt32(_entId);
            writer.Write(_name);
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, _value);
                byte[] serializedObject = ms.ToArray();
                writer.WriteBytesAndSize(serializedObject, serializedObject.Length);
            }
        }
    }
}