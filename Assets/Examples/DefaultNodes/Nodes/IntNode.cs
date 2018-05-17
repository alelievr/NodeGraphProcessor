using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;

[System.Serializable, NodeMenuItem("Primitives/Int")]
public class IntNode : BaseNode
{
	[Output]
	public int		output;

	public override string name { get { return  "IntNode"; } }
}