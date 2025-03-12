using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PVZEngine.Entities
{
    public interface IEntityCollider
    {
        string Name { get; }
        Entity Entity { get; }
        bool Enabled { get; }
        EntityColliderReference ToReference();
        bool CheckSphere(Vector3 center, float radius);
        Bounds GetBoundingBox();
    }
}
