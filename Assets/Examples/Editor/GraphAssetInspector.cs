using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;

[CustomEditor(typeof(BaseGraph))]
public class GraphAssetInspector : Editor
{
	public override void OnInspectorGUI()
	{
		if (GUILayout.Button("Open base graph window"))
			DefaultGraphWindow.Open().InitializeGraph(target as BaseGraph);
		if (GUILayout.Button("Open custom context menu graph window"))
			CustomContextMenuGraphWindow.Open().InitializeGraph(target as BaseGraph);
		if (GUILayout.Button("Open custom toolbar graph window"))
			CustomToolbarGraphWindow.Open().InitializeGraph(target as BaseGraph);
		if (GUILayout.Button("Open exposed properties graph window"))
			ExposedPropertiesGraphWindow.Open().InitializeGraph(target as BaseGraph);
	}
}
