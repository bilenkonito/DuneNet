using UnityEngine.Networking;

namespace DuneNet.Shared.Messages
{
    public class SharedSetEntityParent : MessageBase
    {
        public uint EntId;
        public uint ParentEntId;

        public SharedSetEntityParent()
        {
        }
        
        public SharedSetEntityParent(uint entId, uint parentEntId)
        {
            EntId = entId;
            ParentEntId = parentEntId;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.WritePackedUInt32(EntId);
            writer.WritePackedUInt32(ParentEntId);
        }

        public override void Deserialize(NetworkReader reader)
        {
            EntId = reader.ReadPackedUInt32();
            ParentEntId = reader.ReadPackedUInt32();
        }
    }
}