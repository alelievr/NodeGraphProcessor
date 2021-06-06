using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;

[NodeCustomEditor(typeof(FloatNode))]
public class FloatNodeView : BaseNodeView
{
	public override void Enable()
	{
		var floatNode = nodeTarget as FloatNode;

		DoubleField floatField = new DoubleField
		{
			value = floatNode.input
		};

		floatNode.onProcessed += () => floatField.value = floatNode.input;

		floatField.RegisterValueChangedCallback((v) => {
			owner.RegisterCompleteObjectUndo("Updated floatNode input");
			floatNode.input = (float)v.newValue;
		});

		controlsContainer.Add(floatField);
	}
}