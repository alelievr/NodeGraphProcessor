using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;

public class CustomContextMenuGraphWindow : BaseGraphWindow
{

	[MenuItem("Window/02_CustomContextMenu")]
	public static BaseGraphWindow Open()
	{
		var graphWindow = GetWindow< CustomContextMenuGraphWindow >();

		graphWindow.Show();

		return graphWindow;
	}

	protected override void InitializeWindow(BaseGraph graph)
	{
		titleContent = new GUIContent("Context Menu Graph");

		var graphView = new CustomContextMenuGraphView(this);

		rootView.Add(graphView);

		graphView.Add(new MiniMapView(graphView));
	}
}
