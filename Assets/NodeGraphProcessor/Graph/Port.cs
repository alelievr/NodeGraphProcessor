using System.Collections.Generic;

namespace GraphProcessor
{
	public class NodePort
	{
		public delegate void PushData();

		public List< SerializableEdge >	edges = new List< SerializableEdge >();
	}
}