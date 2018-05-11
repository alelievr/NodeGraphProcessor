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
		public BaseGraph						graph;
		
		public EdgeConnectorListener			connectorListener;

		List< BaseNodeView >					nodeViews = new List< BaseNodeView >();
		Dictionary< BaseNode, BaseNodeView >	nodeViewsPerNode = new Dictionary< BaseNode, BaseNodeView >();

		public BaseGraphView()
		{
			serializeGraphElements = SerializeGraphElementsImplementation;
			canPasteSerializedData = CanPasteSerializedDataImplementation;
			unserializeAndPaste = UnserializeAndPasteImplementation;
			deleteSelection = DeleteSelection;
			
			InitializeManipulators();
			
			SetupZoom(0.1f, ContentZoomer.DefaultMaxScale);
	
			this.StretchToParentSize();
		}
	
		protected override bool canCopySelection
		{
			get { return selection.OfType< BaseNodeView >().Any(); }
		}

		string SerializeGraphElementsImplementation(IEnumerable<GraphElement> elements)
		{
			Debug.Log("TODO !");
			return JsonUtility.ToJson("", true);
		}

		bool CanPasteSerializedDataImplementation(string serializedData)
		{
			Debug.Log("TODO !");
			return true;
		}

		void UnserializeAndPasteImplementation(string operationName, string serializedData)
		{
			Debug.Log("TODO !");
		}

		void DeleteSelection(string operationName, AskUser askUser)
		{
			Debug.Log("operationName: " + operationName);

			nodeViews.RemoveAll(n => {
				if (n.selected)
				{
					graph.RemoveNode(n.nodeTarget);
					return true;
				}
				return false;
			});

			//TODO: delete edges
		}

		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			var compatiblePorts = new List<Port>();

			compatiblePorts.AddRange(ports.ToList().Where(p => {
				if (p.direction == startPort.direction)
					return false;

				if (!p.portType.IsAssignableFrom(startPort.portType))
					return false;
					
				return true;
			}));

			return compatiblePorts;
		}

		public void Initialize(BaseGraph graph)
		{
			this.graph = graph;
			
            connectorListener = new EdgeConnectorListener(this);

			InitializeGraphView();
			InitializeNodeViews();
			InitializeEdgeViews();
		}

		void InitializeNodeViews()
		{
			graph.nodes.RemoveAll(n => n == null);
			
			foreach (var node in graph.nodes)
				AddNodeView(node);
		}

		void InitializeGraphView()
		{
			Debug.Log("Graph position: " + graph.position);
			// viewTransform.position = graph.position;
			contentViewContainer.transform.position = graph.position;
			UpdateViewTransform(graph.position, graph.scale);

			Debug.Log("graph pos: " + viewTransform.position);
			Debug.Log("graph scale: " + viewTransform.scale);
		}

		void InitializeEdgeViews()
		{
			foreach (var serializedEdge in graph.edges)
			{
				var edge = new Edge();

				var inputNodeView = nodeViewsPerNode[serializedEdge.inputNode];
				var outputNodeView = nodeViewsPerNode[serializedEdge.outputNode];

				edge.input = inputNodeView.GetPortFromFieldName(serializedEdge.inputFieldName);
			}
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

			nodeViewsPerNode[node] = baseNodeView;

			return true;
		}

		public void Connect(Edge e)
		{
			if (e.input == null || e.output == null)
				return ;

			AddElement(e);

			var inputNodeView = e.input.node as BaseNodeView;
			var outputNodeView = e.output.node as BaseNodeView;

			graph.Connect(inputNodeView.nodeTarget, e.input.name, outputNodeView.nodeTarget, e.output.name);
		}
	
		protected virtual void InitializeManipulators()
		{
			this.AddManipulator(new BaseGraphContentDragger());
			this.AddManipulator(new BaseGraphZoomer());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());
			this.AddManipulator(new ClickSelector());
		}

	}
}