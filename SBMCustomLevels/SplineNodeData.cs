using System.Collections.Generic;
using UnityEngine;
using SplineMesh;

namespace SBM_CustomLevels
{
    public class SplineNodeData : MonoBehaviour
    {
        public SplineNode node;
        public Spline spline;

        public bool doSmoothing = false;

        private SplineSmoother splineSmoother;

        private Vector3 lastPosition;
        private float changeThreshold = 0.0025f;

        private void Start()
        {
            if (doSmoothing)
            {
                splineSmoother = transform.parent.GetComponent<SplineSmoother>();
            }

            lastPosition = transform.localPosition;

            // rail breaks when start node is updated first, likely because direction vector of 2nd node is not set yet
            if (spline.nodes.IndexOf(node) != 0)
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
            int nodeID = spline.nodes.IndexOf(node);

            node.Position = transform.localPosition;

            if (nodeID == 0)
            {
                node.Direction = node.Position + spline.nodes[1].Direction - spline.nodes[1].Position;

                if (doSmoothing)
                {
                    splineSmoother.SmoothNode(node);
                    splineSmoother.SmoothNode(spline.nodes[nodeID + 1]);
                }

                return;
            }

            node.Direction = node.Position - spline.nodes[nodeID - 1].Position + spline.nodes[nodeID - 1].Direction;

            if (doSmoothing)
            {
                splineSmoother.SmoothNode(node);
                splineSmoother.SmoothNode(spline.nodes[nodeID - 1]);

                if (spline.nodes.Count - 1 != nodeID)
                {
                    splineSmoother.SmoothNode(spline.nodes[nodeID + 1]);
                }
            }
        }
    }
}
