using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using UnityEditor.ProjectWindowCallback;

namespace GraphProcessor
{
	/// <summary>
	/// To add the menu items that create node C# script templates files you can inherit from this class and use it's API combined with [MenuItem]
	/// See GraphProcessorMenuItems.cs in examples for implementation details
	/// </summary>
	public class NodeGraphProcessorMenuItems
	{
		static readonly string		nodeBaseName = "Node.cs";
		static readonly string		nodeViewBaseName = "NodeView.cs";
        static string      _nodeTemplatePath = null;
        static string      nodeTemplatePath
		{
			get
			{
				if (_nodeTemplatePath == null)
				{
					var template = Resources.Load<TextAsset>("NodeTemplate.cs");
					_nodeTemplatePath = AssetDatabase.GetAssetPath(template);
				}
				return _nodeTemplatePath;
			}
		}
        static string      _nodeViewTemplatePath;
        static string      nodeViewTemplatePath
		{
			get
			{
				if (_nodeViewTemplatePath == null)
				{
					var template = Resources.Load<TextAsset>("NodeViewTemplate.cs");
					_nodeViewTemplatePath = AssetDatabase.GetAssetPath(template);
				}
				return _nodeViewTemplatePath;
			}
		}

		protected static class MenuItemPosition
		{
			public const int afterCreateScript = 81;
			public const int beforeCreateScript = 79;
		}

        protected static string GetCurrentProjectWindowPath()
        {
			var path = "";
			var obj = Selection.activeObject;

			if (obj == null)
                return null;
			else
				path = AssetDatabase.GetAssetPath(obj.GetInstanceID());

			if (path.Length > 0)
			{
				if (Directory.Exists(path))
					return path;
				else
					return new FileInfo(path).Directory.FullName;
			}
			return null;
        }

		protected static void CreateDefaultNodeCSharpScritpt()
		{
			ProjectWindowUtil.CreateScriptAssetFromTemplateFile(nodeTemplatePath, nodeBaseName);
		}

		protected static void CreateDefaultNodeViewCSharpScritpt()
		{
			ProjectWindowUtil.CreateScriptAssetFromTemplateFile(nodeViewTemplatePath, nodeViewBaseName);
		}
	}
}
