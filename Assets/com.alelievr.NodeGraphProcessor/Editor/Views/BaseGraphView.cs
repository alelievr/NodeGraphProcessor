﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System;
using UnityEditor.SceneManagement;

using Status = UnityEngine.UIElements.DropdownMenuAction.Status;
using Object = UnityEngine.Object;

namespace GraphProcessor
{
	/// <summary>
	/// Base class to write a custom view for a node
	/// </summary>
	public class BaseGraphView : GraphView, IDisposable
	{
		public delegate void ComputeOrderUpdatedDelegate();
		public delegate void NodeDuplicatedDelegate(BaseNode duplicatedNode, BaseNode newNode);

		/// <summary>
		/// Graph that owns of the node
		/// </summary>
		public BaseGraph							graph;

		/// <summary>
		/// Connector listener that will create the edges between ports
		/// </summary>
		public BaseEdgeConnectorListener			connectorListener;

		/// <summary>
		/// List of all node views in the graph
		/// </summary>
		/// <typeparam name="BaseNodeView"></typeparam>
		/// <returns></returns>
		public List< BaseNodeView >					nodeViews = new List< BaseNodeView >();

		/// <summary>
		/// Dictionary of the node views accessed view the node instance, faster than a Find in the node view list
		/// </summary>
		/// <typeparam name="BaseNode"></typeparam>
		/// <typeparam name="BaseNodeView"></typeparam>
		/// <returns></returns>
		public Dictionary< BaseNode, BaseNodeView >	nodeViewsPerNode = new Dictionary< BaseNode, BaseNodeView >();

		/// <summary>
		/// List of all edge views in the graph
		/// </summary>
		/// <typeparam name="EdgeView"></typeparam>
		/// <returns></returns>
		public List< EdgeView >						edgeViews = new List< EdgeView >();

		/// <summary>
		/// List of all group views in the graph
		/// </summary>
		/// <typeparam name="GroupView"></typeparam>
		/// <returns></returns>
        public List< GroupView >         			groupViews = new List< GroupView >();

		/// <summary>
		/// List of all stack node views in the graph
		/// </summary>
		/// <typeparam name="BaseStackNodeView"></typeparam>
		/// <returns></returns>
		public List< BaseStackNodeView >			stackNodeViews = new List< BaseStackNodeView >();

		Dictionary< Type, PinnedElementView >		pinnedElements = new Dictionary< Type, PinnedElementView >();

		CreateNodeMenuWindow						createNodeMenu;

		/// <summary>
		/// Triggered just after the graph is initialized
		/// </summary>
		public event Action							initialized;

		/// <summary>
		/// Triggered just after the compute order of the graph is updated
		/// </summary>
		public event ComputeOrderUpdatedDelegate	computeOrderUpdated;

		// Safe event relay from BaseGraph (safe because you are sure to always point on a valid BaseGraph
		// when one of these events is called), a graph switch can occur between two call tho
		/// <summary>
		/// Same event than BaseGraph.onExposedParameterListChanged
		/// Safe event (not triggered in case the graph is null).
		/// </summary>
		public event Action				onExposedParameterListChanged;

		/// <summary>
		/// Same event than BaseGraph.onExposedParameterModified
		/// Safe event (not triggered in case the graph is null).
		/// </summary>
		public event Action< string >	onExposedParameterModified;

		/// <summary>
		/// Triggered when a node is duplicated (crt-d) or copy-pasted (crtl-c/crtl-v)
		/// </summary>
		public event NodeDuplicatedDelegate	nodeDuplicated;

		/// <summary>
		/// Object to handle nodes that shows their UI in the inspector.
		/// </summary>
		[NonSerialized]
		protected NodeInspectorObject		nodeInspector;

