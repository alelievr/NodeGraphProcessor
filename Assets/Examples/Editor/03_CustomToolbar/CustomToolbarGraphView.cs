using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using GraphProcessor;
using System;

public class CustomToolbarGraphView : BaseGraphView
{
	public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
	{
		// foreach (var nodeMenuItem in NodeProvider.GetNodeMenuEntries())
		// {
		// 	Vector2 nodePosition = evt.mousePosition - (Vector2)viewTransform.position;
		// 	evt.menu.AppendAction("Create/" + nodeMenuItem.Key,
		// 		(e) => CreateNodeOfType(nodeMenuItem.Value, nodePosition),
		// 		DropdownMenu.MenuAction.AlwaysEnabled
		// 	);
		// }
	}
}