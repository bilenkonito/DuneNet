namespace DuneNet.Shared.Events
{
    public interface IEventCompatible
    {
        /// <summary>
        /// Registers the instance with the EventController.
        /// </summary>
        /// <remarks>
        /// This should never be called manually.
        /// </remarks>
        void RegisterEventInstance();
    }
}