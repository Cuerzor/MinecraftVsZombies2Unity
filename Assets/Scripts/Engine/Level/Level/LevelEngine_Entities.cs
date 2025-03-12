using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using PVZEngine.Entities;
using PVZEngine.Level.Collisions;
using Tools.Mathematics;
using UnityEngine;

namespace PVZEngine.Level
{
    public partial class LevelEngine
    {
        private void AddEntity(Entity entity)
        {
        }
        internal void RemoveEntity(Entity entity)
        {
            entities.Remove(entity);
            entityTrash.Add(entity);
            collisionSystem.RemoveEntity(entity);
            OnEntityRemove?.Invoke(entity);
        }
        public Entity Spawn(EntityDefinition entityDef, Vector3 pos, Entity spawner, SpawnParams param = null)
        {
            return Spawn(entityDef, pos, spawner, entityRandom.Next(), param);
        }
        public Entity Spawn(NamespaceID entityRef, Vector3 pos, Entity spawner, SpawnParams param = null)
        {
            var entityDef = Content.GetEntityDefinition(entityRef);
            if (entityDef == null)
                return null;
            return Spawn(entityDef, pos, spawner, param);
        }
        public Entity Spawn(EntityDefinition entityDef, Vector3 pos, Entity spawner, int seed, SpawnParams param = null)
        {
            long id = AllocEntityID();
            var spawned = new Entity(this, id, new EntityReferenceChain(spawner), entityDef, seed);
            spawned.Position = pos;
            if (param != null)
            {
                param.Apply(spawned);
            }
            entities.Add(spawned);
            OnEntitySpawn?.Invoke(spawned);
            spawned.Init(spawner);
            collisionSystem.AddEntity(spawned);
            return spawned;
        }
        public Entity Spawn(NamespaceID entityRef, Vector3 pos, Entity spawner, int seed, SpawnParams param = null)
        {
            var entityDef = Content.GetEntityDefinition(entityRef);
            if (entityDef == null)
                return null;
            return Spawn(entityDef, pos, spawner, seed, param);
        }

        #region 查询
        public Entity[] GetEntities(params int[] filterTypes)
        {
            if (filterTypes == null || filterTypes.Length <= 0)
                return entities.ToArray();
            return FindEntities(predicate);

            bool predicate(Entity e)
            {
                return filterTypes.Contains(e.Type);
            }
        }
        public Entity[] FindEntities(EntityDefinition def)
        {
            if (def == null)
                return Array.Empty<Entity>();
            return FindEntities(predicate);

            bool predicate(Entity e)
            {
                return e.Definition == def;
            }
        }
        public Entity[] FindEntities(NamespaceID id)
        {
            if (!NamespaceID.IsValid(id))
                return Array.Empty<Entity>();
            return FindEntities(predicate);

            bool predicate(Entity e)
            {
                return e.IsEntityOf(id);
            }
        }
        public Entity[] FindEntities(Func<Entity, bool> predicate)
        {
            return entities.Where(predicate).ToArray();
        }
        public int GetEntityCount(EntityDefinition def)
        {
            if (def == null)
                return 0;
            return GetEntityCount(predicate);

            bool predicate(Entity e)
            {
                return e.Definition == def;
            }
        }
        public int GetEntityCount(NamespaceID id)
        {
            if (!NamespaceID.IsValid(id))
                return 0;
            return GetEntityCount(predicate);

            bool predicate(Entity e)
            {
                return e.IsEntityOf(id);
            }
        }
        public int GetEntityCount(Func<Entity, bool> predicate)
        {
            int count = 0;
            foreach (var entity in entities)
            {
                if (predicate(entity))
                {
                    count++;
                }
            }
            return count;
        }
        public void FindEntitiesNonAlloc(Func<Entity, bool> predicate, List<Entity> results)
        {
            foreach (var entity in entities)
            {
                if (predicate(entity))
                {
                    results.Add(entity);
                }
            }
        }
        public Entity FindEntityByID(long id)
        {
            foreach (var entity in entities)
            {
                if (entity.ID == id)
                    return entity;
            }
            return FindEntityInTrash(id);
        }
        public Entity FindFirstEntity(EntityDefinition def)
        {
            if (def == null)
                return null;
            return FindFirstEntity(predicate);

            bool predicate(Entity e)
            {
                return e.Definition == def;
            }
        }
        public Entity FindFirstEntity(NamespaceID id)
        {
            if (!NamespaceID.IsValid(id))
                return null;
            return FindFirstEntity(predicate);

            bool predicate(Entity e)
            {
                return e.IsEntityOf(id);
            }
        }
        public Entity FindFirstEntity(Func<Entity, bool> predicate)
        {
            foreach (var entity in entities)
            {
                if (predicate(entity))
                    return entity;
            }
            return null;
        }
        public Entity FindFirstEntityWithTheLeast<TKey>(Func<Entity, bool> predicate, Func<Entity, TKey> keySelector)
        {
            Entity targetEntity = null;
            TKey targetKey = default;
            var comparer = Comparer<TKey>.Default;
            foreach (var entity in entities)
            {
                if (!predicate(entity))
                    continue;
                var key = keySelector(entity);
                if (targetEntity != null && comparer.Compare(targetKey, key) < 0)
                    continue;
                targetEntity = entity;
                targetKey = key;
            }
            return targetEntity;
        }
        public Entity FindFirstEntityWithTheMost<TKey>(Func<Entity, bool> predicate, Func<Entity, TKey> keySelector)
        {
            Entity targetEntity = null;
            TKey targetKey = default;
            var comparer = Comparer<TKey>.Default;
            foreach (var entity in entities)
            {
                if (!predicate(entity))
                    continue;
                var key = keySelector(entity);
                if (targetEntity != null && comparer.Compare(targetKey, key) > 0)
                    continue;
                targetEntity = entity;
                targetKey = key;
            }
            return targetEntity;
        }
        public bool EntityExists(long id)
        {
            return entities.Exists(predicate);

            bool predicate(Entity e)
            {
                return e.ID == id;
            }
        }
        public bool EntityExists(EntityDefinition def)
        {
            return entities.Exists(predicate);

            bool predicate(Entity e)
            {
                return e.Definition == def;
            }
        }
        public bool EntityExists(NamespaceID id)
        {
            return entities.Exists(predicate);

            bool predicate(Entity e)
            {
                return e.IsEntityOf(id);
            }
        }
        public bool EntityExists(Predicate<Entity> predicate)
        {
            return entities.Exists(predicate);
        }
        #endregion

