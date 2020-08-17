﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Unity.Jobs;
using System.Linq;

namespace GraphProcessor
{
	public delegate IEnumerable< PortData > CustomPortBehaviorDelegate(List< SerializableEdge > edges);

	[Serializable]
	public abstract class BaseNode
	{
		/// <summary>
		/// Name of the node, it will be displayed in the title section
		/// </summary>
		/// <returns></returns>
		public virtual string       name => GetType().Name;

		/// <summary>
		/// Set a custom uss file for the node. We use a Resources.Load to get the stylesheet so be sure to put the correct resources path
		/// https://docs.unity3d.com/ScriptReference/Resources.Load.html
		/// </summary>
        public virtual string       layoutStyle => string.Empty;

		/// <summary>
		/// If the node can be locked or not
		/// </summary>
        public virtual bool         unlockable => true; 

		/// <summary>
		/// Is the node is locked (if locked it can't be moved)
		/// </summary>
        public virtual bool         isLocked => nodeLock; 

        //id
        public string				GUID;

		public int					computeOrder = -1;

		/// <summary>Tell wether or not the node can be processed. Do not check anything from inputs because this step happens before inputs are sent to the node</summary>
		public virtual bool			canProcess => true;

		/// <summary>Show the node controlContainer only when the mouse is over the node</summary>
		public virtual bool			showControlsOnHover => false;

		/// <summary>
		/// Container of input ports
		/// </summary>
		[NonSerialized]
		public readonly NodeInputPortContainer	inputPorts;
		/// <summary>
		/// Container of output ports
		/// </summary>
		[NonSerialized]
		public readonly NodeOutputPortContainer	outputPorts;

		//Node view datas
		public Rect					position;
		/// <summary>
		/// Is the node expanded
		/// </summary>
		public bool					expanded;
		/// <summary>
		/// Is debug visible
		/// </summary>
		public bool					debug;
		/// <summary>
		/// Node locked state
		/// </summary>
        public bool                 nodeLock;

        public delegate void		ProcessDelegate();

		/// <summary>
		/// Triggered when the node is processes
		/// </summary>
		public event ProcessDelegate	onProcessed;
		public event Action< string, NodeMessageType >	onMessageAdded;
		public event Action< string >					onMessageRemoved;
		/// <summary>
		/// Triggered after an edge was connected on the node
		/// </summary>
		public event Action< SerializableEdge >			onAfterEdgeConnected;
		/// <summary>
		/// Triggered after an edge was disconnected on the node
		/// </summary>
		public event Action< SerializableEdge >			onAfterEdgeDisconnected;

		/// <summary>
		/// Triggered after a single/list of port(s) is updated, the parameter is the field name
		/// </summary>
		public event Action< string >					onPortsUpdated;

		[NonSerialized]
		bool _needsInspector = false;

		/// <summary>
		/// Does the node needs to be visible in the inspector (when selected).
		/// </summary>
		public virtual bool			needsInspector => _needsInspector;

		[NonSerialized]
		Dictionary< string, NodeFieldInformation >	nodeFields = new Dictionary< string, NodeFieldInformation >();

		[NonSerialized]
		List< string >				messages = new List< string >();

		[NonSerialized]
		protected BaseGraph			graph;

		class NodeFieldInformation
		{
			public string						name;
			public string						fieldName;
			public FieldInfo					info;
			public bool							input;
			public bool							isMultiple;
			public string						tooltip;
			public CustomPortBehaviorDelegate	behavior;

			public NodeFieldInformation(FieldInfo info, string name, bool input, bool isMultiple, string tooltip, CustomPortBehaviorDelegate behavior)
			{
				this.input = input;
				this.isMultiple = isMultiple;
				this.info = info;
				this.name = name;
				this.fieldName = info.Name;
				this.behavior = behavior;
				this.tooltip = tooltip;
			}
		}

		struct PortUpdate
		{
			public List<string>	fieldNames;
			public BaseNode		node;

			public void Deconstruct(out List<string> fieldNames, out BaseNode node)
			{
				fieldNames = this.fieldNames;
				node = this.node;
			}
		}

