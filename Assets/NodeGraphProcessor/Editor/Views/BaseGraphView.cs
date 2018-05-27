using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using System.Linq;
using System;

using StatusFlags = UnityEngine.Experimental.UIElements.ContextualMenu.MenuAction.StatusFlags;

using Object = UnityEngine.Object;

namespace GraphProcessor
{
	public class BaseGraphView : GraphView
	{
		public BaseGraph							graph;
		
		public EdgeConnectorListener				connectorListener;

		public List< BaseNodeView >					nodeViews = new List< BaseNodeView >();
		public Dictionary< BaseNode, BaseNodeView >	nodeViewsPerNode = new Dictionary< BaseNode, BaseNodeView >();
		public List< EdgeView >						edgeViews = new List< EdgeView >();
        public List< CommentBlockView >         	commentBlockViews = new List< CommentBlockView >();

		Dictionary< Type, BaseGraphElementView >	uniqueElements = new Dictionary< Type, BaseGraphElementView >();

		public delegate void ComputeOrderUpdatedDelegate();

		public event ComputeOrderUpdatedDelegate	computeOrderUpdated;

		public BaseGraphView()
		{
			serializeGraphElements = SerializeGraphElementsCallback;
			canPasteSerializedData = CanPasteSerializedDataCallback;
			unserializeAndPaste = UnserializeAndPasteCallback;
            graphViewChanged = GraphViewChangedCallback;
			viewTransformChanged = ViewTransformChangedCallback;
            elementResized = ElementResizedCallback;

			InitializeManipulators();

			RegisterCallback< KeyDownEvent >(KeyDownCallback);
			
			SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
	
			this.StretchToParentSize();
		}

		#region Callbacks
	
		protected override bool canCopySelection
		{
            get { return selection.Any(e => e is BaseNodeView || e is CommentBlockView); }
		}

		protected override bool canCutSelection
		{
            get { return selection.Any(e => e is BaseNodeView || e is CommentBlockView); }
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

            foreach (var serializedCommentBlock in data.copiedCommentBlocks)
            {
                var commentBlock = JsonSerializer.Deserialize<CommentBlock>(serializedCommentBlock);

                //Same than for node
                commentBlock.OnCreated();
                commentBlock.position.position += new Vector2(20, 20);

                AddCommentBlock(commentBlock);
            }
		}

