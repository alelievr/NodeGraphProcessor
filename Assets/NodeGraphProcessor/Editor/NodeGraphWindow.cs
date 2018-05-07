using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Properties;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;

using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Drawing;

public class NodeGraphWindow : EditorWindow
{
	VisualElement		root;

	[MenuItem("Window/NodeGraphProcessor")]
	public static void Open()
	{
		GetWindow< NodeGraphWindow >().Show();
	}

	private void OnEnable()
	{
		root = this.GetRootVisualContainer();

		root.AddStyleSheetPath("Styles/Test");

		var graphProcessorView = new GraphProcessorView();

		graphProcessorView.AddManipulator(new ContentDragger());
		graphProcessorView.AddManipulator(new SelectionDragger());
		graphProcessorView.AddManipulator(new RectangleSelector());
		graphProcessorView.AddManipulator(new ClickSelector());

		for (int i = 0; i < 100; i++)
		{
			var node = new BasicNodeView();

			node.Initialize();

			graphProcessorView.AddElement(node);

			graphProcessorView.zoomerMaxElementCountWithPixelCacheRegen = 10;
		}

		graphProcessorView.SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

		graphProcessorView.StretchToParentSize();
		
		root.Add(graphProcessorView);
	}
}
