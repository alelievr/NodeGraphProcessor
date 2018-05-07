using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
	public class GraphProcessor : ScriptableObject
	{
		public new string			name;

		public List< NodeAsset >	nodes;

		public void AddNode(NodeAsset node)
		{
			nodes.Add(node);

			#if UNITY_EDITOR
				UnityEditor.AssetDatabase.AddObjectToAsset(node, this);
			#endif
		}

		public void RemoveNode(NodeAsset node)
		{
			nodes.Remove(node);

			#if UNITY_EDITOR
				UnityEditor.Undo.DestroyObjectImmediate(node);
			#else
				Destroy(node);
			#endif
		}
	}
}