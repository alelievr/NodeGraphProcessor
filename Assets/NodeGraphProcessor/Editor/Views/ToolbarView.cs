using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Experimental.UIElements;
using UnityEditor;

namespace GraphProcessor
{
	public class ToolbarView : VisualElement
	{
		public ToolbarView()
		{
            AddStyleSheetPath("GraphProcessorStyles/ToolbarView");

			name = "ToolbarView";

			// TODO: draw ImGUI toolbar
            Add(new IMGUIContainer(DrawToolbarImGUI)
			{
                clippingOptions = VisualElement.ClippingOptions.ClipAndCacheContents
            });
		}

		private bool collapse = false;
		private bool clearOnPlay = false;
		private bool errorPause = false;
		private bool showLog = false;
		private bool showWarnings = false;
		private bool showErrors = false;

		void DrawToolbarImGUI()
		{
			EditorGUILayout.BeginHorizontal();

			collapse = EditorGUILayout.Toggle(new GUIContent("Collapse"), collapse, EditorStyles.toolbarButton);

			EditorGUILayout.EndHorizontal();
		}
	}
}
