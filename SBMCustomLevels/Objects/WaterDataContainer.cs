using UnityEngine;
using System.Collections.Generic;

namespace SBM_CustomLevels
{
    public class WaterDataContainer : MonoBehaviour
    {
        public float width = 0;
        public float height = 0;

        public bool w5 = false;

        public List<Keyframe> keyframes = new List<Keyframe>();
    }
}
