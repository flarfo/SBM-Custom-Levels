using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using SBM_CustomLevels.Objects;

namespace SBM_CustomLevels.ObjectWrappers
{
    [Serializable]
    public class WaterObject : DefaultObject
    {
        public float waterHeight;
        public float waterWidth;

        public bool w5;

        public List<Keyframe> keyframes;

        [JsonConstructor]
        public WaterObject()
        {

        }

        public WaterObject(GameObject gameObject) : base(gameObject)
        {
            objectType = ObjectType.Water;

            WaterDataContainer fakeWater = gameObject.GetComponent<WaterDataContainer>();

            waterHeight = fakeWater.height;
            waterWidth = fakeWater.width;

            w5 = fakeWater.w5;

            keyframes = fakeWater.keyframes;
        }
    }
}
