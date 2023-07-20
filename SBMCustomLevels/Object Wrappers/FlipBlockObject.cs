using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace SBM_CustomLevels
{
    [Serializable]
    public class FlipBlockObject : MeshSliceObject
    {
        public bool[] spikesEnabled;
        public bool direction; // 0 -> left, 1 -> right

        public float flipDegrees;
        public float flipTime;

        [JsonConstructor]
        public FlipBlockObject()
        {

        }

        public FlipBlockObject(GameObject gameObject) : base(gameObject)
        {
            var flipBlock = gameObject.GetComponent<SBM.Objects.World5.FlipBlock>();

            spikesEnabled = flipBlock.spikesEnabled;
            direction = (flipBlock.direction == SBM.Objects.World5.FlipBlock.FlipDirection.Right);

            flipDegrees = flipBlock.degreesPerFlip;
            flipTime = flipBlock.timeBetweenFlips;
        }
    }
}