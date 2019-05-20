using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace GraphProcessor
{
	[ExecuteAlways]
	public class DeleteCallback : UnityEditor.AssetModificationProcessor
	{
		static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
		{
			var graph = AssetDatabase.LoadAssetAtPath(path, typeof(BaseGraph));

			if (graph != null)
			{
				foreach (var graphWindow in Resources.FindObjectsOfTypeAll< BaseGraphWindow >())
					graphWindow.OnGraphDeleted();
			}

			return AssetDeleteResult.DidNotDelete;
		}
	}

}