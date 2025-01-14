namespace DuneNet.Shared.Messages
{
    /// <summary>
    /// A static class representing the basic channels that both Network Controllers always have. 
    /// </summary>
    public static class MessageChannels
    {
        /// <summary>
        /// Used for general messages that require reliability and order.
        /// </summary>
        public const short GeneralReliableSequenced = 0;
        
        /// <summary>
        /// Used for general messages that require fast transmission and no reliability.
        /// </summary>
        public const short GeneralUnreliable = 1;
        
        /// <summary>
        /// Used to synchronize entity positions and rotations. Unreliable
        /// </summary>
        public const short PositionAndRotationUnreliable = 2;
        
        /// <summary>
        /// Used to synchronize entity Networked Variables and User Messages. Reliable sequenced.
        /// </summary>
        public const short EntityDataReliableSequenced = 3;
    }
}