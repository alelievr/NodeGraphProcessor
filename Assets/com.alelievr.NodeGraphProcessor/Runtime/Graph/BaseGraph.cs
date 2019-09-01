using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace GraphProcessor
{
	[System.Serializable]
	public class ExposedParameter
	{
		public string				guid; // unique id to keep track of the parameter
		public string				name;
		public string				type;
		public SerializableObject	serializedValue;
		public bool					input = true;
	}

	public class GraphChanges
	{
		public SerializableEdge	removedEdge;
		public SerializableEdge	addedEdge;
		public BaseNode			removedNode;
		public BaseNode			addedNode;
		public CommentBlock		addedCommentBlock;
		public CommentBlock		removedCommentBlock;
	}

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
		public List< PinnedElement >					pinnedElements = new List< PinnedElement >();

		[SerializeField]
		public List< ExposedParameter >					exposedParameters = new List< ExposedParameter >();

		[System.NonSerialized]
		Dictionary< BaseNode, int >						computeOrderDictionary = new Dictionary< BaseNode, int >();

		//graph visual properties
		public Vector3					position = Vector3.zero;
		public Vector3					scale = Vector3.one;

		public event Action				onExposedParameterListChanged;
		public event Action< string >	onExposedParameterModified;
		public event Action				onEnabled;

		public event Action< GraphChanges > onGraphChanges;

		[System.NonSerialized]
		bool _isEnabled = false;
		public bool isEnabled { get => _isEnabled; private set => _isEnabled = value; }

        protected virtual void OnEnable()
        {
			Deserialize();

			DestroyBrokenGraphElements();
			UpdateComputeOrder();
			isEnabled = true;
			onEnabled?.Invoke();
        }

		public BaseNode AddNode(BaseNode node)
		{
			nodes.Add(node);
			node.Initialize(this);

			onGraphChanges?.Invoke(new GraphChanges{ addedNode = node });

			return node;
		}

		public void RemoveNode(BaseNode node)
		{
			nodes.Remove(node);

			onGraphChanges?.Invoke(new GraphChanges{ removedNode = node });
		}

		public SerializableEdge Connect(NodePort inputPort, NodePort outputPort, bool autoDisconnectInputs = true)
		{
			var edge = SerializableEdge.CreateNewEdge(this, inputPort, outputPort);
			
			//If the input port does not support multi-connection, we remove them
			if (autoDisconnectInputs && !inputPort.portData.acceptMultipleEdges)
			{
				foreach (var e in inputPort.GetEdges().ToList())
				{
					// TODO: do not disconnect them if the connected port is the same than the old connected
					Disconnect(e);
				}
			}
			// same for the output port:
			if (autoDisconnectInputs && !outputPort.portData.acceptMultipleEdges)
			{
				foreach (var e in outputPort.GetEdges().ToList())
				{
					// TODO: do not disconnect them if the connected port is the same than the old connected
					Disconnect(e);
				}
			}

			edges.Add(edge);

			onGraphChanges?.Invoke(new GraphChanges{ addedEdge = edge });
			
			// Add the edge to the list of connected edges in the nodes
			inputPort.owner.OnEdgeConnected(edge);
			outputPort.owner.OnEdgeConnected(edge);

			return edge;
		}

		public void Disconnect(BaseNode inputNode, string inputFieldName, BaseNode outputNode, string outputFieldName)
		{
			edges.RemoveAll(r => {
				bool remove = r.inputNode == inputNode
				&& r.outputNode == outputNode
				&& r.outputFieldName == outputFieldName
				&& r.inputFieldName == inputFieldName;

				if (remove)
					onGraphChanges?.Invoke(new GraphChanges{ removedEdge = r });

				return remove;
			});
		}

		public void Disconnect(SerializableEdge edge) => Disconnect(edge.GUID);

		public void Disconnect(string edgeGUID)
		{
			edges.RemoveAll(r => {
				if (r.GUID == edgeGUID)
				{
					onGraphChanges?.Invoke(new GraphChanges{ removedEdge = r });
					r.inputNode?.OnEdgeDisconnected(r);
				}
				return r.GUID == edgeGUID;
			});
		}

        public void AddCommentBlock(CommentBlock block)
        {
            commentBlocks.Add(block);
			onGraphChanges?.Invoke(new GraphChanges{ addedCommentBlock = block });
        }

        public void RemoveCommentBlock(CommentBlock block)
        {
            commentBlocks.Remove(block);
			onGraphChanges?.Invoke(new GraphChanges{ removedCommentBlock = block });
        }

		public PinnedElement OpenPinned(Type viewType)
		{
			var pinned = pinnedElements.Find(p => p.editorType.type == viewType);

			if (pinned == null)
			{
				pinned = new PinnedElement(viewType);
				pinnedElements.Add(pinned);
			}
			else
				pinned.opened = true;

			return pinned;
		}

		public void ClosePinned(Type viewType)
		{
			var pinned = pinnedElements.Find(p => p.editorType.type == viewType);

			pinned.opened = false;
		}

		public void OnBeforeSerialize()
		{
			serializedNodes.Clear();

			foreach (var node in nodes)
				serializedNodes.Add(JsonSerializer.SerializeNode(node));
		}

		// We can deserialize data here because it's called in a unity context
		// so we can load objects references
		public void Deserialize()
		{
			nodes.Clear();

			foreach (var serializedNode in serializedNodes.ToList())
			{
				var node = JsonSerializer.DeserializeNode(serializedNode) as BaseNode;
				if (node == null)
				{
					serializedNodes.Remove(serializedNode);
					continue ;
				}
				AddNode(node);
				nodesPerGUID[node.GUID] = node;
			}

			foreach (var edge in edges.ToList())
			{
				edge.Deserialize();
				edgesPerGUID[edge.GUID] = edge;

				// Sanity check for the edge:
				if (edge.inputPort == null || edge.outputPort == null)
				{
					Disconnect(edge.GUID);
					continue;
				}

				// Add the edge to the non-serialized port data
				edge.inputPort.owner.OnEdgeConnected(edge);
				edge.outputPort.owner.OnEdgeConnected(edge);
			}
		}

		public void OnAfterDeserialize() {}

		public void UpdateComputeOrder()
		{
			if (nodes.Count == 0)
				return ;

			computeOrderDictionary.Clear();

			foreach (var node in nodes)
				UpdateComputeOrder(0, node);
		}

		public string AddExposedParameter(string name, Type type, object value)
		{
			string guid = Guid.NewGuid().ToString(); // Generated once and unique per parameter

			exposedParameters.Add(new ExposedParameter{
				guid = guid,
				name = name,
				type = type.AssemblyQualifiedName,
				serializedValue = new SerializableObject(value)
			});

			onExposedParameterListChanged?.Invoke();

			return guid;
		}

		public void RemoveExposedParameter(ExposedParameter ep)
		{
			exposedParameters.Remove(ep);

			onExposedParameterListChanged?.Invoke();
		}

		public void RemoveExposedParameter(string guid)
		{
			if (exposedParameters.RemoveAll(e => e.guid == guid) != 0)
				onExposedParameterListChanged?.Invoke();
		}

		public void UpdateExposedParameter(string guid, object value)
		{
			var param = exposedParameters.Find(e => e.guid == guid);
			string valueType = value.GetType().AssemblyQualifiedName;

			if (valueType != param.type)
				throw new Exception("Type mismatch when updating parameter " + param.name + ": from " + param.type + " to " + valueType);

			param.serializedValue.value = value;
			onExposedParameterModified.Invoke(param.guid);
		}

		public void UpdateExposedParameterName(ExposedParameter parameter, string name)
		{
			parameter.name = name;
			onExposedParameterModified.Invoke(name);
		}

		public ExposedParameter GetExposedParameter(string name)
		{
			return exposedParameters.FirstOrDefault(e => e.name == name);
		}

		public ExposedParameter GetExposedParameterFromGUID(string guid)
		{
			return exposedParameters.FirstOrDefault(e => e.guid == guid);
		}

		public bool SetParameterValue(string name, object value)
		{
			var e = exposedParameters.FirstOrDefault(p => p.name == name);

			if (e == null)
				return false;

			e.serializedValue.value = value;

			return true;
		}

		public object GetParameterValue(string name) => exposedParameters.FirstOrDefault(p => p.name == name)?.serializedValue?.value;

		public T GetParameterValue< T >(string name) => (T)GetParameterValue(name);

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
		
		/// <summary>
		/// Tell if two types can be connected in the context of a graph
		/// </summary>
		/// <param name="t1"></param>
		/// <param name="t2"></param>
		/// <returns></returns>
		public static bool TypesAreConnectable(Type t1, Type t2)
		{
			//Check if there is custom adapters for this assignation
			if (CustomPortIO.IsAssignable(t1, t2))
				return true;

			//Check for type assignability
			if (t2.IsReallyAssignableFrom(t1))
				return true;

			// User defined type convertions
			if (TypeAdapter.AreAssignable(t1, t2))
				return true;

			return false;
		}

	}
}