		// Used in port update algorithm
		Stack<PortUpdate> fieldsToUpdate = new Stack<PortUpdate>();
		HashSet<PortUpdate> updatedFields = new HashSet<PortUpdate>();

		/// <summary>
		/// Creates a node of type T at a certain position
		/// </summary>
		/// <param name="position">position in the graph in pixels</param>
		/// <typeparam name="T">type of the node</typeparam>
		/// <returns>the node instance</returns>
		public static T CreateFromType< T >(Vector2 position) where T : BaseNode
		{
			return CreateFromType(typeof(T), position) as T;
		}

		/// <summary>
		/// Creates a node of type nodeType at a certain position
		/// </summary>
		/// <param name="position">position in the graph in pixels</param>
		/// <typeparam name="nodeType">type of the node</typeparam>
		/// <returns>the node instance</returns>
		public static BaseNode CreateFromType(Type nodeType, Vector2 position)
		{
			if (!nodeType.IsSubclassOf(typeof(BaseNode)))
				return null;

			var node = Activator.CreateInstance(nodeType) as BaseNode;

			node.position = new Rect(position, new Vector2(100, 100));

			ExceptionToLog.Call(() => node.OnNodeCreated());

			return node;
		}

		#region Initialization

		// called by the BaseGraph when the node is added to the graph
		public void Initialize(BaseGraph graph)
		{
			this.graph = graph;

			ExceptionToLog.Call(() => Enable());

			InitializePorts();
		}

		internal void InitializePorts()
		{
			foreach (var nodeFieldKP in nodeFields.ToList().OrderByDescending(kp => kp.Value.info.MetadataToken))
			{
				var nodeField = nodeFieldKP.Value;

				if (nodeField.behavior != null)
				{
					UpdatePortsForField(nodeField.fieldName);
				}
				else
				{
					// If we don't have a custom behavior on the node, we just have to create a simple port
					AddPort(nodeField.input, nodeField.fieldName, new PortData { acceptMultipleEdges = nodeField.isMultiple, displayName = nodeField.name, tooltip = nodeField.tooltip });
				}
			}
		}

		protected BaseNode()
		{
			inputPorts = new NodeInputPortContainer(this);
			outputPorts = new NodeOutputPortContainer(this);

			InitializeInOutDatas();
		}

		/// <summary>
		/// Update all ports of the node
		/// </summary>
		public bool UpdateAllPorts()
		{
			bool changed = false;

			foreach (var field in nodeFields)
				changed |= UpdatePortsForField(field.Value.fieldName);

			return changed;
		}

		/// <summary>
		/// Update the ports related to one C# property field (only for this node)
		/// </summary>
		/// <param name="fieldName"></param>
		public bool UpdatePortsForFieldLocal(string fieldName)
		{
			bool changed = false;

			if (!nodeFields.ContainsKey(fieldName))
				return false;

			var fieldInfo = nodeFields[fieldName];

			if (fieldInfo.behavior == null)
				return false;

			List< string > finalPorts = new List< string >();

			var portCollection = fieldInfo.input ? (NodePortContainer)inputPorts : outputPorts;

			// Gather all fields for this port (before to modify them)
			var nodePorts = portCollection.Where(p => p.fieldName == fieldName);
			// Gather all edges connected to these fields:
			var edges = nodePorts.SelectMany(n => n.GetEdges()).ToList();

			foreach (var portData in fieldInfo.behavior(edges))
			{
				var port = nodePorts.FirstOrDefault(n => n.portData.identifier == portData.identifier);
				// Guard using the port identifier so we don't duplicate identifiers
				if (port == null)
				{
					AddPort(fieldInfo.input, fieldName, portData);
					changed = true;
				}
				else
				{
					// in case the port type have changed for an incompatible type, we disconnect all the edges attached to this port
					if (!BaseGraph.TypesAreConnectable(port.portData.displayType, portData.displayType))
					{
						foreach (var edge in port.GetEdges().ToList())
							graph.Disconnect(edge.GUID);
					}

					// patch the port data
					if (port.portData != portData)
					{
						port.portData.CopyFrom(portData);
						changed = true;
					}
				}

				finalPorts.Add(portData.identifier);
			}

			// TODO
			// Remove only the ports that are no more in the list
			if (nodePorts != null)
			{
				var currentPortsCopy = nodePorts.ToList();
				foreach (var currentPort in currentPortsCopy)
				{
					// If the current port does not appear in the list of final ports, we remove it
					if (!finalPorts.Any(id => id == currentPort.portData.identifier))
					{
						RemovePort(fieldInfo.input, currentPort);
						changed = true;
					}
				}
			}

			// Make sure the port order is correct:
			portCollection.Sort((p1, p2) => {
				int p1Index = finalPorts.FindIndex(id => p1.portData.identifier == id);
				int p2Index = finalPorts.FindIndex(id => p2.portData.identifier == id);

				if (p1Index == -1 || p2Index == -1)
					return 0;

				return p1Index.CompareTo(p2Index);
			});

			onPortsUpdated?.Invoke(fieldName);

			return changed;
		}

