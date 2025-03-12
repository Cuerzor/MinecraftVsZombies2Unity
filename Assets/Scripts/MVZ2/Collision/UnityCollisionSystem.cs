using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MVZ2.Collision;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Level.Collisions;
using Tools.Mathematics;
using UnityEngine;

namespace MVZ2.Collisions
{
    public class UnityCollisionSystem : MonoBehaviour, ICollisionSystem
    {
        public UnityCollisionSystem()
        {
        }
        void ICollisionSystem.Update()
        {
            int count = entities.Count;
            entities.CopyTo(simulateBuffer);
            for (int i = 0; i < count; i++)
            {
                var entity = simulateBuffer[i];
                entity.Simulate();
            }
            Physics.Simulate(1);
        }
        public void InitEntity(Entity entity)
        {
            var col = CreateCollisionEntity(entity);
            col.CreateCollider(EntityCollisionHelper.NAME_MAIN);
        }
        public void DestroyEntity(Entity entity)
        {
            DestroyCollisionEntity(entity);
        }
        public void GetCurrentCollisions(Entity entity, List<EntityCollision> collisions)
        {
            var collisionEnt = GetCollisionEntity(entity);
            if (!collisionEnt)
                return;
            collisionEnt.GetCollisions(collisions);
        }

        private UnityCollisionEntity CreateCollisionEntity(Entity entity)
        {
            var ent = Instantiate(collisionEntityTemplate, entityRoot).GetComponent<UnityCollisionEntity>();
            ent.gameObject.SetActive(true);
            ent.Init(entity);
            entities.Add(ent);
            return ent;
        }
        private void DestroyCollisionEntity(Entity entity)
        {
            var collisionEnt = GetCollisionEntity(entity);
            if (collisionEnt)
            {
                collisionEnt.gameObject.SetActive(false);
                Destroy(collisionEnt.gameObject);
            }
            entities.Remove(collisionEnt);
        }
        private UnityCollisionEntity GetCollisionEntity(Entity entity)
        {
            return entities.Find(e => e.Entity == entity);
        }

        #region 碰撞体
        public void AddCollider(Entity entity, IEntityCollider collider)
        {
            if (collider is not UnityEntityCollider entCol)
                return;
            var collisionEnt = GetCollisionEntity(entity);
            if (!collisionEnt)
                return;
            collisionEnt.AddCollider(entCol);
        }
        public bool RemoveCollider(Entity entity, string name)
        {
            var collisionEnt = GetCollisionEntity(entity);
            if (!collisionEnt)
                return false;
            return collisionEnt.RemoveCollider(name);
        }
        public UnityEntityCollider GetCollider(Entity entity, string name)
        {
            var collisionEnt = GetCollisionEntity(entity);
            if (!collisionEnt)
                return null;
            return collisionEnt.GetCollider(name);
        }
        IEntityCollider ICollisionSystem.GetCollider(Entity entity, string name) => GetCollider(entity, name);
        #endregion

