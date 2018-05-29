using UnityEngine;
using System;

namespace GraphProcessor
{
	public sealed class OutputPortAdapter : OutputPortAdapter< object, object >
	{
		public override int priority { get => -10; }

		public override object TransfertOutputData(object source)
		{
			return source;
		}
	}
}