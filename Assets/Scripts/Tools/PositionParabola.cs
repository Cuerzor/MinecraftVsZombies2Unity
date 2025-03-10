﻿using Tools.Mathematics;
using UnityEngine;
namespace Tools
{
    [ExecuteInEditMode]
    public class PositionParabola : PositionTransitor
    {
        protected override Vector3 Transit(Vector3 start, Vector3 end, float time)
        {
            var relativeY = end.y - start.y;
            var y = MathTool.LerpParabolla(0, 0, Mathf.Max(relativeY, maxHeight), time, inner);
            return Vector3.Lerp(start, end, time) + axis * y;
        }
        [SerializeField]
        private Vector3 axis;
        [SerializeField]
        private float maxHeight;
        [SerializeField]
        private bool inner;
    }
}
