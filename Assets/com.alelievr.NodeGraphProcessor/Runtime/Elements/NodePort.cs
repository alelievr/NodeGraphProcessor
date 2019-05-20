using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq.Expressions;
using System;

namespace GraphProcessor
{
	// Class that describe port attributes for it's creation
	public class PortData : IEquatable< PortData >
	{
		public string	identifier;
		public string	displayName;
		public Type		displayType;
		// public bool		input; // TODO: Is this useful ?
		public bool		acceptMultipleEdges;

        public bool Equals(PortData other)
        {
			return identifier == other.identifier
				&& displayName == other.displayName
				&& displayType == other.displayType
				&& acceptMultipleEdges == other.acceptMultipleEdges;
        }
    }

	public class NodePort
	{
		public string				fieldName;
		public BaseNode				owner;
		public FieldInfo			fieldInfo;
		public PortData				portData;
		List< SerializableEdge >	edges = new List< SerializableEdge >();
		Dictionary< SerializableEdge, PushDataDelegate >	pushDataDelegates = new Dictionary< SerializableEdge, PushDataDelegate >();
		List< SerializableEdge >	edgeWithRemoteCustomIO = new List< SerializableEdge >();

		CustomPortIODelegate		customPortIOMethod;

		public delegate void PushDataDelegate();

		public NodePort(BaseNode owner, string fieldName, PortData portData)
		{
			this.fieldName = fieldName;
			this.owner = owner;
			this.portData = portData;

			fieldInfo = owner.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			customPortIOMethod = CustomPortIO.GetCustomPortMethod(owner.GetType(), fieldName);
		}

		public void Add(SerializableEdge edge)
		{
			if (!edges.Contains(edge))
				edges.Add(edge);

			if (edge.inputNode == owner)
			{
				if (edge.outputPort.customPortIOMethod != null)
					edgeWithRemoteCustomIO.Add(edge);
			}
			else
			{
				if (edge.inputPort.customPortIOMethod != null)
					edgeWithRemoteCustomIO.Add(edge);
			}

			//if we have a custom io implementation, we don't need to genereate the defaut one
			if (edge.inputPort.customPortIOMethod != null || edge.outputPort.customPortIOMethod != null)
				return ;

			PushDataDelegate edgeDelegate = CreatePushDataDelegateForEdge(edge);

			if (edgeDelegate != null)
				pushDataDelegates[edge] = edgeDelegate;
		}

		PushDataDelegate CreatePushDataDelegateForEdge(SerializableEdge edge)
		{
			try
			{
				//Creation of the delegate to move the data from the input node to the output node:
				FieldInfo inputField = edge.inputNode.GetType().GetField(edge.inputFieldName, BindingFlags.Public | BindingFlags.Instance);
				FieldInfo outputField = edge.outputNode.GetType().GetField(edge.outputFieldName, BindingFlags.Public | BindingFlags.Instance);

// We keep slow checks inside the editor
#if UNITY_EDITOR
				if (!inputField.FieldType.IsReallyAssignableFrom(outputField.FieldType))
				{
					Debug.LogError("Can't convert from " + inputField.FieldType + " to " + outputField.FieldType + ", you must specify a custom port function (i.e CustomPortInput or CustomPortOutput) for non-implicit convertions");
				}
#endif

				MemberExpression inputParamField = Expression.Field(Expression.Constant(edge.inputNode), inputField);
				MemberExpression outputParamField = Expression.Field(Expression.Constant(edge.outputNode), outputField);

				BinaryExpression assign = Expression.Assign(inputParamField, Expression.Convert(outputParamField, inputField.FieldType));
				return Expression.Lambda< PushDataDelegate >(assign).Compile();
			} catch (Exception e) {
				Debug.LogError(e);
				return null;
			}
		}

		public void Remove(SerializableEdge edge)
		{
			pushDataDelegates.Remove(edge);
			edges.Remove(edge);
		}

		public List< SerializableEdge > GetEdges()
		{
			return edges;
		}

		//This method can only be called on output ports
		public void PushData()
		{
			if (customPortIOMethod != null)
			{
				customPortIOMethod(owner, edges);
				return ;
			}

			foreach (var pushDataDelegate in pushDataDelegates)
				pushDataDelegate.Value();

			if (edgeWithRemoteCustomIO.Count == 0)
				return ;

			//if there are custom IO implementation on the other ports, they'll need our value in the passThrough buffer
			object ourValue = fieldInfo.GetValue(owner);
			foreach (var edge in edgeWithRemoteCustomIO)
				edge.passThroughBuffer = ourValue;
		}

		// This method can only be called on input ports
		public void PullData()
		{
			if (customPortIOMethod != null)
			{
				customPortIOMethod(owner, edges);
				return ;
			}

			// check if this port have connection to ports that have custom output functions
			if (edgeWithRemoteCustomIO.Count == 0)
				return ;

			// Only one input connection is handled by this code, if you wany to
			// take multiple inputs, you must create a custom input function see CustomPortsNode.cs
			fieldInfo.SetValue(owner, edges.First().passThroughBuffer);
		}
	}

	public abstract class NodePortContainer : List< NodePort >
	{
		protected BaseNode node;

		public NodePortContainer(BaseNode node)
		{
			this.node = node;
		}

		public void Remove(SerializableEdge edge)
		{
			ForEach(p => p.Remove(edge));
		}

		public void Add(SerializableEdge edge)
		{
			string portFieldName = (edge.inputNode == node) ? edge.inputFieldName : edge.outputFieldName;
			string portIdentifier = (edge.inputNode == node) ? edge.inputPortIdentifier : edge.outputPortIdentifier;

			// Force empty string to null since portIdentifier is a serialized value
			if (String.IsNullOrEmpty(portIdentifier))
				portIdentifier = null;

			var port = this.FirstOrDefault(p =>
			{
				return p.fieldName == portFieldName && p.portData.identifier == portIdentifier;
			});

			if (port == null)
			{
				Debug.LogError("The edge can't be properly connected because it's ports can't be found");
				return;
			}

			port.Add(edge);
		}
	}

	public class NodeInputPortContainer : NodePortContainer
	{
		public NodeInputPortContainer(BaseNode node) : base(node) {}

		public void PullDatas()
		{
			ForEach(p => p.PullData());
		}
	}

	public class NodeOutputPortContainer : NodePortContainer
	{
		public NodeOutputPortContainer(BaseNode node) : base(node) {}

		public void PushDatas()
		{
			ForEach(p => p.PushData());
		}
	}
}