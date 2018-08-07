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

		Dictionary< Type, PinnedElementView >		pinnedElements = new Dictionary< Type, PinnedElementView >();

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

		public override List< Port > GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			var compatiblePorts = new List< Port >();
			var startPortView = startPort as PortView;

			compatiblePorts.AddRange(ports.ToList().Where(p => {
				var portView = p as PortView;

				if (portView.direction == startPortView.direction)
					return false;

				//Check if there is custom adapters for this assignation
				if (CustomPortIO.IsAssignable(startPortView.portType, portView.portType))
					return true;
				
				//Check for type assignability
				if (!portView.portType.IsReallyAssignableFrom(startPortView.portType))
					return false;
				
				//Check if the edge already exists
				if (portView.GetEdges().Any(e => e.input == startPortView || e.output == startPort))
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
			evt.menu.AppendAction("View/Processor", (e) => ToggleView< ProcessorView >(), (e) => GetPinnedElementStatus< ProcessorView >());
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
			foreach (var viewType in graph.pinnedWindows)
				OpenPinned(viewType.editorType.type);
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
					Disconnect(edge, serializeToGraph);

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
		
		public void Disconnect(EdgeView e, bool serializeToGraph = true)
		{
			var serializableEdge = e.userData as SerializableEdge;

			RemoveElement(e);
			
			if (e?.input?.node != null)
			{
				var inputNodeView = e.input.node as BaseNodeView;
				e.input.Disconnect(e);
				inputNodeView.nodeTarget.OnEdgeDisonnected(e.serializedEdge);
				inputNodeView.RefreshPorts();
			}
			if (e?.output?.node != null)
			{
				var outputNodeView = e.output.node as BaseNodeView;
				e.output.Disconnect(e);
				outputNodeView.nodeTarget.OnEdgeDisonnected(e.serializedEdge);
				outputNodeView.RefreshPorts();
			}

			// Remove the serialized edge if there was one
			if (serializableEdge != null)
			{
				if (serializeToGraph)
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

		public void ToggleView< T >() where T : PinnedElementView
		{
			ToggleView(typeof(T));
		}

		public void ToggleView(Type type)
		{
			PinnedElementView view;
			pinnedElements.TryGetValue(type, out view);

			if (view == null)
				OpenPinned(type);
			else
				ClosePinned(type, view);
		}

		public void OpenPinned(Type type)
		{
			PinnedElementView view;

			if (type == null)
				return ;

			PinnedElement elem = graph.OpenPinned(type);

			view = Activator.CreateInstance(type) as PinnedElementView;
			pinnedElements[type] = view;

			view.InitializeGraphView(elem, this);
			
			ConfinedDragger masterPreviewViewDraggable = new ConfinedDragger(this);
			masterPreviewViewDraggable.onDragEnd = () => elem.position = view.transform.position;
			view.AddManipulator(masterPreviewViewDraggable);
			Add(view);
		}

		public void ClosePinned(Type type, PinnedElementView elem)
		{
			pinnedElements.Remove(type);
			Remove(elem);
			graph.ClosePinned(type);
		}

		public StatusFlags GetPinnedElementStatus< T >() where T : PinnedElementView
		{
			return GetPinnedElementStatus(typeof(T));
		}

		public StatusFlags GetPinnedElementStatus(Type type)
		{
			var pinned = graph.pinnedWindows.Find(p => p.editorType.type == type);

			if (pinned != null && pinned.opened)
				return StatusFlags.Checked;
			else
				return StatusFlags.Normal;
		}

		#endregion

	}
}