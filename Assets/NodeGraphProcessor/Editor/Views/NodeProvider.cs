using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace GraphProcessor
{
	public static class NodeProvider
	{
		static Dictionary< Type, Type >		nodeViewPerType = new Dictionary< Type, Type >();
		static Dictionary< string, Type >	nodePerMenuTitle = new Dictionary< string, Type >();

		static NodeProvider()
		{
            // First build up temporary data structure containing group & title as an array of strings (the last one is the actual title) and associated node type.
			foreach (var type in GetAllTypesInCurrentDomain())
			{
				if (type.IsClass && !type.IsAbstract)
				{
					if (type.IsSubclassOf(typeof(BaseNode)))
						AddNodeType(type);
					if (type.IsSubclassOf(typeof(BaseNodeView)))
						AddNodeViewType(type);
				}
            }
		}

		static IEnumerable< Type >	GetAllTypesInCurrentDomain()
		{
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
				Type[] types = {};
				
                try {
					types = assembly.GetTypes();
				} catch {
					//just ignore it ...
				}

				foreach (var type in types)
					yield return type;
			}
		}

		static void AddNodeType(Type type)
		{
			var attrs = type.GetCustomAttributes(typeof(NodeMenuItemAttribute), false) as NodeMenuItemAttribute[];

			if (attrs != null && attrs.Length > 0)
			{
				Debug.Log("Node found: " + type);
				//TODO: add this type to the dictionary
            }
		}

		static void	AddNodeViewType(Type type)
		{
			var attrs = type.GetCustomAttributes(typeof(NodeCustomEditor), false) as NodeCustomEditor[];

			if (attrs != null && attrs.Length > 0)
			{
				Debug.Log("Detected custom node editor: " + type);
				nodeViewPerType[attrs.First().nodeType] = type;
            }
		}

		public static Type GetNodeViewTypeFromType(Type nodeType)
		{
			Type	view;

			nodeViewPerType.TryGetValue(nodeType, out view);

			return view;
		}
	}
}
