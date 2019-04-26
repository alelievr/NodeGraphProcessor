using System.Collections;
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
		public virtual string       name => GetType().Name;

		//id
		public string				GUID;

		public int					computeOrder = -1;
		public bool					canProcess = true;

		[NonSerialized]
		public readonly NodeInputPortContainer	inputPorts;
		[NonSerialized]
		public readonly NodeOutputPortContainer	outputPorts;

		//Node view datas
		public Rect					position;
		public bool					expanded;
		public bool					debug;

		public delegate void		ProcessDelegate();

		public event ProcessDelegate	onProcessed;

		[NonSerialized]
		public Dictionary< string, NodeFieldInformation >	nodeFields = new Dictionary< string, NodeFieldInformation >();

		[NonSerialized]
		protected BaseGraph			graph;

		public class NodeFieldInformation
		{
			public string						name;
			public string						fieldName;
			public FieldInfo					info;
			public bool							input;
			public bool							isMultiple;
			public CustomPortBehaviorDelegate	behavior;

			public NodeFieldInformation(FieldInfo info, string name, bool input, bool isMultiple, CustomPortBehaviorDelegate behavior)
			{
				this.input = input;
				this.isMultiple = isMultiple;
				this.info = info;
				this.name = name;
				this.fieldName = info.Name;
				this.behavior = behavior;
			}
		}

		public static T CreateFromType< T >(Vector2 position) where T : BaseNode
		{
			return CreateFromType(typeof(T), position) as T;
		}

		public static BaseNode CreateFromType(Type nodeType, Vector2 position)
		{
			if (!nodeType.IsSubclassOf(typeof(BaseNode)))
				return null;

			var node = Activator.CreateInstance(nodeType) as BaseNode;

			node.position = new Rect(position, new Vector2(100, 100));

			node.OnNodeCreated();

			return node;
		}

		#region Initialization

		// called by the BaseGraph when the node is added to the graph
		public void Initialize(BaseGraph graph)
		{
			this.graph = graph;

			Enable();
		}

		protected BaseNode()
		{
			InitializeInOutDatas();

			inputPorts = new NodeInputPortContainer(this);
			outputPorts = new NodeOutputPortContainer(this);

			foreach (var nodeFieldKP in nodeFields)
			{
				var nodeField = nodeFieldKP.Value;

				// By default we only create one port, when the node view is initialized,
				// it will create other port views for the processing
				AddPort(nodeField.input, nodeField.fieldName, null);
			}
		}

		~BaseNode()
		{
			Disable();
		}

		/// <summary>
		/// Called only when the node is created, not when instantiated
		/// </summary>
		public virtual void	OnNodeCreated()
		{
			GUID = Guid.NewGuid().ToString();
		}

		void InitializeInOutDatas()
		{
			var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			foreach (var field in fields)
			{
				var inputAttribute = field.GetCustomAttribute< InputAttribute >();
				var outputAttribute = field.GetCustomAttribute< OutputAttribute >();
				bool isMultiple = false;
				bool input = false;
				string name = field.Name;

				if (inputAttribute == null && outputAttribute == null)
					continue ;

				//check if field is a collection type
				isMultiple = (inputAttribute != null) ? inputAttribute.allowMultiple : false;
				input = inputAttribute != null;

				if (!String.IsNullOrEmpty(inputAttribute?.name))
					name = inputAttribute.name;
				if (!String.IsNullOrEmpty(outputAttribute?.name))
					name = outputAttribute.name;

				// By default we set the behavior to null, if the field have a custom behavior, it will be set in the loop just below
				nodeFields[field.Name] = new NodeFieldInformation(field, name, input, isMultiple, null);
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
					Debug.LogError("The function " + method + " can be converted to the required delegate format: " + typeof(CustomPortBehaviorDelegate));
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
		}

		public void OnEdgeDisonnected(SerializableEdge edge)
		{
			if (edge == null)
				return ;

			bool input = edge.inputNode == this;
			NodePortContainer portCollection = (input) ? (NodePortContainer)inputPorts : outputPorts;

			portCollection.Remove(edge);
		}

		public void OnProcess()
		{
			inputPorts.PullDatas();

			Process();

			onProcessed?.Invoke();

			outputPorts.PushDatas();
		}

		protected virtual void Enable() {}
		protected virtual void Disable() {}

		protected virtual void Process() {}

		#endregion

		#region API and utils

		public void AddPort(bool input, string fieldName, string identifier) // TODO: default value
		{
			if (input)
				inputPorts.Add(new NodePort(this, fieldName, identifier));
			else
				outputPorts.Add(new NodePort(this, fieldName, identifier));
		}

		public void RemovePort(bool input, NodePort port)
		{
			if (input)
				inputPorts.Remove(port);
			else
				outputPorts.Remove(port);
		}

		public void RemovePort(bool input, string fieldName)
		{
			if (input)
				inputPorts.RemoveAll(p => p.fieldName == fieldName);
			else
				outputPorts.RemoveAll(p => p.fieldName == fieldName);
		}

		public IEnumerable< BaseNode > GetInputNodes()
		{
			foreach (var port in inputPorts)
				foreach (var edge in port.GetEdges())
					yield return edge.outputNode;
		}

		public IEnumerable< BaseNode > GetOutputNodes()
		{
			foreach (var port in outputPorts)
				foreach (var edge in port.GetEdges())
					yield return edge.inputNode;
		}

		public NodePort	GetPort(string fieldName)
		{
			return inputPorts.Concat(outputPorts).FirstOrDefault(p => p.fieldName == fieldName);
		}

		#endregion
	}
}