		GraphViewChange GraphViewChangedCallback(GraphViewChange changes)
		{
			if (changes.elementsToRemove != null)
			{
				RegisterCompleteObjectUndo("Remove Elements");

				//Handle ourselves the edge and node remove
				changes.elementsToRemove.RemoveAll(e => {
					var edge = e as EdgeView;
					var node = e as BaseNodeView;
                    var commentBlock = e as CommentBlockView;
	
					if (edge != null)
					{
						Disconnect(edge);
						return true;
					}
					else if (node != null)
					{
						graph.RemoveNode(node.nodeTarget);
						RemoveElement(node);
						return true;
					}
                    else if (commentBlock != null)
                    {
                        graph.RemoveCommentBlock(commentBlock.commentBlock);
                        RemoveElement(commentBlock);
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

        void ElementResizedCallback(VisualElement elem)
        {
            var commentBlockView = elem as CommentBlockView;

            if (commentBlockView != null)
                commentBlockView.commentBlock.size = commentBlockView.GetPosition().size;
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

			compatiblePorts.AddRange(ports.ToList().Where(p => {
				var portView = p as PortView;

				if (portView.direction == startPort.direction)
					return false;
				
				if (!portView.portType.IsReallyAssignableFrom(startPort.portType))
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
			BuildCreateContextualMenu(evt);
			BuildViewContextualMenu(evt);
			base.BuildContextualMenu(evt);
		}

		protected void BuildCreateContextualMenu(ContextualMenuPopulateEvent evt)
		{
			Vector2 position = evt.mousePosition - (Vector2)viewTransform.position;
            evt.menu.AppendAction("Create/Comment Block", (e) => AddCommentBlock(new CommentBlock("New Comment Block", position)), ContextualMenu.MenuAction.AlwaysEnabled);
		}

		protected void BuildViewContextualMenu(ContextualMenuPopulateEvent evt)
		{
			evt.menu.AppendAction("View/Processor", (e) => ToggleView< ProcessorView >(), (e) => GetViewStatus< ProcessorView >());
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
			InitializeViews();
            InitializeCommentBlocks();

			UpdateComputeOrder();
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

		void InitializeViews()
		{
			foreach (var viewType in graph.views)
				OpenView(viewType.type, false);
		}

        void InitializeCommentBlocks()
        {
            foreach (var commentBlock in graph.commentBlocks)
                AddCommentBlockView(commentBlock);
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
			
			UpdateComputeOrder();

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

        public void AddCommentBlock(CommentBlock block)
        {
            graph.AddCommentBlock(block);
            block.OnCreated();
            AddCommentBlockView(block);
        }

		public void AddCommentBlockView(CommentBlock block)
		{
			var c = new CommentBlockView();

			c.Initialize(this, block);

			AddElement(c);

            commentBlockViews.Add(c);
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
				Debug.LogError("Connect aborted !");
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
			
			inputNodeView.nodeTarget.OnEdgeConnected(e.userData as SerializableEdge);
			outputNodeView.nodeTarget.OnEdgeConnected(e.userData as SerializableEdge);

			e.isConnected = true;

			if (serializeToGraph)
				UpdateComputeOrder();
		}
		
		public void Disconnect(EdgeView e)
		{
			var serializableEdge = e.userData as SerializableEdge;

			RemoveElement(e);
			
			if (e?.input?.node != null)
			{
				var inputNodeView = e.input.node as BaseNodeView;
				e.input.Disconnect(e);
				inputNodeView.RefreshPorts();
				inputNodeView.nodeTarget.OnEdgeDisonnected(e.serializedEdge);
			}
			if (e?.output?.node != null)
			{
				var outputNodeView = e.output.node as BaseNodeView;
				e.output.Disconnect(e);
				outputNodeView.RefreshPorts();
				outputNodeView.nodeTarget.OnEdgeDisonnected(e.serializedEdge);
			}

			if (serializableEdge != null)
			{
				graph.Disconnect(serializableEdge.GUID);
				UpdateComputeOrder();
			}
		}

		public void UpdateComputeOrder()
		{
			graph.UpdateComputeOrder();

			if (computeOrderUpdated != null)
				computeOrderUpdated();
		}

		public void RegisterCompleteObjectUndo(string name)
		{
			Undo.RegisterCompleteObjectUndo(graph, name);
		}

		public void SaveGraphToDisk()
		{
			EditorUtility.SetDirty(graph);
		}

		public void ToggleView< T >() where T : BaseGraphElementView
		{
			ToggleView(typeof(T));
		}

		public void ToggleView(Type type)
		{
			BaseGraphElementView elem;
			uniqueElements.TryGetValue(type, out elem);

			if (elem == null)
				OpenView(type);
			else
				CloseView(type, elem);
		}

		public void OpenView(Type type, bool serializeToGraph = true)
		{
			BaseGraphElementView elem;

			if (type == null)
				return ;

			elem = Activator.CreateInstance(type) as BaseGraphElementView;
			uniqueElements[type] = elem;

			elem.InitializeGraphView(this);

			AddElement(elem);
			
			if (serializeToGraph)
				graph.AddView(type);
		}

		public void CloseView(Type type, BaseGraphElementView elem)
		{
			uniqueElements.Remove(type);
			RemoveElement(elem);
			graph.RemoveView(type);
		}

		public StatusFlags GetViewStatus< T >() where T : GraphElement
		{
			return GetViewStatus(typeof(T));
		}

		public StatusFlags GetViewStatus(Type type)
		{
			if (uniqueElements.ContainsKey(type))
				return StatusFlags.Checked;
			else
				return StatusFlags.Normal;
		}

		#endregion

	}
}