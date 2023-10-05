using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBM_CustomLevels.Extensions
{
    public static class Extensions
    {
        public static bool TryGetComponentInParent<T>(this Transform gameObject, out T component) where T : Component
        {
            component = gameObject.GetComponentInParent<T>();

            if (component)
            {
                return true;
            }

            return false;
        }
    }
}
