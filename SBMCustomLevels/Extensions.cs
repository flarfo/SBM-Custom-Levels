using System;
using System.Collections.Generic;
using System.Reflection;
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

        public static void SetPropertyValue(object target, string propertyName, object value)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            Type targetType = target.GetType();
            PropertyInfo propertyInfo = targetType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (propertyInfo == null)
            {
                throw new MissingMemberException(targetType.FullName, propertyName);
            }

            MethodInfo setMethod = propertyInfo.GetSetMethod(true);

            if (setMethod == null)
            {
                throw new MissingMethodException(targetType.FullName, $"set_{propertyName}");
            }

            setMethod.Invoke(target, new object[] { value });
        }

        public static void SetStaticPropertyValue(Type targetType, string propertyName, object value)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException(nameof(targetType));
            }

            PropertyInfo propertyInfo = targetType.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (propertyInfo == null)
            {
                throw new MissingMemberException(targetType.FullName, propertyName);
            }

            MethodInfo setMethod = propertyInfo.GetSetMethod(true);

            if (setMethod == null)
            {
                throw new MissingMethodException(targetType.FullName, $"set_{propertyName}");
            }

            setMethod.Invoke(null, new object[] { value });
        }
    }
}
