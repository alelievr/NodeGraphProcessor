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

	[CustomPortInput(nameof(inputs), typeof(float), allowCast = true)]
	public void GetInputs(List< SerializableEdge > edges)
	{
		inputs = edges.Select(e => (float)e.passThroughBuffer);
	}
}
