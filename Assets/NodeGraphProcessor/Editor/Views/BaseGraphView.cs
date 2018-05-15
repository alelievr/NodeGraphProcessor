using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using System.Linq;
using System;

using Object = UnityEngine.Object;

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
			serializeGraphElements = SerializeGraphElementsCallback;
			canPasteSerializedData = CanPasteSerializedDataCallback;
			unserializeAndPaste = UnserializeAndPasteCallback;
            graphViewChanged = GraphViewChangedCallback;

			InitializeManipulators();
			
			SetupZoom(0.1f, ContentZoomer.DefaultMaxScale);
	
			this.StretchToParentSize();

			UpdateViewTransform(Vector2.one * 500, Vector3.one);
		}
	
		protected override bool canCopySelection
		{
			get { return selection.OfType< BaseNodeView >().Any(); }
		}

		string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
		{
			//TODO: make a copy-paste class to store these serialized datas
			var serializedNodes = new List< JsonElement >();

			foreach (var node in elements.Where(e => e is BaseNodeView))
			{
				Debug.Log("added one node: " + node);
				serializedNodes.Add(JsonSerializer.Serialize< BaseNode >(((node) as BaseNodeView).nodeTarget));
			}
			
			return JsonUtility.ToJson(serializedNodes, true);
		}

		bool CanPasteSerializedDataCallback(string serializedData)
		{
			return !String.IsNullOrEmpty(serializedData);
		}

		void UnserializeAndPasteCallback(string operationName, string serializedData)
		{
			Debug.Log("json: " + serializedData);
            graph.RegisterCompleteObjectUndo(operationName);

			var nodes = JsonUtility.FromJson< List< BaseNode > >(serializedData);

			foreach (var node in nodes)
			{
				Debug.Log("Added one node: " + node);
				AddNode(node);
			}
		}

		GraphViewChange GraphViewChangedCallback(GraphViewChange changes)
		{
			if (changes.elementsToRemove != null)
			{
				graph.RegisterCompleteObjectUndo("Remove Elements");

				//Handle ourselve the edge and node remove
				changes.elementsToRemove.RemoveAll(e => {
					var edge = e as EdgeView;
					var node = e as BaseNodeView;
	
					if (edge != null)
					{
						Disconnect(edge);
						return true;
					}
					if (node != null)
					{
						graph.RemoveNode(node.nodeTarget);
						RemoveElement(node);
						return true;
					}
					return false;
				});
			}

			return changes;
		}

		public void Disconnect(EdgeView e)
		{
			var serializableEdge = e.userData as SerializableEdge;

			if (serializableEdge != null)
				graph.Disconnect(serializableEdge.GUID);

			RemoveElement(e);
			
			if (e.input != null)
			{
				var inputNodeView = e.input.node as BaseNodeView;
				inputNodeView.RefreshPorts();
				e.input.Disconnect(e);
			}
			if (e.output != null)
			{
				var outputNodeView = e.output.node as BaseNodeView;
				e.output.Disconnect(e);
				outputNodeView.RefreshPorts();
			}

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
			// contentViewContainer.transform.position = graph.position;
			// UpdateViewTransform(graph.position, graph.scale);

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
					userData = serializedEdge,
					input = inputNodeView.GetPortFromFieldName(serializedEdge.inputFieldName),
					output = outputNodeView.GetPortFromFieldName(serializedEdge.outputFieldName)
				};
				
				Connect(edgeView, false);
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
				e.userData = graph.Connect(
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