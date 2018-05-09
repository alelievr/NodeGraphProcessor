using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;

public class GraphProcessorWindow : BaseGraphProcessorWindow
{
	
	[MenuItem("Window/GraphProcessor")]
	public static void Open()
	{
		GetWindow< GraphProcessorWindow >().Show();
	}

}
