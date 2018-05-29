using UnityEngine;
using System;

namespace GraphProcessor
{
	public sealed class DefaultInputPortAdapter : InputPortAdapter< object, object >
	{
		public override int priority { get => -10; }

		public override void ReceiveIncomingData(object input, ref object dest)
		{
			dest = input;
		}
	}
}