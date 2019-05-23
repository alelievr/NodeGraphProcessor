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
	ParameterNode	parameterNode;

	public override void Enable()
	{
		parameterNode = nodeTarget as ParameterNode;

        parameterNode.onParameterChanged += UpdateView;
        UpdateView();
	}

    void UpdateView()
    {
        title = parameterNode.parameter?.name;
    }
}
