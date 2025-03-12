using MVZ2.Vanilla.Detections;
using PVZEngine.Entities;
using UnityEngine;

namespace MVZ2.GameContent.Detections
{
    public class GravityPadDetector : Detector
    {
        public GravityPadDetector(bool forEnemy, float affectHeight)
        {
            if (forEnemy)
            {
                mask = EntityCollisionHelper.MASK_ENEMY;
                factionTarget = FactionTarget.Hostile;
            }
            else
            {
                mask = EntityCollisionHelper.MASK_PROJECTILE;
                factionTarget = FactionTarget.Friendly;
            }
            this.affectHeight = affectHeight;
        }
        protected override Bounds GetDetectionBounds(Entity self)
        {
            var padSize = self.GetScaledSize();
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
            if (!target.IsFactionTarget(param.faction, factionTarget))
                return false;
            if (target.Type == EntityTypes.PROJECTILE)
            {
                return target.Position.y > param.entity.Position.y + MIN_HEIGHT;
            }
            return true;
        }
        public const float MIN_HEIGHT = 5;
        public float affectHeight;
    }
}
