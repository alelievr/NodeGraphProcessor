using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
	[System.Serializable]
	public class SerializableEdge : ISerializationCallbackReceiver
	{
		public string	GUID;

		[SerializeField]
		BaseGraph		owner;

		[SerializeField]
		string			inputNodeGUID;
		[SerializeField]
		string			outputNodeGUID;

		[System.NonSerialized]
		public BaseNode	inputNode;

		[System.NonSerialized]
		public NodePort	inputPort;
		[System.NonSerialized]
		public NodePort outputPort;

		//temporary object used to send port to port data when a custom input/output function is used.
		[System.NonSerialized]
		public object	passThroughBuffer;

		[System.NonSerialized]
		public BaseNode	outputNode;

		public string	inputFieldName;
		public string	outputFieldName;

		//Private constructor so we can't instantiate this class
		private SerializableEdge() {}

		public static SerializableEdge CreateNewEdge(BaseGraph graph, BaseNode inputNode, string inputFieldName, BaseNode outputNode, string outputFieldName)
		{
			SerializableEdge	edge = new SerializableEdge();

			edge.owner = graph;
			edge.GUID = System.Guid.NewGuid().ToString();
			edge.inputNode = inputNode;
			edge.inputFieldName = inputFieldName;
			edge.outputNode = outputNode;
			edge.outputFieldName = outputFieldName;
			edge.inputPort = inputNode.GetPort(inputFieldName);
			edge.outputPort = outputNode.GetPort(outputFieldName);

			return edge;
		}

		public void OnBeforeSerialize()
		{
			outputNodeGUID = outputNode.GUID;
			inputNodeGUID = inputNode.GUID;
		}

		public void OnAfterDeserialize() {}

		//here our owner have been deserialized
		public void Deserialize()
		{
			outputNode = owner.nodesPerGUID[outputNodeGUID];
			inputNode = owner.nodesPerGUID[inputNodeGUID];
			inputPort = inputNode.GetPort(inputFieldName);
			outputPort = outputNode.GetPort(outputFieldName);
		}
	}
}
