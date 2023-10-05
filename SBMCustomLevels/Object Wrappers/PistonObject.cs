using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using SBM_CustomLevels.Objects;

namespace SBM_CustomLevels.ObjectWrappers
{
    [Serializable]
    public class PistonObject : MeshSliceObject
    {
        public float pistonMaxTravel;
        public float pistonShaftLength;
        public List<Keyframe> keyframes;

        [JsonConstructor]
        public PistonObject()
        {

        }

        public PistonObject(GameObject gameObject) : base(gameObject)
        {
            objectType = ObjectType.Piston;

            var pistonPlatform = gameObject.GetComponent<SBM.Objects.World5.PistonPlatform>();
            pistonMaxTravel = pistonPlatform.pistonMaxTravel;
            pistonShaftLength = pistonPlatform.extraShaftLength;

            var pistonData = gameObject.GetComponent<PistonDataContainer>();

            keyframes = new List<Keyframe>();
            for (int i = 0; i < pistonData.keyframes.Count; i++)
            {
                // 'value' item in keyframe should not be greater than 1 for pistons, since the AnimationCurve is assumed to be normalized
                if (pistonData.keyframes[i].value > 1)
                {
                    keyframes.Add(new Keyframe(pistonData.keyframes[i].time, 1));
                }
                else
                {
                    keyframes.Add(pistonData.keyframes[i]);
                }
                
            }
        }
    }
}