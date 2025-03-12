using System.Collections.Generic;
using PVZEngine.Entities;
using UnityEngine;

namespace PVZEngine.Level.Collisions
{
    public class QuadTreeCollider : QuadTree<EntityCollider>
    {
        public QuadTreeCollider(Rect size, int maxObjects = 1, int maxDepth = 5) : base(size, maxObjects, maxDepth)
        {
        }
    }
}
