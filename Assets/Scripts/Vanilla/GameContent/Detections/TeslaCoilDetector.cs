using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using PVZEngine.Entities;
using Tools.Mathematics;
using UnityEngine;

namespace MVZ2.GameContent.Detections
{
    public class TeslaCoilDetector : Detector
    {
        public TeslaCoilDetector(float attackHeight)
        {
            this.attackHeight = attackHeight;
        }
        protected override Bounds GetDetectionBounds(Entity self)
        {
            var range = self.GetRange();
            var sizeX = range * 2;
            var sizeY = attackHeight;
            var sizeZ = range * 2;
            var centerX = self.Position.x;
            var centerY = self.Position.y + sizeY * 0.5f;
            var centerZ = self.Position.z;
            return new Bounds(new Vector3(centerX, centerY, centerZ), new Vector3(sizeX, sizeY, sizeZ));
        }
        protected override bool ValidateCollider(DetectionParams param, IEntityCollider collider)
        {
            if (!base.ValidateCollider(param, collider))
                return false;
            var self = param.entity;

            var range = self.GetRange();
            var center = self.GetCenter();

            return collider.CheckSphere(center, range);
        }
        private float attackHeight;
    }
}
