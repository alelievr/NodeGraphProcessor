using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Custom/MultiPorts")]
public class CustomPortsNode : BaseNode
{
    [Input]
	public MultiPorts       	inputs;

	[Output]
	public MultiPorts			output;

	List< object >				values;

	public override string		name => "CustomPorts";

	protected override void Process()
	{
		// do things with values
	}

	[CustomPortInput(nameof(inputs), typeof(float))]
	void PullInputs(List< SerializableEdge > inputEdges)
	{
		values = inputEdges.Select(e => e.passThroughBuffer).ToList();
	}

	void PushOutputs(List< SerializableEdge > connectedEdges)
	{
		// Values length is supposed to match connected edges length
		for (int i = 0; i < connectedEdges.Count; i++)
			connectedEdges[i].passThroughBuffer = values[i];
	}
}
