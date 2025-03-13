using Tools.Mathematics;
using UnityEngine;

namespace PVZEngine.Entities
{
    public abstract class Hitbox
    {
        public Hitbox(Entity entity)
        {
            Entity = entity;
        }
        public void ReevaluateBounds()
        {
            var scale = Entity.GetScale();

            Vector3 pivot = GetPivot();
            Vector3 size = GetSize();
            size.Scale(scale);

            var center = Vector3.Scale(Vector3.one * 0.5f - pivot, size);

            size.x = Mathf.Abs(size.x);
            size.y = Mathf.Abs(size.y);
            size.z = Mathf.Abs(size.z);
            cache = new Bounds(center, size);
        }
        public bool IsInSphere(Vector3 center, float radius)
        {
            var bounds = GetBounds();
            return MathTool.CollideBetweenCubeAndSphere(center, radius, bounds.center, bounds.size);
        }
        public Vector3 GetBoundsCenter()
        {
            return GetLocalCenter() + Entity.Position;
        }
        public Bounds GetBounds()
        {
            var bounds = GetLocalBounds();
            bounds.center += Entity.Position;
            return bounds;
        }
        public Vector3 GetBoundsSize()
        {
            return cache.size;
        }
        public Bounds GetLocalBounds()
        {
            return cache;
        }
        public Vector3 GetLocalCenter()
        {
            return cache.center;
        }
        public bool Intersects(Hitbox other)
        {
            var bounds = GetBounds();
            var otherBounds = other.GetBounds();
            return bounds.Intersects(otherBounds);
        }
        public bool DoCollision(Hitbox other, Vector3 offset, out Vector3 seperation)
        {
            var selfBounds = GetBounds();
            selfBounds.center += offset;
            var otherBounds = other.GetBounds();
                
            if (selfBounds.Intersects(otherBounds))
            {
                seperation = otherBounds.center - selfBounds.center;
                return true;
            }
            seperation = Vector3.zero;
            return false;
        }
        public abstract Vector3 GetSize();
        public abstract Vector3 GetPivot();
        public abstract SerializableHitbox ToSerializable();
        public Entity Entity { get; }
        private Bounds cache;
    }
}
