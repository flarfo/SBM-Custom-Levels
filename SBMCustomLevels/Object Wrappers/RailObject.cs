﻿using System;
using System.Linq;
using SplineMesh;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using SBM_CustomLevels.Objects;

namespace SBM_CustomLevels.ObjectWrappers
{
    [Serializable]
    public class RailObject : DefaultObject
    {
        public List<RailSplineNodeObject> nodes;

        [JsonConstructor]
        public RailObject()
        {

        }

        public RailObject(GameObject gameObject) : base(gameObject)
        {
            objectType = ObjectType.Rail;

            // preserve node order
            List<SplineNode> splineNodes = gameObject.GetComponentsInChildren<SplineMeshNodeData>().OrderBy(x => x.nodeID).Select(x => x.node).ToList();

            nodes = new List<RailSplineNodeObject>();

            foreach (SplineNode node in splineNodes)
            {
                nodes.Add(new RailSplineNodeObject(node));
            }
        }
    }

    [Serializable]
    public class RailSplineNodeObject
    {
        public float[] position = new float[3];
        public float[] direction = new float[3];
        public float[] up = new float[3];

        [JsonConstructor]
        public RailSplineNodeObject()
        {

        }

        public RailSplineNodeObject(SplineNode node)
        {
            position[0] = node.Position.x;
            position[1] = node.Position.y;
            position[2] = node.Position.z;

            direction[0] = node.Direction.x;
            direction[1] = node.Direction.y;
            direction[2] = node.Direction.z;

            up[0] = node.Up.x;
            up[1] = node.Up.y;
            up[2] = node.Up.z;
        }

        public SplineNode GetAsSplineNode()
        {
            Vector3 pos = new Vector3(position[0], position[1], position[2]);
            Vector3 dir = new Vector3(direction[0], direction[1], direction[2]);
            Vector3 _up = new Vector3(up[0], up[1], up[2]);

            SplineNode node = new SplineNode(pos, dir);
            node.Up = _up;

            return node;
        }
    }
}
