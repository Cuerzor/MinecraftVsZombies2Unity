using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Level;
using PVZEngine.Entities;
using UnityEngine;

namespace MVZ2.GameContent.Detections
{
    public class WitherDetector : Detector
    {
        public WitherDetector(int mode)
        {
            this.mode = mode;
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
                case MODE_EAT:
                    {
                        sizeX = 320;
                        sizeY = 160;
                        sizeZ = self.Level.GetGridTopZ() - self.Level.GetGridBottomZ();

                        if (self.IsFacingLeft())
                        {
                            var column = self.Level.GetMaxColumnCount() - 1;
                            var x = self.Level.GetEntityColumnX(column);
                            centerX = x - sizeX * 0.5f;
                        }
                        else
                        {
                            var column = 0;
                            var x = self.Level.GetEntityColumnX(column);
                            centerX = x + sizeX * 0.5f;
                        }
                        centerY = 80;
                        centerZ = self.Level.GetLawnCenterZ();
                    }
                    break;
                default:
                    {
                        sizeX = VanillaLevelExt.RIGHT_BORDER - VanillaLevelExt.LEFT_BORDER;
                        sizeY = 800;
                        sizeZ = self.Level.GetGridTopZ() - self.Level.GetGridBottomZ();
                        centerX = (VanillaLevelExt.LEFT_BORDER + VanillaLevelExt.RIGHT_BORDER) * 0.5f;
                        centerY = 400;
                        centerZ = self.Level.GetLawnCenterZ(); ;
                    }
                    break;
            }
            return new Bounds(new Vector3(centerX, centerY, centerZ), new Vector3(sizeX, sizeY, sizeZ));
        }
        private int mode;
        public const int MODE_SKULL = 0;
        public const int MODE_CHARGE = 1;
        public const int MODE_EAT = 2;
    }
}