		public BaseGraphView(EditorWindow window)
		{
			serializeGraphElements = SerializeGraphElementsCallback;
			canPasteSerializedData = CanPasteSerializedDataCallback;
			unserializeAndPaste = UnserializeAndPasteCallback;
            graphViewChanged = GraphViewChangedCallback;
			viewTransformChanged = ViewTransformChangedCallback;
            elementResized = ElementResizedCallback;

			RegisterCallback< KeyDownEvent >(KeyDownCallback);
			RegisterCallback< DragPerformEvent >(DragPerformedCallback);
			RegisterCallback< DragUpdatedEvent >(DragUpdatedCallback);
			RegisterCallback< MouseDownEvent >(MouseDownCallback);
			RegisterCallback< MouseUpEvent >(MouseUpCallback);

			InitializeManipulators();

			SetupZoom(0.05f, 2f);

			Undo.undoRedoPerformed += ReloadView;

			createNodeMenu = ScriptableObject.CreateInstance< CreateNodeMenuWindow >();
			createNodeMenu.Initialize(this, window);

			if (nodeInspector == null)
				nodeInspector = CreateNodeInspectorObject();

			this.StretchToParentSize();
		}

		protected virtual NodeInspectorObject CreateNodeInspectorObject()
		{
			var inspector = ScriptableObject.CreateInstance<NodeInspectorObject>();
			inspector.name = "Node Inspector";
			inspector.hideFlags = HideFlags.HideAndDontSave ^ HideFlags.NotEditable;

			return inspector;
		}

		#region Callbacks

		protected override bool canCopySelection
		{
            get { return selection.Any(e => e is BaseNodeView || e is GroupView); }
		}

		protected override bool canCutSelection
		{
            get { return selection.Any(e => e is BaseNodeView || e is GroupView); }
		}

		string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
		{
			var data = new CopyPasteHelper();

			foreach (BaseNodeView nodeView in elements.Where(e => e is BaseNodeView))
				data.copiedNodes.Add(JsonSerializer.SerializeNode(nodeView.nodeTarget));

			foreach (GroupView groupView in elements.Where(e => e is GroupView))
				data.copiedGroups.Add(JsonSerializer.Serialize(groupView.group));

			foreach (EdgeView edgeView in elements.Where(e => e is EdgeView))
				data.copiedEdges.Add(JsonSerializer.Serialize(edgeView.serializedEdge));

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

			Dictionary<string, BaseNode> copiedNodesMap = new Dictionary<string, BaseNode>();

			foreach (var serializedNode in data.copiedNodes)
			{
				var node = JsonSerializer.DeserializeNode(serializedNode);

				if (node == null)
					continue ;

				string sourceGUID = node.GUID;
				graph.nodesPerGUID.TryGetValue(sourceGUID, out var sourceNode);
				//Call OnNodeCreated on the new fresh copied node
				node.OnNodeCreated();
				//And move a bit the new node
				node.position.position += new Vector2(20, 20);

				var newNodeView = AddNode(node);

				// If the nodes were copied from another graph, then the source is null
				if (sourceNode != null)
					nodeDuplicated?.Invoke(sourceNode, node);
				copiedNodesMap[sourceGUID] = node;

				//Select the new node
				AddToSelection(nodeViewsPerNode[node]);
			}

            foreach (var serializedGroup in data.copiedGroups)
            {
                var group = JsonSerializer.Deserialize<Group>(serializedGroup);

                //Same than for node
                group.OnCreated();

				// try to centre the created node in the screen
                group.position.position += new Vector2(20, 20);

				var oldGUIDList = group.innerNodeGUIDs.ToList();
				group.innerNodeGUIDs.Clear();
				foreach (var guid in oldGUIDList)
				{
					graph.nodesPerGUID.TryGetValue(guid, out var node);
					
					// In case group was copied from another graph
					if (node == null)
					{
						copiedNodesMap.TryGetValue(guid, out node);
						group.innerNodeGUIDs.Add(node.GUID);
					}
					else
					{
						group.innerNodeGUIDs.Add(copiedNodesMap[guid].GUID);
					}
				}

                AddGroup(group);
            }

            foreach (var serializedEdge in data.copiedEdges)
			{
				var edge = JsonSerializer.Deserialize<SerializableEdge>(serializedEdge);

				edge.Deserialize();

				// Find port of new nodes:
				copiedNodesMap.TryGetValue(edge.inputNode.GUID, out var oldInputNode);
				copiedNodesMap.TryGetValue(edge.outputNode.GUID, out var oldOutputNode);

				// We avoid to break the graph by replacing unique connections:
				if (oldInputNode == null && !edge.inputPort.portData.acceptMultipleEdges || !edge.outputPort.portData.acceptMultipleEdges)
					continue;

				oldInputNode = oldInputNode ?? edge.inputNode;
				oldOutputNode = oldOutputNode ?? edge.outputNode;

				var inputPort = oldInputNode.GetPort(edge.inputPort.fieldName, edge.inputPortIdentifier);
				var outputPort = oldOutputNode.GetPort(edge.outputPort.fieldName, edge.outputPortIdentifier);

				var newEdge = SerializableEdge.CreateNewEdge(graph, inputPort, outputPort);

				if (nodeViewsPerNode.ContainsKey(oldInputNode) && nodeViewsPerNode.ContainsKey(oldOutputNode))
				{
					var edgeView = new EdgeView()
					{
						userData = newEdge,
						input = nodeViewsPerNode[oldInputNode].GetPortViewFromFieldName(newEdge.inputFieldName, newEdge.inputPortIdentifier),
						output = nodeViewsPerNode[oldOutputNode].GetPortViewFromFieldName(newEdge.outputFieldName, newEdge.outputPortIdentifier)
					};

					Connect(edgeView);
				}
			}
		}

