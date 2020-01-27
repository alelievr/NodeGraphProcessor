using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System.Reflection;

[System.Serializable, NodeMenuItem("Conditional/Start")]
public class StartNode : BaseNode, IConditionalNode
{
	[Output(name = "Executes")]
	public ConditionalLink		executes;

	public override string		name => "Start";

	public IEnumerable< ConditionalNode >	GetExecutedNodes()
	{
		// Return all the nodes connected to the executes port
		return GetOutputNodes().Where(n => n is ConditionalNode).Select(n => n as ConditionalNode);
	}

	public override FieldInfo[] GetNodeFields() => base.GetNodeFields();
}
