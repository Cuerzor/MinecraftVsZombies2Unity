using System;
using PVZEngine.Level;
using UnityEngine;

namespace PVZEngine.Entities
{
    public class EntityCollision
    {
        public EntityCollision(IEntityCollider collider, IEntityCollider otherCollider)
        {
            Collider = collider;
            OtherCollider = otherCollider;
        }
        public SerializableEntityCollision ToSerializable()
        {
            return new SerializableEntityCollision()
            {
                collider = new EntityColliderReference(Collider),
                otherCollider = new EntityColliderReference(OtherCollider),
                seperation = Seperation
            };
        }
        public static EntityCollision FromSerializable(SerializableEntityCollision serializable, LevelEngine level)
        {
            return new EntityCollision(serializable.collider.GetCollider(level), serializable.otherCollider.GetCollider(level))
            {
                Seperation = serializable.seperation
            };
        }
        public IEntityCollider Collider { get; set; }
        public IEntityCollider OtherCollider { get; set; }
        public Vector3 Seperation { get; set; }
        public Entity Entity => Collider.Entity;
        public Entity Other => OtherCollider.Entity;
        public bool Checked { get; set; }
    }
    [Serializable]
    public class SerializableEntityCollision
    {
        public EntityColliderReference collider;
        public EntityColliderReference otherCollider;
        public Vector3 seperation;
    }
    [Serializable]
    public class EntityColliderReference
    {
        public long entityId;
        public string unitName;

        public EntityColliderReference(IEntityCollider collider) : this(collider.Entity.ID, collider.Name)
        {
        }
        public EntityColliderReference(long entityId, string unitName)
        {
            this.entityId = entityId;
            this.unitName = unitName;
        }

        public IEntityCollider GetCollider(LevelEngine engine)
        {
            var entity = engine.FindEntityByID(entityId);
            if (entity == null)
                return null;
            return entity.GetCollider(unitName);
        }
        public override bool Equals(object obj)
        {
            if (obj is not EntityColliderReference other)
                return false;
            return entityId == other.entityId && unitName == other.unitName;
        }
        public override int GetHashCode()
        {
            return entityId.GetHashCode() * 31 + unitName.GetHashCode();
        }
        public static bool operator ==(EntityColliderReference lhs, EntityColliderReference rhs)
        {
            return lhs.Equals(rhs);
        }
        public static bool operator !=(EntityColliderReference lhs, EntityColliderReference rhs)
        {
            return !(lhs == rhs);
        }
    }
}
