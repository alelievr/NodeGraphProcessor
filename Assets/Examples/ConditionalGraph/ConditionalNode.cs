using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable]
/// <summary>
/// This is the base class for every node that is executed by the conditional processor, it takes an executed bool as input to 
/// </summary>
public abstract class ConditionalNode : BaseNode, IConditionalNode
{
	// These booleans will controls wether or not the execution of the folowing nodes will be done or discarded.
	[Input(name = "Executed", allowMultiple = true)]
    public ConditionalLink	executed;

	public abstract IEnumerable< ConditionalNode >	GetExecutedNodes();
}

[System.Serializable]
/// <summary>
/// This class represent a simple node which takes one event in parameter and pass it to the next node
/// </summary>
public abstract class LinearConditionalNode : ConditionalNode, IConditionalNode
{
	[Output(name = "Executes")]
	public ConditionalLink	executes;

	public override IEnumerable< ConditionalNode >	GetExecutedNodes()
	{
		// Return all the nodes connected to the executes port
		return outputPorts.FirstOrDefault(n => n.fieldName == nameof(executes))
			.GetEdges().Select(e => e.inputNode as ConditionalNode);
	}
}
