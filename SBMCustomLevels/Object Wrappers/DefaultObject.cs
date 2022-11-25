using UnityEngine;
using System;
using Newtonsoft.Json;

namespace SBM_CustomLevels
{
    [Serializable]
    public class DefaultObject
    {
        public string objectName;

        public float[] position = new float[3];
        public float[] rotation = new float[3];
        public float[] scale = new float[3];

        [JsonConstructor]
        public DefaultObject()
        {

        }

        public DefaultObject(GameObject gameObject)
        {
            objectName = RecordLevel.NameToPath(gameObject.name);

            position[0] = gameObject.transform.position.x;
            position[1] = gameObject.transform.position.y;
            position[2] = gameObject.transform.position.z;

            rotation[0] = gameObject.transform.rotation.eulerAngles.x;
            rotation[1] = gameObject.transform.rotation.eulerAngles.y;
            rotation[2] = gameObject.transform.rotation.eulerAngles.z;

            scale[0] = gameObject.transform.localScale.x;
            scale[1] = gameObject.transform.localScale.y;
            scale[2] = gameObject.transform.localScale.z;
        }


        public Vector3 GetPosition()
        {
            return new Vector3(position[0], position[1], position[2]);
        }

        public Vector3 GetRotation()
        {
            return new Vector3(rotation[0], rotation[1], rotation[2]);
        }

        public Vector3 GetScale()
        {
            return new Vector3(scale[0], scale[1], scale[2]);
        }
    }
}
