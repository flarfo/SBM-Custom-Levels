using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBM_CustomLevels.Objects
{
    public class SplineMakerParent : MonoBehaviour
    {
        public List<SplineMakerNodeData> nodes = new List<SplineMakerNodeData>();
        public SplineMaker spline;
    }
}
