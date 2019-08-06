using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using System.Linq;

[NodeCustomEditor(typeof(ParameterNode))]
public class ParameterNodeView : BaseNodeView
{
    ParameterNode parameterNode;

    public override void Enable()
    {
        parameterNode = nodeTarget as ParameterNode;

        EnumField accessorSelector = new EnumField(parameterNode.accessor);
        accessorSelector.SetValueWithoutNotify(parameterNode.accessor);
        accessorSelector.RegisterValueChangedCallback(evt =>
        {
            parameterNode.accessor = (ParameterAccessor)evt.newValue;
            controlsContainer.MarkDirtyRepaint();
            ForceUpdatePorts();
        });
        controlsContainer.Add(accessorSelector);

        parameterNode.onParameterChanged += UpdateView;
        UpdateView();
    }

    void UpdateView()
    {
        title = parameterNode.parameter?.name;
    }
}
