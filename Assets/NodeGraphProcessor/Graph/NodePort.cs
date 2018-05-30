using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq.Expressions;

namespace GraphProcessor
{
	public class NodePort
	{
		public string				fieldName;
		public BaseNode				owner;
		List< SerializableEdge >	edges = new List< SerializableEdge >();
		Dictionary< SerializableEdge, PushDataDelegate >	pushDataDelegates = new Dictionary< SerializableEdge, PushDataDelegate >();

		public delegate void PushDataDelegate();

		public NodePort(BaseNode owner, string fieldName)
		{
			this.fieldName = fieldName;
			this.owner = owner;
		}

		public void Add(SerializableEdge edge)
		{
			if (!edges.Contains(edge))
				edges.Add(edge);

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

				MemberExpression inputParamField = Expression.Field(Expression.Constant(edge.inputNode), inputField);
				MemberExpression outputParamField = Expression.Field(Expression.Constant(edge.outputNode), outputField);
	
				BinaryExpression assign = Expression.Assign(outputParamField, Expression.Convert(inputParamField, outputField.FieldType));
				return Expression.Lambda< PushDataDelegate >(assign).Compile();
			} catch {
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

		public void PushData()
		{
			foreach (var pushDataDelegate in pushDataDelegates)
				pushDataDelegate.Value();
		}
	}

	public class NodePortContainer : List< NodePort >
	{
		BaseNode node;

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
		
		public void PushData()
		{
			ForEach(p => p.PushData());
		}
	}
}