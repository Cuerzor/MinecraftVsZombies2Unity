using System;
using System.Collections.Generic;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using PVZEngine;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools.Mathematics;
using UnityEngine;

namespace MVZ2.Vanilla.Detections
{
    public abstract class Detector
    {
        public bool DetectExists(DetectionParams self)
        {
            var collider = Detect(self);
            if (collider == null || collider.Entity == null || !collider.Entity.Exists())
                return false;
            return true;
        }
        public IEntityCollider Detect(DetectionParams self)
        {
            foreach (var collider in DetectColliders(self))
            {
                if (!ValidateCollider(self, collider))
                    continue;
                return collider;
            }
            return null;
        }
        public IEntityCollider DetectWithTheLeast<T>(DetectionParams self, Func<IEntityCollider, T> keySelector)
        {
            IEntityCollider least = null;
            T leastKey = default;
            var comparer = Comparer<T>.Default;
            foreach (var collider in DetectColliders(self))
            {
                if (!ValidateCollider(self, collider))
                    continue;
                var key = keySelector(collider);
                if (least != null && comparer.Compare(leastKey, key) < 0)
                    continue;
                least = collider;
                leastKey = key;
            }
            return least;
        }
        public IEntityCollider DetectWithTheMost<T>(DetectionParams self, Func<IEntityCollider, T> keySelector)
        {
            IEntityCollider most = null;
            T mostKey = default;
            var comparer = Comparer<T>.Default;
            foreach (var collider in DetectColliders(self))
            {
                if (!ValidateCollider(self, collider))
                    continue;
                var key = keySelector(collider);
                if (most != null && comparer.Compare(mostKey, key) > 0)
                    continue;
                most = collider;
                mostKey = key;
            }
            return most;
        }
        public void DetectMultiple(DetectionParams self, List<IEntityCollider> results)
        {
            foreach (var collider in DetectColliders(self))
            {
                if (!ValidateCollider(self, collider))
                    continue;
                results.Add(collider);
            }
        }
        public Entity DetectEntityWithTheLeast<T>(DetectionParams self, Func<Entity, T> keySelector)
        {
            Entity least = null;
            T leastKey = default;
            var comparer = Comparer<T>.Default;
            foreach (var collider in DetectColliders(self))
            {
                if (!ValidateCollider(self, collider))
                    continue;
                var entity = collider.Entity;
                var key = keySelector(entity);
                if (least != null && comparer.Compare(leastKey, key) < 0)
                    continue;
                least = entity;
                leastKey = key;
            }
            return least;
        }
        public Entity DetectEntityWithTheMost<T>(DetectionParams self, Func<Entity, T> keySelector)
        {
            Entity most = null;
            T mostKey = default;
            var comparer = Comparer<T>.Default;
            foreach (var collider in DetectColliders(self))
            {
                if (!ValidateCollider(self, collider))
                    continue;
                var entity = collider.Entity;
                var key = keySelector(entity);
                if (most != null && comparer.Compare(mostKey, key) > 0)
                    continue;
                most = entity;
                mostKey = key;
            }
            return most;
        }
        public void DetectEntities(DetectionParams self, List<Entity> results)
        {
            entityBuffer.Clear();
            foreach (var collider in DetectColliders(self))
            {
                if (!ValidateCollider(self, collider))
                    continue;
                var entity = collider.Entity;
                if (entityBuffer.Contains(entity))
                    continue;
                entityBuffer.Add(entity);
                results.Add(entity);
            }
        }
        public int DetectEntityCount(DetectionParams self)
        {
            entityBuffer.Clear();
            int count = 0;
            foreach (var collider in DetectColliders(self))
            {
                if (!ValidateCollider(self, collider))
                    continue;
                var entity = collider.Entity;
                if (entityBuffer.Contains(entity))
                    continue;
                entityBuffer.Add(entity);
                count++;
            }
            return count;
        }
        public virtual bool ValidateTarget(DetectionParams self, Entity target)
        {
            if (target == null)
                return false;
            if (self.entity == target && !includeSelf)
                return false;
            if (target.IsDead)
                return false;
            if (!target.IsFactionTarget(self.faction, factionTarget))
                return false;
            if (!canDetectInvisible && target.IsInvisible())
                return false;
            if (!target.IsVulnerableEntity() && (invulnerableFilter == null || !invulnerableFilter(self, target)))
                return false;
            return true;
        }
        protected abstract Bounds GetDetectionBounds(Entity self);
        protected virtual bool ValidateCollider(DetectionParams param, IEntityCollider collider)
        {
            if (!ValidateTarget(param, collider.Entity))
                return false;
            return true;
        }
        protected bool TargetInLawn(Entity target)
        {
            return TargetInLawn(target.Position.x);
        }
        protected bool TargetInLawn(float x)
        {
            return x > VanillaLevelExt.GetAttackBorderX(false) && x < VanillaLevelExt.GetAttackBorderX(true);
        }
        protected EntityDefinition GetEntityDefinition(LevelEngine level, NamespaceID entityID)
        {
            if (!definitionCaches.TryGetValue(entityID, out var cache))
            {
                cache = level.Content.GetEntityDefinition(entityID);
                definitionCaches.Add(entityID, cache);
            }
            return cache;
        }
        private List<IEntityCollider> DetectColliders(DetectionParams param)
        {
            var bounds = GetDetectionBounds(param.entity);

            var hostileMask = factionTarget == FactionTarget.Friendly ? 0 : mask;
            var friendlyMask = factionTarget == FactionTarget.Hostile ? 0 : mask;
            resultsBuffer.Clear();
            param.entity.Level.OverlapBoxNonAlloc(bounds.center, bounds.size, param.faction, hostileMask, friendlyMask, resultsBuffer);
            return resultsBuffer;
        }
        public int mask = EntityCollisionHelper.MASK_VULNERABLE;
        public FactionTarget factionTarget = FactionTarget.Hostile;
        public bool canDetectInvisible;
        public bool ignoreBoss;
        public bool includeSelf;
        public Func<DetectionParams, Entity, bool> invulnerableFilter;
        private List<IEntityCollider> resultsBuffer = new List<IEntityCollider>();
        private List<Entity> entityBuffer = new List<Entity>();
        private Dictionary<NamespaceID, EntityDefinition> definitionCaches = new Dictionary<NamespaceID, EntityDefinition>();
    }
    public struct DetectionParams
    {
        public Entity entity;
        public int faction;

        public static implicit operator DetectionParams(Entity entity)
        {
            return new DetectionParams()
            {
                entity = entity,
                faction = entity.GetFaction()
            };
        }
    }
}
