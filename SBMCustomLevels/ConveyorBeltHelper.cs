using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;

namespace SBM_CustomLevels
{
    public static class ConveyorBeltHelper
    {
        private static string conveyorPath = Path.Combine("prefabs", "level", "world5", "ConveyorBelt");

        public static GameObject SpawnNewConveyor(Vector3 position)
        {
            GameObject conveyorObj = GameObject.Instantiate(Resources.Load(conveyorPath) as GameObject);

            conveyorObj.transform.position = position;

            Spline conveyorSpline = conveyorObj.GetComponentInChildren<Spline>();

            Vector3 startPos = new Vector3(-1, 0, 0);
            SplineNode startNode = new SplineNode(startPos, new Vector3(1, 0, 0));
            conveyorSpline.AddNode(startNode);

            Vector3 endPos = new Vector3(1, 0, 0);
            SplineNode endNode = new SplineNode(endPos, new Vector3(1, 0, 0));
            conveyorSpline.AddNode(endNode);

            conveyorSpline.start = startNode;
            conveyorSpline.end = endNode;

            // first rail handle
            SplineNodeData point1 = CreateNodeHandle(conveyorSpline.transform, startNode);
            point1.transform.localPosition = startPos;

            // second rail handle
            SplineNodeData point2 = CreateNodeHandle(conveyorSpline.transform, endNode);
            point2.transform.localPosition = endPos;

            return conveyorObj;
        }

        public static GameObject CreateRailFromObject(RailObject railObject, bool isEditor)
        {
            GameObject rail = new GameObject("MinecartRail");

            Spline railSpline = rail.AddComponent<Spline>();
            SplineSmoother smoother = rail.AddComponent<SplineSmoother>();

            SplineMeshTiling splineTile = rail.AddComponent<SplineMeshTiling>();
            //splineTile.mesh = railSplineTile;
            //splineTile.material = railMaterial;
            splineTile.mode = MeshBender.FillingMode.Repeat;
            splineTile.updateInPlayMode = true;

            rail.transform.position = railObject.GetPosition();
            rail.transform.rotation = Quaternion.Euler(railObject.GetRotation());
            rail.transform.localScale = railObject.GetScale();

            foreach (SplineNodeObject nodeObject in railObject.nodes)
            {
                SplineNode node = nodeObject.GetAsSplineNode();

                railSpline.AddNode(node);

                if (isEditor)
                {
                    SplineNodeData railNode = CreateNodeHandle(rail.transform, node);
                    railNode.transform.localPosition = node.Position;
                }
            }

            return rail;
        }

        private static SplineNodeData CreateNodeHandle(Transform parentRail, SplineNode node)
        {
            GameObject nodeHandle = GameObject.Instantiate(MinecartRailHelper.railNodeHandle);
            nodeHandle.name = "ConveyorNode";
            nodeHandle.transform.parent = parentRail;

            nodeHandle.AddComponent<Outline>();
            nodeHandle.AddComponent<EditorSelectable>();
            nodeHandle.AddComponent<MeshCollider>();

            SplineNodeData conveyorNode = nodeHandle.AddComponent<SplineNodeData>();
            conveyorNode.spline = parentRail.GetComponent<Spline>();
            conveyorNode.node = node;
            //make node a cube that renders on top
            return conveyorNode;
        }
    }
}
