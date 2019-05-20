using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Custom/MultiPorts")]
public class CustomPortsNode : BaseNode
{
    [Input]
	public List< float >       	inputs;

	[Output]
	public List< float >		outputs; // TODO: custom function for this one

	List< object >				values;

	public override string		name => "CustomPorts";

	// We keep the max port count so it doesn't cause binding issues
	[SerializeField, HideInInspector]
	int							portCount = 1;

	protected override void Process()
	{
		// do things with values
	}

	[CustomPortBehavior(nameof(inputs))]
	IEnumerable< PortData > ListPortBehavior(List< SerializableEdge > edges)
	{
		portCount = Mathf.Max(portCount, edges.Count + 1);

		for (int i = 0; i < portCount; i++)
		{
			yield return new PortData {
				displayName = "In " + i,
				displayType = typeof(float),
				identifier = i.ToString(), // Must be unique
			};
		}
	}

	[CustomPortInput(nameof(inputs), typeof(float))]
	void PullInputs(List< SerializableEdge > inputEdges)
	{
		values = inputEdges.Select(e => e.passThroughBuffer).ToList();
	}

	[CustomPortOutput(nameof(outputs), typeof(float))]
	void PushOutputs(List< SerializableEdge > connectedEdges)
	{
		// Values length is supposed to match connected edges length
		for (int i = 0; i < connectedEdges.Count; i++)
			connectedEdges[i].passThroughBuffer = values[i];
	}
}
