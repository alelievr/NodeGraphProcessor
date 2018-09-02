using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using UnityEditor.ProjectWindowCallback;

namespace ProceduralWorlds.Editor
{
	public static class NodeScriptMenuItem
	{
		static readonly string		nodeBaseName = "Node.cs";
		static readonly string		nodeViewBaseName = "NodeView.cs";
        static readonly string      nodeTemplatePath = "Assets/NodeGraphProcessor/Editor/Callbacks/NodeTemplate.cs.txt";
        static readonly string      nodeViewTemplatePath = "Assets/NodeGraphProcessor/Editor/Callbacks/NodeViewTemplate.cs.txt";

        static string GetCurrentPath()
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
	
		[MenuItem("Assets/Create/Node C# Script", false, 20)]
		private static void CreateNodeCSharpScritpt()
		{
			string	path = GetCurrentPath() + "/" + nodeBaseName;
			path = AssetDatabase.GenerateUniqueAssetPath(path);

			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
				0,
				ScriptableObject.CreateInstance< DoCreateNodeScript >(),
				path,
				EditorGUIUtility.FindTexture("cs Script Icon"),
				Path.GetFullPath(nodeTemplatePath)
			);

			AssetDatabase.Refresh();
		}
        
		[MenuItem("Assets/Create/Node View C# Script", false, 21)]
		private static void CreateNodeViewCSharpScritpt()
		{
			string	path = GetCurrentPath() + "/" + nodeViewBaseName;
			path = AssetDatabase.GenerateUniqueAssetPath(path);

			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
				0,
				ScriptableObject.CreateInstance< DoCreateNodeScript >(),
				path,
				EditorGUIUtility.FindTexture("cs Script Icon"),
				Path.GetFullPath(nodeViewTemplatePath)
			);

			AssetDatabase.Refresh();
		}

        class DoCreateNodeScript : EndNameEditAction
        {
            static MethodInfo	createScriptAsset = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetFromTemplate", BindingFlags.Static | BindingFlags.NonPublic);

            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                if (!File.Exists(resourceFile))
                {
                    Debug.LogError("Can't find template: " + resourceFile);
                    return ;
                }

                createScriptAsset.Invoke(null, new object[]{ pathName, resourceFile });

                var asset = AssetDatabase.LoadAssetAtPath(pathName, typeof(MonoScript));
                ProjectWindowUtil.ShowCreatedAsset(asset);

                AssetDatabase.Refresh();
            }
        }
	}
}
