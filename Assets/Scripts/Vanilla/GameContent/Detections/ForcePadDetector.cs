using MVZ2.Vanilla.Detections;
using PVZEngine.Entities;
using UnityEngine;

namespace MVZ2.GameContent.Detections
{
    public class ForcePadDetector : Detector
    {
        public ForcePadDetector(int mask, float affectHeight, float sizeMultiplier)
        {
            this.mask = mask;
            this.affectHeight = affectHeight;
            this.sizeMultiplier = sizeMultiplier;
        }
        protected override Bounds GetDetectionBounds(Entity self)
        {
            var padSize = self.GetScaledSize() * sizeMultiplier;
            var sizeX = padSize.x;
            var sizeY = affectHeight;
            var sizeZ = padSize.z;
            var source = self.Position;
            var centerX = source.x;
            var centerY = source.y + sizeY * 0.5f;
            var centerZ = source.z;
            return new Bounds(new Vector3(centerX, centerY, centerZ), new Vector3(sizeX, sizeY, sizeZ));
        }
        protected override bool ValidateCollider(DetectionParams param, IEntityCollider collider)
        {
            var target = collider.Entity;
            if (target == null)
                return false;
            if (target.IsDead)
                return false;
            return true;
        }
        private float affectHeight;
        private float sizeMultiplier;
    }
}
