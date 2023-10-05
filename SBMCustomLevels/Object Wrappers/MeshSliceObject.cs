using System;
using UnityEngine;
using Newtonsoft.Json;
using SBM_CustomLevels.Objects;

namespace SBM_CustomLevels.ObjectWrappers
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
            objectType = ObjectType.MeshSlice;

            MeshSliceData meshData = gameObject.GetComponent<MeshSliceData>();

            meshHeight = meshData.height;
            meshWidth = meshData.width;
            meshDepth = meshData.depth;
        }
    }
}
