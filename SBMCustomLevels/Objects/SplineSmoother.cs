using UnityEngine;

namespace SplineMesh
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Spline))]
    public class SplineSmoother : MonoBehaviour
    {
        private Spline spline;
        private Spline Spline
        {
            get
            {
                if (spline == null) spline = GetComponent<Spline>();
                return spline;
            }
        }

        public float curvature = 0.3f;

        private void OnEnable()
        {
            SmoothAll();
        }

        public void SmoothNode(SplineNode node)
        {
            var index = Spline.nodes.IndexOf(node);
            var pos = node.Position;
            // For the direction, we need to compute a smooth vector.
            // Orientation is obtained by substracting the vectors to the previous and next way points,
            // which give an acceptable tangent in most situations.
            // Then we apply a part of the average magnitude of these two vectors, according to the smoothness we want.
            var dir = Vector3.zero;
            float averageMagnitude = 0;
            if (index != 0)
            {
                var previousPos = Spline.nodes[index - 1].Position;
                var toPrevious = pos - previousPos;
                averageMagnitude += toPrevious.magnitude;
                dir += toPrevious.normalized;
            }
            if (index != Spline.nodes.Count - 1)
            {
                var nextPos = Spline.nodes[index + 1].Position;
                var toNext = pos - nextPos;
                averageMagnitude += toNext.magnitude;
                dir -= toNext.normalized;
            }
            averageMagnitude *= 0.5f;
            // This constant should vary between 0 and 0.5, and allows to add more or less smoothness.
            dir = dir.normalized * averageMagnitude * curvature;

            // In SplineMesh, the node direction is not relative to the node position. 
            var controlPoint = dir + pos;

            // We only set one direction at each spline node because SplineMesh only support mirrored direction between curves.
            node.Direction = controlPoint;
        }

        public void SmoothAll()
        {
            foreach (var node in Spline.nodes)
            {
                SmoothNode(node);
            }
        }
    }
}
