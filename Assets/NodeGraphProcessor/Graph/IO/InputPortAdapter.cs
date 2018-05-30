using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using System.Reflection;

namespace GraphProcessor
{
	public abstract class InputPortAdapter< InputT, DestinationT > : PortAdapter
	{
		public abstract void ReceiveIncomingData(List< SerializableEdge > inputEdges, ref DestinationT dest);

		public override MethodInfo GetAdapterMethod()
		{
			return GetType().GetMethod(nameof(ReceiveIncomingData), BindingFlags.Public | BindingFlags.Instance);
		}
	}
}