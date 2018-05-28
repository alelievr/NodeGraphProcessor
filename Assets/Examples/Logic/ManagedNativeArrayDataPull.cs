using UnityEngine;
using System;
using NativeCollections;
using System.Linq;

namespace GraphProcessor
{
	public sealed class ManagedNativeArrayDataPull< T > : MultiDataPull< T, ManagedNativeArray< T > > where T : struct
	{
		public override void MultiPull(T[] inputArray, ManagedNativeArray<T> target)
		{
			target.CopyFrom(inputArray);
		}
	}
}