using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;

[System.Serializable, NodeMenuItem("Primitives/Add")]
public class AddNode : BaseNode
{
	[Input]
	public PortArray< float >	input;

	[Output]
	public float				output;

	public override string name { get { return "Add"; } }
}
