using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Experimental.UIElements;
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
			foreach (var type in AppDomain.CurrentDomain.GetAllTypes())
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

		static void AddNodeType(Type type)
		{
			var attrs = type.GetCustomAttributes(typeof(NodeMenuItemAttribute), false) as NodeMenuItemAttribute[];

			if (attrs != null && attrs.Length > 0)
				nodePerMenuTitle[attrs.First().menuTitle] = type;
		}

		static void	AddNodeViewType(Type type)
		{
			var attrs = type.GetCustomAttributes(typeof(NodeCustomEditor), false) as NodeCustomEditor[];

			if (attrs != null && attrs.Length > 0)
				nodeViewPerType[attrs.First().nodeType] = type;
		}

		public static Type GetNodeViewTypeFromType(Type nodeType)
		{
			Type	view;

			nodeViewPerType.TryGetValue(nodeType, out view);

			return view;
		}

		public static Dictionary< string, Type >	GetNodeMenuEntries()
		{
			return nodePerMenuTitle;
		}
	}
}
