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
			public MethodInfo	customIOMethod;
			public bool			input;
			public bool			isMultiple;

			public NodeFieldInformation(FieldInfo info, string name, MethodInfo customIOMethod, bool input, bool isMultiple)
			{
				this.customIOMethod = customIOMethod;
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
				isMultiple = typeof(IEnumerable).IsAssignableFrom(field.FieldType);
				input = inputAttribute != null;
			
				if (!String.IsNullOrEmpty(inputAttribute?.name))
					name = inputAttribute.name;
				if (!String.IsNullOrEmpty(outputAttribute?.name))
					name = outputAttribute.name;
				
				string customIOMethod = inputAttribute?.customPullerName;

				if (customIOMethod == null)
					customIOMethod = outputAttribute?.customPusherName;
				
				MethodInfo ioMethod = null;

				if (!String.IsNullOrEmpty(customIOMethod))
					ioMethod = GetFieldIOMethod(customIOMethod, input, isMultiple);

				nodeFields[field.Name] = new NodeFieldInformation(field, name, ioMethod, input, isMultiple);
			}
		}

		MethodInfo GetFieldIOMethod(string methodName, bool input, bool isMultiple)
		{
			MethodInfo method = GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
			Type matchingDelegate = null;

			if (method == null)
			{
				Debug.LogError("Can't find the method " + methodName + " in the node " + GetType() + ", Make sure that the method is non-static");
				return null;
			}

			if (input)
			{
				if (isMultiple)
					matchingDelegate = typeof(MultiInputPullerDelegate);
				else
					matchingDelegate = typeof(InputPullerDelegate);
			}
			else
				matchingDelegate = typeof(OutputPusherDelegate);

			if ((Delegate.CreateDelegate(matchingDelegate, method, false) != null))
				Debug.Log("Custom IO method is not matching delegate: " + matchingDelegate);

			return method;
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
				MethodInfo customPullMethod = info?.customIOMethod;
				SerializableEdge firstEdge = edgeKP.Value.First();

				//TODO: optimize this
				if (customPullMethod != null)
					customPullMethod.Invoke(firstEdge.inputNode, new []{ edgeKP.Value.Select(e => e.passthroughBuffer) });
				else
				{
					var inputField = firstEdge.inputNode.GetType().GetField(firstEdge.inputFieldName, BindingFlags.Instance | BindingFlags.Public);
					inputField.SetValue(firstEdge.inputNode, firstEdge.passthroughBuffer);
				}
			}
		}

		void PushOutputs()
		{
			foreach (var edge in outputEdges)
			{
				NodeFieldInformation info;
				edge.outputNode.nodeFields.TryGetValue(edge.outputFieldName, out info);
				MethodInfo customPushMethod = info?.customIOMethod;

				//TODO: optimize this
				if (customPushMethod != null)
				{
					Debug.Log("using custom push method: " + customPushMethod);
					edge.passthroughBuffer = customPushMethod.Invoke(edge.outputNode, new object[]{});
				}
				else
				{
					var outputField = edge.outputNode.GetType().GetField(edge.outputFieldName, BindingFlags.Instance | BindingFlags.Public);
					edge.passthroughBuffer = outputField.GetValue(edge.outputNode);
				}
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
