using UnityEngine.Networking;

namespace DuneNet.Shared.Messages
{
    /// <inheritdoc />
    /// <summary>
    /// A network message that supports authority.
    /// </summary>
    public abstract class SharedAuthorityMessage : MessageBase
    {
        public bool HasAuthority;
    }
}