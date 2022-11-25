using System;
using UnityEngine;
using Newtonsoft.Json;

namespace SBM_CustomLevels
{
    [Serializable]
    public class WaterObject : DefaultObject
    {
        public float waterHeight;
        public float waterWidth;

        [JsonConstructor]
        public WaterObject()
        {

        }

        public WaterObject(GameObject gameObject) : base(gameObject)
        {
            FakeWater fakeWater = gameObject.GetComponent<FakeWater>();

            waterHeight = fakeWater.height;
            waterWidth = fakeWater.width;
        }
    }
}
