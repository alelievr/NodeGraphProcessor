using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;

public class DefaultGraphWindow : BaseGraphWindow
{

	[MenuItem("Window/01_DefaultGraph")]
	public static BaseGraphWindow Open()
	{
		var graphWindow = GetWindow< DefaultGraphWindow >();

		graphWindow.Show();

		return graphWindow;
	}

	protected override void Initialize(BaseGraph graph)
	{
		titleContent = new GUIContent("Default Graph");

		var graphView = new BaseGraphView();
		graphView.Initialize(graph);

		rootView.Add(graphView);
	}
}
