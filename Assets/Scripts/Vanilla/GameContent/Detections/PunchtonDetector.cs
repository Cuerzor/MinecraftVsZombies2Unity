using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using PVZEngine.Entities;
using UnityEngine;

namespace MVZ2.GameContent.Detections
{
    public class PunchtonDetector : Detector
    {
        public PunchtonDetector()
        {
            mask = EntityCollisionHelper.MASK_PLANT | EntityCollisionHelper.MASK_ENEMY | EntityCollisionHelper.MASK_OBSTACLE;
        }
        protected override Bounds GetDetectionBounds(Entity self)
        {
            var sizeX = self.GetRange();
            if (infiniteRange)
            {
                sizeX = 800;
            }
            var sizeY = 48;
            var sizeZ = 48;
            var source = self.Position;
            var centerX = source.x + sizeX * 0.5f * self.GetFacingX();
            var centerY = source.y + sizeY * 0.5f;
            var centerZ = source.z;
            return new Bounds(new Vector3(centerX, centerY, centerZ), new Vector3(sizeX, sizeY, sizeZ));
        }
        protected override bool ValidateCollider(DetectionParams param, IEntityCollider collider)
        {
            if (!base.ValidateCollider(param, collider))
                return false;
            var target = collider.Entity;
            if (!TargetInLawn(target))
                return false;
            return true;
        }
        public bool infiniteRange = false;
    }
}
