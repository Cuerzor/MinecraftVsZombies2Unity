using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using PVZEngine.Entities;
using UnityEngine;

namespace MVZ2.GameContent.Detections
{
    public class MagichestDetector : Detector
    {
        public MagichestDetector(float rangeAddition = 0)
        {
            mask = EntityCollisionHelper.MASK_ENEMY;
            this.rangeAddition = rangeAddition;
        }
        protected override Bounds GetDetectionBounds(Entity self)
        {
            var sizeX = self.GetRange() + rangeAddition;
            var sizeY = 48;
            var sizeZ = 48;
            var source = self.Position;
            var centerX = source.x + sizeX * 0.5f * self.GetFacingX();
            var centerY = source.y + sizeY * 0.5f;
            var centerZ = source.z;
            return new Bounds(new Vector3(centerX, centerY, centerZ), new Vector3(sizeX, sizeY, sizeZ));
        }
        protected override bool ValidateCollider(DetectionParams self, IEntityCollider collider)
        {
            if (!base.ValidateCollider(self, collider))
                return false;
            var target = collider.Entity;
            if (!TargetInLawn(target))
                return false;
            return true;
        }
        private float rangeAddition;
    }
}
