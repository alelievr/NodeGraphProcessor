using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using GraphProcessor;

[NodeCustomEditor(typeof(CircleRadians))]
public class CircleRadiansView : BaseNodeView
{
	CircleRadians	node;
	VisualElement	listContainer;

	public override void Enable()
	{
		node = nodeTarget as CircleRadians;

		listContainer = new VisualElement();
        // Create your fields using node's variables and add them to the controlsContainer

		controlsContainer.Add(listContainer);

		// TODO: find a way to get PortView from here
		UpdateOutputRadians(GetPortFromFieldName("outputRadians").connectionCount);
	}

	void UpdateOutputRadians(int count)
	{
		node.outputRadians = new List<float>();

		listContainer.Clear();

		for (int i = 0; i < count; i++)
		{
			float r = (Mathf.PI * 2 / count) * i;
			node.outputRadians.Add(r);
			listContainer.Add(new Label(r.ToString("F3")));
		}
	}

	public override void OnPortConnected(PortView port)
	{
		// There is only one port on this node so it can only be the output
		UpdateOutputRadians(port.connectionCount);
	}

	public override void OnPortDisconnected(PortView port)
	{
		UpdateOutputRadians(port.connectionCount);
	}
}