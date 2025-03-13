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
            var fireBoundsPivot = fireBreathDef.GetBoundsPivot();

            var positionOffset = Vector3.Scale(Vector3.one * 0.5f - fireBoundsPivot, fireSize);
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
        public static readonly Vector3 defaultPivot = new Vector3(0.5f, 0f, 0.5f);
    }
}
