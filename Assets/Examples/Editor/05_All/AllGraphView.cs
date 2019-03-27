using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using GraphProcessor;
using System;

public class AllGraphView : BaseGraphView
{
    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
	{
		evt.menu.AppendSeparator();

		foreach (var nodeMenuItem in NodeProvider.GetNodeMenuEntries())
		{
			var mousePos = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
			Vector2 nodePosition = mousePos;
			evt.menu.AppendAction("Create/" + nodeMenuItem.Key,
				(e) => CreateNodeOfType(nodeMenuItem.Value, nodePosition),
				DropdownMenuAction.AlwaysEnabled
			);
		}

		base.BuildContextualMenu(evt);
	}

	void CreateNodeOfType(Type type, Vector2 position)
	{
		RegisterCompleteObjectUndo("Added " + type + " node");
		AddNode(BaseNode.CreateFromType(type, position));
	}
}