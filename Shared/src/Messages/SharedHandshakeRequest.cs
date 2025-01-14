using DuneNet.Shared.Modules;
using UnityEngine.Networking;

namespace DuneNet.Shared.Messages
{
    public class SharedHandshakeRequest : MessageBase
    {
        public string IDToken;
        public byte[] Secret;

        public SharedHandshakeRequest()
        {
        }

        public SharedHandshakeRequest(HandshakeRequest request)
        {
            IDToken = request.IDToken;
            Secret = request.Secret;
        }

        public SharedHandshakeRequest(string idToken, byte[] secret)
        {
            IDToken = idToken;
            Secret = secret;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(IDToken);
            writer.WriteBytesAndSize(Secret, Secret.Length);
        }

        public override void Deserialize(NetworkReader reader)
        {
            IDToken = reader.ReadString();
            Secret = reader.ReadBytesAndSize();
        }
    }
}