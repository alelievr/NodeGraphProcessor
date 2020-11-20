﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace GraphProcessor
{
	[System.Serializable]
	public class ExposedParameter
	{
		public string				guid; // unique id to keep track of the parameter
		public string				name;
		public string				type;
		public SerializableObject	serializedValue;
		public bool					input = true;
		public ExposedParameterSettings settings;
		public string shortType => Type.GetType(type)?.Name;
	}
	
	[Serializable]
	public class ExposedParameterSettings
	{
		public bool  isHidden;
	}

	public class GraphChanges
	{
		public SerializableEdge	removedEdge;
		public SerializableEdge	addedEdge;
		public BaseNode			removedNode;
		public BaseNode			addedNode;
		public BaseNode			nodeChanged;
		public Group			addedGroups;
		public Group			removedGroups;
		public BaseStackNode	addedStackNode;
		public BaseStackNode	removedStackNode;
		public StickyNote		addedStickyNotes;
		public StickyNote		removedStickyNotes;
	}

	/// <summary>
	/// Compute order type used to determine the compute order integer on the nodes
	/// </summary>
	public enum ComputeOrderType
	{
		DepthFirst,
		BreadthFirst,
	}

	[System.Serializable]
	public class BaseGraph : ScriptableObject, ISerializationCallbackReceiver
	{
		static readonly int			maxComputeOrderDepth = 1000;
		
		/// <summary>Invalid compute order number of a node when it's inside a loop</summary>
		public static readonly int loopComputeOrder = -2;
		/// <summary>Invalid compute order number of a node can't process</summary>
		public static readonly int invalidComputeOrder = -1;

		/// <summary>
		/// Json list of nodes (Serialized)
		/// </summary>
		/// <typeparam name="JsonElement"></typeparam>
		/// <returns></returns>
		[SerializeField]
		public List< JsonElement >						serializedNodes = new List< JsonElement >();

		/// <summary>
		/// List of all the nodes in the graph.
		/// </summary>
		/// <typeparam name="BaseNode"></typeparam>
		/// <returns></returns>
		[System.NonSerialized]
		public List< BaseNode >							nodes = new List< BaseNode >();

		/// <summary>
		/// Dictionary to access node per GUID, faster than a search in a list
		/// </summary>
		/// <typeparam name="string"></typeparam>
		/// <typeparam name="BaseNode"></typeparam>
		/// <returns></returns>
		[System.NonSerialized]
		public Dictionary< string, BaseNode >			nodesPerGUID = new Dictionary< string, BaseNode >();

		/// <summary>
		/// Json list of edges
		/// </summary>
		/// <typeparam name="SerializableEdge"></typeparam>
		/// <returns></returns>
		[SerializeField]
		public List< SerializableEdge >					edges = new List< SerializableEdge >();
		/// <summary>
		/// Dictionary of edges per GUID, faster than a search in a list
		/// </summary>
		/// <typeparam name="string"></typeparam>
		/// <typeparam name="SerializableEdge"></typeparam>
		/// <returns></returns>
		[System.NonSerialized]
		public Dictionary< string, SerializableEdge >	edgesPerGUID = new Dictionary< string, SerializableEdge >();

		/// <summary>
		/// All groups in the graph
		/// </summary>
		/// <typeparam name="Group"></typeparam>
		/// <returns></returns>
        [SerializeField, FormerlySerializedAs("commentBlocks")]
        public List< Group >                     		groups = new List< Group >();

		/// <summary>
		/// All Stack Nodes in the graph
		/// </summary>
		/// <typeparam name="stackNodes"></typeparam>
		/// <returns></returns>
		[SerializeField, SerializeReference] // Polymorphic serialization
		public List< BaseStackNode >					stackNodes = new List< BaseStackNode >();

		/// <summary>
		/// All pinned elements in the graph
		/// </summary>
		/// <typeparam name="PinnedElement"></typeparam>
		/// <returns></returns>
		[SerializeField]
		public List< PinnedElement >					pinnedElements = new List< PinnedElement >();

		/// <summary>
		/// All exposed parameters in the graph
		/// </summary>
		/// <typeparam name="ExposedParameter"></typeparam>
		/// <returns></returns>
		[SerializeField]
		public List< ExposedParameter >					exposedParameters = new List< ExposedParameter >();

		[SerializeField]
		public List< StickyNote >						stickyNotes = new List<StickyNote>();

		[System.NonSerialized]
		Dictionary< BaseNode, int >						computeOrderDictionary = new Dictionary< BaseNode, int >();

		//graph visual properties
		public Vector3					position = Vector3.zero;
		public Vector3					scale = Vector3.one;

		/// <summary>
		/// Triggered when something is changed in the list of exposed parameters
		/// </summary>
		public event Action				onExposedParameterListChanged;
		public event Action< string >	onExposedParameterModified;
		/// <summary>
		/// Triggered when the graph is enabled
		/// </summary>
		public event Action				onEnabled;

		/// <summary>
		/// Triggered when the graph is changed
		/// </summary>
		public event Action< GraphChanges > onGraphChanges;

		[System.NonSerialized]
		bool _isEnabled = false;
		public bool isEnabled { get => _isEnabled; private set => _isEnabled = value; }
		
		public HashSet< BaseNode >		graphOutputs { get; private set; } = new HashSet<BaseNode>();

        protected virtual void OnEnable()
        {
			Deserialize();

			DestroyBrokenGraphElements();
			UpdateComputeOrder();
			isEnabled = true;
			onEnabled?.Invoke();
        }

		protected virtual void OnDisable()
		{
			foreach (var node in nodes)
				node.DisableInternal();
		}

		/// <summary>
		/// Adds a node to the graph
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public BaseNode AddNode(BaseNode node)
		{
			nodesPerGUID[node.GUID] = node;

			nodes.Add(node);
			node.Initialize(this);

			onGraphChanges?.Invoke(new GraphChanges{ addedNode = node });

			return node;
		}

		/// <summary>
		/// Removes a node from the graph
		/// </summary>
		/// <param name="node"></param>
		public void RemoveNode(BaseNode node)
		{
			node.DestroyInternal();

			nodesPerGUID.Remove(node.GUID);

			nodes.Remove(node);

			onGraphChanges?.Invoke(new GraphChanges{ removedNode = node });
		}

		/// <summary>
		/// Connect two ports with an edge
		/// </summary>
		/// <param name="inputPort">input port</param>
		/// <param name="outputPort">output port</param>
		/// <param name="DisconnectInputs">is the edge allowed to disconnect another edge</param>
		/// <returns>the connecting edge</returns>
		public SerializableEdge Connect(NodePort inputPort, NodePort outputPort, bool autoDisconnectInputs = true)
		{
			var edge = SerializableEdge.CreateNewEdge(this, inputPort, outputPort);
			
			//If the input port does not support multi-connection, we remove them
			if (autoDisconnectInputs && !inputPort.portData.acceptMultipleEdges)
			{
				foreach (var e in inputPort.GetEdges().ToList())
				{
					// TODO: do not disconnect them if the connected port is the same than the old connected
					Disconnect(e);
				}
			}
			// same for the output port:
			if (autoDisconnectInputs && !outputPort.portData.acceptMultipleEdges)
			{
				foreach (var e in outputPort.GetEdges().ToList())
				{
					// TODO: do not disconnect them if the connected port is the same than the old connected
					Disconnect(e);
				}
			}

			edges.Add(edge);
			
			// Add the edge to the list of connected edges in the nodes
			inputPort.owner.OnEdgeConnected(edge);
			outputPort.owner.OnEdgeConnected(edge);

			onGraphChanges?.Invoke(new GraphChanges{ addedEdge = edge });

			return edge;
		}

		/// <summary>
		/// Disconnect two ports
		/// </summary>
		/// <param name="inputNode">input node</param>
		/// <param name="inputFieldName">input field name</param>
		/// <param name="outputNode">output node</param>
		/// <param name="outputFieldName">output field name</param>
		public void Disconnect(BaseNode inputNode, string inputFieldName, BaseNode outputNode, string outputFieldName)
		{
			edges.RemoveAll(r => {
				bool remove = r.inputNode == inputNode
				&& r.outputNode == outputNode
				&& r.outputFieldName == outputFieldName
				&& r.inputFieldName == inputFieldName;

				if (remove)
				{
					r.inputNode?.OnEdgeDisconnected(r);
					r.outputNode?.OnEdgeDisconnected(r);
					onGraphChanges?.Invoke(new GraphChanges{ removedEdge = r });
				}

				return remove;
			});
		}

		/// <summary>
		/// Disconnect an edge
		/// </summary>
		/// <param name="edge"></param>
		public void Disconnect(SerializableEdge edge) => Disconnect(edge.GUID);

		/// <summary>
		/// Disconnect an edge
		/// </summary>
		/// <param name="edgeGUID"></param>
		public void Disconnect(string edgeGUID)
		{
			List<(BaseNode, SerializableEdge)> disconnectEvents = new List<(BaseNode, SerializableEdge)>();

			edges.RemoveAll(r => {
				if (r.GUID == edgeGUID)
				{
					disconnectEvents.Add((r.inputNode, r));
					disconnectEvents.Add((r.outputNode, r));
					onGraphChanges?.Invoke(new GraphChanges{ removedEdge = r });
				}
				return r.GUID == edgeGUID;
			});

			// Delay the edge disconnect event to avoid recursion
			foreach (var (node, edge) in disconnectEvents)
				node?.OnEdgeDisconnected(edge);
		}

		/// <summary>
		/// Add a group
		/// </summary>
		/// <param name="block"></param>
        public void AddGroup(Group block)
        {
            groups.Add(block);
			onGraphChanges?.Invoke(new GraphChanges{ addedGroups = block });
        }

		/// <summary>
		/// Removes a group
		/// </summary>
		/// <param name="block"></param>
        public void RemoveGroup(Group block)
        {
            groups.Remove(block);
			onGraphChanges?.Invoke(new GraphChanges{ removedGroups = block });
        }

		/// <summary>
		/// Add a StackNode
		/// </summary>
		/// <param name="stackNode"></param>
		public void AddStackNode(BaseStackNode stackNode)
		{
			stackNodes.Add(stackNode);
			onGraphChanges?.Invoke(new GraphChanges{ addedStackNode = stackNode });
		}
		
		/// <summary>
		/// Remove a StackNode
		/// </summary>
		/// <param name="stackNode"></param>
		public void RemoveStackNode(BaseStackNode stackNode)
		{
			stackNodes.Remove(stackNode);
			onGraphChanges?.Invoke(new GraphChanges{ removedStackNode = stackNode });
		}

		/// <summary>
		/// Add a sticky note 
		/// </summary>
		/// <param name="note"></param>
        public void AddStickyNote(StickyNote note)
        {
            stickyNotes.Add(note);
			onGraphChanges?.Invoke(new GraphChanges{ addedStickyNotes = note });
        }

		/// <summary>
		/// Removes a sticky note 
		/// </summary>
		/// <param name="note"></param>
        public void RemoveStickyNote(StickyNote note)
        {
            stickyNotes.Remove(note);
			onGraphChanges?.Invoke(new GraphChanges{ removedStickyNotes = note });
        }

		/// <summary>
		/// Invoke the onGraphChanges event, can be used as trigger to execute the graph when the content of a node is changed 
		/// </summary>
		/// <param name="node"></param>
		public void NotifyNodeChanged(BaseNode node) => onGraphChanges?.Invoke(new GraphChanges { nodeChanged = node });

		/// <summary>
		/// Open a pinned element of type viewType
		/// </summary>
		/// <param name="viewType">type of the pinned element</param>
		/// <returns>the pinned element</returns>
		public PinnedElement OpenPinned(Type viewType)
		{
			var pinned = pinnedElements.Find(p => p.editorType.type == viewType);

			if (pinned == null)
			{
				pinned = new PinnedElement(viewType);
				pinnedElements.Add(pinned);
			}
			else
				pinned.opened = true;

			return pinned;
		}

		/// <summary>
		/// Closes a pinned element of type viewType
		/// </summary>
		/// <param name="viewType">type of the pinned element</param>
		public void ClosePinned(Type viewType)
		{
			var pinned = pinnedElements.Find(p => p.editorType.type == viewType);

			pinned.opened = false;
		}

		public void OnBeforeSerialize()
		{
			serializedNodes.Clear();

			foreach (var node in nodes)
				serializedNodes.Add(JsonSerializer.SerializeNode(node));
			
			// Cleanup stackNodes
			stackNodes.RemoveAll(s => s == null);
		}

		// We can deserialize data here because it's called in a unity context
		// so we can load objects references
		public void Deserialize()
		{
			// Disable nodes correctly before removing them:
			if (nodes != null)
			{
				foreach (var node in nodes)
					node.DisableInternal();
			}

			nodes.Clear();

			foreach (var serializedNode in serializedNodes.ToList())
			{
				var node = JsonSerializer.DeserializeNode(serializedNode) as BaseNode;
				if (node == null)
				{
					serializedNodes.Remove(serializedNode);
					continue ;
				}
				AddNode(node);
				nodesPerGUID[node.GUID] = node;
			}

			foreach (var edge in edges.ToList())
			{
				edge.Deserialize();
				edgesPerGUID[edge.GUID] = edge;

				// Sanity check for the edge:
				if (edge.inputPort == null || edge.outputPort == null)
				{
					Disconnect(edge.GUID);
					continue;
				}

				// Add the edge to the non-serialized port data
				edge.inputPort.owner.OnEdgeConnected(edge);
				edge.outputPort.owner.OnEdgeConnected(edge);
			}
		}

		public void OnAfterDeserialize() {}

		/// <summary>
		/// Update the compute order of the nodes in the graph
		/// </summary>
		/// <param name="type">Compute order type</param>
		public void UpdateComputeOrder(ComputeOrderType type = ComputeOrderType.DepthFirst)
		{
			if (nodes.Count == 0)
				return ;

			// Find graph outputs (end nodes) and reset compute order
			graphOutputs.Clear();
			foreach (var node in nodes)
			{
				if (node.GetOutputNodes().Count() == 0)
					graphOutputs.Add(node);
				node.computeOrder = 0;
			}

			computeOrderDictionary.Clear();
			infiniteLoopTracker.Clear();

			switch (type)
			{
				default:
				case ComputeOrderType.DepthFirst:
					UpdateComputeOrderDepthFirst();
					break;
				case ComputeOrderType.BreadthFirst:
					foreach (var node in nodes)
						UpdateComputeOrderBreadthFirst(0, node);
					break;
			}
		}

		/// <summary>
		/// Add an exposed parameter
		/// </summary>
		/// <param name="name">parameter name</param>
		/// <param name="type">parameter type</param>
		/// <param name="value">default value</param>
		/// <returns></returns>
		public string AddExposedParameter(string name, Type type, object value)
		{
			string guid = Guid.NewGuid().ToString(); // Generated once and unique per parameter

			exposedParameters.Add(new ExposedParameter{
				guid = guid,
				name = name,
				type = type.AssemblyQualifiedName,
				settings = new ExposedParameterSettings(),
				serializedValue = new SerializableObject(value, type)
			});

			onExposedParameterListChanged?.Invoke();

			return guid;
		}

		/// <summary>
		/// Remove an exposed parameter
		/// </summary>
		/// <param name="ep">the parameter to remove</param>
		public void RemoveExposedParameter(ExposedParameter ep)
		{
			exposedParameters.Remove(ep);

			onExposedParameterListChanged?.Invoke();
		}

		/// <summary>
		/// Remove an exposed parameter
		/// </summary>
		/// <param name="guid">GUID of the parameter</param>
		public void RemoveExposedParameter(string guid)
		{
			if (exposedParameters.RemoveAll(e => e.guid == guid) != 0)
				onExposedParameterListChanged?.Invoke();
		}

		/// <summary>
		/// Update an exposed parameter value
		/// </summary>
		/// <param name="guid">GUID of the parameter</param>
		/// <param name="value">new value</param>
		public void UpdateExposedParameter(string guid, object value)
		{
			var param = exposedParameters.Find(e => e.guid == guid);
			if (param == null)
				return;

			if (value != null && value.GetType().AssemblyQualifiedName != param.type)
				throw new Exception("Type mismatch when updating parameter " + param.name + ": from " + param.type + " to " + value.GetType().AssemblyQualifiedName);

			param.serializedValue.value = value;
			onExposedParameterModified.Invoke(param.guid);
		}

		/// <summary>
		/// Update the exposed parameter name
		/// </summary>
		/// <param name="parameter">The parameter</param>
		/// <param name="name">new name</param>
		public void UpdateExposedParameterName(ExposedParameter parameter, string name)
		{
			parameter.name = name;
			onExposedParameterModified.Invoke(name);
		}

		/// <summary>
		/// Update parameter visibility
		/// </summary>
		/// <param name="parameter">The parameter</param>
		/// <param name="isHidden">is Hidden</param>
		public void UpdateExposedParameterVisibility(ExposedParameter parameter, bool isHidden)
		{
			parameter.settings.isHidden = isHidden;
			onExposedParameterModified.Invoke(name);
		}

		/// <summary>
		/// Get the exposed parameter from name
		/// </summary>
		/// <param name="name">name</param>
		/// <returns>the parameter or null</returns>
		public ExposedParameter GetExposedParameter(string name)
		{
			return exposedParameters.FirstOrDefault(e => e.name == name);
		}

		/// <summary>
		/// Get exposed parameter from GUID
		/// </summary>
		/// <param name="guid">GUID of the parameter</param>
		/// <returns>The parameter</returns>
		public ExposedParameter GetExposedParameterFromGUID(string guid)
		{
			return exposedParameters.FirstOrDefault(e => e.guid == guid);
		}

		/// <summary>
		/// Set parameter value from name. (Warning: the parameter name can be changed by the user)
		/// </summary>
		/// <param name="name">name of the parameter</param>
		/// <param name="value">new value</param>
		/// <returns>true if the value have been assigned</returns>
		public bool SetParameterValue(string name, object value)
		{
			var e = exposedParameters.FirstOrDefault(p => p.name == name);

			if (e == null)
				return false;

			e.serializedValue.value = value;

			return true;
		}

		/// <summary>
		/// Get the parameter value
		/// </summary>
		/// <param name="name">parameter name</param>
		/// <returns>value</returns>
		public object GetParameterValue(string name) => exposedParameters.FirstOrDefault(p => p.name == name)?.serializedValue?.value;

		/// <summary>
		/// Get the parameter value template
		/// </summary>
		/// <param name="name">parameter name</param>
		/// <typeparam name="T">type of the parameter</typeparam>
		/// <returns>value</returns>
		public T GetParameterValue< T >(string name) => (T)GetParameterValue(name);

		HashSet<BaseNode> infiniteLoopTracker = new HashSet<BaseNode>();
		int UpdateComputeOrderBreadthFirst(int depth, BaseNode node)
		{
			int computeOrder = 0;

			if (depth > maxComputeOrderDepth)
			{
				Debug.LogError("Recursion error while updating compute order");
				return -1;
			}

			if (computeOrderDictionary.ContainsKey(node))
				return node.computeOrder;

			if (!infiniteLoopTracker.Add(node))
				return -1;

			if (!node.canProcess)
			{
				node.computeOrder = -1;
				computeOrderDictionary[node] = -1;
				return -1;
			}

			foreach (var dep in node.GetInputNodes())
			{
				int c = UpdateComputeOrderBreadthFirst(depth + 1, dep);

				if (c == -1)
				{
					computeOrder = -1;
					break ;
				}

				computeOrder += c;
			}

			if (computeOrder != -1)
				computeOrder++;

			node.computeOrder = computeOrder;
			computeOrderDictionary[node] = computeOrder;

			return computeOrder;
		}

		void UpdateComputeOrderDepthFirst()
		{
			Stack<BaseNode> dfs = new Stack<BaseNode>();

			GraphUtils.FindCyclesInGraph(this, (n) => {
				PropagateComputeOrder(n, loopComputeOrder);
			});

			int computeOrder = 0;
			foreach (var node in GraphUtils.DepthFirstSort(this))
			{
				if (node.computeOrder == loopComputeOrder)
					continue;
				if (!node.canProcess)
					node.computeOrder = -1;
				else
					node.computeOrder = computeOrder++;
			}
		}

		void PropagateComputeOrder(BaseNode node, int computeOrder)
		{
			Stack<BaseNode> deps = new Stack<BaseNode>();
			HashSet<BaseNode> loop = new HashSet<BaseNode>();

			deps.Push(node);
			while (deps.Count > 0)
			{
				var n = deps.Pop();
				n.computeOrder = computeOrder;
			
				if (!loop.Add(n))
					continue;

				foreach (var dep in n.GetOutputNodes())
					deps.Push(dep);
			}
		}

		void DestroyBrokenGraphElements()
		{
			edges.RemoveAll(e => e.inputNode == null
				|| e.outputNode == null
				|| string.IsNullOrEmpty(e.outputFieldName)
				|| string.IsNullOrEmpty(e.inputFieldName)
			);
			nodes.RemoveAll(n => n == null);
		}
		
		/// <summary>
		/// Tell if two types can be connected in the context of a graph
		/// </summary>
		/// <param name="t1"></param>
		/// <param name="t2"></param>
		/// <returns></returns>
		public static bool TypesAreConnectable(Type t1, Type t2)
		{
			if (t1 == null || t2 == null)
				return false;

			if (TypeAdapter.AreIncompatible(t1, t2))
				return false;

			//Check if there is custom adapters for this assignation
			if (CustomPortIO.IsAssignable(t1, t2))
				return true;

			//Check for type assignability
			if (t2.IsReallyAssignableFrom(t1))
				return true;

			// User defined type convertions
			if (TypeAdapter.AreAssignable(t1, t2))
				return true;

			return false;
		}
	}
}