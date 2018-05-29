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
		public abstract string		name { get; }

		//id
		public string				GUID;

		public int					computeOrder = -1;
		public bool					canProcess = true;

		[NonSerialized]
		public readonly bool		needsSchedule = typeof(BaseNode).GetMethod("Schedule", BindingFlags.NonPublic | BindingFlags.Instance).DeclaringType != typeof(BaseNode);

		[NonSerialized]
		public readonly List< SerializableEdge > inputEdges = new List< SerializableEdge >();
		[NonSerialized]
		public readonly List< SerializableEdge > outputEdges = new List< SerializableEdge >();

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
			public FieldInfo	info;
			public bool			input;
			public bool			isMultiple;

			public NodeFieldInformation(FieldInfo info, string name, bool input, bool isMultiple)
			{
				this.input = input;
				this.isMultiple = isMultiple;
				this.info = info;
				this.name = name;
			}
		}

		public static BaseNode		CreateFromType(Type nodeType, Vector2 position)
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
		}
		
		~BaseNode()
		{
			Disable();
		}


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
				
				//check if field is a collection of list type
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
			var edgeCollection = (input) ? inputEdges : outputEdges;
			var edgeField = (input) ? edge.inputFieldName : edge.outputFieldName;

			if (!edgeCollection.Contains(edge))
				edgeCollection.Add(edge);
		}

		public void OnEdgeDisonnected(SerializableEdge edge)
		{
			if (edge == null)
				return ;
				
			bool input = edge.inputNode == this;
			var edgeCollection = (input) ? inputEdges : outputEdges;
			var edgeField = (input) ? edge.inputFieldName : edge.outputFieldName;
			
			edgeCollection.Remove(edge);
		}

		public JobHandle OnSchedule(JobHandle handle)
		{
			JobHandle ret = default(JobHandle);

			if (needsSchedule)
				ret = Schedule(handle);
			if (onProcessed != null)
				onProcessed();

			PushPortDatas();

			return ret;
		}

		void PushPortDatas()
		{
			foreach (var edge in outputEdges)
			{
			}
		}
		
		protected virtual void Enable() {}
		protected virtual void Disable() {}

		protected virtual JobHandle Schedule(JobHandle dependencies) { return default(JobHandle); }

		#endregion

		#region API and utils

		public IEnumerable< BaseNode > GetInputNodes()
		{
			foreach (var edge in inputEdges)
				yield return edge.outputNode;
		}

		public IEnumerable< BaseNode > GetOutputNodes()
		{
			foreach (var edge in outputEdges)
				yield return edge.inputNode;
		}

		#endregion
	}
}
