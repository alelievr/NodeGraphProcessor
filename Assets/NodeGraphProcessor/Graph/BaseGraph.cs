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

		//graph visual properties
		public Vector3				position;
		public Vector3				scale;

        void OnEnable()
        {
            Undo.undoRedoPerformed += UndoRedoPerformed;
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

		public void RemoveNode(BaseNode node)
		{
			Debug.Log("Remove node: " + node);
			nodes.Remove(node);
		}

		public void OnBeforeSerialize()
		{
			Debug.Log("Before serialization node count: " + nodes.Count);

			serializedNodes.Clear();
			
			foreach (var node in nodes)
				serializedNodes.Add(JsonSerializer.Serialize(node));
		}

		public void OnAfterDeserialize()
		{
			nodes.Clear();

			foreach (var serializedNode in serializedNodes)
				nodes.Add(JsonSerializer.DeserializeNode(serializedNode) as BaseNode);
		}
	}
}