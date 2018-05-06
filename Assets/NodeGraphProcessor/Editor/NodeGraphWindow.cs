using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Properties;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;

public class NodeGraphWindow : EditorWindow
{
	VisualElement		root;

	BasicNode			node;

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
			node = new BasicNode();
			node.SetSize(new Vector2(200, 100));
			node.AddManipulator(new Dragger());
			node.title = "TEST !";

			graphProcessorView.AddElement(node);
		}

		graphProcessorView.SetupZoom(0.1f, ContentZoomer.DefaultMaxScale);

		graphProcessorView.StretchToParentSize();

		node.SavePersistentData();
		
		root.Add(graphProcessorView);
	}
}
