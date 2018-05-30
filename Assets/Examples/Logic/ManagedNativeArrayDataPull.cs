using UnityEngine;
using System;
using System.Collections.Generic;
using NativeCollections;
using Unity.Collections;

namespace GraphProcessor
{
	public class ManagedNativeArrayInputPortAdapter< T > : InputPortAdapter< T, ManagedNativeArray< T > > where T : struct
	{
		public override void ReceiveIncomingData(List< SerializableEdge > edges, ref ManagedNativeArray< T > destination)
		{
			Debug.Log("TODO !");
			// destination.CopyFrom(input);
		}
	}

	public class ManagedFloatNativeArrayInputPortAdapter : ManagedNativeArrayInputPortAdapter< float > {}
}