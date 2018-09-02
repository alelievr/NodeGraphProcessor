using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Unity.Jobs;
using System.Linq;

namespace GraphProcessor
{
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

		public delegate void		ProcessDelegate();

		public event ProcessDelegate	onProcessed;

		[NonSerialized]
		public Dictionary< string, NodeFieldInformation >	nodeFields = new Dictionary< string, NodeFieldInformation >();

		public class NodeFieldInformation
		{
			public string		name;
			public string		fieldName;
			public FieldInfo	info;
			public bool			input;
			public bool			isMultiple;

			public NodeFieldInformation(FieldInfo info, string name, bool input, bool isMultiple)
			{
				this.input = input;
				this.isMultiple = isMultiple;
				this.info = info;
				this.name = name;
				this.fieldName = info.Name;
			}
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

		protected BaseNode()
		{
			InitializeInOutDatas();

			Enable();

			inputPorts = new NodeInputPortContainer(this);
			outputPorts = new NodeOutputPortContainer(this);

			foreach (var nodeFieldKP in nodeFields)
			{
				AddPort(nodeFieldKP.Value.input, nodeFieldKP.Value.fieldName);
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

				nodeFields[field.Name] = new NodeFieldInformation(field, name, input, isMultiple);
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

		public void AddPort(bool input, string fieldName)
		{
			if (input)
				inputPorts.Add(new NodePort(this, fieldName));
			else
				outputPorts.Add(new NodePort(this, fieldName));
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
