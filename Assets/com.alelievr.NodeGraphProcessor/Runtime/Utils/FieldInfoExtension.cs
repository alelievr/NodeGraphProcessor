using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GraphProcessor
{
    public static class FieldInfoExtension
    {
        public static bool HasCustomAttribute<T>(this FieldInfo fieldInfo)
        {
            return Attribute.IsDefined(fieldInfo, typeof(T));
        }

        public static bool HasCustomAttribute(this FieldInfo fieldInfo, Type type)
        {
            return Attribute.IsDefined(fieldInfo, type);
        }

        public static object GetValueAt(this IList<FieldInfo> list, object startingValue, int index)
        {
            object currentValue = startingValue;
            for (int i = 0; i < list.Count; i++)
            {
                currentValue = list[i].GetValue(currentValue);
                if (i == index) break;
            }
            return currentValue;
        }

        public static object GetFinalValue(this IList<FieldInfo> list, object startingValue)
        {
            object currentValue = startingValue;
            for (int i = 0; i < list.Count; i++)
            {
                currentValue = list[i].GetValue(currentValue);
            }
            return currentValue;

        }

        public static string GetPath(this IList<FieldInfo> list)
        {
            string path = "";
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0) path += ".";
                path += list[i].Name;
            }
            return path;
        }

        public static bool IsValid(this IList<FieldInfo> list)
        {
            return list.Any(x => x == null);
        }
    }
}