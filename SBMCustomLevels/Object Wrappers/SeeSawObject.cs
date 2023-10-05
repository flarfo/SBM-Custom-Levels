using System;
using Newtonsoft.Json;
using UnityEngine;

namespace SBM_CustomLevels.ObjectWrappers
{
    [Serializable]
    public class SeeSawObject : MeshSliceObject
    {
        public float[] pivotPos = new float[3];

        [JsonConstructor]
        public SeeSawObject()
        {

        }

        public SeeSawObject(GameObject gameObject) : base(gameObject)
        {
            objectType = ObjectType.SeeSaw;

            Vector3 pivot = gameObject.GetComponent<SBM.Objects.World5.SeeSaw>().cj.anchor;

            pivotPos[0] = pivot.x;
            pivotPos[1] = pivot.y;
            pivotPos[2] = pivot.z;
        }
    }
}
