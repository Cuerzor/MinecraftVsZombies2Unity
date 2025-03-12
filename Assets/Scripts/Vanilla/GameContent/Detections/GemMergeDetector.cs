using MVZ2.Vanilla.Detections;
using PVZEngine;
using PVZEngine.Entities;
using UnityEngine;

namespace MVZ2.GameContent.Detections
{
    public class GemMergeDetector : Detector
    {
        public GemMergeDetector(float size, NamespaceID gemID)
        {
            mask = EntityCollisionHelper.MASK_PICKUP;
            this.size = size;
            this.gemID = gemID;
        }
        protected override Bounds GetDetectionBounds(Entity self)
        {
            var sizeX = size;
            var sizeY = size;
            var sizeZ = size;
            var centerX = self.Position.x;
            var centerY = self.Position.y + sizeY * 0.5f;
            var centerZ = self.Position.z;
            return new Bounds(new Vector3(centerX, centerY, centerZ), new Vector3(sizeX, sizeY, sizeZ));
        }
        protected override bool ValidateCollider(DetectionParams param, IEntityCollider collider)
        {
            return collider.Entity.IsEntityOf(gemID);
        }
        private float size;
        private NamespaceID gemID;
    }
}
