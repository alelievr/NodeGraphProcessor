using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Conditional/Switch")]
public class SwitchNode : BaseNode
{
	[Input(name = "In")]
    public float                input;

	[Output(name = "Out")]
	public float				output;

	public override string		name => "Switch";

	protected override void Process()
	{
	    output = input * 42;
	}
}
