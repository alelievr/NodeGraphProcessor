using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Custom/VisibleIfAttributeNode")]
public class VisibleIfAttributeNode : BaseNode
{
	public enum Test1
	{
		A,
		B,
		C,
		D
	}

	public enum Test2
	{
		T1,
		T2,
		T3,
	}

	public Test1 t1;

	[VisibleIf(nameof(t1), Test1.A)]
	public float f1;
	[VisibleIf(nameof(t1), Test1.B)]
	public int f2;

	[VisibleIf(nameof(t1), Test1.C)]
	public string s1;
	[VisibleIf(nameof(t1), Test1.C)]
	public Test2 t2;

	[Input(name = "In")]
    public float                input;

	[Output(name = "Out")]
	public float				output;

	public override string		name => "VisibleIfAttributeNode";

	protected override void Process()
	{
	    output = input * 42;
	}
}
