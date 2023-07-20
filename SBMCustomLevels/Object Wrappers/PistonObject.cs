using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace SBM_CustomLevels
{
    [Serializable]
    public class PistonObject : MeshSliceObject
    {
        public float pistonMaxTravel;
        public float pistonShaftLength;

        [JsonConstructor]
        public PistonObject()
        {

        }

        public PistonObject(GameObject gameObject) : base(gameObject)
        {
            var pistonPlatform = gameObject.GetComponent<SBM.Objects.World5.PistonPlatform>();

            pistonMaxTravel = pistonPlatform.pistonMaxTravel;
            pistonShaftLength = pistonPlatform.extraShaftLength;
        }
    }
}