using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System;

[System.Serializable]
public class ParameterNode : BaseNode
{
	[Output(name = "Out")]
	public object				output;

	public string				propertyName;

	public override string		name => "Parameter";

	[SerializeField]
	ExposedParameter			parameter;

	protected override void Enable()
	{
		UpdateOutput();
	}

	void UpdateOutput()
	{
		parameter = graph.exposedParameters.FirstOrDefault(e => e.name == propertyName);

		if (parameter == null)
			Debug.Log("Exposed property \"" + propertyName + "\" not found !");
		else
			output = parameter.value;
	}

	protected override void Process()
	{
		UpdateOutput();
	}
}
