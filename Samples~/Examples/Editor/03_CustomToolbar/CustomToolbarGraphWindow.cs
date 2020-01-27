using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;

public class CustomToolbarGraphWindow : BaseGraphWindow
{

	[MenuItem("Window/03_CustomContextMenu")]
	public static BaseGraphWindow Open()
	{
		var graphWindow = GetWindow< CustomToolbarGraphWindow >();

		graphWindow.Show();

		return graphWindow;
	}

	protected override void InitializeWindow(BaseGraph graph)
	{
		titleContent = new GUIContent("Custom Toolbar Graph");

		var graphView = new CustomToolbarGraphView(this);

		rootView.Add(graphView);

		graphView.Add(new CustomToolbarView(graphView));
	}
}
