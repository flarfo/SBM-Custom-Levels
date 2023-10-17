using System;
using UnityEngine;
using Newtonsoft.Json;
using SBM_CustomLevels.Objects;

namespace SBM_CustomLevels.ObjectWrappers
{
    [Serializable]
    public class ColorBlockObject : DefaultObject
    {
        public int r;
        public int g;
        public int b;
        public bool isCorner = false;

        [JsonConstructor]
        public ColorBlockObject()
        {

        }

        public ColorBlockObject(GameObject gameObject) : base(gameObject)
        {
            objectType = ObjectType.ColorBlock;

            ColorData colorData = gameObject.GetComponent<ColorData>();
            r = colorData.color.r;
            g = colorData.color.g;
            b = colorData.color.b;

            if (gameObject.name.Contains("Corner"))
            {
                isCorner = true;
            }
        }
    }
}
