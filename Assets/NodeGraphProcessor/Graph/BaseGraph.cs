using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GraphProcessor
{
	[System.Serializable]
	public class BaseGraph : ScriptableObject, ISerializationCallbackReceiver
	{
		//JSon datas contaning all elements of the graph
		[SerializeField]
		public List< JsonElement >						serializedNodes = new List< JsonElement >();

		[System.NonSerialized]
		public List< BaseNode >							nodes = new List< BaseNode >();
		[System.NonSerialized]
		public Dictionary< string, BaseNode >			nodesPerGUID = new Dictionary< string, BaseNode >();

		[SerializeField]
		public List< SerializableEdge >					edges = new List< SerializableEdge >();
		[System.NonSerialized]
		public Dictionary< string, SerializableEdge >	edgesPerGUID = new Dictionary< string, SerializableEdge >();

        [SerializeField]
        public List< CommentBlock >                     commentBlocks = new List< CommentBlock >();

		//graph visual properties
		public Vector3				position;
		public Vector3				scale;

        void OnEnable()
        {
			DestroyBrokenGraphElements();
        }
		
		public void AddNode(BaseNode node)
		{
			Debug.Log("Added node: " + node);
			nodes.Add(node);
		}

		public void RemoveNode(BaseNode node)
		{
			Debug.Log("Remove node: " + node);
			nodes.Remove(node);
		}

		public SerializableEdge Connect(BaseNode inputNode, string inputFieldName, BaseNode outputNode, string outputFieldName)
		{
			var edge = SerializableEdge.CreateNewEdge(this, inputNode, inputFieldName, outputNode, outputFieldName);

			edges.Add(edge);

			return edge;
		}

		public void Disconnect(BaseNode inputNode, string inputFieldName, BaseNode outputNode, string outputFieldName)
		{
			edges.RemoveAll(r => r.inputNode == inputNode
				&& r.outputNode == outputNode
				&& r.outputFieldName == outputFieldName
				&& r.inputFieldName == inputFieldName
			);
		}

		public void Disconnect(string edgeGUID)
		{
			edges.RemoveAll(r => r.GUID == edgeGUID);
		}

        public void AddCommentBlock(CommentBlock block)
        {
            commentBlocks.Add(block);
        }

        public void RemoveCommentBlock(CommentBlock block)
        {
            commentBlocks.Remove(block);
        }

		public void OnBeforeSerialize()
		{
			serializedNodes.Clear();
			
			foreach (var node in nodes)
				serializedNodes.Add(JsonSerializer.Serialize(node));
		}

		public void OnAfterDeserialize()
		{
			nodes.Clear();

			Debug.Log("Serialized nodes count: " + serializedNodes.Count);

			foreach (var serializedNode in serializedNodes)
			{
				var node = JsonSerializer.DeserializeNode(serializedNode) as BaseNode;
				nodes.Add(node);
				nodesPerGUID[node.GUID] = node;
			}

			foreach (var edge in edges)
			{
				edge.Deserialize();
				edgesPerGUID[edge.GUID] = edge;
			}
		}

		void DestroyBrokenGraphElements()
		{
			edges.RemoveAll(e => e.inputNode == null
				|| e.outputNode == null
				|| string.IsNullOrEmpty(e.outputFieldName)
				|| string.IsNullOrEmpty(e.inputFieldName)
			);
			nodes.RemoveAll(n => n == null);
		}
	}
}