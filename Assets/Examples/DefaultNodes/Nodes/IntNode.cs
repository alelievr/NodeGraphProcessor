using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
	[System.Serializable, NodeMenuItem("Primitives/IntNode")]
	public class IntNode : BaseNode
	{
		[Output]
		public int		output;

		public override string name { get { return  "IntNode"; } }
	}
}