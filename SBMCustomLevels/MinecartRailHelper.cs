﻿using System;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;

namespace SBM_CustomLevels
{
    public static class MinecartRailHelper
    {
        public static Mesh railSplineTile;
        public static Material railMaterial;
        public static GameObject railNodeHandle;

        public static GameObject SpawnNewRail(Vector3 position)
        {
            GameObject rail = new GameObject("MinecartRail");
            rail.transform.position = position;

            Spline railSpline = rail.AddComponent<Spline>();
            rail.AddComponent<SplineSmoother>();

            SplineMeshTiling splineTile = rail.AddComponent<SplineMeshTiling>();
            splineTile.mesh = railSplineTile;
            splineTile.material = railMaterial;
            splineTile.mode = MeshBender.FillingMode.Repeat;
            splineTile.updateInPlayMode = true;

            Vector3 startPos = new Vector3(-1, 0, 0);
            SplineNode startNode = new SplineNode(startPos, new Vector3(1, 0, 0));
            railSpline.AddNode(startNode);

            Vector3 endPos = new Vector3(1, 0, 0);
            SplineNode endNode = new SplineNode(endPos, new Vector3(1, 0, 0));
            railSpline.AddNode(endNode);

            railSpline.start = startNode;
            railSpline.end = endNode;

            // first rail handle
            SplineNodeData point1 = CreateNodeHandle(rail.transform, startNode);
            point1.transform.localPosition = startPos;

            // second rail handle
            SplineNodeData point2 = CreateNodeHandle(rail.transform, endNode);
            point2.transform.localPosition = endPos;

            return rail;
        }

        public static GameObject CreateRailFromObject(RailObject railObject, bool isEditor)
        {
            GameObject rail = new GameObject("MinecartRail");

            Spline railSpline = rail.AddComponent<Spline>();
            SplineSmoother smoother = rail.AddComponent<SplineSmoother>();

            SplineMeshTiling splineTile = rail.AddComponent<SplineMeshTiling>();
            splineTile.mesh = railSplineTile;
            splineTile.material = railMaterial;
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
            GameObject nodeHandle = GameObject.Instantiate(railNodeHandle);
            nodeHandle.name = "RailNode";
            nodeHandle.transform.parent = parentRail;

            nodeHandle.AddComponent<Outline>();
            nodeHandle.AddComponent<EditorSelectable>();
            nodeHandle.AddComponent<MeshCollider>();

            SplineNodeData railNode = nodeHandle.AddComponent<SplineNodeData>();
            railNode.doSmoothing = true;
            railNode.spline = parentRail.GetComponent<Spline>();
            railNode.node = node;
            //make node a cube that renders on top
            return railNode;
        }

        public static SplineNodeData AddNodeAfterSelected(Spline spline, SplineNode selectedNode)
        {
            SplineNode newNode = new SplineNode(selectedNode.Direction, selectedNode.Direction + selectedNode.Direction - selectedNode.Position);
            newNode.Up = selectedNode.Up;

            var index = spline.nodes.IndexOf(selectedNode);

            if (index == spline.nodes.Count - 1)
            {
                spline.AddNode(newNode);
            }
            else
            {
                spline.InsertNode(index + 1, newNode);
            }

            SplineNodeData node = CreateNodeHandle(spline.transform, newNode);
            node.doSmoothing = true;
            node.transform.localPosition = selectedNode.Direction;

            return node;
        }
    }
}
