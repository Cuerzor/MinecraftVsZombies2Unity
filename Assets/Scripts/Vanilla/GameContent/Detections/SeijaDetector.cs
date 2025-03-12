using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using PVZEngine.Entities;
using UnityEngine;

namespace MVZ2.GameContent.Detections
{
    public class SeijaDetector : Detector
    {
        public SeijaDetector(int mode)
        {
            this.mode = mode;
            if (mode == MODE_CAMERA)
            {
                mask = EntityCollisionHelper.MASK_PLANT;
            }
        }
        protected override Bounds GetDetectionBounds(Entity self)
        {
            var source = self.Position;
            float sizeX = 0;
            float sizeY = 0;
            float sizeZ = 0;
            float centerX = source.x;
            float centerY = source.y;
            float centerZ = source.z;
            switch (mode)
            {
                case MODE_SMASH:
                    {
                        sizeX = 40;
                        sizeY = 40;
                        sizeZ = 40;
                        centerX = source.x + sizeX * 0.5f * self.GetFacingX();
                        centerY = source.y + sizeY * 0.5f;
                        centerZ = source.z;
                    }
                    break;
                case MODE_PLACE_BOMB:
                    {
                        sizeX = 240;
                        sizeY = 240;
                        sizeZ = 240;
                        centerX = source.x;
                        centerY = source.y;
                        centerZ = source.z;
                    }
                    break;
                case MODE_GAP_BOMB:
                    {
                        sizeX = 240;
                        sizeY = 240;
                        sizeZ = 240;
                        if (self.IsFacingLeft())
                        {
                            centerX = self.Level.GetEntityColumnX(1);
                        }
                        else
                        {
                            centerX = self.Level.GetEntityColumnX(self.Level.GetMaxColumnCount() - 2);
                        }
                        centerZ = source.z;
                        centerY = self.Level.GetGroundY(centerX, centerZ);
                    }
                    break;
                case MODE_CAMERA:
                    {
                        sizeX = 160;
                        sizeY = 800;
                        sizeZ = 240;
                        centerX = source.x + self.GetFacingX() * 240;
                        centerZ = source.z;
                        centerY = self.Level.GetGroundY(centerX, centerZ);
                    }
                    break;
                default:
                    {
                        sizeX = 240;
                        sizeY = 64;
                        sizeZ = 40;
                        centerX = source.x + sizeX * 0.5f * self.GetFacingX();
                        centerY = source.y + sizeY * 0.5f;
                        centerZ = source.z;
                    }
                    break;
            }
            return new Bounds(new Vector3(centerX, centerY, centerZ), new Vector3(sizeX, sizeY, sizeZ));
        }
        protected override bool ValidateCollider(DetectionParams param, IEntityCollider collider)
        {
            if (mode == MODE_CAMERA)
            {
                var target = collider.Entity;
                if (!target.CanDeactive())
                    return false;
                if (target.IsAIFrozen())
                    return false;
            }
            return base.ValidateCollider(param, collider);
        }
        private int mode;
        public const int MODE_DETECT = 0;
        public const int MODE_SMASH = 1;
        public const int MODE_PLACE_BOMB = 2;
        public const int MODE_GAP_BOMB = 3;
        public const int MODE_CAMERA = 4;
    }
}
