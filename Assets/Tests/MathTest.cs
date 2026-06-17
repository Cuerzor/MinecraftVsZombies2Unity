using PVZEngine.Tools.Geometry;
using UnityEngine;

namespace MVZ2.Tests
{
    [ExecuteAlways]
    public class MathTest : MonoBehaviour
    {
        private void Update()
        {
            var box = new Bounds(boxCenter, boxSize);
            var capsule = new Capsule(lineStart, lineEnd, radius);
            var distance = Mathf.Sqrt(Geometry.GetSegmentAABBSqrDistance(lineStart, lineEnd, box));
            collision = Geometry.CollideBetweenCubeAndCapsule(capsule, box);
            Debug.Log($"Distance: {distance}");
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = collision ? Color.red : Color.white;
            Gizmos.DrawCube(boxCenter, boxSize);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(lineStart, radius);
            Gizmos.DrawSphere(lineEnd, radius);
            Gizmos.DrawSphere(nearest, 0.1f);
            Gizmos.DrawLine(lineStart, lineEnd);

        }
        private bool collision = false;
        private Vector3 nearest = Vector3.zero;
        [SerializeField]
        private Vector3 boxCenter;
        [SerializeField]
        private Vector3 boxSize;
        [SerializeField]
        private Vector3 lineStart;
        [SerializeField]
        private Vector3 lineEnd;
        [SerializeField]
        private float radius;
    }
}
