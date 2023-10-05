using UnityEngine;
using SBM_CustomLevels.Editor;

namespace SBM_CustomLevels.Objects
{
    // creates a "node" handle at the location of the see saw pivot, when moved adjusts said object's position
    public class SeeSawPivotNode : MonoBehaviour
    {
        public ConfigurableJoint pivotToMove; // reference to original objects transform.position
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
            pivotToMove.anchor = transform.localPosition;
        }
    }

    public static class PivotNodeHelper
    {
        public static SeeSawPivotNode CreatePivotNode(Transform parentObject)
        {
            GameObject nodeHandle = GameObject.Instantiate(MinecartRailHelper.railNodeHandle);
            nodeHandle.name = "PivotNode";
            nodeHandle.transform.parent = parentObject;
            nodeHandle.transform.position = parentObject.position;

            nodeHandle.AddComponent<Outline>();
            nodeHandle.AddComponent<EditorSelectable>();
            nodeHandle.AddComponent<MeshCollider>();
            var node = nodeHandle.AddComponent<SeeSawPivotNode>();

            return node;
        }
    }
}
