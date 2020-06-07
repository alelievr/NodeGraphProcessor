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
		onPortConnected += (p) => UpdatePrintLabel();
		onPortDisconnected += (p) => UpdatePrintLabel();

		UpdatePrintLabel();
	}

	void UpdatePrintLabel()
	{
		if (printNode.obj != null)
			printLabel.text = printNode.obj.ToString();
		else
			printLabel.text = "null";
	}
}

[NodeCustomEditor(typeof(ConditionalPrintNode))]
public class ConditionalPrintNodeView : BaseNodeView
{
	Label		printLabel;
	ConditionalPrintNode	printNode;

	public override void Enable()
	{
		printNode = nodeTarget as ConditionalPrintNode;

		printLabel = new Label();
		controlsContainer.Add(printLabel);

		nodeTarget.onProcessed += UpdatePrintLabel;
		onPortConnected += (p) => UpdatePrintLabel();
		onPortDisconnected += (p) => UpdatePrintLabel();

		UpdatePrintLabel();
	}

	void UpdatePrintLabel()
	{
		if (printNode.obj != null)
			printLabel.text = printNode.obj.ToString();
		else
			printLabel.text = "null";
	}
}
