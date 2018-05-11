using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;

namespace GraphProcessor
{
	[NodeCustomEditor(typeof(IntNode))]
	public class IntNodeView : BaseNodeView
	{
		public override void Enable()
		{
			var intNode = nodeTarget as IntNode;
			
			IntegerField	intField = new IntegerField();

			intField.value = intNode.output;

			intField.OnValueChanged((v) => {
				Debug.Log("Value: " + v.newValue);
				Debug.Log("IntNode value: " + intNode.output);
				intNode.output = (int)v.newValue;
				owner.graph.RegisterCompleteObjectUndo("Updated IntNode output");
			});

			Debug.Log("mainContainerName: " + mainContainer.name);
			Debug.Log("extentionContainerName: " + extensionContainer.name);

			controlsContainer.Add(intField);
		}
	}
}
