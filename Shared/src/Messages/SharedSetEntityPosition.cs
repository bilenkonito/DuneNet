using UnityEngine;
using UnityEngine.Networking;

namespace DuneNet.Shared.Messages
{
    public class SharedSetEntityPosition : MessageBase
    {
        public uint EntId;
        public Vector3 Position;

        public SharedSetEntityPosition()
        {
        }

        public SharedSetEntityPosition(uint entId, Vector3 position)
        {
            EntId = entId;
            Position = position;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.WritePackedUInt32(EntId);
            writer.Write(Position);
        }
        
        public override void Deserialize(NetworkReader reader)
        {
            EntId = reader.ReadPackedUInt32();
            Position = reader.ReadVector3();
        }
    }
}