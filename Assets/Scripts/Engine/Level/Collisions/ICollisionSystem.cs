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
        void InitEntity(Entity entity);
        void DestroyEntity(Entity entity);
        void AddCollider(Entity entity, IEntityCollider collider);
        bool RemoveCollider(Entity entity, string name);
        IEntityCollider GetCollider(Entity entity, string name);
        void GetCurrentCollisions(Entity entity, List<EntityCollision> collisions);


        ISerializableCollisionSystem ToSerializable();
        void LoadFromSerializable(LevelEngine level, ISerializableCollisionSystem seri);


        IEntityCollider[] OverlapBox(Vector3 center, Vector3 size, int faction, int hostileMask, int friendlyMask);
        void OverlapBoxNonAlloc(Vector3 center, Vector3 size, int faction, int hostileMask, int friendlyMask, List<IEntityCollider> results);
        IEntityCollider[] OverlapSphere(Vector3 center, float radius, int faction, int hostileMask, int friendlyMask);
        void OverlapSphereNonAlloc(Vector3 center, float radius, int faction, int hostileMask, int friendlyMask, List<IEntityCollider> results);
        IEntityCollider[] OverlapCapsule(Vector3 center, float radius, float height, int faction, int hostileMask, int friendlyMask);
        void OverlapCapsuleNonAlloc(Vector3 center, float radius, float height, int faction, int hostileMask, int friendlyMask, List<IEntityCollider> results);
    }
    public interface ISerializableCollisionSystem
    {

    }
}
