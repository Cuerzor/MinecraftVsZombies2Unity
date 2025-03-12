using MVZ2.Vanilla.Detections;
using PVZEngine;
using PVZEngine.Entities;
using UnityEngine;

namespace MVZ2.GameContent.Detections
{
    public class FireBreathDetector : Detector
    {
        protected override Bounds GetDetectionBounds(Entity self)
        {
            var fireBreathDef = GetEntityDefinition(self.Level, fireBreathID);
            if (fireBreathDef == null)
                return new Bounds(Vector3.zero, Vector3.zero);
            var fireSize = fireBreathDef.GetSize();
            var fireBoundsOffset = fireBreathDef.GetBoundsOffset();

            var positionOffset = offset;
            positionOffset += fireBoundsOffset;
            if (self.IsFacingLeft())
            {
                positionOffset.x *= -1;
            }
            return new Bounds(self.Position + positionOffset, fireSize);
        }
        protected override bool ValidateCollider(DetectionParams self, IEntityCollider collider)
        {
            if (!base.ValidateCollider(self, collider))
                return false;
            if (!ValidateTarget(self, collider.Entity))
                return false;
            var targetBounds = collider.GetBoundingBox();
            if (!TargetInLawn(targetBounds.center.x))
                return false;
            return true;
        }
        public NamespaceID fireBreathID;
        public Vector3 offset;
        public static readonly Vector3 defaultPivot = new Vector3(0.5f, 0f, 0.5f);
    }
}
