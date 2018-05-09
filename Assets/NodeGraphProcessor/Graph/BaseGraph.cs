using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace GraphProcessor
{
	public class BaseGraph : ScriptableObject, ISerializationCallbackReceiver
	{
		//JSon datas contaning all elements of the graph
		[SerializeField]
		public List< JsonElement >	serializedNodes = new List< JsonElement >();

		[System.NonSerialized]
		public List< BaseNode >		nodes = new List< BaseNode >();

        void OnEnable()
        {
            Undo.undoRedoPerformed += UndoRedoPerformed;
            UndoRedoPerformed();
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
        }

        void UndoRedoPerformed()
        {
			Debug.Log("TODO !");
        }

		public void RegisterCompleteObjectUndo(string name)
		{
			Debug.Log("TODO !");
		}
		
		public void AddNode(BaseNode node)
		{
			Debug.Log("Added node: " + node);
			nodes.Add(node);
		}

		public void OnBeforeSerialize()
		{
			Debug.Log("Before serialization node count: " + nodes.Count);

			serializedNodes.Clear();

			Debug.Log("first node: " + nodes.FirstOrDefault());
			
			foreach (var node in nodes)
				serializedNodes.Add(JsonSerializer.Serialize(node));
			
			Debug.Log("Serialized nodes: " + serializedNodes.FirstOrDefault());
		}

		public void OnAfterDeserialize()
		{
			nodes.Clear();

			foreach (var serializedNode in serializedNodes)
				nodes.Add(JsonSerializer.DeserializeNode(serializedNode) as BaseNode);

			Debug.Log("Deserialized nodes: " + nodes.Count);
		}
	}
}