		GraphViewChange GraphViewChangedCallback(GraphViewChange changes)
		{
			if (changes.elementsToRemove != null)
			{
				RegisterCompleteObjectUndo("Remove Graph Elements");

				// Destroy priority of objects
				// We need nodes to be destroyed first because we can have a destroy operation that uses node connections
				changes.elementsToRemove.Sort((e1, e2) => {
					int GetPriority(GraphElement e)
					{
						if (e is BaseNodeView)
							return 0;
						else
							return 1;
					}
					return GetPriority(e1).CompareTo(GetPriority(e2));
				});

				//Handle ourselves the edge and node remove
				changes.elementsToRemove.RemoveAll(e => {

					switch (e)
					{
						case EdgeView edge:
							Disconnect(edge);
							return true;
						case BaseNodeView node:
							ExceptionToLog.Call(() => node.OnRemoved());
							graph.RemoveNode(node.nodeTarget);
							RemoveElement(node);
							if (Selection.activeObject == nodeInspector)
								UpdateNodeInspectorSelection();
							return true;
						case GroupView group:
							graph.RemoveGroup(group.group);
							RemoveElement(group);
							return true;
						case ExposedParameterFieldView blackboardField:
							graph.RemoveExposedParameter(blackboardField.parameter);
							return true;
						case BaseStackNodeView stackNodeView:
							graph.RemoveStackNode(stackNodeView.stackNode);
							RemoveElement(stackNodeView);
							return true;
					}

					return false;
				});
			}

			return changes;
		}

		void GraphChangesCallback(GraphChanges changes)
		{
			if (changes.removedEdge != null)
			{
				var edge = edgeViews.FirstOrDefault(e => e.serializedEdge == changes.removedEdge);

				DisconnectView(edge);
			}
		}

		void ViewTransformChangedCallback(GraphView view)
		{
			if (graph != null)
			{
				graph.position = viewTransform.position;
				graph.scale = viewTransform.scale;
			}
		}

        void ElementResizedCallback(VisualElement elem)
        {
            var groupView = elem as GroupView;

            if (groupView != null)
                groupView.group.size = groupView.GetPosition().size;
        }

		public override List< Port > GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			var compatiblePorts = new List< Port >();

			compatiblePorts.AddRange(ports.ToList().Where(p => {
				var portView = p as PortView;

				if (portView.owner == (startPort as PortView).owner)
					return false;

				if (p.direction == startPort.direction)
					return false;

				//Check for type assignability
				if (!BaseGraph.TypesAreConnectable(startPort.portType, p.portType))
					return false;

				//Check if the edge already exists
				if (portView.GetEdges().Any(e => e.input == startPort || e.output == startPort))
					return false;

				return true;
			}));

			return compatiblePorts;
		}

