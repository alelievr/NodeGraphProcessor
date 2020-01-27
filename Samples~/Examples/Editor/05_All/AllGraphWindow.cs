using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;

public class AllGraphWindow : BaseGraphWindow
{
	[MenuItem("Window/03_CustomContextMenu")]
	public static BaseGraphWindow Open()
	{
		var graphWindow = GetWindow< AllGraphWindow >();

		graphWindow.Show();

		return graphWindow;
	}

	protected new void OnEnable()
	{
		base.OnEnable();
		// graphLoaded += g => Debug.Log("Load: " + g);
		// graphUnloaded += g => Debug.Log("Unload: " + g);
	}

	protected override void InitializeWindow(BaseGraph graph)
	{
		titleContent = new GUIContent("All Graph");

		var graphView = new AllGraphView(this);

		rootView.Add(graphView);

		graphView.Add(new CustomToolbarView(graphView));
	}

	protected override void InitializeGraphView(BaseGraphView view)
	{
		view.OpenPinned< ExposedParameterView >();
	}
}
