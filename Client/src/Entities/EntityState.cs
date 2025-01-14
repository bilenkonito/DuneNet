using System;
using UnityEngine;

namespace DuneNet.Client.Entities
{
    [Serializable]
    internal class EntityState
    {
        internal int Timestamp;
        internal Vector3 Position;
        internal Quaternion Rotation;
    }
}