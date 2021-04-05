using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
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
		onPortConnected += OnPortUpdate;
		onPortDisconnected += OnPortUpdate;

		UpdateOutputRadians(GetFirstPortViewFromFieldName("outputRadians").connectionCount);
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

	public void OnPortUpdate(PortView port)
	{
		// There is only one port on this node so it can only be the output
		UpdateOutputRadians(port.connectionCount);
	}
}