using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System;

[System.Serializable]
public class ParameterNode : BaseNode
{
    [Input]
    public object input;

    [Output]
    public object output;

    public override string name => "huifehfgw";

    // We serialize the GUID of the exposed parameter in the graph so we can retrieve the true ExposedParameter from the graph
    [SerializeField, HideInInspector]
    public string parameterGUID;

    public ExposedParameter parameter { get; private set; }

    public event Action onParameterChanged;

    public ParameterAccessor accessor;

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
        parameter = graph.GetExposedParameterFromGUID(parameterGUID);

        if (parameter == null)
        {
            Debug.Log("Property \"" + parameterGUID + "\" Can't be found !");

            // Delete this node as the property can't be found
            graph.RemoveNode(this);
            return;
        }

        output = parameter.serializedValue.value;
    }

    void OnParamChanged(string modifiedParameterName)
    {
        if (parameter?.name == modifiedParameterName)
        {
            onParameterChanged?.Invoke();
        }
    }

    [CustomPortBehavior(nameof(output))]
    IEnumerable<PortData> GetOutputPort(List<SerializableEdge> edges)
    {
        if (accessor == ParameterAccessor.GET)
        {
            yield return new PortData
            {
                identifier = "output",
                displayName = "Value",
                displayType = (parameter == null) ? typeof(object) : Type.GetType(parameter.type),
            };
        }
    }

    [CustomPortBehavior(nameof(input))]
    IEnumerable<PortData> GetInputPort(List<SerializableEdge> edges)
    {
        if (accessor == ParameterAccessor.SET)
        {
            yield return new PortData
            {
                identifier = "input",
                displayName = "Value",
                displayType = (parameter == null) ? typeof(object) : Type.GetType(parameter.type),
            };
        }
    }


    protected override void Process()
    {
        if (accessor == ParameterAccessor.GET)
            output = parameter?.serializedValue.value;
        else
            parameter.serializedValue.value = input;
    }
}

public enum ParameterAccessor
{
    GET,
    SET
}