using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
		public bool					needsProcess = typeof(BaseNode).GetMethod("Process").DeclaringType != typeof(BaseNode);
		[NonSerialized]
		public bool					needsEnable = typeof(BaseNode).GetMethod("Enable").DeclaringType != typeof(BaseNode);

		[NonSerialized]
		public readonly List< SerializableEdge > inputEdges = new List< SerializableEdge >();
		[NonSerialized]
		public readonly List< SerializableEdge > outputEdges = new List< SerializableEdge >();

		//Node view datas
		public Rect					position;
		public bool					expanded;

		public virtual void			Enable() {}
		public virtual void			Process() {}

		public virtual void			OnNodeCreated()
		{
			GUID = Guid.NewGuid().ToString();
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

		public virtual void OnProcess() {}
	}
}
