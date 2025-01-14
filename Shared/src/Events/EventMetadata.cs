namespace DuneNet.Shared.Events
{
    internal struct EventMetadata
    {
        internal EventDelegate Callback;
        internal bool NetworkInvoked;
    }
}