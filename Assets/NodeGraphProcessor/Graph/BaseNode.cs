using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Unity.Jobs;

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
		public bool					needsProcess = typeof(BaseNode).GetMethod("Process", BindingFlags.NonPublic | BindingFlags.Instance).DeclaringType != typeof(BaseNode);
		[NonSerialized]
		public bool					needsEnable = typeof(BaseNode).GetMethod("Enable", BindingFlags.NonPublic | BindingFlags.Instance).DeclaringType != typeof(BaseNode);

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

		public BaseNode()
		{
			InitializeInOutDatas();
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
				isMultiple = typeof(IList).IsAssignableFrom(field.FieldType);
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

			var methodParameters = method.GetParameters();

			if ((Delegate.CreateDelegate(matchingDelegate, method, false) != null))
				Debug.Log("Custom IO method is not matching delegate: " + matchingDelegate);

			return method;
		}

		#endregion

		#region Events and Processing

		public void OnEdgeConnected(SerializableEdge edge)
		{
			if (edge.inputNode == this)
				inputEdges.Add(edge);
			else
				outputEdges.Add(edge);
		}

		public void OnEdgeDisonnected(SerializableEdge edge)
		{
			if (edge == null)
				return ;
			
			if (edge.inputNode == this)
				inputEdges.Remove(edge);
			else
				outputEdges.Remove(edge);
		}

		public void OnProcess()
		{
			PullInputs();

			Process();

			if (onProcessed != null)
				onProcessed();

			PushOutputs();
		}

		void PullInputs()
		{
			foreach (var edge in inputEdges)
			{
				MethodInfo customIOMethod = nodeFields[edge.inputFieldName]?.customIOMethod;

				//TODO: optimize this
				if (customIOMethod != null)
					customIOMethod.Invoke(edge.inputNode, new []{ edge.passthroughBuffer });
				else
				{
					var inputField = edge.inputNode.GetType().GetField(edge.inputFieldName, BindingFlags.Instance | BindingFlags.Public);
					inputField.SetValue(edge.inputNode, edge.passthroughBuffer);
				}
			}
		}

		void PushOutputs()
		{
			foreach (var edge in outputEdges)
			{
				MethodInfo customIOMethod = edge.outputNode.nodeFields[edge.inputFieldName]?.customIOMethod;

				//TODO: optimize this
				if (customIOMethod != null)
					edge.passthroughBuffer = customIOMethod.Invoke(edge.outputNode, new object[]{});
				else
				{
					var outputField = edge.outputNode.GetType().GetField(edge.outputFieldName, BindingFlags.Instance | BindingFlags.Public);
					edge.passthroughBuffer = outputField.GetValue(edge.outputNode);
				}
			}
		}
		
		protected virtual void Enable() {}
		protected virtual void Process() {}

		public virtual JobHandle Schedule(params JobHandle[] dependencies) { return default(JobHandle); }

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
