using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace SBM_CustomLevels
{
    [Serializable]
    public class MeshSliceObject : DefaultObject
    {
        public float meshHeight;
        public float meshWidth;
        public float meshDepth;

        [JsonConstructor]
        public MeshSliceObject()
        {

        }

        public MeshSliceObject(GameObject gameObject) : base(gameObject)
        {
            MeshSliceData meshData = gameObject.GetComponent<MeshSliceData>();

            meshHeight = meshData.height;
            meshWidth = meshData.width;
            meshDepth = meshData.depth;
        }
    }
}
