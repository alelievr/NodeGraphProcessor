using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Custom/MessageNode")]
public class MessageNode : BaseNode
{
	const string k_InputIsNot42Error = "Input is not 42 !";

	[Input(name = "In")]
    public float                input;

	public override string		name => "MessageNode";

	[Setting("Message Type")]
	public NodeMessageType messageType = NodeMessageType.Error;

	protected override void Process()
	{
		if (input != 42)
			AddMessage(k_InputIsNot42Error, messageType);
		else
			RemoveMessage(k_InputIsNot42Error);
	}
}
