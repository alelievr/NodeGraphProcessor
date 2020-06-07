using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;

public class AllGraphWindow : BaseGraphWindow
{
	BaseGraph	tmpGraph;

	[MenuItem("Window/05 All Combined")]
	public static BaseGraphWindow OpenWithTmpGraph()
	{
		var graphWindow = CreateWindow< AllGraphWindow >();

		// When the graph is opened from the window, we don't save the graph to disk
		graphWindow.tmpGraph = ScriptableObject.CreateInstance<BaseGraph>();
		graphWindow.tmpGraph.hideFlags = HideFlags.HideAndDontSave;
		graphWindow.InitializeGraph(graphWindow.tmpGraph);

		graphWindow.Show();

		return graphWindow;
	}

	protected override void OnDestroy() => DestroyImmediate(tmpGraph);

	protected override void InitializeWindow(BaseGraph graph)
	{
		titleContent = new GUIContent("All Graph");

		if (graphView == null)
		{
			graphView = new AllGraphView(this);
			graphView.Add(new CustomToolbarView(graphView));
		}

		rootView.Add(graphView);
	}

	protected override void InitializeGraphView(BaseGraphView view)
	{
		view.OpenPinned< ExposedParameterView >();
	}
}
