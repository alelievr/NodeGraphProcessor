using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;

[NodeCustomEditor(typeof(TypeSwitchNode))]
public class TypeSwitchNodeView : BaseNodeView
{
	public override void Enable()
	{
		var node = nodeTarget as TypeSwitchNode;

		var t = new Toggle("Swith type"){ value = node.toggleType };
		t.RegisterValueChangedCallback(e => {
			node.toggleType = e.newValue;
			ForceUpdatePorts();
		});

		controlsContainer.Add(t);
	}
}