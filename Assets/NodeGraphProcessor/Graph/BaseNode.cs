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

		[NonSerialized]
		public bool					needsProcess = typeof(BaseNode).GetMethod("Process").DeclaringType != typeof(BaseNode);
		[NonSerialized]
		public bool					needsEnable = typeof(BaseNode).GetMethod("Enable").DeclaringType != typeof(BaseNode);

		public virtual void			Enable() {}
		public virtual void			Process() {}
	}
}
