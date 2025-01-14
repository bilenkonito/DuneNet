using UnityEngine;
using UnityEngine.Networking;

namespace DuneNet.Shared.Messages
{
    public class SharedSetEntityRotation : MessageBase
    {
        public uint EntId;
        public Quaternion Rotation;

        public SharedSetEntityRotation()
        {
        }

        public SharedSetEntityRotation(uint entId, Quaternion rotation)
        {
            EntId = entId;
            Rotation = rotation;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.WritePackedUInt32(EntId);
            writer.Write(Rotation);
        }
        
        public override void Deserialize(NetworkReader reader)
        {
            EntId = reader.ReadPackedUInt32();
            Rotation = reader.ReadQuaternion();
        }
    }
}