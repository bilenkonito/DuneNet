using System;

namespace DuneNet.Shared.Modules
{
    /// <summary>
    /// A Handshake Eesponse used to authenticate clients.
    /// </summary>
    [Serializable]
    public class HandshakeResponse
    {
        /// <summary>
        /// The server authentication token
        /// </summary>
        public string AuthenticationToken = "";
        
        /// <summary>
        /// Whether the client was successfully authenticated. True if the client successfully authenticated, False otherwise.
        /// </summary>
        public bool Allowed = true;
        
        /// <summary>
        /// The server authentication error. Only populated if the authentication attempt was rejected.
        /// </summary>
        public string Error = "";
    }
}