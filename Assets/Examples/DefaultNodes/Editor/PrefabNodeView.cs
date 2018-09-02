using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using GraphProcessor;

[NodeCustomEditor(typeof(PrefabNode))]
public class PrefabNodeView : BaseNodeView
{
	public override void Enable()
	{
		var prefabNode = nodeTarget as PrefabNode;

        var objField = new ObjectField
        {
            value = prefabNode.output
        };

		controlsContainer.Add(objField);
	}
}