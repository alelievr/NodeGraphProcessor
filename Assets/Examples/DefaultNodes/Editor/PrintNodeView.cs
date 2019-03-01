using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using Unity.Jobs;

[NodeCustomEditor(typeof(PrintNode))]
public class PrintNodeView : BaseNodeView
{
	Label		printLabel;
	PrintNode	printNode;

	public override void Enable()
	{
		printNode = nodeTarget as PrintNode;

		printLabel = new Label();
		controlsContainer.Add(printLabel);

		nodeTarget.onProcessed += UpdatePrintLabel;

		UpdatePrintLabel();
	}

	void UpdatePrintLabel()
	{
		if (printNode.obj != null)
			printLabel.text = printNode.obj.ToString();
		else
			printLabel.text = "null";
	}

	public override void OnPortConnected(PortView port)
	{
		UpdatePrintLabel();
	}

	public override void OnPortDisconnected(PortView port)
	{
		UpdatePrintLabel();
	}
}
