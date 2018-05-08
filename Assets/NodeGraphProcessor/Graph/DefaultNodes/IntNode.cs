using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
	public class IntNode : BaseNode
	{
		[Output]
		public int		output;

		public override string name { get { return  "IntNode"; } }

		public override void Enable()
		{
		}
	}
}