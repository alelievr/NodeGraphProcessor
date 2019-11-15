using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Conditional/ForLoop")]
public class ForLoopNode : ConditionalNode
{
	[Output(name = "Loop Body")]
	public ConditionalLink		loopBody;
	
	[Output(name = "Loop Completed")]
	public ConditionalLink		loopCompleted;

	public int					start = 0;
	public int					end = 10;

	[Output]
	public int					index;

	public override string		name => "ForLoop";

	protected override void Process()
	{
		// TODO
	}

	public override IEnumerable< ConditionalNode >	GetExecutedNodes()
	{
		yield break;
	}
}
