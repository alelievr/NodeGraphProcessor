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
		public readonly Dictionary< string, List< SerializableEdge > > inputEdgesPerFields = new Dictionary< string, List< SerializableEdge > >();
		[NonSerialized]
		public readonly List< SerializableEdge > outputEdges = new List< SerializableEdge >();
		[NonSerialized]
		public readonly Dictionary< string, List< SerializableEdge > > outputEdgesPerFields = new Dictionary< string, List< SerializableEdge > >();

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
			var edgePerField = (input) ? inputEdgesPerFields : outputEdgesPerFields;
			var edgeField = (input) ? edge.inputFieldName : edge.outputFieldName;

			if (!edgeCollection.Contains(edge))
				edgeCollection.Add(edge);
			if (!edgePerField.ContainsKey(edgeField))
				edgePerField[edgeField] = new List< SerializableEdge >();
			edgePerField[edgeField].Add(edge);
		}

		public void OnEdgeDisonnected(SerializableEdge edge)
		{
			if (edge == null)
				return ;
				
			bool input = edge.inputNode == this;
			var edgeCollection = (input) ? inputEdges : outputEdges;
			var edgePerField = (input) ? inputEdgesPerFields : outputEdgesPerFields;
			var edgeField = (input) ? edge.inputFieldName : edge.outputFieldName;
			
			edgeCollection.Remove(edge);
			if (edgePerField.ContainsKey(edgeField))
				edgePerField[edgeField].Remove(edge);
		}

		public JobHandle OnSchedule(JobHandle handle)
		{
			JobHandle ret = default(JobHandle);

			PullInputs();
			if (needsSchedule)
				ret = Schedule(handle);
			if (onProcessed != null)
				onProcessed();
			PushOutputs();

			return ret;
		}

		void PullInputs()
		{
			foreach (var edgeKP in inputEdgesPerFields)
			{
				NodeFieldInformation info;
				nodeFields.TryGetValue(edgeKP.Key, out info);
				SerializableEdge firstEdge = edgeKP.Value.First();
				FieldInfo inputField = GetType().GetField(firstEdge.inputFieldName, BindingFlags.Instance | BindingFlags.Public);

				//TODO: optimize this
				if (info.isMultiple)
				{
					var arr = new []{ edgeKP.Value.Select(e => e.passthroughBuffer) };
					var customMultiPull = GraphIO.GetMultiPullMethod(info.info.FieldType);
					customMultiPull.GenericMultiPull(arr, inputField.GetValue(this));
				}
				else
				{
					var customPull = GraphIO.GetPullMethod(info.info.FieldType);
					inputField.SetValue(this, customPull.GenericPull(firstEdge.passthroughBuffer));
				}
			}
		}

		void PushOutputs()
		{
			foreach (var edge in outputEdges)
			{
				NodeFieldInformation info;
				edge.outputNode.nodeFields.TryGetValue(edge.outputFieldName, out info);
				FieldInfo inputField = edge.outputNode.GetType().GetField(edge.outputFieldName, BindingFlags.Instance | BindingFlags.Public);
				var value = inputField.GetValue(edge.outputNode);

				var pushMethod = GraphIO.GetPushMethod(inputField.FieldType);

				edge.passthroughBuffer = pushMethod.GenericPush(value);
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
