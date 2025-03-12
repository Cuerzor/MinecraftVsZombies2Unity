using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using PVZEngine;
using PVZEngine.Entities;
using UnityEngine;

namespace MVZ2.GameContent.Detections
{
    public class FrankensteinGunDetector : Detector
    {
        public FrankensteinGunDetector(NamespaceID projectileID)
        {
            this.projectileID = projectileID;
        }
        protected override Bounds GetDetectionBounds(Entity self)
        {
            var projectileDef = GetEntityDefinition(self.Level, projectileID);
            var projectileSize = projectileDef.GetProperty<Vector3>(EngineEntityProps.SIZE);

            var source = self.Position;

            var sizeX = 800;
            var sizeY = 1000;
            var sizeZ = projectileSize.z;
            var centerX = source.x + sizeX * 0.5f * self.GetFacingX();
            var centerY = source.y;
            var centerZ = source.z;
            return new Bounds(new Vector3(centerX, centerY, centerZ), new Vector3(sizeX, sizeY, sizeZ));
        }
        protected override bool ValidateCollider(DetectionParams self, IEntityCollider collider)
        {
            if (!base.ValidateCollider(self, collider))
                return false;
            var target = collider.Entity;
            if (!TargetInLawn(target))
                return false;
            return true;
        }
        private NamespaceID projectileID;
    }
}
