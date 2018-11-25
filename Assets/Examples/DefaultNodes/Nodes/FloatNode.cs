using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;

[System.Serializable, NodeMenuItem("Primitives/Float")]
public class FloatNode : BaseNode
{
    [Output]
	public float		output;

	public override string name => "Float";
}