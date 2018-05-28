using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
	public interface IDataPush
	{
		object GenericPush(object source);
	}

	public abstract class DataPush< OutputT, SourceT > : DataIO, IDataPush
	{
		public object GenericPush(object source)
		{
			return Push((SourceT)source);
		}

		public abstract OutputT Push(SourceT output);
	}
}