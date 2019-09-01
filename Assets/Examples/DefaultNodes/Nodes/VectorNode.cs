using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Custom/Vector")]
public class VectorNode : BaseNode
{
	[Output(name = "Out")]
	public Vector4				output;
	
	[Input(name = "In"), SerializeField]
	public Vector4				input;

	public override string		name => "Vector";

	protected override void Process()
	{
		output = input;
	}
}
