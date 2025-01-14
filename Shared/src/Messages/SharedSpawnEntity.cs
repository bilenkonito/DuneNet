using UnityEngine;
using UnityEngine.Networking;

namespace DuneNet.Shared.Messages
{
    public class SharedSpawnEntity : SharedAuthorityMessage
    {
        public uint EntId;
        public string EntName;
        public Vector3 EntPos;
        public Quaternion EntRot;

        public SharedSpawnEntity()
        {
        }
        
        public SharedSpawnEntity(uint entId, string entName, Vector3 entPos, Quaternion entRot)
        {
            EntId = entId;
            EntName = entName;
            EntPos = entPos;
            EntRot = entRot;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.WritePackedUInt32(EntId);
            writer.Write(EntName);
            writer.Write(HasAuthority);
            writer.Write(EntPos);
            writer.Write(EntRot);
        }
        
        public override void Deserialize(NetworkReader reader)
        {
            EntId = reader.ReadPackedUInt32();
            EntName = reader.ReadString();
            HasAuthority = reader.ReadBoolean();
            EntPos = reader.ReadVector3();
            EntRot = reader.ReadQuaternion();
        }
    }
}