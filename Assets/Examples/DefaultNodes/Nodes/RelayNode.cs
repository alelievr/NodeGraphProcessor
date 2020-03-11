using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System;

[System.Serializable, NodeMenuItem("Custom/Relay")]
public class RelayNode : BaseNode
{
	[Input(name = "In")]
    public object	input;

	[Output(name = "Out")]
	public object	output;

	protected override void Enable()
	{
		onAfterEdgeConnected += EdgeConnected;
		onAfterEdgeDisconnected += EdgeDisconnected;
	}

	protected override void Process() => output = input;

	public override string layoutStyle => "CustomStyles/RelayNode";

	[SerializeField]
	Type	relayType = typeof(object);

	void EdgeConnected(SerializableEdge edge)
	{
		if (edge.inputPort.portData.identifier == "0")
			relayType = edge.outputPort.portData.displayType ?? edge.outputPort.fieldInfo.FieldType;
		else
			relayType = edge.inputPort.portData.displayType ?? edge.inputPort.fieldInfo.FieldType;
		
		// Sanitize
		relayType = relayType ?? typeof(object);
	}

	void EdgeDisconnected(SerializableEdge edge)
	{
		if (edge.inputPort.portData.identifier == "0")
			relayType = edge.outputPort.portData.displayType;
		else
			relayType = edge.inputPort.portData.displayType;
		relayType = typeof(object);
	}

	[CustomPortBehavior(nameof(input))]
	IEnumerable< PortData > InputPortBehavior(List< SerializableEdge > edges)
	{
		yield return new PortData {
			displayName = "",
			displayType = relayType,
			identifier = "0",
		};
	}

	[CustomPortBehavior(nameof(output))]
	IEnumerable< PortData > OutputPortBehavior(List< SerializableEdge > edges)
	{
		yield return new PortData {
			displayName = "",
			displayType = relayType,
			identifier = "1",
			acceptMultipleEdges = true,
		};
	}
}
