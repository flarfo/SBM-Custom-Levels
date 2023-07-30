using System;
using System.Linq;
using SplineMesh;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace SBM_CustomLevels
{
    [Serializable]
    public class SplineObject : DefaultObject
    {
        public List<float[]> nodes;

        [JsonConstructor]
        public SplineObject()
        {

        }

        public SplineObject(GameObject gameObject) : base(gameObject)
        {
            // preserve node order
            List<SplineMakerNodeData> splineNodes = gameObject.GetComponentsInChildren<SplineMakerNodeData>().OrderBy(x => x.nodeID).ToList();

            nodes = new List<float[]>();

            foreach (SplineMakerNodeData node in splineNodes)
            {
                float[] position = new float[3];
                position[0] = node.transform.localPosition.x;
                position[1] = node.transform.localPosition.y;
                position[2] = node.transform.localPosition.z;

                nodes.Add(position);
            }
        }
    }
}
