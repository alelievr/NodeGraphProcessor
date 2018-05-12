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
		
		List< EdgeView >						edgeViews = new List< EdgeView >();

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
					RemoveElement(n);
					return true;
				}
				return false;
			});

			edgeViews.RemoveAll(e => {
				if (e.selected)
				{
					Disconnect(e);
					return true;
				}
				return false;
			});
		}

		public void Disconnect(EdgeView e)
		{
			var inputNodeView = e.input.node as BaseNodeView;
			var outputNodeView = e.output.node as BaseNodeView;

			graph.Disconnect(inputNodeView.nodeTarget, e.input.portName, outputNodeView.nodeTarget, e.output.portName);

			e.input.Disconnect(e);
			e.output.Disconnect(e);

			inputNodeView.RefreshPorts();
			outputNodeView.RefreshPorts();

			RemoveElement(e);
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

			Debug.Log("INIT with graph: " + graph);

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
				var inputNodeView = nodeViewsPerNode[serializedEdge.inputNode];
				var outputNodeView = nodeViewsPerNode[serializedEdge.outputNode];
				var edgeView = new EdgeView() {
					input = inputNodeView.GetPortFromFieldName(serializedEdge.inputFieldName),
					output = outputNodeView.GetPortFromFieldName(serializedEdge.outputFieldName)
				};
				
				Connect(edgeView, false);
			}
		}

		protected bool AddNode(BaseNode node)
		{
			AddNodeView(node);

			Debug.Log("graph: " + graph);

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

			nodeViews.Add(baseNodeView);
			nodeViewsPerNode[node] = baseNodeView;

			return true;
		}

		public void Connect(EdgeView e, bool serializeToGraph = true)
		{
			if (e.input == null || e.output == null)
				return ;

			AddElement(e);
			
			e.input.Connect(e);
			e.output.Connect(e);

			var inputNodeView = e.input.node as BaseNodeView;
			var outputNodeView = e.output.node as BaseNodeView;
			
			edgeViews.Add(e);

			if (serializeToGraph)
			{
				graph.Connect(
					inputNodeView.nodeTarget, e.input.portName,
					outputNodeView.nodeTarget, e.output.portName
				);
			}
			
			inputNodeView.RefreshPorts();
			outputNodeView.RefreshPorts();

			e.isConnected = true;
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