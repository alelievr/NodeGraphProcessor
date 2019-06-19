using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UIElements;
using System.Linq;
using System.IO;
using System.Reflection;

namespace GraphProcessor
{
	public static class NodeProvider
	{
		static Dictionary< Type, Type >		nodeViewPerType = new Dictionary< Type, Type >();
		static Dictionary< string, Type >	nodePerMenuTitle = new Dictionary< string, Type >();
		static Dictionary< Type, string >	nodeViewScripts = new Dictionary< Type, string >();
		static Dictionary< Type, string >	nodeScripts = new Dictionary< Type, string >();
		static List< Type >					slotTypes = new List< Type >();

		static NodeProvider()
		{
			foreach (var type in AppDomain.CurrentDomain.GetAllTypes())
			{
				if (type.IsClass && !type.IsAbstract)
				{
					if (type.IsSubclassOf(typeof(BaseNode)))
						AddNodeType(type);
					if (type.IsSubclassOf(typeof(BaseNodeView)))
						AddNodeViewType(type);

					foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
					{
						if (field.GetCustomAttributes().Any(c => c is InputAttribute || c is OutputAttribute))
							slotTypes.Add(field.FieldType);
					}
				}
            }
		}

		static void AddNodeType(Type type)
		{
			var attrs = type.GetCustomAttributes(typeof(NodeMenuItemAttribute), false) as NodeMenuItemAttribute[];

			if (attrs != null && attrs.Length > 0)
				nodePerMenuTitle[attrs.First().menuTitle] = type;

			var nodeScriptAsset = FindScriptFromClassName(type.Name);
			if (nodeScriptAsset != null)
				nodeScripts[type] = nodeScriptAsset;
		}

		static void	AddNodeViewType(Type type)
		{
			var attrs = type.GetCustomAttributes(typeof(NodeCustomEditor), false) as NodeCustomEditor[];

			if (attrs != null && attrs.Length > 0)
			{
				Type nodeType = attrs.First().nodeType;
				nodeViewPerType[nodeType] = type;

				var nodeViewScriptAsset = FindScriptFromClassName(type.Name);

				if (nodeViewScriptAsset != null)
					nodeViewScripts[type] = nodeViewScriptAsset;
			}
		}

		static string FindScriptFromClassName(string className)
		{
			var scriptGUIDs = AssetDatabase.FindAssets(className);

			if (scriptGUIDs.Length == 0)
				return null;

			foreach (var scriptGUID in scriptGUIDs)
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(scriptGUID);
				if (className == Path.GetFileNameWithoutExtension(assetPath))
					return assetPath;
			}

			return null;
		}

		public static Type GetNodeViewTypeFromType(Type nodeType)
		{
			Type	view;

			if (nodeViewPerType.TryGetValue(nodeType, out view))
				return view;

			// Allow for inheritance in node views: multiple C# node using the same view
			foreach (var type in nodeViewPerType)
			{
				if (nodeType.IsSubclassOf(type.Key))
					return type.Value;
			}

			return view;
		}

		public static Dictionary< string, Type >	GetNodeMenuEntries()
		{
			return nodePerMenuTitle;
		}

		public static string GetNodeViewScript(Type type)
		{
			string scriptPath;

			nodeViewScripts.TryGetValue(type, out scriptPath);

			return scriptPath;
		}

		public static string GetNodeScript(Type type)
		{
			string scriptPath;

			nodeScripts.TryGetValue(type, out scriptPath);

			return scriptPath;
		}

		public static List<Type> GetSlotTypes()
		{
			return slotTypes;
		}
	}
}
