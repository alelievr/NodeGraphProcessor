using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using GraphProcessor;

[NodeCustomEditor(typeof(FloatNode))]
public class FloatNodeView : BaseNodeView
{
	public override void Enable()
	{
		var floatNode = nodeTarget as FloatNode;

		DoubleField floatField = new DoubleField
		{
			value = floatNode.output
		};

		floatField.OnValueChanged((v) => {
			floatNode.output = (float)v.newValue;
			owner.RegisterCompleteObjectUndo("Updated floatNode output");
		});

		controlsContainer.Add(floatField);
	}
}