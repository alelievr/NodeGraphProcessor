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
        static readonly string      nodeTemplatePath = "NodeTemplate.cs";
        static readonly string      nodeViewTemplatePath = "NodeViewTemplate.cs";

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

		[MenuItem("Assets/Create/Node C# Script", false, 81)]
		private static void CreateNodeCSharpScritpt()
		{
			string	path = GetCurrentPath() + "/" + nodeBaseName;
			path = AssetDatabase.GenerateUniqueAssetPath(path);

			var template = Resources.Load<TextAsset>(nodeTemplatePath);
			string templatePath = AssetDatabase.GetAssetPath(template);

			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
				0,
				ScriptableObject.CreateInstance< DoCreateNodeScript >(),
				path,
				EditorGUIUtility.FindTexture("cs Script Icon"),
				Path.GetFullPath(templatePath)
			);

			AssetDatabase.Refresh();
		}

		[MenuItem("Assets/Create/Node View C# Script", false, 82)]
		private static void CreateNodeViewCSharpScritpt()
		{
			string	path = GetCurrentPath() + "/" + nodeViewBaseName;
			path = AssetDatabase.GenerateUniqueAssetPath(path);

			var template = Resources.Load<TextAsset>(nodeViewTemplatePath);
			string templatePath = AssetDatabase.GetAssetPath(template);

			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
				0,
				ScriptableObject.CreateInstance< DoCreateNodeScript >(),
				path,
				EditorGUIUtility.FindTexture("cs Script Icon"),
				Path.GetFullPath(templatePath)
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
