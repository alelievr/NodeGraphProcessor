using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;

public class DefaultGraphWindow : BaseGraphWindow
{
	
	[MenuItem("Window/01_DefaultGraph")]
	public static void Open()
	{
		var graphWindow = GetWindow< DefaultGraphWindow >();

		var graphView = new BaseGraphView();

		graphWindow.rootView.Add(graphView);

		graphWindow.Show();
	}

}