        #region 碰撞检测
        public EntityCollider[] OverlapBox(Vector3 center, Vector3 size, int faction, int hostileMask, int friendlyMask)
        {
            return collisionSystem.OverlapBox(center, size, faction, hostileMask, friendlyMask);
        }
        public void OverlapBoxNonAlloc(Vector3 center, Vector3 size, int faction, int hostileMask, int friendlyMask, List<EntityCollider> results)
        {
            collisionSystem.OverlapBoxNonAlloc(center, size, faction, hostileMask, friendlyMask, results);
        }
        public EntityCollider[] OverlapSphere(Vector3 center, float radius, int faction, int hostileMask, int friendlyMask)
        {
            return collisionSystem.OverlapSphere(center, radius, faction, hostileMask, friendlyMask);
        }
        public void OverlapSphereNonAlloc(Vector3 center, float radius, int faction, int hostileMask, int friendlyMask, List<EntityCollider> results)
        {
            collisionSystem.OverlapSphereNonAlloc(center, radius, faction, hostileMask, friendlyMask, results);
        }
        public EntityCollider[] OverlapCapsule(Vector3 center, float radius, float height, int faction, int hostileMask, int friendlyMask)
        {
            return collisionSystem.OverlapCapsule(center, radius, height, faction, hostileMask, friendlyMask);
        }
        public void OverlapCapsuleNonAlloc(Vector3 center, float radius, float height, int faction, int hostileMask, int friendlyMask, List<EntityCollider> results)
        {
            collisionSystem.OverlapCapsuleNonAlloc(center, radius, height, faction, hostileMask, friendlyMask, results);
        }
        #endregion
        private long AllocEntityID()
        {
            long id = currentEntityID;
            currentEntityID++;
            return id;
        }
        private Entity FindEntityInTrash(long id)
        {
            foreach (var entity in entityTrash)
            {
                if (entity.ID == id)
                    return entity;
            }
            return null;
        }
        private void ClearEntityTrash()
        {
            entityTrash.Clear();
        }

        private void UpdateEntities()
        {
            entityUpdateBuffer.Clear();
            entityUpdateBuffer.AddRange(entities);
            foreach (var entity in entityUpdateBuffer)
            {
                entity.Update();
            }
        }
        #region 碰撞
        private void CollisionUpdate()
        {
            collisionSystem.Update();
        }
        #endregion

        #region 事件
        public Action<Entity> OnEntitySpawn;
        public Action<Entity> OnEntityRemove;
        #endregion
        private long currentEntityID = 1;
        private List<Entity> entities = new List<Entity>();
        private List<Entity> entityTrash = new List<Entity>();
        private List<Entity> entityUpdateBuffer = new List<Entity>();
        private ICollisionSystem collisionSystem;
    }
}