using MVZ2.Vanilla.Detections;
using PVZEngine.Entities;
using UnityEngine;

namespace MVZ2.GameContent.Detections
{
    public class CameraFlashDetector : Detector
    {
        public CameraFlashDetector()
        {
            mask = EntityCollisionHelper.MASK_PLANT | EntityCollisionHelper.MASK_PROJECTILE;
        }
        protected override Bounds GetDetectionBounds(Entity self)
        {
            return self.GetBounds();
        }
        protected override bool ValidateCollider(DetectionParams param, IEntityCollider collider)
        {
            var target = collider.Entity;
            if (target.Type == EntityTypes.PROJECTILE)
            {
                if (target == null)
                    return false;
                if (param.entity == target && !includeSelf)
                    return false;
                if (target.IsDead)
                    return false;
                if (!target.IsFactionTarget(param.faction, factionTarget))
                    return false;
                return true;
            }
            return base.ValidateCollider(param, collider);
        }
    }
}
