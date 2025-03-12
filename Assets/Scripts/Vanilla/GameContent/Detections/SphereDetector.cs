using MVZ2.Vanilla.Detections;
using PVZEngine.Entities;
using Tools.Mathematics;
using UnityEngine;

namespace MVZ2.GameContent.Detections
{
    public class SphereDetector : Detector
    {
        public SphereDetector(float radius)
        {
            this.radius = radius;
        }
        protected override Bounds GetDetectionBounds(Entity self)
        {
            var sizeX = radius * 2;
            var sizeY = radius * 2;
            var sizeZ = radius * 2;
            var center = self.GetCenter();
            return new Bounds(center, new Vector3(sizeX, sizeY, sizeZ));
        }
        protected override bool ValidateCollider(DetectionParams param, IEntityCollider collider)
        {
            if (!base.ValidateCollider(param, collider))
                return false;
            var self = param.entity;
            var center = self.GetCenter();
            return collider.CheckSphere(center, radius);
        }
        private float radius;
    }
}
