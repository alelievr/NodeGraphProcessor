using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;

[NodeCustomEditor(typeof(SettingsNode))]
public class SettingsNodeView : BaseNodeView
{
	protected override bool hasSettings => true;

	SettingsNode	settingsNode;

	public override void Enable()
	{
		settingsNode = nodeTarget as SettingsNode;

		controlsContainer.Add(new Label("Hello World !"));
	}

	protected override VisualElement CreateSettingsView()
	{
		var settings = new VisualElement();

		settings.Add(new EnumField("S", settingsNode.setting));

		return settings;
	}
}