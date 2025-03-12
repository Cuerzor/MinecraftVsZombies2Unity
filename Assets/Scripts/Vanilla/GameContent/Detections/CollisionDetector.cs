using MVZ2.Vanilla.Detections;
using PVZEngine.Entities;
using Tools.Mathematics;
using UnityEngine;

namespace MVZ2.GameContent.Detections
{
    public class CollisionDetector : Detector
    {
        public CollisionDetector()
        {
        }
        protected override Bounds GetDetectionBounds(Entity self)
        {
            return self.GetBounds();
        }
    }
}
