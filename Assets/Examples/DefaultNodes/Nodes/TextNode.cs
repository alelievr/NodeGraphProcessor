using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Primitives/Text")]
public class TextNode : BaseNode
{
	[Output(name = "Label"), SerializeField]
	public string				output;

	public override string		name => "Text";
}
