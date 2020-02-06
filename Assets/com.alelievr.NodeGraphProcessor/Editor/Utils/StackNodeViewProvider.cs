using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace GraphProcessor
{
	public static class StackNodeViewProvider
	{
		static Dictionary< Type, Type >		stackNodeViewPerType = new Dictionary< Type, Type >();

        static StackNodeViewProvider()
        {
            foreach (var t in TypeCache.GetTypesWithAttribute<CustomStackNodeView>())
            {
                var attr = t.GetCustomAttributes(false).Select(a => a as CustomStackNodeView).FirstOrDefault();

                stackNodeViewPerType.Add(attr.stackNodeType, t);
                // Debug.Log("Add " + attr.stackNodeType);
            }
        }

        public static Type GetStackNodeCustomViewType(Type stackNodeType)
        {
            // Debug.Log(stackNodeType);
            foreach (var t in stackNodeViewPerType)
            {
                // Debug.Log(t.Key + " -> " + t.Value);
            }
            stackNodeViewPerType.TryGetValue(stackNodeType, out var view);
            return view;
        }
    }
}