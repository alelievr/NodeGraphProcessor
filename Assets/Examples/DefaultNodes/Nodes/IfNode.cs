using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

[System.Serializable, NodeMenuItem("Conditional/If")]
public class IfNode : BaseNode
{
	[Input(name = "Condition")]
    public float    c;

	[Output(name = "True")]
	public float	@true;
	[Output(name = "False")]
	public float	@false;

	public CompareFunction		compareOperator;

	public override string		name => "If";

	protected override void Process()
	{
		switch (compareOperator)
		{
			case CompareFunction.Disabled:
			case CompareFunction.Always:
				@true = c; break;
			case CompareFunction.Never:
				@false = c; break;
		}
	}
}
