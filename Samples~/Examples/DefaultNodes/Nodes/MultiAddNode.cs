using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Custom/MultiAdd")]
public class MultiAddNode : BaseNode
{
	[Input]
	public IEnumerable< float >	inputs = null;

	[Output(allowMultiple = false)]
	public float				output;

	public override string		name => "Add";

	protected override void Process()
	{
		output = 0;

		if (inputs == null)
			return ;

		foreach (float input in inputs)
			output += input;
	}

	[CustomPortBehavior(nameof(inputs))]
	IEnumerable< PortData > GetPortsForInputs(List< SerializableEdge > edges)
	{
		int index = 0;

		// We wont have edges in the final API
		// foreach (var edgeOfInput in edges)
		// {
        //     // unique port key that will be serialized into the edges and used to re-connect the
        //     // the edges when the graph is reloaded
        //     yield return new PortData {
		// 		displayName = "In " + index,
		// 		displayType = typeof(float),
		// 		identifier = edgeOfInput.GUID
		// 	};
		// }

		// Dummy last port to allow connecting additional edges
		yield return new PortData{ displayName = "In " + index, displayType = typeof(float), acceptMultipleEdges = true};
	}

	[CustomPortInput(nameof(inputs), typeof(float), allowCast = true)]
	public void GetInputs(List< SerializableEdge > edges)
	{
		inputs = edges.Select(e => (float)e.passThroughBuffer);
	}
}
