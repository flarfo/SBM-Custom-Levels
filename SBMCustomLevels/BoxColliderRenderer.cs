using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SBM_CustomLevels
{
    public class BoxColliderRenderer : MonoBehaviour
    {
        public List<BoxCollider> boxColliders;

        public Material mat;

        private void Awake()
        {
            boxColliders = new List<BoxCollider>();
            mat = LevelLoader_Mod.colliderRenderMat;
        }

        private void OnPostRender()
        {
            foreach (BoxCollider boxCollider in boxColliders)
            {
                DrawCollider(boxCollider);
            }
        }

        private Vector3[] GetColliderVertices(BoxCollider boxCollider)
        {
            Vector3[] points = new Vector3[8];

            Vector3 min = boxCollider.center - boxCollider.size * 0.5f;
            Vector3 max = boxCollider.center + boxCollider.size * 0.5f;

            points[0] = new Vector3(min.x, min.y, min.z); // left, bottom, back
            points[1] = new Vector3(max.x, min.y, min.z); // right, bottom, back
            points[2] = new Vector3(max.x, max.y, min.z); // right, top, back
            points[3] = new Vector3(min.x, max.y, min.z); // left, top, back
            points[4] = new Vector3(min.x, min.y, max.z); // left, bottom, front
            points[5] = new Vector3(max.x, min.y, max.z); // right, bottom, front
            points[6] = new Vector3(max.x, max.y, max.z); // right, top, front
            points[7] = new Vector3(min.x, max.y, max.z); // left, top, front

            return points;
        }

        public void SetColliderColor(Color color)
        {
            mat.SetColor("_Color", color);
        }

        private void DrawCollider(BoxCollider boxCollider)
        {
            Vector3[] points = GetColliderVertices(boxCollider);

            mat.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(boxCollider.transform.localToWorldMatrix);
            GL.Begin(GL.LINES);

            // back horizontal of cube
            GL.Vertex(points[0]);
            GL.Vertex(points[1]);
            GL.Vertex(points[2]);
            GL.Vertex(points[3]);

            // back vertical of cube
            GL.Vertex(points[0]);
            GL.Vertex(points[3]);
            GL.Vertex(points[1]);
            GL.Vertex(points[2]);

            // front horizontal of cube
            GL.Vertex(points[4]);
            GL.Vertex(points[5]);
            GL.Vertex(points[6]);
            GL.Vertex(points[7]);

            // front vertical of cube
            GL.Vertex(points[4]);
            GL.Vertex(points[7]);
            GL.Vertex(points[5]);
            GL.Vertex(points[6]);

            // left horizontal of cube
            GL.Vertex(points[0]);
            GL.Vertex(points[4]);
            GL.Vertex(points[3]);
            GL.Vertex(points[7]);

            // right horizontal of cube
            GL.Vertex(points[1]);
            GL.Vertex(points[5]);
            GL.Vertex(points[2]);
            GL.Vertex(points[6]);

            GL.End();
            GL.PopMatrix();
        }
    }
}
