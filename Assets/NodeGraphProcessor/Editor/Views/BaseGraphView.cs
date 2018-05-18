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
			viewTransformChanged = ViewTransformChangedCallback;

			InitializeManipulators();

			RegisterCallback< KeyDownEvent >(KeyDownCallback);
			
			SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
	
			this.StretchToParentSize();
		}

		#region Callbacks
	
		protected override bool canCopySelection
		{
			//TODO: add block comment type
			get { return selection.OfType< BaseNodeView >().Any(); }
		}

		protected override bool canCutSelection
		{
			//TODO: add block comment type
			get { return selection.OfType< BaseNodeView >().Any(); }
		}

		string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
		{
			var data = new CopyPasteHelper();

			foreach (var nodeView in elements.Where(e => e is BaseNodeView))
			{
				var node = ((nodeView) as BaseNodeView).nodeTarget;
				data.copiedNodes.Add(JsonSerializer.Serialize< BaseNode >(node));
			}

			foreach (var commentBlockView in elements.Where(e => e is CommentBlockView))
			{
				var commentBlock = (commentBlockView as CommentBlockView).commentBlock;
				data.copiedCommentBlocks.Add(JsonSerializer.Serialize< CommentBlock >(commentBlock));
			}

			ClearSelection();

			return JsonUtility.ToJson(data, true);
		}

		bool CanPasteSerializedDataCallback(string serializedData)
		{
			try {
				return JsonUtility.FromJson(serializedData, typeof(CopyPasteHelper)) != null;
			} catch {
				return false;
			}
		}

		void UnserializeAndPasteCallback(string operationName, string serializedData)
		{
			var data = JsonUtility.FromJson< CopyPasteHelper >(serializedData);

            RegisterCompleteObjectUndo(operationName);

			foreach (var serializedNode in data.copiedNodes)
			{
				var node = JsonSerializer.DeserializeNode(serializedNode);

				//Call OnNodeCreated on the new fresh copied node
				node.OnNodeCreated();
				//And move a bit the new node
				node.position.position += new Vector2(20, 20);

				AddNode(node);

				//Select the new node
				AddToSelection(nodeViewsPerNode[node]);
			}

			//TODO: comment block
		}

		GraphViewChange GraphViewChangedCallback(GraphViewChange changes)
		{
			if (changes.elementsToRemove != null)
			{
				RegisterCompleteObjectUndo("Remove Elements");

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

		void ViewTransformChangedCallback(GraphView view)
		{
			graph.position = viewTransform.position;
			graph.scale = viewTransform.scale;
		}

		public override void OnPersistentDataReady()
		{
			//We set the position and scale saved in the graph asset file
			Vector3 pos = graph.position;
			Vector3 scale = graph.scale;

			base.OnPersistentDataReady();

			UpdateViewTransform(pos, scale);
		}

		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			var compatiblePorts = new List<Port>();

			Type startPortType = startPort.portType;

			if ((startPort as PortView).isMultiple)
				startPortType = startPortType.GetGenericArguments()[0];

			compatiblePorts.AddRange(ports.ToList().Where(p => {
				var portView = p as PortView;
				Type portType = portView.portType;

				if (portView.isMultiple)
					portType = portType.GetGenericArguments()[0];

				if (portView.direction == startPort.direction)
					return false;
				
				if (!portType.IsReallyAssignableFrom(startPortType))
					return false;
				
				//if the edge already exists
				if (portView.GetEdges().Any(e => e.input == startPort || e.output == startPort))
					return false;
				
				return true;
			}));

			return compatiblePorts;
		}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			Vector2 position = evt.mousePosition - (Vector2)viewTransform.position;
			evt.menu.AppendAction("Create/Comment Block", (e) => AddCommentBlockView(new CommentBlock("New Comment Block"), position), ContextualMenu.MenuAction.AlwaysEnabled);
			base.BuildContextualMenu(evt);
		}

		void KeyDownCallback(KeyDownEvent e)
		{
			if (e.keyCode == KeyCode.S)
			{
				SaveGraphToDisk();
				e.StopPropagation();
			}
		}

		#endregion

		#region Initialization

		public void Initialize(BaseGraph graph)
		{
			if (this.graph != null)
				SaveGraphToDisk();
			
			this.graph = graph;
			
            connectorListener = new EdgeConnectorListener(this);

			InitializeNodeViews();
			InitializeEdgeViews();
		}

		void InitializeNodeViews()
		{
			graph.nodes.RemoveAll(n => n == null);
			
			foreach (var node in graph.nodes)
				AddNodeView(node);
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

		protected virtual void InitializeManipulators()
		{
			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new ContentZoomer());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());
			this.AddManipulator(new ClickSelector());
		}

		#endregion

		#region Graph content modification

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
				viewType = typeof(BaseNodeView);

			var baseNodeView = Activator.CreateInstance(viewType) as BaseNodeView;
			baseNodeView.Initialize(this, node);
			AddElement(baseNodeView);

			nodeViews.Add(baseNodeView);
			nodeViewsPerNode[node] = baseNodeView;

			return true;
		}

		public void AddCommentBlock(string title, Vector2 position)
		{
			AddCommentBlockView(new CommentBlock(title), position);
		}

		public void AddCommentBlockView(CommentBlock block, Vector2 positiont)
		{
			var c = new CommentBlockView();

			c.Initialize(this, block);

			AddElement(c);
		}

		public void Connect(EdgeView e, bool serializeToGraph = true)
		{
			if (e.input == null || e.output == null)
				return ;
			
			//If the input port does not support multi-connection, we remove them
			if (!(e.input as PortView).isMultiple)
				foreach (var edge in edgeViews.Where(ev => ev.input == e.input))
					Disconnect(edge);

			AddElement(e);
			
			e.input.Connect(e);
			e.output.Connect(e);

			var inputNodeView = e.input.node as BaseNodeView;
			var outputNodeView = e.output.node as BaseNodeView;

			if (inputNodeView == null || outputNodeView == null)
			{
				Debug.Log("Connect aborted !");
				return ;
			}
			
			edgeViews.Add(e);

			if (serializeToGraph)
			{
				e.userData = graph.Connect(
					inputNodeView.nodeTarget, (e.input as PortView).fieldName,
					outputNodeView.nodeTarget, (e.output as PortView).fieldName
				);
			}
			
			inputNodeView.RefreshPorts();
			outputNodeView.RefreshPorts();

			e.isConnected = true;
		}
		
		public void Disconnect(EdgeView e)
		{
			var serializableEdge = e.userData as SerializableEdge;

			if (serializableEdge != null)
				graph.Disconnect(serializableEdge.GUID);

			RemoveElement(e);
			
			if (e?.input?.node != null)
			{
				var inputNodeView = e.input.node as BaseNodeView;
				inputNodeView.RefreshPorts();
				e.input.Disconnect(e);
			}
			if (e?.output?.node != null)
			{
				var outputNodeView = e.output.node as BaseNodeView;
				e.output.Disconnect(e);
				outputNodeView.RefreshPorts();
			}
		}

		public void RegisterCompleteObjectUndo(string name)
		{
			Undo.RegisterCompleteObjectUndo(graph, name);
		}

		public void SaveGraphToDisk()
		{
			EditorUtility.SetDirty(graph);
		}

		#endregion

	}
}