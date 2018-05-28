using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

namespace GraphProcessor
{
	public interface IDataPull
	{
		object GenericPull(object input);
	}

	public interface IMultiDataPull
	{
		void GenericMultiPull(object[] inputArray, object target);
	}

	public abstract class DataPull< InputT, TargetT > : DataIO, IDataPull
	{
		public object GenericPull(object input)
		{
			return (TargetT)Pull((InputT)input);
		}

		public abstract TargetT Pull(InputT input);
	}

	public abstract class MultiDataPull< InputT, TargetT > : DataIO, IMultiDataPull
	{
		public void GenericMultiPull(object[] inputArray, object target)
		{
			MultiPull(inputArray.Select(i => (InputT)i).ToArray(), (TargetT)target);
		}

		public abstract void MultiPull(InputT[] inputArray, TargetT target);
	}
}