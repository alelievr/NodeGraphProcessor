using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Custom/MultiAdd")]
public class MultiAddNode : BaseNode
{
	[Input(name = "In Values", allowMultiple = true)]
	public IEnumerable< float >	inputs = null;

	[Output]
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
		foreach (var edgeOfInput in edges)
		{
			PortData pd;
			 // unique port key that will be serialized into the edges and used to re-connect the
			 // the edges when the graph is reloaded
			pd.name = edgeOfInput.GUID;
			pd.type = typeof(float);

			yield return pd;
		}

		// Dummy last port to allow connecting additional edges
		yield return new PortData{ name = "", type = typeof(float)};
	}

	[CustomPortInput(nameof(inputs), typeof(float), allowCast = true)]
	public void GetInputs(List< SerializableEdge > edges)
	{
		inputs = edges.Select(e => (float)e.passThroughBuffer);
	}
}
