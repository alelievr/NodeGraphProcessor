using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GraphProcessor
{
	[System.Serializable]
	public abstract class BaseNode : UnityEngine.Object
	{
		public abstract new string	name { get; }

		public bool					needsProcess = typeof(BaseNode).GetMethod("Process").DeclaringType != typeof(BaseNode);
		public bool					needsEnable = typeof(BaseNode).GetMethod("Enable").DeclaringType != typeof(BaseNode);

		public virtual void			Enable() {}
		public virtual void			Process() {}
	}
}
