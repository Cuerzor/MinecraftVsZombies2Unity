using System;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using PVZEngine.Entities;
using UnityEngine;

namespace MVZ2.GameContent.Detections
{
    public class DispenserDetector : Detector
    {
        protected override Bounds GetDetectionBounds(Entity self)
        {
            var source = self.GetShootPoint();
            var projectileID = self.GetProjectileID();
            var projectileDef = GetEntityDefinition(self.Level, projectileID);
            var range = self.GetRange();
            var projectileSize = projectileDef.GetProperty<Vector3>(EngineEntityProps.SIZE);

            var sizeX = range < 0 ? 800 : range;
            var centerX = source.x + sizeX * 0.5f * self.GetFacingX();

            float sizeY;
            float centerY;
            if (ignoreHighEnemy)
            {
                if (ignoreLowEnemy)
                {
                    sizeY = projectileSize.y;
                    centerY = source.y;
                }
                else
                {
                    sizeY = 1000;
                    centerY = source.y + projectileSize.y - sizeY * 0.5f;
                }
            }
            else
            {
                if (ignoreLowEnemy)
                {
                    sizeY = 1000;
                    centerY = source.y + sizeY * 0.5f;
                }
                else
                {
                    sizeY = 1000;
                    centerY = source.y;
                }
            }

            var sizeZ = projectileSize.z;
            var centerZ = source.z;


            return new Bounds(new Vector3(centerX, centerY, centerZ), new Vector3(sizeX, sizeY, sizeZ));
        }
        protected override bool ValidateCollider(DetectionParams self, IEntityCollider collider)
        {
            if (!ValidateTarget(self, collider.Entity))
                return false;
            Bounds targetBounds = collider.GetBoundingBox();
            var center = targetBounds.center;
            if (!TargetInLawn(center.x))
                return false;
            if (colliderFilter != null && !colliderFilter(self, collider))
                return false;
            return true;
        }
        public bool ignoreLowEnemy;
        public bool ignoreHighEnemy;
        public Func<DetectionParams, IEntityCollider, bool> colliderFilter;
    }
}
