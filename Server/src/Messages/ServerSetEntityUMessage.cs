using UnityEngine.Networking;

namespace DuneNet.Server.Messages
{
    internal class ServerSetEntityUMessage : MessageBase
    {
        public uint EntId;
        public string Name;
        public byte[] Value;
        
        public override void Deserialize(NetworkReader reader)
        {
            EntId = reader.ReadPackedUInt32();
            Name = reader.ReadString();
            Value = reader.ReadBytesAndSize();
        }
    }
}