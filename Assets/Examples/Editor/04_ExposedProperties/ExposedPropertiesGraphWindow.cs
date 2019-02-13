using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;

public class ExposedPropertiesGraphWindow : BaseGraphWindow
{

	[MenuItem("Window/03_CustomContextMenu")]
	public static BaseGraphWindow Open()
	{
		var graphWindow = GetWindow< ExposedPropertiesGraphWindow >();

		graphWindow.Show();

		return graphWindow;
	}

	protected override void Initialize(BaseGraph graph)
	{
		titleContent = new GUIContent("Properties Graph");

		var graphView = new ExposedPropertiesGraphView();

		rootView.Add(graphView);

		graphView.OpenPinned< ExposedParameterView >();
	}
}
