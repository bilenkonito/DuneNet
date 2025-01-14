using DuneNet.Shared.Modules;
using UnityEngine.Networking;

namespace DuneNet.Shared.Messages
{
    public class SharedHandshakeResponse : MessageBase
    {
        public bool Allowed;
        public string Error;

        public SharedHandshakeResponse()
        {
        }

        public SharedHandshakeResponse(string idToken, HandshakeResponse response)
        {
            Allowed = response.Allowed;
            Error = response.Error;
        }

        public SharedHandshakeResponse(string idToken, bool allowed, string error)
        {
            Allowed = allowed;
            Error = error;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Allowed);
            
            if (!Allowed)
            {
                writer.Write(Error);
            }
        }

        public override void Deserialize(NetworkReader reader)
        {
            Allowed = reader.ReadBoolean();

            if (!Allowed)
            {
                Error = reader.ReadString();
            }
        }
    }
}