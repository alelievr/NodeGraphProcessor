using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System;

[System.Serializable]
public class ParameterNode : BaseNode
{
	[Output(name = "Value")]
	public object				output;

	public override string		name => "Parameter";

	// We serialize the name of the exposed parameter in the graph so we can retrieve the true ExposedParameter from the graph
	[SerializeField, HideInInspector]
	public string				parameterName;

	public ExposedParameter		parameter { get; private set; }

	public event Action			onParameterChanged;

	protected override void Enable()
	{
		// load the parameter
		LoadExposedParameter();

		graph.onExposedParameterModified += OnParamChanged;
		if (onParameterChanged != null)
			onParameterChanged?.Invoke();
	}

	void LoadExposedParameter()
	{
		parameter = graph.GetExposedParameter(parameterName);

		if (parameter == null)
		{
			Debug.Log("Property \"" + parameterName + "\" Can't be found !");
			return ;
		}

		output = parameter.serializedValue.value;
	}

	void OnParamChanged(string modifiedParameterName)
	{
		if (parameterName == modifiedParameterName)
		{
			onParameterChanged?.Invoke();
		}
	}

	protected override void Process()
	{
		output = parameter?.serializedValue.value;
	}
}
