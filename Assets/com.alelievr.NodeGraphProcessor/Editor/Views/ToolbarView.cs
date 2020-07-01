using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor;
using System.Linq;
using System;

using Status = UnityEngine.UIElements.DropdownMenuAction.Status;

namespace GraphProcessor
{
	public class ToolbarView : VisualElement
	{
		protected class ToolbarButtonData
		{
			public GUIContent		content;
			public bool				toggle;
			public bool				value;
			public bool				visible = true;
			public Action			buttonCallback;
			public Action< bool >	toggleCallback;
		}

		List< ToolbarButtonData >	leftButtonDatas = new List< ToolbarButtonData >();
		List< ToolbarButtonData >	rightButtonDatas = new List< ToolbarButtonData >();
		protected BaseGraphView		graphView;
		
		ToolbarButtonData showProcessor;
		ToolbarButtonData showParameters;

		public ToolbarView(BaseGraphView graphView)
		{
			name = "ToolbarView";
			this.graphView = graphView;

			graphView.initialized += AddButtons;

			Add(new IMGUIContainer(DrawImGUIToolbar));
		}

		protected ToolbarButtonData AddButton(string name, Action callback, bool left = true)
			=> AddButton(new GUIContent(name), callback, left);

		protected ToolbarButtonData AddButton(GUIContent content, Action callback, bool left = true)
		{
			var data = new ToolbarButtonData{
				content = content,
				toggle = false,
				buttonCallback = callback
			};
			((left) ? leftButtonDatas : rightButtonDatas).Add(data);
			return data;
		}

		protected ToolbarButtonData AddToggle(string name, bool defaultValue, Action< bool > callback, bool left = true)
			=> AddToggle(new GUIContent(name), defaultValue, callback, left);

		protected ToolbarButtonData AddToggle(GUIContent content, bool defaultValue, Action< bool > callback, bool left = true)
		{
			var data = new ToolbarButtonData{
				content = content,
				toggle = true,
				value = defaultValue,
				toggleCallback = callback
			};
			((left) ? leftButtonDatas : rightButtonDatas).Add(data);
			return data;
		}

		/// <summary>
		/// Also works for toggles
		/// </summary>
		/// <param name="name"></param>
		/// <param name="left"></param>
		protected void RemoveButton(string name, bool left)
		{
			((left) ? leftButtonDatas : rightButtonDatas).RemoveAll(b => b.content.text == name);
		}
		
		/// <summary>
		/// Hide the button
		/// </summary>
		/// <param name="name">Display name of the button</param>
		protected void HideButton(string name)
		{
			leftButtonDatas.Concat(rightButtonDatas).All(b => {
				if (b.content.text == name)
					b.visible = false;
				return true;
			});
		}

		/// <summary>
		/// Show the button
		/// </summary>
		/// <param name="name">Display name of the button</param>
		protected void ShowButton(string name)
		{
			leftButtonDatas.Concat(rightButtonDatas).All(b => {
				if (b.content.text == name)
					b.visible = true;
				return true;
			});
		}

		protected virtual void AddButtons()
		{
			leftButtonDatas.Clear();
			rightButtonDatas.Clear();

			AddButton("Center", graphView.ResetPositionAndZoom);

			bool processorVisible = graphView.GetPinnedElementStatus< ProcessorView >() != Status.Hidden;
			showProcessor = AddToggle("Show Processor", processorVisible, (v) => graphView.ToggleView< ProcessorView>());
			bool exposedParamsVisible = graphView.GetPinnedElementStatus< ExposedParameterView >() != Status.Hidden;
			showParameters = AddToggle("Show Parameters", exposedParamsVisible, (v) => graphView.ToggleView< ExposedParameterView>());

			AddButton("Show In Project", () => EditorGUIUtility.PingObject(graphView.graph), false);
		}

		public virtual void UpdateButtonStatus()
		{
			if (showProcessor != null)
				showProcessor.value = graphView.GetPinnedElementStatus< ProcessorView >() != Status.Hidden;
			if (showParameters != null)
				showParameters.value = graphView.GetPinnedElementStatus< ExposedParameterView >() != Status.Hidden;
		}

		void DrawImGUIButtonList(List< ToolbarButtonData > buttons)
		{
			foreach (var button in buttons.ToList())
			{
				if (!button.visible)
					continue;

				if (button.toggle)
				{
					EditorGUI.BeginChangeCheck();
					button.value = GUILayout.Toggle(button.value, button.content, EditorStyles.toolbarButton);
					if (EditorGUI.EndChangeCheck() && button.toggleCallback != null)
						button.toggleCallback(button.value);
				}
				else
				{
					if (GUILayout.Button(button.content, EditorStyles.toolbarButton) && button.buttonCallback != null)
						button.buttonCallback();
				}
			}
		}

		void DrawImGUIToolbar()
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);

			DrawImGUIButtonList(leftButtonDatas);

			GUILayout.FlexibleSpace();

			DrawImGUIButtonList(rightButtonDatas);

			GUILayout.EndHorizontal();
		}
	}
}
