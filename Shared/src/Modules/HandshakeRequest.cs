using System;

namespace DuneNet.Shared.Modules
{
    /// <summary>
    /// A Handshake Request used to authenticate clients.
    /// </summary>
    [Serializable]
    public class HandshakeRequest
    {
        /// <summary>
        /// The client authentication token.
        /// </summary>
        public string IDToken = "";
        
        /// <summary>
        /// The client authentication secret.
        /// </summary>
        public byte[] Secret = new byte[0];
    }
}