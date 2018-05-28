using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System;

namespace NativeCollections
{
	public class ManagedNativeArray< T > : IEnumerable< T > where T : struct
	{
		public NativeArray< T >	native;

		public void CopyFrom(T[] array, Allocator newAllocator = Allocator.Persistent)
		{
			if (native.Length != array.Length)
			{
				if (native.IsCreated)
					native.Dispose();
				native = new NativeArray< T >(array.Length, newAllocator);
			}

			native.CopyFrom(array);
		}

		public void CopyTo(T[] array)
		{
			native.CopyTo(array);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return native.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return native.GetEnumerator();
		}

		~ManagedNativeArray()
		{
			//RAII ???
			if (native.IsCreated)
				native.Dispose();
		}
	}
}