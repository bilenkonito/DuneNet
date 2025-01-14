namespace DuneNet.Shared.Entities
{
    /// <summary>
    /// A networked variable.
    /// </summary>
    public class NetworkedVariable
    {
        /// <summary>
        /// The value of the networked variable.
        /// </summary>
        public object Value;
        
        /// <summary>
        /// Whether the networked variable has changed since the last synchronization. True if the networked variable has changed, False otherwise.
        /// </summary>
        public bool DirtyBit;
    }
}