		/// <summary>
		/// Update the ports related to one C# property field and all connected nodes in the graph
		/// </summary>
		/// <param name="fieldName"></param>
		public bool UpdatePortsForField(string fieldName)
		{
			bool changed  = false;

			fieldsToUpdate.Clear();
			updatedFields.Clear();

			fieldsToUpdate.Push(new PortUpdate{fieldNames = new List<string>(){fieldName}, node = this});

			// Iterate through all the ports that needs to be updated, following graph connection when the 
			// port is updated. This is required ton have type propagation multiple nodes that changes port types
			// are connected to each other (i.e. the relay node)
			while (fieldsToUpdate.Count != 0)
			{
				var (fields, node) = fieldsToUpdate.Pop();

				// Avoid updating twice a port
				if (updatedFields.Any((t) => t.node == node && fields.SequenceEqual(t.fieldNames)))
					continue;
				updatedFields.Add(new PortUpdate{fieldNames = fields, node = node});

				foreach (var field in fields)
				{
					if (node.UpdatePortsForFieldLocal(field))
					{
						foreach (var port in node.IsFieldInput(field) ? (NodePortContainer)node.inputPorts : node.outputPorts)
						{
							if (port.fieldName != field)
								continue;

							foreach(var edge in port.GetEdges())
							{
								var edgeNode = (node.IsFieldInput(field)) ? edge.outputNode : edge.inputNode;
								var fieldsWithBehavior = edgeNode.nodeFields.Values.Where(f => f.behavior != null).Select(f => f.fieldName).ToList();
								fieldsToUpdate.Push(new PortUpdate{fieldNames = fieldsWithBehavior, node = edgeNode});
							}
						}
						changed = true;
					}
				}
			}

			return changed;
		}

		HashSet<BaseNode> portUpdateHashSet = new HashSet<BaseNode>();

		internal void DisableInternal() => ExceptionToLog.Call(() => Disable());
		internal void DestroyInternal() => ExceptionToLog.Call(() => Destroy());

		/// <summary>
		/// Called only when the node is created, not when instantiated
		/// </summary>
		public virtual void	OnNodeCreated() => GUID = Guid.NewGuid().ToString();

		public virtual FieldInfo[] GetNodeFields()
			=> GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

