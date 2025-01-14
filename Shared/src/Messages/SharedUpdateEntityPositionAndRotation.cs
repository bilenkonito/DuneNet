using UnityEngine;
using UnityEngine.Networking;

namespace DuneNet.Shared.Messages
{
    public class SharedUpdateEntityPositionAndRotation : MessageBase
    {
        public int Timestamp;
        public uint EntId;
        public Vector3 Position;
        public Quaternion Rotation;

        public SharedUpdateEntityPositionAndRotation()
        {
        }
        
        public SharedUpdateEntityPositionAndRotation(uint entId, Vector3 position, Quaternion rotation)
        {
            Timestamp = NetworkTransport.GetNetworkTimestamp();
            EntId = entId;
            Position = position;
            Rotation = rotation;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Timestamp);
            writer.WritePackedUInt32(EntId);
            writer.Write(Position);
            writer.Write(Rotation);
        }
        
        public override void Deserialize(NetworkReader reader)
        {
            Timestamp = reader.ReadInt32();
            EntId = reader.ReadPackedUInt32();
            Position = reader.ReadVector3();
            Rotation = reader.ReadQuaternion();
        }
    }
}