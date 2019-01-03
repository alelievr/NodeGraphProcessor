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
		var graphWindow = GetWindow< CustomContextMenuGraphWindow >();

		graphWindow.Show();

		return graphWindow;
	}

	protected override void Initialize(BaseGraph graph)
	{
		titleContent = new GUIContent("Context Menu Graph");

		var graphView = new CustomContextMenuGraphView();

		rootView.Add(graphView);

		graphView.Add(new MiniMapView(graphView));

		// graphView.AddElement(new NodeSelectorMenu());

		// graphView.AddElement(new HelloWorld()); //Comment block
	}

}
