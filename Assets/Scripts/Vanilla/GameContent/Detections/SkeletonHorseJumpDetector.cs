using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using PVZEngine.Entities;
using UnityEngine;

namespace MVZ2.GameContent.Detections
{
    public class SkeletonHorseJumpDetector : Detector
    {
        protected override Bounds GetDetectionBounds(Entity self)
        {
            var sizeX = 40;
            var sizeY = 40;
            var sizeZ = 40;
            var centerX = self.Position.x + self.GetFacingX() * sizeX * 0.5f;
            var centerY = self.Position.y + sizeY * 0.5f;
            var centerZ = self.Position.z;
            return new Bounds(new Vector3(centerX, centerY, centerZ), new Vector3(sizeX, sizeY, sizeZ));
        }
        protected override bool ValidateCollider(DetectionParams param, IEntityCollider collider)
        {
            if (!base.ValidateCollider(param, collider))
                return false;
            var target = collider.Entity;
            if (target.IsFloor())
                return false;
            var bounds = collider.GetBoundingBox();
            if (!Detection.IsInFrontOf(param.entity, bounds.center.x))
                return false;
            return true;
        }
    }
}
