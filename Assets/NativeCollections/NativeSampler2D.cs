using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using System.Diagnostics;

namespace NativeCollections
{
	[NativeContainer]
	[NativeContainerSupportsMinMaxWriteRestriction]
	[DebuggerDisplay("Length = {Length}")]
	[DebuggerTypeProxy(typeof(NativeSampler2DDebugView<>))]
	public unsafe struct NativeSampler2D<T> : IDisposable, IEnumerable< T >, IEnumerable where T : struct
	{
		[NativeDisableUnsafePtrRestriction]
		internal void *		buffer;
		internal int		length;

		#if ENABLE_UNITY_COLLECTIONS_CHECKS
			internal int				minIndex;
			internal int				maxIndex;
			internal AtomicSafetyHandle	safety;
			internal DisposeSentinel	disposeSentinel;
		#endif

		internal Allocator	allocatorLabel;

		public NativeSampler2D(int width, int height, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory)
		{
			length = width * height;
			allocatorLabel = allocator;

			long totalSize = (long)UnsafeUtility.SizeOf< T >() * (long)length;

			#if ENABLE_UNITY_COLLECTIONS_CHECKS
				// Native allocation is only valid for Temp, Job and Persistent
				if (allocator <= Allocator.None)
					throw new ArgumentException("Allocator must be Temp, TempJob or Persistent", "allocator");
				if (length < 0)
					throw new ArgumentOutOfRangeException("length", "Length must be >= 0");
				if (!UnsafeUtility.IsBlittable<T>())
					throw new ArgumentException(string.Format("{0} used in NativeCustomArray<{0}> must be blittable", typeof(T)));
			#endif

			buffer = UnsafeUtility.Malloc(totalSize, UnsafeUtility.AlignOf< T >(), allocator);

			if (options == NativeArrayOptions.ClearMemory)
				UnsafeUtility.MemClear(buffer, totalSize);
		
			#if ENABLE_UNITY_COLLECTIONS_CHECKS
				minIndex = 0;
				maxIndex = length - 1;
				DisposeSentinel.Create(out safety, out disposeSentinel, 0);
			#endif
		}

		public int Length { get { return length; } }

		public unsafe T this[int index]
		{
			get
			{
				#if ENABLE_UNITY_COLLECTIONS_CHECKS
					AtomicSafetyHandle.CheckReadAndThrow(safety);
	
					if (index < minIndex || index > maxIndex)
						FailOutOfRangeError(index);
				#endif

				return UnsafeUtility.ReadArrayElement<T>(buffer, index);
			}
			set
			{
				#if ENABLE_UNITY_COLLECTIONS_CHECKS
					AtomicSafetyHandle.CheckWriteAndThrow(safety);
	
					if (index < minIndex || index > maxIndex)
						FailOutOfRangeError(index);
				#endif

				UnsafeUtility.WriteArrayElement(buffer, index, value);
			}
		}

		#if ENABLE_UNITY_COLLECTIONS_CHECKS
			private void FailOutOfRangeError(int index)
			{
				if (index < Length && (minIndex != 0 || maxIndex != Length - 1))
					throw new IndexOutOfRangeException(string.Format(
							"Index {0} is out of restricted IJobParallelFor range [{1}...{2}] in ReadWriteBuffer.\n" +
							"ReadWriteBuffers are restricted to only read & write the element at the job index. " +
							"You can use double buffering strategies to avoid race conditions due to " +
							"reading & writing in parallel to the same elements from a job.",
							index, minIndex, maxIndex));
	
				throw new IndexOutOfRangeException(string.Format("Index {0} is out of range of '{1}' Length.", index, Length));
			}
		#endif

		public bool IsCreated
		{
			get { return buffer != null; }
		}


		public void Dispose()
		{
			#if ENABLE_UNITY_COLLECTIONS_CHECKS
				DisposeSentinel.Dispose(safety, ref disposeSentinel);
			#endif
	
			UnsafeUtility.Free(buffer, allocatorLabel);
			buffer = null;
			length = 0;
		}

		public T[] ToArray()
		{
			#if ENABLE_UNITY_COLLECTIONS_CHECKS
				AtomicSafetyHandle.CheckReadAndThrow(safety);
			#endif
	
			var array = new T[Length];
			for (var i = 0; i < Length; i++)
				array[i] = UnsafeUtility.ReadArrayElement< T >(buffer, i);
			return array;
		}

		public IEnumerator< T > GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}

	internal sealed class NativeSampler2DDebugView< T > where T : struct
	{
		private NativeSampler2D< T >	sampler2D;

		public NativeSampler2DDebugView(NativeSampler2D< T > sampler2D)
		{
			this.sampler2D = sampler2D;
		}

		public T[] Items
		{
			get { return sampler2D.ToArray(); }
		}
	}
}