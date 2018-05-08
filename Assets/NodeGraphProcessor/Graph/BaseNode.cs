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

		public bool					needsProcess = typeof(BaseNode).GetMethod("Process").DeclaringType == typeof(BaseNode);

		public abstract void		Enable();
		public virtual void			Process() {}
	}
}
