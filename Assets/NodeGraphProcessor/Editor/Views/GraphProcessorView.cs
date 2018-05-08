using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using System.Linq;
using System;

namespace GraphProcessor
{
	public class GraphProcessorView : GraphView
	{
		public GraphProcessorView()
		{
			serializeGraphElements = SerializeGraphElementsImplementation;
			canPasteSerializedData = CanPasteSerializedDataImplementation;
			unserializeAndPaste = UnserializeAndPasteImplementation;
		}
	
		protected override bool canCopySelection
		{
			get { return selection.OfType<Node>().Any(); }
		}

		string SerializeGraphElementsImplementation(IEnumerable<GraphElement> elements)
		{
			return JsonUtility.ToJson("", true);
		}

		bool CanPasteSerializedDataImplementation(string serializedData)
		{
			return true;
		}

		void UnserializeAndPasteImplementation(string operationName, string serializedData)
		{
			Debug.Log("TODO !");
		}

		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			var compatiblePorts = new List<Port>();

			compatiblePorts.AddRange(ports.ToList().Where(p => p.portType.IsAssignableFrom(startPort.portType)));

			return compatiblePorts;
		}

	}
}