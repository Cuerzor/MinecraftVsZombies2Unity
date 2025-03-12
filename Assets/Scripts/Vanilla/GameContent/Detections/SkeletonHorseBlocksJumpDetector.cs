using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Detections;
using PVZEngine.Entities;
using UnityEngine;

namespace MVZ2.GameContent.Detections
{
    public class SkeletonHorseBlocksJumpDetector : Detector
    {
        protected override Bounds GetDetectionBounds(Entity self)
        {
            return self.GetBounds();
        }
        protected override bool ValidateCollider(DetectionParams param, IEntityCollider collider)
        {
            if (!base.ValidateCollider(param, collider))
                return false;
            var bounds = collider.GetBoundingBox();
            if (!Detection.IsInFrontOf(param.entity, bounds.center.x))
                return false;
            var target = collider.Entity;
            if (!target.BlocksJump())
                return false;
            return true;
        }
    }
}
