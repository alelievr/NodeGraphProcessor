using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace GraphProcessor
{
	public class EdgeView : Edge
	{
		public bool					isConnected = false;

		public SerializableEdge		serializedEdge { get { return userData as SerializableEdge; } }

		readonly string				edgeStyle = "GraphProcessorStyles/EdgeView";

		public EdgeView()
		{
			styleSheets.Add(Resources.Load<StyleSheet>(edgeStyle));
		}
	}
}