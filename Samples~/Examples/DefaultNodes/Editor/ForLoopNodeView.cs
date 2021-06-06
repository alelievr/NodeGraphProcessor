using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;

[NodeCustomEditor(typeof(ForLoopNode))]
public class ForLoopNodeView : BaseNodeView
{
	public override void Enable()
	{
		var node = nodeTarget as ForLoopNode;

		DrawDefaultInspector();

        // Create your fields using node's variables and add them to the controlsContainer

		// controlsContainer.Add(new Label("Hello World !"));
	}
}