using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using GraphProcessor;
using System;
using UnityEditor;

public class AllGraphView : BaseGraphView
{
	// Nothing special to add for now
	public AllGraphView(EditorWindow window) : base(window) {}

	public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
	{
		BuildStackNodeContextualMenu(evt);
		base.BuildContextualMenu(evt);
	}

	/// <summary>
	/// Add the New Stack entry to the context menu
	/// </summary>
	/// <param name="evt"></param>
	protected void BuildStackNodeContextualMenu(ContextualMenuPopulateEvent evt)
	{
		Vector2 position = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
		evt.menu.AppendAction("New Stack", (e) => AddStackNode(new BaseStackNode(position)), DropdownMenuAction.AlwaysEnabled);
	}
}