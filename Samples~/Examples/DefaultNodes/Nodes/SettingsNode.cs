using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

public enum Setting
{
	S1,
	S2,
	S3,
}

[System.Serializable, NodeMenuItem("Custom/SettingsNode")]
public class SettingsNode : BaseNode
{
	public Setting				setting;
	public override string		name => "SettingsNode";

	[Input]
	public float			input;
	
	[Output]
	public float			output;

	protected override void Process() {}
}
