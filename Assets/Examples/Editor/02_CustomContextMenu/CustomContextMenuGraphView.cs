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
			evt.menu.AppendAction("Create/" + nodeMenuItem.Key, (e) => CreateNodeOfType(nodeMenuItem.Value), ContextualMenu.MenuAction.AlwaysEnabled);
		
		base.BuildContextualMenu(evt);
	}

	void CreateNodeOfType(Type type)
	{
		var node = Activator.CreateInstance(type);

		AddNode(node as BaseNode);
	}
}