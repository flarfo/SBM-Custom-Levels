using System.Collections.Generic;
using UnityEngine;
using SplineMesh;

namespace SBM_CustomLevels.Objects
{
    // creates a "node" handle along the SplineMesh spline to allow user to click and edit the spline in the SBM editor
    // specifically for working with the <<SplineMesh>> package ----> SplineSmoother ----> MinecartRailHelper 
    public class SplineMeshNodeData : MonoBehaviour
    {
        public SplineNode node;
        public Spline spline;
        public int nodeID;

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

            
            nodeID = spline.nodes.IndexOf(node);

            // rail breaks when start node is updated first, likely because direction vector of 2nd node is not set yet
            if (nodeID != 0)
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
            nodeID = spline.nodes.IndexOf(node);

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

        public SplineMeshNodeData AddNodeAfter()
        {
            SplineNode newNode = new SplineNode(node.Direction, node.Direction + node.Direction - node.Position);
            newNode.Up = node.Up;

            //var index = spline.nodes.IndexOf(node);

            if (nodeID == spline.nodes.Count - 1)
            {
                spline.AddNode(newNode);
            }
            else
            {
                spline.InsertNode(nodeID + 1, newNode);
            }

            SplineMeshNodeData nodeHandle = MinecartRailHelper.CreateNodeHandle(spline.transform, newNode);
            nodeHandle.doSmoothing = true;
            nodeHandle.transform.localPosition = node.Direction;

            return nodeHandle;
        }
    }
}