		void InitializeInOutDatas()
		{
			var fields = GetNodeFields();
			var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (var field in fields)
			{
				var inputAttribute = field.GetCustomAttribute< InputAttribute >();
				var outputAttribute = field.GetCustomAttribute< OutputAttribute >();
				var tooltipAttribute = field.GetCustomAttribute< TooltipAttribute >();
				var showInInspector = field.GetCustomAttribute< ShowInInspector >();
				bool isMultiple = false;
				bool input = false;
				string name = field.Name;
				string tooltip = null;

				if (showInInspector != null)
					_needsInspector = true;

				if (inputAttribute == null && outputAttribute == null)
					continue ;

				//check if field is a collection type
				isMultiple = (inputAttribute != null) ? inputAttribute.allowMultiple : (outputAttribute.allowMultiple);
				input = inputAttribute != null;
				tooltip = tooltipAttribute?.tooltip;

				if (!String.IsNullOrEmpty(inputAttribute?.name))
					name = inputAttribute.name;
				if (!String.IsNullOrEmpty(outputAttribute?.name))
					name = outputAttribute.name;

				// By default we set the behavior to null, if the field have a custom behavior, it will be set in the loop just below
				nodeFields[field.Name] = new NodeFieldInformation(field, name, input, isMultiple, tooltip, null);
			}

			foreach (var method in methods)
			{
				var customPortBehaviorAttribute = method.GetCustomAttribute< CustomPortBehaviorAttribute >();
				CustomPortBehaviorDelegate behavior = null;

				if (customPortBehaviorAttribute == null)
					continue ;

				// Check if custom port behavior function is valid
				try {
					var referenceType = typeof(CustomPortBehaviorDelegate);
					behavior = (CustomPortBehaviorDelegate)Delegate.CreateDelegate(referenceType, this, method, true);
				} catch {
					Debug.LogError("The function " + method + " cannot be converted to the required delegate format: " + typeof(CustomPortBehaviorDelegate));
				}

				if (nodeFields.ContainsKey(customPortBehaviorAttribute.fieldName))
					nodeFields[customPortBehaviorAttribute.fieldName].behavior = behavior;
				else
					Debug.LogError("Invalid field name for custom port behavior: " + method + ", " + customPortBehaviorAttribute.fieldName);
			}
		}

		#endregion

		#region Events and Processing

		public void OnEdgeConnected(SerializableEdge edge)
		{
			bool input = edge.inputNode == this;
			NodePortContainer portCollection = (input) ? (NodePortContainer)inputPorts : outputPorts;

			portCollection.Add(edge);

			UpdateAllPorts();

			onAfterEdgeConnected?.Invoke(edge);
		}

		public void OnEdgeDisconnected(SerializableEdge edge)
		{
			if (edge == null)
				return ;

			bool input = edge.inputNode == this;
			NodePortContainer portCollection = (input) ? (NodePortContainer)inputPorts : outputPorts;

			portCollection.Remove(edge);

			// Reset default values of input port:
			bool haveConnectedEdges = edge.inputNode.inputPorts.Where(p => p.fieldName == edge.inputFieldName).Any(p => p.GetEdges().Count != 0);
			if (edge.inputNode == this && !haveConnectedEdges)
				edge.inputPort?.ResetToDefault();

			UpdateAllPorts();

			onAfterEdgeDisconnected?.Invoke(edge);
		}

		public void OnProcess()
		{
			inputPorts.PullDatas();

			ExceptionToLog.Call(() => Process());

			InvokeOnProcessed();

			outputPorts.PushDatas();
		}

		public void InvokeOnProcessed() => onProcessed?.Invoke();

		/// <summary>
		/// Called when the node is enabled
		/// </summary>
		protected virtual void Enable() {}
		/// <summary>
		/// Called when the node is disabled
		/// </summary>
		protected virtual void Disable() {}
		/// <summary>
		/// Called when the node is removed
		/// </summary>
		protected virtual void Destroy() {}

		/// <summary>
		/// Override this method to implement custom processing
		/// </summary>
		protected virtual void Process() {}

		#endregion

		#region API and utils

		/// <summary>
		/// Add a port
		/// </summary>
		/// <param name="input">is input port</param>
		/// <param name="fieldName">C# field name</param>
		/// <param name="portData">Data of the port</param>
		public void AddPort(bool input, string fieldName, PortData portData)
		{
			// Fixup port data info if needed:
			if (portData.displayType == null)
				portData.displayType = nodeFields[fieldName].info.FieldType;

			if (input)
				inputPorts.Add(new NodePort(this, fieldName, portData));
			else
				outputPorts.Add(new NodePort(this, fieldName, portData));
		}

