using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

                ent1.ExitCollision();
            }
        }
        public void AddEntity(Entity entity)
        {
            for (int i = 0; i < entity.GetEnabledColliderCount(); i++)
            {
                var collider = entity.GetEnabledColliderAt(i);
                var flag = entity.TypeCollisionFlag;
                InsertCollider(flag, collider);
            }
            entity.OnColliderEnabled += OnColliderEnabledCallback;
            entity.OnColliderDisabled += OnColliderDisabledCallback;
        }
        public void RemoveEntity(Entity entity)
        {
            for (int i = 0; i < entity.GetEnabledColliderCount(); i++)
            {
                var collider = entity.GetEnabledColliderAt(i);
                var flag = entity.TypeCollisionFlag;
                RemoveCollider(flag, collider);
            }
            entity.OnColliderEnabled -= OnColliderEnabledCallback;
            entity.OnColliderDisabled -= OnColliderDisabledCallback;
        }
        public QuadTreeCollider GetCollisionQuadTree(int flag)
        {
            if (quadTrees.TryGetValue(flag, out var tree))
                return tree;
            return null;
        }

        #region 检测
        public EntityCollider[] OverlapBox(Vector3 center, Vector3 size, int faction, int hostileMask, int friendlyMask)
        {
            var min = center - size * 0.5f;
            var filterRect = new Rect(min.x, min.z, size.x, size.y);
            var bounds = new Bounds(center, size);
            return Overlap(filterRect, faction, hostileMask, friendlyMask, h => bounds.Intersects(h.GetBounds()));
        }
        public void OverlapBoxNonAlloc(Vector3 center, Vector3 size, int faction, int hostileMask, int friendlyMask, List<EntityCollider> results)
        {
            var min = center - size * 0.5f;
            var filterRect = new Rect(min.x, min.z, size.x, size.y);
            var bounds = new Bounds(center, size);
            OverlapNonAlloc(filterRect, faction, hostileMask, friendlyMask, h => bounds.Intersects(h.GetBounds()), results);
        }
        public EntityCollider[] OverlapSphere(Vector3 center, float radius, int faction, int hostileMask, int friendlyMask)
        {
            var min = center - Vector3.one * radius;
            var filterRect = new Rect(min.x, min.z, radius * 2, radius * 2);
            return Overlap(filterRect, faction, hostileMask, friendlyMask, h => MathTool.CollideBetweenCubeAndSphere(center, radius, h.GetBoundsCenter(), h.GetBoundsSize()));
        }
        public void OverlapSphereNonAlloc(Vector3 center, float radius, int faction, int hostileMask, int friendlyMask, List<EntityCollider> results)
        {
            var min = center - Vector3.one * radius;
            var filterRect = new Rect(min.x, min.z, radius * 2, radius * 2);
            OverlapNonAlloc(filterRect, faction, hostileMask, friendlyMask, h => MathTool.CollideBetweenCubeAndSphere(center, radius, h.GetBoundsCenter(), h.GetBoundsSize()), results);
        }
        public EntityCollider[] OverlapCapsule(Vector3 center, float radius, float height, int faction, int hostileMask, int friendlyMask)
        {
            var min = center - Vector3.one * radius;
            var filterRect = new Rect(min.x, min.z, radius * 2, radius * 2);
            var capsule = new Capsule(Axis.Y, center, height, radius);
            return Overlap(filterRect, faction, hostileMask, friendlyMask, h => MathTool.CollideBetweenCubeAndCapsule(capsule, h.GetBoundsCenter(), h.GetBoundsSize()));
        }
        public void OverlapCapsuleNonAlloc(Vector3 center, float radius, float height, int faction, int hostileMask, int friendlyMask, List<EntityCollider> results)
        {
            var min = center - Vector3.one * radius;
            var filterRect = new Rect(min.x, min.z, radius * 2, radius * 2);
            var capsule = new Capsule(Axis.Y, center, height, radius);
            OverlapNonAlloc(filterRect, faction, hostileMask, friendlyMask, h => MathTool.CollideBetweenCubeAndCapsule(capsule, h.GetBoundsCenter(), h.GetBoundsSize()), results);
        }
        public EntityCollider[] Overlap(Rect filterRect, int faction, int hostileMask, int friendlyMask, Predicate<Hitbox> predicate)
        {
            if (predicate == null)
                return Array.Empty<EntityCollider>();
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
        private void OverlapNonAlloc(Rect filterRect, int faction, int hostileMask, int friendlyMask, Predicate<Hitbox> predicate, List<EntityCollider> results)
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
        private void InsertCollider(int flag, EntityCollider collider)
        {
            if (!quadTrees.TryGetValue(flag, out var tree))
            {
                tree = CreateQuadTree();
                quadTrees.Add(flag, tree);
            }
            tree.Insert(collider);
        }
        private void RemoveCollider(int flag, EntityCollider collider)
        {
            if (!quadTrees.TryGetValue(flag, out var tree))
            {
                return;
            }
            tree.Remove(collider);
        }
        private void OnColliderEnabledCallback(EntityCollider collider)
        {
            InsertCollider(collider.Entity.TypeCollisionFlag, collider);
        }
        private void OnColliderDisabledCallback(EntityCollider collider)
        {
            RemoveCollider(collider.Entity.TypeCollisionFlag, collider);
        }
        private QuadTreeCollider CreateQuadTree()
        {
            return new QuadTreeCollider(quadTreeParams.size, quadTreeParams.maxObjects, quadTreeParams.maxDepth);
        }
        public LevelEngine level;
        private List<EntityCollider> colliderBuffer = new List<EntityCollider>();
        private List<EntityCollider> collisionBuffer = new List<EntityCollider>();
        private Dictionary<int, QuadTreeCollider> quadTrees = new Dictionary<int, QuadTreeCollider>();
        private QuadTreeParams quadTreeParams;
    }
}
