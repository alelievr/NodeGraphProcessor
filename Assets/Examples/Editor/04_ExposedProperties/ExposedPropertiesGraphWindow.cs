using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;

public class ExposedPropertiesGraphWindow : BaseGraphWindow
{
	BaseGraph	tmpGraph;

	[MenuItem("Window/04 Exposed Properties")]
	public static BaseGraphWindow OpenWithTmpGraph()
	{
		var graphWindow = CreateWindow< ExposedPropertiesGraphWindow >();

		// When the graph is opened from the window, we don't save the graph to disk
		graphWindow.tmpGraph = ScriptableObject.CreateInstance<BaseGraph>();
		graphWindow.tmpGraph.hideFlags = HideFlags.HideAndDontSave;
		graphWindow.InitializeGraph(graphWindow.tmpGraph);

		graphWindow.Show();

		return graphWindow;
	}

	protected override void OnDestroy()
	{
		graphView?.Dispose();
		DestroyImmediate(tmpGraph);
	}

	protected override void InitializeWindow(BaseGraph graph)
	{
		titleContent = new GUIContent("Properties Graph");

		if (graphView == null)
			graphView = new ExposedPropertiesGraphView(this);

		rootView.Add(graphView);
	}

	protected override void InitializeGraphView(BaseGraphView view)
	{
		view.OpenPinned< ExposedParameterView >();
	}
}