		/// <summary>
		/// Remove a port
		/// </summary>
		/// <param name="input">is input port</param>
		/// <param name="port">the port to delete</param>
		public void RemovePort(bool input, NodePort port)
		{
			if (input)
				inputPorts.Remove(port);
			else
				outputPorts.Remove(port);
		}

		/// <summary>
		/// Remove port(s) from field name
		/// </summary>
		/// <param name="input">is input</param>
		/// <param name="fieldName">C# field name</param>
		public void RemovePort(bool input, string fieldName)
		{
			if (input)
				inputPorts.RemoveAll(p => p.fieldName == fieldName);
			else
				outputPorts.RemoveAll(p => p.fieldName == fieldName);
		}

		/// <summary>
		/// Get all the nodes connected to the input ports of this node
		/// </summary>
		/// <returns>an enumerable of node</returns>
		public IEnumerable< BaseNode > GetInputNodes()
		{
			foreach (var port in inputPorts)
				foreach (var edge in port.GetEdges())
					yield return edge.outputNode;
		}

		/// <summary>
		/// Get all the nodes connected to the output ports of this node
		/// </summary>
		/// <returns>an enumerable of node</returns>
		public IEnumerable< BaseNode > GetOutputNodes()
		{
			foreach (var port in outputPorts)
				foreach (var edge in port.GetEdges())
					yield return edge.inputNode;
		}

		/// <summary>
		/// Return a node matching the condition in the dependencies of the node
		/// </summary>
		/// <param name="condition">Condition to choose the node</param>
		/// <returns>Matched node or null</returns>
		public BaseNode FindInDependencies(Func<BaseNode, bool> condition)
		{
			Stack<BaseNode> dependencies = new Stack<BaseNode>();

			dependencies.Push(this);

			int depth = 0;
			while (dependencies.Count > 0)
			{
				var node = dependencies.Pop();

				// Guard for infinite loop (faster than a HashSet based solution)
				depth++;
				if (depth > 2000)
					break;

				if (condition(node))
					return node;
				
				foreach (var dep in node.GetInputNodes())
					dependencies.Push(dep);
			}
			return null;
		}

		/// <summary>
		/// Get the port from field name and identifier
		/// </summary>
		/// <param name="fieldName">C# field name</param>
		/// <param name="identifier">Unique port identifier</param>
		/// <returns></returns>
		public NodePort	GetPort(string fieldName, string identifier)
		{
			return inputPorts.Concat(outputPorts).FirstOrDefault(p => {
				var bothNull = String.IsNullOrEmpty(identifier) && String.IsNullOrEmpty(p.portData.identifier);
				return p.fieldName == fieldName && (bothNull || identifier == p.portData.identifier);
			});
		}

		/// <summary>
		/// Is the port an input
		/// </summary>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public bool IsFieldInput(string fieldName) => nodeFields[fieldName].input;

		/// <summary>
		/// Add a message on the node
		/// </summary>
		/// <param name="message"></param>
		/// <param name="messageType"></param>
		public void AddMessage(string message, NodeMessageType messageType)
		{
			if (messages.Contains(message))
				return;

			onMessageAdded?.Invoke(message, messageType);
			messages.Add(message);
		}

		/// <summary>
		/// Remove a message on the node
		/// </summary>
		/// <param name="message"></param>
		public void RemoveMessage(string message)
		{
			onMessageRemoved?.Invoke(message);
			messages.Remove(message);
		}

		/// <summary>
		/// Remove a message that contains
		/// </summary>
		/// <param name="subMessage"></param>
		public void RemoveMessageContains(string subMessage)
		{
			string toRemove = messages.Find(m => m.Contains(subMessage));
			messages.Remove(toRemove);
			onMessageRemoved?.Invoke(toRemove);
		}

		/// <summary>
		/// Remove all messages on the node
		/// </summary>
		public void ClearMessages()
		{
			foreach (var message in messages)
				onMessageRemoved?.Invoke(message);
			messages.Clear();
		}

		#endregion
	}
}
