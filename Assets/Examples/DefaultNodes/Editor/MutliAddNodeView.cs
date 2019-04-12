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

		controlsContainer.Add(floatField);
	}

	[CustomPortView(nameof(MultiAdNode.inputs))]
	public IEnumerable< PortView > Multiport(NodePort port, PortCreationAttributes attrs)
	{
		int index = 0;
		string portName = "Input ";

		foreach (var edge in port.GetEdges())
		{
			yield return AddPort(portName + index++, attrs);
		}

		yield return AddPort(portName + index, attrs);
	}

	public override void OnPortConnected(PortView port)
	{
		// re-create ports:

	}

	public override void OnPortDisconnected(PortView port)
	{

	}
}