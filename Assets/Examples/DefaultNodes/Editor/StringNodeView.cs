using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using System.Linq;

[NodeCustomEditor(typeof(StringNode))]
public class StringNodeView : BaseNodeView
{
	public override void Enable()
	{
		var node = nodeTarget as StringNode;

		var textArea = new TextField(-1, true, false, '*') { value = node.output };
		textArea.Children().First().style.unityTextAlign = TextAnchor.UpperLeft;
		textArea.style.width = 200;
		textArea.style.height = 100;
		textArea.RegisterValueChangedCallback(v => {
			owner.RegisterCompleteObjectUndo("Edit string node");
			node.output = v.newValue;
		});
		controlsContainer.Add(textArea);
	}
}