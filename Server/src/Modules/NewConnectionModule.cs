using DuneNet.Server.Entities;
using DuneNet.Shared;

namespace DuneNet.Server.Modules
{
    /// <inheritdoc />
    /// <summary>
    /// A basic DuneModule to handle new connections from clients.
    /// </summary>
    public class NewConnectionModule : DuneModule
    {
#region public
        
        protected override void OnNetReady(DuneConnection conn)
        {
            foreach (Entity ent in DuneServer.EntityController.GetEntities())
            {
                ent.SendEntityToConnection(conn);
            }
        }
        
#endregion
    }
}