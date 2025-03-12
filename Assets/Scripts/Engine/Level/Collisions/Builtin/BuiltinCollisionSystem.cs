using System;
using System.Collections.Generic;
using PVZEngine.Entities;
using Tools.Mathematics;
using UnityEngine;

namespace PVZEngine.Level.Collisions
{
    public class BuiltinCollisionSystem : ICollisionSystem
    {
        public BuiltinCollisionSystem(QuadTreeParams treeParams)
        {
            quadTreeParams = treeParams;
        }
        public void Update()
        {
            colliderBuffer.Clear();
            foreach (var quadTree in quadTrees.Values)
            {
                quadTree.Update();
                quadTree.GetAllTargets(colliderBuffer);
            }

            foreach (var collider1 in colliderBuffer)
            {
                var ent1 = collider1.Entity;
                var detection = ent1.Cache.CollisionDetection;
                if (detection == EntityCollisionHelper.DETECTION_IGNORE)
                    continue;
                int maskHostile = ent1.CollisionMaskHostile;
                int maskFriendly = ent1.CollisionMaskFriendly;
                var maskTotal = maskHostile | maskFriendly;
                int ent1Faction = ent1.Cache.Faction;

                var ent1Motion = ent1.Position - ent1.PreviousPosition;


                var rect1 = collider1.GetCollisionRect();
                var collisionPoints = 1;
                if (detection == EntityCollisionHelper.DETECTION_CONTINUOUS)
                {
                    collisionPoints = Mathf.CeilToInt(ent1Motion.magnitude / ent1.Cache.CollisionSampleLength);
                    collisionPoints = Mathf.Max(collisionPoints, 1);
                }
                for (int p = collisionPoints - 1; p >= 0; p--) // 从上一帧的位置开始向当前位置回溯
                {
                    var rewind = p / (float)collisionPoints;
                    var ent1Offset = -ent1Motion * rewind;
                    var offsetedRect = rect1;
                    offsetedRect.x += ent1Offset.x;
                    offsetedRect.y += ent1Offset.z;

                    collisionBuffer.Clear();
                    foreach (var pair in quadTrees)
                    {
                        var flag = pair.Key;
                        if ((flag & maskTotal) == 0)
                            continue;
                        var tree = pair.Value;
                        tree.FindTargetsInRect(offsetedRect, collisionBuffer, rewind);
                    }
                    foreach (var collider2 in collisionBuffer)
                    {
                        if (collider1 == collider2)
                            continue;
                        var ent2 = collider2.Entity;
                        if (ent1 == ent2)
                            continue;
                        var detection2 = ent2.Cache.CollisionDetection;
                        if (detection2 == EntityCollisionHelper.DETECTION_IGNORE)
                            continue;
                        var ent2Faction = ent2.Cache.Faction;
                        var mask = EngineEntityExt.IsHostile(ent1Faction, ent2Faction) ? ent1.CollisionMaskHostile : ent1.CollisionMaskFriendly;
                        if (!EntityCollisionHelper.CanCollide(mask, ent2))
                            continue;
                        var ent2Motion = ent2.Position - ent2.PreviousPosition;
                        var ent2Offset = -ent2Motion * rewind;

                        collider1.DoCollision(collider2, ent1Offset - ent2Offset);
                    }
                }

                ExitCollision(ent1);
            }
        }
        public void InitEntity(Entity entity)
        {
            var mainCollider = new EntityCollider(entity, EntityCollisionHelper.NAME_MAIN, new EntityHitbox(entity));
            AddCollider(entity, mainCollider);
        }
        public void DestroyEntity(Entity entity)
        {
            if (!entityColliders.TryGetValue(entity.ID, out var colliders))
                return;
            var flag = entity.TypeCollisionFlag;
            foreach (var collider in colliders)
            {
                collider.OnEnabled -= OnColliderEnabledCallback;
                collider.OnDisabled -= OnColliderDisabledCallback;
                if (collider.Enabled)
                {
                    RemoveColliderFromTree(flag, collider);
                }
            }
            colliders.Clear();
            entityColliders.Remove(entity.ID);
        }
        public void GetCurrentCollisions(Entity entity, List<EntityCollision> collisions)
        {
            if (!entityColliders.TryGetValue(entity.ID, out var colliders))
                return;
            foreach (var collider in colliders)
            {
                collider.GetCollisions(collisions);
            }
        }
        private void ExitCollision(Entity entity)
        {
            if (!entityColliders.TryGetValue(entity.ID, out var colliders))
                return;
            foreach (var collider in colliders)
            {
                collider.ExitCollision();
            }
        }


