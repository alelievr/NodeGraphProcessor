using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;

namespace GraphProcessor
{
	public class EdgeView : Edge
	{
		public bool					isConnected = false;
		
		public SerializableEdge		serializedEdge { get { return userData as SerializableEdge; } }
	
		public EdgeView()
		{
			AddStyleSheetPath("GraphProcessorStyles/EdgeView");
		}
	}
}