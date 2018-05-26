using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;

[System.Serializable, NodeMenuItem("Primitives/Add")]
public class AddNode : BaseNode
{
	[Input("Input", nameof(PullInputFields))]
	public PortArray< float >	inputs = new PortArray< float >();

	[Output]
	public float				output;

	public override string		name { get { return "Add"; } }

	public int					test;

	protected override void Process()
	{
		output = 0;

		foreach (float input in inputs)
			output += input;
	}

	void PullInputFields(IEnumerable< object > values)
	{
		foreach (var value in values)
			inputs.Add((float)value);
	}
}
