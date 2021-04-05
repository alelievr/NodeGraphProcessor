using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;

[NodeCustomEditor(typeof(MultiAddNode))]
public class MultiAddNodeView : BaseNodeView
{
	public override void Enable()
	{
		var floatNode = nodeTarget as MultiAddNode;

		DoubleField floatField = new DoubleField
		{
			value = floatNode.output
		};

		// Update the UI value after each processing
		nodeTarget.onProcessed += () => floatField.value = floatNode.output;

		controlsContainer.Add(floatField);
	}
}