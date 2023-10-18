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


        //searches all objects of specific name, returns object matching name if found
        public static GameObject FindInactiveGameObject(string name)
        {
            GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();

            for (int i = 0; i < objects.Length; i++)
            {
                if (name == objects[i].name)
                {
                    return objects[i];
                }
            }

            return null;
        }

        //searches all objects of specific type and name, returns object matching name if found
        public static T FindInactiveGameObject<T>(string name) where T : UnityEngine.Object
        {
            T[] objects = Resources.FindObjectsOfTypeAll<T>();

            for (int i = 0; i < objects.Length; i++)
            {
                if (name == objects[i].name)
                {
                    return objects[i];
                }
            }

            return null;
        }
    }
}