		/// <summary>
		/// Build the contextual menu shown when right clicking inside the graph view
		/// </summary>
		/// <param name="evt"></param>
		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			BuildGroupContextualMenu(evt);
			base.BuildContextualMenu(evt);
			BuildViewContextualMenu(evt);
			BuildSelectAssetContextualMenu(evt);
			BuildSaveAssetContextualMenu(evt);
			BuildHelpContextualMenu(evt);
		}

		/// <summary>
		/// Add the New Group entry to the context menu
		/// </summary>
		/// <param name="evt"></param>
		protected virtual void BuildGroupContextualMenu(ContextualMenuPopulateEvent evt)
		{
			Vector2 position = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            evt.menu.AppendAction("New Group", (e) => AddSelectionsToGroup(AddGroup(new Group("New Group", position))), DropdownMenuAction.AlwaysEnabled);
		}

		/// <summary>
		/// Add the View entry to the context menu
		/// </summary>
		/// <param name="evt"></param>
		protected virtual void BuildViewContextualMenu(ContextualMenuPopulateEvent evt)
		{
			evt.menu.AppendAction("View/Processor", (e) => ToggleView< ProcessorView >(), (e) => GetPinnedElementStatus< ProcessorView >());
		}

		/// <summary>
		/// Add the Select Asset entry to the context menu
		/// </summary>
		/// <param name="evt"></param>
		protected virtual void BuildSelectAssetContextualMenu(ContextualMenuPopulateEvent evt)
		{
			evt.menu.AppendAction("Select Asset", (e) => EditorGUIUtility.PingObject(graph), DropdownMenuAction.AlwaysEnabled);
		}

		/// <summary>
		/// Add the Save Asset entry to the context menu
		/// </summary>
		/// <param name="evt"></param>
		protected virtual void BuildSaveAssetContextualMenu(ContextualMenuPopulateEvent evt)
		{
			evt.menu.AppendAction("Save Asset", (e) => {
				EditorUtility.SetDirty(graph);
				AssetDatabase.SaveAssets();
			}, DropdownMenuAction.AlwaysEnabled);
		}

		/// <summary>
		/// Add the Help entry to the context menu
		/// </summary>
		/// <param name="evt"></param>
		protected void BuildHelpContextualMenu(ContextualMenuPopulateEvent evt)
		{
			evt.menu.AppendAction("Help/Reset Pinned Windows", e => {
				foreach (var kp in pinnedElements)
					kp.Value.ResetPosition();
			});
		}

		protected virtual void KeyDownCallback(KeyDownEvent e)
		{
			if (e.keyCode == KeyCode.S && e.commandKey)
			{
				SaveGraphToDisk();
				e.StopPropagation();
			}
			else if(nodeViews.Count > 0 && e.commandKey && e.altKey)
			{
				//	Node Aligning shortcuts
				switch(e.keyCode)
				{
					case KeyCode.LeftArrow:
						nodeViews[0].AlignToLeft();
						e.StopPropagation();
						break;
					case KeyCode.RightArrow:
						nodeViews[0].AlignToRight();
						e.StopPropagation();
						break;
					case KeyCode.UpArrow:
						nodeViews[0].AlignToTop();
						e.StopPropagation();
						break;
					case KeyCode.DownArrow:
						nodeViews[0].AlignToBottom();
						e.StopPropagation();
						break;
					case KeyCode.C:
						nodeViews[0].AlignToCenter();
						e.StopPropagation();
						break;
					case KeyCode.M:
						nodeViews[0].AlignToMiddle();
						e.StopPropagation();
						break;
				}
			}
		}

		void MouseUpCallback(MouseUpEvent e)
		{
			schedule.Execute(() => {
				if (DoesSelectionContainsInspectorNodes())
					UpdateNodeInspectorSelection();
			}).ExecuteLater(1);
		}

		void MouseDownCallback(MouseDownEvent e)
		{
			// When left clicking on the graph (not a node or something else)
			if (e.button == 0)
			{
				// Close all settings windows:
				nodeViews.ForEach(v => v.CloseSettings());
			}

			if (DoesSelectionContainsInspectorNodes())
				UpdateNodeInspectorSelection();
		}

		bool DoesSelectionContainsInspectorNodes()
			=> selection.Any(s => s is BaseNodeView v && v.nodeTarget.needsInspector);

		void DragPerformedCallback(DragPerformEvent e)
		{
			var mousePos = (e.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, e.localMousePosition);
			var dragData = DragAndDrop.GetGenericData("DragSelection") as List< ISelectable >;

			if (dragData == null)
				return;

			var exposedParameterFieldViews = dragData.OfType<ExposedParameterFieldView>();
			if (exposedParameterFieldViews.Any())
			{
				foreach (var paramFieldView in exposedParameterFieldViews)
				{
					RegisterCompleteObjectUndo("Create Parameter Node");
					var paramNode = BaseNode.CreateFromType< ParameterNode >(mousePos);
					paramNode.parameterGUID = paramFieldView.parameter.guid;
					AddNode(paramNode);
				}
			}
		}

		void DragUpdatedCallback(DragUpdatedEvent e)
        {
            var dragData = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
            bool dragging = false;

            if (dragData != null)
            {
                // Handle drag from exposed parameter view
                if (dragData.OfType<ExposedParameterFieldView>().Any())
				{
                    dragging = true;
				}
            }

            if (dragging)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            }

			UpdateNodeInspectorSelection();
        }

		#endregion

		#region Initialization

		void ReloadView()
		{
			// Force the graph to reload his datas (Undo have updated the serialized properties of the graph
			// so the one that are not serialized need to be synchronized)
			graph.Deserialize();

			// Remove everything
			RemoveNodeViews();
			RemoveEdges();
			RemoveGroups();
			RemoveStackNodeViews();

			// And re-add with new up to date datas
			InitializeNodeViews();
			InitializeEdgeViews();
            InitializeGroups();
			InitializeStackNodes();

			Reload();

			UpdateComputeOrder();
		}

		public void Initialize(BaseGraph graph)
		{
			if (this.graph != null)
			{
				SaveGraphToDisk();
				// Close pinned windows from old graph:
				pinnedElements.Clear();
			}

			this.graph = graph;

            connectorListener = CreateEdgeConnectorListener();

			// When pressing ctrl-s, we save the graph
			EditorSceneManager.sceneSaved += _ => SaveGraphToDisk();

			ClearGraphElements();

			InitializeGraphView();
			InitializeNodeViews();
			InitializeEdgeViews();
			InitializeViews();
            InitializeGroups();
			InitializeStackNodes();

			initialized?.Invoke();
			UpdateComputeOrder();

			InitializeView();
		}

		public void ClearGraphElements()
		{
			RemoveNodeViews();
			RemoveEdges();
			RemoveGroups();
			RemoveStackNodeViews();
			RemovePinnedElementViews();
		}

		/// <summary>
		/// Allow you to create your own edge connector listener
		/// </summary>
		/// <returns></returns>
		protected virtual BaseEdgeConnectorListener CreateEdgeConnectorListener()
		 => new BaseEdgeConnectorListener(this);

		void InitializeGraphView()
		{
			graph.onExposedParameterListChanged += () => onExposedParameterListChanged?.Invoke();
			graph.onExposedParameterModified += (s) => onExposedParameterModified?.Invoke(s);
			graph.onGraphChanges += GraphChangesCallback;
			viewTransform.position = graph.position;
			viewTransform.scale = graph.scale;
			nodeCreationRequest = (c) => SearchWindow.Open(new SearchWindowContext(c.screenMousePosition), createNodeMenu);
		}

		void InitializeNodeViews()
		{
			graph.nodes.RemoveAll(n => n == null);

			foreach (var node in graph.nodes)
			{
				var v = AddNodeView(node);
			}
		}

		void InitializeEdgeViews()
		{
			// Sanitize edges in case a node broke something while loading
			graph.edges.RemoveAll(edge => edge == null || edge.inputNode == null || edge.outputNode == null);

			foreach (var serializedEdge in graph.edges)
			{
				nodeViewsPerNode.TryGetValue(serializedEdge.inputNode, out var inputNodeView);
				nodeViewsPerNode.TryGetValue(serializedEdge.outputNode, out var outputNodeView);
				if (inputNodeView == null || outputNodeView == null)
					continue;

				var edgeView = new EdgeView() {
					userData = serializedEdge,
					input = inputNodeView.GetPortViewFromFieldName(serializedEdge.inputFieldName, serializedEdge.inputPortIdentifier),
					output = outputNodeView.GetPortViewFromFieldName(serializedEdge.outputFieldName, serializedEdge.outputPortIdentifier)
				};

				ConnectView(edgeView);
			}
		}

		void InitializeViews()
		{
			foreach (var pinnedElement in graph.pinnedElements)
			{
				if (pinnedElement.opened)
					OpenPinned(pinnedElement.editorType.type);
			}
		}

        void InitializeGroups()
        {
            foreach (var group in graph.groups)
                AddGroupView(group);
        }

		void InitializeStackNodes()
		{
			foreach (var stackNode in graph.stackNodes)
				AddStackNodeView(stackNode);
		}

		protected virtual void InitializeManipulators()
		{
			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());
		}

		protected virtual void Reload() {}

		#endregion

		#region Graph content modification

		public void UpdateNodeInspectorSelection()
		{
			if (nodeInspector.previouslySelectedObject != Selection.activeObject)
				nodeInspector.previouslySelectedObject = Selection.activeObject;

			HashSet<BaseNodeView> selectedNodeViews = new HashSet<BaseNodeView>();
			nodeInspector.selectedNodes.Clear();
			foreach (var e in selection)
			{
				if (e is BaseNodeView v && this.Contains(v) && v.nodeTarget.needsInspector)
					selectedNodeViews.Add(v);
			}

			if (selectedNodeViews.Count > 0)
			{
				nodeInspector.UpdateSelectedNodes(selectedNodeViews);
				if (Selection.activeObject != nodeInspector)
					Selection.activeObject = nodeInspector;
			}
		}

		public BaseNodeView AddNode(BaseNode node)
		{
			// This will initialize the node using the graph instance
			graph.AddNode(node);

			var view = AddNodeView(node);

			// Call create after the node have been initialized
			ExceptionToLog.Call(() => view.OnCreated());

			UpdateComputeOrder();

			return view;
		}

		public BaseNodeView AddNodeView(BaseNode node)
		{
			var viewType = NodeProvider.GetNodeViewTypeFromType(node.GetType());

			if (viewType == null)
				viewType = typeof(BaseNodeView);

			var baseNodeView = Activator.CreateInstance(viewType) as BaseNodeView;
			baseNodeView.Initialize(this, node);
			AddElement(baseNodeView);

			nodeViews.Add(baseNodeView);
			nodeViewsPerNode[node] = baseNodeView;

			return baseNodeView;
		}

		public void RemoveNodeView(BaseNodeView nodeView)
		{
			RemoveElement(nodeView);
			nodeViews.Remove(nodeView);
			nodeViewsPerNode.Remove(nodeView.nodeTarget);
		}

		void RemoveNodeViews()
		{
			foreach (var nodeView in nodeViews)
				RemoveElement(nodeView);
			nodeViews.Clear();
			nodeViewsPerNode.Clear();
		}

		void RemoveStackNodeViews()
		{
			foreach (var stackView in stackNodeViews)
				RemoveElement(stackView);
			stackNodeViews.Clear();
		}

		void RemovePinnedElementViews()
		{
			foreach (var pinnedView in pinnedElements.Values)
			{
				if (Contains(pinnedView))
					Remove(pinnedView);
			}
			pinnedElements.Clear();
		}

        public GroupView AddGroup(Group block)
        {
            graph.AddGroup(block);
            block.OnCreated();
            return AddGroupView(block);
        }

		public GroupView AddGroupView(Group block)
		{
			var c = new GroupView();

			c.Initialize(this, block);

			AddElement(c);

            groupViews.Add(c);
            return c;
		}

		public BaseStackNodeView AddStackNode(BaseStackNode stackNode)
		{
			graph.AddStackNode(stackNode);
			return AddStackNodeView(stackNode);
		}

		public BaseStackNodeView AddStackNodeView(BaseStackNode stackNode)
		{
			var viewType = StackNodeViewProvider.GetStackNodeCustomViewType(stackNode.GetType()) ?? typeof(BaseStackNodeView);
			var stackView = Activator.CreateInstance(viewType, stackNode) as BaseStackNodeView;

			AddElement(stackView);
			stackNodeViews.Add(stackView);

			stackView.Initialize(this);

			return stackView;
		}

		public void RemoveStackNodeView(BaseStackNodeView stackNodeView)
		{
			stackNodeViews.Remove(stackNodeView);
			RemoveElement(stackNodeView);
		}

        public void AddSelectionsToGroup(GroupView view)
        {
            foreach (var selectedNode in selection)
            {
                if (selectedNode is BaseNodeView)
                {
                    if (groupViews.Exists(x => x.ContainsElement(selectedNode as BaseNodeView)))
                        continue;

                    view.AddElement(selectedNode as BaseNodeView);
                }
            }
        }

		public void RemoveGroups()
		{
			foreach (var groupView in groupViews)
				RemoveElement(groupView);
			groupViews.Clear();
		}

		public bool CanConnectEdge(EdgeView e, bool autoDisconnectInputs = true)
		{
			if (e.input == null || e.output == null)
				return false;

			var inputPortView = e.input as PortView;
			var outputPortView = e.output as PortView;
			var inputNodeView = inputPortView.node as BaseNodeView;
			var outputNodeView = outputPortView.node as BaseNodeView;

			if (inputNodeView == null || outputNodeView == null)
			{
				Debug.LogError("Connect aborted !");
				return false;
			}

			return true;
		}

		public bool ConnectView(EdgeView e, bool autoDisconnectInputs = true)
		{
			if (!CanConnectEdge(e, autoDisconnectInputs))
				return false;
			
			var inputPortView = e.input as PortView;
			var outputPortView = e.output as PortView;
			var inputNodeView = inputPortView.node as BaseNodeView;
			var outputNodeView = outputPortView.node as BaseNodeView;

			//If the input port does not support multi-connection, we remove them
			if (autoDisconnectInputs && !(e.input as PortView).portData.acceptMultipleEdges)
			{
				foreach (var edge in edgeViews.Where(ev => ev.input == e.input).ToList())
				{
					// TODO: do not disconnect them if the connected port is the same than the old connected
					DisconnectView(edge);
				}
			}
			// same for the output port:
			if (autoDisconnectInputs && !(e.output as PortView).portData.acceptMultipleEdges)
			{
				foreach (var edge in edgeViews.Where(ev => ev.output == e.output).ToList())
				{
					// TODO: do not disconnect them if the connected port is the same than the old connected
					DisconnectView(edge);
				}
			}

			AddElement(e);

			e.input.Connect(e);
			e.output.Connect(e);

			// If the input port have been removed by the custom port behavior
			// we try to find if it's still here
			if (e.input == null)
				e.input = inputNodeView.GetPortViewFromFieldName(inputPortView.fieldName, inputPortView.portData.identifier);
			if (e.output == null)
				e.output = inputNodeView.GetPortViewFromFieldName(outputPortView.fieldName, outputPortView.portData.identifier);

			edgeViews.Add(e);

			inputNodeView.RefreshPorts();
			outputNodeView.RefreshPorts();

			// In certain cases the edge color is wrong so we patch it
			schedule.Execute(() => {
				e.UpdateEdgeControl();
			}).ExecuteLater(1);

			e.isConnected = true;

			return true;
		}

		public bool Connect(PortView inputPortView, PortView outputPortView, bool autoDisconnectInputs = true)
		{
			var inputPort = inputPortView.owner.nodeTarget.GetPort(inputPortView.fieldName, inputPortView.portData.identifier);
			var outputPort = outputPortView.owner.nodeTarget.GetPort(outputPortView.fieldName, outputPortView.portData.identifier);

			// Checks that the node we are connecting still exists
			if (inputPortView.owner.parent == null || outputPortView.owner.parent == null)
				return false;

			var newEdge = SerializableEdge.CreateNewEdge(graph, inputPort, outputPort);

			var edgeView = new EdgeView()
			{
				userData = newEdge,
				input = inputPortView,
				output = outputPortView,
			};

			return Connect(edgeView);
		}

		public bool Connect(EdgeView e, bool autoDisconnectInputs = true)
		{
			if (!CanConnectEdge(e, autoDisconnectInputs))
				return false;

			var inputPortView = e.input as PortView;
			var outputPortView = e.output as PortView;
			var inputNodeView = inputPortView.node as BaseNodeView;
			var outputNodeView = outputPortView.node as BaseNodeView;
			var inputPort = inputNodeView.nodeTarget.GetPort(inputPortView.fieldName, inputPortView.portData.identifier);
			var outputPort = outputNodeView.nodeTarget.GetPort(outputPortView.fieldName, outputPortView.portData.identifier);

			e.userData = graph.Connect(inputPort, outputPort, autoDisconnectInputs);

			ConnectView(e, autoDisconnectInputs);

			UpdateComputeOrder();

			return true;
		}

		public void DisconnectView(EdgeView e, bool refreshPorts = true)
		{
			if (e == null)
				return ;

			RemoveElement(e);

			if (e?.input?.node is BaseNodeView inputNodeView)
			{
				e.input.Disconnect(e);
				if (refreshPorts)
					inputNodeView.RefreshPorts();
			}
			if (e?.output?.node is BaseNodeView outputNodeView)
			{
				e.output.Disconnect(e);
				if (refreshPorts)
					outputNodeView.RefreshPorts();
			}

			edgeViews.Remove(e);
		}

		public void Disconnect(EdgeView e, bool refreshPorts = true)
		{
			// Remove the serialized edge if there is one
			if (e.userData is SerializableEdge serializableEdge)
				graph.Disconnect(serializableEdge.GUID);

			DisconnectView(e, refreshPorts);

			UpdateComputeOrder();
		}

		public void RemoveEdges()
		{
			foreach (var edge in edgeViews)
				RemoveElement(edge);
			edgeViews.Clear();
		}

		public void UpdateComputeOrder()
		{
			graph.UpdateComputeOrder();

			computeOrderUpdated?.Invoke();
		}

		public void RegisterCompleteObjectUndo(string name)
		{
			Undo.RegisterCompleteObjectUndo(graph, name);
		}

		public void SaveGraphToDisk()
		{
			if (graph == null)
				return ;

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

		public void OpenPinned< T >() where T : PinnedElementView
		{
			OpenPinned(typeof(T));
		}

		public void OpenPinned(Type type)
		{
			PinnedElementView view;

			if (type == null)
				return ;

			PinnedElement elem = graph.OpenPinned(type);

			if (!pinnedElements.ContainsKey(type))
			{
				view = Activator.CreateInstance(type) as PinnedElementView;
				if (view == null)
					return ;
				pinnedElements[type] = view;
				view.InitializeGraphView(elem, this);
			}
			view = pinnedElements[type];

			if (!Contains(view))
				Add(view);
		}

		public void ClosePinned< T >(PinnedElementView view) where T : PinnedElementView
		{
			ClosePinned(typeof(T), view);
		}

		public void ClosePinned(Type type, PinnedElementView elem)
		{
			pinnedElements.Remove(type);
			Remove(elem);
			graph.ClosePinned(type);
		}

		public Status GetPinnedElementStatus< T >() where T : PinnedElementView
		{
			return GetPinnedElementStatus(typeof(T));
		}

		public Status GetPinnedElementStatus(Type type)
		{
			var pinned = graph.pinnedElements.Find(p => p.editorType.type == type);

			if (pinned != null && pinned.opened)
				return Status.Normal;
			else
				return Status.Hidden;
		}

		public void ResetPositionAndZoom()
		{
			graph.position = Vector3.zero;
			graph.scale = Vector3.one;

			UpdateViewTransform(graph.position, graph.scale);
		}

		/// <summary>
		/// Deletes the selected content, can be called form an IMGUI container
		/// </summary>
		public void DelayedDeleteSelection() => this.schedule.Execute(() => DeleteSelectionOperation("Delete", AskUser.DontAskUser)).ExecuteLater(0);

		protected virtual void InitializeView() {}

		public virtual IEnumerable< KeyValuePair< string, Type > > FilterCreateNodeMenuEntries()
		{
			// By default we don't filter anything
			foreach (var nodeMenuItem in NodeProvider.GetNodeMenuEntries())
				yield return nodeMenuItem;

			// TODO: add exposed properties to this list
		}

		public RelayNodeView AddRelayNode(PortView inputPort, PortView outputPort, Vector2 position)
		{
			var relayNode = BaseNode.CreateFromType<RelayNode>(position);
			var view = AddNode(relayNode) as RelayNodeView;

			if (outputPort != null)
				Connect(view.inputPortViews[0], outputPort);
			if (inputPort != null)
				Connect(inputPort, view.outputPortViews[0]);

			return view;
		}

		/// <summary>
		/// Call this function when you want to remove this view
		/// </summary>
        public void Dispose()
        {
			ClearGraphElements();
			RemoveFromHierarchy();
			Undo.undoRedoPerformed -= ReloadView;
			Object.DestroyImmediate(nodeInspector);
        }

        #endregion

    }
}