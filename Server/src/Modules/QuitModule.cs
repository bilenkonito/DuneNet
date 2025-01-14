namespace DuneNet.Server.Modules
{
    public class QuitModule : DuneModule
    {
        protected override void OnStopUsing()
        {
            DuneServer.EntityController.DestroyAllEntities();
            DuneServer.NetworkController.Disconnect();
        }
    }
}