        #region 碰撞体
        public void AddCollider(Entity entity, IEntityCollider collider)
        {
            if (collider is not EntityCollider entCol)
                return;
            var flag = entity.TypeCollisionFlag;
            InsertColliderToTree(flag, entCol);
            if (!entityColliders.TryGetValue(entity.ID, out var colliders))
            {
                colliders = new List<EntityCollider>();
                entityColliders.Add(entity.ID, colliders);
            }
            colliders.Add(entCol);
            entCol.OnEnabled += OnColliderEnabledCallback;
            entCol.OnDisabled += OnColliderDisabledCallback;
        }
        public bool RemoveCollider(Entity entity, string name)
        {
            if (entityColliders.TryGetValue(entity.ID, out var colliders))
            {
                foreach (var collider in colliders)
                {
                    if (collider.Name == name)
                    {
                        colliders.Remove(collider);
                        collider.OnEnabled -= OnColliderEnabledCallback;
                        collider.OnDisabled -= OnColliderDisabledCallback;
                        if (collider.Enabled)
                        {
                            var flag = entity.TypeCollisionFlag;
                            RemoveColliderFromTree(flag, collider);
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        public EntityCollider GetCollider(Entity entity, string name)
        {
            if (entityColliders.TryGetValue(entity.ID, out var colliders))
            {
                foreach (var collider in colliders)
                {
                    if (collider.Name == name)
                        return collider;
                }
            }
            return null;
        }
        IEntityCollider ICollisionSystem.GetCollider(Entity entity, string name) => GetCollider(entity, name);
        #endregion

        #region 检测
        public IEntityCollider[] OverlapBox(Vector3 center, Vector3 size, int faction, int hostileMask, int friendlyMask)
        {
            var min = center - size * 0.5f;
            var filterRect = new Rect(min.x, min.z, size.x, size.y);
            var bounds = new Bounds(center, size);
            return Overlap(filterRect, faction, hostileMask, friendlyMask, h => bounds.Intersects(h.GetBounds()));
        }
        public void OverlapBoxNonAlloc(Vector3 center, Vector3 size, int faction, int hostileMask, int friendlyMask, List<IEntityCollider> results)
        {
            var min = center - size * 0.5f;
            var filterRect = new Rect(min.x, min.z, size.x, size.z);
            var bounds = new Bounds(center, size);
            OverlapNonAlloc(filterRect, faction, hostileMask, friendlyMask, h => bounds.Intersects(h.GetBounds()), results);
        }
        public IEntityCollider[] OverlapSphere(Vector3 center, float radius, int faction, int hostileMask, int friendlyMask)
        {
            var min = center - Vector3.one * radius;
            var filterRect = new Rect(min.x, min.z, radius * 2, radius * 2);
            return Overlap(filterRect, faction, hostileMask, friendlyMask, h => MathTool.CollideBetweenCubeAndSphere(center, radius, h.GetBoundsCenter(), h.GetBoundsSize()));
        }
        public void OverlapSphereNonAlloc(Vector3 center, float radius, int faction, int hostileMask, int friendlyMask, List<IEntityCollider> results)
        {
            var min = center - Vector3.one * radius;
            var filterRect = new Rect(min.x, min.z, radius * 2, radius * 2);
            OverlapNonAlloc(filterRect, faction, hostileMask, friendlyMask, h => MathTool.CollideBetweenCubeAndSphere(center, radius, h.GetBoundsCenter(), h.GetBoundsSize()), results);
        }
        public IEntityCollider[] OverlapCapsule(Vector3 point0, Vector3 point1, float radius, int faction, int hostileMask, int friendlyMask)
        {
            var center = (point1 + point0) * 0.5f;
            var min = center - Vector3.one * radius;
            var filterRect = new Rect(min.x, min.z, radius * 2, radius * 2);
            var capsule = new Capsule(point0, point1, radius);
            return Overlap(filterRect, faction, hostileMask, friendlyMask, h => MathTool.CollideBetweenCubeAndCapsule(capsule, h.GetBoundsCenter(), h.GetBoundsSize()));
        }
        public void OverlapCapsuleNonAlloc(Vector3 point0, Vector3 point1, float radius, int faction, int hostileMask, int friendlyMask, List<IEntityCollider> results)
        {
            var center = (point1 + point0) * 0.5f;
            var min = center - Vector3.one * radius;
            var filterRect = new Rect(min.x, min.z, radius * 2, radius * 2);
            var capsule = new Capsule(point0, point1, radius);
            OverlapNonAlloc(filterRect, faction, hostileMask, friendlyMask, h => MathTool.CollideBetweenCubeAndCapsule(capsule, h.GetBoundsCenter(), h.GetBoundsSize()), results);
        }
        public IEntityCollider[] Overlap(Rect filterRect, int faction, int hostileMask, int friendlyMask, Predicate<Hitbox> predicate)
        {
            if (predicate == null)
                return Array.Empty<IEntityCollider>();
            var entityColliders = new List<EntityCollider>();
            var totalMask = hostileMask | friendlyMask;
            FindCollidersRange(totalMask, filterRect, entityColliders);
            for (int c = entityColliders.Count - 1; c >= 0; c--)
            {
                var collider = entityColliders[c];
                var entity = collider.Entity;
                var mask = entity.IsHostile(faction) ? hostileMask : friendlyMask;
                bool valid = false;
                if (EntityCollisionHelper.CanCollide(mask, entity) && entity.GetCollisionDetection() != EntityCollisionHelper.DETECTION_IGNORE)
                {
                    for (int i = 0; i < collider.GetHitboxCount(); i++)
                    {
                        var hitbox = collider.GetHitbox(i);
                        if (predicate(hitbox))
                        {
                            valid = true;
                            break;
                        }
                    }
                }
                if (!valid)
                {
                    entityColliders.RemoveAt(c);
                    continue;
                }
            }
            return entityColliders.ToArray();
        }
        private void OverlapNonAlloc(Rect filterRect, int faction, int hostileMask, int friendlyMask, Predicate<Hitbox> predicate, List<IEntityCollider> results)
        {
            if (predicate == null)
                return;
            var entityColliders = new List<EntityCollider>();
            var totalMask = hostileMask | friendlyMask;
            FindCollidersRange(totalMask, filterRect, entityColliders);
            foreach (var collider in entityColliders)
            {
                if (results.Contains(collider))
                    continue;
                var entity = collider.Entity;
                var mask = entity.IsHostile(faction) ? hostileMask : friendlyMask;
                if (!EntityCollisionHelper.CanCollide(mask, entity))
                    continue;
                if (entity.GetCollisionDetection() == EntityCollisionHelper.DETECTION_IGNORE)
                    continue;
                for (int i = 0; i < collider.GetHitboxCount(); i++)
                {
                    var hitbox = collider.GetHitbox(i);
                    if (predicate(hitbox))
                    {
                        results.Add(collider);
                        break;
                    }
                }
            }
        }
        #endregion

        #region 四叉树
        public QuadTreeCollider GetCollisionQuadTree(int flag)
        {
            if (quadTrees.TryGetValue(flag, out var tree))
                return tree;
            return null;
        }
        private void FindCollidersRange(int mask, Rect rect, List<EntityCollider> collider)
        {
            foreach (var pair in quadTrees)
            {
                var flag = pair.Key;
                if ((flag & mask) == 0)
                    continue;
                var quadTree = pair.Value;
                quadTree.FindTargetsInRect(rect, collider);
            }
        }
        private void InsertColliderToTree(int flag, EntityCollider collider)
        {
            if (!quadTrees.TryGetValue(flag, out var tree))
            {
                tree = CreateQuadTree();
                quadTrees.Add(flag, tree);
            }
            tree.Insert(collider);
        }
        private void RemoveColliderFromTree(int flag, EntityCollider collider)
        {
            if (!quadTrees.TryGetValue(flag, out var tree))
            {
                return;
            }
            tree.Remove(collider);
        }
        private QuadTreeCollider CreateQuadTree()
        {
            return new QuadTreeCollider(quadTreeParams.size, quadTreeParams.maxObjects, quadTreeParams.maxDepth);
        }
        #endregion

        #region 回调
        private void OnColliderEnabledCallback(EntityCollider collider)
        {
            InsertColliderToTree(collider.Entity.TypeCollisionFlag, collider);
        }
        private void OnColliderDisabledCallback(EntityCollider collider)
        {
            RemoveColliderFromTree(collider.Entity.TypeCollisionFlag, collider);
        }
        #endregion

        public SerializableBuiltinCollisionSystem ToSerializable()
        {
            var seri = new SerializableBuiltinCollisionSystem();
            var entities = new List<SerializableBuiltinCollisionSystemEntity>();
            foreach (var pair in entityColliders)
            {
                var entity = new SerializableBuiltinCollisionSystemEntity()
                {
                    id = pair.Key,
                    colliders = pair.Value.ConvertAll(g => g.ToSerializable()).ToArray()
                };
                entities.Add(entity);
            }
            seri.entities = entities.ToArray();
            return seri;
        }
        public void LoadFromSerializable(LevelEngine level, SerializableBuiltinCollisionSystem seri)
        {
            foreach (var seriEnt in seri.entities)
            {
                var colliders = new List<EntityCollider>();
                entityColliders.Add(seriEnt.id, colliders);
                var ent = level.FindEntityByID(seriEnt.id);
                foreach (var seriCollider in seriEnt.colliders)
                {
                    var collider = EntityCollider.FromSerializable(seriCollider, ent);
                    colliders.Add(collider);
                }
            }
            foreach (var seriEnt in seri.entities)
            {
                var colliders = entityColliders[seriEnt.id];
                for (int i = 0; i < colliders.Count; i++)
                {
                    var collider = colliders[i];
                    var seriCollider = seriEnt.colliders[i];
                    collider.LoadCollisions(level, seriCollider);
                }
            }
        }
        ISerializableCollisionSystem ICollisionSystem.ToSerializable()
        {
            return ToSerializable();
        }
        void ICollisionSystem.LoadFromSerializable(LevelEngine level, ISerializableCollisionSystem seri)
        {
            if (seri is not SerializableBuiltinCollisionSystem sys)
                return;
            LoadFromSerializable(level, sys);
        }


        private List<EntityCollider> colliderBuffer = new List<EntityCollider>();
        private List<EntityCollider> collisionBuffer = new List<EntityCollider>();
        private Dictionary<int, QuadTreeCollider> quadTrees = new Dictionary<int, QuadTreeCollider>();
        private QuadTreeParams quadTreeParams;

        private Dictionary<long, List<EntityCollider>> entityColliders = new Dictionary<long, List<EntityCollider>>();
    }
    public class SerializableBuiltinCollisionSystem : ISerializableCollisionSystem
    {
        public SerializableBuiltinCollisionSystemEntity[] entities;
    }
    public class SerializableBuiltinCollisionSystemEntity
    {
        public long id;
        public SerializableEntityCollider[] colliders;
    }
}
