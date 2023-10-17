using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SBM_CustomLevels.Objects
{
    // creates a "node" handle along the SplineMaker spline (at anchor points) to allow user to click and edit the spline in the SBM editor
    // specifically for working with the <<TubeRenderer>> package ----> SplineMakerHelper 
    public class SplineMakerNodeData : MonoBehaviour
    {
        public SplineMakerParent splineParent;

        // updated via UI whenever a new anchorPoint is added
        public int nodeID;

        private Vector3 lastPosition;
        private float changeThreshold = 0.0025f;


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
            // update nodeIDs
            nodeID = splineParent.nodes.IndexOf(this);

            splineParent.spline.anchorPoints[nodeID].x = transform.localPosition.x;
            splineParent.spline.anchorPoints[nodeID].y = transform.localPosition.y;
            splineParent.spline.anchorPoints[nodeID].z = transform.localPosition.z;

            splineParent.spline.UpdatePoints();
        }

        public SplineMakerNodeData AddNodeAfter()
        {
            // create new node with slight position offset
            Vector3 position = new Vector3(splineParent.spline.anchorPoints[nodeID].x + 1, splineParent.spline.anchorPoints[nodeID].y, splineParent.spline.anchorPoints[nodeID].z);

            // update spline anchorPoints with a new anchorPoint at new position
            List<Vector3> newAnchorPoints = splineParent.spline.anchorPoints.ToList();
            newAnchorPoints.Insert(nodeID + 1, position);
            splineParent.spline.anchorPoints = newAnchorPoints.ToArray();

            // create handle at new node position
            SplineMakerNodeData nodeHandle = SplineMakerHelper.CreateNodeHandle(splineParent, position, nodeID + 1);

            splineParent.spline.UpdatePoints();

            return nodeHandle;
        }

        public void RemoveNode()
        {
            Vector3[] newAnchorPoints = new Vector3[splineParent.spline.anchorPoints.Length - 1];

            int j = 0;
            for (int i = 0; i < splineParent.nodes.Count; i++)
            {
                if (i != nodeID)
                {
                    newAnchorPoints[j] = splineParent.nodes[i].transform.localPosition;

                    j++;
                }
            }

            splineParent.nodes.Remove(this);

            splineParent.spline.anchorPoints = newAnchorPoints;
            splineParent.spline.UpdatePoints();
        }
    }
}
