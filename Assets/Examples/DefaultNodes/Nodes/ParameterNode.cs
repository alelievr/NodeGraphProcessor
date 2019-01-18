using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Custom/Parameter")]
public class ParameterNode : BaseNode
{
	[Output(name = "Out")]
	public object				output;

	public string				propertyName;

	public override string		name => "Parameter";

	protected override void Enable()
	{
		UpdateOutput();
	}

	void UpdateOutput()
	{
		var param = graph.exposedParameters.FirstOrDefault(e => e.name == propertyName);

		if (param == null)
			Debug.Log("Exposed property \"" + propertyName + "\" not found !");
		else
			output = param.value;
	}
}
