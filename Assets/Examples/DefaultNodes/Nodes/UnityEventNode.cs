using UnityEngine;
using GraphProcessor;
using UnityEngine.Events;

[System.Serializable, NodeMenuItem("Custom/Unity Event Node")]
public class UnityEventNode : BaseNode
{
	[Input(name = "In")]
    public float                input;

	[Output(name = "Out")]
	public float				output;

	public UnityEvent			evt;

	public override string		name => "Unity Event Node";

	protected override void Process()
	{
	    output = input * 42;
	}
}
