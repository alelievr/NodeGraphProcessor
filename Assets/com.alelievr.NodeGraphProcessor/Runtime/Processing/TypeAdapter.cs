using UnityEngine;
using System;
using System.Collections.Generic;

namespace GraphProcessor
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TypeAdapterAttribute : Attribute
    {
        Type    t1;
        Type    t2;

        public TypeAdapterAttribute(Type t1, Type t2)
        {
            this.t1 = t1;
            this.t2 = t2;
        }
    }

    /// <summary>
    /// Implement this interface to use the [TypeAdapter] attribute on methods inside your class
    /// </summary>
    public interface ITypeAdapter {}

    public static class TypeAdapter
    {
        delegate object ConvertDelegate(object from);

        static Dictionary< (Type, Type), ConvertDelegate > adapters = new Dictionary< (Type, Type), ConvertDelegate >();

        [System.NonSerialized]
        static bool adaptersLoaded = false;

        static void LoadAllAdapters()
        {
            foreach (var type in AppDomain.GetAllTypes())
            {
                if (typeof(ITypeAdapter).IsAssignableFrom(type))
                {
                    type.GetMethods();
                }
            }
        }

        public static bool AreAssignable(Type t1, Type t2)
        {
            if (!adaptersLoaded)
                LoadAllAdapters();

            return adapters.ContainsKey((t1, t2));
        }

        public static object Convert(object from, Type targetType)
        {
            if (!adaptersLoaded)
                LoadAllAdapters();

            ConvertDelegate convertionFunction;
            if (adapters.TryGetValue((from.GetType(), targetType), out convertionFunction))
                return convertionFunction?.Invoke(from);

            return null;
        }
    }
}