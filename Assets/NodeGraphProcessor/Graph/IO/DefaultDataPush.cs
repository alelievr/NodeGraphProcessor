using UnityEngine;
using System;

namespace GraphProcessor
{
	public sealed class DefaultDataPush : DataPush< object, object >
	{
		public override int priority { get => -10; }

		public override object Push(object source)
		{
			return source;
		}
	}
}