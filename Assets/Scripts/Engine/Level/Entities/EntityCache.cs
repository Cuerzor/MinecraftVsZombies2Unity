using Tools;
using UnityEngine;

namespace PVZEngine.Entities
{
    public class EntityCache
    {
        public void UpdateAll(Entity entity)
        {
            Faction = entity.GetProperty<int>(EngineEntityProps.FACTION);
            Gravity = entity.GetGravity();
            Friction = entity.GetFriction();
            GroundLimitOffset = entity.GetGroundLimitOffset();
            VelocityDampen = entity.GetVelocityDampen();
            Size = entity.GetSize();
            Scale = entity.GetScale();
            BoundsPivot = entity.GetBoundsPivot();
            CollisionDetection = entity.GetCollisionDetection();
            CollisionSampleLength = entity.GetCollisionSampleLength();
        }
        public void UpdateProperty(Entity entity, PropertyKey name, object value)
        {
            if (name == EngineEntityProps.FACTION)
            {
                Faction = value.ToGeneric<int>();
            }
            else if (name == EngineEntityProps.GRAVITY)
            {
                Gravity = value.ToGeneric<float>();
            }
            else if (name == EngineEntityProps.FRICTION)
            {
                Friction = value.ToGeneric<float>();
            }
            else if (name == EngineEntityProps.GROUND_LIMIT_OFFSET)
            {
                GroundLimitOffset = value.ToGeneric<float>();
            }
            else if (name == EngineEntityProps.VELOCITY_DAMPEN)
            {
                VelocityDampen = value.ToGeneric<Vector3>();
            }
            else if (name == EngineEntityProps.SIZE)
            {
                Size = value.ToGeneric<Vector3>();
            }
            else if (name == EngineEntityProps.SCALE)
            {
                Scale = value.ToGeneric<Vector3>();
            }
            else if (name == EngineEntityProps.BOUNDS_PIVOT)
            {
                BoundsPivot = value.ToGeneric<Vector3>();
            }
            else if (name == EngineEntityProps.COLLISION_DETECTION)
            {
                CollisionDetection = value.ToGeneric<int>();
            }
            else if (name == EngineEntityProps.COLLISION_SAMPLE_LENGTH)
            {
                CollisionSampleLength = value.ToGeneric<float>();
            }
        }
        public int Faction { get; private set; }
        public float Gravity { get; private set; }
        public float Friction { get; private set; }
        public float GroundLimitOffset { get; private set; }
        public Vector3 VelocityDampen { get; private set; }
        public Vector3 Size { get; private set; }
        public Vector3 Scale { get; private set; }
        public Vector3 BoundsPivot { get; private set; }
        public int CollisionDetection { get; private set; }
        public float CollisionSampleLength { get; private set; }
    }
}
