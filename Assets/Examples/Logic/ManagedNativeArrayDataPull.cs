using UnityEngine;
using System;
using NativeCollections;
using Unity.Collections;

namespace GraphProcessor
{
	public class ManagedNativeArrayInputPortAdapter< T > : MultiInputPortAdapter< T, ManagedNativeArray< T > > where T : struct
	{
		public override void ReceiveIncomingData(T input, ref ManagedNativeArray< T > destination, int index)
		{
			Debug.Log("TODO !");
			// destination.CopyFrom(input);
		}
	}

	public class ManagedFloatNativeArrayInputPortAdapter : ManagedNativeArrayInputPortAdapter< float > {}
}