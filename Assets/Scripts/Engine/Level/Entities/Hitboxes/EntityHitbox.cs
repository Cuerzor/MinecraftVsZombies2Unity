using UnityEngine;

namespace PVZEngine.Entities
{
    public class EntityHitbox : Hitbox
    {
        public EntityHitbox(Entity entity) : base(entity)
        {
        }
        public override SerializableHitbox ToSerializable()
        {
            return new SerializableEntityHitbox(this);
        }
        public override Vector3 GetSize() => Entity.Cache.Size;
        public override Vector3 GetPivot() => Entity.Cache.BoundsPivot;
    }
}
