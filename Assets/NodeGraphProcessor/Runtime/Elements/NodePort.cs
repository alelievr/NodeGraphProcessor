using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq.Expressions;
using System;

namespace GraphProcessor
{
	public class NodePort
	{
		
		public string				fieldName;
		public BaseNode				owner;
		List< SerializableEdge >	edges = new List< SerializableEdge >();
		Dictionary< SerializableEdge, PushDataDelegate >	pushDataDelegates = new Dictionary< SerializableEdge, PushDataDelegate >();
		List< SerializableEdge >	edgeWithRemoteCustomIO = new List< SerializableEdge >();

		CustomPortIODelegate		customPortIOMethod;
		FieldInfo					ourValueField;

		public delegate void PushDataDelegate();

		public NodePort(BaseNode owner, string fieldName)
		{
			this.fieldName = fieldName;
			this.owner = owner;

			ourValueField = owner.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

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

			//if we have a custom io implementation, we don't need
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
				FieldInfo inputField = edge.inputNode.GetType().GetField(edge.trueInputFieldName, BindingFlags.Public | BindingFlags.Instance);
				FieldInfo outputField = edge.outputNode.GetType().GetField(edge.trueOutputFieldName, BindingFlags.Public | BindingFlags.Instance);

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
			object ourValue = ourValueField.GetValue(owner);
			foreach (var edge in edgeWithRemoteCustomIO)
				edge.passThroughBuffer = ourValue;
		}
		
		public void PullData()
		{
			if (customPortIOMethod != null)
			{
				customPortIOMethod(owner, edges);
			}
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
			var port = this.FirstOrDefault(p => p.fieldName == portFieldName);
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