using UnityEngine;

namespace PVZEngine.Entities
{
    public class CustomHitbox : Hitbox
    {
        public CustomHitbox(Entity entity) : base(entity)
        {
        }
        public void SetSize(Vector3 value) => size = value;
        public void SetPivot(Vector3 value) => pivot = value;
        public override SerializableHitbox ToSerializable()
        {
            var seri = new SerializableCustomHitbox(this);
            seri.size = size;
            seri.pivot = pivot;
            return seri;
        }
        public override Vector3 GetSize() => size;
        public override Vector3 GetPivot() => pivot;

        private Vector3 size;
        private Vector3 pivot;
    }
}
