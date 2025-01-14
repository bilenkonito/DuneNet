using UnityEngine.Networking;

namespace DuneNet.Shared
{
    /// <summary>
    /// A network connection used by DuneNet.
    /// </summary>
    public class DuneConnection : NetworkConnection
    {
        /// <summary>
        /// The client authentication token.
        /// </summary>
        public string IDToken;
        
        /// <summary>
        /// Whether the client is authenticated. True if the client is authenticated, False otherwise.
        /// </summary>
        public bool Authenticated;
        
        /// <summary>
        /// The server authentication token
        /// </summary>
        public string AuthenticationToken;

        /// <summary>
        /// Whether the client is allowed to set itself ready. True if the client is allowed, False otherwise.
        /// </summary>
        public bool LocalReadiness = true;
    }
}