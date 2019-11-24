using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;
using Status = UnityEngine.UIElements.DropdownMenuAction.Status;

public class CustomToolbarView : ToolbarView
{
	public CustomToolbarView(BaseGraphView graphView) : base(graphView) {}

	protected override void AddButtons()
	{
		// Add the hello world button on the left of the toolbar
		AddButton("Hello !", () => Debug.Log("Hello World"), left: false);

		// add the default buttons (center, show processor and show in project)
		base.AddButtons();

		bool conditionalProcessorVisible = graphView.GetPinnedElementStatus< ConditionalProcessorView >() != Status.Hidden;
		AddToggle("Show Conditional Processor", conditionalProcessorVisible, (v) => graphView.ToggleView< ConditionalProcessorView>());
	}
}