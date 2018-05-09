using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;

public class CustomContextMenuGraphWindow : BaseGraphWindow
{
	
	[MenuItem("Window/02_CustomContextMenu")]
	public static void Open()
	{
		var graphWindow = GetWindow< CustomContextMenuGraphWindow >();

		var graphView = new CustomContextMenuGraphView();

		graphWindow.rootView.Add(graphView);

		graphView.AddElement(new MiniMapView());

		// graphView.AddElement(new NodeSelectorMenu());

		// graphView.AddElement(new HelloWorld()); //Comment block

		graphWindow.Show();
	}

	protected override void Initialize(BaseGraph graph)
	{
		
	}

}
