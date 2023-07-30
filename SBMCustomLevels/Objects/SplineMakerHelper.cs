using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBM_CustomLevels
{
    public static class SplineMakerHelper
    {
        public static Material splineMaterial;
        public static GameObject splineNodeHandle;

        public static GameObject SpawnNewSpline(Vector3 position)
        {
            GameObject splineObject = new GameObject("SplineObject");
            splineObject.transform.position = position;

            splineObject.AddComponent<MeshRenderer>().material = splineMaterial;

            TubeRenderer tubeRenderer = splineObject.AddComponent<TubeRenderer>();
            // set TubeRenderer settings to match those of SBM's 
            tubeRenderer.radius = 0.1f;
            tubeRenderer._cubeTube = true;
            tubeRenderer._cubeTubeWidth = 7.7f;
            tubeRenderer._cubeTubeHeight = 0.7f;
            tubeRenderer.edgeCount = 4;
            tubeRenderer.normalMode = TubeRenderer.NormalMode.HardEdges;
            tubeRenderer.postprocessContinously = true;
            tubeRenderer.forwardAngleOffset = 45;
            tubeRenderer._generateMeshCollider = true;

            // add listener to spline anchorNode changes so the mesh updates on position change
            SplineMaker spline = splineObject.AddComponent<SplineMaker>();
            spline.onUpdated.AddListener(delegate (Vector3[] points)
            {
                tubeRenderer.points = points;
            });

            Vector3 startPos = new Vector3(-1, 0, 0);
            Vector3 endPos = new Vector3(1, 0, 0);

            spline.anchorPoints = new Vector3[2];

            SplineMakerParent splineParent = splineObject.AddComponent<SplineMakerParent>();
            splineParent.spline = spline;

            // first node handle
            SplineMakerNodeData point1 = CreateNodeHandle(splineParent, startPos, 0);

            // second node handle
            SplineMakerNodeData point2 = CreateNodeHandle(splineParent, endPos, 1);

            return splineObject;
        }

        public static GameObject CreateSplineFromObject(SplineObject splineObject, bool isEditor)
        {
            GameObject splineGO = new GameObject("SplineObject");
            splineGO.transform.position = splineObject.GetPosition();

            splineGO.AddComponent<MeshRenderer>().material = splineMaterial;

            TubeRenderer tubeRenderer = splineGO.AddComponent<TubeRenderer>();
            // set TubeRenderer settings to match those of SBM's 
            tubeRenderer.radius = 0.1f;
            tubeRenderer._cubeTube = true;
            tubeRenderer._cubeTubeWidth = 7.7f;
            tubeRenderer._cubeTubeHeight = 0.7f;
            tubeRenderer.edgeCount = 4;
            tubeRenderer.normalMode = TubeRenderer.NormalMode.HardEdges;
            tubeRenderer.postprocessContinously = true;
            tubeRenderer.forwardAngleOffset = 45;
            tubeRenderer._generateMeshCollider = true;

            // add listener to spline anchorNode changes so the mesh updates on position change
            SplineMaker spline = splineGO.AddComponent<SplineMaker>();
            spline.onUpdated.AddListener(delegate (Vector3[] points)
            {
                tubeRenderer.points = points;
            });

            SplineMakerParent splineParent = splineGO.AddComponent<SplineMakerParent>();
            splineParent.spline = spline;

            spline.anchorPoints = new Vector3[splineObject.nodes.Count];
            for (int i = 0; i < splineObject.nodes.Count; i++)
            {
                Vector3 position = new Vector3(splineObject.nodes[i][0], splineObject.nodes[i][1], splineObject.nodes[i][2]);
                spline.anchorPoints[i] = position;

                if (isEditor)
                {
                    SplineMakerNodeData splineNode = CreateNodeHandle(splineParent, position, i);
                }

                spline.UpdatePoints();
            }

            return splineGO;
        }

        public static SplineMakerNodeData CreateNodeHandle(SplineMakerParent splineParent, Vector3 position, int nodeID)
        {
            GameObject nodeHandle = GameObject.Instantiate(splineNodeHandle);
            nodeHandle.name = "SplineNode";
            nodeHandle.transform.parent = splineParent.transform;

            nodeHandle.AddComponent<Outline>();
            nodeHandle.AddComponent<EditorSelectable>();
            nodeHandle.AddComponent<MeshCollider>();

            SplineMakerNodeData splineNode = nodeHandle.AddComponent<SplineMakerNodeData>();
            splineNode.splineParent = splineParent;
            splineNode.nodeID = nodeID;
            splineNode.transform.localPosition = position;

            splineParent.nodes.Insert(nodeID, splineNode);

            return splineNode;
        }
    }
}
