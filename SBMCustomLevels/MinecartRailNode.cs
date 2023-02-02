using System.Collections.Generic;
using UnityEngine;
using SplineMesh;

namespace SBM_CustomLevels
{
    public class MinecartRailNode : MonoBehaviour
    {
        public SplineNode node;
        public Spline railSpline;

        private SplineSmoother railSmoother;

        private Vector3 lastPosition;
        private float changeThreshold = 0.0025f;

        private void Start()
        {
            railSmoother = transform.parent.GetComponent<SplineSmoother>();

            lastPosition = transform.localPosition;

            // rail breaks when start node is updated first, likely because direction vector of 2nd node is not set yet
            if (railSpline.nodes.IndexOf(node) != 0)
            {
                UpdateNode();
            }
        }

        private void Update()
        {
            Vector3 changeVector = transform.localPosition - lastPosition;

            if (changeVector.sqrMagnitude > changeThreshold)
            {
                //changed 
                UpdateNode();

                lastPosition = transform.localPosition;
            }
        }

        public void UpdateNode()
        {
            int nodeID = railSpline.nodes.IndexOf(node);

            node.Position = transform.localPosition;

            if (nodeID == 0)
            {
                node.Direction = node.Position + railSpline.nodes[1].Direction - railSpline.nodes[1].Position;

                railSmoother.SmoothNode(node);
                railSmoother.SmoothNode(railSpline.nodes[nodeID + 1]);

                return;
            }

            node.Direction = node.Position - railSpline.nodes[nodeID - 1].Position + railSpline.nodes[nodeID - 1].Direction;

            railSmoother.SmoothNode(node);
            railSmoother.SmoothNode(railSpline.nodes[nodeID - 1]);

            if (railSpline.nodes.Count-1 != nodeID)
            {
                railSmoother.SmoothNode(railSpline.nodes[nodeID + 1]);
            }
        }
    }
}
