using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using GraphProcessor;
using System;

public class CustomContextMenuGraphView : BaseGraphView
{
	public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
	{
		evt.menu.AppendSeparator();
		
		foreach (var nodeMenuItem in NodeProvider.GetNodeMenuEntries())
		{
			Vector2 nodePosition = evt.mousePosition - (Vector2)viewTransform.position;
			evt.menu.AppendAction("Create/" + nodeMenuItem.Key,
				(e) => CreateNodeOfType(nodeMenuItem.Value, nodePosition),
				ContextualMenu.MenuAction.AlwaysEnabled
			);
		}
		
		base.BuildContextualMenu(evt);
	}

	void CreateNodeOfType(Type type, Vector2 position)
	{
		var node = Activator.CreateInstance(type) as BaseNode;

		node.position = new Rect(position, new Vector2(100, 100));

		AddNode(node as BaseNode);
	}
}