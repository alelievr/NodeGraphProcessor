using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;

[System.Serializable, NodeMenuItem("Primitives/Add")]
public class AddNode : BaseNode
{
	[Input("Input")]
	public PortArray< float >	inputs;

	[Output]
	public float				output;

	public override string		name { get { return "Add"; } }

	public override void Process()
	{
		output = 0;

		foreach (float input in inputs)
			output += input;
	}
}
