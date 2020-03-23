using UnityEngine;
using UnityEngine.UIElements;
using GraphProcessor;
using UnityEditor;
using System.Linq;
using System;
using UnityEditor.Experimental.GraphView;

[NodeCustomEditor(typeof(RelayNode))]
public class RelayNodeView : BaseNodeView
{
	RelayNode	relay => nodeTarget as RelayNode;
	VisualElement input => this.Q("input");
	VisualElement output => this.Q("output");

	public override void Enable()
	{
		// Remove useless elements
		this.Q("title").RemoveFromHierarchy();
		this.Q("divider").RemoveFromHierarchy();

		onPortConnected += (e) => UpdatePortTypes(e);
		onPortDisconnected += (e) => UpdatePortTypes(e, typeof(object));

		owner.graph.onGraphChanges += OnGraphChanges;

		ForceUpdatePorts();
	}

	public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
	{
		evt.menu.AppendAction("Unpack Output", ToggleUnpackOutput, UnpackOutputStatus);
		base.BuildContextualMenu(evt);
	}

	void ToggleUnpackOutput(DropdownMenuAction action)
	{
		relay.unpackOutput = !relay.unpackOutput;

		ForceUpdatePorts();
		UpdateSize();
		MarkDirtyRepaint();
	}

	void OnGraphChanges(GraphChanges changes)
	{
		schedule.Execute(() => {
			ForceUpdatePorts();
			UpdateSize();
			MarkDirtyRepaint();
		}).ExecuteLater(1);
	}

	DropdownMenuAction.Status UnpackOutputStatus(DropdownMenuAction action)
	{
		if (relay.GetNonRelayEdges().Count == 0)
			return DropdownMenuAction.Status.Disabled;

		if (relay.unpackOutput)
			return DropdownMenuAction.Status.Checked;
		else
			return DropdownMenuAction.Status.Normal;
	}

	public override void SetPosition(Rect newPos)
	{
		base.SetPosition(new Rect(newPos.position, new Vector2(200, 200)));
		UpdateSize();
	}

	void UpdatePortTypes(PortView portView, Type forceType = null)
	{
		// TODO: remove me
		schedule.Execute(() => {
			ForceUpdatePorts();
		}).ExecuteLater(1);
	}

	void UpdateSize()
	{
		int inputEdgeCount = relay.GetNonRelayEdges().Count;

		if (relay.unpackOutput)
		{
			style.height = Mathf.Max(30, 24 * inputEdgeCount + 5);
			style.width = -1;
			input.style.height = -1;
			if (output != null)
				output.style.height = -1;
			RemoveFromClassList("hideLabels");
		}
		else
		{
			style.height = 20;
			style.width = 50;
			input.style.height = 16;
			if (output != null)
				output.style.height = 16;
			AddToClassList("hideLabels");
		}
	}
}