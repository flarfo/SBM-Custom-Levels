using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace SBM_CustomLevels
{
    [Serializable]
    public class TubeRendererObject :  DefaultObject
    {

        [JsonConstructor]
        public TubeRendererObject()
        {

        }

        public TubeRendererObject(GameObject gameObject) : base(gameObject)
        {

        }
    }
}
