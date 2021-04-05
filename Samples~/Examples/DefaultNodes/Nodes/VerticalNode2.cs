using UnityEngine;
using GraphProcessor;

[System.Serializable, NodeMenuItem("Custom/Vertical 2")]
public class VerticalNode2 : BaseNode
{
	[Input, Vertical]
    public float                input;

	[Input]
    public float                input2;

	[Output, Vertical]
	public float				output;

	[Output]
	public float				output2;

	public override string		name => "Vertical 2";

	protected override void Process()
	{
	    output = input * 42;
	}
}
