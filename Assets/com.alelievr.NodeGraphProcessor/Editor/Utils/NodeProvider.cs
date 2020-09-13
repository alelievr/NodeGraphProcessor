using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Reflection;
using UnityEditor.Experimental.GraphView;

namespace GraphProcessor
{
	public static class NodeProvider
	{
		public struct PortDescription
		{
			public Type nodeType;
			public Type portType;
			public bool isInput;
			public string portFieldName;
			public string portIdentifier;
			public string portDisplayName;
		}

		static Dictionary< Type, Type >			nodeViewPerType = new Dictionary< Type, Type >();
		static Dictionary< string, Type >		nodePerMenuTitle = new Dictionary< string, Type >();
		static Dictionary< Type, MonoScript >	nodeViewScripts = new Dictionary< Type, MonoScript >();
		static Dictionary< Type, MonoScript >	nodeScripts = new Dictionary< Type, MonoScript >();
		static List< Type >						slotTypes = new List< Type >();
		static List< PortDescription >			nodeCreatePortDescription = new List<PortDescription>();

		static NodeProvider()
		{
			foreach (var nodeType in TypeCache.GetTypesDerivedFrom<BaseNode>())
			{
				if (nodeType.IsAbstract)
					continue;

				AddNodeType(nodeType);

				foreach (var field in nodeType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				{
					if (field.GetCustomAttribute<HideInInspector>() == null && field.GetCustomAttributes().Any(c => c is InputAttribute || c is OutputAttribute))
						slotTypes.Add(field.FieldType);
				}

				ProvideNodePortCreationDescription(nodeType);
			}

			foreach (var nodeViewType in TypeCache.GetTypesDerivedFrom<BaseNodeView>())
			{
				if (!nodeViewType.IsAbstract)
					AddNodeViewType(nodeViewType);
			}
		}

		static void ProvideNodePortCreationDescription(Type nodeType)
		{
			var node = Activator.CreateInstance(nodeType) as BaseNode;
			try {
				node.InitializePorts();
				node.UpdateAllPorts();
			} catch (Exception) {}

			foreach (var p in node.inputPorts)
				AddPort(p, true);
			foreach (var p in node.outputPorts)
				AddPort(p, false);

			void AddPort(NodePort p, bool input)
			{
				nodeCreatePortDescription.Add(new PortDescription{
					nodeType = nodeType,
					portType = p.portData.displayType ?? p.fieldInfo.FieldType,
					isInput = input,
					portFieldName = p.fieldName,
					portDisplayName = p.portData.displayName ?? p.fieldName,
					portIdentifier = p.portData.identifier,
				});
			}
		}

		static void AddNodeType(Type type)
		{
			var attrs = type.GetCustomAttributes(typeof(NodeMenuItemAttribute), false) as NodeMenuItemAttribute[];

			if (attrs != null && attrs.Length > 0)
			{
				foreach (var attr in attrs)
					nodePerMenuTitle[attr.menuTitle] = type;
			}

			var nodeScriptAsset = FindScriptFromClassName(type.Name);

			// Try find the class name with Node name at the end
			if (nodeScriptAsset == null)
				nodeScriptAsset = FindScriptFromClassName(type.Name + "Node");
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
				if (nodeViewScriptAsset == null)
					nodeViewScriptAsset = FindScriptFromClassName(type.Name + "View");
				if (nodeViewScriptAsset == null)
					nodeViewScriptAsset = FindScriptFromClassName(type.Name + "NodeView");

				if (nodeViewScriptAsset != null)
					nodeViewScripts[type] = nodeViewScriptAsset;
			}
		}

		static MonoScript FindScriptFromClassName(string className)
		{
			var scriptGUIDs = AssetDatabase.FindAssets($"t:script {className}");

			if (scriptGUIDs.Length == 0)
				return null;

			foreach (var scriptGUID in scriptGUIDs)
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(scriptGUID);
				var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);

				if (script != null && String.Equals(className, Path.GetFileNameWithoutExtension(assetPath), StringComparison.OrdinalIgnoreCase))
					return script;
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

		public static MonoScript GetNodeViewScript(Type type)
		{
			nodeViewScripts.TryGetValue(type, out var script);

			return script;
		}

		public static MonoScript GetNodeScript(Type type)
		{
			nodeScripts.TryGetValue(type, out var script);

			return script;
		}

		public static List<Type> GetSlotTypes() => slotTypes;

		public static List<PortDescription> GetEdgeCreationNodeMenuEntry(PortView portView)
		{
			return nodeCreatePortDescription.Where(n => {
				if (portView.direction == Direction.Input && n.isInput || portView.direction == Direction.Output && !n.isInput)
					return false;
	
				if (!BaseGraph.TypesAreConnectable(n.portType, portView.portType))
					return false;

				return true;
			}).ToList();
		}
	}
}
