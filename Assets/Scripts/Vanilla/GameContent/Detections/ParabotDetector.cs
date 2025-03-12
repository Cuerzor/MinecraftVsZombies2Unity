using MVZ2.Vanilla.Detections;
using PVZEngine.Entities;
using UnityEngine;

namespace MVZ2.GameContent.Detections
{
    public class ParabotDetector : Detector
    {
        public ParabotDetector(float range)
        {
            this.range = range;
        }
        protected override Bounds GetDetectionBounds(Entity self)
        {
            var sizeX = range * 2;
            var sizeY = range * 2;
            var sizeZ = range * 2;
            var center = self.GetCenter();
            return new Bounds(center, new Vector3(sizeX, sizeY, sizeZ));
        }
        protected override bool ValidateCollider(DetectionParams param, IEntityCollider collider)
        {
            if (!base.ValidateCollider(param, collider))
                return false;
            Bounds targetBounds = collider.GetBoundingBox();
            var center = targetBounds.center;
            return Vector3.Distance(param.entity.GetCenter(), center) < range;
        }
        public float range;
    }
}