        #region 检测
        public IEntityCollider[] OverlapBox(Vector3 center, Vector3 size, int faction, int hostileMask, int friendlyMask)
        {
            var results = new List<IEntityCollider>();
            OverlapBoxNonAlloc(center, size, faction, hostileMask, friendlyMask, results);
            return results.ToArray();
        }
        public void OverlapBoxNonAlloc(Vector3 center, Vector3 size, int faction, int hostileMask, int friendlyMask, List<IEntityCollider> results)
        {
            var hostileLayerMask = UnityCollisionHelper.ToObjectLayerMask(hostileMask);
            var hostileColliderCount = Physics.OverlapBoxNonAlloc(center, size * 0.5f, overlapBuffer, Quaternion.identity, hostileLayerMask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < hostileColliderCount; i++)
            {
                var col = overlapBuffer[i];
                var collider = col.GetComponent<UnityEntityCollider>();
                if (results.Contains(collider))
                    continue;
                if (!collider.Entity.IsHostile(faction))
                    continue;
                results.Add(collider);
            }

            var friendlyLayerMask = UnityCollisionHelper.ToObjectLayerMask(friendlyMask);
            var friendlyColliderCount = Physics.OverlapBoxNonAlloc(center, size * 0.5f, overlapBuffer, Quaternion.identity, friendlyLayerMask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < friendlyColliderCount; i++)
            {
                var col = overlapBuffer[i];
                var collider = col.GetComponent<UnityEntityCollider>();
                if (results.Contains(collider))
                    continue;
                if (!collider.Entity.IsFriendly(faction))
                    continue;
                results.Add(collider);
            }
        }
        public IEntityCollider[] OverlapSphere(Vector3 center, float radius, int faction, int hostileMask, int friendlyMask)
        {
            var results = new List<IEntityCollider>();
            OverlapSphereNonAlloc(center, radius, faction, hostileMask, friendlyMask, results);
            return results.ToArray();
        }
        public void OverlapSphereNonAlloc(Vector3 center, float radius, int faction, int hostileMask, int friendlyMask, List<IEntityCollider> results)
        {
            var hostileLayerMask = UnityCollisionHelper.ToObjectLayerMask(hostileMask);
            var hostileColliderCount = Physics.OverlapSphereNonAlloc(center, radius, overlapBuffer, hostileLayerMask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < hostileColliderCount; i++)
            {
                var col = overlapBuffer[i];
                var collider = col.GetComponent<UnityEntityCollider>();
                if (results.Contains(collider))
                    continue;
                if (!collider.Entity.IsHostile(faction))
                    continue;
                results.Add(collider);
            }

            var friendlyLayerMask = UnityCollisionHelper.ToObjectLayerMask(friendlyMask);
            var friendlyColliderCount = Physics.OverlapSphereNonAlloc(center, radius, overlapBuffer, friendlyLayerMask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < friendlyColliderCount; i++)
            {
                var col = overlapBuffer[i];
                var collider = col.GetComponent<UnityEntityCollider>();
                if (results.Contains(collider))
                    continue;
                if (!collider.Entity.IsFriendly(faction))
                    continue;
                results.Add(collider);
            }
        }
        public IEntityCollider[] OverlapCapsule(Vector3 point0, Vector3 point1, float radius, int faction, int hostileMask, int friendlyMask)
        {
            var results = new List<IEntityCollider>();
            OverlapCapsuleNonAlloc(point0, point1, radius, faction, hostileMask, friendlyMask, results);
            return results.ToArray();
        }
        public void OverlapCapsuleNonAlloc(Vector3 point0, Vector3 point1, float radius, int faction, int hostileMask, int friendlyMask, List<IEntityCollider> results)
        {
            var hostileLayerMask = UnityCollisionHelper.ToObjectLayerMask(hostileMask);
            var hostileColliderCount = Physics.OverlapCapsuleNonAlloc(point0, point1, radius, overlapBuffer, hostileLayerMask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < hostileColliderCount; i++)
            {
                var col = overlapBuffer[i];
                var collider = col.GetComponent<UnityEntityCollider>();
                if (!collider.Entity.IsHostile(faction))
                    continue;
                results.Add(collider);
            }

            var friendlyLayerMask = UnityCollisionHelper.ToObjectLayerMask(friendlyMask);
            var friendlyColliderCount = Physics.OverlapCapsuleNonAlloc(point0, point1, radius, overlapBuffer, friendlyLayerMask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < friendlyColliderCount; i++)
            {
                var col = overlapBuffer[i];
                var collider = col.GetComponent<UnityEntityCollider>();
                if (!collider.Entity.IsFriendly(faction))
                    continue;
                results.Add(collider);
            }
        }
        #endregion

        public SerializableUnityCollisionSystem ToSerializable()
        {
            var seri = new SerializableUnityCollisionSystem();
            var seriEntities = new List<SerializableUnityCollisionEntity>();
            foreach (var ent in entities)
            {
                if (!ent || ent.Entity == null)
                    continue;
                var entity = ent.ToSerializable();
                seriEntities.Add(entity);
            }
            seri.entities = seriEntities.ToArray();
            return seri;
        }
        public void LoadFromSerializable(LevelEngine level, SerializableUnityCollisionSystem seri)
        {
            foreach (var seriEnt in seri.entities)
            {
                var ent = level.FindEntityByID(seriEnt.id);
                if (ent == null)
                    continue;
                var colEntity = CreateCollisionEntity(ent);
                colEntity.LoadFromSerializable(seriEnt, ent);
            }
            foreach (var entity in entities)
            {
                var seriEnt = seri.entities.FirstOrDefault(e => e.id == entity.Entity.ID);
                if (seriEnt == null)
                    continue;
                entity.LoadCollisions(level, seriEnt);
            }
        }
        ISerializableCollisionSystem ICollisionSystem.ToSerializable()
        {
            return ToSerializable();
        }
        void ICollisionSystem.LoadFromSerializable(LevelEngine level, ISerializableCollisionSystem seri)
        {
            if (seri is not SerializableUnityCollisionSystem sys)
                return;
            LoadFromSerializable(level, sys);
        }
        [SerializeField]
        private GameObject collisionEntityTemplate;
        [SerializeField]
        private Transform entityRoot;
        private Collider[] overlapBuffer = new Collider[1024];
        private UnityCollisionEntity[] simulateBuffer = new UnityCollisionEntity[1024];
        private List<UnityCollisionEntity> entities = new List<UnityCollisionEntity>();
    }
    public class SerializableUnityCollisionSystem : ISerializableCollisionSystem
    {
        public SerializableUnityCollisionEntity[] entities;
    }
}
