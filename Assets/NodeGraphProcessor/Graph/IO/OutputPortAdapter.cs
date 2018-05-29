using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace GraphProcessor
{
	public abstract class OutputPortAdapter< OutputT, SourceT > : PortAdapter
	{
		public override MethodInfo GetAdapterMethod()
		{
			return GetType().GetMethod(nameof(TransfertOutputData), BindingFlags.Public | BindingFlags.Instance);
		}

		public abstract void TransfertOutputData(SourceT source, NodePort port);
	}
}