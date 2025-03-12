using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using PVZEngine.Entities;
using UnityEngine;

namespace MVZ2.GameContent.Detections
{
    public class SpikeBlockDetector : Detector
    {
        public SpikeBlockDetector(bool evoked = false)
        {
            this.evoked = evoked;
        }
        protected override Bounds GetDetectionBounds(Entity self)
        {
            var bounds = self.GetBounds();
            if (evoked)
            {
                var size = bounds.size;
                size.x = 1600;
                size.y = 800;
                bounds.size = size;

                var center = bounds.center;
                center.y = self.Position.y + size.y * 0.5f;
                bounds.center = center;
            }
            return bounds;
        }
        protected override bool ValidateCollider(DetectionParams param, IEntityCollider collider)
        {
            if (!collider.IsMain())
                return false;
            if (evoked && collider.Entity.GetRelativeY() > param.entity.GetSize().y)
                return false;
            if (!base.ValidateCollider(param, collider))
                return false;
            return true;
        }
        private bool evoked;
    }
}
