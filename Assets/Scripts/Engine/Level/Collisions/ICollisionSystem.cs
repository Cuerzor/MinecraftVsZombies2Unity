using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PVZEngine.Entities;
using Tools.Mathematics;
using UnityEngine;

namespace PVZEngine.Level.Collisions
{
    public interface ICollisionSystem
    {
        void Update();
        void AddEntity(Entity entity);
        void RemoveEntity(Entity entity);
        EntityCollider[] OverlapBox(Vector3 center, Vector3 size, int faction, int hostileMask, int friendlyMask);
        void OverlapBoxNonAlloc(Vector3 center, Vector3 size, int faction, int hostileMask, int friendlyMask, List<EntityCollider> results);
        EntityCollider[] OverlapSphere(Vector3 center, float radius, int faction, int hostileMask, int friendlyMask);
        void OverlapSphereNonAlloc(Vector3 center, float radius, int faction, int hostileMask, int friendlyMask, List<EntityCollider> results);
        EntityCollider[] OverlapCapsule(Vector3 center, float radius, float height, int faction, int hostileMask, int friendlyMask);
        void OverlapCapsuleNonAlloc(Vector3 center, float radius, float height, int faction, int hostileMask, int friendlyMask, List<EntityCollider> results);
    }
}
