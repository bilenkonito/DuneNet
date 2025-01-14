using DuneNet.Client.Entities;
using DuneNet.Shared;

namespace DuneNet.Client.Modules
{
    /// <inheritdoc />
    /// <summary>
    /// A basic DuneModule to handle client disconnections from the server.
    /// </summary>
    public class DisconnectModule : DuneModule
    {
#region public
        
        protected override void OnNetDisconnected(DuneConnection conn)
        {
            foreach (Entity ent in DuneClient.EntityController.GetEntities())
            {
                DuneClient.EntityController.DestroyEntity(ent);
            }
        }
        
#endregion
    }
}