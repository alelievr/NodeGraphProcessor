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

	public override string		name => "Parameter";

	[SerializeField]
	ExposedParameter			_parameter;

	public ExposedParameter			parameter
	{
		get => _parameter;
		set => UpdateParameter(value);
	}

	void UpdateParameter(ExposedParameter newValue)
	{
		_parameter = newValue;

		if (parameter == null)
			Debug.Log("Null Exposed property assigned to property node not found !");
		else
			output = parameter.value;
	}

	protected override void Process()
	{
		UpdateParameter(_parameter);
	}
}
