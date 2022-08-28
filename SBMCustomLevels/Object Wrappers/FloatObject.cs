using System;
using UnityEngine;

namespace SBM_CustomLevels
{
    [Serializable]
    public class FloatObject
    {
        public float[] position = new float[3];

        public FloatObject(GameObject gameObject)
        {
            position[0] = gameObject.transform.position.x;
            position[1] = gameObject.transform.position.y;
            position[2] = gameObject.transform.position.z;
        }

        public Vector3 GetPosition()
        {
            return new Vector3(position[0], position[1], position[2]);
        }
    }
}
