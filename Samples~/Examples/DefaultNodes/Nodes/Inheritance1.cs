using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Custom/Inheritance1")]
public class Inheritance1 :InheritanceBase 
{
	[Input(name = "In 1")]
    public float                input1;

	[Output(name = "Out 1")]
	public float				output1;

	public float				field1;

	public override string		name => "Inheritance1";

	protected override void Process()
	{
	    output = input * 42;
	}
}
