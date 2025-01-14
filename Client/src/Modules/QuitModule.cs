namespace DuneNet.Client.Modules
{
    public class QuitModule : DuneModule
    {
        protected override void OnStopUsing()
        {
            DuneClient.EntityController.DestroyAllEntities();
            DuneClient.NetworkController.Disconnect();
        }
    }
}