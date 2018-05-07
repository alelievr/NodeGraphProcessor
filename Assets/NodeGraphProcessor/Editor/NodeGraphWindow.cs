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

namespace GraphProcessor
{
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

            var connectorListener = new EdgeConnectorListener(graphProcessorView);
	
			for (int i = 0; i < 10; i++)
			{
				var node = new BasicNodeView();

				var slot = new ColorRGBAMaterialSlot();
	
				// node.Add(new PortView(Orientation.Horizontal, Direction.Input, typeof(int)));
				node.Add(ShaderPort.Create(slot, connectorListener));

				node.SetPosition(new Rect(0, 0, i * 100, 0));
	
				node.Initialize();
	
				graphProcessorView.AddElement(node);
			}
	
			graphProcessorView.SetupZoom(0.05f, ContentZoomer.DefaultMaxScale);
	
			graphProcessorView.StretchToParentSize();
			
			root.Add(graphProcessorView);
		}
	}
}