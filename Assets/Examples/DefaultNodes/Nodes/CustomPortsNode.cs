using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Primitives/CustomPorts")]
public class CustomPortsNode : BaseNode
{
    [Input]
	public MultiPorts       	inputs = null;

	[Output]
	public MultiPorts			output;

	public override string		name => "CustomPorts";
}
