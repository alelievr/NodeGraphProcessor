using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace GraphProcessor
{
	[System.Serializable]
	public class BaseGraph : ScriptableObject, ISerializationCallbackReceiver
	{
		static readonly int			maxComputeOrderDepth = 1000;

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

		[SerializeField]
		public List< PinnedElement >					pinnedWindows = new List< PinnedElement >();

		[System.NonSerialized]
		Dictionary< BaseNode, int >						computeOrderDictionary = new Dictionary< BaseNode, int >();

		//graph visual properties
		public Vector3				position = Vector3.zero;
		public Vector3				scale = Vector3.one;

        void OnEnable()
        {
			DestroyBrokenGraphElements();
			UpdateComputeOrder();
        }
		
		public void AddNode(BaseNode node)
		{
			nodes.Add(node);
		}

		public void RemoveNode(BaseNode node)
		{
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

		public PinnedElement OpenPinned(Type viewType)
		{
			var pinned = pinnedWindows.Find(p => p.editorType.type == viewType);

			if (pinned == null)
			{
				pinned = new PinnedElement(viewType);
				pinnedWindows.Add(pinned);
			}
			else
				pinned.opened = true;
			
			return pinned;
		}

		public void ClosePinned(Type viewType)
		{
			var pinned = pinnedWindows.Find(p => p.editorType.type == viewType);

			pinned.opened = false;
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

		public void UpdateComputeOrder()
		{
			if (nodes.Count == 0)
				return ;
			
			computeOrderDictionary.Clear();

			foreach (var node in nodes)
				UpdateComputeOrder(0, node);
		}

		int UpdateComputeOrder(int depth, BaseNode node)
		{
			int computeOrder = 0;

			if (depth > maxComputeOrderDepth)
			{
				Debug.LogError("Recursion error while updating compute order");
				return -1;
			}

			if (computeOrderDictionary.ContainsKey(node))
				return node.computeOrder;

			if (!node.canProcess)
			{
				node.computeOrder = -1;
				computeOrderDictionary[node] = -1;
				return -1;
			}

			foreach (var dep in node.GetInputNodes())
			{
				int c = UpdateComputeOrder(depth + 1, dep);

				if (c == -1)
				{
					computeOrder = -1;
					break ;
				}

				computeOrder += c;
			}

			if (computeOrder != -1)
				computeOrder++;

			node.computeOrder = computeOrder;
			computeOrderDictionary[node] = computeOrder;

			return computeOrder;
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