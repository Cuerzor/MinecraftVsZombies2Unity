using System;
using System.Collections.Generic;
using System.Linq;
using MVZ2.Collision;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.Collisions
{
    public class UnityEntityCollider : MonoBehaviour, IEntityCollider
    {
        public void Init(Entity entity, string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"The name of an EntityCollider cannot be null or empty.");
            Entity = entity;
            Name = name;
            gameObject.name = Name;
        }
        public void SetEnabled(bool enabled)
        {
            if (Enabled == enabled)
                return;
            Enabled = enabled;
            boxCollider.enabled = enabled;
        }
        public void UpdateBounds(Vector3 offset, Vector3 size)
        {
            boxCollider.center = offset;
            boxCollider.size = size;
        }
        public void Simulate()
        {
            foreach (var collider in touchingColliders)
            {
                if (!collider)
                    continue;
                var otherCollider = collider.GetComponent<UnityEntityCollider>();
                if (!otherCollider)
                    continue;

                var other = otherCollider.Entity;
                if (other == null)
                    continue;

                bool enter = false;
                var collision = collisionList.Find(c => (c.OtherCollider as UnityEntityCollider) == otherCollider);
                if (collision == null)
                {
                    enter = true;
                    collision = new EntityCollision(this, otherCollider);
                }
                if (CallPreCollision(collision))
                {
                    if (enter)
                    {
                        CallPostCollision(collision, EntityCollisionHelper.STATE_ENTER);
                        collisionList.Add(collision);
                    }
                    else
                    {
                        CallPostCollision(collision, EntityCollisionHelper.STATE_STAY);
                    }
                    collision.Checked = true;
                }
            }
            int collisionCount = collisionList.Count;
            collisionList.CopyTo(exitedCollisionBuffer);
            for (int i = 0; i < collisionCount; i++)
            {
                var collision = exitedCollisionBuffer[i];
                if (!collision.Checked)
                {
                    CallPostCollision(collision, EntityCollisionHelper.STATE_EXIT);
                    collisionList.Remove(collision);
                }
                collision.Checked = false;
            }
            touchingColliders.Clear();
        }
        public Bounds GetBoundingBox()
        {
            return boxCollider.bounds;
        }
        private void OnTriggerStay(Collider other)
        {
            if (touchingColliders.Contains(other))
                return;
            var otherCollider = other.GetComponent<UnityEntityCollider>();
            if (!otherCollider)
                return;
            var targetEnt = otherCollider.Entity;
            var mask = Entity.IsHostile(targetEnt) ? Entity.CollisionMaskHostile : Entity.CollisionMaskFriendly;
            if (!EntityCollisionHelper.CanCollide(mask, targetEnt))
                return;
            touchingColliders.Add(other);
        }

        #region 检测
        public bool CheckSphere(Vector3 center, float radius)
        {
            var closest = boxCollider.ClosestPoint(center);
            return (closest - (transform.position + boxCollider.center)).sqrMagnitude < radius * radius;
        }
        #endregion

        #region 碰撞
        public void GetCollisions(List<EntityCollision> collisions)
        {
            collisions.AddRange(collisionList);
        }
        private bool CallPreCollision(EntityCollision collision)
        {
            return Entity.PreCollision(collision);
        }
        private void CallPostCollision(EntityCollision collision, int state)
        {
            Entity.PostCollision(collision, state);
        }
        #endregion

        #region 序列化
        public EntityColliderReference ToReference()
        {
            return new EntityColliderReference(Entity.ID, Name);
        }
        public SerializableUnityEntityCollider ToSerializable()
        {
            return new SerializableUnityEntityCollider()
            {
                name = Name,
                enabled = Enabled,
                collisionList = collisionList.Select(c => c.ToSerializable()).ToArray(),
            };
        }
        public void LoadFromSerializable(SerializableUnityEntityCollider seri, Entity entity)
        {
            Entity = entity;
            Name = seri.name;
            gameObject.name = Name;
            SetEnabled(seri.enabled);
        }
        public void LoadCollisions(LevelEngine level, SerializableUnityEntityCollider seri)
        {
            collisionList = seri.collisionList.Select(s => EntityCollision.FromSerializable(s, level)).ToList();
        }
        #endregion

        public bool Enabled { get; private set; } = true;
        public string Name { get; private set; }
        public Entity Entity { get; private set; }
        [SerializeField]
        private BoxCollider boxCollider;
        private HashSet<Collider> touchingColliders = new HashSet<Collider>();
        private List<EntityCollision> collisionList = new List<EntityCollision>();
        private EntityCollision[] exitedCollisionBuffer = new EntityCollision[1024];
    }
    public class SerializableUnityEntityCollider
    {
        public string name;
        public bool enabled;
        public SerializableEntityCollision[] collisionList;
    }
}
