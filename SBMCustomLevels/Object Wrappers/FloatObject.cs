using System;
using UnityEngine;
using Newtonsoft.Json;

namespace SBM_CustomLevels
{
    [Serializable]
    public class FloatObject
    {
        public float[] position = new float[3];

        [JsonConstructor]
        public FloatObject()
        {

        }

        public FloatObject(GameObject gameObject)
        {
            position[0] = gameObject.transform.position.x;
            position[1] = gameObject.transform.position.y;
            position[2] = gameObject.transform.position.z;
        }

        public FloatObject(Vector3 pos)
        {
            position[0] = pos.x;
            position[1] = pos.y;
            position[2] = pos.z;
        }

        public Vector3 GetPosition()
        {
            return new Vector3(position[0], position[1], position[2]);
        }
    }
}
