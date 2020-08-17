using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using NodeGraphProcessor.Examples;

[NodeMenuItem("Print")]
public class PrintNode : BaseNode
{
	[Input]
	public object	obj;

	public override string name => "Print";
}

[NodeMenuItem("Conditional/Print")]
public class ConditionalPrintNode : LinearConditionalNode
{
	[Input]
	public object	obj;

	public override string name => "Print";
}
