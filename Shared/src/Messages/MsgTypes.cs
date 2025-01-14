using System;
using UnityEngine.Networking;

namespace DuneNet.Shared.Messages
{
    /// <inheritdoc />
    /// <summary>
    /// A class representing all the message types used by DuneNet.
    /// </summary>
    [Serializable]
    public class MsgTypes : MsgType
    {
        public const short RequestHandshake = 1000;
        public const short RespondHandshake = 1001;
        
        public const short SpawnEntity = 2000;
        public const short DestroyEntity = 2001;
        public const short UpdateEntityPositionAndRotation = 2002;
        public const short SetEntityPosition = 2003;
        public const short SetEntityRotation = 2004;
        public const short SetEntityParent = 2005;
        public const short SetEntityData = 2006;
        public const short SetEntityUMessage = 2007;
        
        public const short InvokeEvent = 3000;
    }
}