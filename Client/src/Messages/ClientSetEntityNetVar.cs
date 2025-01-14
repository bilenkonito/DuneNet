using UnityEngine.Networking;

namespace DuneNet.Client.Messages
{
    internal class ClientSetEntityNetVar : MessageBase
    {
        internal uint EntId;
        internal string EntVarName;
        internal byte[] EntVarValue;

        public override void Deserialize(NetworkReader reader)
        {
            EntId = reader.ReadPackedUInt32();
            EntVarName = reader.ReadString();
            EntVarValue = reader.ReadBytesAndSize();
        }
    }
}