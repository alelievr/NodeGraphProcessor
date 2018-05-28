using UnityEngine;
using System;

namespace GraphProcessor
{
	public sealed class DefaultDataPull : DataPull< object, object >
	{
		public override int priority { get => -10; }

		public override object Pull(object input)
		{
			return input;
		}
	}
}