using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using System.Linq;
using System;

namespace GraphProcessor
{
	public class BaseGraphView : GraphView
	{
		public BaseGraph				graph;
		
		public EdgeConnectorListener	connectorListener;

		public BaseGraphView()
		{
			serializeGraphElements = SerializeGraphElementsImplementation;
			canPasteSerializedData = CanPasteSerializedDataImplementation;
			unserializeAndPaste = UnserializeAndPasteImplementation;
			
			InitializeManipulators();
			
			SetupZoom(0.05f, ContentZoomer.DefaultMaxScale);
	
			this.StretchToParentSize();
		}
	
		protected override bool canCopySelection
		{
			get { return selection.OfType<Node>().Any(); }
		}

		string SerializeGraphElementsImplementation(IEnumerable<GraphElement> elements)
		{
			return JsonUtility.ToJson("", true);
		}

		bool CanPasteSerializedDataImplementation(string serializedData)
		{
			return true;
		}

		void UnserializeAndPasteImplementation(string operationName, string serializedData)
		{
			Debug.Log("TODO !");
		}

		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			var compatiblePorts = new List<Port>();

			compatiblePorts.AddRange(ports.ToList().Where(p => p.portType.IsAssignableFrom(startPort.portType)));

			return compatiblePorts;
		}

		public void Initialize(BaseGraph graph)
		{
			this.graph = graph;
			
            connectorListener = new EdgeConnectorListener(this);

			nodeCreationRequest = CreateNode;

			InitializeNodeViews();
		}

		void CreateNode(NodeCreationContext context)
		{
			Debug.Log("TODO !");
		}

		void InitializeNodeViews()
		{
			graph.nodes.RemoveAll(n => n == null);
			
			foreach (var node in graph.nodes)
				AddNodeView(node);
		}

		protected bool AddNode(BaseNode node)
		{
			AddNodeView(node);

			graph.AddNode(node);

			return true;
		}

		protected bool AddNodeView(BaseNode node)
		{
			var viewType = NodeProvider.GetNodeViewTypeFromType(node.GetType());

			if (viewType == null)
				return false;

			var baseNodeView = Activator.CreateInstance(viewType) as BaseNodeView;

			baseNodeView.Initialize(this, node);

			AddElement(baseNodeView);

			return true;
		}
	
		protected virtual void InitializeManipulators()
		{
			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());
			this.AddManipulator(new ClickSelector());
		}

	}
}