using UnityEngine.Networking;

namespace DuneNet.Shared.Messages
{
    public class SharedDestroyEntity : MessageBase
    {
        public uint EntId;

        public SharedDestroyEntity()
        {
        }
        
        public SharedDestroyEntity(uint entId)
        {
            EntId = entId;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.WritePackedUInt32(EntId);
        }

        public override void Deserialize(NetworkReader reader)
        {
            EntId = reader.ReadPackedUInt32();
        }
    }
}