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

		[NonSerialized]
		public bool					needsProcess = typeof(BaseNode).GetMethod("Process").DeclaringType != typeof(BaseNode);
		[NonSerialized]
		public bool					needsEnable = typeof(BaseNode).GetMethod("Enable").DeclaringType != typeof(BaseNode);

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
	}
}
