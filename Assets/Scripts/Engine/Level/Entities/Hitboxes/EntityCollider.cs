using System;
using System.Collections.Generic;
using System.Linq;
using PVZEngine.Level;
using PVZEngine.Level.Collisions;
using UnityEngine;

namespace PVZEngine.Entities
{
    public class EntityCollider : IQuadTreeNodeObject
    {
        public EntityCollider(Entity entity, string name, params Hitbox[] hitboxes)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"The name of an EntityCollider cannot be null or empty.");
            Entity = entity;
            Name = name;
            this.hitboxes.AddRange(hitboxes);
        }
        private EntityCollider()
        {
        }
        public void SetEnabled(bool enabled)
        {
            if (Enabled == enabled)
                return;
            Enabled = enabled;
            if (Enabled)
            {
                OnEnabled?.Invoke(this);
            }
            else
            {
                OnDisabled?.Invoke(this);
            }
        }

        #region 碰撞箱
        public void Update()
        {
            foreach (var hitbox in hitboxes)
            {
                hitbox.Update();
            }
            EvaluateBoundingBox();
        }
        public void DoCollision(EntityCollider other, Vector3 offset)
        {
            for (int i1 = 0; i1 < hitboxes.Count; i1++)
            {
                var hitbox1 = hitboxes[i1];
                for (int i2 = 0; i2 < other.hitboxes.Count; i2++)
                {
                    var hitbox2 = other.hitboxes[i2];
                    if (hitbox1.DoCollision(hitbox2, offset, out var seperation))
                    {
                        var collision = collisionList.Find(c => c.OtherCollider == other);
                        bool enter = false;
                        if (collision == null)
                        {
                            enter = true;
                            collision = new EntityCollision(this, other);
                        }
                        collision.Seperation = seperation;
                        if (CallPreCollision(collision))
                        {
                            if (enter)
                            {
                                collisionList.Add(collision);
                                CallPostCollision(collision, EntityCollisionHelper.STATE_ENTER);
                            }
                            else
                            {
                                CallPostCollision(collision, EntityCollisionHelper.STATE_STAY);
                            }
                            collisionThisTick.Add(collision);
                        }
                        return;
                    }
                }
            }
        }
        public void AddHitbox(Hitbox hitbox)
        {
            hitboxes.Add(hitbox);
            EvaluateBoundingBox();
        }
        public void RemoveHitbox(Hitbox hitbox)
        {
            hitboxes.Remove(hitbox);
            EvaluateBoundingBox();
        }
        public Hitbox GetHitbox(int index)
        {
            return hitboxes[index];
        }
        public int GetHitboxCount()
        {
            return hitboxes.Count;
        }
        private void EvaluateBoundingBox()
        {
            if (hitboxes.Count <= 0)
            {
                boundingBox = new Bounds(Vector3.zero, Vector3.zero);
                bottomRect = new Rect(Vector2.zero, Vector2.zero);
                return;
            }
            var min = new Vector3(10000, 10000, 10000);
            var max = new Vector3(-10000, -10000, -10000);
            foreach (var hitbox in hitboxes)
            {
                var bounds = hitbox.GetBounds();
                min = Vector3.Min(bounds.min, min);
                max = Vector3.Max(bounds.max, max);
            }
            var size = max - min;
            var center = min + size * 0.5f;
            boundingBox = new Bounds(center, size);
            bottomRect = new Rect(min.x, min.z, size.x, size.z);
        }
        public Bounds GetBoundingBox()
        {
            return boundingBox;
        }
        public Vector3 GetCenter()
        {
            return boundingBox.center;
        }
        public Rect GetCollisionRect(float rewind = 0)
        {
            if (rewind == 0)
                return bottomRect;
            var rect = bottomRect;
            var entityMotion = Entity.Position - Entity.PreviousPosition;
            var offset = -entityMotion * rewind;
            rect.x += offset.x;
            rect.y += offset.z;
            return rect;
        }
        #endregion

        #region 碰撞
        public IEnumerable<EntityCollision> GetCollisions()
        {
            return collisionList.ToArray();
        }
        public void ExitCollision()
        {
            exitBuffer.Clear();
            exitBuffer.AddRange(collisionList);
            foreach (var collision in collisionThisTick)
            {
                exitBuffer.Remove(collision);
            }
            foreach (var collision in exitBuffer)
            {
                if (collision != null)
                {
                    CallPostCollision(collision, EntityCollisionHelper.STATE_EXIT);
                }
                collisionList.Remove(collision);
            }
            collisionThisTick.Clear();
        }
        private bool CallPreCollision(EntityCollision collision)
        {
            return PreCollision?.Invoke(collision) ?? true;
        }
        private void CallPostCollision(EntityCollision collision, int state)
        {
            PostCollision?.Invoke(collision, state);
        }
        #endregion

        #region 序列化
        public EntityColliderReference ToReference()
        {
            return new EntityColliderReference(Entity.ID, Name);
        }
        public SerializableEntityCollider ToSerializable()
        {
            return new SerializableEntityCollider()
            {
                name = Name,
                enabled = Enabled,
                hitboxes = hitboxes.Select(h => h.ToSerializable()).ToArray(),
                collisionList = collisionList.Select(c => c.ToSerializable()).ToArray(),
            };
        }
        public static EntityCollider FromSerializable(SerializableEntityCollider seri, Entity entity)
        {
            var collider = new EntityCollider();
            collider.Entity = entity;
            collider.Name = seri.name;
            collider.Enabled = seri.enabled;
            collider.hitboxes = seri.hitboxes.Select(h => h.ToDeserialized(entity)).ToList();
            collider.EvaluateBoundingBox();
            return collider;
        }
        public void LoadCollisions(LevelEngine level, SerializableEntityCollider seri)
        {
            collisionList = seri.collisionList.Select(s => EntityCollision.FromSerializable(s, level)).ToList();
        }
        #endregion

        bool IEquatable<IQuadTreeNodeObject>.Equals(IQuadTreeNodeObject other)
        {
            if (other is not EntityCollider collider)
                return false;
            return this == collider;
        }

        public event Func<EntityCollision, bool> PreCollision;
        public event Action<EntityCollision, int> PostCollision;
        public event Action<EntityCollider> OnEnabled;
        public event Action<EntityCollider> OnDisabled;
        public bool Enabled { get; private set; } = true;
        public string Name { get; set; }
        public Entity Entity { get; set; }
        private Bounds boundingBox;
        private Rect bottomRect;
        private List<Hitbox> hitboxes = new List<Hitbox>();
        private List<EntityCollision> collisionThisTick = new List<EntityCollision>();
        private List<EntityCollision> collisionList = new List<EntityCollision>();
        private List<EntityCollision> exitBuffer = new List<EntityCollision>();
    }
}
